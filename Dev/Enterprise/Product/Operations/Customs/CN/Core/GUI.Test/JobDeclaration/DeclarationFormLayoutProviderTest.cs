using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(DeclarationFormLayoutProvider))]
	sealed class DeclarationFormLayoutProviderTest : Customs.GUI.Testing.DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
	{
		protected override Type ExpectedDeclarationDetailsLayoutType => null;

		protected override Type ExpectedDeclarationShipmentTypeLayoutType => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type>
		{
			{ JobMessageTypeList.Codes.Export, typeof(MiscOptionsLayouts) },
			{ JobMessageTypeList.Codes.Import, typeof(MiscOptionsLayouts) },
		};

		protected override Type ExpectedDeclarationGetOrganisationsLayoutType => null;

		protected override Type ExpectedDeclarationShipmentDetailsLayoutType => null;
		 
		protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
		{
			{ JobMessageTypeList.Codes.Export, typeof(EntryInstructionDetailsLayout) },
			{ JobMessageTypeList.Codes.Import, typeof(EntryInstructionDetailsLayout) },
		};

		protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
	}
}
