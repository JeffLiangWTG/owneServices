using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(ReceivableJournalData),
	Enterprise.Core.Constants.DocManagerCodes.ReceivableJournal)]

namespace Enterprise.Accounting.Business
{
	public class ReceivableJournalData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ARAP.Journal.ARJournal); } }
		protected override Type CollectionType
		{
			get { return typeof(TransactionHeaderCollection); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("7ea03feb-2f4a-4fcd-bc7f-6abd3b13ee34", "Receivable Journal"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ARTransaction; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			TransactionHeaderCollection collection = new TransactionHeaderCollection(factory, filter);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.Journal), false));
			return collection;
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			TransactionHeaderCollection collection = new TransactionHeaderCollection(factory, filter, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.Journal), false));
			return collection;
		}
	}
}
