using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(PayableAdjustmentNoteData),
	Enterprise.Core.Constants.DocManagerCodes.PayableAdjustmentNote)]

namespace Enterprise.Accounting.Business
{
	public class PayableAdjustmentNoteData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ARAP.Invoicing.APAdjustmentNote); } }
		protected override Type CollectionType
		{
			get { return typeof(APAdjustmentNoteCollectionForDocScanning); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("a96893c3-a19c-4802-8766-9abad6ab2876", "Payable Adjustment Note"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.APTransaction; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.AdjustmentNote);
			APAdjustmentNoteCollectionForDocScanning collection = new APAdjustmentNoteCollectionForDocScanning(factory, filter);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.AdjustmentNote), false));
			return collection;
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.AdjustmentNote);
			APAdjustmentNoteCollectionForDocScanning collection = new APAdjustmentNoteCollectionForDocScanning(factory, filter, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.AdjustmentNote), false));
			return collection;
		}
	}
}
