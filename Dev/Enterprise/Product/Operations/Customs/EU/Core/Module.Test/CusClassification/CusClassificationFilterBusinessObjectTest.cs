using CargoWise.Types;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(CusClassificationFilterBusinessObject))]
	class CusClassificationFilterBusinessObjectTest : Customs.Module.Testing.CusClassificationFilterBusinessObjectTest
	{
		public void TestGetCpcFilter()
		{
			var stripBO = (CusClassificationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)stripBO["CPC"];
			AssertNotNull(filter);

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "A";
			classification.CC_ProcedureCode = "1234567";
			var classification2 = Factory.New<CusClassification>();
			classification2.CC_LookupCode = "B";
			classification2.CC_ProcedureCode = "121212";
			Factory.Save();

			var classificationCollection = new Customs.Business.BaseClassificationCollection<CusClassification>(Factory);
			classificationCollection.Load(stripBO.Filter);
			Assert(classificationCollection.Contains(classification));

			filter.Property = "12";
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			classificationCollection.Load(stripBO.Filter);
			Assert(classificationCollection.Contains(classification));
			Assert(classificationCollection.Contains(classification2));

			filter.Property = "123";
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			classificationCollection.Load(stripBO.Filter);
			Assert(classificationCollection.Contains(classification));
			Assert(!classificationCollection.Contains(classification2));

			filter.Property = "121212";
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			classificationCollection.Load(stripBO.Filter);
			Assert(!classificationCollection.Contains(classification));
			Assert(classificationCollection.Contains(classification2));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusClassificationFilterBusinessObject();
	}
}
