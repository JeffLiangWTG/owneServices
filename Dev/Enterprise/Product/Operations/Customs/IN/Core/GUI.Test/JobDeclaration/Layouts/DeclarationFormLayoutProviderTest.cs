using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(DeclarationFormLayoutProvider))]
sealed class DeclarationFormLayoutProviderTest : DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, BaseJobDeclaration>
{
	protected override Type ExpectedDeclarationDetailsLayoutType => null;

	protected override Type ExpectedDeclarationShipmentDetailsLayoutType => typeof(ShipmentDetailsLayouts);

	protected override Type ExpectedDeclarationShipmentTypeLayoutType => typeof(ShipmentTypeLayouts);

	protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(TransportDetailsLayouts) } };

	protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(MiscOptionsLayouts) } };

	protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(CommonOrganisationsLayouts);

	protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(ExportInvoiceDetailsLayoutProvider) },
		{ JobMessageTypeList.Codes.Import, typeof(ImportInvoiceDetailsLayoutProvider) }
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(ExportEntryInstructionDetailsLayout) },
		{ JobMessageTypeList.Codes.Import, typeof(ImportEntryInstructionDetailsLayout) },
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
}
