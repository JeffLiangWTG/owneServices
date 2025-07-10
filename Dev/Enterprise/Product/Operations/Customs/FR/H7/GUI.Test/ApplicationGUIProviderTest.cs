using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.Customs.FR.H7.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.H7.GUI.Testing
{
	[TestedType(typeof(H7ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : H7ApplicationGUIProviderAbstractTest<H7ApplicationGUIProvider, H7ManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(FRH7BillLayouts);

		protected override IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => [];

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[]
		{
			typeof(FRH7PackTabUserControl),
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
}
