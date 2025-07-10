using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class GroupInvoiceCharge : EU.Business.Declaration.GroupInvoiceCharge, Integration.Customs.IE.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceGroupHeader GroupInvoice => (JobComInvoiceGroupHeader)base.GroupInvoice;

		protected override JobComInvHeaderChargeLookups GetNewLookups() =>
			GroupInvoice?.JobDeclaration is JobDeclaration declaration && declaration.IsImport ? new ImportGroupInvoiceChargeLookups(this) : base.GetNewLookups();
	}
}
