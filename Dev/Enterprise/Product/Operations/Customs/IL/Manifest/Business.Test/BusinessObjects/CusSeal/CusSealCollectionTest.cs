using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(CusSealCollection))]
	sealed class CusSealCollectionTest : ActiveBusinessObjectCollectionTestCase<CusSealCollection>
	{
		public void TestConstructorAndDefaults()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var container = header.Containers.AddNew();
			var additionalSealsCollection = container.AdditionalSeals;

			AssertContains(
				"Should set correct BK_ParentTableCode for filter.", @"BK_ParentTableCode = 'ACN'", additionalSealsCollection.CompleteFilter.LiteralTextADO
			);
			var containerSeal = additionalSealsCollection.AddNew();
			AssertEquals("New element should have correct BK_ParentTableCode.", container.TablePrefix, containerSeal.BK_ParentTableCode);
		}

		public void TestMaxCountValidation()
		{
			var container = Factory.New<AsycudaContainer>();

			var sealCollection = new CusSealCollectionForTest(container);
			var maxCountValidator = ((ISupportMaxCountValidation)sealCollection).MaxCountValidator;
			var notification = maxCountValidator.Notification;

			AssertEquals("MaxCount", 999, maxCountValidator.MaxCount);
			AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
			AssertEquals("Notification Message", "The maximum number of 999 Additional Seals has been exceeded.", notification.Message);
			AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);
		}

		public void TestAllowNew_AvailableOnlyIfAll3ContainersSealAreFilled()
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

		protected override CusSealCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			return new CusSealCollection(container);
		}

		void AssertAllowNewCusSeal(ZString seal1, ZString seal2, ZString seal3, bool shouldBeReadOnly)
		{
			var seals = CreatNewAdditionalSeals(seal1, seal2, seal3);

			AssertEquals($"ACN_Seal1 = {seal1},ACN_Seal2 = {seal2}, ACN_Seal3 = {seal3} then AdditionalSeals AllowNewCusSeal value should be {shouldBeReadOnly} ", shouldBeReadOnly, seals.AllowNewExposed);
		}

		CusSealCollectionForTest CreatNewAdditionalSeals(ZString seal1, ZString seal2, ZString seal3)
		{
			var container = Factory.New<AsycudaContainer>();
			var seals = new CusSealCollectionForTest(container);

			container.ACN_Seal1 = seal1;
			container.ACN_Seal2 = seal2;
			container.ACN_Seal3 = seal3;
			return seals;
		}
	}

	class CusSealCollectionForTest : CusSealCollection
	{
		public CusSealCollectionForTest(AsycudaContainer master) : base(master)
		{
		}

		public bool AllowNewExposed => base.AllowNew;
	}
}
