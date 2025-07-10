using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class ListHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetDescription()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", "ABC TESTING");
			list.AddPair("DEF", "DEF TESTING");
			AssertEquals("ABC TESTING", ListHelper.GetDescription("ABC", list));
			AssertEquals("DEF TESTING", ListHelper.GetDescription("DEF", list));
			AssertNull(ListHelper.GetDescription("GHI", list));
			AssertNull(ListHelper.GetDescription("", list));
		}

		public void TestGetWithDescription_IFindBoxListProvider()
		{
			var pk = ZGuid.NewZGuid();
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory.BOFactory, new ZQuery(DummyBizoSchema.Z0_Guid, pk));
			var bizObj1 = collection.AddNew();
			bizObj1.Z0_Guid = pk;
			bizObj1.Z0_Code = "ABC";
			bizObj1.Z0_Description = "ABC TESTING";
			var bizObj2 = collection.AddNew();
			bizObj2.Z0_Guid = pk;
			bizObj2.Z0_Code = "DEF";
			bizObj2.Z0_Description = "DEF TESTING".PadRight(80, '1');

			var pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>("ABC", collection);
			AssertEquals("ABC", pair.Code);
			AssertEquals("ABC TESTING", pair.Description);

			pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>("DEF", collection);
			AssertEquals("DEF", pair.Code);
			AssertEquals("DEF TESTING".PadRight(80, '1'), pair.Description);

			pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>("GHI", collection);
			AssertEquals("GHI", pair.Code);
			AssertNull(pair.Description);

			pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>("", collection);
			AssertEquals("", pair.Code);
			AssertNull(pair.Description);
		}

		public void TestGetWithDescription_ICodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", "ABC TESTING");
			list.AddPair("DEF", "DEF TESTING".PadRight(70, '1'));
			var pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>("ABC", list);
			AssertEquals("ABC", pair.Code);
			AssertEquals("ABC TESTING", pair.Description);

			pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>("DEF", list);
			AssertEquals("DEF", pair.Code);
			AssertEquals("DEF TESTING".PadRight(70, '1'), pair.Description);

			pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>("GHI", list);
			AssertEquals("GHI", pair.Code);
			AssertNull(pair.Description);

			pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>("", list);
			AssertEquals("", pair.Code);
			AssertNull(pair.Description);
		}

		public void TestGetWithDescription_IFindBoxListProvider_PK()
		{
			var pk = ZGuid.NewZGuid();
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory.BOFactory, new ZQuery(DummyBizoSchema.Z0_Guid, pk));
			var bizObj1 = collection.AddNew();
			bizObj1.Z0_Guid = pk;
			bizObj1.Z0_Code = "ABC";
			bizObj1.Z0_Description = "ABC TESTING";
			var bizObj2 = collection.AddNew();
			bizObj2.Z0_Guid = pk;
			bizObj2.Z0_Code = "DEF";
			bizObj2.Z0_Description = "DEF TESTING".PadRight(80, '1');

			var pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>(bizObj1.PK, collection);
			AssertEquals("ABC", pair.Code);
			AssertEquals("ABC TESTING", pair.Description);

			pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>(bizObj2.PK, collection);
			AssertEquals("DEF", pair.Code);
			AssertEquals("DEF TESTING".PadRight(80, '1'), pair.Description);

			pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>(ZGuid.NewZGuid(), collection);
			AssertNull(pair.Code);
			AssertNull(pair.Description);

			pair = ListHelper.GetWithDescription<Universal.CodeDescriptionPair>(ZGuid.Empty, collection);
			AssertNull(pair.Code);
			AssertNull(pair.Description);
		}

		public void TestGetWithName()
		{
			var pk = ZGuid.NewZGuid();
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory.BOFactory, new ZQuery(DummyBizoSchema.Z0_Guid, pk));
			var bizObj1 = collection.AddNew();
			bizObj1.Z0_Guid = pk;
			bizObj1.Z0_Code = "ABC";
			bizObj1.Z0_Description = "ABC TESTING";
			var bizObj2 = collection.AddNew();
			bizObj2.Z0_Guid = pk;
			bizObj2.Z0_Code = "DEF";
			bizObj2.Z0_Description = "DEF TESTING".PadRight(80, '1');

			var pair = ListHelper.GetWithName<Staff>("ABC", collection);
			AssertEquals("ABC", pair.Code);
			AssertEquals("ABC TESTING", pair.Name);

			pair = ListHelper.GetWithName<Staff>("DEF", collection);
			AssertEquals("DEF", pair.Code);
			AssertEquals("DEF TESTING".PadRight(80, '1'), pair.Name);

			pair = ListHelper.GetWithName<Staff>("GHI", collection);
			AssertEquals("GHI", pair.Code);
			AssertNull(pair.Name);

			pair = ListHelper.GetWithName<Staff>("", collection);
			AssertEquals("", pair.Code);
			AssertNull(pair.Name);
		}

		public void TestGetWithName_UNLOCO()
		{
			var pk = ZGuid.NewZGuid();
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory.BOFactory, new ZQuery(DummyBizoSchema.Z0_Guid, pk));
			var bizObj1 = collection.AddNew();
			bizObj1.Z0_Guid = pk;
			bizObj1.Z0_Code = "ABC";
			bizObj1.Z0_Description = "ABC TESTING";
			var bizObj2 = collection.AddNew();
			bizObj2.Z0_Guid = pk;
			bizObj2.Z0_Code = "DEF";
			bizObj2.Z0_Description = "DEF TESTING".PadRight(80, '1');

			UNLOCO pair = ListHelper.GetWithName("ABC", collection);
			AssertEquals("ABC", pair.Code);
			AssertEquals("ABC TESTING", pair.Name);

			pair = ListHelper.GetWithName("DEF", collection);
			AssertEquals("DEF", pair.Code);
			AssertEquals("DEF TESTING".PadRight(80, '1'), pair.Name);

			pair = ListHelper.GetWithName("GHI", collection);
			AssertEquals("GHI", pair.Code);
			AssertNull(pair.Name);

			pair = ListHelper.GetWithName("", collection);
			AssertEquals("", pair.Code);
			AssertNull(pair.Name);
		}
	}
}
