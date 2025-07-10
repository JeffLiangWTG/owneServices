using System.Collections.Immutable;
using System.Data;
using System.Linq;

using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportLine : AccComplianceReportLineBase
	{
		public AccComplianceReportLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public AccComplianceReportLine() : base()
		{
		}

		public new abstract class Schema : AccComplianceReportLineBase.Schema
		{
			public const string ACL_ReportSequence = "ACL_ReportSequence";
			public const string OH_Code = "OH_Code";
			public const string OH_FullName = "OH_FullName";
			public const string OrgCountryCode = "OrgCountryCode";
			public const string GC_RN_NKCountryCode = "GC_RN_NKCountryCode";
			public const string OK_CustomsRegNo = "OK_CustomsRegNo";
			public const string RepCountryRegNo = "RepCountryRegNo";
			public const string GC_RX_NKLocalCurrency = "GC_RX_NKLocalCurrency";
			public const string AH_PK = "AH_PK";
			public const string AH_Ledger = "AH_Ledger";
			public const string AH_TransactionType = "AH_TransactionType";
			public const string PostDate = "PostDate";
			public const string InvoiceDate = "InvoiceDate";
			public const string AH_TransactionNum = "AH_TransactionNum";
			public const string AH_TransactionReference = "AH_TransactionReference";
			public const string AH_ComplianceSubType = "AH_ComplianceSubType";
			public const string AH_InvoiceAmount = "AH_InvoiceAmount";
			public const string AH_GSTAmount = "AH_GSTAmount";
			public const string SPVTaxAmount = "SPVTaxAmount";
			public const string ReportSubCode = "ReportSubCode";
			public const string AG_AccountNum = "AG_AccountNum";
			public const string AG_Description = "AG_Description";
			public const string GB_Code = "GB_Code";
			public const string GE_Code = "GE_Code";
			public const string AT_Type = "AT_Type";
			public const string AT_Code = "AT_Code";
			public const string AT_ExtraTaxRateType = "AT_ExtraTaxRateType";
			public const string TaxMessage = "TaxMessage";
			public const string AL_A9_VATClass = "AL_A9_VATClass";
			public const string GLAccountPK = "GLAccountPK";
			public const string AL_TaxRate = "AL_TaxRate";
			public const string ComplianceSequence = "ComplianceSequence";
			public const string TaxGroupCode = "TaxGroupCode";
			public const string TaxGroupDescription = "TaxGroupDescription";
		}

		public static new ZDataTable GetDataTable(AccComplianceReport report)
		{
			var result = new ZDataTable();
			result.Columns.AddRange(report.GetReportLineColumns().ToArray());
			return result;
		}

		#region Properties

		public ZInt ACL_ReportSequence => new ZInt(GetColumnValueSafe(Schema.ACL_ReportSequence));

		public ZString OH_Code => new ZString(GetColumnValueSafe(Schema.OH_Code));

		public ZString OH_FullName => new ZString(GetColumnValueSafe(Schema.OH_FullName));

		public ZString OrgCountryCode => new ZString(GetColumnValueSafe(Schema.OrgCountryCode));

		public ZString GC_RN_NKCountryCode => new ZString(GetColumnValueSafe(Schema.GC_RN_NKCountryCode));

		public ZString OK_CustomsRegNo => new ZString(GetColumnValueSafe(Schema.OK_CustomsRegNo));

		public ZString RepCountryRegNo => new ZString(GetColumnValueSafe(Schema.RepCountryRegNo));

		public ZString GC_RX_NKLocalCurrency => new ZString(GetColumnValueSafe(Schema.GC_RX_NKLocalCurrency));

		public ZGuid AH_PK => new ZGuid(GetColumnValueSafe(Schema.AH_PK));

		public ZString AH_Ledger => new ZString(GetColumnValueSafe(Schema.AH_Ledger));

		public ZString AH_TransactionType => new ZString(GetColumnValueSafe(Schema.AH_TransactionType));

		public ZDateTime PostDate => new ZDateTime(GetColumnValueSafe(Schema.PostDate));

		public ZDateTime InvoiceDate => new ZDateTime(GetColumnValueSafe(Schema.InvoiceDate));

		public ZString AH_TransactionNum => new ZString(GetColumnValueSafe(Schema.AH_TransactionNum));

		public ZString AH_TransactionReference => new ZString(GetColumnValueSafe(Schema.AH_TransactionReference));

		public ZString AH_ComplianceSubType => new ZString(GetColumnValueSafe(Schema.AH_ComplianceSubType));

		public ZDecimal AH_InvoiceAmount => new ZDecimal(GetColumnValueSafe(Schema.AH_InvoiceAmount));

		public ZDecimal AH_GSTAmount => new ZDecimal(GetColumnValueSafe(Schema.AH_GSTAmount));

		public ZDecimal AH_LocalTotal => AH_InvoiceAmount + AH_GSTAmount;

		public ZDecimal SPVTaxAmount => new ZDecimal(GetColumnValueSafe(Schema.SPVTaxAmount));

		public ZString ReportSubCode => new ZString(GetColumnValueSafe(Schema.ReportSubCode));

		public ZString AG_AccountNum => new ZString(GetColumnValueSafe(Schema.AG_AccountNum));

		public ZString AG_Description => new ZString(GetColumnValueSafe(Schema.AG_Description));

		public ZString GB_Code => new ZString(GetColumnValueSafe(Schema.GB_Code));

		public ZString GE_Code => new ZString(GetColumnValueSafe(Schema.GE_Code));

		public ZString AT_Type => new ZString(GetColumnValueSafe(Schema.AT_Type));

		public ZString AT_Code => new ZString(GetColumnValueSafe(Schema.AT_Code));

		public ZString AT_ExtraTaxRateType => new ZString(GetColumnValueSafe(Schema.AT_ExtraTaxRateType));

		public ZDecimal AL_TaxRate => new ZDecimal(GetColumnValueSafe(Schema.AL_TaxRate));

		public ZString ComplianceSequence => new ZString(GetColumnValueSafe(Schema.ComplianceSequence));

		public ZString TaxMessage => new ZString(GetColumnValueSafe(Schema.TaxMessage));

		public ZString TaxGroupCode => new ZString(GetColumnValueSafe(Schema.TaxGroupCode));

		[ResourceStringData("41EA9F2D-2FEB-4BB9-9A4C-8765F4707AE0", Caption = "Tax Group")]
		public ZString TaxGroupDescription => !TaxGroupCode.IsEmpty ?
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.Value.OfType<CodeDescriptionBoolRelatedItem>()
			.FirstOrDefault(x => x.Code == TaxGroupCode)?.Description ?? string.Empty : string.Empty;

		public ZGuid AL_A9_VATClass
		{
			get { return new ZGuid(Row[Schema.AL_A9_VATClass]); }
		}

		public ZGuid GLAccountPK
		{
			get { return new ZGuid(Row[Schema.GLAccountPK]); }
		}

		public ZBool IsGoods
		{
			get { return new ZBool(!GoodsExTaxAmount.IsEmpty); }
		}

		#endregion

		#region Calculated Properties

		public ZBool IsService => new ZBool(!ServiceExTaxAmount.IsEmpty);

		public ZBool IsDebitLine => new ZBool(GeneralLedgerAmountDR > 0);

		public ZBool IsCreditLine => new ZBool(GeneralLedgerAmountCR > 0);

		/// <summary>
		/// This property is currently used in the AU PRTS Compliance Report to show pre-calculated Payment Amount stored on the ACQ_ReportSubCode while queuing.
		/// A string representation of the Amount should be the first thing on the ReportSubCode. It could be following by other data  separated by '|' character.
		/// This property can be used for the new types of reports, if necessary.
		/// </summary>
		protected override ZDecimal GetPreCalculatedAmount() => TryParseZDecimal(SplitAndTrim(ReportSubCode).FirstOrDefault());

		/// <summary>
		/// This property is currently used in one of the AU PTRS Compliance Reports to represent a number of days in which Invoice was paid.
		/// It is a string representation of an int value which was pre-calculated while queuing the report.
		/// It is the second element of data in the ReportSubCode separated by the '|' character from the first decimal one.
		/// This property can be used for the new types of reports, if necessary.
		/// </summary>
		public ZInt PreCalculatedCount => TryParseZInt(SplitAndTrim(ReportSubCode).Skip(1).FirstOrDefault());

		/// <summary>
		/// This property is currently used in one of the AU PTRS Compliance Reports to represent a number of days before Due Date in which Invoice was paid.
		/// It is a string representation of an int value which was pre-calculated while queuing the report.
		/// It is the third element of data in the ReportSubCode separated by the '|' character from the previous integer one.
		/// This property can be used for the new types of reports, if necessary.
		/// </summary>
		public ZInt PreCalculatedCount2 => TryParseZInt(SplitAndTrim(ReportSubCode).Skip(2).FirstOrDefault());

		/// <summary>
		/// This property is currently used in one of the AU PTRS Compliance Reports to represent a number of days before Due Date in which Invoice was paid.
		/// It is a string representation of an int value which was pre-calculated while queuing the report.
		/// It is the fourth element of data in the ReportSubCode separated by the '|' character from the previous integer one.
		/// This property can be used for the new types of reports, if necessary.
		/// </summary>
		public ZInt PreCalculatedCount3 => TryParseZInt(SplitAndTrim(ReportSubCode).Skip(3).FirstOrDefault());

		#region DaysRange

		/// <summary>
		/// This property is currently used in one of the AU PTRS Compliance Reports to represent a reported range of number of days in which Invoice was paid.
		/// It is based on the PreCalculatedCount, which potentially could be used for other purpose thana number of days. 
		/// </summary>
		public ZString DaysRange
		{
			get
			{
				var days = PreCalculatedCount;

				if (days < 0 || days == int.MaxValue)
				{
					return ZString.Empty;
				}
				else if (days <= 20)
				{
					return DaysRange0_20;
				}
				else if (days <= 30)
				{
					return DaysRange21_30;
				}
				else if (days <= 60)
				{
					return DaysRange31_60;
				}
				else if (days <= 90)
				{
					return DaysRange61_90;
				}
				else if (days <= 120)
				{
					return DaysRange91_120;
				}
				else
				{
					return DaysRange121Plus;
				}
			}
		}

		/// <summary>
		/// This property is currently used in one of the AU PTRS Compliance Reports 2024 to represent a reported range of number of days in which Invoice was paid.
		/// It is based on the PreCalculatedCount, which potentially could be used for other purpose thana number of days. 
		/// </summary>
		public ZString DaysRange30_60
		{
			get
			{
				var days = PreCalculatedCount;

				if (days < 0 || days == int.MaxValue)
				{
					return ZString.Empty;
				}
				else if (days <= 30)
				{
					return DaysRange0_30;
				}
				else if (days <= 60)
				{
					return DaysRange31_60;
				}
				else
				{
					return DaysRange61Plus;
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		// Use static ZString to save memory. No way to use ResString with static.
		static readonly ZString DaysRange0_20 = new ZString("0-20 Days");
		static readonly ZString DaysRange21_30 = new ZString("21-30 Days");
		static readonly ZString DaysRange31_60 = new ZString("31-60 Days");
		static readonly ZString DaysRange61_90 = new ZString("61-90 Days");
		static readonly ZString DaysRange91_120 = new ZString("91-120 Days");
		static readonly ZString DaysRange121Plus = new ZString("121+ Days");

		// PTRS 2024 uses different ranges with 30 and 60 days key points
		static readonly ZString DaysRange0_30 = new ZString("0-30 Days");
		static readonly ZString DaysRange61Plus = new ZString("61+ Days");

		#endregion

		#endregion

		#region PTRS 2024 report Small Business and Full Payment flags (TCP dataset)

		const int NotSmallBusinessFullyPaid = 1;
		const int SmallBusinessPartiallyPaid = 2;
		const int SmallBusinessFullyPaid = 3;

		public ZBool IsSmallBusiness => PreCalculatedCount == SmallBusinessPartiallyPaid || PreCalculatedCount == SmallBusinessFullyPaid;

		public ZBool IsFullyPaid => PreCalculatedCount == NotSmallBusinessFullyPaid || PreCalculatedCount == SmallBusinessFullyPaid;

		#endregion

		static ZDecimal TryParseZDecimal(ZString input) => StartsWithApplicableCharacter(input) && ZDecimal.TryParse(input, out var decimalValue)
			? decimalValue
			: ZDecimal.Zero;

		static ZInt TryParseZInt(ZString input) => StartsWithApplicableCharacter(input) && ZInt.TryParse(input, out var intValue)
			? intValue
			: ZInt.Zero;

		static ZString[] SplitAndTrim(ZString input) => input.Split('|').Select(x => x.Trim()).ToArray();

		static bool StartsWithApplicableCharacter(ZString input) => !input.IsEmpty && ApplicableCharacters.Contains(input[0]);

		static readonly ImmutableArray<char> ApplicableCharacters = ImmutableArray.Create('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '+', '.');

		#endregion
	}
}
