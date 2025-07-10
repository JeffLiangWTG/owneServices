using System;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(EMCSLayoutProvider))]
	sealed class EMCSLayoutProviderBaseOnlyTest : EMCSLayoutProviderAbstractTest<EMCSLayoutProvider>
	{
		protected override Type ExpectedInvoiceLineDetailsPanelLayoutWithGridType => typeof(InvoiceLineDetailsLayoutWithGrid);

		protected override Type ExpectedDeclarationOrganizationsPanelLayoutType => typeof(DeclarationOrganizationsLayout);

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Latvia;
	}
}
