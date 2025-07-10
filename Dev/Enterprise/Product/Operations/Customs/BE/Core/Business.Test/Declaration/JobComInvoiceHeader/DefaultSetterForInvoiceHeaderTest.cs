using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class DefaultSetterForInvoiceHeaderTest : TestCaseWithFactory
{
	public void TestDefaultForNewElement()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		declaration.JE_ShipmentIncoTermPlace = "BRUSSELS";
		declaration.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;

		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				CreateNewInvoiceAndAssertCommonDefaults(declaration);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				CreateNewInvoiceAndAssertCommonDefaults(declaration);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				CreateNewInvoiceAndAssertCommonDefaults(declaration);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				CreateNewInvoiceAndAssertCommonDefaults(declaration);
			}
		});
	}

	static void CreateNewInvoiceAndAssertCommonDefaults(JobDeclaration declaration)
	{
		var invoice = declaration.Invoices.AddNew();

		AssertEquals("JZ_IncoTerm default value", Core.Constants.IncoTerms.FreeOnBoard, invoice.JZ_IncoTerm);
		AssertEquals("JZ_IncoTermPlace default value", "BRUSSELS", invoice.JZ_IncoTermPlace);
		AssertEquals("ZG_AgreedPlaceCode default value", Core.Constants.CountryCodes.Belgium, invoice.ZG_AgreedPlaceCode);
	}
}
