using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(BranchCodeMappingRegistryDataType))]
	class BranchCodeMappingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BranchCodeMappingRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "BranchCodeMappingRegistryItemEditor"; }
		}

		protected override BranchCodeMappingRegistryDataType GetNewDataType()
		{
			return new BranchCodeMappingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BranchCodeMappingRegistryBusinessObjectCollection collection = new BranchCodeMappingRegistryBusinessObjectCollection();
			BranchCodeMappingRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.CodePK = GetChargeCodeToTest().PK;
			bizObj.ExternalCode = "BOB";

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
