using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobComInvoiceLine))]
	sealed class EMCSJobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public override void TestMakeCustomsQuantityReadOnly()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
				AssertEquals("Customs unit qty equal to KG and Qty field should be editable", false, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);

				invoiceLine.JI_CustomsUnitQty = ZString.Empty;
				AssertEquals("Customs unit qty empty Qty field should be readonly", true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			});
		}

		public void TestEMCSProvider()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var invoiceLine = declaration.InvoiceHeader.InvoiceLines.AddNew();
			var provider = invoiceLine.EMCSProvider;

			CombineAssertions(() =>
			{
				AssertNotNull(provider);
				AssertEquals("Enterprise.Customs.EU.EMCS.Business.EMCSProvider", provider.GetType().FullName);
			});
		}

		public void TestOutturn_Readonly()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			var invoice = declaration.InvoiceHeader;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals(true, invoiceLine1.Outturn.ReadOnly);

				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.SetReadOnlyIncludingChildren(true);
				AssertEquals(true, invoiceLine2.Outturn.ReadOnly);
			});
		}

		public void TestZG_DeclaredValue_ReadOnly()
		{
			AssertEquals(true, invoiceLine.ZG_DeclaredValueInfo.ReadOnly);
		}

		public void TestOutturnCreatedOnNewEMCSInvoiceLine()
		{
			var query = new ZQuery(CusOutturnSchema.C5_ParentID, invoiceLine.PK);
			AssertNotNull(Factory.LoadTop1<EMCSInvoiceLineCusOutturn>(query));
		}

		public void TestDelete()
		{
			var outturn = invoiceLine.Outturn;
			invoiceLine.Delete();

			AssertEquals(true, outturn.IsDeleted);
		}

		public void TestIsWine()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_ExciseProductCode = ZString.Empty;
				AssertEquals("Not Wine", false, invoiceLine.IsWine);

				invoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
				AssertEquals("Is Wine", true, invoiceLine.IsWine);
			});
		}

		public void TestZG_ExciseProductCode_Caption()
		{
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ExciseProductCodeInfo);
				AssertEquals("Caption", "Excise Product Code", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Excise Code", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Excise", resourceStringData.ShortCaption);
			});
		}

		public void TestJI_NetWeightUQ_ReadOnly()
		{
			AssertEquals(true, invoiceLine.JI_NetWeightUQInfo.ReadOnly);
		}

		public void TestZG_WineCategory_ReadOnly()
		{
			AssertWinePropertyReadOnly(invoiceLine.ZG_WineCategoryInfo);
		}

		public void TestZG_GrowingZone_ReadOnly()
		{
			AssertWinePropertyReadOnly(invoiceLine.ZG_GrowingZoneInfo);
		}

		public void TestZG_WineCountryOrigin_ReadOnly()
		{
			AssertWinePropertyReadOnly(invoiceLine.ZG_WineCountryOriginInfo);
		}

		public void TestJI_WineDetailsComments_ReadOnly()
		{
			AssertWinePropertyReadOnly(invoiceLine.JI_WineDetailsCommentsInfo);
		}

		public void TestJI_WineDetailsComments_MaxLength()
		{
			AssertEquals(350, invoiceLine.JI_WineDetailsCommentsInfo.MaxLength);
		}

		public void TestZG_MaturationPeriodOrAgeOfProducts_MaxLength()
		{
			AssertEquals(350, invoiceLine.ZG_MaturationPeriodOrAgeOfProductsInfo.MaxLength);
		}

		public void TestZG_MaturationPeriodOrAgeOfProducts_HumanReadableName()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_MaturationPeriodOrAgeOfProductsInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Maturation Period Or Age Of Products", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Maturation Period/Product Age", resourceStringData.MediumCaption);
			});
		}

		public void TestOperationCodeDataCollection_ReadOnly()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_ExciseProductCode = "E200";
				AssertEquals("ReadOnly", true, invoiceLine.OperationCodeDataCollection.ReadOnly);
				invoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
				AssertEquals("Read Write", false, invoiceLine.OperationCodeDataCollection.ReadOnly);
			});
		}

		public void TestCustomsUnitQtyDescription()
		{
			invoiceLine.JI_CustomsUnitQty = EMCSCustomsQuantityTypeList.Codes.KG;
			AssertEquals(EMCSCustomsQuantityTypeList.Descriptions.KG, invoiceLine.CustomsUnitQtyDescription);
		}

		public void TestGetConvertedStockUnit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "EMCS Pack Types");

			var cusCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "U1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Countable, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Countable, RefCusCodeListAttributeTypes.Codes.Countable);

			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var packType = Factory.NewWithValidTestData<CusRefPacks>();
			packType.RP_CommercialPack = Core.Constants.PkgUnit.Package;
			packType.RP_CustomsPack = "U1";
			packType.RP_CustomsCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			packType = Factory.NewWithValidTestData<CusRefPacks>();
			packType.RP_CommercialPack = Core.Constants.PkgUnit.Bag;
			packType.RP_CustomsPack = "U2";
			packType.RP_CustomsCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();

			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.InvoiceHeader;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var convertedUnit = invoiceLine.GetConvertedStockUnit(Core.Constants.PkgUnit.Package);

			CombineAssertions(() =>
			{
				AssertEquals("Should find it from the RefPackType.", "U1", convertedUnit);

				convertedUnit = invoiceLine.GetConvertedStockUnit(Core.Constants.PkgUnit.Bag);
				AssertEquals("The U2 is not in the list of InvoiceUQ.", string.Empty, convertedUnit);
			});
		}

		public void TestJI_WeightUQ()
		{
			AssertEquals(true, invoiceLine.JI_WeightUQInfo.ReadOnly);
		}

		public void TestTypeDeciderEuAndGb()
		{
			Assert("EMCSJobComInvoiceLine only load from EMCSJobDeclaration or EMCSJobComInvoiceHeader.", true);
		}

		public void TestSetDefaultTaxOrFeeCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes);
			var cusCodeList = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "AX", ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(nameof(ExciseProductCodeAttribute.RelTrf), "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCodeList.Attributes.AddNew(nameof(ExciseProductCodeAttribute.RelTrf), "20180802");

			Factory.Save();

			invoiceLine.ZG_ExciseProductCode = string.Empty;
			invoiceLine.JI_Tariff = "20180802";

			CombineAssertions(() =>
			{
				AssertEquals("Should get the first reference excise product code from database.", cusCodeList.ZZD_Code, invoiceLine.ZG_ExciseProductCode);

				invoiceLine.ZG_ExciseProductCode = string.Empty;
				invoiceLine.JI_Tariff = "20180803";

				AssertEquals("There is no matched reference excise product code for 20180803.", string.Empty, invoiceLine.ZG_ExciseProductCode);

				invoiceLine.ZG_ExciseProductCode = "TX";
				invoiceLine.JI_Tariff = "20180802";

				AssertEquals("Only update the value when ZG_ExciseProductCode is empty.", "TX", invoiceLine.ZG_ExciseProductCode);
			});
		}

		public void TestIsContainerLinkMandatory()
		{
			AssertEquals(false, invoiceLine.IsContainerLinkMandatory);
		}

		public void TestJI_NDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, "Excise Movement Control System (EMCS) CN-Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, "29024100", "o-Xylol", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, "29024101", ZString.Empty.PadRight(ZZRefCusCodeListCombinedSchema.ZZD_Description.MaxLength, '1'), ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Tariff = "29024100";
				AssertEquals("Valid Code", "o-Xylol", invoiceLine.JI_NDescription);

				invoiceLine.JI_Tariff = "12345678";
				AssertEquals("Invalid Code, old value isn't cleared", "o-Xylol", invoiceLine.JI_NDescription);

				invoiceLine.JI_Tariff = "29024101";
				AssertEquals("Valid Code and ZZD_Description exceeds maximum length", ZString.Empty.PadRight(JobComInvoiceLineSchema.JI_NDescription.MaxLength, '1'), invoiceLine.JI_NDescription);
			});
		}

		public void TestZG_ExciseProductCode_ClearAndDisableWineDetailsIfNeeded()
		{
			invoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
			invoiceLine.ZG_WineCategory = EMCSWineCategoryList.Codes.ImportedWine;
			invoiceLine.ZG_GrowingZone = EMCSGrowingZoneList.Codes.Cii;
			invoiceLine.ZG_WineCountryOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_WineDetailsComments = "NICE HUNTER VALLEY WHITE";
			invoiceLine.OperationCodeDataCollection.AddNew();

			invoiceLine.ZG_ExciseProductCode = "X200";
			CombineAssertions(() =>
			{
				AssertEquals("ZG_WineCategory", ZString.Empty, invoiceLine.ZG_WineCategory);
				AssertEquals("ZG_GrowingZone", ZString.Empty, invoiceLine.ZG_GrowingZone);
				AssertEquals("ZG_WineCountryOrigin", ZString.Empty, invoiceLine.ZG_WineCountryOrigin);
				AssertEquals("JI_WineDetailsComments", ZString.Empty, invoiceLine.JI_WineDetailsComments);
				AssertEquals("OperationCodeDataCollection Empty", 0, invoiceLine.OperationCodeDataCollection.Count);
				AssertEquals("OperationCodeDataCollection ReadOnly", true, invoiceLine.OperationCodeDataCollection.ReadOnly);
			});
		}

		public void TestZG_IsMainPack()
		{
			AssertEquals("Not Main Pack", false, invoiceLine.ZG_IsMainPack);
		}

		public void TestZG_IsMainPack_Caption()
		{
			AssertEquals("Is Main Pack?", DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_IsMainPackInfo).Caption);
		}

		public void TestNotUseUniversalTariffForValidatingJI_Tariff()
		{
			AssertEquals(false, invoiceLine.UseUniversalTariff);
		}

		public override void TestChargeTypeList()
		{
			ICommonInvoice commonInvoice = invoiceLine;

			CombineAssertions(() =>
			{
				AssertSame("cached", commonInvoice.ChargeTypeList, commonInvoice.ChargeTypeList);
				AssertEquals("ADD, COM, DED, DIS, EXW, FIF, LCH, OFT, ONS, OTH, PAC, STA", commonInvoice.ChargeTypeList.CodesAsString);
			});
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			CombineAssertions(() =>
			{
				AssertEquals("Empty UQ", true, invoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
				AssertEquals("Empty Qty", true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);

				invoiceLine.JI_CustomsUnitQty = EMCSCustomsQuantityTypeList.Codes.FifteenLitre;
				AssertEquals("Entered UQ from product", true, invoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
				AssertEquals("UQ allows Qty to be entered", false, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			return declaration.InvoiceHeader.JobComInvoiceLines.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => invoiceLine;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => invoiceLine;

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceLineChargeCollection<InvoiceLineCharge>);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			invoiceLine = declaration.InvoiceHeader.JobComInvoiceLines.AddNew();
		}
		EMCSJobDeclaration declaration;
		EMCSJobComInvoiceLine invoiceLine;

		void AssertWinePropertyReadOnly(ZPropertyInfo propertyInfo)
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_ExciseProductCode = "E200";
				AssertEquals("ReadOnly", true, propertyInfo.ReadOnly);
				invoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
				AssertEquals("Read Write", false, propertyInfo.ReadOnly);
			});
		}
	}
}
