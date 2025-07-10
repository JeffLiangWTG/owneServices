using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class GroupInvoiceCharge : EU.Business.Declaration.GroupInvoiceCharge, Integration.Customs.ES.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new GroupInvoiceChargeLookups Lookups => (GroupInvoiceChargeLookups)base.Lookups;

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new GroupInvoiceChargeLookups(this);

		public new GroupInvoiceChargeValidation Validation => (GroupInvoiceChargeValidation)base.Validation;

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new GroupInvoiceChargeValidation(this);

		protected override bool GetIncludedInITOTReadOnly()
		{
			return !IsExport && base.GetIncludedInITOTReadOnly();
		}

		ZBool IsExport => GroupInvoice?.JobDeclaration.IsExport ?? ZBool.False;
	}
}
