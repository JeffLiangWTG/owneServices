using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	[TestedType(typeof(DeclarationFormLayoutProvider))]
	sealed class DeclarationFormLayoutProviderTest : DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
	{
		protected override Type ExpectedDeclarationDetailsLayoutType => null;

		protected override Type ExpectedDeclarationShipmentTypeLayoutType => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(MiscOptionsLayouts) } };

		protected override Type ExpectedDeclarationGetOrganisationsLayoutType => null;

		protected override Type ExpectedDeclarationShipmentDetailsLayoutType => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
	}
}
