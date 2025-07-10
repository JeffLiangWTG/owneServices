using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(HSExtensionCode))]
	sealed class HSExtensionCodeTest : CusCodeDataTest<HSExtensionCode>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.HsExtensionCode, Factory.New<HSExtensionCode>().CY_Type);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => hsExtensionCode;

		protected override BusinessObject GetNewBusinessObject() => hsExtensionCode;

		public void TestCategoryAndClassificationType()
		{
			SetUpTariffData();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = "0301929090";
			var hsExtensionCode1 = CreateHsExtensionCode(0, "01", invoiceLine.PK, invoiceLine.TablePrefix);
			AssertEquals("Category", hsExtensionCode1.ClassificationType);
			AssertEquals("CAT", hsExtensionCode1.Category);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "0301929090";
			var hsExtensionCode2 = CreateHsExtensionCode(2, "2L", pivot.PK, pivot.TablePrefix);
			AssertEquals("Sub-Category 2", hsExtensionCode2.ClassificationType);
			AssertEquals("SCA", hsExtensionCode2.Category);
		}
			
		public void TestCategoryDescription()
		{
			SetUpTariffData();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = "0301929090";
			var hsExtensionCode = CreateHsExtensionCode(0, "01", invoiceLine.PK, invoiceLine.TablePrefix);
			AssertEquals("CAT description", hsExtensionCode.CategoryDescription);

			hsExtensionCode = CreateHsExtensionCode(2, "2L", invoiceLine.PK, invoiceLine.TablePrefix);
			AssertEquals("SCA description", hsExtensionCode.CategoryDescription);

			invoiceLine.JI_Tariff = "0105949000";
			hsExtensionCode = CreateHsExtensionCode(0, "03", invoiceLine.PK, invoiceLine.TablePrefix);
			AssertEquals(ZString.Empty, hsExtensionCode.CategoryDescription);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "0301929090";
			hsExtensionCode = CreateHsExtensionCode(0, "01", pivot.PK, pivot.TablePrefix);
			AssertEquals("CAT description", hsExtensionCode.CategoryDescription);

			hsExtensionCode = CreateHsExtensionCode(2, "2L", pivot.PK, pivot.TablePrefix);
			AssertEquals("SCA description", hsExtensionCode.CategoryDescription);

			invoiceLine.JI_Tariff = "0105949000";
			hsExtensionCode = CreateHsExtensionCode(0, "03", pivot.PK, pivot.TablePrefix);
			AssertEquals(ZString.Empty, hsExtensionCode.CategoryDescription);
		}
		void SetUpTariffData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0301929090", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tariffAdditionalCodeCAT = helper.CreateTariffAdditionalCodeView(tariff, "CAT", "01", "CAT description", "KR");
			var tariffAdditionalCodeSCA = helper.CreateTariffAdditionalCodeView(tariff, "SCA", "2L", "SCA description", "KR");

			var emptyTariff = helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0105949000", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		}

		public void TestCY_Code()
		{
			new TestDataSetupHelper(Factory).SetTariffAdditionalCodes();
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "0301929090";
			var mainAdditionalCodes = invoiceLine.UniversalTariff.AdditionalCodes.Where(x => x.ZY2_ZY3_NKCategory == HsExtensionCodes.Code.Category);
			Assert(mainAdditionalCodes.Any(x => x.ZY2_AdditionalCode == "01"));
			Assert(mainAdditionalCodes.Any(x => x.ZY2_AdditionalCode == "02"));
			Assert(mainAdditionalCodes.Any(x => x.ZY2_AdditionalCode == "03"));
			Assert(mainAdditionalCodes.Any(x => x.ZY2_AdditionalCode == "04"));
			var subAdditionalCodes = invoiceLine.UniversalTariff.AdditionalCodes.Where(x => x.ZY2_ZY3_NKCategory == HsExtensionCodes.Code.SubCategory);

			AssertEquals(1, invoiceLine.HSExtensionCodeCollection.Count);
			var mainHSExtensionCode = invoiceLine.HSExtensionCodeCollection.FirstOrDefault();
			AssertHSExtensionCode("01", new ZString[] { "1A", "1B", "2C", "2D", "2E" }, 3);
			AssertHSExtensionCode("02", new ZString[] { "1F", "1G", "2H", "2I", "2J" }, 3);
			AssertHSExtensionCode("03", new ZString[] { "1K", "1M", "2L", "2N", "2O" }, 3);
			AssertHSExtensionCode("04", new ZString[] { "1P", "1Q" }, 2);

			void AssertHSExtensionCode(string code, ZString[] subAdditionalCodeList, int totalCollectionCount)
			{
				mainHSExtensionCode.CY_Code = code;

				var selectSubAdditionalCodes = subAdditionalCodes.Where(x => x.ZY2_ParentAdditionalCode == code);
				AssertEquals(subAdditionalCodeList.Length, selectSubAdditionalCodes.Count());
				Assert(selectSubAdditionalCodes.Any(x => subAdditionalCodeList.Contains(x.ZY2_AdditionalCode)));

				AssertEquals(totalCollectionCount, invoiceLine.HSExtensionCodeCollection.Count);
				AssertEquals("CAT is default", totalCollectionCount - 1, invoiceLine.HSExtensionCodeCollection.Where(x => x.Category == "SCA").Count());
			}
		}

		public void TestCaption()
		{
			var hsExtensionCode = Factory.New<HSExtensionCode>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(hsExtensionCode.GetType(), "ClassificationType", true, attrib => attrib.Caption == "Classification Type");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(hsExtensionCode.GetType(), "CY_Code", true, attrib => attrib.Caption == "Category Code");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(hsExtensionCode.GetType(), "CategoryDescription", true, attrib => attrib.Caption == "Category Desc.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(hsExtensionCode.GetType(), "CategoryDescription", true, attrib => attrib.FullDescription == "Category Description");
		}

		HSExtensionCode CreateHsExtensionCode(short order, string code, ZGuid parentID, string parentTableCode)
		{
			var hsExtensionCode = Factory.New<HSExtensionCode>();
			hsExtensionCode.CY_Order = order;
			hsExtensionCode.CY_ParentID = parentID;
			hsExtensionCode.CY_ParentTableCode = parentTableCode;
			hsExtensionCode.CY_Code = code;
			return hsExtensionCode;
		}

		protected override void SetUp()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			hsExtensionCode = Factory.New<HSExtensionCode>();
			hsExtensionCode.CY_ParentID = invoiceLine.PK;
			hsExtensionCode.CY_ParentTableCode = invoiceLine.TablePrefix;
			Factory.Save();
		}
		HSExtensionCode hsExtensionCode;
	}
}
