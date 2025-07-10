using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Registry.Business.Testing
{
	[TestedType(typeof(CognosModeMappingRegistryItem))]
	class CognosModeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<CognosModeMapping>
	{
		public void TestRegistryItem()
		{
			CognosModeMappingRegistryItem registryItem = new CognosModeMappingRegistryItem("TestCategory");
			AssertEquals("TestCategory", registryItem.Category);
			AssertEquals("CognosModeMapping", registryItem.Name);
			AssertEquals("Cognos Mode Mapping", registryItem.Caption);
			AssertEquals("Please Map COGNOS modes to CargoWise One Departments", registryItem.Hint);
			AssertEquals(typeof(CognosModeMappingRegistryDataType), registryItem.DataType.GetType());
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
		}

		protected override StronglyTypedRegistryItem<CognosModeMapping, CognosModeMapping> GetNewRegistryItem()
		{
			return new CognosModeMappingRegistryItem("Cognos");
		}

		protected override CognosModeMapping ValidValue
		{
			get
			{
				CognosModeMapping result = new CognosModeMapping();
				result.SelectedMode = "AI";
				result.MapDepartments(DeptCollection[0], DeptCollection[1]);
				result.SelectedMode = "AE";
				result.MapDepartments(DeptCollection[2]);
				result.SelectedMode = "ME";
				result.MapDepartments(DeptCollection[3], DeptCollection[4]);
				return result;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeptCollection = LoadDepartmentCollection();
		}

		GlbDepartmentCollection LoadDepartmentCollection()
		{
			GlbDepartmentCollection collection = new GlbDepartmentCollection(new BusinessObjectFactory());
			ZQuery query = new ZQuery();
			query.MaximumRows = 5;
			collection.AdditionalFilter = query;
			AssertEquals("Pre-condition. There should be at least 5 Departments in test database, add manually if this fails", 5, collection.Count);
			return collection;
		}

		GlbDepartmentCollection DeptCollection;
	}
}
