using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using ITNctsDepartureCargoDesc = Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc;
using ITNctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(ITNctsDepartureCargoDescWrapper))]
sealed class ITNctsDepartureCargoDescWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestBox33Commodity()
	{
		item.BY_HarmonisedTariff = "1234567890";
		CombineAssertions("BY_HarmonisedTariff is truncated only if it is longer than 8 characters", () =>
		{
			var wrapper = ITNctsDepartureCargoDescWrapper.New(item, Factory);
			AssertEquals(nameof(ITNctsDepartureCargoDescWrapper.BOX33COMMODITY), "12345678", wrapper.BOX33COMMODITY);

			item.BY_HarmonisedTariff = "1234";
			AssertEquals(nameof(ITNctsDepartureCargoDescWrapper.BOX33COMMODITY), "1234", wrapper.BOX33COMMODITY);

			item.BY_HarmonisedTariff = "";
			AssertEquals(nameof(ITNctsDepartureCargoDescWrapper.BOX33COMMODITY), "---", wrapper.BOX33COMMODITY);
		});
	}

	public void TestBox40Documents_Phase4()
	{
		var header = Factory.New<ITNctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		item = header.MovementHeader.GoodsItems.AddNew();

		var documentOnFirstLine = item.PreviousDocuments.AddNew();
		documentOnFirstLine.CSI_Procedure = "7";
		documentOnFirstLine.CSI_SubType = Enterprise.Customs.EU.Business.PreviousDocumentClassList.Codes.PreviousDocument;
		documentOnFirstLine.CSI_Code = "380";
		documentOnFirstLine.CSI_ReferenceNumber = "34217890";
		documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
		documentOnFirstLine.CSI_Status = "G";
		documentOnFirstLine.CSI_CustomsOffice = "IT279100";
		documentOnFirstLine.CSI_LineNo = 1;

		var documentOnSecondLine = item.PreviousDocuments.AddNew();
		documentOnSecondLine.CSI_Procedure = "7";
		documentOnSecondLine.CSI_SubType = "Y";
		documentOnSecondLine.CSI_Code = "CLE";
		documentOnSecondLine.CSI_ReferenceNumber2 = "20070701";
		documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
		documentOnSecondLine.CSI_Status = "G";
		documentOnSecondLine.CSI_CustomsOffice = "IT279100";
		documentOnSecondLine.CSI_LineNo = 1;

		var wrapper = ITNctsDepartureCargoDescWrapper.New(item, Factory);

		AssertEquals(nameof(ITNctsDepartureCargoDescWrapper.BOX40DOCUMENTS), "Z-380-7-34217890 G-02/01/2000-IT279100-1; Y-CLE-7-20070701 G-03/01/2000-IT279100-1", wrapper.BOX40DOCUMENTS);
	}

	public void TestBox40Documents_Phase5()
	{
		header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		var bill = header.Bills.AddNew();
		var item = bill.GoodsItems.AddNew();

		var billDocument = bill.PreviousDocuments.AddNew();
		billDocument.CSI_Procedure = "7";
		billDocument.CSI_SubType = "Z";
		billDocument.CSI_Code = "380";
		billDocument.CSI_ReferenceNumber = "34217890";
		billDocument.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
		billDocument.CSI_Status = "G";
		billDocument.CSI_CustomsOffice = "IT279100";
		billDocument.CSI_LineNo = 1;

		var itemDocument = item.PreviousDocuments.AddNew();
		itemDocument.CSI_Procedure = "5";
		itemDocument.CSI_SubType = "X";
		itemDocument.CSI_Code = "230";
		itemDocument.CSI_ReferenceNumber = "5768357";
		itemDocument.CSI_DateOfIssue = new ZDateTime(2005, 5, 8);
		itemDocument.CSI_Status = "G";
		itemDocument.CSI_CustomsOffice = "IT98700";
		itemDocument.CSI_LineNo = 1;

		var wrapper = ITNctsDepartureCargoDescWrapper.New(item, Factory);
		AssertEquals(nameof(ITNctsDepartureCargoDescWrapper.BOX40DOCUMENTS), "380-34217890; 230-5768357", wrapper.BOX40DOCUMENTS);
	}

	public void TestGetSupportingDocumentsFormatted_Phase4()
	{
		var header = Factory.New<ITNctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var item = header.MovementHeader.GoodsItems.AddNew();
		var supportingDocument = item.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "N380";
		supportingDocument.CSI_ReferenceNumber = "A0023";
		supportingDocument.CSI_RN_NKCountryCode = "IT";
		supportingDocument.CSI_YearOfIssue = "2021";
		supportingDocument.CSI_UnitOfQuantity = "XYZ";
		supportingDocument.CSI_Quantity = 343.43m;

		var wrapper = ITNctsDepartureCargoDescWrapper.New(item, Factory);
		AssertEquals(nameof(ITNctsDepartureCargoDescWrapper.BOX44), "N380-IT-2021-A0023-XYZ-343.43", wrapper.BOX44);
	}

	public void TestGetSupportingDocumentsFormatted_Phase5()
	{
		var bill = header.Bills.AddNew();
		var billDocument = bill.SupportingDocuments.AddNew();
		billDocument.CSI_Code = "420";
		billDocument.CSI_ReferenceNumber = "123";

		var item = bill.GoodsItems.AddNew();
		var itemDocument = item.SupportingDocuments.AddNew();
		itemDocument.CSI_Code = "N380";
		itemDocument.CSI_ReferenceNumber = "A0023";
		itemDocument.CSI_RN_NKCountryCode = "IT";
		itemDocument.CSI_YearOfIssue = "2021";
		itemDocument.CSI_UnitOfQuantity = "XYZ";
		itemDocument.CSI_Quantity = 343.43m;

		var wrapper = ITNctsDepartureCargoDescWrapper.New(item, Factory);
		AssertEquals(nameof(ITNctsDepartureCargoDescWrapper.BOX44), "420-123; N380-A0023", wrapper.BOX44);
	}

	public void TestBox441DocsAndCerts_Phase4()
	{
		var header = Factory.New<ITNctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var item = header.MovementHeader.GoodsItems.AddNew();

		var wrapper = ITNctsDepartureCargoDescWrapper.New(item, Factory);
		AssertEquals(nameof(ITNctsDepartureCargoDescWrapper.BOX441DOCSANDCERTS), "", wrapper.BOX441DOCSANDCERTS);

		var supportingDocument1 = item.SupportingDocuments.AddNew();
		var supportingDocument2 = item.SupportingDocuments.AddNew();
		var supportingDocument3 = item.SupportingDocuments.AddNew();
		var supportingDocument4 = item.SupportingDocuments.AddNew();
		var supportingDocument5 = item.SupportingDocuments.AddNew();

		supportingDocument1.CSI_Code = "N380";
		supportingDocument1.CSI_ReferenceNumber = "A0023";
		supportingDocument1.CSI_RN_NKCountryCode = "IT";
		supportingDocument1.CSI_YearOfIssue = "2021";
		supportingDocument1.CSI_UnitOfQuantity = "XYZ";
		supportingDocument1.CSI_Quantity = 343.43m;

		supportingDocument2.CSI_Code = "C601";
		supportingDocument2.CSI_ReferenceNumber = "A0050";
		supportingDocument2.CSI_RN_NKCountryCode = "DE";
		supportingDocument2.CSI_YearOfIssue = "2021";
		supportingDocument2.CSI_UnitOfQuantity = "KGM";

		supportingDocument3.CSI_Code = "N381";
		supportingDocument3.CSI_ReferenceNumber = "A0033";
		supportingDocument3.CSI_RN_NKCountryCode = "IT";
		supportingDocument3.CSI_UnitOfQuantity = "KGM";

		supportingDocument4.CSI_Code = "C888";
		supportingDocument4.CSI_ReferenceNumber = "A0987";

		supportingDocument5.CSI_Code = "CXXX";

		wrapper = ITNctsDepartureCargoDescWrapper.New(item, Factory);
		AssertEquals(nameof(ITNctsDepartureCargoDescWrapper.BOX441DOCSANDCERTS), "N380-IT-2021-A0023-XYZ-343.43; C601-DE-2021-A0050-KGM; N381-IT-A0033-KGM; C888-A0987; CXXX", wrapper.BOX441DOCSANDCERTS);
	}

	public void TestBox441DocsAndCerts_Phase5()
	{
		var bill = header.Bills.AddNew();
		var billDocument = bill.SupportingDocuments.AddNew();
		billDocument.CSI_Code = "420";
		billDocument.CSI_ReferenceNumber = "123";

		var item = bill.GoodsItems.AddNew();
		var itemDocument = item.SupportingDocuments.AddNew();
		itemDocument.CSI_Code = "N380";
		itemDocument.CSI_ReferenceNumber = "A0023";
		itemDocument.CSI_RN_NKCountryCode = "IT";
		itemDocument.CSI_YearOfIssue = "2021";
		itemDocument.CSI_UnitOfQuantity = "XYZ";
		itemDocument.CSI_Quantity = 343.43m;

		var wrapper = ITNctsDepartureCargoDescWrapper.New(item, Factory);
		AssertEquals(nameof(ITNctsDepartureCargoDescWrapper.BOX441DOCSANDCERTS), "420-123; N380-A0023", wrapper.BOX441DOCSANDCERTS);
	}

	public void TestBox35GrossMass()
	{
		CombineAssertions(() =>
		{
			var wrapper = ITNctsDepartureCargoDescWrapper.New(item, Factory);
			AssertEquals(nameof(wrapper.BOX35GROSSMASS), "---", wrapper.BOX35GROSSMASS);

			item.BY_GrossWeight = 30;
			item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(nameof(wrapper.BOX35GROSSMASS), "30", wrapper.BOX35GROSSMASS);

			item.BY_GrossWeight = 123.4678m;
			item.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals(nameof(wrapper.BOX35GROSSMASS), "0.123468", wrapper.BOX35GROSSMASS);

			item.BY_GrossWeight = 99.12345698m;
			item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(nameof(wrapper.BOX35GROSSMASS), "99.123457", wrapper.BOX35GROSSMASS);
		});
	}

	public void TestBox38NetMass()
	{
		CombineAssertions(() =>
		{
			var wrapper = ITNctsDepartureCargoDescWrapper.New(item, Factory);
			AssertEquals(nameof(wrapper.BOX38NETTMASS), "---", wrapper.BOX38NETTMASS);

			item.BY_NetWeight = 30;
			item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(nameof(wrapper.BOX38NETTMASS), "30", wrapper.BOX38NETTMASS);

			item.BY_NetWeight = 123.4678m;
			item.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals(nameof(wrapper.BOX38NETTMASS), "0.123468", wrapper.BOX38NETTMASS);

			item.BY_NetWeight = 99.12345698m;
			item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(nameof(wrapper.BOX38NETTMASS), "99.123457", wrapper.BOX38NETTMASS);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => ITNctsDepartureCargoDescWrapper.New(Factory.New<ITNctsDepartureCargoDesc>(), Factory);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<ITNctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		item = header.Bills.AddNew().GoodsItems.AddNew();
	}

	ITNctsHeader header;
	ITNctsDepartureCargoDesc item;
}
