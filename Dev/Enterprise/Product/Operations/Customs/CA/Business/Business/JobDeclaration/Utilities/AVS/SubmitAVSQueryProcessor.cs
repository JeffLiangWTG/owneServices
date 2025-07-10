using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	internal class SubmitAVSQueryProcessor : IProcessor
	{
		public SubmitAVSQueryProcessor(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}
		readonly JobDeclaration declaration;

		#region Process

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var key = CACustomsDataRegistry.Instance.AIRSValidationKey.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), declaration.Branch.PK.ToGuid(), Guid.Empty);
			if (string.IsNullOrEmpty(key))
			{
				notifications.AddWarning(Res.GetString("03019717-2b1d-4e8e-b788-b350a4a6ccea",
						"Submit AIRS Validation Query for Declaration {0} aborted. AIRS Validation Key hasn't been setup for Company {1}. The key is allocated by CFIA. Please set it up in Registry -> {2}",
						declaration.JE_DeclarationReference, declaration.Company.GC_Code, ((Integration.IRegistryItemInternals)CACustomsDataRegistry.Instance.AIRSValidationKey).Location));
			}
			else
			{
				var messages = FindQueuedAVSMessages();
				if (messages.Length == 0)
				{
					CreateNewAVSMessage();
				}
				else
				{
					foreach (var message in messages)
					{
						token.ThrowIfCancellationRequested();
						SqlApplicationLock sqlAppLock = null;
						try
						{
							if (AVSQueryMessageProcessor.TryGetLock(Db.Connection, message, out sqlAppLock))
							{
								message.EM_HeldUntilDate = ZDateTime.UtcNow;
								message.EM_MessageText = ZString.Empty;
							}
						}
						finally
						{
							if (sqlAppLock != null)
							{
								sqlAppLock.Dispose();
							}
						}
					}
				}
			}
		}

		#endregion

		#region CreateNewAVSMessage

		void CreateNewAVSMessage()
		{
			var message = declaration.Factory.New<AVSQueryMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationReference = declaration.JE_DeclarationReference;
			message.EM_HeldUntilDate = ZDateTime.UtcNow;
			message.EM_LinkedObject = declaration.ReleaseEntryHeader;
			message.EM_GB = declaration.JE_GB;
		}

		#endregion

		#region FindQueuedAVSMessages

		AVSQueryMessage[] FindQueuedAVSMessages()
		{
			var query = GetQueuedAVSMessageQuery();
			return declaration.Factory.Load<AVSQueryMessage>(query);
		}

		ZQuery GetQueuedAVSMessageQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.AVSQuery);
			result.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageTypeList.Codes.AVSQuery);
			result.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			result.AddToFilter(EDIMessageSchema.EM_ApplicationReference, declaration.JE_DeclarationReference);
			result.AddToFilter(EDIMessageSchema.EM_GB, declaration.JE_GB);
			result.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			const string indexName = "NR_RX__EM_ApplicationCode_EM_ApplicationReference";
			result.TableIndexHints.Add(new TableIndexHint(indexName));

			return result;
		}

		#endregion
	}
}
