using System;
using System.Data;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportSummaryLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public AccComplianceReportSummaryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public AccComplianceReportSummaryLine() : base()
		{
		}

		public abstract class Schema
		{
			public const string PK = "PK";
			public const string ColumnNumber = "ColumnNumber";
			public const string ColumnName = "ColumnName";
			public const string ColumnDescription = "ColumnDescription";
			public const string Value = "Value";
			public const string Adjustment = "Aggiustamento";
			public const string ReasonCode = "ReasonCode";
			public const string Comment = "Comment";
			public const string Total = "Total";
		}

		public static ZDataTable GetDataTable(AccComplianceReport report)
		{
			var result = new ZDataTable();
			result.Columns.Add(Schema.PK, typeof(Guid));
			result.Columns.Add(Schema.ColumnNumber, typeof(int));
			result.Columns.Add(Schema.ColumnName, typeof(string));
			result.Columns.Add(Schema.ColumnDescription, typeof(string));
			result.Columns.Add(Schema.Value, typeof(decimal));
			result.Columns.Add(Schema.Adjustment, typeof(decimal));
			result.Columns.Add(Schema.ReasonCode, typeof(string));
			result.Columns.Add(Schema.Comment, typeof(string));
			result.Columns.Add(Schema.Total, typeof(decimal));

			return result;
		}

		protected DataRow Row
		{
			get { return ((IBusinessObjectInternals)this).Row; }
		}

		#region Properties

		public ZInt ColumnNumber
		{
			get { return new ZInt(Row[Schema.ColumnNumber]); }
		}

		public ZString ColumnName
		{
			get { return new ZString(Row[Schema.ColumnName]); }
		}

		public ZString ColumnDescription
		{
			get { return new ZString(Row[Schema.ColumnDescription]); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Value
		{
			get { return new ZDecimal(Row[Schema.Value]); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Adjustment
		{
			get { return new ZDecimal(Row[Schema.Adjustment]); }
		}

		public ZString ReasonCode
		{
			get { return new ZString(Row[Schema.ReasonCode]); }
		}

		public ZString Comment
		{
			get { return new ZString(Row[Schema.Comment]); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Total
		{
			get { return new ZDecimal(Row[Schema.Total]); }
		}

		#endregion
	}
}
