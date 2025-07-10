using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(DeclarationFormLayoutProvider))]
	sealed class DeclarationFormLayoutProviderTest : Customs.GUI.Testing.DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
	{
		protected override Type ExpectedDeclarationDetailsLayoutType => null;

		protected override Type ExpectedDeclarationShipmentDetailsLayoutType => null;

		protected override Type ExpectedDeclarationShipmentTypeLayoutType => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => null;

		protected override Type ExpectedDeclarationGetOrganisationsLayoutType => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(CommercialInvoiceDetailsLayout) } };

		protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
		{
			{ JobMessageTypeList.Codes.Import, typeof(EntryInstructionDetailsBasicLayout) },
		};

		protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
	}
}
