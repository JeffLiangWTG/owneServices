using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(KREntryHeaderDetailsViewCollection))]
	sealed class KREntryHeaderDetailsViewCollectionTest : ActiveBusinessObjectCollectionTestCase<KREntryHeaderDetailsViewCollection>
	{
		public void TestDefaultFilterOfMessageForExport()
		{
			var declaration = GetIncludedOrgDeclaration(1);
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceLine1.JI_Tariff = "0101299000";
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceLine2.JI_Tariff = "8888710000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry1 = declaration.CustomsEntryHeaders[0];
			var entry2 = declaration.CustomsEntryHeaders[1];
			entry1.CH_Status = Messaging.CustomsMessageStatusTypeList.Codes.OriginalSent;
			entry2.CH_Status = Messaging.CustomsMessageStatusTypeList.Codes.OriginalRejected;

			Factory.Save();

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5ACAnd5GW();
			var collection = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Export, query, GlbCompany.CurrentCompany.PK);
			AssertEquals(1, collection.Count);
			AssertEquals(entry2.EntryNumber, collection[0].KEH_EntryNum);

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Name = "Test Company Name";
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var newCompanyCollection = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Export, query, newCompany.PK);
			AssertEquals(0, newCompanyCollection.Count);
		}

		public void TestDefaultFilterOfMessageForImportRejected()
		{
			var declaration = GetIncludedOrgDeclaration(1);
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var entryWithIssueDateAndExpiry1 = declaration.CustomsEntryHeaders.AddNew();
			entryWithIssueDateAndExpiry1.EntryNumber = "1001";
			entryWithIssueDateAndExpiry1.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms;

			var entryWithIssueDateAndExpiry2 = declaration.CustomsEntryHeaders.AddNew();
			entryWithIssueDateAndExpiry2.EntryNumber = "1002";
			entryWithIssueDateAndExpiry2.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;

			var entryWithExpiryOnly = declaration.CustomsEntryHeaders.AddNew();
			entryWithExpiryOnly.EntryNumber = "1003";
			entryWithExpiryOnly.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var entryEmptyDates = declaration.CustomsEntryHeaders.AddNew();
			entryEmptyDates.EntryNumber = "1004";
			entryEmptyDates.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var entryNum = entryWithIssueDateAndExpiry1.EntryNumbers.GetOrCreateCusEntryNum(KRJobMessageTypeList.Codes.Import);
			entryNum.CE_IssueDate = ZDateTime.Today;
			entryNum = entryWithIssueDateAndExpiry2.EntryNumbers.GetOrCreateCusEntryNum(KRJobMessageTypeList.Codes.Import);
			entryNum.CE_IssueDate = ZDateTime.Today;

			var entryNum934 = entryWithIssueDateAndExpiry1.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			entryNum934.CE_ExpiryDate = ZDateTime.Today;
			entryNum934 = entryWithIssueDateAndExpiry2.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			entryNum934.CE_ExpiryDate = ZDateTime.Today;
			entryNum934 = entryWithExpiryOnly.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			entryNum934.CE_ExpiryDate = ZDateTime.Today;

			Factory.Save();

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5ACAnd5GW();
			var collection = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK).OrderBy(x => x.KEH_EntryNum).ToArray();
			AssertEquals(2, collection.Length);
			AssertEquals(entryWithExpiryOnly.EntryNumber, collection[0].KEH_EntryNum);
			AssertEquals(entryEmptyDates.EntryNumber, collection[1].KEH_EntryNum);

			query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG(new List<ZString> { entryWithIssueDateAndExpiry1.EntryNumber });
			collection = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK).ToArray();
			AssertEquals(0, collection.Length);
		}

		public void TestAdditionalFilterOfMessage()
		{
			var declaration1 = GetIncludedOrgDeclaration(1);
			declaration1.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			declaration1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry1 = declaration1.CustomsEntryHeaders[0];
			entry1.CH_Status = Messaging.CustomsMessageStatusTypeList.Codes.OriginalSent;

			var declaration2 = GetIncludedOrgDeclaration(2);
			declaration2.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry2 = declaration2.CustomsEntryHeaders[0];
			entry2.CH_Status = Messaging.CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var declaration3 = GetIncludedOrgDeclaration(3);
			declaration3.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			declaration3.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry3 = declaration3.CustomsEntryHeaders[0];
			entry3.CH_Status = Messaging.CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal;

			Factory.Save();

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5ACAnd5GW();
			var collection = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Export, query, GlbCompany.CurrentCompany.PK);
			AssertEquals(1, collection.Count);
			AssertEquals(entry3.EntryNumber, collection[0].KEH_EntryNum);
		}
		JobDeclaration GetIncludedOrgDeclaration(int idx)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA" + (2 * idx - 1).ToString(), "모나리자(주)" + idx);
			declaration.JE_OH_DutyPayer = payer.PK;
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA" + (2 * idx).ToString(), "레디코리아" + idx);
			declaration.JE_OH_Supplier = supplier.PK;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			return declaration;
		}
		protected override KREntryHeaderDetailsViewCollection GetCollectionToTest() => new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Export, new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5ACAnd5GW(), GlbCompany.CurrentCompany.PK);
	}
}
