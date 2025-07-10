using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSAddInfoJobComInvoiceLineValidationTest : EUEMCSAddInfoValidationTest
	{
		public void TestCheckZG_GrowingZone()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_GrowingZoneInfo, "X", "1");
		}

		public void TestCheckZG_WineCategory()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_WineCategoryInfo, "X", "1");

				invoiceLine.OperationCodeDataCollection.RemoveAndDeleteAll();

				invoiceLine.ZG_WineCategory = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateZG_WineCategory();
				AssertNoNotifications(invoiceLine.ZG_WineCategoryInfo);
			});
		}

		public void TestCheckZG_ExciseProductCode()
		{
			CreateExciseProductCode();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.ZG_ExciseProductCodeInfo, "XX", "AX");
		}

		public void TestCheckZG_ExciseProductCode_Tariff()
		{
			var message = "Excise Product Code must be S500 when tariff is 10000000.";

			invoiceLine.JI_Tariff = "10000000";
			invoiceLine.ZG_ExciseProductCode = string.Empty;

			CombineAssertions(() =>
			{
				invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
				AssertHasMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "S500";
				AssertNoMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);
			});
		}

		public void TestCheckZG_ExciseProductCode_ExciseProductCategoryIsE_WhenGuarantorTypeIs5()
		{
			var testDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var euGrouping = testDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "Desc.");
			testDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			testDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", euGrouping);

			testDataHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "Excise Product Codes");

			var cusCodeListDe = testDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "DE1", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "DE2", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var cusCodeListEu = testDataHelper.CreateCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "EU1", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testDataHelper.CreateCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "EU2", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			testDataHelper.CreateCusCodeListAttribute(cusCodeListDe.PK, "ExciseProductCategory", "E");
			testDataHelper.CreateCusCodeListAttribute(cusCodeListEu.PK, "ExciseProductCategory", "E");

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var message = "Excise Product Code must start with 'E' (Energy Products) when Guarantor Type is 5.";

				invoiceLine = GetEMCSInvoiceLine();
				invoiceLine.ZG_ExciseProductCode = ZString.Empty;

				var declaration = invoiceLine.Declaration;
				declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingMemberStateToEuMovements;

				invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
				AssertHasMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "DE1";
				AssertNoMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "DE2";
				AssertHasMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var message = "Excise Product Code must start with 'E' (Energy Products) when Guarantor Type is 5.";

				invoiceLine = GetEMCSInvoiceLine();
				invoiceLine.ZG_ExciseProductCode = ZString.Empty;

				var declaration = invoiceLine.Declaration;
				declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingMemberStateToEuMovements;

				invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
				AssertHasMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "EU1";
				AssertNoMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "EU2";
				AssertHasMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);
			}
		}

		public void TestCheckZG_ExciseProductCode_DestinationType_AndStartsWithE()
		{
			var testDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var euGrouping = testDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "Desc.");
			testDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			testDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", euGrouping);

			testDataHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "Excise Product Codes");

			var cusCodeListDe = testDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "DE1", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "DE2", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var cusCodeListEu = testDataHelper.CreateCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "EU1", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			testDataHelper.CreateCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "EU2", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			testDataHelper.CreateCusCodeListAttribute(cusCodeListDe.PK, "ExciseProductCategory", "E");
			testDataHelper.CreateCusCodeListAttribute(cusCodeListEu.PK, "ExciseProductCategory", "E");

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var message = "Excise Product Code must start with 'E' (Energy Products) when Destination Type is 8.";

				invoiceLine = GetEMCSInvoiceLine();
				invoiceLine.ZG_ExciseProductCode = "S500";

				var declaration = invoiceLine.Declaration;
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;

				invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
				AssertHasMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "DE1";
				AssertNoMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "DE2";
				AssertHasMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var message = "Excise Product Code must start with 'E' (Energy Products) when Destination Type is 8.";

				invoiceLine = GetEMCSInvoiceLine();
				invoiceLine.ZG_ExciseProductCode = "S500";

				var declaration = invoiceLine.Declaration;
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;

				invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
				AssertHasMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "EU1";
				AssertNoMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "EU2";
				AssertHasMessageError(invoiceLine.ZG_ExciseProductCodeInfo, message);
			}
		}

		public void TestCheckZG_ExciseProductCode_RelTrf()
		{
			const string message = "The Excise Product code AX is only mapped for these tariffs.\r\n20180801, 20180802";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes);

			var cusCodeList = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "AX", ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(nameof(ExciseProductCodeAttribute.RelTrf), "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCodeList.Attributes.AddNew(nameof(ExciseProductCodeAttribute.RelTrf), "20180802");
			cusCodeList.Attributes.AddNew(nameof(ExciseProductCodeAttribute.RelTrf), "20180801");

			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "BY", ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));

			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.Declaration.ZG_GuarantorType = "1";

				invoiceLine.ZG_ExciseProductCode = string.Empty;
				invoiceLine.JI_Tariff = string.Empty;

				invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
				AssertNoWarning("Excise Product code is Empty", invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "AX";
				invoiceLine.JI_Tariff = "20180803";

				invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
				AssertHasWarning("Excise Product code is AX, Tariff is unrelated", invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.JI_Tariff = "20180802";

				invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
				AssertNoWarning("Excise Product code is AX, Tariff is related", invoiceLine.ZG_ExciseProductCodeInfo, message);

				invoiceLine.ZG_ExciseProductCode = "BY";
				invoiceLine.JI_Tariff = "20180803";

				invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
				AssertNoWarning("Excise Product code doesn't need related tariff", invoiceLine.ZG_ExciseProductCodeInfo, message);
			});
		}

		public void TestCheckZG_Density()
		{
			AssertMandatoryIfAttributeExists(ExciseProductCodeAttribute.Density, invoiceLine.ZG_DensityInfo);
		}

		public void TestCheckZG_DegreePlato()
		{
			AssertMandatoryIfAttributeExists(ExciseProductCodeAttribute.DegreePlato, invoiceLine.ZG_DegreePlatoInfo);
		}

		public void TestCheckZG_AlcoholicStrength_Negative()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(invoiceLine.ZG_AlcoholicStrengthInfo);
		}

		public void TestCheckZG_AlcoholicStrength()
		{
			AssertMandatoryIfAttributeExists(ExciseProductCodeAttribute.AlcoholicStrength, invoiceLine.ZG_AlcoholicStrengthInfo);
		}

		public void TestCheckZG_AlcoholicStrength_Range()
		{
			CreateExciseProductCode(nameof(ExciseProductCodeAttribute.AlcoholicStrength));

			invoiceLine.ZG_ExciseProductCode = "AX";
			var info = invoiceLine.ZG_AlcoholicStrengthInfo;

			CombineAssertions(() =>
			{
				var message = "Must be greater than or equal to 0.5 and less than or equal to 100.";
				invoiceLine.ZG_AlcoholicStrength = 0.4m;
				AssertHasMessageError("ZG_AlcoholicStrength = 0.4m", info, message);

				invoiceLine.ZG_AlcoholicStrength = 101m;
				AssertHasMessageError("ZG_AlcoholicStrength = 101m", info, message);

				invoiceLine.ZG_AlcoholicStrength = 0.5m;
				AssertNoMessageError("ZG_AlcoholicStrength = 0.5m", info, message);

				invoiceLine.ZG_AlcoholicStrength = 20m;
				AssertNoMessageError("ZG_AlcoholicStrength = 20m", info, message);

				invoiceLine.ZG_AlcoholicStrength = 100m;
				AssertNoMessageError("ZG_AlcoholicStrength = 100m", info, message);

				invoiceLine.ZG_ExciseProductCode = "AA";
				invoiceLine.ZG_AlcoholicStrength = 0.4m;
				AssertNoMessageError("No attribute and ZG_AlcoholicStrength = 0.4m", info, message);
			});
		}

		public void TestCheckZG_WineCountryOrigin()
		{
			invoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
			invoiceLine.ZG_WineCategory = EMCSWineCategoryList.Codes.VarietalWineWithoutPdoPgi;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.ZG_WineCountryOriginInfo);
				invoiceLine.ZG_WineCategory = EMCSWineCategoryList.Codes.ImportedWine;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.ZG_WineCountryOriginInfo);
			});
		}

		void AssertMandatoryIfAttributeExists(ExciseProductCodeAttribute attribute, ZPropertyInfo propertyInfo)
		{
			CreateExciseProductCode(attribute.ToString());
			invoiceLine.ZG_ExciseProductCode = "AX";
			CombineAssertions(() =>
			{
				propertyInfo.Value = (ZDecimal)20m;
				AssertNoMessageErrorContaining("Value Entered", propertyInfo, MandatoryValidation.ValueCannotBeZero);

				propertyInfo.Value = ZDecimal.Zero;
				AssertHasMessageErrorContaining("Value Zero", propertyInfo, MandatoryValidation.ValueCannotBeZero);

				invoiceLine.ZG_ExciseProductCode = "BX";
				propertyInfo.Value = (ZDecimal)20m;
				propertyInfo.Value = ZDecimal.Zero;
				AssertNoMessageErrorContaining("Has attribute for EUN, but no attribute for specific country, no message error", propertyInfo, MandatoryValidation.ValueCannotBeZero);
			});
		}

		void CreateExciseProductCode(string attributeName = "")
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes);

			var cusCodeList = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "AX", new ZDateTime(1990, 01, 01), ZDateTime.Now.AddYears(1));
			var cusCodeList2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "BX", new ZDateTime(1990, 01, 01), ZDateTime.Now.AddYears(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "BX", new ZDateTime(1990, 01, 01), ZDateTime.Now.AddYears(1));

			if (!string.IsNullOrEmpty(attributeName))
			{
				helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				cusCodeList.Attributes.AddNew(attributeName, Customs.Business.YesNoList.Codes.Yes);
				cusCodeList2.Attributes.AddNew(attributeName, Customs.Business.YesNoList.Codes.Yes);
			}

			Factory.Save();
		}

		public void TestCheckZG_IsMainPack()
		{
			const string errorMessage = "is already marked as Main Pack for one or more selected Packages on this Line.";
			CombineAssertions(() =>
			{
				var declaration = Factory.New<EMCSJobDeclaration>();
				declaration.EMCSPackages.AddNew();
				var invoiceHeader = declaration.Invoices.AddNew();

				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				var packagePivot1 = invoiceLine1.EMCSPackagePivots[0];

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				var packagePivot2 = invoiceLine2.EMCSPackagePivots[0];
				packagePivot2.IsForInvoiceLine = true;
				invoiceLine2.ZG_IsMainPack = true;
				AssertNoMessageErrorContaining("invoiceLine2", invoiceLine2.ZG_IsMainPackInfo, errorMessage);

				packagePivot1.IsForInvoiceLine = true;
				invoiceLine1.AddInfoValidation.ValidateZG_IsMainPack();
				AssertNoMessageErrorContaining("invoiceLine1 ZG_IsMainPack=False", invoiceLine1.ZG_IsMainPackInfo, errorMessage);

				invoiceLine1.ZG_IsMainPack = true;
				AssertHasMessageErrorContaining("invoiceLine1 ZG_IsMainPack=True", invoiceLine1.ZG_IsMainPackInfo, errorMessage);
			});
		}

		public void TestJI_Tariff_EmptyPackaging()
		{
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");

			declaration.EMCSPackages.AddNew();

			var packagePivot1 = invoiceLine.EMCSPackagePivots[0];
			packagePivot1.IsForInvoiceLine = true;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			packagePivot1.IsForInvoiceLine = false;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
			invoiceLine = GetEMCSInvoiceLine();
		}
		EMCSJobDeclaration declaration;
		EMCSJobComInvoiceLine invoiceLine;

		EMCSJobComInvoiceLine GetEMCSInvoiceLine() => declaration.InvoiceHeader.InvoiceLines.AddNew();
	}
}
