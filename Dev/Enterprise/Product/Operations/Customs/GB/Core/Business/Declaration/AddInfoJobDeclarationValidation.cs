using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public partial class JobDeclarationValidation
	{
		protected override void CheckJE_UsePostponedVatAccounting()
		{
			base.CheckJE_UsePostponedVatAccounting();

			var parent = Parent;
			if (parent.JE_UsePostponedVatAccounting && !parent.ZG_VATDeferType.IsEmpty)
			{
				parent.JE_UsePostponedVatAccountingInfo.AddMessageError("This field is mutually exclusive with VAT field.");
			}
		}

		protected override void CheckJE_VATDeferType()
		{
			base.CheckJE_VATDeferType();

			var parent = Parent;
			if (parent.ZG_UsePostponedVatAccounting && !parent.ZG_VATDeferType.IsEmpty)
			{
				parent.ZG_VATDeferTypeInfo.AddMessageError("This field is mutually exclusive with 'Use postponed VAT accounting?' field.");
			}
		}
		public static ResourceString CSPZG_GatewayValidationError => ResString.GetMultilingualString("727190F6-DBDC-4799-BA2A-DA3B85263594", "It is not possible to transmit to Customs when the gateway field is blank. Set this field indirectly by selecting a valid profile.");
	}
}
