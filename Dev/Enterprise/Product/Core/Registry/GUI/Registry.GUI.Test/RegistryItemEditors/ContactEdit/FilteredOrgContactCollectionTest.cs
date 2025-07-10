using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Registry.GUI.ZContactBusinessObject;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(FilteredOrgContactCollection))]
	sealed class FilteredOrgContactCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var collection = new FilteredOrgContactCollection(Factory);
			var selectedOrganisationPK = Environment.Env.CurrentCompanyPK;
			collection.SetRelationshipFilter(new ZQuery(OrgContactSchema.OC_OH, selectedOrganisationPK));
			collection.Load();
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<OrgContact>();
		}
	}
}
