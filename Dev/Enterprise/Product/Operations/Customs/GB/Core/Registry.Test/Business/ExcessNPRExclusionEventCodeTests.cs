using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Business.Testing
{
	[TestedType(typeof(ExcessNPRExclusionEventCodeSetting))]
	public class ExcessNPRExclusionEventCodeTests : RegistryBusinessObjectTemplateTestCase<ExcessNPRExclusionEventCodeSetting>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override ExcessNPRExclusionEventCodeSetting GetBusinessObjectToClone()
		{
			return new ExcessNPRExclusionEventCodeSetting(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override ExcessNPRExclusionEventCodeSetting GetBusinessObjectToSerialise()
		{
			return new ExcessNPRExclusionEventCodeSetting() { EventCode = "Z00" };
		}
	}

	[TestedType(typeof(ExcessNPRExclusionEventCodeSettingCollectionRegistryItem))]
	public class ExcessNPRExclusionEventCodeSettingCollectionTest1 : StronglyTypedRegistryItemTestCase<ExcessNPRExclusionEventCodeSettingCollection>
	{
		protected override StronglyTypedRegistryItem<ExcessNPRExclusionEventCodeSettingCollection, ExcessNPRExclusionEventCodeSettingCollection> GetNewRegistryItem()
		{
			return new ExcessNPRExclusionEventCodeSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(ExcessNPRExclusionEventCodeSettingCollection))]
	public class ExcessNPRExclusionEventCodeSettingCollectionTest2 : RegistryBusinessObjectCollectionTemplateTestCase<ExcessNPRExclusionEventCodeSettingCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override ExcessNPRExclusionEventCodeSettingCollection GetCollectionToTest()
		{
			return new ExcessNPRExclusionEventCodeSettingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExcessNPRExclusionEventCodeSetting() { EventCode = "Z00" };
		}
	}

	[TestedType(typeof(ExcessNPRExclusionEventCodeRegistryDataType))]
	public class ExcessNPRExclusionEventCodeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ExcessNPRExclusionEventCodeRegistryDataType>
	{
		protected override ExcessNPRExclusionEventCodeRegistryDataType GetNewDataType()
		{
			return new ExcessNPRExclusionEventCodeRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var coll1 = new ExcessNPRExclusionEventCodeSettingCollection
			{
				new ExcessNPRExclusionEventCodeSetting { EventCode = "Z00" },
				new ExcessNPRExclusionEventCodeSetting { EventCode = "Z05" },
			};

			var bytes1 = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,69,0,120,0,99,0,101,0,115,0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,0,105,0,111,0,110,0,69,0,
				118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,
				0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,
				0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,
				0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,69,0,120,0,99,0,101,0,115,0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,0,105,0,111,
				0,110,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,62,0,90,0,48,0,48,0,
				60,0,47,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,69,0,120,0,99,0,101,0,115,0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,0,105,0,111,0,110,0,69,0,
				118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,69,0,120,0,99,0,101,0,115,0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,0,
				105,0,111,0,110,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,62,0,90,
				0,48,0,53,0,60,0,47,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,69,0,120,0,99,0,101,0,115,0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,0,105,0,111,
				0,110,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,69,0,120,0,99,0,101,0,115,
				0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,0,105,0,111,0,110,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0
			};

			var coll2 = new ExcessNPRExclusionEventCodeSettingCollection
			{
				new ExcessNPRExclusionEventCodeSetting { EventCode = "Z06" },
				new ExcessNPRExclusionEventCodeSetting { EventCode = "Z07" },
			};

			var bytes2 = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,69,0,120,0,99,0,101,0,115,0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,0,105,0,111,0,110,0,
				69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,
				0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,
				0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,
				76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,69,0,120,0,99,0,101,0,115,0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,
				0,105,0,111,0,110,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,62,0,90,0,
				48,0,54,0,60,0,47,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,69,0,120,0,99,0,101,0,115,0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,0,105,0,111,0,110,
				0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,69,0,120,0,99,0,101,0,115,0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,
				0,105,0,111,0,110,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,62,0,90,0,
				48,0,55,0,60,0,47,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,69,0,120,0,99,0,101,0,115,0,115,0,78,0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,0,105,0,111,0,110,
				0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,69,0,120,0,99,0,101,0,115,0,115,0,78,
				0,80,0,82,0,69,0,120,0,99,0,108,0,117,0,115,0,105,0,111,0,110,0,69,0,118,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(coll1, bytes1),
				new ValidSampleAndBinaryValueInDB(coll2, bytes2)
			};
		}

		protected override string ExpectedEditorName => "ExcessNPRExclusionEventCodeRegistryItemEditor";
	}
}
