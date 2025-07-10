using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExtendedHoursRequestLineCollection))]
	sealed class ExtendedHoursRequestLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExtendedHoursRequestLineCollection>
	{
		protected override ExtendedHoursRequestLineCollection GetCollectionToTest() => new ExtendedHoursRequestLineCollection(new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK));
		protected override BusinessObject GetNewElementToAddToTheCollection() => new ExtendedHoursRequestLine(new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK));

		public void TestAddNewData()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_IsReciprocal = true;
			Factory.Save();

			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), 0.85m, uSD);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Category = "BUS";
			orgHeader.OH_Code = "RK1";
			orgHeader.OH_FullName = "RK TestData1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OH_Supplier = orgHeader.PK;
			declaration.JE_OA_SupplierAddress = orgHeader.MainAddress.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "EXP";
			entryNum.CE_EntryNum = "1234522123451X";
			entryNum.CE_IssueDate = ZDateTime.Today;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 1000;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_NoOfPacks = 100;
			invoice.JZ_Weight = 100;
			invoice.JZ_WeightUQ = "KG";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();

			var view = Factory.LoadTop1<KREntryHeaderDetailsView>(new ZQuery(KREntryHeaderDetailsViewSchema.KEH_JE_PK, declaration.PK));
			var header = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var line = header.ExtendedHoursRequestLines.AddNewLine(view);
			AssertEquals("1234522123451X", line.ReferenceNumber);
			AssertEquals("1,000KRW / 0.85 USD Currency = 1,176.470588235294", 1176m, line.CustomsValue);
			AssertEquals(100, line.PackageCount);
			AssertEquals(100m, line.TotalWeight);
			AssertEquals("RK TestData1", line.SupplierName);
		}

		public void TestCustomsValueExport()
		{
			AssertCustomsValue(true);
		}

		public void TestCustomsValueImport()
		{
			AssertCustomsValue(false);
		}
		void AssertCustomsValue(bool isExport)
		{
			var messageType = isExport ? "EXP" : "IMP";
			var rateType = isExport ? Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary : Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_IsReciprocal = true;
			Factory.Save();

			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SetExchangeRate(GlbCompany.CurrentCompany, rateType, new ZDateTime(2022, 01, 01), new ZDateTime(2022, 01, 08), 0.85m, uSD);
			SetExchangeRate(GlbCompany.CurrentCompany, rateType, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), 0.95m, uSD);

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = messageType;
			declaration1.JE_ApplicationCode = "BLT";
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var entryNum1 = entry1.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = messageType;
			entryNum1.CE_EntryNum = "1234522123451X";
			entryNum1.CE_IssueDate = new ZDateTime(2022, 01, 03);
			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 1000;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = messageType;
			declaration2.JE_ApplicationCode = "BLT";
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = messageType;
			entryNum2.CE_EntryNum = "1234522123452X";
			entryNum2.CE_IssueDate = ZDateTime.Empty;
			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 1000;
			Factory.Save();

			var view1 = Factory.LoadTop1<KREntryHeaderDetailsView>(new ZQuery(KREntryHeaderDetailsViewSchema.KEH_JE_PK, declaration1.PK));
			var view2 = Factory.LoadTop1<KREntryHeaderDetailsView>(new ZQuery(KREntryHeaderDetailsViewSchema.KEH_JE_PK, declaration2.PK));
			var documentType = isExport ? ElectronicDocumentTypeList.Codes._5AC : ElectronicDocumentTypeList.Codes._5GW;
			var header = new ExtendedHoursRequestHeader(Factory, documentType, GlbCompany.CurrentCompany.PK);
			var line1 = header.ExtendedHoursRequestLines.AddNewLine(view1);
			var line2 = header.ExtendedHoursRequestLines.AddNewLine(view2);

			AssertEquals(1000m, view1.KEH_TotalCustomsValueInKRW);
			AssertEquals("If KEH_EntryNumIssueDate is not empty, the search is performed by used it. (1,000KRW / 0.85 USD Currency = 1,176.470588235294)", 1176m, line1.CustomsValue);

			AssertEquals(1000m, view2.KEH_TotalCustomsValueInKRW);
			AssertEquals("If KEH_EntryNumIssueDate is empty, the search is performed by the current date. (1,000KRW / 0.95 USD Currency = 1,052.631578947368)", 1053m, line2.CustomsValue);
		}

		RefExchangeRate SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = endDate;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			Factory.Save();

			return result;
		}
	}
}

