using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Registry.Business.Testing
{
	[TestedType(typeof(CognosModeMappingRegistryDataType))]
	class CognosModeMappingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CognosModeMappingRegistryDataType>
	{
		protected override CognosModeMappingRegistryDataType GetNewDataType()
		{
			return new CognosModeMappingRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "CognosModeMappingRegistryEditor";
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CognosModeMapping modeMapping = new CognosModeMapping();
			modeMapping.SelectedMode = "AI";
			modeMapping.MapDepartments(DeptCollection[0], DeptCollection[1]);
			modeMapping.SelectedMode = "AE";
			modeMapping.MapDepartments(DeptCollection[2]);
			modeMapping.SelectedMode = "ME";
			modeMapping.MapDepartments(DeptCollection[3], DeptCollection[4]);
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(modeMapping, new byte[] { 255, 254, 60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102, 0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 67, 0, 111, 0, 103, 0, 110, 0, 111, 0, 115, 0, 77, 0, 111, 0, 100, 0, 101, 0, 77, 0, 97, 0, 112, 0, 112, 0, 105, 0, 110, 0, 103, 0, 62, 0, 60, 0, 83, 0, 69, 0, 76, 0, 69, 0, 67, 0, 84, 0, 69, 0, 68, 0, 77, 0, 79, 0, 68, 0, 69, 0, 62, 0, 77, 0, 69, 0, 60, 0, 47, 0, 83, 0, 69, 0, 76, 0, 69, 0, 67, 0, 84, 0, 69, 0, 68, 0, 77, 0, 79, 0, 68, 0, 69, 0, 62, 0, 60, 0, 77, 0, 79, 0, 68, 0, 69, 0, 32, 0, 78, 0, 65, 0, 77, 0, 69, 0, 61, 0, 34, 0, 65, 0, 73, 0, 34, 0, 62, 0, 60, 0, 77, 0, 65, 0, 80, 0, 80, 0, 73, 0, 78, 0, 71, 0, 62, 0, 48, 0, 49, 0, 50, 0, 57, 0, 56, 0, 100, 0, 54, 0, 50, 0, 45, 0, 54, 0, 98, 0, 97, 0, 57, 0, 45, 0, 52, 0, 57, 0, 102, 0, 50, 0, 45, 0, 57, 0, 56, 0, 53, 0, 98, 0, 45, 0, 56, 0, 102, 0, 55, 0, 50, 0, 55, 0, 57, 0, 97, 0, 51, 0, 52, 0, 50, 0, 51, 0, 49, 0, 60, 0, 47, 0, 77, 0, 65, 0, 80, 0, 80, 0, 73, 0, 78, 0, 71, 0, 62, 0, 60, 0, 77, 0, 65, 0, 80, 0, 80, 0, 73, 0, 78, 0, 71, 0, 62, 0, 48, 0, 49, 0, 100, 0, 100, 0, 102, 0, 101, 0, 54, 0, 99, 0, 45, 0, 48, 0, 100, 0, 98, 0, 54, 0, 45, 0, 52, 0, 48, 0, 51, 0, 57, 0, 45, 0, 56, 0, 56, 0, 54, 0, 100, 0, 45, 0, 101, 0, 48, 0, 49, 0, 50, 0, 56, 0, 50, 0, 56, 0, 57, 0, 56, 0, 54, 0, 57, 0, 99, 0, 60, 0, 47, 0, 77, 0, 65, 0, 80, 0, 80, 0, 73, 0, 78, 0, 71, 0, 62, 0, 60, 0, 47, 0, 77, 0, 79, 0, 68, 0, 69, 0, 62, 0, 60, 0, 77, 0, 79, 0, 68, 0, 69, 0, 32, 0, 78, 0, 65, 0, 77, 0, 69, 0, 61, 0, 34, 0, 65, 0, 69, 0, 34, 0, 62, 0, 60, 0, 77, 0, 65, 0, 80, 0, 80, 0, 73, 0, 78, 0, 71, 0, 62, 0, 48, 0, 54, 0, 51, 0, 48, 0, 52, 0, 100, 0, 57, 0, 97, 0, 45, 0, 51, 0, 48, 0, 57, 0, 102, 0, 45, 0, 52, 0, 97, 0, 51, 0, 52, 0, 45, 0, 57, 0, 57, 0, 101, 0, 57, 0, 45, 0, 100, 0, 54, 0, 51, 0, 102, 0, 102, 0, 97, 0, 49, 0, 51, 0, 53, 0, 48, 0, 50, 0, 54, 0, 60, 0, 47, 0, 77, 0, 65, 0, 80, 0, 80, 0, 73, 0, 78, 0, 71, 0, 62, 0, 60, 0, 47, 0, 77, 0, 79, 0, 68, 0, 69, 0, 62, 0, 60, 0, 77, 0, 79, 0, 68, 0, 69, 0, 32, 0, 78, 0, 65, 0, 77, 0, 69, 0, 61, 0, 34, 0, 77, 0, 69, 0, 34, 0, 62, 0, 60, 0, 77, 0, 65, 0, 80, 0, 80, 0, 73, 0, 78, 0, 71, 0, 62, 0, 48, 0, 56, 0, 54, 0, 57, 0, 97, 0, 97, 0, 98, 0, 55, 0, 45, 0, 51, 0, 55, 0, 98, 0, 99, 0, 45, 0, 52, 0, 49, 0, 57, 0, 99, 0, 45, 0, 97, 0, 51, 0, 100, 0, 48, 0, 45, 0, 57, 0, 57, 0, 97, 0, 56, 0, 53, 0, 53, 0, 49, 0, 49, 0, 55, 0, 52, 0, 49, 0, 99, 0, 60, 0, 47, 0, 77, 0, 65, 0, 80, 0, 80, 0, 73, 0, 78, 0, 71, 0, 62, 0, 60, 0, 77, 0, 65, 0, 80, 0, 80, 0, 73, 0, 78, 0, 71, 0, 62, 0, 54, 0, 98, 0, 53, 0, 51, 0, 101, 0, 55, 0, 51, 0, 55, 0, 45, 0, 101, 0, 55, 0, 48, 0, 99, 0, 45, 0, 52, 0, 49, 0, 98, 0, 97, 0, 45, 0, 98, 0, 99, 0, 49, 0, 101, 0, 45, 0, 101, 0, 98, 0, 50, 0, 56, 0, 54, 0, 55, 0, 99, 0, 49, 0, 48, 0, 101, 0, 55, 0, 102, 0, 60, 0, 47, 0, 77, 0, 65, 0, 80, 0, 80, 0, 73, 0, 78, 0, 71, 0, 62, 0, 60, 0, 47, 0, 77, 0, 79, 0, 68, 0, 69, 0, 62, 0, 60, 0, 47, 0, 67, 0, 111, 0, 103, 0, 110, 0, 111, 0, 115, 0, 77, 0, 111, 0, 100, 0, 101, 0, 77, 0, 97, 0, 112, 0, 112, 0, 105, 0, 110, 0, 103, 0, 62, 0 }) };
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeptCollection = LoadDepartmentCollection();
		}

		GlbDepartmentCollection LoadDepartmentCollection()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbDepartmentCollection collection = new GlbDepartmentCollection(factory);
			ZQuery query = new ZQuery();
			query.MaximumRows = 5;
			collection.AdditionalFilter = query;
			AssertEquals("Pre-condition. There should be at least 5 Departments in test database, add manually if this fails", 5, collection.Count);
			return collection;
		}

		GlbDepartmentCollection DeptCollection;
	}
}
