using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.GUI.Testing;

[TestedType(typeof(H7ApplicationGUIProvider))]
sealed class H7ApplicationGUIProviderTest : H7ApplicationGUIProviderAbstractTest<H7ApplicationGUIProvider, AsycudaManifestHeader>
{
	protected override Type ExpectedBillLayoutType => typeof(ITH7BillLayouts);

	protected override Type ExpectedH7ItemDetailsLayoutsType => typeof(ITH7ItemDetailsLayouts);

	protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

	protected override IList<KeyValuePair<string, Type>> ExpectedControlsOnItemDetailsTab =>
	[
		new KeyValuePair<string, Type>("TariffFindBox", typeof(Universal.GUI.TariffFindBox)),
		new KeyValuePair<string, Type>("IntrinsicValueConvertToLocalCurrencyControl", typeof(ConvertToLocalCurrencyControl)),
		new KeyValuePair<string, Type>("SupplementaryDropEdit", typeof(ZCalcDropEdit)),
		new KeyValuePair<string, Type>("GoodsDescriptionTextBox", typeof(ZTextBox)),
		new KeyValuePair<string, Type>("CustomEntriesSeparatorUserControl", typeof(SeparatorUserControl)),
		new KeyValuePair<string, Type>("CustomEntriesGrid", typeof(ZGrid)),
		new KeyValuePair<string, Type>("ColumnSeparator0", typeof(ZPanel)),
	];

	protected override IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => [];

	protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[]
	{
		typeof(EUH7PackUserControl),
		typeof(EUH7ItemUserControl),
		typeof(AdditionalDocumentsUserControl),
		typeof(RelatedDocumentsUserControlWithGrid),
		typeof(RelatedDocumentsUserControlWithGrid),
	};

	protected override IReadOnlyList<Type> ExpectedItemAdditionalTabPageUserControls => new[]
	{
		typeof(EUH7ItemPacksUserControl),
		typeof(AdditionalDocumentsUserControl),
		typeof(RelatedDocumentsUserControlWithGrid),
		typeof(RelatedDocumentsUserControlWithGrid),
	};
}
