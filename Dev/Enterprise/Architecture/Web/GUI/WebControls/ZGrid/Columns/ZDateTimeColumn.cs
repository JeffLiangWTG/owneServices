using System.ComponentModel;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Column to present DateTime values in the grid
	/// Proper implementation should involve inheriting from Template column
	/// </summary>
	public class ZDateTimeColumn : ZTemplateColumn, IExcelExportCustomValue
	{
		public ZDateTimeColumn(string headerText, string bindTo)
			: base(headerText, bindTo)
		{
			NoWrap = true;
		}

		public ZDateTimeColumn(string headerText, string bindTo, ZDateTimePickerFormat dateFormat)
			: this(headerText, bindTo)
		{
			this.DateTimeFormat = dateFormat;
		}

		public bool CanBeEnabledByClient { get; set; }

		protected internal override ITemplate GetItemTemplate()
		{
			return GetItemTemplateCore();
		}

		protected virtual ITemplate GetItemTemplateCore()
		{
			return new ZDateTimeColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return GetEditItemTemplateCore();
		}

		protected virtual ITemplate GetEditItemTemplateCore()
		{
			return new ZDateTimeColumnEditItemTemplate(this);
		}

		[DefaultValue(ZDateTimePickerFormat.Short)]
		public ZDateTimePickerFormat DateTimeFormat
		{
			get { return fDateTimeFormat; }
			set { fDateTimeFormat = value; }
		}
		ZDateTimePickerFormat fDateTimeFormat = ZDateTimePickerFormat.Short;

		#region IExcelExportCustomValue Members

		public IZType GetCustomValue(BusinessObject bizObj)
		{
			IZType result = GetCustomValueCore(bizObj);
			if (!result.IsEmpty && result is ZDateTime && (ZDateTime)result == SuppressUtil.SuppressedDateTime)
			{
				result = SuppressUtil.SuppressedText;
			}
			return result;
		}

		public ZString GetValueFormat(IZType value)
		{
			ZString result;

			if (value is ZDateTime && !value.IsEmpty && (ZDateTime)value == SuppressUtil.SuppressedDateTime)
			{
				result = "";
			}
			else if (DateTimeFormat == ZDateTimePickerFormat.Short)
			{
				result = ZDateTime.ShortDateFormat;
			}
			else if (DateTimeFormat == ZDateTimePickerFormat.Time)
			{
				result = ZDateTime.ShortTimeFormat;
			}
			else if (DateTimeFormat != ZDateTimePickerFormat.TimeUpTo999HoursAnd45Minutes)
			{
				result = ZDateTime.LongTimeFormat;
			}

			return result;
		}

		public ZString GetDescription()
		{
			string description = this.HeaderText;
			if (char.IsControl(description[description.Length - 1])) // remove bad chars at the end of the string
			{
				description = description.Remove(description.Length - 2, 2);
			}
			return description;
		}

		protected virtual IZType GetCustomValueCore(BusinessObject bizObj)
		{
			return bizObj[this.BindTo] as IZType ?? ZDateTime.Empty;
		}

		#endregion

	}
}
