using System;
using System.Collections.Generic;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.KR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DeclarationFormLayoutProvider))]
	sealed class DeclarationFormLayoutProviderTest : DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
	{
		protected override Type ExpectedDeclarationDetailsLayoutType => null;

		protected override Type ExpectedDeclarationShipmentDetailsLayoutType => typeof(ShipmentDetailsLayout);

		protected override Type ExpectedDeclarationShipmentTypeLayoutType => typeof(ShipmentTypeLayout);

		protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { KRJobMessageTypeList.Codes.Export, typeof(EXPDeclarationTransportDetailsLayout) }, { KRJobMessageTypeList.Codes.Import, typeof(IMPDeclarationTransportDetailsLayout) }, { KRJobMessageTypeList.Codes.LocalExport, typeof(LEXDeclarationTransportDetailsLayout) } };

		protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(OrganisationsLayout);

		protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { KRJobMessageTypeList.Codes.Export, typeof(MiscOptionsLayout) }, { KRJobMessageTypeList.Codes.LocalExport, typeof(LocalExportMiscOptionsLayout) } };

		protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { KRJobMessageTypeList.Codes.Export, typeof(ExportCommercialInvoiceDetailsLayout) }, { KRJobMessageTypeList.Codes.Import, typeof(ImportCommercialInvoiceDetailsLayout) } };

		protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
	}
}
