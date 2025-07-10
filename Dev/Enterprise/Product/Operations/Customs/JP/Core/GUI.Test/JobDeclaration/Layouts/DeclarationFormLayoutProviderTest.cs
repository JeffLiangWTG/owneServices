using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.JP.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(DeclarationFormLayoutProvider))]
	sealed class DeclarationFormLayoutProviderTest : DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, JobDeclaration>
	{
		public void TestProviderRegisteredToJapan()
		{
			using (var mainControl = new CustomsBrokerageUserControl())
			{
				mainControl.JobDeclaration = GetNewDeclaration();
				mainControl.LoadMiscOptionsUserControl();
				CombineAssertions(() =>
				{
					AssertNotNull(mainControl.DynamicMiscOptions);
					AssertNull(mainControl.MiscOptions);
				});
			}
		}

		protected override Type ExpectedDeclarationDetailsLayoutType => null;

		protected override Type ExpectedDeclarationShipmentDetailsLayoutType => null;

		protected override Type ExpectedDeclarationShipmentTypeLayoutType => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => null;

		protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(OrganizationsLayouts);

		protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(MiscOptionsLayouts) } };

		protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes
		{
			get
			{
				return  new Dictionary<string, Type>
							{
								{ JobMessageTypeList.Codes.Export, typeof(JobComInvoiceHeaderLayouts) },
								{ JobMessageTypeList.Codes.Import, typeof(JobComInvoiceHeaderLayouts) },
							};
			}
		}

		protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => null;

		protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => null;
	}
}
