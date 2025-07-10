using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B3XAdjustmentsDocPage))]
	sealed class B3XAdjustmentsDocPageTest : AdjustmentsDocPageTest
	{
		public override void TestSubHeaderWithNoLines()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b3x.B2AsClaimedForInvoices.AddNew();

			var docPage = new B3XAdjustmentsDocPage().GetPages(subHeader);
			AssertEquals(1, docPage.Count());
		}

		public override void TestGetPages()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			var tranastionNumber = b3x.TransactionNumber;
			tranastionNumber.AccountSecurityCode = "12345";
			tranastionNumber.SequentialNumber = "87654321";
			var subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var accountLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine.CA_OriginalLineNo = "1";
			var duty = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;

			var docPage = new B3XAdjustmentsDocPage().GetPages(subHeader.CorrespondingAsClaimedForInvoice);
			AssertEquals(2, docPage.Count());

			AssertEquals("12345876543219", docPage.ElementAt(0).TransactionNumber);
			AssertEquals(ZString.Empty, docPage.ElementAt(1).TransactionNumber);
		}

		public void TestFormattedVendor()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.Import;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;

			RefCountry country = Factory.New<RefCountry>();
			country.RN_Code = "AA";
			country.RN_AddressFormattingRule = "UCU";
			country.RN_Desc = "Country Name";

			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AAYYY";
			unloco.RL_RN_NKCountryCode = country.Code;

			var orgS = Factory.New<OrgHeader>();
			orgS.OH_RL_NKClosestPort = "AAYYY";
			var mainAddress = orgS.Addresses.AddNewMainAddress();
			mainAddress.OA_RL_NKRelatedPortCode = "AAYYY";

			var sellingPartyAddress = b3x.DocAddresses.AddNew();
			sellingPartyAddress.E2_AddressType = DocAddressTypes.Codes.SellingParty;
			sellingPartyAddress.OrganisationPK = orgS.PK;
			sellingPartyAddress.Organisation.OH_FullName = "SELLINGDOCPARTY";
			sellingPartyAddress.Organisation.MainAddress.OA_City = "CITY";
			sellingPartyAddress.Organisation.MainAddress.OA_PostCode = "3333";
			sellingPartyAddress.Organisation.MainAddress.OA_State = "NY";

			AssertEquals("VendorDocAddress", sellingPartyAddress.PK, b3x.VendorDocAddress.PK);

			var subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "NS1";
			subHeader.JZ_OH_Supplier = orgS.PK;
			var asAccountedLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine.CA_OriginalLineNo = "1";
			subHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			subHeader.JZ_RW_NKOriginState = "AL";
			subHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			subHeader.CA_USStateOfExport = "IL";
			subHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			subHeader.JZ_ValuationDateOverride = new ZDateTime(2012, 3, 15);
			subHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			subHeader.CA_TimeLimit = 3;
			subHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Week;
			subHeader.MarkAsNeedingValidation();

			var docPage = new B3XAdjustmentsDocPage().GetPages(subHeader).ElementAt(0);
			var adjPage = docPage as B3XAdjustmentsDocPage;
			AssertEquals("CompanyTruncatedName Address State City Postcode", (ZString)@"SELLINGDOCPARTY
CITY NY 3333 Country Name", adjPage.VendorFormatted);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_RL_NKClosestPort = "USYYY";
			var mainAddress2 = org2.Addresses.AddNewMainAddress();
			mainAddress2.OA_RL_NKRelatedPortCode = "USYYY";

			var sellingPartyAddress2 = b3x.DocAddresses.AddNew();
			sellingPartyAddress2.E2_AddressType = DocAddressTypes.Codes.SellingParty;
			sellingPartyAddress2.OrganisationPK = org2.PK;
			sellingPartyAddress2.Organisation.OH_FullName = "SELLINGDOCPARTY2";
			sellingPartyAddress2.Organisation.MainAddress.OA_City = "CITY";
			sellingPartyAddress2.Organisation.MainAddress.OA_PostCode = "55555";
			sellingPartyAddress2.Organisation.MainAddress.OA_State = "NY";
			b3x.DocAddresses.RemoveAndDeleteAll();
			subHeader.JZ_OH_Supplier = org2.PK;
			var docPage2 = new B3XAdjustmentsDocPage().GetPages(subHeader).ElementAt(0);
			var adjPage2 = docPage2 as B3XAdjustmentsDocPage;
			AssertEquals("CompanyTruncatedName Address State City Postcode", (ZString)@"SELLINGDOCPARTY2
NY 55555", adjPage2.VendorFormatted);
		}

		public override void TestAdjustmentsDocFirstPage()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "NS1";
			var asAccountedLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine.CA_OriginalLineNo = "1";
			subHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			subHeader.JZ_RW_NKOriginState = "AL";
			subHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			subHeader.CA_USStateOfExport = "IL";
			subHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			subHeader.JZ_ValuationDateOverride = new ZDateTime(2012, 3, 15);
			subHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			subHeader.CA_TimeLimit = 3;
			subHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Week;
			subHeader.MarkAsNeedingValidation();

			var docPage = new B3XAdjustmentsDocPage().GetPages(subHeader.CorrespondingAsClaimedForInvoice).ElementAt(0);
			AssertEquals("NS", docPage.SubHeaderNo);
			AssertEquals("UAL", docPage.CountryOfOrigin);
			AssertEquals("UIL", docPage.PlaceOfExport);
			AssertEquals(TariffTreatmentCodes.Codes.UnitedStates, docPage.TariffTreatment);
			AssertEquals("03", docPage.DirectShipmentMonth);
			AssertEquals("15", docPage.DirectShipmentDay);
			AssertEquals("2012", docPage.DirectShipmentYear);
			AssertEquals(Core.Constants.CurrencyCodes.Canada, docPage.CurrencyCode);
			AssertEquals("3", docPage.TimeLimit);
			AssertEquals(TimeLimitUnitCodes.Codes.Week, docPage.TimeCode);
		}

		public override void TestSortLinesNumerically()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var asAccountedLine1 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine1.CA_OriginalLineNo = "1";

			var asAccountedLine2 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine2.CA_OriginalLineNo = "10";

			var asAccountedLine3 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine3.CA_OriginalLineNo = "1/SL";

			var asAccountedLine4 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine4.CA_OriginalLineNo = "1.1";

			var asAccountedLine5 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine5.CA_OriginalLineNo = "2";

			var asAccountedLine6 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine6.CA_OriginalLineNo = "20";

			var tempPage = new B3XAdjustmentsDocPage();
			var docPage1 = tempPage.GetPages(subHeader.CorrespondingAsClaimedForInvoice).ElementAt(0);
			AssertEquals("1", docPage1.AsAccountForDocLine1.OriginalLineNo);
			AssertEquals("1/SL", docPage1.AsAccountForDocLine2.OriginalLineNo);
			var docPage2 = tempPage.GetPages(subHeader.CorrespondingAsClaimedForInvoice).ElementAt(1);
			AssertEquals("1.1", docPage2.AsAccountForDocLine1.OriginalLineNo);
			AssertEquals("2", docPage2.AsAccountForDocLine2.OriginalLineNo);
			var docPage3 = tempPage.GetPages(subHeader.CorrespondingAsClaimedForInvoice).ElementAt(2);
			AssertEquals("10", docPage3.AsAccountForDocLine1.OriginalLineNo);
			AssertEquals("20", docPage3.AsAccountForDocLine2.OriginalLineNo);
		}

		public override void TestSplitLineShouldBeAtTheBackOfEachGroup()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var asAccountedLine1 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedLine1.CA_OriginalLineNo = "1";

			var asClaimedSubHeader = subHeader.CorrespondingAsClaimedForInvoice;
			var splitLine = asClaimedSubHeader.AsClaimForFilteredInvoiceLines.AddNew();
			splitLine.CA_OriginalLineNo = "1";
			asClaimedSubHeader.AsClaimForFilteredInvoiceLines.Sort((IComparer)new CAInvoiceLineComparer(true, JobComInvoiceLine.Schema.CA_OriginalLineNo));

			var tempPage = new B3XAdjustmentsDocPage();
			var docPage1 = tempPage.GetPages(subHeader.CorrespondingAsClaimedForInvoice).ElementAt(0);
			AssertEquals("1", docPage1.AsAccountForDocLine1.OriginalLineNo);
			AssertEquals("1", docPage1.AsClaimForDocLine1.OriginalLineNo);
			AssertEquals("", docPage1.AsAccountForDocLine2.OriginalLineNo);
			AssertEquals("1/SL", docPage1.AsClaimForDocLine2.OriginalLineNo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var subHeader = b3x.Invoices.AddNew();
			subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			return new B3XAdjustmentsDocPage().GetPages(subHeader).ElementAt(0);
		}
	}
}
