using System.Collections.Generic;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI;

public sealed class SupportingDocumentsGridColumnStylesHelper
{
	public string[] GetColumnStyles(JobDeclaration declaration)
	{
		if (declaration is null || !declaration.IsUCC6)
		{
			return GetNonUCC6ColumnsStyles();
		}

		if (declaration.IsImport)
		{
			return GetImportColumnStyles();
		}

		return GetExportUCC6ColumnStyles();
	}

	public string[] GetColumnStylesForPartPivot()
	{
		return new ColumnStylesBuilder()
			.IncludeSharedColumns()
			.IncludeUCC6Columns()
			.IncludeExportUCC6Columns()
			.IncludeNonUCC6Columns()
			.Build();
	}

	#region Implementation

	string[] GetImportColumnStyles()
	{
		return new ColumnStylesBuilder()
			.IncludeSharedColumns()
			.IncludeUCC6Columns()
			.Build();
	}

	string[] GetExportUCC6ColumnStyles()
	{
		return new ColumnStylesBuilder()
			.IncludeSharedColumns()
			.IncludeUCC6Columns()
			.IncludeExportUCC6Columns()
			.Build();
	}

	string[] GetNonUCC6ColumnsStyles()
	{
		return new ColumnStylesBuilder()
			.IncludeSharedColumns()
			.IncludeNonUCC6Columns()
			.Build();
	}

	sealed class ColumnStylesBuilder
	{
		readonly List<string> columnStyles = new List<string>();

		public ColumnStylesBuilder IncludeSharedColumns()
		{
			columnStyles.Add(SupportingDocument.Schema.CSI_Code);
			columnStyles.Add(SupportingDocument.Schema.CSI_ReferenceNumber);
			columnStyles.Add(SupportingDocument.Schema.CSI_Quantity);
			columnStyles.Add(SupportingDocument.Schema.CSI_UnitOfQuantity);
			columnStyles.Add(SupportingDocument.Schema.CSI_YearOfIssue);
			columnStyles.Add(SupportingDocument.Schema.CSI_RN_NKCountryCode);
			return this;
		}

		public ColumnStylesBuilder IncludeUCC6Columns()
		{
			columnStyles.Add(SupportingDocument.Schema.CSI_DateOfExpiry);
			columnStyles.Add(SupportingDocument.Schema.CSI_ReferenceNumber2);
			columnStyles.Add(SupportingDocument.Schema.CSI_Value);
			columnStyles.Add(SupportingDocument.Schema.CSI_RX_NKCurrency);
			return this;
		}

		public ColumnStylesBuilder IncludeExportUCC6Columns()
		{
			columnStyles.Add(SupportingDocument.Schema.CSI_LineNo);
			return this;
		}

		public ColumnStylesBuilder IncludeNonUCC6Columns()
		{
			columnStyles.Add(SupportingDocument.Schema.CSI_Status);
			return this;
		}

		public string[] Build() => columnStyles.ToArray();
	}

	#endregion
}
