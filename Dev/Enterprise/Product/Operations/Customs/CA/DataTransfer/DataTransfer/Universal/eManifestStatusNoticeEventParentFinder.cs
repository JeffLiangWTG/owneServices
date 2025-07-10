using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using CusEntryNumHelperForCargoControlNumber = Enterprise.Customs.Business.CusEntryNumHelperForCargoControlNumber;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	class eManifestStatusNoticeEventParentFinder : EventParentFinder
	{
		internal eManifestStatusNoticeEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var result = new List<BusinessObject>();
			var dataContext = eventDataObject?.DataContext;
			if (dataContext?.RecipientRoleCollection?.FirstOrDefault()?.Code == RecipientRoleType.CD4)
			{
				var uniqueIDValue = eventDataObject.GetTargetKeyByType(nameof(DataContextType.CAeManifestStatusNotice));

				if (!uniqueIDValue.IsEmpty)
				{
					var applicationReference = DuplicateUniversalEventChecker.GetApplicationReference(eventDataObject);
					if (DuplicateUniversalEventChecker.CheckDuplicateMessages(applicationReference, factory, new[] { UniversalEventMessageTypes.Codes.D4Notices }))
					{
						throw new DuplicateMessageException(applicationReference);
					}
					else
					{
						var noticeRecipientType = eventDataObject.GetContextValueByType(UniversalEventMessageProcessorConstants.ContextType.NoticeRecipientType);
						if (noticeRecipientType != UniversalEventMessageProcessorConstants.NoticeRecipientType.WarehouseOperator)
						{
							result.AddRange(factory.Load<CusEntryHeader>(CusEntryHeaderByNumberQuery(uniqueIDValue)));
							result.AddRange(factory.Load<JobDeclaration>(DeclarationsByCCNQuery(uniqueIDValue)));

							var sendersRef = eventDataObject.GetContextValueByType(UniversalEventMessageProcessorConstants.ContextType.OrganizationReference);
							result.AddRange(factory.Load<CusCAeMHHouse>(CusCAeMHHouseByCCNQuery(sendersRef, uniqueIDValue)));
							result.AddRange(factory.Load<CusCAeMHMaster>(CusCAeMHMasterByCCNQuery(sendersRef, uniqueIDValue)));
						}

						result.AddRange(factory.Load<ForwardingShipment>(ShipmentsByCCNQuery(uniqueIDValue)));
						result.AddRange(factory.Load<ForwardingConsol>(ForwardingConsolByCCNQuery(uniqueIDValue)));
					}
				}

				if (result.Count == 0)
				{
					SendAcknowledgementReport(eventDataObject);
				}
			}
			return result.ToArray();
		}

		#region Query

		ZQuery CusEntryHeaderByNumberQuery(ZString number)
		{
			var cusEntryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			cusEntryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, number);
			cusEntryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, MessageTypeList.Codes.EDIRelease);
			return cusEntryHeaderQuery;
		}

		ZQuery DeclarationsByCCNQuery(ZString ccn)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			jobDeclarationQuery.OrderBy = JobDeclarationSchema.JE_DeclarationReference.Name;
			jobDeclarationQuery.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusAddInfoQuery(SQLComparisonOperator.Equal, ccn), JoinCondition.And);
			return jobDeclarationQuery;
		}

		ZQuery ShipmentsByCCNQuery(ZString ccn)
		{
			var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipmentQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, true);
			shipmentQuery.OrderBy = JobShipmentSchema.JS_UniqueConsignRef.Name;
			shipmentQuery.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusEntryNumberQuery(JobShipmentSchema.Constants.TableName, SQLComparisonOperator.Equal, ccn), JoinCondition.And);
			return shipmentQuery;
		}

		ZQuery CusCAeMHHouseByCCNQuery(ZString sendersRef, ZString ccn)
		{
			if (!sendersRef.IsEmpty && sendersRef.Length > 8 && sendersRef.StartsWith(CusCAeMHHouse.JobIdentificationPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return new ZQuery(CusCAeMHHouseSchema.BW_MessageReference, sendersRef.Substring(4));
			}
			else
			{
				var cusCAeMHHouseQuery = new ZDBOnlyQuery(typeof(CusCAeMHHouse));
				cusCAeMHHouseQuery.OrderBy = CusCAeMHHouseSchema.BW_MessageReference.Name;
				cusCAeMHHouseQuery.AddToFilter(CusCAeMHHouseSchema.BW_HouseCCN, ccn);
				return cusCAeMHHouseQuery;
			}
		}

		ZQuery CusCAeMHMasterByCCNQuery(ZString sendersRef, ZString ccn)
		{
			if (!sendersRef.IsEmpty && sendersRef.Length > 8 && sendersRef.StartsWith(CusCAeMHMaster.JobIdentificationPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return new ZQuery(CusCAeMHMasterSchema.BP_MessageReference, sendersRef.Substring(4));
			}
			else
			{
				var cusCAeMHMasterQuery = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
				cusCAeMHMasterQuery.OrderBy = CusCAeMHMasterSchema.BP_MessageReference.Name;
				cusCAeMHMasterQuery.AddToFilter(CusCAeMHMasterSchema.BP_PrimaryCCN, ccn);
				if (ccn.Length > 8)
				{
					cusCAeMHMasterQuery.AddToFilter(JoinCondition.Or, CusCAeMHMasterSchema.BP_PrimaryCCN, ccn.Substring(4));
				}
				return cusCAeMHMasterQuery;
			}
		}

		ZQuery ForwardingConsolByCCNQuery(ZString ccn)
		{
			var forwardingConsolQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
			forwardingConsolQuery.AddToFilter(JobConsolSchema.JK_IsForwarding, true);
			forwardingConsolQuery.OrderBy = JobConsolSchema.JK_UniqueConsignRef.Name;
			var cusEntryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, ccn);
			if (ccn.Length > 8)
			{
				cusEntryNumberQuery.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryNum, ccn.Substring(4));
			}
			cusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Enterprise.Core.Constants.CountryCodes.Canada);
			cusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CanadaAdditionalReferenceNumberTypes.Codes.CCN);
			forwardingConsolQuery.AddSubQuery(cusEntryNumberQuery, JoinCondition.And);
			return forwardingConsolQuery;
		}

		#endregion

		#region SendReport

		void SendAcknowledgementReport(UniversalEvent universalEvent)
		{
			var emailBuilder = new EmailDefBuilder(AutoEvents.CustomsManifestStatus.Description, ZString.Empty, EmailDefBuilder.HtmlTemplates.IIDResponse);
			var messageInterpretation = new UniversalEventMessageInterpretationGenerator(factory, universalEvent).GetInterpretatedHTML();
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, messageInterpretation);
			emailBuilder.AddArgReplacementRange(ZString.Empty, "A message has been received from the CBSA, but no associated business object was found.");
			var email = emailBuilder.ToEmail();
			CopyGroupToEmails(email, CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.Value);
			SendReport(email);
		}

		void CopyGroupToEmails(EmailDef email, ZGuid groupToCopy)
		{
			if (!groupToCopy.IsEmpty)
			{
				var group = factory.Load<GlbGroup>(groupToCopy);

				if (group != null)
				{
					var emailGroupUtility = new EmailGroupUtility();

					foreach (GlbStaff staff in group.Staff)
					{
						if (!staff.GS_EmailAddress.IsEmpty && !emailGroupUtility.IsHostNotificationEmail(staff.GS_EmailAddress))
						{
							AddRecipientCore(email, staff.GS_EmailAddress, RecipientDef.RecipientTypes.CC);
						}
					}
				}
			}
		}

		void AddRecipientCore(EmailDef emailDef, ZString email, RecipientDef.RecipientTypes type)
		{
			emailDef.AddRecipientForUserCommunication(email, type);
		}

		void SendReport(EmailDef email)
		{
			if (email.Recipients.Count > 0 || email.CCRecipients.Count > 0 || email.BCCRecipients.Count > 0)
			{
				try
				{
					if (factory != null)
					{
						Env.OutgoingMailManager.Create(factory, email);
					}
					else // Just in case there's some error handling trying to report something when the factory has not been set.
					{
						Env.OutgoingMailManager.CreateAndSave(email);
					}
				}
				catch (EmailSendFailedException e)
				{
					logger.LogBoth(Integration.LogType.Error, "Couldn't send email: " + e.Message + ".  Here are the contents of the email that couln't be sent:\r\n\r\n" +
						"SUBJECT: " + email.Subject + "\r\n" +
						"BODY: " + email.Body + "\r\n");
				}
			}
		}

		#endregion
	}
}
