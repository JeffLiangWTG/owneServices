using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CollectionBatchData),
	Enterprise.Core.Constants.DocManagerCodes.CollectionBatch)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.Riba;
	using Enterprise.Environment;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using Enterprise.ZArchitecture.Schema;
	using WTG.ProductionRules.Core;

	public class CollectionBatchData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get
			{
				return typeof(AccCollectionBatch);
			}
		}

		protected override Type CollectionType
		{
			get
			{
				return typeof(AccCollectionBatchCollection);
			}
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccCollectionBatchCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccCollectionBatch;
			}
		}

		public override string ReferenceType
		{
			get
			{
				return Core.Constants.ReferenceTypes.Accounting;
			}
		}

		public override MultilingualString HumanReadableName
		{
			get
			{
				return ResString.GetMultilingualString("05bf24b5-f047-4455-8089-5b65c3e6834f", "Collection Batch");
			}
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get
			{
				return true;
			}
		}

		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new CollectionBatchEDocsViaUniversalXmlSupport();

		class CollectionBatchEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
		{
			public ZString ExampleCodeFormat => new ZString("00001000");

			public ZString ExpectedCodeFormat => new ZString("BatchNumber");

			public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
			{
				Argument.NotNull(factory, nameof(factory));
				Argument.NotNullOrWhitespace(code, nameof(code));

				var filter = new ZQuery(AccCollectionBatchSchema.ACB_GC, Env.CurrentCompany.PK);
				filter.AddToFilter(AccCollectionBatchSchema.ACB_BatchNumber, code);
				return factory.LoadTop1<AccCollectionBatch>(filter);
			}
		}
	}
}
