using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(DepartmentCodeMappingRegistryDataType))]
	class DepartmentCodeMappingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DepartmentCodeMappingRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "DepartmentCodeMappingRegistryItemEditor"; }
		}

		protected override DepartmentCodeMappingRegistryDataType GetNewDataType()
		{
			return new DepartmentCodeMappingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DepartmentCodeMappingRegistryBusinessObjectCollection collection = new DepartmentCodeMappingRegistryBusinessObjectCollection();
			DepartmentCodeMappingRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.CodePK = GetChargeCodeToTest().PK;
			bizObj.ExternalCode = "BOB";

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)) };
		}

		GlbDepartment GetChargeCodeToTest()
		{
			return Factory.LoadTop1<GlbDepartment>(new ZQuery());
		}

		#region Factory
		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
		#endregion
	}
}
