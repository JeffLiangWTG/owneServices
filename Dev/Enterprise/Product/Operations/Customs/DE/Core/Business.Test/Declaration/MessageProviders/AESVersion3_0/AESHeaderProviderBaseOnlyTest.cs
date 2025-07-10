using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(AESHeaderProvider))]
	sealed class AESHeaderProviderBaseOnlyTest : AESHeaderProviderAbstractTest<AESHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new AESHeaderProviderBase(null));
		}

		public void TestLocalReferenceNumber()
		{
			entryHeader.LocalReferenceNumber = "12345";
			AssertEquals("12345", Provider.LocalReferenceNumber);
		}

		public void TestIsContainerized()
		{
			CombineAssertions(() =>
			{
				foreach (var containerFlag in new ZString[] { Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.ULD, Core.Constants.ContainerModes.Containerised })
				{
					declaration.JE_ContainerMode = containerFlag;
					AssertEquals($"IsContainerised = '{containerFlag}'", true, Provider.IsContainerized);
				}
				declaration.JE_ContainerMode = "AAA";
				AssertEquals("Miscellaneous IsContainerised", false, Provider.IsContainerized);
			});
		}

		public void TestContainerIndicatorSpecified()
		{
			CombineAssertions(() =>
			{
				AssertEquals("JE_ContainerMode is empty", false, Provider.ContainerIndicatorSpecified);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Other;
				AssertEquals("JE_ContainerMode isn't empty", true, Provider.ContainerIndicatorSpecified);
			});
		}

		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter("MRN1", new ZDateTime(2019, 6, 12));
			AssertEquals("MRN value", "MRN1", Provider.MRN);
		}

		public void TestExportCustomsOffice()
		{
			declaration.JE_CustomsOffice = "CO001";
			AssertEquals("ExportCustomsOffice", "CO001", Provider.ExportCustomsOffice);
		}

		public void TestSupplementaryDeclarationCustomsOffice()
		{
			AssertEquals("OFFICE1", Provider.SupplementaryDeclarationCustomsOffice);
		}

		public void TestDeclarant_WithoutEntryInstruction()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var declarant = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "GREOR1", Provider.Declarant.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS1", Provider.Declarant.EoriBranchSuffix);
			});
		}

		public void TestDeclarant_WithEntryInstructionAndDeclarant()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var declarant = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			PrepareAndLinkEntryInstruction(ZString.Empty);
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "GREOR1", Provider.Declarant.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS1", Provider.Declarant.EoriBranchSuffix);
			});
		}

		public void TestDeclarant_WithEntryInstructionAndDeclarantAndInvalidConstellation()
		{
			var declarant = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			PrepareAndLinkEntryInstruction("0100");
			AssertEquals("Declarant is null", null, Provider.Declarant);
		}

		public void TestRepresentative_WithoutEntryInstruction()
		{
			var representative = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS2");
			declaration.JE_OA_Representative = representative.PK;
			AssertEquals("Representative is null", null, Provider.Representative);
		}

		public void TestRepresentative_WithEntryInstructionAndRepresentative()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var representative = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS2");
			declaration.JE_OA_Representative = representative.PK;
			PrepareAndLinkEntryInstruction(PartyConstellationCodeList.Codes._0011);
			using (Factory.SetTemporaryCurrentUser("Sachbearbeiter", "Bob Baumeister", "06131474747", "bob.baumeister@samplefreight.de"))
			{
				AssertPartyWithContactPerson(Provider.Representative
					, "GREOR2"
					, "EBS2"
					, "MAX MUSTERMANN"
					, "TESTSTRASSE 1"
					, "MAINZ"
					, "55126"
					, Core.Constants.CountryCodes.Germany
					, "Sachbearbeiter"
					, "Bob Baumeister"
					, "06131474747"
					, ""
					, "bob.baumeister@samplefreight.de");
			}
		}

		public void TestRepresentative_WithEntryInstructionAndRepresentativeAndInvalidConstellation()
		{
			var representative = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS2");
			declaration.JE_OA_Representative = representative.PK;
			PrepareAndLinkEntryInstruction("0200");
			AssertEquals("Representative is null", null, Provider.Representative);
		}

		public void TestInlandTransportMeansMode()
		{
			CombineAssertions(() =>
			{
				declaration.JE_TransportModeInland = TransportTypeList.Codes.Mail;
				AssertEquals("CEI_Style isn't XXX9XX", ModeOfTransportList.Codes._5_PostalConsignment, Provider.InlandTransportMeansMode);
			});
		}

		public void TestInlandTransportMeansMode_Style4thDigitIs9()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Mail;
			AssertNull("CEI_Style is XXX9XX", Provider.InlandTransportMeansMode);
		}

		public void TestActiveBorderTransportMeansSpecified()
		{
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				declaration.ZG_BorderTransportMeans = ZString.Empty;
				AssertEquals("ZG_BorderTransportMeans is Empty", false, Provider.ActiveBorderTransportMeansSpecified);

				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._21;
				AssertEquals("Both Entered", true, Provider.ActiveBorderTransportMeansSpecified);

				declaration.JE_TransportMode = ZString.Empty;
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._10;
				AssertEquals("JE_TransportMode is Empty", false, Provider.ActiveBorderTransportMeansSpecified);
			});
		}

		public void TestBorderTransportMeansMode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.BorderTransportMeansMode);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Sea", "1", Provider.BorderTransportMeansMode);
			});
		}

		public void TestBorderTransportMeansType()
		{
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
			AssertEquals(ExportBorderTransportMeansList.Codes._40, Provider.BorderTransportMeansType);
		}

		public void TestBorderTransportMeansIdentity()
		{
			CombineAssertions(() =>
			{
				AssertBorderTransportMeansIdentity(ZString.Empty, ExportBorderTransportMeansList.Codes._10, null);
				AssertBorderTransportMeansIdentity(Core.Constants.TransportModes.Mail, ZString.Empty, null);
			});
		}

		public void TestBorderTransportMeansIdentity_Sea_10()
		{
			AssertBorderTransportMeansIdentity(Core.Constants.TransportModes.Sea, ExportBorderTransportMeansList.Codes._10, "7894450");
		}

		public void TestBorderTransportMeansIdentity_Sea_Not10()
		{
			AssertBorderTransportMeansIdentity(Core.Constants.TransportModes.Sea, ExportBorderTransportMeansList.Codes._11, "TESTVESSEL");
		}

		public void TestBorderTransportMeansIdentity_Air()
		{
			AssertBorderTransportMeansIdentity(Core.Constants.TransportModes.Air, ExportBorderTransportMeansList.Codes._40, "Flight123");
		}

		public void TestBorderTransportMeansIdentity_NotSEAOrAir()
		{
			AssertBorderTransportMeansIdentity(Core.Constants.TransportModes.Mail, ExportBorderTransportMeansList.Codes._10, "TESTVESSEL");
		}

		public void TestBorderTransportMeansNationality()
		{
			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, Provider.BorderTransportMeansNationality);
		}

		public void TestTransactionType_NoInvoices()
		{
			AssertEquals("No Invoices", ZString.Empty, Provider.TransactionType);
		}

		public void TestTransactionType_SingleInvoice()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationCode = "3";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			AssertEquals("Single Invoice", "3", Provider.TransactionType);
		}

		public void TestTransactionType_SameValue()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationCode = "3";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "3";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals("Same", "3", Provider.TransactionType);
		}

		public void TestTransactionType_DifferentValue()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationCode = "3";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "4";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals("Different", ZString.Empty, Provider.TransactionType);
		}

		public void TestInvoiceAmount()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_FreeOfCharge = ZBool.True;
			invoice1.JZ_InvoiceAmount = 2.2222m;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			invoice2.JZ_FreeOfCharge = ZBool.False;
			invoice2.JZ_InvoiceAmount = 3.3333m;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_RX_NKInvoice_Currency = "AUD";
			invoice3.JZ_FreeOfCharge = ZBool.False;
			invoice3.JZ_InvoiceAmount = 2.2222m;
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_CL = entryLine.PK;

			AssertEquals(4.42m, Provider.InvoiceAmount);
		}

		public void TestInvoiceAmount_SameCurrency()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			invoice1.JZ_FreeOfCharge = ZBool.False;
			invoice1.JZ_InvoiceAmount = 2.2222m;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_FreeOfCharge = ZBool.False;
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			invoice2.JZ_InvoiceAmount = 3.3333m;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals(5.56m, Provider.InvoiceAmount);
		}

		public void TestInvoiceAmountCurrency_NonLocalCurrency()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_FreeOfCharge = ZBool.True;
			invoice.JZ_RX_NKInvoice_Currency = "TRY";
			invoice.JZ_FreeOfCharge = ZBool.False;
			invoice.JZ_InvoiceAmount = 3.3333m;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 3.3333m;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			CombineAssertions(() =>
			{
				AssertEquals(3.33m, Provider.InvoiceAmount);
				AssertEquals("TRY", Provider.Currency);
			});
		}

		public void TestInvoiceAmountCurrency_MultipleInvoiceHeadersWithMultipleCurrencies()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_FreeOfCharge = ZBool.False;
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			invoice1.JZ_InvoiceAmount = 2.2222m;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2.222m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();

			invoice2.JZ_RX_NKInvoice_Currency = "TRY";
			invoice2.JZ_FreeOfCharge = ZBool.False;
			invoice2.JZ_InvoiceAmount = 3.3333m;
			invoiceLine2.JI_LinePrice = 3.3333m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			CombineAssertions(() =>
			{
				AssertEquals(4.84m, Provider.InvoiceAmount);
				AssertEquals("EUR", Provider.Currency);
			});
		}

		public void TestInvoiceAmountCurrency_MultipleInvHeadersWithNonLocalCurrency()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_FreeOfCharge = ZBool.False;
			invoice1.JZ_RX_NKInvoice_Currency = "TRY";
			invoice1.JZ_InvoiceAmount = 2.2222m;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2.222m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();

			invoice2.JZ_RX_NKInvoice_Currency = "TRY";
			invoice2.JZ_FreeOfCharge = ZBool.False;
			invoice2.JZ_InvoiceAmount = 3.3333m;
			invoiceLine2.JI_LinePrice = 3.3333m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			CombineAssertions(() =>
			{
				AssertEquals(5.56m, Provider.InvoiceAmount);
				AssertEquals("TRY", Provider.Currency);
			});
		}

		public void TestInvoiceAmountCurrency_MultipleInvHeadersWithNonLocalCurrency_FreeOfChargeCurrencyIsExcluded()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_FreeOfCharge = ZBool.False;
			invoice1.JZ_RX_NKInvoice_Currency = "TRY";
			invoice1.JZ_InvoiceAmount = 2.2222m;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2.222m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();

			invoice2.JZ_RX_NKInvoice_Currency = "TRY";
			invoice2.JZ_FreeOfCharge = ZBool.False;
			invoice2.JZ_InvoiceAmount = 3.3333m;
			invoiceLine2.JI_LinePrice = 3.3333m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var entryLine3 = entryHeader.MergedLines.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();

			invoice3.JZ_RX_NKInvoice_Currency = "EUR";
			invoice3.JZ_FreeOfCharge = ZBool.True;
			invoice3.JZ_InvoiceAmount = 3.3333m;
			invoiceLine3.JI_LinePrice = 3.3333m;
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_CL = entryLine3.PK;

			CombineAssertions(() =>
			{
				AssertEquals(5.56m, Provider.InvoiceAmount);
				AssertEquals("TRY", Provider.Currency);
			});
		}

		public void TestInvoiceAmount_Normalize()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_FreeOfCharge = ZBool.False;
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			invoice1.JZ_InvoiceAmount = 2.2000m;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_LinePrice = 2.2000m;
			AssertEquals("2.2", Provider.InvoiceAmount.ToString());
		}

		public void TestCurrency_NoInvoiceLines()
		{
			AssertEquals("No Invoice Lines", "EUR", Provider.Currency);
		}

		public void TestCurrency_SingleInvoice()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			AssertEquals("Single Invoice", "AUD", Provider.Currency);
		}

		public void TestCurrency_SameValue()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "AUD";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals("Same Currencies", "AUD", Provider.Currency);
		}

		public void TestCurrency_DifferentValue()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals("Different Currencies", "EUR", Provider.Currency);
		}

		public void TestInvoiceAmountAndCurrencySpecified_TotalAmount0()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_FreeOfCharge = ZBool.True;
			invoice1.JZ_InvoiceAmount = 2.2222m;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_FreeOfCharge = ZBool.True;
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			AssertEquals("Total amount = 0", true, Provider.InvoiceAmountAndCurrencySpecified);
		}

		public void TestInvoiceAmountAndCurrencySpecified_CurrencyOfAllInvHeadersEmpty()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_FreeOfCharge = ZBool.True;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_FreeOfCharge = ZBool.True;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			AssertEquals("JZ_RX_NKInvoice_Currency are empty of all InvHeaders", false, Provider.InvoiceAmountAndCurrencySpecified);
		}

		public void TestInvoiceAmountAndCurrencySpecified_AmountGreater0_TotalAmount0()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_FreeOfCharge = ZBool.True;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_FreeOfCharge = ZBool.True;
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			invoice1.JZ_InvoiceAmount = 2.2222m;
			AssertEquals("Exist amount > 0 and total amount = 0", true, Provider.InvoiceAmountAndCurrencySpecified);
		}

		public void TestInvoiceAmountAndCurrencySpecified_FreeOfChargeFalse_TotalAmount0()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_FreeOfCharge = ZBool.True;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_FreeOfCharge = ZBool.True;
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			invoice2.JZ_FreeOfCharge = ZBool.False;
			Assert("Exist JZ_FreeOfCharge are false and total amount = 0", Provider.InvoiceAmountAndCurrencySpecified);
		}

		public void TestInvoiceAmountAndCurrencySpecified_FreeOfChargeFalse_TotalAmountGrreater0()
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_FreeOfCharge = ZBool.True;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_FreeOfCharge = ZBool.True;
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			invoice2.JZ_FreeOfCharge = ZBool.False;
			invoice1.JZ_InvoiceAmount = 2.2222m;
			invoice2.JZ_InvoiceAmount = 3.3333m;
			Assert("Exist JZ_FreeOfCharge are false and total amount > 0", Provider.InvoiceAmountAndCurrencySpecified);
		}

		public void TestCommercialReferenceNumber_AllSame()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_UCR = "123";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_UCR = "123";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals("123", Provider.CommercialReferenceNumber);
		}

		public void TestCommercialReferenceNumber_Different()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_UCR = "123";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_UCR = "456";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals(ZString.Empty, Provider.CommercialReferenceNumber);
		}

		public void TestDeliveryTerms_NoInvoices()
		{
			AssertEquals(null, Provider.DeliveryTerms);
		}

		public void TestDeliveryTerms_SingleInvoice()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_IncoTermPlace = "DEWIB";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			AssertNotNull("Single Invoice and IncotermCode not empty", Provider.DeliveryTerms);
		}

		public void TestDeliveryTerms_IncotermEmpty()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTermPlace = "DEWIB";
			invoice1.JZ_IncoTerm = string.Empty;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			AssertNull("IncotermCode empty", Provider.DeliveryTerms);
		}

		public void TestDeliveryTerms_SameValue()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_IncoTermPlace = "DEWIB";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_IncoTermPlace = "DEWIB";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			AssertNotNull("Same IncotermCode and not empty", Provider.DeliveryTerms);
		}

		public void TestDeliveryTerms_DifferentValue()
		{
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_IncoTermPlace = "DEWIB";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "CIF";
			invoice1.JZ_IncoTermPlace = "DEWIB";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			AssertNull("Different IncotermCode", Provider.DeliveryTerms);
		}

		public void TestTotalGrossMass()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 2007100m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Milligrams;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			AssertEquals("Total weight", 2.007m, Provider.TotalGrossMass);
		}

		public void TestTotalGrossMass_Normalize()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 2100000m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Milligrams;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			AssertEquals("Total weight", 2.1m, Provider.TotalGrossMass);
		}

		public void TestTotalGrossMass_Invalid()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 2100000m;
			invoice.JZ_WeightUQ = ZString.Empty;
			declaration.JE_TotalWeightUnit = ZString.Empty;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			AssertEquals("Total weight", ZDecimal.Zero, Provider.TotalGrossMass);
		}

		[TestDate(2019, 2, 7, 3, 8, 9, 297)]
		[TestTimeZone]
		public void TestSubmissionDateAndTimeUtc()
		{
			AssertEquals(new DateTime(2019, 2, 7, 3, 8, 9), Provider.SubmissionDateAndTimeUtc.DateAndTime);
		}

		public void TestDepartureTransportMeans()
		{
			Assert(Provider.DepartureTransportMeans is IReadOnlyCollection<DepartureTransportMeansProvider>);
		}

		public void TestTransportEquipments()
		{
			PrepareTransportEquipment();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(2, Provider.TransportEquipments.Count);
		}

		public void TestTransportEquipments_NotPopulated()
		{
			PrepareTransportEquipment();
			declaration.JE_ContainerMode = "AAA";
			AssertEquals(false, Provider.TransportEquipments.Any());
		}

		protected override IEnumerable<Expression<Func<AESHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Declarant;
			yield return x => x.Representative;
			yield return x => x.DepartureTransportMeans;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Mail;
			declaration.ZG_BorderTransportMeans = ImportBorderTransportMeansList.Codes.Other;
			declaration.ZG_Box18TransportID = "1";
			declaration.ZG_Box18TransportNationality = "CN";
			var customsOffice = declaration.CustomsOffices.Cast<DEOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice)
							?? declaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice;
			customsOffice.CY_Data = "OFFICE1";
			entryLine = entryHeader.MergedLines.AddNew();
		}
		CusEntryLine entryLine;

		protected override AESHeaderProvider GetProvider() => new AESHeaderProviderBase(entryHeader);

		new IAESHeader Provider => base.Provider;

		CusEntryInstruction PrepareAndLinkEntryInstruction(ZString partyConstellation)
		{
			var entryInstruction = PrepareAndLinkEntryInstruction();
			entryInstruction.ZG_PartyConstellation = partyConstellation;
			return entryInstruction;
		}

		CusEntryInstruction PrepareAndLinkEntryInstruction()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_JE = declaration.PK;
			return entryInstruction;
		}

		void AssertBorderTransportMeansIdentity(ZString transportMode, ZString borderTransportMeans, string expected)
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TESTVESSEL";
			declaration.JE_TransportMode = transportMode;
			declaration.ZG_BorderTransportMeans = borderTransportMeans;

			declaration.JE_VesselName = vessel.RV_Code;
			declaration.JE_VoyageFlightNo = "Flight123";
			vessel.RV_LloydsNumber = "7894450";
			AssertEquals($"JE_TransportMode is {transportMode} and ZG_BorderTransportMeans is {borderTransportMeans}", expected, Provider.BorderTransportMeansIdentity);
		}
	}

	class AESHeaderProviderBase : AESHeaderProvider
	{
		public AESHeaderProviderBase(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}
	}
}
