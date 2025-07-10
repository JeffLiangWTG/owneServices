using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(IM2AdjustmentsDocHeader))]
	sealed class IM2AdjustmentsDocumentHeaderTest : NonPersistentBusinessObjectTestCase
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestIM2AdjustmentsDocumentHeaderMembers()
		{
			var im2 = GetCopiedIM2();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTR";
			importer.OH_FullName = "IMPORTR";
			importer.OH_RL_NKClosestPort = "CAVAN";
			var address = importer.MainAddress;
			address.OA_Address1 = "ADDRESS1";
			address.OA_Address2 = "ADDRESS2";
			address.OA_City = "CANADACITY";
			address.OA_PostCode = "123";
			address.OA_State = "AB";
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "BizNo4ImEx", Core.Constants.CountryCodes.Canada);
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "GST", Core.Constants.CountryCodes.Canada);
			im2.JE_OH_Importer = importer.PK;
			im2.JE_OH_NotifyParty = importer.PK;

			im2.JE_CustomsOffice = "0019";
			im2.CA_OriginalTransactionNo = "10207000002752";
			im2.CA_K84AccountingDate = new ZDateTime(2012, 9, 13);
			im2.CA_SecurityNo = "SECURITY";
			im2.CA_AmendmentTo = AmendmentToList.Codes.OriginalB3;
			im2.CA_OriginalAccountingDate = new ZDateTime(2012, 9, 14);

			var header = new IM2AdjustmentsDocHeader(im2);
			AssertEquals(@"IMPORTR
ADDRESS1 ADDRESS2
CANADACITY AB 123
Canada", header.ImporterFormatted);
			AssertEquals("BizNo4ImEx", header.BusinessNumber);
			AssertEquals("GST", header.GSTNumber);
			AssertEquals("19", header.CBSAOffice);
			AssertEquals("10207000002752", header.OriginalTransactionNo);
			AssertEquals("09", header.Month);
			AssertEquals("14", header.Day);
			AssertEquals("2012", header.Year);
			AssertEquals("SECURITY", header.SecurityNo);

			AssertEquals(ZString.Empty, header.MailToFormatted);
			var mailTo = Factory.New<OrgHeader>();
			mailTo.OH_FullName = "MAILT";
			mailTo.OH_Code = "MAILT";
			mailTo.OH_RL_NKClosestPort = "USCHI";
			address = mailTo.MainAddress;
			address.OA_Address1 = "ADDRESS1";
			address.OA_Address2 = "ADDRESS2";
			address.OA_City = "NEWYORK";
			address.OA_PostCode = "123";
			address.OA_State = "AB";
			im2.JE_OH_NotifyParty = mailTo.PK;
			im2.CA_AmendmentTo = AmendmentToList.Codes.B2;
			header = new IM2AdjustmentsDocHeader(im2);
			AssertEquals(@"MAILT
ADDRESS1 ADDRESS2
NEWYORK AB 123
United States", header.MailToFormatted);
			AssertEquals("10207000002752", header.OriginalTransactionNo);
			im2.TransactionNumber.AccountSecurityCode = "12345";
			im2.TransactionNumber.SequentialNumber = "00006789";
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			Factory.Save();
			var newim2 = im2.GetNewCopyToB2Declaration();

			newim2.CA_AmendmentTo = AmendmentToList.Codes.OriginalB3;
			var newheader = new IM2AdjustmentsDocHeader(newim2);
			AssertEquals("10207000002752", newheader.OriginalTransactionNo);

			newim2.CA_AmendmentTo = AmendmentToList.Codes.B2;
			newheader = new IM2AdjustmentsDocHeader(newim2);
			AssertEquals("12345000067897", newheader.OriginalTransactionNo);
		}

		JobDeclaration GetCopiedIM2()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine.CA_TreatmentCode = "03";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			return im2;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var im2 = GetCopiedIM2();
			return new IM2AdjustmentsDocHeader(im2);
		}
	}
}
