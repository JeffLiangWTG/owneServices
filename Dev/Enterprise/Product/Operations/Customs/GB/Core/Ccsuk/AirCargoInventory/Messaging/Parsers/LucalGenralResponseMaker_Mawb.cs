
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class LucalGenralResponseMaker_Mawb : LucalGenralResponseMaker_Success
	{
		public LucalGenralResponseMaker_Mawb(string requestText, IBusiness mainBusinessObject, IBusiness[] childrenBusinessObjects, EDIMessage inboundEdiMessage, ZString commonAccessReference)
			: base(requestText, mainBusinessObject, childrenBusinessObjects, inboundEdiMessage, commonAccessReference)
		{
		}

		internal override void MakeAndSendAllResponses()
		{
			if (IsResponseForImportFSR && childrenBusinessObjects.Length == 0)
			{
				SendFsaForParent(CIMFSR.MakeOsiTextForGoodResponseForSingleWholeRecord(mainBusinessObject as ICcsukCusAwb));
			}
			else
			{
				if (IsResponseForImportFSR)
				{
					SendFsaForParent(OsiLineForParent);
					SendGenralsFromOneLargePayload(MakeOneLargePayloadForAllChildren_ImportFSR(childrenBusinessObjects));
				}
				else if (IsResponseForExportGenralDEP)
				{
					SendGenralsFromOneLargePayload(MakeOneLargePayloadForAllChildren_ExportGenralDEP(childrenBusinessObjects));
				}
			}
		}

		bool IsResponseForImportFSR
		{
			get { return (mainBusinessObject as ICcsukCusAwb) != null; }
		}

		bool IsResponseForExportGenralDEP
		{
			get { return (mainBusinessObject as ForwardingConsol) != null; }
		}

		internal string OsiLineForParent = "SEE PRINTER SPOOLER";

		void SendFsaForParent(string osiText)
		{
			var how = new CcsukTransmissionMessageFunction.CIM.FSA.OSI(osiText, this.inboundEdiMessage, commonAccessReference);
			var manager = new CcsukInventoryMessageManager((BusinessObject)mainBusinessObject, how, new biz.SendsMessagesToCustomsShutterUpperer());
			manager.SendToCommunity();
		}

		protected string MakeOneLargePayloadForAllChildren_ImportFSR(IBusiness[] children)
		{
			var sb = new ZStringBuilder();
			if (children.Length > 0)
			{
				sb.Append(string.Format("START response to RS interrogation {0} from {1}", enquiryString, inboundEdiMessage.Interchange.EI_To.Replace("/", "").Right(6)));
				sb.Append("HAWB     SRF NPX NPR NPD DESC                GWT    AGT CAC DATE     EC");
				foreach (var child in children)
				{
					var hawb = child as CusHAWB;
					if (hawb != null)
					{
						var row = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}",
							hawb.CS_HAWB.TrimAndPad(8),
							"".TrimAndPad(3),  // SRF
							hawb.CS_PiecesManifested.ToString().TrimAndPad(3),
							hawb.CS_PiecesLanded.ToString().TrimAndPad(3),
							hawb.NumberOfPiecesDelivered.ToString().TrimAndPad(3), // NPD
							hawb.CS_GoodsDescription.TrimAndPad(19),
							hawb.CS_Weight.ToString("#####.#").TrimAndPad(6),
							hawb.AgentBadge.TrimAndPad(3),
							hawb.CustomsActionCode.TrimAndPad(3),
							hawb.CustomsActionDate.ToString("yyyyMMdd").TrimAndPad(8),
							"".TrimAndPad(2) // EC?? 
							);
						sb.Append(row);
					}
					else
					{
						var split = child as SplitConsignment;
						if (split != null)
						{
							var row = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}",
								GetReferenceNumber(split).TrimAndPad(8),
								split.SplitReference.TrimAndPad(3),  // SRF
								split.NumberOfPiecesExpected.ToString().TrimAndPad(3),
								split.NumberOfPiecesReceived.ToString().TrimAndPad(3),
								split.NumberOfPiecesDelivered.ToString().TrimAndPad(3), // NPD
								split.DescriptionOfGoods.TrimAndPad(19),
								split.Weight.ToString("#####.#").TrimAndPad(6),
								split.AgentBadge.TrimAndPad(3),
								split.CustomsActionCode.TrimAndPad(3),
								split.CustomsActionDate.ToString("yyyyMMdd").TrimAndPad(8),
								"".TrimAndPad(2) // EC?? 
								);
							sb.Append(row);
						}
					}
				}
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}

		ZString MakeOneLargePayloadForAllChildren_ExportGenralDEP(IBusiness[] childrenBusinessObjects)
		{
			var sb = new ZStringBuilder();
			sb.Append("ducr                                part  npx description     aod s");
			sb.Append("----------------------------------- ---- ---- --------------- --- -");
			var ducrAndRowPairs = new Dictionary<ZString, ZString>();
			foreach (var child in (from BusinessObject bo in childrenBusinessObjects orderby bo.HumanReadableName select bo))
			{
				var declaration = child as JobDeclaration;
				if (declaration != null)
				{
					if (declaration.ActiveEntryHeaders.Count > 0)
					{
						foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
						{
							ducrAndRowPairs.Add(entry.CH_BGMReference, FormatOneEntryRowForExportEntry(declaration, entry.PackagesCount, entry.CH_BGMReference));
						}
					}
					else
					{
						ducrAndRowPairs.Add(declaration.JE_UCR, FormatOneEntryRowForExportEntry(declaration, declaration.JE_TotalNoOfPacks, declaration.JE_UCR));
					}
				}
			}

			var sortedPairs = (from pair in ducrAndRowPairs orderby pair.Key ascending select pair);
			foreach (var p in sortedPairs)
			{
				sb.Append(p.Value);
			}

			return sb.ToStringWithNewLineBetweenAppends();
		}

		string FormatOneEntryRowForExportEntry(JobDeclaration declaration, int packages, ZString fullDucrAndPart)
		{
			var ducrAndPart = (new ZString(fullDucrAndPart + "/")).Split('/');
			var row = string.Format("{0} {1} {2} {3} {4} {5}",
								ducrAndPart[0].PadRight(35),
								(ducrAndPart[1].IsEmpty ? "" : "/" + ducrAndPart[1]).PadRight(4),
								packages.ToString().PadRight(4),
								declaration.JE_GoodsDescription.Left(15).PadRight(15),
								declaration.JE_RL_NKOrigin.Right(3).PadRight(3),
								declaration.ZG_StyleOfEntrySOE.PadRight(1)
								);
			return row;
		}

		ZString GetReferenceNumber(SplitConsignment split)
		{
			var splitHouse = split as SplitHouse;
			return splitHouse != null ? ((CusHAWB)splitHouse.AWB).CS_HAWB : new ZString("(basic)");
		}
	}
}
