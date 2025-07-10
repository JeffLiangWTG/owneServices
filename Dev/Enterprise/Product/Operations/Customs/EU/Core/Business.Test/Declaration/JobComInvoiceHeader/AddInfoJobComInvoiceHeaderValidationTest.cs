using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_AgreedPlaceCode_SameAsIncoTermOnInvoice()
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore", true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				declaration.ZG_AgreedPlaceCode = "1";
				invoiceHeader.ZG_AgreedPlaceCode = "2";
				AssertHasMessageError(invoiceHeader.ZG_AgreedPlaceCodeInfo, "Incoterm place code values do not match between Declaration and Invoice Header.");

				invoiceHeader.ZG_AgreedPlaceCode = "1";
				AssertNoMessageError(invoiceHeader.ZG_AgreedPlaceCodeInfo, "Incoterm place code values do not match between Declaration and Invoice Header.");
			}
		}

		public void TestCheckZG_TransportChargesMethodOfPayment()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "Transport Charges Method Of Payment");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "A", "Test 1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo, "Z", "A");
		}

		public void TestCheckZG_AgreedPlaceCode_IsRequired()
		{
			const string messageError = "Incoterm Place Code or Country Code is required";
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, true))
				{
					invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
					invoice.ZG_AgreedPlaceCode = CargoWise.Types.ZString.Empty;
					AssertHasMessageError("Empty ZG_AgreedPlaceCode", invoice.ZG_AgreedPlaceCodeInfo, messageError);

					invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
					invoice.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertNoNotifications("Type is XXX (Other)", invoice.ZG_AgreedPlaceCodeInfo);
				}
			});
		}

		public void TestCheckZG_AgreedPlaceCode_Country()
		{
			const string message = "Incoterm Place Code must be a valid country";
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, true))
				{
					invoice.ZG_AgreedPlaceCode = "XX";
					AssertHasMessageError("ZG_AgreedPlaceCode 'XX' should have an error", invoice.ZG_AgreedPlaceCodeInfo, message);

					invoice.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;
					AssertNoMessageError("ZG_AgreedPlaceCode 'BE' should not have an error", invoice.ZG_AgreedPlaceCodeInfo, message);
				}
			});
		}

		public void TestCheckZG_AgreedPlaceCode_UNLOCODE()
		{
			const string massage = "Incoterm Place Code must be a valid UNLOCODE";
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, true))
				{
					invoice.ZG_AgreedPlaceCode = "BEAAA";
					AssertHasMessageError("ZG_AgreedPlaceCode 'BEAAA' should have an error", invoice.ZG_AgreedPlaceCodeInfo, massage);

					invoice.ZG_AgreedPlaceCode = "BEBRU";
					AssertNoMessageError("ZG_AgreedPlaceCode 'BEBRU' should not have an error", invoice.ZG_AgreedPlaceCodeInfo, massage);
				}
			});
		}

		public void TestCheckZG_AgreedPlaceCode_Invalid()
		{
			const string message = "Incoterm Place Code must either be a valid country or a valid UNLOCODE";
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, true))
				{
					invoice.ZG_AgreedPlaceCode = "TRY";
					AssertHasMessageError("ZG_AgreedPlaceCode 'TRY' should have an error", invoice.ZG_AgreedPlaceCodeInfo, message);

					invoice.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;
					AssertNoMessageError("ZG_AgreedPlaceCode 'BE' should not have an error", invoice.ZG_AgreedPlaceCodeInfo, message);
				}
			});
		}
	}
}
