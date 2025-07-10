using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(H7ApplicationGUIProvider))]
	sealed class H7ApplicationGUIProviderTest : EU.H7.GUI.Testing.H7ApplicationGUIProviderAbstractTest<H7ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => new[]
		{
			typeof(ESH7MessagesUserControl),
		};

		protected override IReadOnlyList<Type> ExpectedItemAdditionalTabPageUserControls => new[]
		{
			typeof(EUH7ItemPacksUserControl),
		};

		protected override Type ExpectedBillLayoutType => typeof(ESH7BillLayout);

		protected override Type ExpectedBillPartiesLayoutType => typeof(ESH7BillPartiesLayout);

		protected override Type ExpectedH7ItemDetailsLayoutsType => typeof(ESH7ItemsDetailsLayouts);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[]
		{
			typeof(ESH7PackTabUserControl),
			typeof(EUH7ItemUserControl),
			typeof(AdditionalDocumentsUserControl),
			typeof(SupportingDocumentsUserControl),
			typeof(PreviousDocumentsUserControl),
		};

		protected override IList<KeyValuePair<string, Type>> ExpectedControlsOnItemDetailsTab => new[]
		{
			new KeyValuePair<string, Type>("TariffFindBox", typeof(Universal.GUI.TariffFindBox)),
			new KeyValuePair<string, Type>("IntrinsicValueConvertToLocalCurrencyControl", typeof(ConvertToLocalCurrencyControl)),
			new KeyValuePair<string, Type>("SupplementaryDropEdit", typeof(ZCalcDropEdit)),
			new KeyValuePair<string, Type>("GrossWeightCalcDropEdit", typeof(ZCalcDropEdit)),
			new KeyValuePair<string, Type>("GoodsOriginCodeFindBox", typeof(ZCodeFindBox)),
			new KeyValuePair<string, Type>("GoodsDescriptionTextBox", typeof(ZTextBox)),
			new KeyValuePair<string, Type>("CustomEntriesSeparatorUserControl", typeof(SeparatorUserControl)),
			new KeyValuePair<string, Type>("CustomEntriesGrid", typeof(ZGrid)),
			new KeyValuePair<string, Type>("ColumnSeparator0", typeof(ZPanel)),
			new KeyValuePair<string, Type>("QuantityCalcEdit", typeof(ZCalcEdit)),
		};

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			CombineAssertions(() =>
			{
				var goodsValueColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_GoodsValue);
				Assert("ABL_GoodsValue", goodsValueColumnStyleInfo.IsVisible);

				var goodsValueCurrencyColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency);
				Assert("ABL_RX_NKGoodsValueCurrency", goodsValueCurrencyColumnStyleInfo.IsVisible);

				var incotermColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_Incoterm);
				Assert("ABL_Incoterm", !incotermColumnStyleInfo.IsVisible);

				var procedureColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_Procedure);
				Assert("ABL_Procedure", procedureColumnStyleInfo.IsVisible);

				var standAloneDeclarationReferenceColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == nameof(AsycudaBill.EntrySummaryReferenceNumber));
				Assert("EntrySummaryReferenceNumber", standAloneDeclarationReferenceColumnStyleInfo.IsVisible);

				var containerNumberColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == nameof(AsycudaBill.ContainerNumber));
				Assert("ContainerNumber", !containerNumberColumnStyleInfo.IsVisible);

				var containerModeColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == nameof(AsycudaBill.ABL_ContainerMode));
				Assert("ABL_ContainerMode", !containerModeColumnStyleInfo.IsVisible);

				var messageStatusColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_MessageStatus);
				Assert("ABL_MessageStatus", messageStatusColumnStyleInfo.IsReadOnly);

				var customStatusColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.ABL_BillStatus);
				Assert("ABL_BillStatus", customStatusColumnStyleInfo.IsReadOnly);

				var documentationRequiredColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == nameof(AsycudaBill.Schema.DocumentationRequiredDescription));
				Assert("DocumentationRequired column: Should not be visible by default", !documentationRequiredColumnStyleInfo.IsVisible);

				var g3LrnColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.G3LocalReferenceNumber);
				Assert("G3LocalReferenceNumber", g3LrnColumnStyleInfo.IsVisible);

				var g3MrnColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.G3MovementReferenceNumber);
				Assert("G3MovementReferenceNumber", g3MrnColumnStyleInfo.IsVisible);

				var h7MrnColumnStyleInfo = columnInfos.FirstOrDefault(i => i.ColumnName == AsycudaBill.Schema.H7MovementReferenceNumber);
				Assert("H7MovementReferenceNumber", h7MrnColumnStyleInfo.IsVisible);
			});
		}

		public void TestGetManifestLayout()
		{
			var header = CreateNewManifest();
			var manifestLayout = ASYCUDA.GUI.ApplicationGUIProvider.GetApplicationGuiProvider(header).GetManifestLayout();
			AssertType<ESH7ManifestLayouts>(manifestLayout);
		}

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);
	}
}
