using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class PreviousDocumentConfiguration
	{
		public ZBool ProcedureCodeSupportsMultiplePreviousDocuments(bool isImport, ZString procedureCode) => GetAvailableColumnsFromProcedureCode(isImport, procedureCode, ZString.Empty).Any();

		public ZBool NctsProcedureCodeSupportsMultiplePreviousDocuments(ZString procedureCode) => GetAvailableNctsColumns(procedureCode).Any();

		public IEnumerable<(string ColumnName, string Caption)> GetAvailableColumnsFromProcedureCode(bool isImport, ZString procedureCode, ZString cusEntryInstructionStyle)
			=> isImport ? GetAvailableColumnsFromProcedureCode_Import(procedureCode, cusEntryInstructionStyle) : GetAvailableColumnsFromProcedureCode_Export(procedureCode);

		#region Available Columns For Import
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Switch statement...")]
		IEnumerable<(string ColumnName, string Caption)> GetAvailableColumnsFromProcedureCode_Import(ZString procedureCode, ZString entryInstructionStyle)
		{
			List<(string ColumnName, string Caption)> result;
			switch (procedureCode)
			{
				case PreviousProcedureList.Codes._ATA:
				case PreviousProcedureList.Codes._ESUMA:
				case PreviousProcedureList.Codes._GB:
				case PreviousProcedureList.Codes._PUEB:
				case PreviousProcedureList.Codes._T1:
				case PreviousProcedureList.Codes._T2:
				case PreviousProcedureList.Codes._TIR:
				case PreviousProcedureList.Codes._VO:
					result = availableColumnsForATA_Import.ToList();
					break;
				case PreviousProcedureList.Codes._ATAV:
					result = availableColumnsForATAV_Import.ToList();
					break;
				case PreviousProcedureList.Codes._ATNEU:
					result = availableColumnsForATNEU_Import.ToList();
					break;
				case PreviousProcedureList.Codes._ATZL:
					result = availableColumnsForATZL_Import.ToList();
					break;
				default:
					return Enumerable.Empty<(string, string)>();
			}

			if (entryInstructionStyle == ImportDeclarationTypeList.Codes.LUZ)
			{
				result.Add((PreviousDocument.Schema.CSI_ItemNumberString, Res.GetString("F64970F4-0366-4E07-BE95-379BAC3853CF", "Invoice Line No.")));
			}

			return result;
		}

		public IEnumerable<(string ColumnName, string Caption)> GetAvailableNctsColumns(string procedure)
		{
			IEnumerable<(string ColumnName, string Caption)> availableColumns = Enumerable.Empty<(string, string)>();
			switch (procedure)
			{
				case NctsPreviousDocTypList.Codes.ATA:
				case NctsPreviousDocTypList.Codes.FV:
				case NctsPreviousDocTypList.Codes.POST:
					availableColumns = availableColumnsForATA_Import.Concat(new[] { (AutoCusSupportingInfo.Schema.CSI_AdditionalDescription, Res.GetString("29D098F0-4B5D-450A-B29C-CEF875061DD2", "Complement")) });
					break;
				case NctsPreviousDocTypList.Codes.ATNEU:
					{
						availableColumns = availableColumnsForATNEU_Import;
						break;
					}
				case NctsPreviousDocTypList.Codes.AT_AV:
					{
						availableColumns = availableColumnsForATAV_Import;
						break;
					}
				case NctsPreviousDocTypList.Codes.AT_ZL:
					{
						availableColumns = availableColumnsForATZL_Import;
						break;
					}
			}
			if (availableColumns.Any())
			{
				availableColumns = availableColumns.Concat(new[] { (CusSupportingInfo.Schema.CSI_Procedure, Res.GetString("8F190393-798E-43E2-ABD5-16FDD22C3F09", "Procedure")) });
			}
			return availableColumns;
		}

		readonly IEnumerable<(string ColumnName, string Caption)> availableColumnsForATA_Import = new[]
		{
			(CusSupportingInfo.Schema.CSI_ReferenceNumber, Res.GetString("86F917A2-8A46-4ED3-8244-6846A6F8C59F", "Reference"))
		};

		readonly IEnumerable<(string ColumnName, string Caption)> availableColumnsForATAV_Import = new[]
		{
			(CusSupportingInfo.Schema.CSI_ReferenceNumber, Res.GetString("71B1798A-813C-4485-A9A2-53CD8FF12374", "Reference")),
			(CusSupportingInfo.Schema.CSI_LineNo, Res.GetString("01BECF61-EFC2-4AD4-862F-FB8760EA1CC9", "Line No.")),
			(PreviousDocument.Schema.Status, Res.GetString("6E1BBFBF-72AE-4F87-A882-BFFA1DAAAAB0", "Entry via ATLAS?")),
			(CusSupportingInfo.Schema.CSI_Description, Res.GetString("657E8B2A-D6B2-460B-8EFD-5B70F9B7F787", "Information")),
		};

		readonly IEnumerable<(string ColumnName, string Caption)> availableColumnsForATNEU_Import = new[]
		{
			(CusSupportingInfo.Schema.CSI_SubType, Res.GetString("77F0CAB3-1A79-4819-8ED8-9DB3C35E9043", "Type")),
			(CusSupportingInfo.Schema.CSI_ReferenceNumber2, Res.GetString("C5D94796-4A5E-4AA0-8273-B686DED10837", "Custodian EORI")),
			(CusSupportingInfo.Schema.CSI_ReferenceNumber, Res.GetString("3AEC0D8E-46A1-4F2C-8958-D01DD4AE6763", "Reference")),
			(CusSupportingInfo.Schema.CSI_LineNo, Res.GetString("56F8DC1E-AB6E-49EE-B544-8F81D4B6778A", "Line No.")),
			(CusSupportingInfo.Schema.CSI_Quantity, Res.GetString("A7CB74A8-E41E-4540-A343-46F08D1058BF", "Package Qty.")),
		};

		readonly IEnumerable<(string ColumnName, string Caption)> availableColumnsForATZL_Import = new[]
		{
			(CusSupportingInfo.Schema.CSI_ReferenceNumber, Res.GetString("078494A6-2AA7-4D90-B967-8779E5AD545C", "Reference")),
			(CusSupportingInfo.Schema.CSI_LineNo, Res.GetString("069F4EDF-0447-4A25-A2A2-780549A0CE3B", "Line No.")),
			(PreviousDocument.Schema.FormattedTariff, Res.GetString("D0E1ECD0-1A28-4654-A8B5-49401D6A3D54", "Commodity Code")),
			(PreviousDocument.Schema.Status, Res.GetString("CC66897A-C29B-4429-AD33-D0DBFD9F1EF9", "Entry via ATLAS?")),
			(PreviousDocument.Schema.UsualProcessingFlag, Res.GetString("24D2A090-637C-43CA-94E9-6D56288BDA43", "Usual Processing Flag")),
			(CusSupportingInfo.Schema.CSI_Quantity, Res.GetString("50EC496C-EF8D-4A74-B472-0468B51B1F71", "Commercial Qty.")),
			(CusSupportingInfo.Schema.CSI_UnitOfQuantity, Res.GetString("E07B1F01-38B7-42A6-98E3-740E497FBA06", "UQ")),
			(CusSupportingInfo.Schema.CSI_Quantity2, Res.GetString("611B511C-5DC7-4477-8476-58CD65EC485D", "Debit Qty.")),
			(CusSupportingInfo.Schema.CSI_UnitOfQuantity2, Res.GetString("D1F718FF-5CDD-4151-8606-A2D04744AFB8", "UQ")),
			(CusSupportingInfo.Schema.CSI_Description, Res.GetString("4EA05D7C-4556-406F-8891-16078442E5F8", "Complementary Information"))
		};
		#endregion

		#region Available Columns For Export
		IEnumerable<(string ColumnName, string Caption)> GetAvailableColumnsFromProcedureCode_Export(ZString procedureCode)
		{
			switch (procedureCode)
			{
				case PreviousProcedureList.Codes._ATAV:
					return availableColumnsForATAV_Export;
				case PreviousProcedureList.Codes._ATZL:
					return availableColumnsForATZL_Export;
				default:
					return Enumerable.Empty<(string, string)>();
			}
		}

		readonly IEnumerable<(string ColumnName, string Caption)> availableColumnsForATAV_Export = new[]
		{
			(CusSupportingInfo.Schema.CSI_ReferenceNumber, Res.GetString("6587C89A-0BD2-4420-8C0B-A7F922DF3B6F", "Reference")),
			(CusSupportingInfo.Schema.CSI_LineNo, Res.GetString("6A3906C7-B7D5-42CE-92CA-17281B43FCC4", "Line No.")),
			(PreviousDocument.Schema.Status, Res.GetString("9AEBE41A-5C14-4775-8D78-21E93C6D2F4C", "Entry via ATLAS?")),
			(CusSupportingInfo.Schema.CSI_Description, Res.GetString("A46B308B-E4B4-481B-97B6-DE52B0A32B73", "Information")),
		};

		readonly IEnumerable<(string ColumnName, string Caption)> availableColumnsForATZL_Export = new[]
		{
			(CusSupportingInfo.Schema.CSI_ReferenceNumber, Res.GetString("F2A02267-DCF0-4046-8A9D-8068651991C5", "Reference")),
			(CusSupportingInfo.Schema.CSI_LineNo, Res.GetString("14779BC4-8FA6-412A-8B89-4E4EDBD98E13", "Line No.")),
			(PreviousDocument.Schema.FormattedTariff, Res.GetString("47A75012-3726-43B8-8ED8-47C2A4C74CB4", "Commodity Code")),
			(PreviousDocument.Schema.Status, Res.GetString("9BD83DBB-E70E-48B3-8254-F86EF20C71A8", "Entry via ATLAS?")),
			(PreviousDocument.Schema.UsualProcessingFlag, Res.GetString("8359B961-F69E-41B9-B03D-823B213A8E4E", "Usual Processing Flag")),
			(CusSupportingInfo.Schema.CSI_Quantity, Res.GetString("3FB5FF99-917A-4CDC-BF58-C8CD1DF7433D", "Commercial Qty.")),
			(CusSupportingInfo.Schema.CSI_UnitOfQuantity, Res.GetString("71CF3EAC-5A8E-42E3-9EAD-7A2262632370", "UQ")),
			(CusSupportingInfo.Schema.CSI_Quantity2, Res.GetString("921114B0-E0A4-430D-B920-9795B196AA35", "Debit Qty.")),
			(CusSupportingInfo.Schema.CSI_UnitOfQuantity2, Res.GetString("601C4D1D-3622-42FE-A01A-82825AF33CF5", "UQ")),
			(CusSupportingInfo.Schema.CSI_Description, Res.GetString("1B26E1D2-A6A5-40F9-96C8-D870FF76D782", "Complementary Info")),
		};

		#endregion

		#region Available Fields For Import

		public string[] GetAvailableFieldsFromProcedureCode(ZString procedureCode)
		{
			switch (procedureCode)
			{
				case PreviousProcedureList.Codes._ATAV:
					return availableFieldsForATAV;
				case PreviousProcedureList.Codes._ATZL:
					return availableFieldsForATZL;
				default:
					return System.Array.Empty<string>();
			}
		}

		readonly string[] availableFieldsForATZL = new[]
		{
			PreviousDocument.Schema.AuthorizationNumber,
			PreviousDocument.Schema.CSI_ReferenceNumber2
		};

		readonly string[] availableFieldsForATAV = new[]
		{
			PreviousDocument.Schema.AuthorizationNumber,
			PreviousDocument.Schema.CSI_CustomsOffice,
			PreviousDocument.Schema.SimplifiedGrantAuthorizationFlag,
			PreviousDocument.Schema.CSI_SubType
		};
		#endregion

		public string[] GetUnavailableColumns(IEnumerable<string> availableColumns)
		{
			return !availableColumns.Any() ? allColumns : allColumns.Except(availableColumns).ToArray();
		}

		readonly string[] allColumns = new[]
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
	}
}
