using System;
using Enterprise.Customs.EU.EMCS.GUI;
using Enterprise.Customs.EU.EMCS.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.GUI.Testing
{
	[TestedType(typeof(EMCSLayoutProvider))]
	sealed class EMCSLayoutProviderTest : EMCSLayoutProviderAbstractTest<EMCSLayoutProvider>
	{
		protected override Type ExpectedInvoiceLineDetailsPanelLayoutWithGridType => typeof(InvoiceLineDetailsLayoutWithGrid);

		protected override Type ExpectedDeclarationOrganizationsPanelLayoutType => typeof(DeclarationOrganizationsLayout);

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Ireland;
	}
}
