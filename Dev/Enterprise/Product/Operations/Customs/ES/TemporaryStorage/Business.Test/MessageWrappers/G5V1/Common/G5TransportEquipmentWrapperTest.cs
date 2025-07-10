using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5TransportEquipmentWrapperTest : WrapperHelperTest<G5TransportEquipmentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if cont is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "cont"), () => GetWrapper(null));
		}

		public void TestId()
		{
			cont.ACN_ContainerNumber = "CONT1";
			AssertEquals("Expected filled Id", "CONT1", wrapper.Id);
		}

		public void TestPackedStatus()
		{
			CombineAssertions(() =>
			{
				cont.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.Empty;
				AssertEquals("Expected empty PackedStatus when ACN_EmptyFullIndicator is A (empty)", ZString.Empty, wrapper.PackedStatus);

				cont.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.NotEmpty;
				AssertEquals("Expected filled PackedStatus when ACN_EmptyFullIndicator is B (not empty)", "B", wrapper.PackedStatus);
			});
		}

		public void TestSealIds()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Seals when no seals declared", 0, wrapper.SealIds.Count);

				cont.ACN_Seal1 = "SEAL1";
				cont.ACN_Seal2 = "SEAL2";
				cont.ACN_Seal3 = "SEAL3";
				var extraSeal1 = cont.AdditionalSeals.AddNew();
				extraSeal1.BK_SealNumber = "SEAL4";
				var extraSeal2 = cont.AdditionalSeals.AddNew();
				extraSeal2.BK_SealNumber = "SEAL5";

				wrapper = GetWrapper(cont);
				var sealIds = wrapper.SealIds;
				AssertEquals("In container, expected 5 SealIds when declared", 5, sealIds.Count);
				AssertSame("In container, cached SealIds", wrapper.SealIds, sealIds);

				AssertContainsExactElementsInAnyOrder("Expected docs with correct Name", new ZString[] { "SEAL1", "SEAL2", "SEAL3", "SEAL4", "SEAL5" }, sealIds.ToArray());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			cont = Factory.New<TemporaryStorageContainer>();
			wrapper = GetWrapper(cont);
		}

		TemporaryStorageContainer cont;
		G5TransportEquipmentWrapper wrapper;

		G5TransportEquipmentWrapper GetWrapper(TemporaryStorageContainer cont) => new G5TransportEquipmentWrapper(cont);

		protected override G5TransportEquipmentWrapper GetProvider() => wrapper;
	}
}
