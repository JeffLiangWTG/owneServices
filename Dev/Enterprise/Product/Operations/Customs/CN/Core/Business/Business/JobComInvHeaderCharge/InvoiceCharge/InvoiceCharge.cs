using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using ECC = Enterprise.Customs.Common;

namespace Enterprise.Customs.CN.Business
{
	public partial class InvoiceCharge : AutoInvoiceCharge, Integration.Customs.CN.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ECC.ICustomsChargeCode charge) => !incoTerm.IsEmpty && IsEmpty
			&& (charge?.Code == CustomsChargeTypeList.Codes.OtherCharges || charge?.Code == CustomsChargeTypeList.Codes.Royalty);

		protected override bool ShouldResetDefaultIsIncludedInITOT(ZString incoTerm) => !incoTerm.IsEmpty && IsEmpty;

		public override ZBool AllowNonWesternEuropeanCharacterForChargeDescription => true;
	}
}
