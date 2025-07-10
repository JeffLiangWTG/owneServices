using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	enum FsaProcessingResult
	{
		Unset,
		ProcessedAndUpdated,
		ProcessedWithoutUpdate,
		NotProcessed // e.g. no record found
	}

	class FsaReportProcessor
	{
		public FsaReportProcessor(FsaResponseMessage report, EDIMessage incomingMessage)
		{
			this.report = report;
			this.incomingMessage = incomingMessage;
		}

		internal void ProcessSolitictedResponseUsingSyscar()
		{
			var outgoingObjectPk = new ZGuid(new Guid(report.CargoWise_CommonAccessReference));
			var outboundMessage = incomingMessage.Factory.Load<EDIMessage>(outgoingObjectPk);
			if (outboundMessage != null)
			{
				if (outboundMessage.EM_MessageSubType == CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry.Subcode)
				{
					ProcessStandaloneEnquiryResponse(outgoingObjectPk, outboundMessage);
				}
				else
				{
					incomingMessage.EM_LinkedObject = outboundMessage.EM_LinkedObject;
					UpdateAwbUsingInboundDataAndSetPrettyInterpretation(CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode, outboundMessage.EM_MessageSubType, outboundMessage, incomingMessage, skipSettingGospelDataForASplitUnderAMaster: false);
				}
			}
		}

		void ProcessStandaloneEnquiryResponse(ZGuid outgoingMessagePk, EDIMessage outboundMessage)
		{
			outboundMessage.EM_LinkedObject = incomingMessage;
			incomingMessage.EM_LinkedObject = outboundMessage;
			incomingMessage.EM_MessageInterpretation = FormatNicely();
			var standaloneFsrRequest = incomingMessage.Factory.Load<StandAloneFsrEnquiry>(outgoingMessagePk);
			if (standaloneFsrRequest != null)
			{
				standaloneFsrRequest.ResponseText = !report.Header_ReportText.IsEmpty ? report.Header_ReportText : report.ChildConsignments[0].IndicatorErrorText;
			}
			incomingMessage.EM_Status = EDIMessage.Status.Received;
			incomingMessage.EM_MessageType = outboundMessage.EM_MessageType;
			incomingMessage.EM_MessageSubType = outboundMessage.EM_MessageSubType;
		}

		internal void ProcessUnsolicitedReport()
		{
			if (report.IsReportP5Insert)
			{
				if (GBCustomsDataRegistry.Instance.CcsukP5ProcessorShouldInsertNewRecords.Value)
				{
					if (report.Header_SplitReference.IsEmpty)
					{
						MakeAndUpdateAwbFromP5InsertReport();  // pertains to whole record only
					}
					else
					{
						if (GBCustomsDataRegistry.Instance.CcsukP5ProcessorSplitHandlingShouldMakeNewSplit.Value)
						{
							MakeAndUpdateAwbFromP5InsertReport();
						}
						else
						{
							MakeDummyBasicRecordForP5Split();
						}
					}
				}
				else
				{
					SendP5NotificationEmailButDoNotInsertAwb();
				}
			}
			else if (report.IsReportP5Delete)
			{
				if (GBCustomsDataRegistry.Instance.CcsukP5ProcessorShouldInsertNewRecords.Value)
				{
					ProcessP5DeleteReport();
				}
				else
				{
					SendP5NotificationEmailButDoNotInsertAwb();
				}
			}
			else
				if (FindEntryFromReportEntryNumberAndDateAndProcess()
					|| FindHawbFromReportAndProcess()
					|| FindMawbFromReportAndProcess()
					)
			{
				UpdateAwbUsingInboundDataAndSetPrettyInterpretation(CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode, report.Header_ReportType, null, incomingMessage, skipSettingGospelDataForASplitUnderAMaster: false);
			}
			else
			{
				HandleCouldNotFindJob();
			}
		}

		void SendP5NotificationEmailButDoNotInsertAwb()
		{
			incomingMessage.EM_MessageInterpretation = MessageInterpretation;
			string jobNumber = report.Header_AirwaybillPrefixAndAirwaybillNumber
								+ (report.Header_HouseWaybillNumber.IsEmpty ? "" : ("-" + report.Header_HouseWaybillNumber))
								+ (report.Header_SplitReference.IsEmpty ? "" : ("/" + report.Header_SplitReference));
			var warningAboutNotInserted = "<h3>This record has not been inserted into your database</h3>";
			new CcsukEmailSender(incomingMessage.Factory, CcsukEmailSender.ToWhom.CustomsGroupOnly, incomingMessage).SendEmail(reportTitle + " for " + jobNumber, warningAboutNotInserted + MessageInterpretation,
								GBCustomsDataRegistry.Instance.NotificationCcsukErrors, "", Guid.Empty, Guid.Empty, Guid.Empty);

			incomingMessage.EM_Status = EDIMessage.Status.Received;
		}

		void HandleCouldNotFindJob()
		{
			if (incomingMessage.Interchange != null && incomingMessage.Interchange.EI_RetryCount < 10)
			{
				incomingMessage.Interchange.EI_RetryCount += 1;
				incomingMessage.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(1);
				Enterprise.Customs.Business.ServiceTaskHelper.NudgeServiceTaskDelay(null, ServiceTask.CcsukServiceTaskConstants.CcsukInterchangePackagerServiceTaskCode, TimeSpan.FromMinutes(1));
			}
			else
			{
				incomingMessage.EM_Status = EDIMessage.Status.Failed;
				new CcsukEmailSender(incomingMessage.Factory, CcsukEmailSender.ToWhom.CustomsGroupOnly, null).SendEmail("Could not find CCSUK job using inbound FSA data in message " + incomingMessage.EM_MessageNum,
									string.Format("No master, house or customs declaration could be found using data in an inbound FSA message despite repeated attempts.  No update has been performed. Examine the message in Maintain>System>EDIMessage using message number {0}.  Message text follows. \r\n\r\n{1}", incomingMessage.EM_MessageNum, incomingMessage.EM_MessageText),
									GBCustomsDataRegistry.Instance.NotificationCcsukErrors, "", Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		CusMAWB FindOrMakeMawbBasicFromP5()
		{
			CusMAWB mawbOrBasicAtOnwardShed = null;
			var matchingMawbsAtAllSheds = new CusMAWB.Loader(incomingMessage.Factory).FindFromMawbNumber(report.Header_AirwaybillPrefixAndAirwaybillNumber, 12, "", "");
			var newAirportAndShed = report.GetAirportAndShed("NEW");
			if (matchingMawbsAtAllSheds.Length > 0)
			{
				mawbOrBasicAtOnwardShed = new CusMAWB.Loader(incomingMessage.Factory).FindFromMawbNumber(report.Header_AirwaybillPrefixAndAirwaybillNumber, 12, "", newAirportAndShed).FirstOrDefault();
			}

			if (mawbOrBasicAtOnwardShed == null)
			{
				mawbOrBasicAtOnwardShed = incomingMessage.Factory.New<CusMAWB>();
				mawbOrBasicAtOnwardShed.CM_MAWB = report.Header_AirwaybillPrefixAndAirwaybillNumber;
				mawbOrBasicAtOnwardShed.MasterLevelHouseHelper.CS_WarehouseLocation = newAirportAndShed;
			}

			if (GBCustomsDataRegistry.Instance.CcsukP5ProcessorLinkToConsole.Value)
			{
				FindAndAttachConsol(mawbOrBasicAtOnwardShed);
			}

			return mawbOrBasicAtOnwardShed;
		}

		void MakeAndUpdateAwbFromP5InsertReport()
		{
			var mawbOrBasic = FindOrMakeMawbBasicFromP5();
			if (incomingMessage.Interchange != null)
			{
				mawbOrBasic.Profile = incomingMessage.Interchange.EI_To.Replace("/", "");
				LicenceAndPimaHelper.SetBranchFromPimaForNewAwb(mawbOrBasic);
			}

			bool setPresenceToISR = true;

			ICcsukCusAwb awb = mawbOrBasic;
			if (!report.Header_HouseWaybillNumber.IsEmpty)
			{
				var hawb = FindOrMakeHawbOnMawbFromP5(mawbOrBasic, report.Header_HouseWaybillNumber);
				hawb.Messages.Add(incomingMessage);
				awb = hawb;
				if (!report.Header_SplitReference.IsEmpty)
				{
					awb = ProcessWhenSplitsExist(hawb.Splits, mawbOrBasic.ReferenceNumberWithShed, "bill");
				}
			}
			else
			{
				mawbOrBasic.Messages.Add(incomingMessage);
				if (!report.Header_SplitReference.IsEmpty)
				{
					if (BasicIsMasterWithRealHouses(mawbOrBasic))
					{
						splitNotAddedMessage = string.Format(CultureInfo.InvariantCulture,
							  "<h3 style='color:red'>An additional split {0} was advised via a P5 report for basic {1}, "
							+ "but you have already added houses to this. No extra split was added. "
							+ "You are recommended to adjust the piece count against the houses for the extra {2} "
							+ "pieces that have been advised.</h3>",
							report.Header_SplitReference,
							mawbOrBasic.ReferenceNumberWithShed,
							report.GetNewNPX());
						setPresenceToISR = false;
					}
					else
					{
						awb = ProcessWhenSplitsExist(mawbOrBasic.Splits, mawbOrBasic.ReferenceNumberWithShed, "basic");
					}
				}
			}
			SendPositiveEmail(awb, null);
			UpdateAwbUsingInboundDataAndSetPrettyInterpretation(CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode, report.Header_ReportType, null, incomingMessage, !setPresenceToISR);

			if (setPresenceToISR)
			{
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
			}
			new PrintP5Provider(awb, incomingMessage, ServiceLogger).DoPrinting();
		}

		ICcsukCusAwb ProcessWhenSplitsExist(SplitCollection splits, ZString referenceNumberWithShed, ZString awbTypeDescription)
		{
			ICcsukCusAwb awb;
			var extantSplit = splits[report.Header_SplitReference];
			if (extantSplit == null)
			{
				var split = splits.AddNew();
				split.SplitReference = report.Header_SplitReference;
				awb = split;
			}
			else
			{
				splitNotAddedMessage = string.Format(CultureInfo.InvariantCulture, "<h3 style='color:red'>A split {0} was advised via a P5 report for {1} {2}, "
					+ "but it already exists. No extra split was added. The existing split will be updated using data from the P5 report.</h3>",
					report.Header_SplitReference,
					awbTypeDescription,
					referenceNumberWithShed);
				awb = extantSplit;
			}

			return awb;
		}

		CusHAWB FindOrMakeHawbOnMawbFromP5(CusMAWB mawbOrBasic, ZString hawbNumber)
		{
			var foundHawb = new CusHAWB.Loader(mawbOrBasic.Factory).FindExistingHawbOnMawb(mawbOrBasic, hawbNumber);
			if (foundHawb == null)
			{
				foundHawb = mawbOrBasic.ChildBills.AddNew();
				foundHawb.CS_HAWB = hawbNumber;
			}

			if (GBCustomsDataRegistry.Instance.CcsukP5ProcessorLinkToConsole.Value)
			{
				FindAndAttachShipment(foundHawb);
			}

			return foundHawb;
		}

		bool BasicIsMasterWithRealHouses(ICcsukCusAwb mawbOrBasic)
		{
			var masterBill = mawbOrBasic as CusMAWB;
			return !masterBill?.IsBasic ?? true;
		}

		void MakeDummyBasicRecordForP5Split()
		{
			var dummyBasic = incomingMessage.Factory.New<CusMAWB>();
			dummyBasic.Messages.Add(incomingMessage);
			var oldItsfAirportAndShed = report.GetAirportAndShed("OLD");
			var houseNumberWithDash = report.Header_HouseWaybillNumber.IsEmpty ? "" : "-" + report.Header_HouseWaybillNumber;
			additionalInformationAfterReportTitle = string.Format(CultureInfo.InvariantCulture, "<h3 style='color: orange'>This is a skeleton record created for {0}-{1}{2}/{3}.</h3> <p>It is this record which should be cleared after creating a full record on CCSUK by sending the FRC message</p>", oldItsfAirportAndShed, report.Header_AirwaybillPrefixAndAirwaybillNumber, houseNumberWithDash, report.Header_SplitReference);
			SendPositiveEmail(dummyBasic, null);
			var updater = new FsaJobUpdater(report, null, incomingMessage);
			dummyBasic.Profile = this.incomingMessage.Interchange.EI_To.Replace("/", "");
			updater.ProcessMawb(dummyBasic, report.ChildConsignments[1]);  // the second consingment is the "NEW" one
			dummyBasic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
			dummyBasic.CM_MAWB = dummyBasic.GetUfoMawbNumber();
			dummyBasic.DescriptionOfGoods = GetDummyP5SplitDescription();

			incomingMessage.EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode;
			incomingMessage.EM_MessageSubType = report.Header_ReportType;
			incomingMessage.EM_Status = EDIMessage.Status.Received;
			incomingMessage.EM_MessageInterpretation = MessageInterpretation;
			additionalInformationAfterReportTitle = "";
		}

		string additionalInformationAfterReportTitle = "";

		ZString GetDummyP5SplitDescription()
		{
			if (report.Header_HouseWaybillNumber.IsEmpty)
			{
				// Split basic
				return report.Header_AirwaybillPrefixAndAirwaybillNumber + "/" + report.Header_SplitReference;
			}
			else
			{
				return report.Header_HouseWaybillNumber + "/" + report.Header_SplitReference;
			}
		}

		void ProcessP5DeleteReport()
		{
			var onwardShed = incomingMessage.Interchange.EI_To.Replace("/", "").Right(6);
			var mawbOrBasicAtOnwardShed = new CusMAWB.Loader(incomingMessage.Factory).FindFromMawbNumber(report.Header_AirwaybillPrefixAndAirwaybillNumber, 12, "", onwardShed).FirstOrDefault();
			ICcsukCusAwb awb = mawbOrBasicAtOnwardShed;
			if (mawbOrBasicAtOnwardShed != null)
			{
				if (!report.Header_HouseWaybillNumber.IsEmpty)
				{
					var hawb = (from CusHAWB h in mawbOrBasicAtOnwardShed.ChildBills where h.CS_HAWB == report.Header_HouseWaybillNumber select h).FirstOrDefault();
					awb = hawb;
					if (!report.Header_SplitReference.IsEmpty && hawb != null && hawb.HasSplits)
					{
						var splitHouse = hawb.Splits[report.Header_SplitReference];
						awb = splitHouse;
					}
				}
				else
				{
					if (!report.Header_SplitReference.IsEmpty && mawbOrBasicAtOnwardShed.HasSplits)
					{
						var splitBasic = mawbOrBasicAtOnwardShed.Splits[report.Header_SplitReference];
						awb = splitBasic;
					}
				}
			}
			if (awb != null)
			{
				SendPositiveEmail(awb, null);
				awb.Messages.Add(incomingMessage);
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.IsrRequestCancelled;
			}
			else
			{
				SendP5NotificationEmailButDoNotInsertAwb();
			}
			incomingMessage.EM_Status = EDIMessage.Status.Received;
		}

		void UpdateAwbUsingInboundDataAndSetPrettyInterpretation(string messsageType, string messageSubType, EDIMessage outboundMessage, EDIMessage incomingMsg, bool skipSettingGospelDataForASplitUnderAMaster)
		{
			incomingMsg.EM_MessageType = messsageType;
			incomingMsg.EM_MessageSubType = messageSubType;
			incomingMsg.EM_Status = EDIMessage.Status.Received;

			try
			{
				var fsaUpdater = new FsaJobUpdater(report, outboundMessage, incomingMsg);
				if (messageSubType == CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate.Subcode)
				{
					procesingResult = fsaUpdater.UpdatePropertiesOfConsignmentUsingGospelCcsukData(true);
				}
				else if (report.IsReportP5Insert && !skipSettingGospelDataForASplitUnderAMaster)
				{
					procesingResult = fsaUpdater.UpdatePropertiesOfConsignmentUsingGospelCcsukData(false);
				}
				else if (messageSubType == CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode)
				{
					fsaUpdater.UpdatePresenceOnNetworkForSimpleNonUpdatingFsrCycle();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("BGB-CUK-FsaReportProcessor", "Could not update job using gospel CCSUK data in FSA: " + ex.Message, ex);
			}
			incomingMsg.EM_MessageInterpretation = MessageInterpretation;
		}

		ZString messageInterpretation;
		ZString MessageInterpretation
		{
			get
			{
				if (messageInterpretation.IsEmpty)
				{ messageInterpretation = FormatNicely(); }
				return messageInterpretation;
			}
		}

		ZString FormatNicely()
		{
			if (report.Header_ReportType.IsEmpty && report.Header_ReportText.IsEmpty)
			{
				reportTitle = "Freight Status Enquiry Answer (FSA)";
			}
			else
			{
				string humanName = GetHumanNameForReport(report.Header_ReportType);
				if (report.Header_ReportText.ToUpper() != humanName.ToUpper())
				{
					reportTitle = "FSA: " + humanName + " " + report.Header_ReportText;
				}
			}

			var headerTableCreator = new HtmlTableCreator(new string[] { "Header Data", "Value" });
			if (!report.Header_AirwaybillPrefixAndAirwaybillNumber.IsEmpty)
			{
				headerTableCreator.WriteRow("Air Waybill", report.Header_AirwaybillPrefixAndAirwaybillNumber);
			}

			if (!report.Header_HouseWaybillNumber.IsEmpty)
			{
				headerTableCreator.WriteRow("House Air Waybill", report.Header_HouseWaybillNumber);
			}

			if (!report.Header_SplitReference.IsEmpty)
			{
				headerTableCreator.WriteRow("Split", report.Header_SplitReference);
			}

			var childTablesForEachConsignment = new List<string>();
			foreach (var consignment in report.ChildConsignments)
			{
				childTablesForEachConsignment.Add(WriteSingleConsignmentAsTable(consignment));
			}

			string maybeNotYourJobAndNoUpdateMessage = procesingResult == FsaProcessingResult.ProcessedWithoutUpdate ? "<h3 style='color:red'>No update of the local record has been performed</h3>" : "";

			string result = string.Format("{2} {3} {5} <h3>{0}</h3> {4}  {1}", reportTitle, headerTableCreator.ToHtml(), MessagePrettierCss.CSS, maybeNotYourJobAndNoUpdateMessage, additionalInformationAfterReportTitle, splitNotAddedMessage);
			foreach (var table in childTablesForEachConsignment)
			{
				result += "<BR/>" + table;
			}
			return result;
		}

		bool FindHawbFromReportAndProcess()
		{
			if (!report.Header_HouseWaybillNumber.IsEmpty)
			{
				var hawb = new CusHAWB.Loader(incomingMessage.Factory).FindHawb(report.Header_HouseWaybillNumber, report.Header_SplitReference, report.Header_AirwaybillPrefixAndAirwaybillNumber, report.GetAirportAndShed(""));
				if (hawb != null)
				{
					hawb.Messages.Add(incomingMessage);
					incomingMessage.EM_GB = hawb.Branch.PK;
					SendPositiveEmail(hawb, hawb.UserInChargeOfJob);
					return true;
				}
			}
			return false;
		}

		bool FindMawbFromReportAndProcess()
		{
			var mawb = new CusMAWB.Loader(incomingMessage.Factory).FindFromMawbNumber(report.Header_AirwaybillPrefixAndAirwaybillNumber, report.Header_SplitReference, report.GetAirportAndShed(""));
			if (mawb != null)
			{
				mawb.Messages.Add(incomingMessage);
				incomingMessage.EM_GB = mawb.Branch.PK;
				SendPositiveEmail(mawb, mawb.UserInChargeOfJob);
				return true;
			}
			return false;
		}

		bool FindEntryFromReportEntryNumberAndDateAndProcess()
		{
			bool foundAtLeastOneEntry = false;
			foreach (var childConsignment in report.ChildConsignments)
			{
				if (!childConsignment.EntryNumber.IsEmpty)
				{
					var fullEntryNum = childConsignment.EntryNumber.Length == 7
						? (ZString)(childConsignment.EntryProcessingUnit + "-" + childConsignment.EntryNumber)
						: childConsignment.EntryNumber;

					var cusEntryHeader = GbExtensionHelpers.GetCusEntryHeaderFromCusEntryNumberAndDate(fullEntryNum, incomingMessage.Factory, childConsignment.EntryDate).FirstOrDefault();
					if (cusEntryHeader != null)
					{
						cusEntryHeader.Messages.Add(incomingMessage);
						if (cusEntryHeader.Declaration != null)
						{
							incomingMessage.EM_GB = cusEntryHeader.Declaration.Branch.PK;
							if (cusEntryHeader.Declaration.ActiveEntryHeaders.Count == 1)
							{
								cusEntryHeader.CH_IrcInventoryReturnCode = childConsignment.InventoryReturnCodeIRC;
							}
						}
						foundAtLeastOneEntry = true;
						SendPositiveEmail(cusEntryHeader, GetEntrysUser(cusEntryHeader));
					}
				}
			}
			return foundAtLeastOneEntry;
		}

		GlbStaff GetEntrysUser(CusEntryHeader cusEntryHeader)
		{
			if (cusEntryHeader != null)
			{
				if (cusEntryHeader.Declaration != null && !cusEntryHeader.Declaration.JE_GS_NKCusAgent.IsEmpty)
				{
					return cusEntryHeader.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, cusEntryHeader.Declaration.JE_GS_NKCusAgent));
				}
				else
				{
					if (cusEntryHeader.Messages != null && cusEntryHeader.Messages.LastOutgoingMessage != null)
					{
						return cusEntryHeader.Messages.LastOutgoingMessage.UserWhoQueuedThisRecord;
					}
				}
			}
			return null;
		}

		string WriteSingleConsignmentAsTable(FsaChildConsignment consignment)
		{
			HtmlTableCreator table = null;
			if (!consignment.ConsignmentReferenceNumber.IsEmpty)
			{
				var consignmentType = new FsaDocumentTypes().GetDescriptionFromCode(consignment.ConsignmentReferenceNumberType);
				var oldOrNew = consignment.OldOrNewDataIndicator;
				consignmentType = oldOrNew + " " + consignmentType;
				table = new HtmlTableCreator(new string[] { consignmentType, consignment.ConsignmentReferenceNumber });
			}
			else
			{
				table = new HtmlTableCreator(new string[] { "Consignment Data", "Value" });
			}
			foreach (PropertyInfo info in consignment.GetType().GetProperties())
			{
				if (info.CanRead)
				{
					var nameFormatted = ZPropertyInfo.GetFriendlyColumnNameShared(info.Name);
					object objectValue = info.GetValue(consignment, null);
					var zType = objectValue as IZType;
					if (zType != null)
					{
						if (!zType.IsEmpty && !zType.IsDefault)
						{
							table.WriteRow(nameFormatted, zType);
						}
					}
					else if (objectValue as IEnumerable != null)
					{
						var sb = new ZStringBuilder();
						foreach (var item in (IEnumerable)objectValue)
						{
							sb.AppendIfNotEmpty(item.ToString());
						}
						if (sb.Length > 0)
						{
							table.WriteRow(nameFormatted, sb.ToStringWithDelimiterBetweenAppends("; "));
						}
					}
					else if (objectValue != null)
					{
						table.WriteRow(nameFormatted, objectValue.ToString());
					}
				}
			}
			return table.ToHtml();
		}

		ZString GetHumanNameForReport(ZString reportCode)
		{
			string result = "";
			switch (reportCode)
			{
				case "JA":
				case "E0":
					result = "Inventory Failure Report";
					break;
				case "G5":
					result = "Pre-arrival Agent Mismatch Report";
					break;
				case "H3":
					result = "Goods Arrival Reprocessing Error Report";
					break;
				case "P5":
					result = "Advice of Inter-Shed Removal Report";
					break;
				case "U":
					result = "Report Duplication Report";
					break;
				case "":
					result = "";
					break;
				default:
					result = "Report " + reportCode;
					break;
			}
			return (result + " " + reportCode).Trim();
		}

		void SendPositiveEmail(ICcsukCusAwb awb, GlbStaff staff)
		{
			string interpretation = MessageInterpretation;
			new CcsukEmailSender(incomingMessage.Factory, CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry, (BusinessObject)awb, staff).SendEmail(reportTitle + " for " + awb.HumanReadableName, interpretation,
									GBCustomsDataRegistry.Instance.NotificationCcsukCukFsaReport, report.Header_ReportType, awb.Branch.Company.PK.ToGuid(), awb.Branch.PK.ToGuid(), Guid.Empty);
		}

		void SendPositiveEmail(CusEntryHeader job, GlbStaff staff)
		{
			string interpretation = MessageInterpretation;
			new CcsukEmailSender(incomingMessage.Factory, CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry, job, staff).SendEmail(reportTitle + " for " + job.HumanReadableName, interpretation,
									GBCustomsDataRegistry.Instance.NotificationCcsukCukFsaReport, report.Header_ReportType, job.Declaration.Branch.Company.PK.ToGuid(), job.Declaration.Branch.PK.ToGuid(), Guid.Empty);
		}

		void FindAndAttachConsol(CusMAWB mawb)
		{
			var consol = ParserHelper.FindBritishConsolWithThisMawbNumber(mawb.Factory, mawb.CM_MAWB);
			if (consol != null)
			{
				mawb.CM_JK = consol.PK;
			}
		}

		void FindAndAttachShipment(CusHAWB hawb)
		{
			var shipment = ParserHelper.FindBritishShipmentWithThisHawbNumberAndOnThisMasterAndMaybeOriginToo(hawb.CS_HAWB, hawb.MAWB.CM_MAWB, hawb.MAWB.AirportOfOrigin, incomingMessage);

			if (shipment != null)
			{
				hawb.CS_JS = shipment.PK;
			}
		}

		FsaProcessingResult procesingResult;
		string reportTitle = "";
		string splitNotAddedMessage = "";
		readonly FsaResponseMessage report;
		readonly EDIMessage incomingMessage;
		public ILogger ServiceLogger { get; set; }
	}
}
