using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsPreviousDocument))]
public sealed partial class NctsPreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<NctsPreviousDocument>
{
	public void TestLookupsType()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertType<NctsPreviousDocumentLookups>("When Phase4, Lookups", previousDocument.Lookups);

		nctsHeader.BH_ApplicationCode = "NC5";
		previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertType<NctsPreviousDocumentPhase5Lookups>("When Phase5, Lookups", previousDocument.Lookups);
	}

	public void TestValidationType()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertType<NctsPreviousDocumentValidation>("When Phase4, Validation", previousDocument.Validation);

			nctsHeader.BH_ApplicationCode = "NC5";
			previousDocument = goodsItem.PreviousDocuments.AddNew();
			AssertType<NctsPreviousDocumentPhase5Validation>("When Phase5, Validation", previousDocument.Validation);
		});
	}

	public void TestSetDefaultValues()
	{
		AssertEquals(Core.Constants.Weight.Kilograms, previousDocument.CSI_UnitOfQuantity);
		AssertEquals(Core.Constants.Weight.Kilograms, previousDocument.CSI_UnitOfQuantity3);
	}

	public void TestCSIProcedure()
	{
		PreviousDocumentHelperTest.TestSetDefaultAndEmptyField(previousDocument);
	}

	public void TestReadOnlyFields()
	{
		PreviousDocumentHelperTest.TestReadOnlyFields(previousDocument);
	}

	public void TestIsTemporaryStorage()
	{
		previousDocument.CSI_Procedure = "";
		AssertEquals("When Procedure is empty, IsTemporaryStorage must return false", false, previousDocument.IsTemporaryStorage);
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaA3;
		AssertEquals("IsTemporaryStorage is true when Procedure = A3", true, previousDocument.IsTemporaryStorage);
	}

	public void TestIsTypeN337()
	{
		previousDocument.CSI_Code = "";
		AssertEquals("When Code <> N337, IsTypeN337 must return false", false, previousDocument.IsTypeN337);
		previousDocument.CSI_Code = "N337";
		AssertEquals("When Code = N337, IsTypeN337 must return true", true, previousDocument.IsTypeN337);
	}

	public void IsIntoWarehouse()
	{
		previousDocument.CSI_Procedure = "";
		AssertEquals("When Procedure is empty, IsIntoWarehouse must return false", false, previousDocument.IsIntoWarehouse);
		previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDeposito;
		AssertEquals("IsTemporaryStorage is true when Procedure = 7", true, previousDocument.IsIntoWarehouse);
	}

	public void TestIMergedPreviousDocumentMembers()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_SubType = "Z";
		previousDocument.CSI_Code = "ZZZ";
		previousDocument.CSI_ReferenceNumber2 = "A";
		previousDocument.CSI_ReferenceNumber = "1A";
		previousDocument.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
		previousDocument.CSI_Status = "S";
		previousDocument.CSI_CustomsOffice = "IT137100";
		previousDocument.CSI_LineNo = 0;
		previousDocument.CSI_PackQty = 20;
		previousDocument.CSI_Quantity3 = 220.82m;
		previousDocument.CSI_Quantity = 300.20m;
		previousDocument.CSI_Quantity2 = 52.43m;
		previousDocument.CSI_Tariff = "400321";

		CombineAssertions(() =>
		{
			var mergedDocument = (IMergedPreviousDocument)previousDocument;
			AssertEquals(nameof(mergedDocument.Type), "Z", mergedDocument.Type);
			AssertEquals(nameof(mergedDocument.Category), "ZZZ", mergedDocument.Category);
			AssertEquals(nameof(mergedDocument.Mrn), "A", mergedDocument.Mrn);
			AssertEquals(nameof(mergedDocument.Register), "A3", mergedDocument.Register);
			AssertEquals(nameof(mergedDocument.ReferenceNumber), "1", mergedDocument.ReferenceNumber);
			AssertEquals(nameof(mergedDocument.ReferenceNumberCin), "A", mergedDocument.ReferenceNumberCin);
			AssertEquals(nameof(mergedDocument.Date), new ZDate(2020, 01, 01), mergedDocument.Date);
			AssertEquals(nameof(mergedDocument.Series), "S", mergedDocument.Series);
			AssertEquals(nameof(mergedDocument.CustomsOffice), "IT137100", mergedDocument.CustomsOffice);
			AssertEquals(nameof(mergedDocument.ItemNumber), (ZShort)0, mergedDocument.ItemNumber);
			AssertEquals(nameof(mergedDocument.IsSummaryDeclarationDocument), true, mergedDocument.IsSummaryDeclarationDocument);
			AssertEquals(nameof(mergedDocument.IsPreviousProcedureDocument), false, mergedDocument.IsPreviousProcedureDocument);
			AssertEquals(nameof(mergedDocument.PackageQuantity), 20, mergedDocument.PackageQuantity);
			AssertEquals(nameof(mergedDocument.GrossMass), 220.82m, mergedDocument.GrossMass);
			AssertEquals(nameof(mergedDocument.NetMass), 300.20m, mergedDocument.NetMass);
			AssertEquals(nameof(mergedDocument.SupplementaryQuantity), 52.43m, mergedDocument.SupplementaryQuantity);
			AssertEquals(nameof(mergedDocument.Tariff), "400321", mergedDocument.Tariff);
		});
	}

	public void TestIMergedPreviousDocumentQuantityMembers()
	{
		var previousDocument = Factory.New<NctsPreviousDocument>();
		previousDocument.CSI_Quantity3 = 220m;
		previousDocument.CSI_UnitOfQuantity3 = "G";
		previousDocument.CSI_Quantity = 0.20m;
		previousDocument.CSI_UnitOfQuantity = "T";
		previousDocument.CSI_Quantity2 = 52.43m;
		previousDocument.CSI_UnitOfQuantity2 = "KG";

		CombineAssertions(() =>
		{
			var mergedDocument = (IMergedPreviousDocument)previousDocument;
			AssertEquals(nameof(mergedDocument.GrossMass), 0.220m, mergedDocument.GrossMass);
			AssertEquals(nameof(mergedDocument.NetMass), 200m, mergedDocument.NetMass);
			AssertEquals(nameof(mergedDocument.SupplementaryQuantity), 52.43m, mergedDocument.SupplementaryQuantity);
		});
	}

	[ExpectNoExceptions]
	public void TestSetCSI_ProcedureTriggersCSI_LineNoValidation()
	{
		var mockNctsPreviousDocument = Factory.NewMoq<NctsPreviousDocument>();
		var mockNctsPreviousDocumentValidation = new Mock<NctsPreviousDocumentValidation>(mockNctsPreviousDocument.Object);
		mockNctsPreviousDocument.Protected()
			.Setup<Customs.Business.CusSupportingInfoValidation>("GetNewPhase4Validation")
			.Returns(mockNctsPreviousDocumentValidation.Object);
		mockNctsPreviousDocumentValidation.Protected().Setup("CheckCSI_LineNo");
		mockNctsPreviousDocument.Object.CSI_Procedure = "2";
		mockNctsPreviousDocument.Verify();
		mockNctsPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_ProcedureTriggersCSI_TariffValidation()
	{
		var mockNctsPreviousDocument = Factory.NewMoq<NctsPreviousDocument>();
		var mockNctsPreviousDocumentValidation = new Mock<NctsPreviousDocumentValidation>(mockNctsPreviousDocument.Object);
		mockNctsPreviousDocument.Protected()
			.Setup<Customs.Business.CusSupportingInfoValidation>("GetNewPhase4Validation")
			.Returns(mockNctsPreviousDocumentValidation.Object);
		mockNctsPreviousDocumentValidation.Protected().Setup("CheckCSI_Tariff");
		mockNctsPreviousDocument.Object.CSI_Procedure = "2";
		mockNctsPreviousDocument.Verify();
		mockNctsPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_TariffTriggersCSI_LineNoValidation()
	{
		var mockNctsPreviousDocument = Factory.NewMoq<NctsPreviousDocument>();
		var mockNctsPreviousDocumentValidation = new Mock<NctsPreviousDocumentValidation>(mockNctsPreviousDocument.Object);
		mockNctsPreviousDocument.Protected()
			.Setup<Customs.Business.CusSupportingInfoValidation>("GetNewPhase4Validation")
			.Returns(mockNctsPreviousDocumentValidation.Object);
		mockNctsPreviousDocumentValidation.Protected().Setup("CheckCSI_LineNo");
		mockNctsPreviousDocument.Object.CSI_Tariff = "1234.5678";
		mockNctsPreviousDocument.Verify();
		mockNctsPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_LineNoTriggersCSI_TariffValidation()
	{
		var mockNctsPreviousDocument = Factory.NewMoq<NctsPreviousDocument>();
		var mockNctsPreviousDocumentValidation = new Mock<NctsPreviousDocumentValidation>(mockNctsPreviousDocument.Object);
		mockNctsPreviousDocument.Protected()
			.Setup<Customs.Business.CusSupportingInfoValidation>("GetNewPhase4Validation")
			.Returns(mockNctsPreviousDocumentValidation.Object);
		mockNctsPreviousDocumentValidation.Protected().Setup("CheckCSI_Tariff");
		mockNctsPreviousDocument.Object.CSI_LineNo = 123;
		mockNctsPreviousDocument.Verify();
		mockNctsPreviousDocumentValidation.VerifyAll();
	}

	public void TestIsSummaryDeclarationDocument()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_Procedure = "";
			AssertEquals("Empty is not a summary declaration document", false, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "2";
			AssertEquals("2 is not a summary declaration document", false, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "A3";
			AssertEquals("A3 is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "PF";
			AssertEquals("PF is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "MRN";
			AssertEquals("MRN is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "NN";
			AssertEquals("NN is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "CIM";
			AssertEquals("CIM is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "A44";
			AssertEquals("A44 is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
		});
	}

	public void TestIsPreviousProcedureDocument()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_Procedure = "";
			AssertEquals("Empty is not a previous procedure document", false, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "A3";
			AssertEquals("A3 is not a previous procedure document", false, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "2";
			AssertEquals("2 is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "2S";
			AssertEquals("2S is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "2T";
			AssertEquals("2T is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "5";
			AssertEquals("5 is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "5S";
			AssertEquals("5S is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "5T";
			AssertEquals("5T is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "7";
			AssertEquals("7 is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "7S";
			AssertEquals("7S is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "7T";
			AssertEquals("7T is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
		});
	}

	public void TestAsIPreviousDocumentReferenceNumberProvider()
	{
		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_ReferenceNumber = "12345X";

		CombineAssertions(() =>
		{
			var referenceNumberProvider = ((IPreviousDocumentReferenceNumberProvider)previousDocument).ReferenceNumberProvider;
			AssertEquals("Procedure", "A3", referenceNumberProvider.Procedure);
			AssertEquals("ReferenceNumber", "12345X", referenceNumberProvider.ReferenceNumber);
			AssertEquals("ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, referenceNumberProvider.ReferenceNumberInfo);
			AssertEquals("ReferenceNumberCin", "X", referenceNumberProvider.ReferenceNumberCin);
			AssertEquals("ReferenceNumberWithoutCin", "12345", referenceNumberProvider.ReferenceNumberWithoutCin);
		});
	}

	[ExpectNoExceptions]
	public void TestSetCSI_ProcedureTriggersPackageQuantityValidation()
	{
		var mockNctsPreviousDocument = Factory.NewMoq<NctsPreviousDocument>();
		var mockNctsPreviousDocumentValidation = new Mock<NctsPreviousDocumentValidation>(mockNctsPreviousDocument.Object);
		mockNctsPreviousDocument.Protected()
			.Setup<Customs.Business.CusSupportingInfoValidation>("GetNewPhase4Validation")
			.Returns(mockNctsPreviousDocumentValidation.Object);
		mockNctsPreviousDocumentValidation.Protected().Setup("CheckCSI_PackQty");
		mockNctsPreviousDocumentValidation.Protected().Setup("CheckCSI_Quantity3");
		mockNctsPreviousDocument.Object.CSI_Procedure = "A3";
		mockNctsPreviousDocument.Verify();
		mockNctsPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_ProcedureNoTriggersWithSuspendValidation()
	{
		var mockNctsPreviousDocument = Factory.NewMoq<NctsPreviousDocument>();
		var mockNctsPreviousDocumentValidation = new Mock<NctsPreviousDocumentValidation>(mockNctsPreviousDocument.Object);
		mockNctsPreviousDocument.Protected()
			.Setup<Customs.Business.CusSupportingInfoValidation>("GetNewPhase4Validation")
			.Returns(mockNctsPreviousDocumentValidation.Object);
		using (mockNctsPreviousDocument.Object.GetValidationSuspender())
		{
			mockNctsPreviousDocument.Object.CSI_Procedure = "A3";
			mockNctsPreviousDocumentValidation.Verify();
			mockNctsPreviousDocumentValidation.Protected().Verify("CheckCSI_PackQty", Times.Never());
			mockNctsPreviousDocumentValidation.Protected().Verify("CheckCSI_Quantity3", Times.Never());
		}
	}

	[ExpectNoExceptions]
	public void TestSetPackageQuantityTriggersPackageQuantityValidation()
	{
		var mockNctsPreviousDocument = Factory.NewMoq<NctsPreviousDocument>();
		var mockNctsPreviousDocumentValidation = new Mock<NctsPreviousDocumentValidation>(mockNctsPreviousDocument.Object);
		mockNctsPreviousDocument.Protected()
			.Setup<Customs.Business.CusSupportingInfoValidation>("GetNewPhase4Validation")
			.Returns(mockNctsPreviousDocumentValidation.Object);
		mockNctsPreviousDocumentValidation.Protected().Setup("CheckCSI_PackQty");
		mockNctsPreviousDocument.Object.CSI_PackQty = 1;
		mockNctsPreviousDocument.Verify();
		mockNctsPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetPackageQuantityNoTriggersWithSuspendValidation()
	{
		var mockNctsPreviousDocument = Factory.NewMoq<NctsPreviousDocument>();
		var mockNctsPreviousDocumentValidation = new Mock<NctsPreviousDocumentValidation>(mockNctsPreviousDocument.Object);
		mockNctsPreviousDocument.Protected()
			.Setup<Customs.Business.CusSupportingInfoValidation>("GetNewPhase4Validation")
			.Returns(mockNctsPreviousDocumentValidation.Object);
		using (mockNctsPreviousDocument.Object.GetValidationSuspender())
		{
			mockNctsPreviousDocument.Object.CSI_PackQty = 1;
			mockNctsPreviousDocumentValidation.Verify();
			mockNctsPreviousDocumentValidation.Protected().Verify("CheckCSI_PackQty", Times.Never());
		}
	}

	public void TestAsIPreviousDocumentUniversalTariffProvider()
	{
		previousDocument.CSI_Tariff = "6402121000";
		previousDocument.CSI_Procedure = "A2";
		CombineAssertions(() =>
		{
			var previousDocumentUniversalTariffProvider = (IPreviousDocumentUniversalTariffProvider)previousDocument;
			AssertEquals("TariffCode", "6402121000", previousDocumentUniversalTariffProvider.TariffCode);
			AssertEquals("FormattedTariff", "6402.12.10 00", previousDocumentUniversalTariffProvider.FormattedTariff);
			AssertEquals("UniversalTariffType", Universal.Constants.TariffTypes.Import, previousDocumentUniversalTariffProvider.UniversalTariffType);
			AssertEquals("Quantity2Info", previousDocument.CSI_Quantity2Info, previousDocumentUniversalTariffProvider.Quantity2Info);
			AssertEquals("Procedure", "A2", previousDocumentUniversalTariffProvider.Procedure);
		});
	}

	public void TestCSI_CodeMaxLength()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertEquals("When Phase4, CSI_Code MaxLength", 3, previousDocument.CSI_CodeInfo.MaxLength);

		nctsHeader.BH_ApplicationCode = "NC5";
		previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertEquals("When Phase5, CSI_Code MaxLength", 4, previousDocument.CSI_CodeInfo.MaxLength);
	}

	public void TestCSI_CodeCaption_Phase5Caption()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_CodeInfo.PropertyDescriptor, NctsHeader.Phase5CaptionKey);
		AssertEquals("Caption", "Type", resourceStringData.Caption);
	}

	public void TestCSI_UnitOfQuantityReadOnly()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertEquals("When Phase4, CSI_UnitOfQuantity Read Only", expected: true, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);

		nctsHeader.BH_ApplicationCode = "NC5";
		previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertEquals("When Phase5, CSI_UnitOfQuantity Read Only", expected: false, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);
	}

	public void TestCSI_QuantityDecimalPlaces()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertHasDecimalPlacesAttribute(previousDocument.CSI_QuantityInfo, 3);

		nctsHeader.BH_ApplicationCode = "NC5";
		var phase5PreviousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertHasDecimalPlacesAttribute("When NCTS Phase 5", phase5PreviousDocument.CSI_QuantityInfo, 6);
	}

	public void TestDefault_CSIUnitOfQuantity()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertEquals("When Phase4, CSI_UnitOfQuantity Default Value", "KG", previousDocument.CSI_UnitOfQuantity);

		nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertEquals("When Phase5, CSI_UnitOfQuantity Default Value", "", previousDocument.CSI_UnitOfQuantity);
	}

	public void TestCSI_QuantityCaption_Phase4Caption()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_QuantityInfo.PropertyDescriptor, NctsHeader.Phase4CaptionKey);

		CombineAssertions("When NCTS4", () =>
		{
			AssertEquals("Caption", "Net Mass", resourceStringData.Caption);
			AssertEquals("Medium Caption", "Net Mass", resourceStringData.MediumCaption);
			AssertEquals("Short Caption", "Net Mass", resourceStringData.ShortCaption);
		});	
	}

	public void TestCSI_QuantityCaption_Phase5Caption()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_QuantityInfo.PropertyDescriptor, NctsHeader.Phase5CaptionKey);

		AssertEquals("Caption", "Quantity", resourceStringData.Caption);
	}

	public void TestCSI_UnitOfQuantity_Phase4Caption()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_UnitOfQuantityInfo.PropertyDescriptor, NctsHeader.Phase4CaptionKey);

		CombineAssertions("When NCTS4", () =>
		{
			AssertEquals("Caption", "Net Mass UQ", resourceStringData.Caption);
			AssertEquals("Medium Caption", "Net Mass Unit", resourceStringData.MediumCaption);
			AssertEquals("Short Caption", "Net Mass UQ", resourceStringData.ShortCaption);
		});
	}

	public void TestCSI_UnitOfQuantity_Phase5Caption()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_UnitOfQuantityInfo.PropertyDescriptor, NctsHeader.Phase5CaptionKey);

		AssertEquals("Caption", "UOM", resourceStringData.Caption);
	}

	public void TestCSI_Reference2MaxLength()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertEquals("When Phase4, CSI_Reference2 MaxLength", 100, previousDocument.CSI_ReferenceNumber2Info.MaxLength);

		nctsHeader.BH_ApplicationCode = "NC5";
		previousDocument = goodsItem.PreviousDocuments.AddNew();
		AssertEquals("When Phase5, CSI_Reference2 MaxLength", 35, previousDocument.CSI_ReferenceNumber2Info.MaxLength);
	}

	public void TestCSI_ReferenceNumber2_Phase5Caption()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_ReferenceNumber2Info.PropertyDescriptor, NctsHeader.Phase5CaptionKey);

		AssertEquals("Caption", "Complement of Information", resourceStringData.Caption);
	}

	public void TestCSI_ReferenceNumber_Phase5Caption()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_ReferenceNumberInfo.PropertyDescriptor, NctsHeader.Phase5CaptionKey);

		AssertEquals("Caption", "Reference Number", resourceStringData.Caption);
	}

	public void TestCSI_PackQty_Phase5Caption()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_PackQtyInfo.PropertyDescriptor, NctsHeader.Phase5CaptionKey);

		AssertEquals("Caption", "Number of Packages", resourceStringData.Caption);
	}

	public void TestCSI_PackType_Phase5Caption()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_PackTypeInfo.PropertyDescriptor, NctsHeader.Phase5CaptionKey);

		AssertEquals("Caption", "Pkg Type", resourceStringData.Caption);
	}

	public void TestCSI_ItemNumber_Phase5Caption()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_ItemNumberInfo.PropertyDescriptor, NctsHeader.Phase5CaptionKey);

		AssertEquals("Caption", "Goods Item Identifier", resourceStringData.Caption);
	}

	protected override void SetUp()
	{
		base.SetUp();
		new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping("IT");
		var procedure = Factory.New<Universal.RefCusProcedure>();
		procedure.ZZ6_ProcedureCode = "10";
		procedure.ZZ6_ZZZ_NKDataGrouping = "IT";
		procedure.ZZ6_Description = "10 Desc";
		Factory.Save();
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		previousDocument = Factory.New<NctsPreviousDocumentForTest>();
		goodsItem.PreviousDocuments.Add(previousDocument);
	}

	NctsPreviousDocumentForTest previousDocument;

	protected override BusinessObject GetNewBusinessObject()
	{
		var header = Factory.NewDepartureNctsHeader();
		var goodItem = header.MovementHeader.GoodsItems.AddNew();
		return goodItem.PreviousDocuments.AddNew();
	}

	protected override IEnumerable<NctsPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var previousDocument = goodsItem.PreviousDocuments.AddNew();
		previousDocument.CSI_Code = "A";
		yield return previousDocument;
	}
}

public class NctsPreviousDocumentForTest : NctsPreviousDocument, IPreviousDocumentForTesting
{
	public NctsPreviousDocumentForTest(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public JobComInvoiceLine ParentLine => null;
}
