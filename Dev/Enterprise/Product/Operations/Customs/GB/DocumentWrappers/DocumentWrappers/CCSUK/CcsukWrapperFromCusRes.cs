using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapperFromCusRes : CcsukWrapper
	{
		public CcsukWrapperFromCusRes(EDIMessage message, BusinessObjectFactory factoryToWrap)
			: base(message, message.EM_LinkedObject, factoryToWrap)
		{
			var parser = new CUSRESInboundParser(ediMessage);
			cusRes = parser.ParseCUSRESForListOfUpdatedFields();
			EDIMessage outboundMessage = null;
			CUSRESParserAndProcessor.LookupCusHawbAndCusUnderbondFromCommonAccessReference(cusRes.CommonAccessReference, cusRes.DocumentNameCode, ediMessage.Factory, out houseBill, out underbond, out outboundMessage);
			mawb = houseBill.MAWB;
		}

		protected override ZString MawbHawbSplitCore
		{
			get
			{
				if (cusRes != null)
				{
					var mawbNumberFormatted = ZString.Format("{0}-{1}", cusRes.AirWaybillPrefixAndNumber.Left(3), cusRes.AirWaybillPrefixAndNumber.Right(8));
					var mawbAndHawb = mawbNumberFormatted + (cusRes.HouseWaybillNumber.IsEmpty ? "" : "-" + cusRes.HouseWaybillNumber);
					return cusRes.SplitReference.IsEmpty ? mawbAndHawb : (mawbAndHawb + "/" + cusRes.SplitReference);
				}
				else
				{
					return base.MawbHawbSplitCore;
				}
			}
		}

		protected override ZString NewShedCore
		{
			get { return cusRes.ShedId; }
		}

		protected override ZString AgentNameCore
		{
			get { return cusRes.AgentName; }
		}

		protected override ZString AgentPhoneCore
		{
			get { return cusRes.AgentsTelephoneNumber; }
		}

		protected override ZString POSCore
		{
			get
			{
				var transhipment = underbond as TranshipmentRemoval;
				return transhipment != null ? transhipment.PortOfShipment : ZString.Empty;
			}
		}

		protected override ZString TrnCore
		{
			get { return underbond is TranshipmentRemoval ? cusRes.EntryNumber : ZString.Empty; }
		}

		protected override ZInt PiecesRelevantToThisRendering
		{
			get { return cusRes.NoOfPackagesExpected; }
		}

		protected override ZBool NeedsT1Statement
		{
			get { return underbond is TranshipmentRemoval; }
		}

		protected override bool IsLicenceIndicatorCore
		{
			get
			{
				var transhipment = underbond as TranshipmentRemoval;
				return transhipment != null && transhipment.LicenseRestrictionInd == Enterprise.Customs.Business.YesNoList.Codes.Yes;
			}
		}

		protected override ZString CooCore
		{
			get { return cusRes.CountryOfOrigin; }
		}

		protected override ZString EntryCore
		{
			get { return FormatEntryNumber(cusRes.EntryNumber); }
		}

		protected override ZString EntryDateCore
		{
			get { return cusRes.EntryDate.ToString("dd/MM/yy"); }
		}

		protected override ZString AgentRefCore
		{
			get { return cusRes.AgentsReferenceNumber.IsEmpty ? underbond.AgentsReference : cusRes.AgentsReferenceNumber; }
		}

		ZString FormatEntryNumber(ZString entryNumber)
		{
			return ZString.Format("{0}-{1}", entryNumber.Left(3), entryNumber.SubstringSafe(3));
		}

		protected override ZString RemovalTypeCore
		{
			get { return underbond != null ? underbond.RemovalTypeHuman : ZString.Empty; }
		}

		protected override ZString CatCore()
		{
			return cusRes.CustomsActionText;
		}

		protected override ZString CacCore()
		{
			return cusRes.CustomsActionCode_StatusOfRequest;
		}

		readonly CusUnderbond underbond;
		readonly CUSRESResponseData cusRes;
	}
}
