using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		public void TestManifestLayoutType()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertType<ManifestLayouts>(provider.GetManifestLayout());
		}

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(BillDetailsLayout);

		protected override Type ExpectedBillPartiesLayoutType => typeof(ILBillPartiesLayouts);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls =>
			new[] {
				typeof(AsycudaPackUserControl),
				typeof(AsycudaPackedItemControl),
				typeof(AsycudaAdditionalInfoUserControl),
				typeof(AsycudaTransportDocumentsUserControl) ,
				typeof(AsycudaSupportingDocumentUserControl) };

		protected override Type ExpectedContainerUserControlType => typeof(AsycudaContainerUserControl);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");

			var manifest = base.CreateNewManifest();
			manifest.AMA_ManifestType = "785";

			return manifest;
		}

		public void TestBillDetailsLayoutType()
		{
			var header = CreateNewManifest();
			var applicationGuiProvider = ASYCUDA.GUI.ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var provider = applicationGuiProvider.GetBillLayout();

			AssertType<BillDetailsLayout>(provider);
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		protected override void AssertGetContainersGridColumnsOrder(string[] columnsOrder)
		{
			CombineAssertions(() =>
			{
				AssertEquals(15, columnsOrder.Length);
				AssertContainsExactElementsInExactOrder(
					new[]
					{
						AsycudaContainer.Schema.ACN_ContainerNumber,
						AsycudaContainer.Schema.ACN_RC_ContainerType,
						AsycudaContainer.Schema.ACN_EmptyFullIndicator,
						AsycudaContainer.Schema.ACN_Seal1,
						AsycudaContainer.Schema.ACN_SealType1,
						AsycudaContainer.Schema.ACN_Seal1UnloadingState,
						AsycudaContainer.Schema.ACN_SealingPartyType,
						AsycudaContainer.Schema.ACN_Seal2,
						AsycudaContainer.Schema.ACN_SealType2,
						AsycudaContainer.Schema.ACN_Seal2UnloadingState,
						AsycudaContainer.Schema.ACN_SealingPartyType2,
						AsycudaContainer.Schema.ACN_Seal3,
						AsycudaContainer.Schema.ACN_SealType3,
						AsycudaContainer.Schema.ACN_Seal3UnloadingState,
						AsycudaContainer.Schema.ACN_SealingPartyType3,
					},
					columnsOrder);
			});
		}

		protected override void AssertGetContainersGridColumnVisibility(IReadOnlyDictionary<bool, string[]> columnVisibility)
		{
			CombineAssertions(() =>
			{
				AssertEquals(2, columnVisibility.Count);
				AssertContainsExactElementsInExactOrder(new[] { true, false }, columnVisibility.Keys);

				if (columnVisibility.TryGetValue(true, out string[] visibleColumns))
				{
					AssertContainsExactElementsInAnyOrder(
						new string[]
						{
							AsycudaContainer.Schema.ACN_Seal2,
							AsycudaContainer.Schema.ACN_Seal3,
							AsycudaContainer.Schema.ACN_SealType1,
							AsycudaContainer.Schema.ACN_SealType2,
							AsycudaContainer.Schema.ACN_SealType3,
							AsycudaContainer.Schema.ACN_SealingPartyType,
							AsycudaContainer.Schema.ACN_SealingPartyType2,
							AsycudaContainer.Schema.ACN_SealingPartyType3,
							AsycudaContainer.Schema.ACN_Seal1UnloadingState,
							AsycudaContainer.Schema.ACN_Seal2UnloadingState,
							AsycudaContainer.Schema.ACN_Seal3UnloadingState,
						},
						visibleColumns);
				}

				if (columnVisibility.TryGetValue(false, out string[] invisibleColumns))
				{
					AssertContainsExactElementsInAnyOrder(
						new string[]
						{
							AsycudaContainer.Schema.ACN_SealingPartyName,
							AsycudaContainer.Schema.ACN_NumberOfPackages,
							AsycudaContainer.Schema.ACN_CommodityCode,
							AsycudaContainer.Schema.ACN_GoodsWeight,
							AsycudaContainer.Schema.ACN_GoodsWeightUQ,
							AsycudaContainer.Schema.ACN_StowageLocation,
						},
						invisibleColumns);
				}
			});
		}

		public void TestGetBillsGridColumnAvailability()
		{
			var header = CreateNewManifest();
			var columnsAvailability = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillsGridColumnAvailability(header);
			CombineAssertions("Grid Columns Availability", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[]
					{
						AsycudaBill.Schema.ABL_SequenceNumber,
						AsycudaBill.Schema.ABL_RL_NKFinalDestination,
						AsycudaBill.Schema.ABL_ManifestQty,
						AsycudaBill.Schema.ABL_GrossWeight,
						AsycudaBill.Schema.ABL_GrossWeightUQ,
						AsycudaBill.Schema.ABL_Volume,
						AsycudaBill.Schema.ABL_VolumeUQ,
						AsycudaBill.Schema.ABL_BillStatus,
						AsycudaBill.Schema.ABL_BillStatusDescription,
						AsycudaBill.Schema.ABL_CustomsValue,
						AsycudaBill.Schema.ABL_BillNumber,
					}, columnsAvailability[true]);

				AssertContainsExactElementsInAnyOrder(new[]
					{
						AsycudaBill.Schema.ABL_CarrierReference,
						AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency,
						AsycudaBill.Schema.DiscountValue,
						AsycudaBill.Schema.DiscountValueCurrency,
						AsycudaBill.Schema.ABL_FreightValue,
						AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
						AsycudaBill.Schema.ABL_GoodsValue,
						AsycudaBill.Schema.ABL_InsuranceValue,
						AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
						AsycudaBill.Schema.OtherChargesValue,
						AsycudaBill.Schema.OtherChargesValueCurrency,
						AsycudaBill.Schema.ABL_TransportValue,
						AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency,
						AsycudaBill.Schema.ABL_PrepaidCollect,
						AsycudaBill.Schema.ABL_CargoStatus,
						AsycudaBill.Schema.ABL_BolType,
						AsycudaBill.Schema.ABL_GoodsDescription,
						AsycudaBill.Schema.ABL_MarksAndNumbers,
						AsycudaBill.Schema.ABL_Remarks,
						AsycudaBill.Schema.ABL_UCRNumber,
						AsycudaBill.Schema.CustomsJobNumber,
						AsycudaBill.Schema.ABL_RL_NKPortOfDischarge,
					}, columnsAvailability[false]);
			});
		}

		public void TestGetBillsGridColumnsOrder()
		{
			var header = CreateNewManifest();
			var columnsOrdered = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillsGridColumnsOrder();
			AssertContainsExactElementsInExactOrder("Bill grid columns must be ordered",
				new[]
				{
					AsycudaBill.Schema.ABL_SequenceNumber,
					AsycudaBill.Schema.ABL_RL_NKOrigin,
					AsycudaBill.Schema.ABL_BillNumber,
					AsycudaBill.Schema.ABL_RL_NKFinalDestination,
					AsycudaBill.Schema.ABL_ManifestQty,
					AsycudaBill.Schema.ABL_ManifestUQ,
					AsycudaBill.Schema.ABL_GrossWeight,
					AsycudaBill.Schema.ABL_GrossWeightUQ,
					AsycudaBill.Schema.ABL_Volume,
					AsycudaBill.Schema.ABL_VolumeUQ,
					AsycudaBill.Schema.ABL_BillStatus,
					AsycudaBill.Schema.ABL_BillStatusDescription,
				}, columnsOrdered);
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInAnyOrder(new[]
				{
					AsycudaBill.Schema.ABL_RL_NKPortOfDischarge,
					AsycudaBill.Schema.ABL_BillStatus,
					AsycudaBill.Schema.ABL_BillStatusDescription,
					AsycudaBill.Schema.ABL_Condition
				}, columnInfos.Select(s => s.ColumnName));
			AssertEquals("Port of Discharge column should have expected default width", 80, columnInfos.First(s => s.ColumnName == AsycudaBill.Schema.ABL_RL_NKPortOfDischarge).Width);
			AssertEquals("Status column should have expected default width", 70, columnInfos.First(s => s.ColumnName == AsycudaBill.Schema.ABL_BillStatus).Width);
			AssertEquals("Status Description column should have expected default width", 120, columnInfos.First(s => s.ColumnName == AsycudaBill.Schema.ABL_BillStatusDescription).Width);
			AssertEquals("Condition column should have expected default width", 70, columnInfos.First(s => s.ColumnName == AsycudaBill.Schema.ABL_Condition).Width);
		}

		protected override void AssertGetBillsGridColumnsWidth(IReadOnlyDictionary<string, int> columnsWidth)
		{
			AssertEquals("Destination column should have expected default width", 80, columnsWidth[AsycudaBill.Schema.ABL_RL_NKFinalDestination]);
		}

		protected override Type ExpectedContainerCountrySpecificUserControlType => typeof(ContainerSpecificUserControl);

		protected override Type ExpectedHeaderMessagesUserControl => typeof(CustomsMessagingControl);

		IDisposable disposableAction;
	}
}
