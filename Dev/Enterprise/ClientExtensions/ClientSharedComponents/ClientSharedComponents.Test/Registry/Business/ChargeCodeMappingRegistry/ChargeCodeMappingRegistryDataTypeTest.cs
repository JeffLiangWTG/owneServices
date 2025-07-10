using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ChargeCodeMappingRegistryDataType))]
	class ChargeCodeMappingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ChargeCodeMappingRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get
			{
				return "ChargeCodeMappingRegistryItemEditor";
			}
		}

		protected override ChargeCodeMappingRegistryDataType GetNewDataType()
		{
			return new ChargeCodeMappingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ChargeCodeMappingRegistryBusinessObjectCollection collection = new ChargeCodeMappingRegistryBusinessObjectCollection();
			ChargeCodeMappingRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.CodePK = GetChargeCodeToTest().PK;
			bizObj.ExternalCode = "BOB";

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)) };
		}

		AccChargeCode GetChargeCodeToTest()
		{
			return Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
		}

		#region Factory
		BusinessObjectFactory Factory
		{
			get
			{
				return factory ?? (factory = new BusinessObjectFactory());
			}
		}
		BusinessObjectFactory factory;
		#endregion
	}
}
