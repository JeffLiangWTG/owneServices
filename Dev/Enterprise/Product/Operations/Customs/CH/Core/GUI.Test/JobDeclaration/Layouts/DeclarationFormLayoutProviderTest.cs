using System;
using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(DeclarationFormLayoutProvider))]
sealed class DeclarationFormLayoutProviderTest : Customs.GUI.Testing.DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
{
	protected override Type ExpectedDeclarationDetailsLayoutType => null;

	protected override Type ExpectedDeclarationShipmentDetailsLayoutType => typeof(ShipmentDetailsLayout);

	protected override Type ExpectedDeclarationShipmentTypeLayoutType => typeof(ShipmentTypeLayout);

	protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
		{
			{ CHJobMessageTypeList.Codes.Import, typeof(TransportDetailsLayout) },
			{ CHJobMessageTypeList.Codes.Export, typeof(TransportDetailsLayout) },
			{ CHJobMessageTypeList.Codes.ExportDeclarationActivation, typeof(TransportDetailsLayout) },
		};

	protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(OrganisationsLayout);

	protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type>
		{
			{ CHJobMessageTypeList.Codes.Import, typeof(ImportEntryInstructionDetailsLayout) },
			{ CHJobMessageTypeList.Codes.Export, typeof(ExportEntryInstructionDetailsLayout) },
			{ CHJobMessageTypeList.Codes.ExportDeclarationActivation, typeof(ExportEntryInstructionDetailsLayout) },
		};

	protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

	protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
}
