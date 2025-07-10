using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing
{
	[TestedType(typeof(CcsukNonstandardPimaSetting))]
	public class CcsukNonstandardPimaTests : RegistryBusinessObjectTemplateTestCase<CcsukNonstandardPimaSetting>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override CcsukNonstandardPimaSetting GetBusinessObjectToClone()
		{
			return new CcsukNonstandardPimaSetting(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override CcsukNonstandardPimaSetting GetBusinessObjectToSerialise()
		{
			return GetExamplePimaSetting(Factory);
		}

		internal static CcsukNonstandardPimaSetting GetExamplePimaSetting(BusinessObjectFactory factory)
		{
			return new CcsukNonstandardPimaSetting("LHRCAX", "BTH", "POOP", factory);
		}

		public void TestValidation()
		{
			var pima = GetBusinessObjectToSerialise();
			AssertNoErrorContaining(pima.AirportAndShedInfo, "Please enter");
			AssertNoErrorContaining(pima.MessageTypeInfo, "Please enter");
			AssertNoErrorContaining(pima.TypeBPimaInfo, "Please enter");
			AssertNoErrorContaining(pima.TypeBPimaInfo, "valid");

			pima.MessageType = "X";
			AssertHasErrorContaining(pima.MessageTypeInfo, "valid");

			pima.AirportAndShed = "";
			pima.MessageType = "";
			pima.TypeBPima = "";
			AssertHasErrorContaining(pima.AirportAndShedInfo, "Please enter");
			AssertHasErrorContaining(pima.MessageTypeInfo, "Please enter");
			AssertHasErrorContaining(pima.TypeBPimaInfo, "Please enter");
		}
	}

	[TestedType(typeof(CcsukNonstandardPimaSettingCollectionRegistryItem))]
	public class CcsukNonstandardPimaSettingCollectionTest1 : StronglyTypedRegistryItemTestCase<CcsukNonstandardPimaSettingCollection>
	{
		protected override StronglyTypedRegistryItem<CcsukNonstandardPimaSettingCollection, CcsukNonstandardPimaSettingCollection> GetNewRegistryItem()
		{
			return new CcsukNonstandardPimaSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System, new CcsukNonstandardPimaSettingCollection().GetDefaultValues());
		}
	}

	[TestedType(typeof(CcsukNonstandardPimaSettingCollection))]
	public class CcsukNonstandardPimaSettingCollectionTest2 : RegistryBusinessObjectCollectionTemplateTestCase<CcsukNonstandardPimaSettingCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return CcsukNonstandardPimaTests.GetExamplePimaSetting(Factory);
		}

		protected override CcsukNonstandardPimaSettingCollection GetCollectionToTest()
		{
			return new CcsukNonstandardPimaSettingCollection();
		}
	}

	[TestedType(typeof(CcsukNonstandardPimaRegistryDataType))]
	public class CcsukNonstandardPimaSettingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CcsukNonstandardPimaRegistryDataType>
	{
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var item1 = new CcsukNonstandardPimaSetting("A", "BTH", "C", factory);
			var item2 = new CcsukNonstandardPimaSetting("D", "FRN", "F", factory);
			var item3 = new CcsukNonstandardPimaSetting("X", "FRD", "Z", factory);
			var coll1 = new CcsukNonstandardPimaSettingCollection();
			coll1.Add(item1);
			coll1.Add(item2);
			coll1.Add(item3);

			var bytes1 = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,
				0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,
				0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,
				0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,
				0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,
				0,116,0,105,0,110,0,103,0,62,0,60,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,0,110,0,100,0,83,0,104,0,101,0,100,0,62,0,65,0,60,0,47,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,0,110,
				0,100,0,83,0,104,0,101,0,100,0,62,0,60,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,84,0,121,0,112,0,101,0,62,0,66,0,84,0,72,0,60,0,47,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,84,0,121,
				0,112,0,101,0,62,0,60,0,84,0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,67,0,60,0,47,0,84,0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,60,0,47,0,67,0,99,0,115,0,117,
				0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,
				0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,0,110,0,100,0,83,0,104,
				0,101,0,100,0,62,0,68,0,60,0,47,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,0,110,0,100,0,83,0,104,0,101,0,100,0,62,0,60,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,84,0,121,0,112,0,101,
				0,62,0,70,0,82,0,78,0,60,0,47,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,84,0,121,0,112,0,101,0,62,0,60,0,84,0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,70,0,60,0,47,0,84,
				0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,60,0,47,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,
				0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,0,116,0,105,0,110,
				0,103,0,62,0,60,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,0,110,0,100,0,83,0,104,0,101,0,100,0,62,0,88,0,60,0,47,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,0,110,0,100,0,83,0,104,
				0,101,0,100,0,62,0,60,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,84,0,121,0,112,0,101,0,62,0,70,0,82,0,68,0,60,0,47,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,84,0,121,0,112,0,101,0,62,
				0,60,0,84,0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,90,0,60,0,47,0,84,0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,60,0,47,0,67,0,99,0,115,0,117,0,107,0,78,0,111,
				0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,99,0,115,
				0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0
			};

			var item4 = new CcsukNonstandardPimaSetting("DJC", "BTH", "LSC", factory);
			var item5 = new CcsukNonstandardPimaSetting("JOHN", "FRN", "LOCKE", factory);
			var item6 = new CcsukNonstandardPimaSetting("BUZZY", "FRD", "BEES", factory);
			var coll2 = new CcsukNonstandardPimaSettingCollection();
			coll2.Add(item4);
			coll2.Add(item5);
			coll2.Add(item6);

			var bytes2 = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,
				0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,
				0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,
				0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,
				0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,
				0,116,0,105,0,110,0,103,0,62,0,60,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,0,110,0,100,0,83,0,104,0,101,0,100,0,62,0,68,0,74,0,67,0,60,0,47,0,65,0,105,0,114,0,112,0,111,0,114,0,116,
				0,65,0,110,0,100,0,83,0,104,0,101,0,100,0,62,0,60,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,84,0,121,0,112,0,101,0,62,0,66,0,84,0,72,0,60,0,47,0,77,0,101,0,115,0,115,0,97,0,103,0,101,
				0,84,0,121,0,112,0,101,0,62,0,60,0,84,0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,76,0,83,0,67,0,60,0,47,0,84,0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,60,0,47,
				0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,67,0,99,0,115,0,117,
				0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,
				0,110,0,100,0,83,0,104,0,101,0,100,0,62,0,74,0,79,0,72,0,78,0,60,0,47,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,0,110,0,100,0,83,0,104,0,101,0,100,0,62,0,60,0,77,0,101,0,115,0,115,
				0,97,0,103,0,101,0,84,0,121,0,112,0,101,0,62,0,70,0,82,0,78,0,60,0,47,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,84,0,121,0,112,0,101,0,62,0,60,0,84,0,121,0,112,0,101,0,66,0,80,0,105,
				0,109,0,97,0,62,0,76,0,79,0,67,0,75,0,69,0,60,0,47,0,84,0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,60,0,47,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,
				0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,
				0,100,0,80,0,105,0,109,0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,0,110,0,100,0,83,0,104,0,101,0,100,0,62,0,66,0,85,0,90,0,90,
				0,89,0,60,0,47,0,65,0,105,0,114,0,112,0,111,0,114,0,116,0,65,0,110,0,100,0,83,0,104,0,101,0,100,0,62,0,60,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,84,0,121,0,112,0,101,0,62,0,70,0,82,
				0,68,0,60,0,47,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,84,0,121,0,112,0,101,0,62,0,60,0,84,0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,66,0,69,0,69,0,83,0,60,0,47,0,84,
				0,121,0,112,0,101,0,66,0,80,0,105,0,109,0,97,0,62,0,60,0,47,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,0,109,0,97,0,83,0,101,
				0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,99,0,115,0,117,0,107,0,78,0,111,0,110,0,115,0,116,0,97,0,110,0,100,0,97,0,114,0,100,0,80,0,105,
				0,109,0,97,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(coll1, bytes1),
				new ValidSampleAndBinaryValueInDB(coll2, bytes2)
			};
		}

		protected override CcsukNonstandardPimaRegistryDataType GetNewDataType()
		{
			return new CcsukNonstandardPimaRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "CcsukNonstandardPimaRegistryItemEditor";
			}
		}
	}
}
