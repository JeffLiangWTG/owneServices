using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.DE.NCTS.Business.Testing.NCTSConditionalFunctionalityTestHelper;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPreviousDocument))]
	sealed class NctsPreviousDocumentTest : CusSupportingInfoTest<NctsPreviousDocument>
	{
		public void TestCSI_Description_MaxLength_PreviousProcedure()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			var cargoDesc = bill.GoodsItems.AddNew();
			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;

			var previousProcedure1 = cargoDesc.PreviousProcedures.AddNew();
			AssertEquals("CSI_Procedure is 9DEZ", 100, previousProcedure1.CSI_DescriptionInfo.MaxLength);

			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			var previousProcedure2 = cargoDesc.PreviousProcedures.AddNew();
			AssertEquals("CSI_Procedure is 9DEY", 350, previousProcedure2.CSI_DescriptionInfo.MaxLength);

			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			var previousProcedure3 = cargoDesc.PreviousProcedures.AddNew();
			AssertEquals("Not 9DEZ or 9DEY", 26, previousProcedure3.CSI_DescriptionInfo.MaxLength);
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals(70, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber_MaxLength_DuringTransition()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals(35, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber_MaxLength_PreviousProcedure()
		{
			var previousProcedure = goodsItem.PreviousProcedures.AddNew();
			previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals("Not N337", 70, previousProcedure.CSI_ReferenceNumberInfo.MaxLength);

				previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
				previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
				AssertEquals("AWB", 44, previousProcedure.CSI_ReferenceNumberInfo.MaxLength);

				previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.ULD;
				AssertEquals("ULD", 44, previousProcedure.CSI_ReferenceNumberInfo.MaxLength);

				previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				AssertEquals("REG", 21, previousProcedure.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber2_MaxLength()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals(35, previousDocument.CSI_ReferenceNumber2Info.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber2_MaxLength_DuringTransition()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals(26, previousDocument.CSI_ReferenceNumber2Info.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber2_MaxLength_PreviousProcedure()
		{
			var previousProcedure = goodsItem.PreviousProcedures.AddNew();
			previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals(35, previousProcedure.CSI_ReferenceNumber2Info.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber_ReadOnly_PreviousProcedure()
		{
			var previousProcedure = goodsItem.PreviousProcedures.AddNew();
			previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			AssertEquals("CSI_ReferenceNumber is always enabled for Previous Procedures", false, previousProcedure.CSI_ReferenceNumberInfo.ReadOnly);
		}

		public void TestCSI_ReferenceNumber_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenAttributeIsMissing(previousDocument.CSI_ReferenceNumberInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.Reference);
		}

		public void TestCSI_AdditionalDescription_MaxLength()
		{
			AssertEquals(300, previousDocument.CSI_AdditionalDescriptionInfo.MaxLength);
		}

		public void TestCSI_ReferenceNumber2_ReadOnly()
		{
			AssertPropertyIsReadOnlyWhenAttributeIsMissing(previousDocument.CSI_ReferenceNumber2Info, DEReferenceConstants.RefCusCodeListAttributes.Name.Complement);
		}

		public void TestCSI_ReferenceNumber2_ReadOnly_PreviousProcedure()
		{
			var previousProcedure = goodsItem.PreviousProcedures.AddNew();
			previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			AssertEquals("CSI_ReferenceNumber2 is always enabled for Previous Procedures", false, previousProcedure.CSI_ReferenceNumber2Info.ReadOnly);
		}

		public void TestUsualProcessingFlag()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			previousDocument.UsualProcessingFlag = true;
			previousDocument.CSI_Quantity = 12.34m;
			previousDocument.CSI_UnitOfQuantity = "KG";

			CombineAssertions(() =>
			{
				AssertEquals("CSI_Quantity editable", false, previousDocument.CSI_QuantityInfo.ReadOnly);
				AssertEquals("CSI_UnitOfQuantity editable", false, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);

				previousDocument.UsualProcessingFlag = false;
				AssertEquals("CSI_Quantity 0", ZDecimal.Zero, previousDocument.CSI_Quantity);
				AssertEquals("CSI_Quantity read-only", true, previousDocument.CSI_QuantityInfo.ReadOnly);
				AssertEquals("CSI_UnitOfQuantity empty", ZString.Empty, previousDocument.CSI_UnitOfQuantity);
				AssertEquals("CSI_UnitOfQuantity read-only", true, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);
			});
		}

		public void TestAuthorizationNumber_MaxLength()
		{
			AssertEquals(35, previousDocument.AuthorizationNumberInfo.MaxLength);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Reference", previousDocument.HumanReadableName);
		}

		public void TestFormattedTariff()
		{
			CombineAssertions(() =>
			{
				previousDocument.FormattedTariff = "12345678901";
				AssertEquals("Set unformatted: FormattedTariff", "1234.56.78 901", previousDocument.FormattedTariff);
				AssertEquals("Set unformatted: CSI_Tariff", "12345678901", previousDocument.CSI_Tariff);

				previousDocument.FormattedTariff = "1234.56.78 901";
				AssertEquals("Set formatted: FormattedTariff", "1234.56.78 901", previousDocument.FormattedTariff);
				AssertEquals("Set formatted: CSI_Tariff", "12345678901", previousDocument.CSI_Tariff);
			});
		}

		public void TestITariffFormatProvider()
		{
			CombineAssertions(() =>
			{
				var provider = (ITariffFormatProvider)previousDocument;
				var tariffFormatter = provider.TariffFormatter;
				AssertType<EU.Business.TariffFormatterEleven>("Type", provider.TariffFormatter);
				AssertSame("Cached", tariffFormatter, provider.TariffFormatter);
			});
		}

		public void TestCSI_AdditionalDescription_Caption()
		{
			AssertEquals("Complement", DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_AdditionalDescriptionInfo).Caption);
		}

		public void TestStatus_Caption()
		{
			AssertEquals("Entry via ATLAS?", DataBoundResourceStrings.GetDataForProperty(previousDocument.StatusInfo).Caption);
		}

		public void TestFormattedTariff_Caption()
		{
			AssertEquals("Commodity Code", DataBoundResourceStrings.GetDataForProperty(previousDocument.FormattedTariffInfo).Caption);
		}

		public void TestUsualProcessingFlag_Caption()
		{
			AssertEquals("Usual Processing Flag", DataBoundResourceStrings.GetDataForProperty(previousDocument.UsualProcessingFlagInfo).Caption);
		}

		public void TestIsProcedure9DEY()
		{
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
				AssertEquals("CSI_Procedure = 9DEY", true, previousDocument.IsProcedure9DEY);

				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
				AssertEquals("CSI_Procedure <> 9DEY", false, previousDocument.IsProcedure9DEY);
			});
		}

		public void TestIsProcedure9DEZ()
		{
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
				AssertEquals("CSI_Procedure = 9DEZ", true, previousDocument.IsProcedure9DEZ);

				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
				AssertEquals("CSI_Procedure <> 9DEZ", false, previousDocument.IsProcedure9DEZ);
			});
		}

		public void TestProcedureN337()
		{
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
				AssertEquals("CSI_Procedure = N337", true, previousDocument.IsProcedureN337);

				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
				AssertEquals("CSI_Procedure <> N337", false, previousDocument.IsProcedureN337);
			});
		}

		public void TestCSI_Quantity_PreviousProcedure()
		{
			CombineAssertions(() =>
			{
				var previousProcedure = goodsItem.PreviousProcedures.AddNew();
				previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
				var tester = new DecimalPlacesAttributeTester(previousProcedure);
				var list = new List<string> { nameof(previousProcedure.CSI_Quantity) };
				tester.CheckConstant(list, nameof(previousProcedure.CSI_Quantity_Digit), 3);
				AssertEquals(19, previousProcedure.CSI_QuantityInfo.MaxLength);

				previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
				tester.CheckConstant(list, nameof(previousProcedure.CSI_Quantity_Digit), 0);
				AssertEquals(5, previousProcedure.CSI_QuantityInfo.MaxLength);
			});
		}

		public void TestCreateOrRemoveAdditionalDocumentInfForN830()
		{
			var previousDocument = goodsItem.PreviousDocuments.AddNew();
			AssertEquals(0, goodsItem.AdditionalInfos.Count);

			previousDocument.CSI_Code = "N830";
			AssertEquals(1, goodsItem.AdditionalInfos.Count);
			var infItem = goodsItem.AdditionalInfos.Single();
			AssertEquals("20300", infItem.CSI_Code);
			AssertEquals("INF", infItem.CSI_SubType);

			var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "N830";
			AssertEquals(1, goodsItem.AdditionalInfos.Count);

			previousDocument2.Delete();
			AssertEquals(1, goodsItem.AdditionalInfos.Count);

			previousDocument.CSI_Code = "N831";
			AssertEquals(0, goodsItem.AdditionalInfos.Count);

			previousDocument.CSI_Code = "N830";
			previousDocument.Delete();
			AssertEquals(0, goodsItem.AdditionalInfos.Count);
		}

		protected override IEnumerable<NctsPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var previousDocument = goodsItem.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "A";

			yield return previousDocument;
		}

		protected override BusinessObject GetNewBusinessObject() => previousDocument;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
		}
		NctsDepartureCargoDesc goodsItem;
		NctsPreviousDocument previousDocument;
		NctsHeader nctsHeader;

		void AssertPropertyIsReadOnlyWhenAttributeIsMissing(ZPropertyInfo propertyInfo, string attributeName)
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, Factory, attributeName);

			CombineAssertions(() =>
			{
				previousDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals($"TypeCode has attribute '{attributeName}' = 'Y'", false, propertyInfo.ReadOnly);

				previousDocument.CSI_Code = refCusCodeList2.ZZD_Code;
				AssertEquals($"TypeCode has attribute '{attributeName}' = 'N'", false, propertyInfo.ReadOnly);

				previousDocument.CSI_Code = refCusCodeList3.ZZD_Code;
				AssertEquals($"TypeCode doesn't have attribute '{attributeName}'", true, propertyInfo.ReadOnly);

				previousDocument.CSI_Code = ZString.Empty;
				AssertEquals("TypeCode is empty", true, propertyInfo.ReadOnly);

				previousDocument.CSI_Code = "AAAA";
				AssertEquals("TypeCode is not in the list", false, propertyInfo.ReadOnly);
			});
		}
	}
}
