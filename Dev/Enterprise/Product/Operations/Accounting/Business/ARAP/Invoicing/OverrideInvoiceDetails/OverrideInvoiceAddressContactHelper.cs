using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideInvoiceAddressContactHelper : OverrideInvoiceDetailsHelper
	{
		public OverrideInvoiceAddressContactHelper(BusinessObjectFactory factory, params ZGuid[] transactionPKs)
			: this(factory, null, true, transactionPKs)
		{
		}

		public OverrideInvoiceAddressContactHelper(BusinessObjectFactory formFactory, BusinessObjectFactory parentFactory, bool canBizOBeSaved, params ZGuid[] transactionPKs)
			: base(formFactory, parentFactory, canBizOBeSaved, transactionPKs)
		{
		}

		protected override string[] ColumnsToOverride()
		{
			return new string[]
			{
				"DisplayInvoiceAddressOverride",
				"DisplayInvoiceContactOverride"
			};
		}

		protected override void SetBusinessContext(BusinessObject bizObj)
		{
			bizObj.SetContext(BusinessContext.OverrideInvoiceAddressContact);
		}
	}
}
