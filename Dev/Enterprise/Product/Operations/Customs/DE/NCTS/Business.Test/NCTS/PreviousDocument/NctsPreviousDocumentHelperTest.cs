using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentHelperTest : TestCaseWithFactory
	{
		public void TestConvertNctsPreviousProcedureCodeToDeclarationProcedureCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("N337", PreviousProcedureList.Codes._ATNEU, NctsPreviousDocumentHelper.ConvertNctsPreviousProcedureCodeToDeclarationProcedureCode(NctsPreviousProcedureList.Codes._N337));
				AssertEquals("9DEZ", PreviousProcedureList.Codes._ATZL, NctsPreviousDocumentHelper.ConvertNctsPreviousProcedureCodeToDeclarationProcedureCode(NctsPreviousProcedureList.Codes._9DEZ));
				AssertEquals("9DEY", PreviousProcedureList.Codes._ATAV, NctsPreviousDocumentHelper.ConvertNctsPreviousProcedureCodeToDeclarationProcedureCode(NctsPreviousProcedureList.Codes._9DEY));
				AssertEquals("Empty string", ZString.Empty, NctsPreviousDocumentHelper.ConvertNctsPreviousProcedureCodeToDeclarationProcedureCode(ZString.Empty));
				AssertEquals("Unknown input", ZString.Empty, NctsPreviousDocumentHelper.ConvertNctsPreviousProcedureCodeToDeclarationProcedureCode("XYZ"));
			});
		}

		public void TestIsAvailable()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_CustomsOffice is available for 9DEY", true, NctsPreviousDocumentHelper.IsAvailable(NctsPreviousProcedureList.Codes._9DEY, NctsPreviousDocument.Schema.CSI_CustomsOffice));
				AssertEquals("CSI_CustomsOffice is not available for 9DEZ", false, NctsPreviousDocumentHelper.IsAvailable(NctsPreviousProcedureList.Codes._9DEZ, NctsPreviousDocument.Schema.CSI_CustomsOffice));
			});
		}

		public void TestNctsProcedureCodeSupportsMultiplePreviousDocuments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("N337", true, NctsPreviousDocumentHelper.NctsProcedureCodeSupportsMultiplePreviousDocuments(NctsPreviousProcedureList.Codes._N337));
				AssertEquals("9DEY", true, NctsPreviousDocumentHelper.NctsProcedureCodeSupportsMultiplePreviousDocuments(NctsPreviousProcedureList.Codes._9DEY));
				AssertEquals("9DEZ", true, NctsPreviousDocumentHelper.NctsProcedureCodeSupportsMultiplePreviousDocuments(NctsPreviousProcedureList.Codes._9DEZ));
				AssertEquals("Empty string", false, NctsPreviousDocumentHelper.NctsProcedureCodeSupportsMultiplePreviousDocuments(ZString.Empty));
			});
		}

		public void TestGetAvailableNctsColumns_N337()
		{
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					(CusSupportingInfo.Schema.CSI_SubType, "Type"),
					(CusSupportingInfo.Schema.CSI_ReferenceNumber2, "Custodian EORI"),
					(CusSupportingInfo.Schema.CSI_ReferenceNumber, "Reference"),
					(CusSupportingInfo.Schema.CSI_LineNo, "Line No."),
					(CusSupportingInfo.Schema.CSI_Quantity, "Package Qty."),
					(CusSupportingInfo.Schema.CSI_Procedure, "Procedure"),
				},
				NctsPreviousDocumentHelper.GetAvailableNctsColumns(NctsPreviousProcedureList.Codes._N337)
			);
		}

		public void TestGetAvailableNctsColumns_9DEY()
		{
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					(NctsPreviousDocument.Schema.CSI_ReferenceNumber, "Reference"),
					(NctsPreviousDocument.Schema.CSI_LineNo, "Line No."),
					(NctsPreviousDocument.Schema.Status, "Entry via ATLAS?"),
					(NctsPreviousDocument.Schema.CSI_Description, "Information"),
					(CusSupportingInfo.Schema.CSI_Procedure, "Procedure"),
				},
				NctsPreviousDocumentHelper.GetAvailableNctsColumns(NctsPreviousProcedureList.Codes._9DEY)
			);
		}

		public void TestGetAvailableNctsColumns_9DEZ()
		{
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					(NctsPreviousDocument.Schema.CSI_ReferenceNumber, "Reference"),
					(NctsPreviousDocument.Schema.CSI_LineNo, "Line No."),
					(NctsPreviousDocument.Schema.FormattedTariff, "Commodity Code"),
					(NctsPreviousDocument.Schema.Status, "Entry via ATLAS?"),
					(NctsPreviousDocument.Schema.UsualProcessingFlag, "Usual Processing Flag"),
					(NctsPreviousDocument.Schema.CSI_Quantity, "Commercial Qty."),
					(NctsPreviousDocument.Schema.CSI_UnitOfQuantity, "UQ"),
					(NctsPreviousDocument.Schema.CSI_Quantity2, "Debit Qty."),
					(NctsPreviousDocument.Schema.CSI_UnitOfQuantity2, "UQ"),
					(NctsPreviousDocument.Schema.CSI_Description, "Complementary Information"),
					(CusSupportingInfo.Schema.CSI_Procedure, "Procedure"),
				},
				NctsPreviousDocumentHelper.GetAvailableNctsColumns(NctsPreviousProcedureList.Codes._9DEZ)
			);
		}

		public void TestGetUnavailableColumns()
		{
			AssertArrayEqualsByElements(
				new[]
				{
					NctsPreviousDocument.Schema.CSI_SubType,
					NctsPreviousDocument.Schema.CSI_DateOfIssue, NctsPreviousDocument.Schema.CSI_LineNo,
					NctsPreviousDocument.Schema.Status, NctsPreviousDocument.Schema.CSI_ReferenceNumber2,
					NctsPreviousDocument.Schema.CSI_Description, NctsPreviousDocument.Schema.FormattedTariff,
					NctsPreviousDocument.Schema.CSI_Quantity, NctsPreviousDocument.Schema.CSI_UnitOfQuantity,
					NctsPreviousDocument.Schema.CSI_Quantity2, NctsPreviousDocument.Schema.CSI_UnitOfQuantity2,
					NctsPreviousDocument.Schema.CSI_CustomsOffice, NctsPreviousDocument.Schema.UsualProcessingFlag,
					NctsPreviousDocument.Schema.AuthorizationNumber, NctsPreviousDocument.Schema.CSI_Code,
					NctsPreviousDocument.Schema.CSI_AdditionalDescription, "CSI_ItemNumberString"
				},
				NctsPreviousDocumentHelper.GetUnavailableColumns(new[] { NctsPreviousDocument.Schema.CSI_ReferenceNumber }));
		}
	}
}
