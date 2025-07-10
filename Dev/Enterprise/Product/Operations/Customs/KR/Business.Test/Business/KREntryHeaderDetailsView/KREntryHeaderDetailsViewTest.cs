using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(KREntryHeaderDetailsView))]
	sealed class KREntryHeaderDetailsViewTest : EnterpriseBusinessObjectTestCase
	{
		// Not relevant for BizOs generated from views
		public override void TestSaveAndDeleteBusinessObject()
		{
		}

		public void TestFormattedEntryNum()
		{
			var krEntryHeaderDetailsView = Factory.New<KREntryHeaderDetailsView>();
			krEntryHeaderDetailsView.KEH_EntryNum = "XXXXXXXXXXXXXX";

			AssertEquals("XXXXX-XX-XXXXXXX", krEntryHeaderDetailsView.FormattedEntryNum);
		}

		public void TestCaptions()
		{
			var entryHeaderDetailsView = Factory.New<KREntryHeaderDetailsView>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "FormattedEntryNum", true, attrib => attrib.Caption == "Entry Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_ExportPackQty", true, attrib => attrib.Caption == "Total Packages");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_TotalCustomsValueInKRW", true, attrib => attrib.Caption == "Customs Value (KRW)");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_TotalWeightInKG", true, attrib => attrib.Caption == "Total Gross Weight (KG)");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_OA_SupplierAddress", true, attrib => attrib.Caption == "Supplier");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_SupplierName", true, attrib => attrib.Caption == "Supplier Company Name");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_EntryNumIssueDate", true, attrib => attrib.Caption == "Declaration Date (Local)");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_EntryCreatedLocalTime", true, attrib => attrib.Caption == "Entry Created Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "EntryNumIssueDateFor5SG", true, attrib => attrib.Caption == "Accepted Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_EntryReleaseDate", true, attrib => attrib.Caption == "Cleared Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_OH_Payer", true, attrib => attrib.Caption == "Payer");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_PayerName", true, attrib => attrib.Caption == "Payer Company Name");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_TotalPaid", true, attrib => attrib.Caption == "Total Payable Amount");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_EstimatedDateOfFinalPrice", true, attrib => attrib.Caption == "Estimated Date Of Final Price");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_ContractExpirationDate", true, attrib => attrib.Caption == "Contact Expiration Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_ProvAdditionalRate", true, attrib => attrib.Caption == "Provisional Additional Rate");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_TotalProvAdditionalAmount", true, attrib => attrib.Caption == "Provisional Additional Amount");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_OH_Importer", true, attrib => attrib.Caption == "Importer");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_ImporterName", true, attrib => attrib.Caption == "Importer Company Name");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "Supplier", true, attrib => attrib.Caption == "Supplier");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_BillNum", true, attrib => attrib.Caption == "House Bill");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_CargoManagementNumber", true, attrib => attrib.Caption == "Cargo Management No.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_HSDescription", true, attrib => attrib.Caption == "Tariff Description");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_ImportPackQty", true, attrib => attrib.Caption == "Total Packages");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(entryHeaderDetailsView.GetType(), "KEH_BondedAreaCode", true, attrib => attrib.Caption == "Bonded Area Code");
		}

		[TestedType(typeof(KREntryHeaderDetailsView.Loader))]
		class Test : LoaderTestCase
		{
			public void TestDefaultFilterOfMessageForImportAccepted()
			{
				var declaration = GetIncludedOrgDeclaration(1);
				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
				var entryWithIssueDateAndExpiry = declaration.CustomsEntryHeaders.AddNew();
				entryWithIssueDateAndExpiry.EntryNumber = "1001";
				entryWithIssueDateAndExpiry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

				var entryWithIssueDateOnly = declaration.CustomsEntryHeaders.AddNew();
				entryWithIssueDateOnly.EntryNumber = "1002";
				entryWithIssueDateOnly.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

				var entryWithExpiryOnly = declaration.CustomsEntryHeaders.AddNew();
				entryWithExpiryOnly.EntryNumber = "1003";
				entryWithExpiryOnly.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

				var entryEmptyDates = declaration.CustomsEntryHeaders.AddNew();
				entryEmptyDates.EntryNumber = "1004";
				entryEmptyDates.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

				var entryNum = entryWithIssueDateAndExpiry.EntryNumbers.GetOrCreateCusEntryNum(KRJobMessageTypeList.Codes.Import);
				entryNum.CE_IssueDate = ZDateTime.Today;
				entryNum = entryWithIssueDateOnly.EntryNumbers.GetOrCreateCusEntryNum(KRJobMessageTypeList.Codes.Import);
				entryNum.CE_IssueDate = ZDateTime.Today;

				var entryNum934 = entryWithIssueDateAndExpiry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
				entryNum934.CE_ExpiryDate = ZDateTime.Today;
				entryNum934 = entryWithExpiryOnly.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
				entryNum934.CE_ExpiryDate = ZDateTime.Today;
				Factory.Save();

				var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5ACAnd5GW();
				var collection = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
				AssertEquals(0, collection.Count);

				query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG(new List<ZString>() { entryWithIssueDateAndExpiry.EntryNumber });
				collection = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
				AssertEquals(1, collection.Count);
				AssertEquals(entryWithIssueDateAndExpiry.EntryNumber, collection[0].KEH_EntryNum);
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

			protected override BusinessObject.Loader GetNewLoaderToTest()
			{
				return new KREntryHeaderDetailsView.Loader(Factory);
			}
		}

		public void TestPayer()
		{
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "한국아이비엠(주)");
			TestOrgDataSetUpHelper.AddOrgContact(payer, "송기홍", true);
			TestOrgDataSetUpHelper.AddOrgAddress(payer.MainAddress, "서울특별시 영등포구 국제금융로 10", "(여의도동, 서울 국제금융 센터)");
			payer.MainAddress.OA_Phone = "02-123-4567";
			Factory.Save();

			var krEntryHeaderDetailsView = Factory.New<KREntryHeaderDetailsView>();
			krEntryHeaderDetailsView.KEH_OH_Payer = payer.PK;

			AssertEquals("한국아이비엠(주)", krEntryHeaderDetailsView.Payer.CompanyName);
			AssertEquals("송기홍", krEntryHeaderDetailsView.Payer.RepresentativeName);
			AssertEquals("서울특별시 영등포구 국제금융로 10", krEntryHeaderDetailsView.Payer.AddressLine1);
			AssertEquals("(여의도동, 서울 국제금융 센터)", krEntryHeaderDetailsView.Payer.AddressLine2);
			AssertEquals("02-123-4567", krEntryHeaderDetailsView.Payer.PhoneNumber);
		}
	}
}
