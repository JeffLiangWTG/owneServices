using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(DeclarationFormLayoutProvider))]
sealed class DeclarationFormLayoutProviderTest : DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
{
	protected override Type ExpectedDeclarationDetailsLayoutType => null;

	protected override Type ExpectedDeclarationShipmentDetailsLayoutType => typeof(ShipmentDetailsLayout);

	protected override Type ExpectedDeclarationShipmentTypeLayoutType => typeof(ShipmentTypeLayout);

	protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(TransportDetailsLayout) } };

	protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(OrganisationsLayout);

	protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(MiscOptionsLayouts) } };

	protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(ExportInvoiceDetailsLayout) },
		{ EUJobMessageTypeList.Codes.MiscellaneousCustoms, typeof(MiscellaneousInvoiceDetailsLayout) },
		{ JobMessageTypeList.Codes.Import, typeof(ImportInvoiceDetailsLayout) },
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
	{
		{ JobMessageTypeList.Codes.Export, typeof(EntryInstructionBasicDetailsLayout) },
		{ EUJobMessageTypeList.Codes.MiscellaneousCustoms, typeof(EntryInstructionBasicDetailsLayout) },
	};

	protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
}
