using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ReportWriter
{
	public class DocumentHeaderRow : RowData
	{
		public DocumentHeaderRow(ReportBizObj parent)
			: base(parent)
		{
		}

		#region Schema
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : AutoRowData.Schema
		{
			public const string IsCustomisedColumnRow = "IsCustomisedColumnRow";
			public const string BackgroundColorInArgb = "BackgroundColorInArgb";
		}
		#endregion

		#region IsCustomisedColumnRow
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:ReportWriter.DocumentHeaderRow|IsCustomisedColumnRow", Caption = "Is Customized Column Row")]
		public ZBool IsCustomisedColumnRow
		{
			get { return isCustomisedColumnRow; }
			set
			{
				var oldValue = IsCustomisedColumnRow;
				SetNonPersistentPropertyValue(IsCustomisedColumnRowInfo, ref isCustomisedColumnRow, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateIsCustomisedColumnRow();
				}
				if (!IsCopying && oldValue != IsCustomisedColumnRow)
				{
					Columns.RefreshBinding();
				}
			}
		}
		ZBool isCustomisedColumnRow;

		public ZPropertyInfo IsCustomisedColumnRowInfo
		{
			get { return GetZPropertyInfo(Schema.IsCustomisedColumnRow); }
		}

		#endregion

		#region BackgroundColorInArgb
		[ReadOnlyMember(nameof(BackgroundColorInArgb_ReadOnly))]
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:ReportWriter.DocumentHeaderRow|BackgroundColorInArgb", MediumCaption = "Background Color In ARGB", Caption = "Customized Column Background Color In ARGB")]
		public virtual ZInt BackgroundColorInArgb
		{
			get
			{
				return backgroundColorInArgb;
			}
			set
			{
				SetNonPersistentPropertyValue(BackgroundColorInArgbInfo, ref backgroundColorInArgb, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBackgroundColorInArgb();
				}
			}
		}
		ZInt backgroundColorInArgb;

		public ZPropertyInfo BackgroundColorInArgbInfo
		{
			get { return this.GetZPropertyInfo(Schema.BackgroundColorInArgb); }
		}

		bool BackgroundColorInArgb_ReadOnly
		{
			get { return !IsCustomisedColumnRow; }
		}
		#endregion

		public new DocumentHeaderColumnCollection Columns
		{
			get { return (DocumentHeaderColumnCollection)base.Columns; }
		}

		protected override ColumnDataCollection GetNewColumnDataCollection()
		{
			return new DocumentHeaderColumnCollection(this);
		}

		public new DocumentHeaderRowValidation Validation
		{
			get { return (DocumentHeaderRowValidation)base.Validation; }
		}

		protected override RowDataValidation GetNewValidation()
		{
			return new DocumentHeaderRowValidation(this);
		}
	}
}
