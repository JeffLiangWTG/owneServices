using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	sealed class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(container.Header);
		}

		public void TestAdditionalSeals()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertType<CusSealCollection>(container.AdditionalSeals);
		}

		public void TestDelete_AdditionalSeals()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			var seal1 = container.AdditionalSeals.AddNew();
			var seal2 = container.AdditionalSeals.AddNew();
			container.Delete();
			AssertEquals("seal1.IsDeleted", true, seal1.IsDeleted);
			AssertEquals("seal2.IsDeleted", true, seal2.IsDeleted);
		}

		public void TestShortSequenceNumberGenerator()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();

			AssertType<ShortSequenceNumberGenerator>("CusContainer should have a ShortSequenceNumberGenerator", container.SealsSequenceNumberGenerator);

			var seal1 = container.AdditionalSeals.AddNew();
			var seal2 = container.AdditionalSeals.AddNew();

			AssertEquals("Should have set the correct BK_SequenceNumber.", (short)1, seal1.BK_SequenceNumber);
			AssertEquals("Should have set the correct BK_SequenceNumber.", (short)2, seal2.BK_SequenceNumber);
		}

		public void TestLookups()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertType<AsycudaContainerLookups>(container.Lookups);
		}

		public void TestACN_Seal1UnloadingState()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertEquals(3, container.ACN_Seal1UnloadingStateInfo.MaxLength);
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_Seal1UnloadingState), false, x => x.ListDataSourceMember == "Lookups.UnloadedStates");
		}

		public void TestACN_Seal2UnloadingState()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertEquals(3, container.ACN_Seal2UnloadingStateInfo.MaxLength);
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_Seal2UnloadingState), false, x => x.ListDataSourceMember == "Lookups.UnloadedStates");
		}

		public void TestACN_Seal3UnloadingState()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertEquals(3, container.ACN_Seal3UnloadingStateInfo.MaxLength);
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaContainer), nameof(AsycudaContainer.ACN_Seal3UnloadingState), false, x => x.ListDataSourceMember == "Lookups.UnloadedStates");
		}

		public void TestCaptions()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertEquals("ACN_SealingPartyType caption", "Seal 1 Party", DataBoundResourceStrings.GetDataForProperty(container.ACN_SealingPartyTypeInfo).Caption);
			AssertEquals("ACN_SealingPartyType2 caption", "Seal 2 Party", DataBoundResourceStrings.GetDataForProperty(container.ACN_SealingPartyType2Info).Caption);
			AssertEquals("ACN_SealingPartyType3 caption", "Seal 3 Party", DataBoundResourceStrings.GetDataForProperty(container.ACN_SealingPartyType3Info).Caption);
			AssertEquals("ACN_SealingPartyType short caption", "Seal 1 Party", DataBoundResourceStrings.GetDataForProperty(container.ACN_SealingPartyTypeInfo).ShortCaption);
			AssertEquals("ACN_SealingPartyType2 short caption", "Seal 2 Party", DataBoundResourceStrings.GetDataForProperty(container.ACN_SealingPartyType2Info).ShortCaption);
			AssertEquals("ACN_SealingPartyType3 short caption", "Seal 3 Party", DataBoundResourceStrings.GetDataForProperty(container.ACN_SealingPartyType3Info).ShortCaption);
		}

		public void TestValidationType()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertType<AsycudaContainerValidation>(container.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Containers.AddNew();
		}
	}
}
