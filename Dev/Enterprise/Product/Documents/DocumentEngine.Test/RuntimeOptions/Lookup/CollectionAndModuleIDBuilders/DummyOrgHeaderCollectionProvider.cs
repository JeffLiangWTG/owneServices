using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestExcludeCollectionProviderAllHaveTestCase]
	public class DummyOrgHeaderCollectionProvider : CollectionProvider
	{
		public DummyOrgHeaderCollectionProvider()
			: base(new BusinessObjectFactory())
		{
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Organisation;

		public void FillWithDummyOrgHeaders(string codePrefix, int numOrgHeaders)
		{
			for (var count = 0; count < numOrgHeaders; count++)
			{
				var orgHeader = OrgHeader.New(BusinessObjectFactory);
				orgHeader.OH_Code = ZString.Format("{0}{1}", codePrefix, count);
			}

			this.Filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, codePrefix)
			{
				MaximumRows = numOrgHeaders
			};
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new OrgHeaderCollection(BusinessObjectFactory, Filter);
		}
	}
}
