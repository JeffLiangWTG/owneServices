using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Messaging.Business;
using biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFSR : CargoImpBase, ICimParser
	{
		/// <summary>
		/// For parsing inbound messages
		/// </summary>
		public CIMFSR(List<string> list, EDIMessage inboundMessage, string commonAccessReference)
			: base(new ErrorCollector())
		{
			linesOfInboundCargoImp = list;
			this.inboundMessage = inboundMessage;
			this.commonAccessReference = commonAccessReference;
		}

		/// <summary>
		/// For making new outbound messages
		/// </summary>
		public CIMFSR(string formattedMasterAirWaybillNumber, ErrorCollector ec)
			: base(ec)
		{
			mawbNumber = formattedMasterAirWaybillNumber;
			DemandFieldsNotEmpty("AWB number", mawbNumber);
		}

		public const string Code = "FSR";

		public override ZString CargoImpCode
		{
			get { return Code; }
		}

		protected override string[] CargoImpLinesWithoutType
		{
			get { return new string[] { mawbNumber }; }
		}

		public override ZString MessageInterpretation
		{
			get
			{
				return MessagePrettierCss.CSS + (inboundMessage != null
													? "<h3>FSR enquiry from " + inboundMessage.Interchange.EI_From
													: "<h3>FSR enquiry for " + mawbNumber
												 ) + "</h3>";
			}
		}

		public bool DoAllProcessingBeforePrinting()
		{
			var consignmentNumber = GetConsignmentNumberFromFsr();
			var recipientPima = inboundMessage.Interchange != null ? inboundMessage.Interchange.EI_To.Replace("/", "") : ZString.Empty;
			Awb = FindAwbFromConsignmentNumber(consignmentNumber, recipientPima);

			string osiText = "";
			if (Awb == null)
			{
				osiText = "NO RECORD FOUND";
				inboundMessage.EM_ApplicationReference = consignmentNumber;
				new CcsukInventoryMessageManager(inboundMessage, new CcsukTransmissionMessageFunction.CIM.FSA.OSI(osiText, inboundMessage, this.commonAccessReference), new biz.SendsMessagesToCustomsShutterUpperer()).SendToCommunity();
			}
			else
			{
				Awb.Messages.Add(inboundMessage);
				inboundMessage.EM_GB = Awb.Branch.PK;
				inboundMessage.EM_ApplicationReference = consignmentNumber;

				if (Awb is CusMAWB)
				{
					var mawbOrBasic = Awb as CusMAWB;
					if (mawbOrBasic.IsBasic)
					{
						if (mawbOrBasic.HasSplits)
						{
							FindChildrenAndSendFsaSayingSeePrintSpoolThenSendGenralForChildren(mawbOrBasic, "Basic consignment with splits, see print spool");
						}
						else
						{
							SendFsaMessageBasedOnConsignment();
						}
					}
					else
					{
						FindChildrenAndSendFsaSayingSeePrintSpoolThenSendGenralForChildren(Awb, "Master air waybill consignment, see print spool");
					}
				}
				else if (Awb is CusHAWB)
				{
					if (Awb.HasSplits)
					{
						FindChildrenAndSendFsaSayingSeePrintSpoolThenSendGenralForChildren(Awb, "House consignment with splits, see print spool");
					}
					else
					{
						osiText = MakeOsiTextForGoodResponseForSingleWholeRecord(Awb);
						new CcsukInventoryMessageManager(Awb, new CcsukTransmissionMessageFunction.CIM.FSA(osiText, inboundMessage, this.commonAccessReference), new biz.SendsMessagesToCustomsShutterUpperer()).SendToCommunity();
					}
				}
			}
			return true;
		}

		void FindChildrenAndSendFsaSayingSeePrintSpoolThenSendGenralForChildren(ICcsukCusAwb mawbOrBasicAwb, string osiLine)
		{
			IBusiness[] children = null;
			LucasGenralEnquiryHandler.FindChildrenAndGrandChildrenOfMawbOrBasic_Import(mawbOrBasicAwb, out children);
			var fsaAndGenralMaker = new LucalGenralResponseMaker_Mawb(mawbOrBasicAwb.ReferenceNumber, mawbOrBasicAwb, children, inboundMessage, commonAccessReference);
			var osiLineWithSdc = string.Format("SDC={0} {1}", mawbOrBasicAwb.ShipmentDescriptionCode, osiLine);
			fsaAndGenralMaker.OsiLineForParent = osiLineWithSdc;
			fsaAndGenralMaker.MakeAndSendAllResponses();
		}

		void SendFsaMessageBasedOnConsignment()
		{
			var osiText = MakeOsiTextForGoodResponseForSingleWholeRecord(Awb);
			CcsukTransmissionMessageFunction.CIM.FSA messageFunction = new CcsukTransmissionMessageFunction.CIM.FSA.OSI(osiText, inboundMessage, this.commonAccessReference);
			new CcsukInventoryMessageManager(Awb, messageFunction, new biz.SendsMessagesToCustomsShutterUpperer()).SendToCommunity();
		}

		internal static ZString MakeOsiTextForGoodResponseForSingleWholeRecord(ICcsukCusAwb awb)
		{
			var stringBuilder = new ZStringBuilder();
			stringBuilder.AppendIfNotEmpty(ZString.Format("SDC {0}", awb.ShipmentDescriptionCode));
			stringBuilder.AppendIfNotEmpty(awb.CustomsActionCode);
			stringBuilder.AppendIfNotEmpty(awb.LatestCustomsActionText);
			stringBuilder.AppendIfNotEmpty(awb.CustomsActionDate.ToString("dd MMM HHmm"));
			stringBuilder.AppendIfNotEmpty(awb.NumberOfPiecesReceived.IsEmpty ? "" : string.Format("NPR={0}", awb.NumberOfPiecesReceived));
			stringBuilder.AppendIfNotEmpty(awb.NumberOfPiecesDelivered.IsEmpty ? "" : string.Format("NPD={0}", awb.NumberOfPiecesDelivered));
			return stringBuilder.ToStringWithDelimiterBetweenAppends(" ").TrimEnd();
		}

		string GetConsignmentNumberFromFsr()
		{
			return linesOfInboundCargoImp[1];
		}

		ICcsukCusAwb FindAwbFromConsignmentNumber(ZString serialNumber, ZString recipientPima)
		{
			var elements = serialNumber.Split('-');
			if (elements.Length == 2)
			{
				// 111-2222222 --> mawb
				if (elements[0] != "HWB")
				{
					var loader = new CusMAWB.Loader(inboundMessage.Factory);
					var mawbNum = elements[0] + elements[1];
					var mawbs = loader.FindMAWBsFromMawbNumber(mawbNum);
					if (mawbs != null)
					{
						if (mawbs.Length == 1)
						{
							return mawbs[0];
						}
						else
						{
							if (LicenceAndPimaHelper.IsShedPIMA(recipientPima))
							{
								var airportAndShed = recipientPima.Right(6);
								return loader.FindFromMawbNumber(mawbNum, airportAndShed);
							}
							else if (LicenceAndPimaHelper.IsAgentPIMA(recipientPima))
							{
								var agent = recipientPima.Right(3);
								return loader.FindFromMawbNumberAndAgentBadge(mawbNum, agent);
							}
						}
					}
				}
				else
				{
					// HWB-3333333 --> hawb
					return new CusHAWB.Loader(inboundMessage.Factory).FindHawb(elements[1], "", "");
				}
			}
			return null;
		}

		public ICcsukCusAwb Awb { get; private set; }

		public void DoPrinting()
		{
		}

		readonly List<string> linesOfInboundCargoImp;
		readonly EDIMessage inboundMessage;
		readonly string mawbNumber;
		readonly string commonAccessReference;
	}
}
