using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideGovernmentAllocatedNumberHelper : OverrideInvoiceDetailsHelper
	{
		public OverrideGovernmentAllocatedNumberHelper(BusinessObjectFactory factory, params ZGuid[] transactionPKs)
			: this(factory, null, true, transactionPKs)
		{
		}

		public OverrideGovernmentAllocatedNumberHelper(BusinessObjectFactory formFactory, BusinessObjectFactory parentFactory, bool canBizOBeSaved, params ZGuid[] transactionPKs)
			: base(formFactory, parentFactory, canBizOBeSaved, transactionPKs)
		{
		}

		protected override string[] ColumnsToOverride()
		{
			return new string[]
			{
				AccTransactionHeaderSchema.AH_GovernmentAllocatedID.Name
			};
		}

		protected override void SetBusinessContext(BusinessObject bizObj)
		{
			bizObj.SetContext(BusinessContext.OverrideGovernmentAllocatedID);
		}
	}
}
