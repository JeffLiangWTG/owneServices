using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(BranchDepartmentCodeMappingRegistryDataType))]
	class BranchDepartmentCodeMappingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BranchDepartmentCodeMappingRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "BranchDepartmentCodeMappingRegistryItemEditor"; }
		}

		protected override BranchDepartmentCodeMappingRegistryDataType GetNewDataType()
		{
			return new BranchDepartmentCodeMappingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BranchDepartmentCodeMappingRegistryBusinessObjectCollection collection = new BranchDepartmentCodeMappingRegistryBusinessObjectCollection();
			BranchDepartmentCodeMappingRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.BranchCodePK = GetChargeCodeToTest().PK;
			bizObj.ProfitCentre = "BOB";

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)) };
		}

		GlbBranch GetChargeCodeToTest()
		{
			return Factory.LoadTop1<GlbBranch>(new ZQuery());
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
