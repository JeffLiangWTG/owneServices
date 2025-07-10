using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public interface IB3AutoSender
	{
		void Process();
	}

	public class B3AutoSender : IB3AutoSender
	{
		public B3AutoSender(ILogger serviceLogger)
		{
			logger = Argument.NotNull(serviceLogger, "serviceLogger");
		}

		readonly ILogger logger;

		public void Process()
		{
			logger.Log(LogType.Information, string.Format("CAD Auto Sending executing for Branch: {0}.", GlbBranch.CurrentBranch.GB_Code));

			foreach (DynamicBusinessObject businessObject in GetCandidateCusEntryHeaders())
			{
				try
				{
					var factory = new BusinessObjectFactory();
					var cusEntryHeader = factory.Load<CusEntryHeader>(new ZGuid(businessObject[CusEntryHeaderSchema.Constants.PK]));
					if (cusEntryHeader != null)
					{
						var declaration = cusEntryHeader.Declaration;
						if (declaration != null)
						{
							if (declaration.ShowSubmitMenuItem)
							{
								logger.Log(LogType.Error, $"CAD Auto Sending failed for Declaration: {declaration.HumanReadableName}, because this job is configured to submit through a designated service provider interface.");
							}
							else if (NeedToSendB3Message(declaration))
							{
								try
								{
									var success = SendMessage(declaration, cusEntryHeader);
									factory.Save();
									if (success)
									{
										logger.Log(LogType.Information, string.Format("CAD Auto Sending for Declartion: {0} succeeded.", declaration.HumanReadableName));
									}
								}
								catch (ZSaveException e)
								{
									ZExceptionReporting.HandleSaveException(e);
									logger.Log(LogType.Error, "CAD Auto Sending failed; unable to save changes.", e);
								}
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					logger.Log(LogType.Error, "CAD Auto Sending failed.", ex);
				}
			}
		}

		#region Get Candidate CusEntryHeader PKs

#if DEBUG
		protected virtual
#endif
		DynamicBusinessObjectCollection GetCandidateCusEntryHeaders()
		{
			var factory = new BusinessObjectFactory();
			var cusEntryHeaderPKs = new DynamicBusinessObjectCollection(factory);
			cusEntryHeaderPKs.Load(GetCandidateCusEntryHeaderPKSql());
			return cusEntryHeaderPKs;
		}

		ZString GetCandidateCusEntryHeaderPKSql()
		{
			return string.Format(@"SELECT {0} FROM {1} INNER JOIN {2} 
					ON {3} = {4}
					OUTER APPLY dbo.csfn_GetAddInfoValueFromCodeInline({16}, 'B3AutoSend') AS B3AutoSend
					OUTER APPLY dbo.csfn_GetAddInfoValueFromCodeInline({16}, 'K84AccountingDate') AS K84AccountingDate
					WHERE {5} = 0
					AND {6} = '{7}'
					AND {8} IN ('{9}')
					AND {10} IN ('{11}')
					AND {12} = '{13}'
					AND {14} > CONVERT(DATETIME, '{15}', 112)
					AND B3AutoSend.Value = 'Y'
					AND K84AccountingDate.Value IS NULL
					AND {17} in ('{18}', '{19}') AND {20} <> '{21}' AND {22} <> '{23}' AND {24} <> '{25}' AND {26} <> '{27}'",
				CusEntryHeaderSchema.Constants.PK, JobDeclarationSchema.Constants.TableName, CusEntryHeaderSchema.Constants.TableName,
				JobDeclarationSchema.Constants.PK, CusEntryHeaderSchema.Constants.CH_JE,
				JobDeclarationSchema.Constants.JE_IsCancelled,
				JobDeclarationSchema.Constants.JE_GB, GlbBranch.CurrentBranch.PK,
				JobDeclarationSchema.Constants.JE_MessageType, JobDeclarationB3SendingStrategy.AllowedMessageTypeList.Aggregate((a, b) => a + "','" + b),
				JobDeclarationSchema.Constants.JE_MessageSubType, JobDeclarationB3SendingStrategy.AllowedMessageSubTypeList.Union(JobDeclarationB3SendingStrategy.AllowedMessageSubTypeListForCAD).Aggregate((a, b) => a + "','" + b),
				JobDeclarationSchema.Constants.JE_MessageStatus, MessageStatusList.Codes.NotSent,
				JobDeclarationSchema.Constants.JE_EntryAuthorisationDate, ZDateTime.Now.AddMonths(-3).ToString("yyyyMMdd"),
				JobDeclarationSchema.Constants.JE_AddInfo,
				CusEntryHeaderSchema.Constants.CH_MessageType, MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration,
				CusEntryHeaderSchema.Constants.CH_EntryStatus, B3EntryStatusList.Codes.Accepted,
				CusEntryHeaderSchema.Constants.CH_EntryStatus, B3EntryStatusList.Codes.Confirmed,
				CusEntryHeaderSchema.Constants.CH_Status, MessageStatusList.Codes.AwaitingOriginal,
				CusEntryHeaderSchema.Constants.CH_EntryStatus, CADEntryStatusList.Codes.Approved);
		}

		ZDateTime GetCurrentDate()
		{
			return EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("CAOTT", ZDateTime.UtcNow.ToDateTime());
		}

		#endregion

		#region Filtering Candidate Jobs

#if DEBUG
		protected virtual
#endif
 bool NeedToSendB3Message(JobDeclaration declaration)
		{
			var sendDate = declaration.ScheduledB3AutoSendingDate;
			return sendDate.IsValid && GetCurrentDate() >= sendDate;
		}

		#endregion

		#region Send Entry Message

#if DEBUG
		protected virtual
#endif
		bool SendMessage(JobDeclaration declaration, CusEntryHeader entryHeader)
		{
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var notification = GetUserNotification(declaration);
			if (entryHeader.IsB3C)
			{
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				var dataWrapper = declaration.IsLVS ? (IB3Header)new LowValueShipmentsMessageWrapper(entryHeader) : new B3ImportMessageWrapper(entryHeader);

				return new B3ImportMessageManager(dataWrapper, notification).SendMessage(MessageSubTypes.Create);
			}
			else if (entryHeader.IsCAD && UniversalReferenceConstants.IsCarmR2)
			{
				var wrapper = new CADMessageWrapper(entryHeader);
				return new CADMessageManager(wrapper, notification).SendMessage();
			}
			return false;
		}

#if DEBUG
		protected
#endif
 Customs.Business.MessageManagers.IUserNotification GetUserNotification(JobDeclaration declaration)
		{
			return new B3AutoSendingLogAndUserNotificationWrapper(declaration, logger);
		}

		#endregion
	}
}
