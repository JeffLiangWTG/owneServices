using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(DirectDebitBatchData),
	Enterprise.Core.Constants.DocManagerCodes.DirectDebitBatch)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class DirectDebitBatchData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(DirectDebitBatchHeader); } }
		protected override Type CollectionType
		{
			get { return typeof(DirectDebitBatchHeaderCollection); }
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.DirectDebitFile; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("d9acc97c-f714-49ef-81d4-68d1c6d1ffa8", "Direct Debit Batch"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new DirectDebitBatchHeaderCollection(factory);
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			return new DirectDebitBatchHeaderCollection(factory, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode));
		}
	}
}
