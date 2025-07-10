using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class DefaultSetterForInvoiceHeaderTest : TestCaseWithFactory
{
	public void TestDefaultForNewElementCore_CopyAdditionalDeliveryTerms_WhenIsExportAndIsUCC6AndShipmentIncoTermIsOther()
	{
		const string myAdditionalDeliveryTerms = "CopyThat";
		var otherIncoTerms = Core.Constants.IncoTerms.Other;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_ShipmentIncoTerm = otherIncoTerms;
			declaration.ZG_AdditionalDeliveryTerms = myAdditionalDeliveryTerms;

			var invoice = declaration.Invoices.AddNew();

			AssertEquals("Default JZ_IncoTerm", otherIncoTerms, invoice.JZ_IncoTerm);
			AssertEquals("Default JZ_AdditionalTerms", myAdditionalDeliveryTerms, invoice.JZ_AdditionalTerms);
		}
	}

	public void TestDefaultForNewElementCore_NotCopyAdditionalDeliveryTerms_WhenIsExportButNotUCC6AndShipmentIncoTermIsOther()
	{
		const string myAdditionalDeliveryTerms = "DoNotCopy";
		var otherIncoTerms = Core.Constants.IncoTerms.Other;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_ShipmentIncoTerm = otherIncoTerms;
			declaration.ZG_AdditionalDeliveryTerms = myAdditionalDeliveryTerms;

			var invoice = declaration.Invoices.AddNew();

			AssertNotNull(invoice);
			AssertEquals("Default JZ_IncoTerm", otherIncoTerms, invoice.JZ_IncoTerm);
			AssertNullOrEmpty("Default JZ_AdditionalTerms Is Empty", invoice.JZ_AdditionalTerms);
		}
	}

	public void TestDefaultForNewElementCore_NotCopyAdditionalDeliveryTerms_WhenIsExportAndIsUCC6AndShipmentIncoTermIsNotOther()
	{
		var freeOnBoardIncoTerms = Core.Constants.IncoTerms.FreeOnBoard;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_ShipmentIncoTerm = freeOnBoardIncoTerms;
			declaration.ZG_AdditionalDeliveryTerms = "DoNotCopy";

			var invoice = declaration.Invoices.AddNew();

			AssertNotNull(invoice);
			AssertEquals("Default JZ_IncoTerm", freeOnBoardIncoTerms, invoice.JZ_IncoTerm);
			AssertNullOrEmpty("Default JZ_AdditionalTerms Is Empty", invoice.JZ_AdditionalTerms);
		}
	}

	public void TestDefaultForNewElementCore_NotCopyAdditionalDeliveryTerms_WhenImportAndShipmentIncoTermIsOther()
	{
		var freeOnBoardIncoTerms = Core.Constants.IncoTerms.Other;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_ShipmentIncoTerm = freeOnBoardIncoTerms;
		declaration.ZG_AdditionalDeliveryTerms = "DoNotCopy";

		var invoice = declaration.Invoices.AddNew();

		AssertNotNull(invoice);
		AssertEquals("Default JZ_IncoTerm", freeOnBoardIncoTerms, invoice.JZ_IncoTerm);
		AssertNullOrEmpty("Default JZ_AdditionalTerms Is Empty", invoice.JZ_AdditionalTerms);
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(JobDeclaration declaration, bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);
}
