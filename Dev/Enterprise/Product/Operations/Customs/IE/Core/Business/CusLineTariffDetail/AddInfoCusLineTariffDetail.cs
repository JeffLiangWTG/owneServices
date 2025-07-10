using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business
{
	public class AddInfoCusLineTariffDetail : EU.Business.AddInfoCusLineTariffDetail
	{
		public AddInfoCusLineTariffDetail(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new AddInfoCusLineTariffDetailValidation Validation => (AddInfoCusLineTariffDetailValidation)base.Validation;

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoCusLineTariffDetailValidation(this);

		public new AddInfoCusLineTariffDetailLookups Lookups => (AddInfoCusLineTariffDetailLookups)base.Lookups;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusLineTariffDetailLookups(this);

		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

		[List(nameof(Lookups) + "." + nameof(AddInfoCusLineTariffDetailLookups.PaymentMethodList))]
		public override ZString ZG_MethodOfPayment { get => base.ZG_MethodOfPayment; set => base.ZG_MethodOfPayment = value; }

		public override ZPropertyInfo ZG_MethodOfPaymentInfo => GetZPropertyInfo(nameof(ZG_MethodOfPayment));
	}
}
