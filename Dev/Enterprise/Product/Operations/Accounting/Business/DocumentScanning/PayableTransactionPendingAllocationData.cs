using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(PayableTransactionPendingAllocationData),
	Enterprise.Core.Constants.DocManagerCodes.PayableTransactionPendingAllocation)]

namespace Enterprise.Accounting.Business
{
	public class PayableTransactionPendingAllocationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ARAP.Invoicing.TransactionPendingAllocation); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("e66cb52b-d10b-42dc-bac2-d1ef2f10f3d4", "AP Transaction Pending Allocation"); } }
		public override ModuleIdentifier ModuleID => ModuleIDs.TransactionsPendingAllocation;

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			var filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.TransactionsPendingAllocation);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new string[] { TransactionTypes.CreditNotePendingAllocation, TransactionTypes.InvoicePendingAllocation });
			var collection = new TransactionHeaderCollection(factory, filter);
			return collection;
		}

		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new TransactionHeaderWithOrgWithTypeEDocsViaUniversalXmlSupport(this);
	}
}
