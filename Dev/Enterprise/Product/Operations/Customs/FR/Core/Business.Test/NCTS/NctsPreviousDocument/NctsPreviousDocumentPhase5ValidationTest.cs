using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsPreviousDocumentPhase5ValidationTest : TestCaseWithFactory
	{
		public void TestCheckCSI_SubType()
		{
			var previousDocument = GetPreviousDocumentForTest();

			previousDocument.CSI_SubType = "";
			AssertNoNotifications("no validation on csi_subtype", previousDocument.CSI_SubTypeInfo);
			previousDocument.CSI_SubType = ";:,ccac*$";
			AssertNoNotifications("no validation on csi_subtype", previousDocument.CSI_SubTypeInfo);
		}

		public void TestCheckCSI_Reference()
		{
			var testedMessage = "The entered IST reference OTHER REGISTER HEADER does not match any Temporary Storage Register.";

			var regHeader = GetRegHeader();
			var previousDocument = GetPreviousDocumentForTest();

			previousDocument.CSI_ReferenceNumber = "";
			AssertHasMessageErrorContaining("Reference Number is mandatory but the value is not provided.", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_ReferenceNumber = "RefNumber";
			AssertNoMessageErrorContaining("Reference Number is mandatory but the value is provided.", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register but document type is not N337.", previousDocument.CSI_ReferenceNumberInfo, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining("A message error is expected when there is no matching register and document type is N337.", previousDocument.CSI_ReferenceNumberInfo, testedMessage);

			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			AssertNoMessageErrorContaining("No message error is expected when a matching register header is found.", previousDocument.CSI_ReferenceNumberInfo, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			AssertHasMessageErrorContaining("A message error is expected when there is no matching register.", previousDocument.CSI_ReferenceNumberInfo, testedMessage);
		}

		public void TestCheckCSI_ItemNumber()
		{
			var testedMessage = "The entered line number does not exist for this IST.";

			var regHeader = GetRegHeader();

			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 99;
			var previousDocument = GetPreviousDocumentForTest();

			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 1;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register line but document type is not N337.", previousDocument.CSI_ItemNumberInfo, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateCSI_ItemNumber();
			AssertHasMessageErrorContaining("A message error is expected when there is no matching register line and document type is N337.", previousDocument.CSI_ItemNumberInfo, testedMessage);

			previousDocument.CSI_ItemNumber = 99;
			AssertNoMessageErrorContaining("No message error is expected when there is a matching register line.", previousDocument.CSI_ItemNumberInfo, testedMessage);

			previousDocument.CSI_ItemNumber = 9;
			AssertHasMessageErrorContaining("A message error is expected when the register line does not match.", previousDocument.CSI_ItemNumberInfo, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 1;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register.", previousDocument.CSI_ItemNumberInfo, testedMessage);
		}

		public void TestCheckCSI_UnitOfQuantity2()
		{
			var testedMessage = "The entered package type does not equal the package type for this line on the IST.";

			var regHeader = GetRegHeader();

			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 99;
			regLine.SRL_PackageType = "PKG";
			var previousDocument = GetPreviousDocumentForTest();

			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_UnitOfQuantity2 = "CTN";
			AssertNoMessageErrorContaining("No message error is expected when the matching register line package type doesn't match the document pack type but document type is not N337.", previousDocument.CSI_UnitOfQuantity2Info, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity2();
			AssertHasMessageErrorContaining("A message error is expected when the matching register line package type doesn't match the document pack type and document type is N337.", previousDocument.CSI_UnitOfQuantity2Info, testedMessage);

			previousDocument.CSI_UnitOfQuantity2 = "PKG";
			AssertNoMessageErrorContaining("No message error is expected when the matching register line package type matches the document pack type.", previousDocument.CSI_UnitOfQuantity2Info, testedMessage);

			previousDocument.CSI_UnitOfQuantity2 = "CTN";
			AssertHasMessageErrorContaining("A message error is expected when the package type doesn't match the document pack type.", previousDocument.CSI_UnitOfQuantity2Info, testedMessage);

			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_UnitOfQuantity2 = "CTN";
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register line.", previousDocument.CSI_UnitOfQuantity2Info, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_UnitOfQuantity2 = "CTN";
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register.", previousDocument.CSI_UnitOfQuantity2Info, testedMessage);
		}

		public void TestCheckCSI_Quantity2()
		{
			var testedMessage = "The entered package quantity should be less than or equal to the corresponding line in the IST.";

			var regHeader = GetRegHeader();

			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 99;
			regLine.SRL_PackagesRemaining = 9;
			var previousDocument = GetPreviousDocumentForTest();

			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_Quantity2 = 10;
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has not enough quantity but document type is not N337.", previousDocument.CSI_Quantity2Info, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateCSI_Quantity2();
			AssertHasMessageErrorContaining("A message error is expected when the matching register line has not enough quantity and document type is N337.", previousDocument.CSI_Quantity2Info, testedMessage);

			previousDocument.CSI_Quantity2 = 9;
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has enough quantity.", previousDocument.CSI_Quantity2Info, testedMessage);

			previousDocument.CSI_Quantity2 = 8;
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has enough quantity.", previousDocument.CSI_Quantity2Info, testedMessage);

			previousDocument.CSI_Quantity2 = 100;
			AssertHasMessageErrorContaining("A message error is expected when the matching register line has exceeds the remaining packages in the matching register line. ", previousDocument.CSI_Quantity2Info, testedMessage);

			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_Quantity2 = 100;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register line.", previousDocument.CSI_Quantity2Info, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_Quantity2 = 100;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register.", previousDocument.CSI_Quantity2Info, testedMessage);
		}

		public void TestCheckCSI_Quantity()
		{
			var testedMessage = "The entered quantity should be less than or equal to the corresponding line in the IST.";

			var regHeader = GetRegHeader();

			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 99;

			var transactionLine = regLine.CusTempStorageRegLineTransactions.AddNew();
			transactionLine.SRT_TransactionStatus = "CON";
			transactionLine.SRT_GrossWeight = 999;
			AssertEquals("Prerequisite", 999m, regLine.GrossWeightRemainingCalculated);

			var previousDocument = GetPreviousDocumentForTest();

			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_Quantity = 1000;
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has not enough weight but document type is not N337.", previousDocument.CSI_QuantityInfo, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateCSI_Quantity();
			AssertHasMessageErrorContaining("A message error is expected when the matching register line has not enough weight and document type is N337.", previousDocument.CSI_QuantityInfo, testedMessage);

			previousDocument.CSI_Quantity = 999;
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has enough weight.", previousDocument.CSI_QuantityInfo, testedMessage);

			previousDocument.CSI_Quantity = 990;
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has enough weight.", previousDocument.CSI_QuantityInfo, testedMessage);

			previousDocument.CSI_Quantity = 1000;
			AssertHasMessageErrorContaining("A message error is expected when the matching register line has exceeds gross weight.", previousDocument.CSI_QuantityInfo, testedMessage);

			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_Quantity = 1000;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register line.", previousDocument.CSI_QuantityInfo, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_Quantity = 1000;
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register.", previousDocument.CSI_QuantityInfo, testedMessage);
		}

		public void TestCheckCSI_UnitOfQuantity()
		{
			var testedMessage = "The entered measurement unit does not equal the unit for this line on the IST.";

			var regHeader = GetRegHeader();
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 99;
			regLine.SRL_GrossWeightUQ = "KGM";

			var previousDocument = GetPreviousDocumentForTest();

			previousDocument.CSI_ReferenceNumber = "REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;

			previousDocument.CSI_UnitOfQuantity = "HLT";
			AssertNoMessageErrorContaining("No message error is expected when the matching register line doesn't have matching unit but document type is not N337.", previousDocument.CSI_UnitOfQuantityInfo, testedMessage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageErrorContaining("A message error is expected when the matching register line doesn't have matching unit and document type is N337.", previousDocument.CSI_UnitOfQuantityInfo, testedMessage);

			previousDocument.CSI_UnitOfQuantity = "KGM";
			AssertNoMessageErrorContaining("No message error is expected when the matching register line has matching unit.", previousDocument.CSI_UnitOfQuantityInfo, testedMessage);

			previousDocument.CSI_UnitOfQuantity = "HLT";
			AssertHasMessageErrorContaining("A message error is expected when the matching register line doesn't have matching unit.", previousDocument.CSI_UnitOfQuantityInfo, testedMessage);

			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_UnitOfQuantity = "HLT";
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register line.", previousDocument.CSI_UnitOfQuantityInfo, testedMessage);

			previousDocument.CSI_ReferenceNumber = "OTHER REGISTER HEADER";
			previousDocument.CSI_ItemNumber = 99;
			previousDocument.CSI_UnitOfQuantity = "HLT";
			AssertNoMessageErrorContaining("No message error is expected when there is no matching register.", previousDocument.CSI_QuantityInfo, testedMessage);
		}

		CusTempStorageRegHeader GetRegHeader()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "REGISTER HEADER";

			var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist.SJH_JobReference = "REGISTER HEADER";
			ist.DDTNumber = "REGISTER HEADER";

			return regHeader;
		}

		NctsPreviousDocument GetPreviousDocumentForTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
			return previousDocument;
		}
		NctsDepartureCargoDesc goodsItem;
		NctsPreviousDocument previousDocument;
	}
}
