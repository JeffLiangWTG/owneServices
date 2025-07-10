using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class ImportCollectionInfoImplExtensionsExtensionsTest : TestCaseWithFactory
	{
		public void TestHeaderText()
		{
			var info = new ImportCollectionInfoImpl(new DummyBusinessObjectCollection(Factory));
			info.AddFlattenedProperty<ImportTestFlattened>("ParentProperty", typeof(ImportTestParent));
			info.AddFlattenedProperty<ImportTestFlattened>("ParentProperty2", typeof(ImportTestParent));
			info.AddFlattenedProperty<ImportTestFlattened>("Child_ChildProperty", typeof(ImportTestChild), "Child_");
			info.AddFlattenedProperty<ImportTestFlattened>("Child_ChildProperty2", typeof(ImportTestChild), "Child_", "Child");

			AssertPropertyHeaderText(info, "ParentProperty", "Parent Property 1");
			AssertPropertyHeaderText(info, "ParentProperty2", "ParentProperty2");
			AssertPropertyHeaderText(info, "Child_ChildProperty", "Child Property 1");
			AssertPropertyHeaderText(info, "Child_ChildProperty2", "Child - ChildProperty2");
		}

		void AssertPropertyHeaderText(IImportCollectionInfo info, string propertyName, string expectedHeaderText)
		{
			var headerText = info.Properties.Single(x => x.MappingName == propertyName).HeaderText;
			AssertEquals(expectedHeaderText, headerText);
		}

		#region Test Classes

		class ImportTestFlattened
		{
			public string ParentProperty { get; set; }

			public string ParentProperty2 { get; set; }

			public string Child_ChildProperty { get; set; }

			public string Child_ChildProperty2 { get; set; }
		}

		class ImportTestParent
		{
			[ResourceStringData("ImportTestParent|ParentProp", Caption = "Parent Property 1")]
			public string ParentProperty { get; set; }

			public string ParentProperty2 { get; set; }

			public ImportTestChild Child { get; set; }
		}

		class ImportTestChild
		{
			[ResourceStringData("ImportTestChild|ChildProp", Caption = "Child Property 1")]
			public string ChildProperty { get; set; }

			public string ChildProperty2 { get; set; }
		}

		#endregion
	}
}
