using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business.DocumentWrappers.Testing
{
	class SummaryCusDecDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor_EntryHeaderIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new SummaryCusDecDocumentWrapper(null));
		}

		public void TestEntryHeader()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals(entryHeader, wrapper.EntryHeader);
		}

		public void TestDeclaration()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals(entryHeader.Declaration, wrapper.Declaration);
		}

		public void TestTotalLineCount()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals(2, wrapper.TotalLineCount);
		}

		public void TestEntryLines()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			var entryLine1 = entryHeader.MergedLines[0];
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines[1];
			entryLine2.CL_LineNumber = 2;

			CombineAssertions(() =>
			{
				AssertEquals(2, wrapper.EntryLines.Count);
				AssertEquals((ZShort)1, wrapper.EntryLines[0].EntryLineNo);
				AssertEquals(entryLine1.PK, wrapper.EntryLines[0].EntryLine.PK);
				AssertEquals((ZShort)2, wrapper.EntryLines[1].EntryLineNo);
				AssertEquals(entryLine2.PK, wrapper.EntryLines[1].EntryLine.PK);
			});
		}

		public void TestDeclarationType()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.EntryInstruction.CEI_Style = "CEI";
			AssertEquals("CEI", wrapper.DeclarationType);
		}

		public void TestOfficeOfExit()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.EntryInstruction.ASY_PortOfExit = "Port";
			AssertEquals("Port", wrapper.OfficeOfExit);
		}

		public void TestCustomsOfficeName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("ZZ", "ParentGrouping");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Botswana, "Botswana", parentGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsUQ", Core.Constants.CountryCodes.Botswana, 4);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Botswana, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ARIA", "Ariamsvlei", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.JE_CustomsOffice = "ARIA";
			AssertEquals("Ariamsvlei", wrapper.CustomsOfficeName);
		}

		public void TestExporterOrganizationCode()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			var supplier = Factory.New<OrgHeader>();
			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "CSC123");
			entryHeader.Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("CSC123", wrapper.ExporterOrganizationCode);
		}

		public void TestImporterOrganizationCode()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CCD123");
			entryHeader.Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("CCD123", wrapper.ImporterOrganizationCode);
		}

		public void TestDutyPayerName()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals("Empty if no DutyPayer", "", wrapper.DutyPayerName);

			var dutyPayer = Factory.New<OrgHeader>();
			dutyPayer.OH_FullName = "Test DutyPayer";
			entryHeader.Declaration.JE_OH_DutyPayer = dutyPayer.PK;
			AssertEquals("DutyPayer Name", "Test DutyPayer", wrapper.DutyPayerName);
		}

		public void TestDutyPayerAddress()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals("Empty if no DutyPayer", "", wrapper.DutyPayerAddress);

			var dutyPayer = Factory.New<OrgHeader>();
			dutyPayer.MainAddress.Address1 = "Test Address";
			entryHeader.Declaration.JE_OH_DutyPayer = dutyPayer.PK;
			AssertEquals("DutyPayer Address", "TEST ADDRESS", wrapper.DutyPayerAddress);
		}

		public void TestDeclarantName()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals("Empty if no Declarant", "", wrapper.DeclarantName);

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_FullName = "Test Declarant";
			entryHeader.Declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertEquals("Declarant Name", "Test Declarant", wrapper.DeclarantName);
		}

		public void TestDeclarantAddress()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals("Empty if no Declarant", "", wrapper.DeclarantAddress);

			var declarant = Factory.New<OrgHeader>();
			var mainAddress = declarant.MainAddress;
			mainAddress.Address1 = "Test Address";
			entryHeader.Declaration.JE_OA_DeclarantAddress = mainAddress.PK;
			AssertEquals("Declarant Address", "TEST ADDRESS", wrapper.DeclarantAddress);
		}

		public void TestOriginCE()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.JE_RL_NKOrigin = "AUSYD";
			AssertEquals(Core.Constants.CountryCodes.Australia, wrapper.OriginCE);
		}

		public void TestDestinationCD()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.JE_RL_NKFinalDestination = "BWBBK";
			AssertEquals(Core.Constants.CountryCodes.Botswana, wrapper.DestinationCD);
		}

		public void TestVessalCountryOfReg()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			var vessel = Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, "BUNGA DELIMA");
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Botswana;
			entryHeader.Declaration.JE_VesselName = "BUNGA DELIMA";
			AssertEquals(Core.Constants.CountryCodes.Botswana, wrapper.VessalCountryOfReg);
		}

		public void TestPortOfArrival()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.JE_RL_NKPortOfArrival = "BWBBK";
			AssertEquals("Kasane", wrapper.PortOfArrival);
		}

		public void TestPortOfLoading()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Sydney", wrapper.PortOfLoading);
		}

		public void TestDeliveryTerms()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.Invoices[0].JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			entryHeader.Declaration.Invoices[1].JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			AssertEquals(2, entryHeader.Declaration.Invoices.Count);
			AssertEquals("MULTIPLE INVOICES", wrapper.DeliveryTerms);

			entryHeader.Declaration.Invoices[1].Delete();
			wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals("FOB Free On Board", wrapper.DeliveryTerms);

			entryHeader.Declaration.Invoices[0].Delete();
			wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals(" ", wrapper.DeliveryTerms);
		}

		public void TestIncoTerm()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.Invoices[0].JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			entryHeader.Declaration.Invoices[1].JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			AssertEquals(Core.Constants.IncoTerms.FreeOnBoard, wrapper.IncoTerm);

			entryHeader.Declaration.Invoices.DeleteAll();
			wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals("", wrapper.IncoTerm);
		}

		public void TestIncoTermDec()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.Invoices[0].JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			entryHeader.Declaration.Invoices[1].JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			AssertEquals("Free On Board", wrapper.IncoTermDec);

			entryHeader.Declaration.Invoices.DeleteAll();
			wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals("", wrapper.IncoTermDec);
		}

		public void TestInvoiceCurrency()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.Invoices[0].JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Namibia;
			AssertEquals(Core.Constants.CurrencyCodes.Namibia, wrapper.InvoiceCurrency);
		}

		public void TestTotalInvoiceAmount()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.Invoices[0].JZ_InvoiceAmount = 50m;
			entryHeader.Declaration.Invoices[1].JZ_InvoiceAmount = 50m;
			AssertEquals(100m, wrapper.TotalInvoiceAmount);
		}

		public void TestInvoiceNo()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.Invoices[0].JZ_InvoiceNumber = "INV1";
			AssertEquals("INV1", wrapper.InvoiceNo);

			entryHeader.Declaration.Invoices[1].JZ_InvoiceNumber = "INV2";
			AssertEquals("INV1,INV2", wrapper.InvoiceNo);
		}

		public void TestInvoiceCurrExRate()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			entryHeader.Declaration.Invoices[0].JZ_InvoiceCurrExRate = 10m;
			AssertEquals(10m, wrapper.InvoiceCurrExRate);
		}

		public void TestFirstInvoice()
		{
			var entryHeader = CreateEntryHeader();
			var wrapper = new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals(entryHeader.Declaration.Invoices[0], wrapper.FirstInvoice);
		}

		public void TestIBODocDataProvider_BusinessObjectToLogAgainst()
		{
			var entryHeader = CreateEntryHeader();
			var provider = (IBODocDataProvider)new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals(entryHeader, provider.BusinessObjectToLogAgainst);
		}

		public void TestIBODocDataProvider_ToString()
		{
			var entryHeader = CreateEntryHeader();
			var provider = (IBODocDataProvider)new SummaryCusDecDocumentWrapper(entryHeader);
			AssertEquals(entryHeader.HumanReadableName, provider.ToString());
		}

		CusEntryHeader CreateEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			return entryHeader;
		}
	}
}
