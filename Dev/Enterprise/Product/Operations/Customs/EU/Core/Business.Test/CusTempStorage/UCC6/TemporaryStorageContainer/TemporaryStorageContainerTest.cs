using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageContainer))]
	internal class TemporaryStorageContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnlyPropertiesUnderTSDStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.ENSReuse = 0;
			var container = header.Containers.AddNew();

			foreach (var status in TemporaryStorageHeaderTest.GetNoEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				var propertyInfos = container.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}
			}
		}

		public void TestHeader()
		{
			var container = (TemporaryStorageContainer)GetNewBusinessObject();
			AssertType<TemporaryStorageHeader>(container.Header);
		}

		public void TestAdditionalSeals()
		{
			var container = (TemporaryStorageContainer)GetNewBusinessObject();
			AssertType<CusSealCollection>(container.AdditionalSeals);
		}

		public void TestCusSealType()
		{
			var container = (TemporaryStorageContainer)GetNewBusinessObject();
			AssertType(((ICusSealTypeSupporter)container).CusSealType, container.AdditionalSeals.AddNew());
		}

		public void TestDelete_AdditionalSeals()
		{
			var container = (TemporaryStorageContainer)GetNewBusinessObjectForDeleteTest(Factory);
			var seal1 = container.AdditionalSeals.AddNew();
			var seal2 = container.AdditionalSeals.AddNew();
			container.Delete();
			AssertEquals("seal1.IsDeleted", true, seal1.IsDeleted);
			AssertEquals("seal2.IsDeleted", true, seal2.IsDeleted);
		}

		public void TestIsAllowedToAddNewCusSeal()
		{
			AssertAllowNewCusSeal(ZString.Empty, ZString.Empty, ZString.Empty, false);

			AssertAllowNewCusSeal("1", ZString.Empty, ZString.Empty, false);
			AssertAllowNewCusSeal(ZString.Empty, "2", ZString.Empty, false);
			AssertAllowNewCusSeal(ZString.Empty, ZString.Empty, "3", false);

			AssertAllowNewCusSeal("1", "2", ZString.Empty, false);
			AssertAllowNewCusSeal("1", ZString.Empty, "3", false);
			AssertAllowNewCusSeal(ZString.Empty, "2", "3", false);

			AssertAllowNewCusSeal("1", "2", "3", true);
		}

		void AssertAllowNewCusSeal(ZString seal1, ZString seal2, ZString seal3, bool shouldBeReadOnly)
		{
			var container = (TemporaryStorageContainer)GetNewBusinessObjectForDeleteTest(Factory);
			var seals = new CusSealCollection(container);
			container.ACN_Seal1 = seal1;
			container.ACN_Seal2 = seal2;
			container.ACN_Seal3 = seal3;

			AssertEquals($"ACN_SealType1 = {seal1},ACN_SealType2 = {seal2}, ACN_SealType3 = {seal3} then AdditionalSeals AllowNewCusSeal value should be {shouldBeReadOnly} ", shouldBeReadOnly, container.AllowNewCusSeal);
		}

		public void TestShortSequenceNumberGenerator()
		{
			var container = (TemporaryStorageContainer)GetNewBusinessObject();

			AssertType<ShortSequenceNumberGenerator>("CusContainer should have a ShortSequenceNumberGenerator", container.SealsSequenceNumberGenerator);
			var seal1 = container.AdditionalSeals.AddNew();
			var seal2 = container.AdditionalSeals.AddNew();

			AssertEquals("Should have set the correct BK_SequenceNumber.", (short)1, seal1.BK_SequenceNumber);
			AssertEquals("Should have set the correct BK_SequenceNumber.", (short)2, seal2.BK_SequenceNumber);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<TemporaryStorageHeader>();
			return header.Containers.AddNew();
		}

		public void TestValidationType()
		{
			var container = (TemporaryStorageContainer)GetNewBusinessObject();
			AssertType<TemporaryStorageContainerValidation>(container.Validation);
		}

		public void TestLookupsType()
		{
			var container = (TemporaryStorageContainer)GetNewBusinessObject();
			AssertType<TemporaryStorageContainerLookups>(container.Lookups);
		}

		public void TestSupportsClone()
		{
			AssertEquals("SupportsClone", true, GetNewBusinessObject().SupportsClone());
		}
	}
}
