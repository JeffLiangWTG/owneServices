using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class PreviousDocumentConfigurationTest : TestCaseWithFactory
	{
		public void TestGetAvailableColumns_Import()
		{
			var availableColumnsForATAGeneric = "CSI_ReferenceNumber";
			var captionsForAvailableColumnsForATAGeneric = "Reference";
			var procedureCodeMappings = new (string, string, string)[]
			{
				(PreviousProcedureList.Codes._ATZL, "CSI_ReferenceNumber, CSI_LineNo, FormattedTariff, Status, UsualProcessingFlag, CSI_Quantity, CSI_UnitOfQuantity, CSI_Quantity2, CSI_UnitOfQuantity2, CSI_Description", "Reference, Line No., Commodity Code, Entry via ATLAS?, Usual Processing Flag, Commercial Qty., UQ, Debit Qty., UQ, Complementary Information"),
				(PreviousProcedureList.Codes._ATNEU, "CSI_SubType, CSI_ReferenceNumber2, CSI_ReferenceNumber, CSI_LineNo, CSI_Quantity", "Type, Custodian EORI, Reference, Line No., Package Qty."),
				(PreviousProcedureList.Codes._ATAV, "CSI_ReferenceNumber, CSI_LineNo, Status, CSI_Description", "Reference, Line No., Entry via ATLAS?, Information"),
				(PreviousProcedureList.Codes._ATA, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._ESUMA, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._GB, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._PUEB, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._T1, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._T2, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._TIR, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._VO, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
			};
			var procedureCodesThatArentMapped = new PreviousProcedureList().GetAllCodes().Except(procedureCodeMappings.Select(x => x.Item1));

			CombineAssertions(() =>
			{
				foreach (var mapping in procedureCodeMappings)
				{
					AssertProcedureCodeReturnsColumns(true, mapping.Item1, mapping.Item2, mapping.Item3, ImportDeclarationTypeList.Codes.AZ);
				}

				foreach (var mapping in procedureCodesThatArentMapped)
				{
					AssertProcedureCodeReturnsColumns(true, mapping, string.Empty, string.Empty, ImportDeclarationTypeList.Codes.AZ);
				}
			});
		}

		public void TestGetAvailableColumns_Import_LUZ()
		{
			var availableColumnsForATAGeneric = "CSI_ReferenceNumber, CSI_ItemNumberString";
			var captionsForAvailableColumnsForATAGeneric = "Reference, Invoice Line No.";
			var procedureCodeMappings = new[]
			{
				(PreviousProcedureList.Codes._ATZL, "CSI_ReferenceNumber, CSI_LineNo, FormattedTariff, Status, UsualProcessingFlag, CSI_Quantity, CSI_UnitOfQuantity, CSI_Quantity2, CSI_UnitOfQuantity2, CSI_Description, CSI_ItemNumberString", "Reference, Line No., Commodity Code, Entry via ATLAS?, Usual Processing Flag, Commercial Qty., UQ, Debit Qty., UQ, Complementary Information, Invoice Line No."),
				(PreviousProcedureList.Codes._ATNEU, "CSI_SubType, CSI_ReferenceNumber2, CSI_ReferenceNumber, CSI_LineNo, CSI_Quantity, CSI_ItemNumberString", "Type, Custodian EORI, Reference, Line No., Package Qty., Invoice Line No."),
				(PreviousProcedureList.Codes._ATAV, "CSI_ReferenceNumber, CSI_LineNo, Status, CSI_Description, CSI_ItemNumberString", "Reference, Line No., Entry via ATLAS?, Information, Invoice Line No."),
				(PreviousProcedureList.Codes._ATA, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._ESUMA, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._GB, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._PUEB, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._T1, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._T2, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._TIR, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
				(PreviousProcedureList.Codes._VO, availableColumnsForATAGeneric, captionsForAvailableColumnsForATAGeneric),
			};
			var procedureCodesThatArentMapped = new PreviousProcedureList().GetAllCodes().Except(procedureCodeMappings.Select(x => x.Item1));

			CombineAssertions(() =>
			{
				foreach (var mapping in procedureCodeMappings)
				{
					AssertProcedureCodeReturnsColumns(true, mapping.Item1, mapping.Item2, mapping.Item3, ImportDeclarationTypeList.Codes.LUZ);
				}

				foreach (var mapping in procedureCodesThatArentMapped)
				{
					AssertProcedureCodeReturnsColumns(true, mapping, string.Empty, string.Empty, ImportDeclarationTypeList.Codes.LUZ);
				}
			});
		}

		public void TestGetAvailableColumns_Export()
		{
			var procedureCodeMappings = new (string, string, string)[]
			{
				(PreviousProcedureList.Codes._ATAV, "CSI_ReferenceNumber, CSI_LineNo, Status, CSI_Description", "Reference, Line No., Entry via ATLAS?, Information"),
				(PreviousProcedureList.Codes._ATZL, "CSI_ReferenceNumber, CSI_LineNo, FormattedTariff, Status, UsualProcessingFlag, CSI_Quantity, CSI_UnitOfQuantity, CSI_Quantity2, CSI_UnitOfQuantity2, CSI_Description", "Reference, Line No., Commodity Code, Entry via ATLAS?, Usual Processing Flag, Commercial Qty., UQ, Debit Qty., UQ, Complementary Info"),
			};
			var procedureCodesThatArentMapped = new PreviousProcedureList().GetAllCodes().Except(procedureCodeMappings.Select(x => x.Item1));

			CombineAssertions(() =>
			{
				foreach (var mapping in procedureCodeMappings)
				{
					AssertProcedureCodeReturnsColumns(false, mapping.Item1, mapping.Item2, mapping.Item3, string.Empty);
				}

				foreach (var mapping in procedureCodesThatArentMapped)
				{
					AssertProcedureCodeReturnsColumns(false, mapping, string.Empty, string.Empty, string.Empty);
				}
			});
		}

		public void TestGetAvailableFields()
		{
			var procedureCodeMappings = new (string, string)[]
			{
				(PreviousProcedureList.Codes._ATZL, "AuthorizationNumber, CSI_ReferenceNumber2"),
				(PreviousProcedureList.Codes._ATAV, "AuthorizationNumber, CSI_CustomsOffice, SimplifiedGrantAuthorizationFlag, CSI_SubType")
			};
			var procedureCodesThatArentMapped = new PreviousProcedureList().GetAllCodes().Except(procedureCodeMappings.Select(x => x.Item1));

			CombineAssertions(() =>
			{
				foreach (var mapping in procedureCodeMappings)
				{
					AssertProcedureCodeReturnsFields(mapping.Item1, mapping.Item2);
				}

				foreach (var mapping in procedureCodesThatArentMapped)
				{
					AssertProcedureCodeReturnsFields(mapping, ZString.Empty);
				}
			});
		}

		public void TestGetAvailableNctsColumns()
		{
			var availableColumnsForATAGeneric = "CSI_Procedure";
			var procedureCodeMappings = new (string, string)[]
			{
				(NctsPreviousDocTypList.Codes.ATA, $"CSI_ReferenceNumber, CSI_AdditionalDescription, {availableColumnsForATAGeneric}"),
				(NctsPreviousDocTypList.Codes.FV, $"CSI_ReferenceNumber, CSI_AdditionalDescription, {availableColumnsForATAGeneric}"),
				(NctsPreviousDocTypList.Codes.POST, $"CSI_ReferenceNumber, CSI_AdditionalDescription, {availableColumnsForATAGeneric}"),
				(NctsPreviousDocTypList.Codes.ATNEU, $"CSI_SubType, CSI_ReferenceNumber2, CSI_ReferenceNumber, CSI_LineNo, CSI_Quantity, {availableColumnsForATAGeneric}"),
				(NctsPreviousDocTypList.Codes.AT_AV, $"CSI_ReferenceNumber, CSI_LineNo, Status, CSI_Description, {availableColumnsForATAGeneric}"),
				(NctsPreviousDocTypList.Codes.AT_ZL,$"CSI_ReferenceNumber, CSI_LineNo, FormattedTariff, Status, UsualProcessingFlag, CSI_Quantity, CSI_UnitOfQuantity, CSI_Quantity2, CSI_UnitOfQuantity2, CSI_Description, {availableColumnsForATAGeneric}")
			};
			var procedureCodesThatArentMapped = new PreviousProcedureList().GetAllCodes().Except(procedureCodeMappings.Select(x => x.Item1));

			CombineAssertions(() =>
			{
				foreach (var mapping in procedureCodeMappings)
				{
					AssertNctsProcedureCodeReturnColumns(mapping.Item1, mapping.Item2);
				}

				foreach (var mapping in procedureCodesThatArentMapped)
				{
					AssertNctsProcedureCodeReturnColumns(mapping, ZString.Empty);
				}
			});
		}

		public void TestAllColumns()
		{
			var expectedColumns = new string[]
			{
				CusSupportingInfo.Schema.CSI_ReferenceNumber,
				CusSupportingInfo.Schema.CSI_SubType,
				CusSupportingInfo.Schema.CSI_DateOfIssue,
				CusSupportingInfo.Schema.CSI_LineNo,
				PreviousDocument.Schema.Status,
				CusSupportingInfo.Schema.CSI_ReferenceNumber2,
				CusSupportingInfo.Schema.CSI_Description,
				PreviousDocument.Schema.FormattedTariff,
				CusSupportingInfo.Schema.CSI_Quantity,
				CusSupportingInfo.Schema.CSI_UnitOfQuantity,
				CusSupportingInfo.Schema.CSI_Quantity2,
				CusSupportingInfo.Schema.CSI_UnitOfQuantity2,
				CusSupportingInfo.Schema.CSI_CustomsOffice,
				PreviousDocument.Schema.UsualProcessingFlag,
				PreviousDocument.Schema.AuthorizationNumber,
				AutoCusSupportingInfo.Schema.CSI_Code,
				AutoCusSupportingInfo.Schema.CSI_AdditionalDescription,
				PreviousDocument.Schema.CSI_ItemNumberString
			};
			var allColumns = config.GetUnavailableColumns(Enumerable.Empty<string>());
			AssertContainsExactElementsInAnyOrder(expectedColumns, allColumns);
		}

		void AssertNctsProcedureCodeReturnColumns(string procedureCode, string expectedCodesAsString) => AssertEquals(procedureCode, expectedCodesAsString, string.Join(", ", config.GetAvailableNctsColumns(procedureCode).Select(x => x.ColumnName).ToList()));

		void AssertProcedureCodeReturnsColumns(bool isImport, string procedureCode, string expectedCodesAsString, string expectedCaptionsAsString, string cusEntryInstructionStyle)
		{
			var availableColumns = config.GetAvailableColumnsFromProcedureCode(isImport, procedureCode, cusEntryInstructionStyle).ToList();
			AssertEquals(procedureCode, expectedCodesAsString, string.Join(", ", availableColumns.Select(x => x.ColumnName)));
			AssertEquals(procedureCode, expectedCaptionsAsString, string.Join(", ", availableColumns.Select(x => x.Caption)));
		}

		void AssertProcedureCodeReturnsFields(string procedureCode, string expectedCodesAsString) => AssertEquals(procedureCode, expectedCodesAsString, string.Join(", ", config.GetAvailableFieldsFromProcedureCode(procedureCode).ToList()));

		protected override void SetUp()
		{
			base.SetUp();
			config = new PreviousDocumentConfiguration();
		}
		PreviousDocumentConfiguration config;
	}
}
