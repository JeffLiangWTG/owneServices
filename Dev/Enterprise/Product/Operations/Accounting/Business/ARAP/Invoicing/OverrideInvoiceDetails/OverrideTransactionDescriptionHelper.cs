using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideTransactionDescriptionHelper : OverrideInvoiceDetailsHelper
	{
		public OverrideTransactionDescriptionHelper(BusinessObjectFactory factory, params ZGuid[] invoicePKs)
			: base(factory, invoicePKs)
		{
		}

		protected override string[] ColumnsToOverride()
		{
			return new string[] { AccTransactionHeaderSchema.AH_Desc.Name };
		}

		protected override void SetBusinessContext(BusinessObject bizObj)
		{
			bizObj.SetContext(BusinessContext.OverrideTransactionDescription);
		}
	}
}