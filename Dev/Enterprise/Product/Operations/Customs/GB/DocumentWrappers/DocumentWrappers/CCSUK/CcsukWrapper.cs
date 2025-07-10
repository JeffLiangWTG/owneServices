using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapper : DocBaseWrapper, Integration.Customs.GB.ICcsukCusunderbondDocumentProvider, IDocTypeCode
	{
		public CcsukWrapper(ICcsukCusAwb awb, BusinessObjectFactory factoryToWrap)
			: base(awb, factoryToWrap)
		{
			iCcsukCusAwb = awb;
			if (awb is CusHAWB)
			{
				houseBill = (CusHAWB)awb;
				mawb = houseBill.MAWB;
			}
			else
			{
				InitialiseAwbsExceptHawb((BusinessObject)awb);
			}
		}

		public CcsukWrapper(EDIMessage message, BusinessObject awb, BusinessObjectFactory factoryToWrap)
			: base(awb, factoryToWrap)
		{
			this.ediMessage = message;
			if (awb != null)
			{
				if (awb is CusHAWB)
				{
					houseBill = (CusHAWB)awb;
					mawb = houseBill.MAWB;
					if (houseBill.CS_IsMasterHouse && mawb.IsBasic && mawb.HasSplits && message != null)
					{
						iCcsukCusAwb = mawb.Splits[message.EM_ApplicationReference];
					}
					else if (!houseBill.CS_IsMasterHouse && houseBill.HasSplits && message != null)
					{
						iCcsukCusAwb = houseBill.Splits[message.EM_ApplicationReference];
					}
					else if (houseBill.CS_IsMasterHouse)
					{
						iCcsukCusAwb = mawb;
					}
					else
					{
						iCcsukCusAwb = houseBill;
					}

					if (iCcsukCusAwb == null)
					{
						iCcsukCusAwb = (ICcsukCusAwb)awb;
					}
				}
				else
				{
					InitialiseAwbsExceptHawb(awb);
				}
			}
		}

		void InitialiseAwbsExceptHawb(BusinessObject awb)
		{
			if (awb is CusMAWB)
			{
				mawb = (CusMAWB)awb;
				houseBill = mawb.MasterLevelHouseHelper;
				iCcsukCusAwb = mawb;
			}
			else if (awb is SplitHouse)
			{
				var split = (SplitHouse)awb;
				houseBill = split.HAWB;
				mawb = split.HAWB.MAWB;
				iCcsukCusAwb = split;
			}
			else if (awb is SplitBasic)
			{
				var split = (SplitBasic)awb;
				houseBill = split.Basic.MasterLevelHouseHelper;
				mawb = split.Basic;
				iCcsukCusAwb = split;
			}
			else
			{
				throw new NotSupportedException(string.Format("Cannot create a CcsukWrapper for that business object, add a case. BizO={0}", awb));
			}
		}

		public static CcsukWrapper New(BusinessObject bizO, BusinessObjectFactory factoryToWrap)
		{
			var ediMessage = bizO as GbEDIMessage;
			if (ediMessage != null)
			{
				switch (ediMessage.EM_MessageType + ediMessage.EM_MessageSubType)
				{
					case CcsukTransmissionMessageFunction.CIM.Code + CcsukTransmissionMessageFunction.CIM.CUKFSR.FSN.Subcode: //CIMFSN
						return new CcsukWrapperFromFsn(ediMessage, ediMessage.Factory);
					case CcsukTransmissionMessageFunction.CUSCAR.Code + CcsukTransmissionMessageFunction.CUSCAR.FRC.Subcode: //CARFRC
						return new CcsukWrapperFromCuscarFrc(ediMessage, ediMessage.Factory);
					case "RESIAR":
					case "RESISR":
					case "RESTSR":
					case "RESFBK":
						return new CcsukWrapperFromCusRes(ediMessage, ediMessage.Factory);
					case "FSAP5":
						return new CcsukWrapperForP5(ediMessage, ediMessage.Factory);
				}
			}
			else
			{
				var hawb = bizO as CusHAWB;
				if (hawb != null)
				{
					return new CcsukWrapper(hawb, bizO.Factory);
				}
				else
				{
					var mawb = bizO as CusMAWB;
					if (mawb != null)
					{
						return new CcsukWrapper(mawb.MasterLevelHouseHelper, bizO.Factory);
					}
					else
					{
						var splitHouse = bizO as SplitHouse;
						if (splitHouse != null)
						{
							return new CcsukWrapper(splitHouse, bizO.Factory);
						}
						else
						{
							var splitBasic = bizO as SplitBasic;
							if (splitBasic != null)
							{
								return new CcsukWrapper(splitBasic, bizO.Factory);
							}
						}
					}
				}
			}

			return null;
		}

		#region public wrapped properties

		public ZString AOO
		{
			get { return PortConverter.UnlocoToIata(iCcsukCusAwb.AirportOfOrigin, iCcsukCusAwb.Factory); }
		}

		public ZString AOD
		{
			get { return iCcsukCusAwb.AirportOfDestination; }
		}

		public ZString SHED
		{
			get { return iCcsukCusAwb.CargoTerminalOperator; }
		}

		public ZString NEWSHED
		{
			get { return NewShedCore; }
		}

		public ZString MAWBHAWBSPLIT
		{
			get { return MawbHawbSplitCore; }
		}

		protected virtual ZString MawbHawbSplitCore
		{
			get
			{
				if (ediMessage != null)
				{
					var superiorRecord = !houseBill.CS_IsMasterHouse ? houseBill.ReferenceNumber : houseBill.MAWB.ReferenceNumber;
					return (ediMessage.EM_ApplicationReference.Length == 2) ? new ZString(superiorRecord + "/" + ediMessage.EM_ApplicationReference) : superiorRecord;
				}
				else
				{
					return
					iCcsukCusAwb.ReferenceNumber;
				}
			}
		}

		public ZString AIRPORT
		{
			get { return iCcsukCusAwb.CargoTerminalOperatorAirport; }
		}

		public ZString BADGE
		{
			get { return iCcsukCusAwb.AgentBadge; }
		}

		public ZString AGENTNAME
		{
			get { return AgentNameCore; }
		}

		public ZString WEIGHT
		{
			get { return iCcsukCusAwb.Weight.ToStringTrimZeros() + iCcsukCusAwb.WeightCode; }
		}

		public ZString DESCRIPTION
		{
			get { return houseBill.CS_GoodsDescription; }
		}

		public ZString CAC
		{
			get { return CacCore(); }
		}

		protected virtual ZString CacCore()
		{
			return iCcsukCusAwb.CustomsActionCode;
		}

		public ZString CAT
		{
			get { return CatCore(); }
		}

		protected virtual ZString CatCore()
		{
			return iCcsukCusAwb.LatestCustomsActionText;
		}

		public ZString CAD
		{
			get { return iCcsukCusAwb.CustomsActionDate.ToString("dd-MMM-yyyy HH:mm"); }
		}

		public ZString AGENTREF
		{
			get { return AgentRefCore; }
		}

		public ZString CURRENCY
		{
			get { return houseBill.CS_RX_NKGoodsCurrency; }
		}

		public ZString VALUE
		{
			get { return houseBill.CS_GoodsValue.IsEmpty ? ZString.Empty : houseBill.CS_GoodsValue.ToStringTrimZeros(0); }
		}

		public ZString POS // Port of shipment
		{
			get { return POSCore; }
		}

		public ZString REMARKS
		{
			get { return RemarksCore; }
		}

		protected virtual ZInt PiecesRelevantToThisRendering
		{
			get
			{
				int piecesToPrint = iCcsukCusAwb.NumberOfPiecesReceived;
				var mostRecentPiecesReleased = NumberOfPiecesReleasedHelper.LastPiecesReleased(iCcsukCusAwb, EventCode);
				if (mostRecentPiecesReleased != 0)
				{
					// Get part release count from dbo.StmALog
					piecesToPrint = mostRecentPiecesReleased;
				}
				return piecesToPrint;
			}
		}

		public ZString PARTRELEASE
		{
			get
			{
				var caption = "";
				if (PiecesRelevantToThisRendering < iCcsukCusAwb.NumberOfPiecesExpected)
				{
					caption = (iCcsukCusAwb.NumberOfPiecesExpected == iCcsukCusAwb.NumberOfPiecesReleasedSoFarCumulative(EventCode)) ? "LAST PART RELEASE" : "PART RELEASE";
				}
				return caption;
			}
		}

		protected virtual Event EventCode
		{
			get
			{
				GbEDIMessage fsnMessage = null;
				if (ediMessage != null)
				{
					fsnMessage = iCcsukCusAwb.Factory.Load<GbEDIMessage>(ediMessage.PK);
				}

				return fsnMessage != null && ReleasePrintHelper.IsSentToRightShed(fsnMessage, iCcsukCusAwb) ? NumberOfPiecesReleasedHelper.ShedEvent
																					  : fsnMessage != null && ReleasePrintHelper.IsSentToRightAgent(fsnMessage, iCcsukCusAwb) ? NumberOfPiecesReleasedHelper.AgentC1Event
																																						: iCcsukCusAwb.Profile.StartsWith(LicenceAndPimaHelper.ShedProfilePrefix, StringComparison.OrdinalIgnoreCase) ? NumberOfPiecesReleasedHelper.ShedEvent
																																																																	  : NumberOfPiecesReleasedHelper.AgentC1Event;
			}
		}

		public ZString PARTIALNOP
		{
			get
			{
				return PiecesRelevantToThisRendering < iCcsukCusAwb.NumberOfPiecesExpected
					? string.Format("PART RELEASE FOR {0} OF {1} PACKAGES ONLY", PiecesRelevantToThisRendering, iCcsukCusAwb.NumberOfPiecesExpected)
					: "";
			}
		}

		public ZString TRN  // Transit reference number
		{
			get { return TrnCore; }
		}

		public ZString COO // country of origin
		{
			get { return CooCore; }
		}

		public ZString NPD
		{
			get { return iCcsukCusAwb.NumberOfPiecesDelivered.ToString(); }
		}

		public ZString NPX
		{
			get { return iCcsukCusAwb.NumberOfPiecesExpected.ToString(); }
		}

		public ZString NPR
		{
			get { return iCcsukCusAwb.NumberOfPiecesReceived.ToString(); }
		}

		public ZString ENTRY
		{
			get { return EntryCore; }
		}

		public ZString ENTRYDATE
		{
			get { return EntryDateCore; }
		}

		public ZString AGENTPHONE
		{
			get { return AgentPhoneCore; }
		}

		public ZString REMOVALTYPE
		{
			get { return RemovalTypeCore; }
		}

		public ZString LICENCEINDICATOR1
		{
			get { return IsLicenceIndicatorCore ? "LICENCE/RESTRICTED" : ""; }
		}

		public ZString LICENCEINDICATOR2
		{
			get { return IsLicenceIndicatorCore ? "INDICATOR DECLARED" : ""; }
		}

		public ZString T1STATEMENT
		{
			get { return NeedsT1Statement ? "* GOODS ARE ONLY ELIGIBLE FOR EXPORT AS T1 STATUS UNDER THIS PROCEDURE *" : ""; }
		}

		public ZString SDC
		{
			get { return iCcsukCusAwb.ShipmentDescriptionCode; }
		}

		#endregion

		#region For Release/Removal Authority RRA

		public CcsukWrapperForDeliveryOrTransferLineCollection OutTurnLines
		{
			get { return outTurnLines ?? (outTurnLines = new CcsukWrapperForDeliveryOrTransferLineCollection(iCcsukCusAwb, iCcsukCusAwb.Factory)); }
		}
		CcsukWrapperForDeliveryOrTransferLineCollection outTurnLines;

		public ZString RELEASEORREMOVALTITLE
		{
			get
			{
				switch (CAC)
				{
					case CustomsStatusCodes.Codes.ReleasedForInterShedRemoval:
					case CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval:
					case CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval:
						return "REMOVAL AUTHORITY";
					default:
						return "RELEASE NOTE";
				}
			}
		}

		public ZString RELEASEONLYTONEWSHED
		{
			get
			{
				switch (CAC)
				{
					case CustomsStatusCodes.Codes.ReleasedForInterShedRemoval:
						return "Goods must only be released to the new shed operator";
					default:
						return "";
				}
			}
		}

		#region Bar code for scanning
		public ZString SCANNINGBARCODERAW
		{
			get { return DocManagerBarcode.TextToEncode; }
		}

		public ZString SCANNINGBARCODEENCODED
		{
			get { return DocManagerBarcode.TextAs128sFontString; }
		}

		protected override TextBarcode DocManagerBarcode
		{
			get
			{
				if (fDocManagerBarcode == null)
				{
					var barCodeGenerator = new BarcodeGenerator();
					var docManagerCode = houseBill.CS_IsMasterHouse ? Enterprise.Core.Constants.DocManagerCodes.AirCargoMaster : Enterprise.Core.Constants.DocManagerCodes.AirCargoHouse;  // ACH or ACG for barcode's "^ACx=.....|"
					fDocManagerBarcode = barCodeGenerator.CreateDocumentBarcode(docManagerCode, DocManagerUniqueID, ((IDocTypeCode)this).DocTypeCode + ";");
				}
				return fDocManagerBarcode;
			}
		}
		TextBarcode fDocManagerBarcode;

		protected override ZString DocManagerUniqueID
		{
			get { return houseBill.CS_IsMasterHouse ? houseBill.MAWB.PK.ToString() : houseBill.PK.ToString(); }
		}

		ZString IDocTypeCode.DocTypeCode
		{
			get { return docTypeCode; }
			set { docTypeCode = value; }
		}
		ZString docTypeCode;

		#endregion

		#endregion

		public ZString RECIPIENT
		{
			get
			{
				return (ediMessage != null && ediMessage.Interchange != null && !ediMessage.Interchange.EI_To.IsEmpty)
						? "for " + ediMessage.Interchange.EI_To.Right(6)
						: "";
			}
		}

		public BusinessObjectCollection OutTurns
		{
			get { return iCcsukCusAwb.OutTurnsCollection; }
		}

		#region virtuals

		protected virtual ZString RemovalTypeCore
		{
			get { return ""; }
		}

		protected virtual ZBool NeedsT1Statement
		{
			get { return false; }
		}

		protected virtual ZString NewShedCore
		{
			get { return ""; }
		}

		protected virtual ZString AgentNameCore
		{
			get { return Utilities.GetAgentNameFromDatabase(iCcsukCusAwb); }
		}

		protected virtual ZString AgentRefCore
		{
			get
			{
				var result = ZString.Empty;
				if (houseBill != null && houseBill.Shipment != null)
				{
					result = houseBill.Shipment.JS_UniqueConsignRef;
				}
				else if (mawb != null && mawb.Consol != null)
				{
					result = mawb.Consol.JK_UniqueConsignRef;
				}
				else if (houseBill != null && houseBill.Declaration != null)
				{
					result = houseBill.Declaration.JE_DeclarationReference;
				}
				return result;
			}
		}

		protected virtual ZString POSCore
		{
			get { return ""; }
		}

		protected virtual ZString RemarksCore
		{
			get { return ""; }
		}

		protected virtual ZString TrnCore
		{
			get { return ""; }
		}

		protected virtual ZString CooCore
		{
			get { return ""; }
		}

		protected virtual ZString EntryCore
		{
			get { return ""; }
		}

		protected virtual ZString EntryDateCore
		{
			get { return ""; }
		}

		protected virtual ZString AgentPhoneCore
		{
			get { return ""; }
		}

		protected virtual bool IsLicenceIndicatorCore
		{
			get { return false; }
		}

		#endregion

		public ZString ReportTypeCode
		{
			get { return REMOVALTYPE; }
		}

		protected ICcsukCusAwb iCcsukCusAwb;
		protected CusHAWB houseBill;
		public CusHAWB Hawb
		{
			get { return houseBill; }
		}
		protected CusMAWB mawb;
		protected EDIMessage ediMessage;
	}
}

