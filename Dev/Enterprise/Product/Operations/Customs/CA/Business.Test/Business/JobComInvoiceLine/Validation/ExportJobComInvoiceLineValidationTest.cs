using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestCheckJI_InvoiceUQ()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_InvoiceUQInfo, "???", "GA");
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_InvoiceUQInfo, "???", "KGM");
		}

		public void TestCheckJI_InvoiceQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			invoiceLine.JI_InvoiceQuantity = -1m;
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			invoiceLine.JI_InvoiceQuantity = 1m;
			AssertNoNotifications(invoiceLine.JI_InvoiceQuantityInfo);
		}

		public void TestCheckJI_CustomsUnitQty()
		{
			invoiceLine.JI_Tariff = ExportTariffCode;

			invoiceLine.JI_CustomsUnitQty = "Z@";
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsUnitQtyInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_CustomsUnitQty = UnitOfMeasureListForDLM.Codes.Dozen;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsUnitQtyInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsUnitQtyInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_Tariff = ExportTariffCodeNoUnits;

			invoiceLine.JI_CustomsUnitQty = "Z@";
			AssertNoNotifications(invoiceLine.JI_CustomsUnitQtyInfo);
			invoiceLine.JI_CustomsUnitQty = "";
			AssertNoNotifications(invoiceLine.JI_CustomsUnitQtyInfo);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			invoiceLine.JI_Tariff = ExportTariffCode;

			invoiceLine.JI_CustomsQuantity = -12m;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_CustomsQuantity = 12m;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_Tariff = ExportTariffCodeNoUnits;

			invoiceLine.JI_CustomsQuantity = -12m;
			AssertNoNotifications(invoiceLine.JI_CustomsQuantityInfo);
			invoiceLine.JI_CustomsQuantity = 0;
			AssertNoNotifications(invoiceLine.JI_CustomsQuantityInfo);

			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			invoiceLine.JI_CustomsQuantity = -1m;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeZero);
		}

		const string conveyanceCountError = "The number of Conveyance IDs entered should match the Customs Number Quantity.";

		public void TestCheckJI_CustomsQuantityWithVins()
		{
			invoiceLine.CA_ConveyanceIdentificationNumber = "111,222 333";
			invoiceLine.JI_CustomsUnitQty = "NMB";
			invoiceLine.JI_CustomsQuantity = 2;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, conveyanceCountError);
			invoiceLine.JI_CustomsQuantity = 3;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, conveyanceCountError);
			invoiceLine.CA_ConveyanceIdentificationNumber = "111,222";
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, conveyanceCountError);
			invoiceLine.CA_ConveyanceIdentificationNumber = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, conveyanceCountError);
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.CA_ConveyanceIdentificationNumber = "111,222";
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, conveyanceCountError);
		}

		public new void TestCheckJI_Description()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			const string maxlengthWarning = "Product Description is too long. It will be truncated to 140 characters in the message sent to Customs.";

			invoiceLine.JI_Description = "Funny Looking Box";
			string messageError = MandatoryValidation.YouHaveNotEntered + " a Product Description.";
			AssertNoMessageError(invoiceLine.JI_DescriptionInfo, messageError);
			AssertNoWarningContaining(invoiceLine.JI_DescriptionInfo, maxlengthWarning);

			invoiceLine.JI_Description = ZString.Empty;
			AssertHasMessageError(invoiceLine.JI_DescriptionInfo, messageError);

			invoiceLine.JI_Description = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			AssertHasWarningContaining(invoiceLine.JI_DescriptionInfo, maxlengthWarning);

			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			invoiceLine.Validation.ValidateJI_Description();
			AssertNoWarningContaining(invoiceLine.JI_DescriptionInfo, maxlengthWarning);
		}

		public void TestCheckJI_Tariff()
		{
			UseExportTarifList();
			invoiceLine.JI_Tariff = "12345600";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff code 12345600 not found in the export tariff code list.");
			invoiceLine.JI_Tariff = ExportTariffCode;
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
			invoiceLine.JI_Tariff = "1111111111";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff code 1111111111 not found in the export tariff code list.");
			invoiceLine.JI_Tariff = "1234";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff code must be at least 8 characters long.");

			UseCustomsTarifList();
			invoiceLine.JI_Tariff = "1234567800";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, ZString.Format("Classification was not found or is not valid for {0}.", ZDateTime.Today.ToShortDateString()));
			invoiceLine.JI_Tariff = CustomsTariffCode;
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
			invoiceLine.JI_Tariff = "1234578";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff code must be 10 characters long.");
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_RN_NKDefaultOrigin = ZString.Empty;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CountryOfOrigin = "12";
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_CountryOfOrigin = "13";
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrors(invoiceLine.JI_CountryOfOriginInfo);
		}

		public void TestCheckJI_StateOrRegionOfOrigin()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = "Z!";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_StateOrRegionOfOrigin = "AB";
			AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_StateOrRegionOfOrigin = "AL";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_StateOrRegionOfOrigin = "Z!";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_StateOrRegionOfOrigin = "AB";
			AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_StateOrRegionOfOrigin = "AL";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.JI_StateOrRegionOfOrigin = "Z!";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_StateOrRegionOfOrigin = "AB";
			AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_StateOrRegionOfOrigin = "AL";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);

			var notEnteredMessageError = MandatoryValidation.YouHaveNotEntered + " a Province of Origin/Shipment";
			var notEnteredWarning = "You have not entered a province of Origin/Shipment so the province of the Exporter (ON) will be sent in the G7 message.";
			invoiceLine.JI_StateOrRegionOfOrigin = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredMessageError);
			var exporter = Factory.New<OrgHeader>();
			exporter.MainAddress.OA_State = "ON";
			exporter.MainAddress.OA_RL_NKRelatedPortCode = "CAXXX";
			declaration.JE_OH_Supplier = exporter.PK;
			invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
			AssertHasWarningContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredWarning);
			AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredMessageError);
			exporter.MainAddress.OA_State = ZString.Empty;
			invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
			AssertNoWarningContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredWarning);
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredMessageError);
			exporter.MainAddress.OA_State = "ON";
			exporter.MainAddress.OA_RL_NKRelatedPortCode = "USXXX";
			invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
			AssertNoWarningContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredWarning);
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredMessageError);
			exporter.MainAddress.OA_State = "ON";
			exporter.MainAddress.OA_RL_NKRelatedPortCode = "CAXXX";
			invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
			AssertHasWarningContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredWarning);
			AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredMessageError);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
			AssertNoWarningContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredWarning);
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, notEnteredMessageError);
		}

		#region Implementation

		void UseCustomsTarifList()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		void UseExportTarifList()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		const string CustomsTariffCode = "1234567890";
		const string CustomsTariffUnits = "NMB";
		const string ExportTariffCode = "1234567800";
		const string ExportTariffDescription = "EXPORT TARIFF DESCRIPTION";
		const string ExportTariffUnits = "KGM";
		const string ExportTariffCodeNoUnits = "1234569900";

		protected override void SetUp()
		{
			base.SetUp();
			var cacClassHeader = Factory.New<CACClassHeader>();
			cacClassHeader.ZA_ClassificationNumber = CustomsTariffCode;
			cacClassHeader.ZA_StatisticalUOMCode = CustomsTariffUnits;
			cacClassHeader.ZA_AreaCode = "AAA";
			cacClassHeader.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			cacClassHeader.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);

			var helper = new CACExportTariffTestCase(Factory);
			helper.CreateNewTariffIfNotExists(ExportTariffCode, ExportTariffDescription, ExportTariffUnits);
			helper.CreateNewTariffIfNotExists(ExportTariffCodeNoUnits, ExportTariffDescription);
			helper.CreateNewTariffIfNotExists(CustomsTariffCode, ExportTariffDescription, CustomsTariffUnits);
			Factory.Save();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		#endregion
	}
}
