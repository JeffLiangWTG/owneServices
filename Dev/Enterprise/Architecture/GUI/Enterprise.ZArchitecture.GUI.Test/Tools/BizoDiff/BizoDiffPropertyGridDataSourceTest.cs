using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.DevTools;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class BizoDiffPropertyGridDataSourceTest : TestCaseWithFactory
	{
		public void TestBuildPropertyCollection_NoValidationErrors()
		{
			var obj = Factory.New<DummyBusinessObject>();
			var compareObj = Factory.New<DummyBusinessObject>();
			var columnProvider = new BizoDiffColumnProvider();
			var dataSource = new BizoDiffPropertyGridDataSource(obj, compareObj, columnProvider, false);

			dataSource.BuildCollections();

			AssertEquals(true, dataSource.SourceProperties.Cast<BizoProperty>().Any(p => p.Name == nameof(obj.Z0_Number)));
			AssertEquals(true, dataSource.SourceProperties.Cast<BizoProperty>().All(p => !p.ValueInfo.HasWarning("Different values found.")));
			AssertEquals(true, dataSource.TargetProperties.Cast<BizoProperty>().Any(p => p.Name == nameof(obj.Z0_Number)));
			AssertEquals(true, dataSource.SourceProperties.Cast<BizoProperty>().All(p => !p.ValueInfo.HasWarning("Different values found.")));
		}

		public void TestBuildPropertyCollection_ShowDifferentValuePropertyWhenSameType()
		{
			var obj = Factory.New<DummyBusinessObject>();
			obj.Z0_Number = 1;
			var compareObj = Factory.New<DummyBusinessObject>();
			compareObj.Z0_Number = 2;
			var columnProvider = new BizoDiffColumnProvider();
			var dataSource = new BizoDiffPropertyGridDataSource(obj, compareObj, columnProvider, false);

			dataSource.BuildCollections();

			AssertEquals(true, dataSource.SourceProperties.Cast<BizoProperty>().Any(p => p.Name == nameof(obj.Z0_Number)));
			AssertEquals("Z0_Number", dataSource.SourceProperties.Cast<BizoProperty>().Single(p => p.ValueInfo.HasWarning("Different values found.")).Name);
			AssertEquals(true, dataSource.TargetProperties.Cast<BizoProperty>().Any(p => p.Name == nameof(obj.Z0_Number)));
			AssertEquals("Z0_Number", dataSource.TargetProperties.Cast<BizoProperty>().Single(p => p.ValueInfo.HasWarning("Different values found.")).Name);
		}

		public void TestBuildPropertyCollection_ShowAllWhenTypesAreDifferent()
		{
			var obj = Factory.New<DummyBusinessObject>();
			var compareObj = Factory.New<DummyLogged>();
			var columnProvider = new BizoDiffColumnProvider();
			var dataSource = new BizoDiffPropertyGridDataSource(obj, compareObj, columnProvider, false);

			dataSource.BuildCollections();

			AssertEquals(true, dataSource.SourceProperties.Cast<BizoProperty>().Any(p => p.Name == nameof(obj.Z0_Number)));
			AssertEquals(true, dataSource.SourceProperties.Cast<BizoProperty>().All(p => !p.ValueInfo.HasWarning("Different values found.")));
			AssertEquals(true, dataSource.TargetProperties.Cast<BizoProperty>().Any(p => p.Name == nameof(compareObj.ZL2_Description)));
			AssertEquals(true, dataSource.TargetProperties.Cast<BizoProperty>().All(p => !p.ValueInfo.HasWarning("Different values found.")));
		}

		public void TestBuildPropertyCollection_NoValidationErrors_HideSameValue()
		{
			var obj = Factory.New<DummyBusinessObject>();
			var compareObj = Factory.New<DummyBusinessObject>();
			var columnProvider = new BizoDiffColumnProvider();
			var dataSource = new BizoDiffPropertyGridDataSource(obj, compareObj, columnProvider, true);

			dataSource.BuildCollections();

			AssertEquals(false, dataSource.SourceProperties.Cast<BizoProperty>().Any());
			AssertEquals(false, dataSource.TargetProperties.Cast<BizoProperty>().Any());
		}

		public void TestBuildPropertyCollection_ShowDifferentValuePropertyWhenSameType_HideSameValue()
		{
			var obj = Factory.New<DummyBusinessObject>();
			obj.Z0_Number = 1;
			var compareObj = Factory.New<DummyBusinessObject>();
			compareObj.Z0_Number = 2;
			var columnProvider = new BizoDiffColumnProvider();
			var dataSource = new BizoDiffPropertyGridDataSource(obj, compareObj, columnProvider, true);

			dataSource.BuildCollections();

			AssertEquals(true, dataSource.SourceProperties.Cast<BizoProperty>().Any(p => p.Name == nameof(obj.Z0_Number)));
			AssertEquals("Z0_Number", dataSource.SourceProperties.Cast<BizoProperty>().Single(p => p.ValueInfo.HasWarning("Different values found.")).Name);
			AssertEquals(true, dataSource.TargetProperties.Cast<BizoProperty>().Any(p => p.Name == nameof(obj.Z0_Number)));
			AssertEquals("Z0_Number", dataSource.TargetProperties.Cast<BizoProperty>().Single(p => p.ValueInfo.HasWarning("Different values found.")).Name);
		}

		public void TestBuildPropertyCollection_ShowAllWhenTypesAreDifferent_HideSameValue()
		{
			var obj = Factory.New<DummyBusinessObject>();
			var compareObj = Factory.New<DummyLogged>();
			var columnProvider = new BizoDiffColumnProvider();
			var dataSource = new BizoDiffPropertyGridDataSource(obj, compareObj, columnProvider, true);

			dataSource.BuildCollections();

			AssertEquals(true, dataSource.SourceProperties.Cast<BizoProperty>().Any(p => p.Name == nameof(obj.Z0_Number)));
			AssertEquals(true, dataSource.SourceProperties.Cast<BizoProperty>().All(p => !p.ValueInfo.HasWarning("Different values found.")));
			AssertEquals(true, dataSource.TargetProperties.Cast<BizoProperty>().Any(p => p.Name == nameof(compareObj.ZL2_Description)));
			AssertEquals(true, dataSource.TargetProperties.Cast<BizoProperty>().All(p => !p.ValueInfo.HasWarning("Different values found.")));
		}
	}
}
