using System;
using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(DeclarationFormLayoutProvider))]
	sealed class DeclarationFormLayoutProviderTest : DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
	{
		protected override Type ExpectedDeclarationDetailsLayoutType => null;

		protected override Type ExpectedDeclarationShipmentTypeLayoutType => typeof(ShipmentTypeLayout);

		protected override Type ExpectedDeclarationShipmentDetailsLayoutType => typeof(ShipmentDetailsLayout);

		protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes
		{
			get
			{
				return new Dictionary<string, Type>
				{
					{ Common.EU.EUJobMessageTypeList.Codes.Export, typeof(TransportDetailsLayout) },
					{ Common.EU.EUJobMessageTypeList.Codes.Import, typeof(TransportDetailsLayout) },
				};
			}
		}

		protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(OrganisationsLayout);

		protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes
		{
			get
			{
				return new Dictionary<string, Type>
				{
					{ Common.EU.EUJobMessageTypeList.Codes.Export, typeof(MiscOptionsLayout) },
					{ Common.EU.EUJobMessageTypeList.Codes.Import, typeof(MiscOptionsLayout) },
				};
			}
		}

		protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes
		{
			get
			{
				return new Dictionary<string, Type>
				{
					{ Common.EU.EUJobMessageTypeList.Codes.Export, typeof(EntryInstructionDetailsBasicUserControlLayout) },
					{ Common.EU.EUJobMessageTypeList.Codes.Import, typeof(EntryInstructionDetailsBasicUserControlLayout) },
				};
			}
		}

		protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
	}
}
