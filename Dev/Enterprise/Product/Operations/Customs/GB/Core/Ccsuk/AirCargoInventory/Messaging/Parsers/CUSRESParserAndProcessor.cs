using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSRES_2_912;
using Enterprise.Customs.GB.Registry;
using Enterprise.Edifact.Auto;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	public class CUSRESParserAndProcessor
	{
		public CUSRESParserAndProcessor(SegmentGroup segmentGroup, EDIMessage ediMessage, ILogger iLogger)
		{
			edifactObject = segmentGroup;
			incomingMessage = ediMessage;
			serviceLogger = iLogger;
		}

		public void DoProcessing()
		{
			var cUSRESEdifact = edifactObject as CUSRESMessage;
			if (cUSRESEdifact != null)
			{
				var parser = new CUSRESInboundParser(incomingMessage);
				var cUSRESResponseData = parser.ParseCUSRESForListOfUpdatedFields();
				JobDeclaration dec = null;
				CusUnderbond underbond = null;
				CusHAWB hawbOrHawbHelper = null;

				EDIMessage outboundMessage = null;
				LookupCusHawbAndCusUnderbondFromCommonAccessReference(cUSRESResponseData.CommonAccessReference, cUSRESResponseData.DocumentNameCode, incomingMessage.Factory, out hawbOrHawbHelper, out underbond, out outboundMessage);
				if (hawbOrHawbHelper == null)
				{
					hawbOrHawbHelper = new CusHAWB.Loader(incomingMessage.Factory).FindHawb(cUSRESResponseData.HouseWaybillNumber, "", cUSRESResponseData.AirWaybillPrefixAndNumber);
					if (hawbOrHawbHelper == null)
					{
						serviceLogger.Log(LogType.Information, "Could not find hawb using Common Access Reference or using House Bill Number, Split Reference and Air Waybill Number. Message number #" + incomingMessage.EM_MessageNum);
					}
				}
				else
				{
					hawbOrHawbHelper.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
					var awb = !hawbOrHawbHelper.CS_IsMasterHouse ? hawbOrHawbHelper : (ICcsukCusAwb)hawbOrHawbHelper.MAWB;
					if (awb.HasSplits && !cUSRESResponseData.SplitReference.IsEmpty)
					{
						var split = awb.Splits[cUSRESResponseData.SplitReference];
						if (split != null)
						{
							split.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
						}
					}
				}

				switch (cUSRESResponseData.DocumentNameCode)
				{
					case Fallback.RemovalCode:
						ProcessFallback(cUSRESResponseData, ref dec, hawbOrHawbHelper);
						break;
					case InterAirportRemoval.RemovalCode:
					case TranshipmentRemoval.RemovalCode:
					case InterShedRemoval.RemovalCode:
						ProcessRemoval(cUSRESResponseData, hawbOrHawbHelper, underbond);
						break;
					default:
						throw new NotSupportedException("CUSRESParserAndProcessor cannot understand that type of CUSRES - " + cUSRESResponseData.DocumentNameCode);
				}

				incomingMessage.EM_Status = EDIMessage.Status.Received;
				hawbOrHawbHelper.Messages.Add(incomingMessage);
				incomingMessage.EM_GB = hawbOrHawbHelper.Branch.PK;
				outboundMessage.EM_Status = EDIMessage.Status.Acknowledged;
				var messageInterpretation = FormatCUSRESHTMLResponse(cUSRESResponseData, hawbOrHawbHelper);
				incomingMessage.EM_MessageInterpretation = MessagePrettierCss.CSS + messageInterpretation;
				incomingMessage.EM_MessageSubType = cUSRESResponseData.DocumentNameCode;
				incomingMessage.EM_MessageType = "RES";
				SendEmail(messageInterpretation, hawbOrHawbHelper, cUSRESResponseData.DocumentNameCode, dec);
				DoAnyPrintingFromCusRes(cUSRESResponseData);
				if (underbond != null)
				{
					underbond.C4_Status = EDIMessage.Status.Acknowledged; // CUSRES implies success.... failure would be CONTRL
				}
			}
		}

		void DoAnyPrintingFromCusRes(CUSRESResponseData cUSRESResponseData)
		{
			var printProvider = new PrintFromCusresProvider(cUSRESResponseData, this.incomingMessage, serviceLogger);
			printProvider.DoPrinting();
		}

		void ProcessFallback(CUSRESResponseData cUSRESResponseData, ref JobDeclaration dec, CusHAWB hawbOrWorkerHawb)
		{
			var awb = !hawbOrWorkerHawb.CS_IsMasterHouse ? hawbOrWorkerHawb : (ICcsukCusAwb)hawbOrWorkerHawb.MAWB;
			if (awb.HasSplits)
			{
				//TODO - think about linking FBK responses for splits to one or several declarations. Maybe set JE_SplitHOuseReference. 
				awb = awb.Splits[cUSRESResponseData.SplitReference];
				if (awb == null)
				{
					return;
				}
				incomingMessage.EM_ApplicationReference = cUSRESResponseData.SplitReference;
			}
			else
			{
				dec = MakeNonInventoryChiefDeclarationSkeletonForHawb(cUSRESResponseData, dec, hawbOrWorkerHawb);
			}
			awb.SetCustomsActionCode(cUSRESResponseData.CustomsActionCode_StatusOfRequest, ZDateTime.Now);
			awb.LatestCustomsActionText = cUSRESResponseData.CustomsActionText;
		}

		const string PreviousDocumentOtherCode = "ZZZ";

		JobDeclaration MakeNonInventoryChiefDeclarationSkeletonForHawb(CUSRESResponseData cUSRESResponseData, JobDeclaration dec, CusHAWB hawbOrWorkerHawb)
		{
			dec = new JobDeclarationAndShipmentFromHawbLookerUpper(hawbOrWorkerHawb, incomingMessage.Factory, incomingMessage).FindDeclaration();

			if (cUSRESResponseData.AirportOfOrigin.Length == 3 && !cUSRESResponseData.CountryOfOrigin.IsEmpty)
			{
				dec.JE_RL_NKOrigin = cUSRESResponseData.CountryOfOrigin + cUSRESResponseData.AirportOfOrigin;
			}
			else
			{
				dec.JE_RL_NKOrigin = cUSRESResponseData.AirportOfOrigin;
			}

			if (cUSRESResponseData.AirportOfReceipt.Length == 3)
			{
				dec.JE_RL_NKFinalDestination = "GB" + cUSRESResponseData.AirportOfReceipt;
			}
			else
			{
				dec.JE_RL_NKFinalDestination = cUSRESResponseData.AirportOfReceipt;
			}

			dec.JE_OwnerRef = cUSRESResponseData.AgentsReferenceNumber;
			dec.JE_EntryStatus = cUSRESResponseData.DocumentNameCode;
			dec.JE_CustomsProfile = cUSRESResponseData.AgentCode;
			dec.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			dec.JE_MasterBill = cUSRESResponseData.AirWaybillPrefixAndNumber;
			dec.JE_VoyageFlightNo = hawbOrWorkerHawb.MAWB.CM_FlightNo;
			dec.JE_GoodsDescription = cUSRESResponseData.DescriptionOfGoods;
			dec.JE_HouseBill = cUSRESResponseData.HouseWaybillNumber;
			dec.JE_TotalNoOfPacks = cUSRESResponseData.NoOfPackagesExpected;
			dec.JE_DeclarationType = "";
			dec.JE_EntrySubStyle = "";
			dec.JE_LocationOfGoods = cUSRESResponseData.AirportOfReceipt;
			dec.SubLocation = cUSRESResponseData.ShedId;
			var pd = dec.PreviousDocuments.AddNew();
			pd.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			pd.CSI_Code = PreviousDocumentOtherCode;
			pd.CSI_ReferenceNumber = "FALLBACK=" + FormatEntryNumber(cUSRESResponseData.EntryNumber);
			dec.JE_MasterUCR = ""; // Any entry after the fallback 			
			return dec;
		}

		ZString FormatEntryNumber(ZString entryNumber)
		{
			return ZString.Format("{0}-{1}", entryNumber.Left(3), entryNumber.SubstringSafe(3));
		}

		string FormatCUSRESHTMLResponse(CUSRESResponseData cUSRESResponseData, CusHAWB hawb)
		{
			var responseData = new Dictionary<string, string>();
			if (cUSRESResponseData.DocumentNameCode == Fallback.RemovalCode)
			{
				responseData.Add("Entry Number", cUSRESResponseData.EntryNumber);
			}
			else if (cUSRESResponseData.DocumentNameCode == TranshipmentRemoval.RemovalCode)
			{
				responseData.Add("Transit Reference Number", cUSRESResponseData.EntryNumber);
			}
			responseData.Add("Customs Action Code", cUSRESResponseData.CustomsActionCode_StatusOfRequest);
			responseData.Add("Airport of Receipt", cUSRESResponseData.AirportOfReceipt);
			responseData.Add("Shed Id", cUSRESResponseData.ShedId);
			responseData.Add("Agent's Reference Number", cUSRESResponseData.AgentsReferenceNumber);
			responseData.Add("Number of Packages Entered", cUSRESResponseData.NoOfPackagesExpected.ToString());

			var headTableCreator = new HtmlTableCreator();
			foreach (var data in responseData)
			{
				headTableCreator.WriteRow(data.Key, data.Value);
			}

			var splitNumber = cUSRESResponseData.SplitReference.IsEmpty ? string.Empty : "/" + cUSRESResponseData.SplitReference;
			return string.Format(
				@"{0}
						<h3>{1} Response</h3>
							<h4>{2}{3}: {4}</h4>
							{5}	",
					MessagePrettierCss.CSS,
					cUSRESResponseData.DocumentNameCode,
					hawb.ReferenceNumber, splitNumber,
					cUSRESResponseData.CustomsActionText,
					headTableCreator.ToHtml()
				 );
		}

		void ProcessRemoval(CUSRESResponseData cUSRESResponseData, CusHAWB hawbOrHouseHelper, CusUnderbond underbond)
		{
			var awb = !hawbOrHouseHelper.CS_IsMasterHouse ? hawbOrHouseHelper : (ICcsukCusAwb)hawbOrHouseHelper.MAWB;
			if (awb.HasSplits)
			{
				var split = awb.Splits[cUSRESResponseData.SplitReference];
				if (split != null)
				{
					awb = split;
					incomingMessage.EM_ApplicationReference = cUSRESResponseData.SplitReference;
				}
			}

			var tsr = underbond as TranshipmentRemoval;
			if (!cUSRESResponseData.EntryNumber.IsEmpty && tsr != null)
			{
				if (!awb.HasSplits)
				{
					hawbOrHouseHelper.CS_TranshipmentEntryNum = cUSRESResponseData.EntryNumber;
				}
				tsr.TranshipmentEntryNumber = cUSRESResponseData.EntryNumber;
			}
		}

		void SendEmail(string body, ICcsukCusAwb hawbOrHawbHelper, ZString cusdecType, JobDeclaration dec)
		{
			body = string.Format("<h3>Response to {0} Request</h3> {1}", cusdecType, body);
			if (cusdecType == Fallback.RemovalCode && dec != null)
			{
				body = string.Format("{0}<p>A customs declaration has been {1} this job. It is accessible via the Shipment screen.</p>", body, (dec.IsInDatabase ? "linked to" : "created for"));
			}

			var subject = string.Format("{0} - response to {1} request", hawbOrHawbHelper.HumanReadableName, cusdecType);
			var emailSender = new CcsukEmailSender(incomingMessage.Factory,
													CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry,
													(BusinessObject)hawbOrHawbHelper,
													hawbOrHawbHelper.UserInChargeOfJob
												   );
			emailSender.SendEmail(subject, body,
					GBCustomsDataRegistry.Instance.GetNotificationItem(cusdecType, "", GBCustomsDataRegistry.Instance.NotificationCcsukCusresResponsesToCusdec), cusdecType,
					hawbOrHawbHelper.Branch.PK.ToGuid(), hawbOrHawbHelper.Branch.Company.PK.ToGuid(), Guid.Empty);
		}

		// public for GB.DocumentWrappers, don't make this private
		public static void LookupCusHawbAndCusUnderbondFromCommonAccessReference(string commonAccessReference, string removalCode, BusinessObjectFactory factory, out CusHAWB hawbOrHouseHelper, out CusUnderbond underbond, out EDIMessage outboundMessage)
		{
			try
			{
				var values = commonAccessReference.Split('/');
				var messageNum = values[0];
				var sendersMessageReference = values[1];
				var msgQuery = new ZQuery(EDIMessageSchema.EM_MessageNum, messageNum);
				msgQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				msgQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
				outboundMessage = factory.LoadTop1<EDIMessage>(msgQuery);
				hawbOrHouseHelper = (CusHAWB)outboundMessage.EM_LinkedObject;
				var underbondQuery = new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, sendersMessageReference);
				underbondQuery.AddToFilter(CusUnderbondSchema.C4_ParentID, hawbOrHouseHelper.PK);
				underbondQuery.AddToFilter(CusUnderbondSchema.C4_MovementReason, removalCode);
				underbond = factory.LoadTop1<CusUnderbond>(underbondQuery);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new FormatException("Failed to parse the Common Access Reference - it should be like 84/U00000001 (OutboundMessageNumber/SendersMessageReference). " + commonAccessReference, ex);
			}
		}

		readonly SegmentGroup edifactObject;
		readonly EDIMessage incomingMessage;
		readonly ILogger serviceLogger;
	}
}
