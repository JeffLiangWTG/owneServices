using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business
{
	public class GroupInvoiceCharge : Customs.Business.BaseGroupInvoiceCharge, Integration.Customs.KR.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		protected override JobComInvHeaderChargeLookups GetNewLookups() => new GroupInvoiceChargeLookups(this);
		protected override JobComInvHeaderChargeValidation GetNewValidation() => new GroupInvoiceChargeValidation(this);

		[ResourceStringData("A11FA87F-0499-4DAD-A803-DF4A7A1EEB22", Caption = "Code", FullDescription = "Select the charge code type related to the commercial invoice to consider in valuation. Example: A CIF Invoice, requires Freight, and Insurance Charge codes to correctly value goods for customs purposes.")]
		public override ZString J7_ChargeType { get => base.J7_ChargeType; set => base.J7_ChargeType = value; }

		public new JobComInvoiceGroupHeader Parent => (JobComInvoiceGroupHeader)base.Parent;
	}
}
