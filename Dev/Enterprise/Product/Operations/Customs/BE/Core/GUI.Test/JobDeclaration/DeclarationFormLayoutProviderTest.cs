using System;
using System.Collections.Generic;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(DeclarationFormLayoutProvider))]
sealed class DeclarationFormLayoutProviderTest : DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
{
	protected override Type ExpectedDeclarationDetailsLayoutType => null;

	protected override Type ExpectedDeclarationShipmentTypeLayoutType => typeof(ShipmentTypeLayout);

	protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(TransportDetailsLayout) } };

	protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(MiscOptionsLayouts) } };

	protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(OrganisationsLayout);

	protected override Type ExpectedDeclarationShipmentDetailsLayoutType => typeof(ShipmentDetailsLayouts);

	protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(ExportInvoiceDetailsLayout) },
		{ JobMessageTypeList.Codes.Import, typeof(ImportInvoiceDetailsLayout) },
		{ JobMessageTypeList.Codes.MiscellaneousCustoms, typeof(ImportInvoiceDetailsLayout) },
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(EntryInstructionDetailsBasicUserControlLayout) },
		{ JobMessageTypeList.Codes.Import, typeof(EntryInstructionDetailsBasicUserControlLayout) },
		{ JobMessageTypeList.Codes.MiscellaneousCustoms, typeof(EntryInstructionDetailsBasicUserControlLayout) },
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
}
