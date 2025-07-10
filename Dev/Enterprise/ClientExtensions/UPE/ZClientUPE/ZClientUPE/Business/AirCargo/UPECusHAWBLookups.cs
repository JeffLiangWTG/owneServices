using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusHAWBLookups : CusHAWBLookups
	{
		public UPECusHAWBLookups(UPECusHAWB parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PrepaidCollectList
		{
			get { return new BillingTermsCodeDescriptionPairList(); }
		}

		public override CodeDescriptionPairList PrepaidCollectListForValidation
		{
			get { return base.PrepaidCollectList; }
		}

		public override ReadOnlyCodeDescriptionPairList ShipmentTypeList
		{
			get { return new ShipmentTypeCodeDescriptionPairList(); }
		}

		public CodeDescriptionPairList DutyTypeList
		{
			get { return new DutyTypeCodeDescriptionPairList(); }
		}

		public CodeDescriptionPairList HoldForCollectDepotList
		{
			get { return new HFCDepotCodeDescriptionPairList(); }
		}
	}
}
