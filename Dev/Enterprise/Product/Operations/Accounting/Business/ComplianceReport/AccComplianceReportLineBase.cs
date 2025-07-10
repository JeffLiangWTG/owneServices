using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportLineBase : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AccComplianceReportLineBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public AccComplianceReportLineBase()
			: base()
		{
		}

		public abstract class Schema
		{
			public const string PK = "PK";
			public const string GoodsExTaxAmount = "GoodsExTaxAmount";
			public const string GoodsTaxAmount = "GoodsTaxAmount";
			public const string ServiceExTaxAmount = "ServiceExTaxAmount";
			public const string ServiceTaxAmount = "ServiceTaxAmount";
			public const string TotalExTaxAmount = "TotalExTaxAmount";
			public const string TotalTaxAmount = "TotalTaxAmount";
			public const string GeneralLedgerAmountDR = "GeneralLedgerAmountDR";
			public const string GeneralLedgerAmountCR = "GeneralLedgerAmountCR";
			public const string TaxRecoverableAmount = "TaxRecoverableAmount";
			public const string TaxNotRecoverableAmount = "TaxNotRecoverableAmount";
			public const string TaxReverseChargeAmount = "TaxReverseChargeAmount";
			public const string TaxReverseChargeInputAmount = "TaxReverseChargeInputAmount";
			public const string TaxReverseChargeOutputAmount = "TaxReverseChargeOutputAmount";
			public const string PreCalculatedAmount = "PreCalculatedAmount";
			public const string Comment = "Comment";
			public const string LineNum = "LineNum";
		}

		public static ZDataTable GetDataTable(AccComplianceReport report)
		{
			var result = new ZDataTable();
			result.Columns.AddRange(report.GetReportTotalLineColumns().ToArray());
			return result;
		}

		protected DataRow Row => ((IBusinessObjectInternals)this).Row;

		#region Properties

		public ZDecimal GoodsExTaxAmount => new ZDecimal(GetColumnValueSafe(Schema.GoodsExTaxAmount));

		public ZDecimal GoodsTaxAmount => new ZDecimal(GetColumnValueSafe(Schema.GoodsTaxAmount));

		public ZDecimal ServiceExTaxAmount => new ZDecimal(GetColumnValueSafe(Schema.ServiceExTaxAmount));

		public ZDecimal ServiceTaxAmount => new ZDecimal(GetColumnValueSafe(Schema.ServiceTaxAmount));

		public ZDecimal TotalExTaxAmount => new ZDecimal(GetColumnValueSafe(Schema.TotalExTaxAmount));

		public ZDecimal TotalTaxAmount => new ZDecimal(GetColumnValueSafe(Schema.TotalTaxAmount));

		[ResourceStringData("GeneralLedgerAmountDR", Caption = "GL Amount Debit", ShortCaption = "GL DR")]
		public ZDecimal GeneralLedgerAmountDR => new ZDecimal(GetColumnValueSafe(Schema.GeneralLedgerAmountDR));

		[ResourceStringData("GeneralLedgerAmountCR", Caption = "GL Amount Credit", ShortCaption = "GL CR")]
		public ZDecimal GeneralLedgerAmountCR => new ZDecimal(GetColumnValueSafe(Schema.GeneralLedgerAmountCR));

		public ZDecimal TaxRecoverableAmount => new ZDecimal(GetColumnValueSafe(Schema.TaxRecoverableAmount));

		public ZDecimal TaxNotRecoverableAmount => new ZDecimal(GetColumnValueSafe(Schema.TaxNotRecoverableAmount));

		public ZDecimal TaxReverseChargeAmount => new ZDecimal(GetColumnValueSafe(Schema.TaxReverseChargeAmount));

		public ZDecimal TaxReverseChargeInputAmount => TaxReverseChargeAmount;

		public ZDecimal TaxReverseChargeOutputAmount => TaxReverseChargeAmount;

		public ZString Comment => new ZString(GetColumnValueSafe(Schema.Comment));

		#region PreCalculatedAmount

		public ZDecimal PreCalculatedAmount => GetPreCalculatedAmount();

		protected virtual ZDecimal GetPreCalculatedAmount() => new ZDecimal(GetColumnValueSafe(Schema.PreCalculatedAmount));

		#endregion

		public ZInt LineNum => new ZInt(GetColumnValueSafe(Schema.LineNum));

		protected object GetColumnValueSafe(string columnName) => Row.Table.Columns.Contains(columnName) ? Row[columnName] : null;

		#endregion
	}
}

