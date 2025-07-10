using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EndpointManagement.Module.Testing
{
	[TestedType(typeof(EdiTrustedSystemFilterBusinessObject))]
	public class EdiTrustedSystemFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EdiTrustedSystemFilterBusinessObject();
		}

		public void TestFilters()
		{
			EDIDataRegistry.Instance.MyAccountTrustedServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection() { "CCC" });

			var sys1 = Factory.New<EdiTrustedSystem>();
			sys1.ETS_Product = "CW1";
			var sys2 = Factory.New<EdiTrustedSystem>();
			sys2.ETS_Product = "ENT";
			sys2.ETS_SystemID = "SID#2";
			var sys3 = Factory.New<EdiTrustedSystem>();
			sys3.ETS_Product = "ENT";
			sys3.ETS_Description = "DESC004";
			Factory.Save();

			var collection = new EdiTrustedSystemCollection(Factory);
			AssertEquals(3, collection.Count);

			var filter = new EdiTrustedSystemFilterBusinessObject();
			var productFilter = (ModuleTextFilter)filter["Product"];
			productFilter.Property = "CW1";
			productFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			productFilter.IsActive = true;
			collection = new EdiTrustedSystemCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(sys1, collection);

			productFilter.IsActive = false;
			var systemIDFilter = (ModuleTextFilter)filter["System ID"];
			systemIDFilter.Property = "SID#2";
			systemIDFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			systemIDFilter.IsActive = true;
			collection = new EdiTrustedSystemCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(sys2, collection);

			systemIDFilter.IsActive = false;
			var descriptionFilter = (ModuleTextFilter)filter["Description"];
			descriptionFilter.Property = "DESC004";
			descriptionFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			descriptionFilter.IsActive = true;
			collection = new EdiTrustedSystemCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(sys3, collection);

			descriptionFilter.IsActive = false;
			var numberFilter = (ModuleTextFilter)filter["System #"];
			numberFilter.Property = sys1.ETS_SystemNumber;
			numberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			numberFilter.IsActive = true;
			collection = new EdiTrustedSystemCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(sys1, collection);
		}
	}
}
