using System;
using System.ComponentModel;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[Designer(typeof(Designer))]
	[ToolboxItem(true)]
	[DefaultBindingProperty("DateTimeOffsetValue")]
	[FrontMostControlProvider(typeof(ZDateEditFrontMostControlProvider))]
	public class ZDateTimeOffsetEdit : ZDateEdit
	{
		#region Bare

		[ToolboxItem(false)]
		public new class Bare : ZDateTimeOffsetEdit
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		public ZDateTimeOffsetEdit() : base()
		{
			DateTimeFormat = ZDateTimePickerFormat.Long;
		}

		[DefaultValue(false)]
		public bool HasTimeZoneFindBox { get; set; }

		#region DateTimeOffsetValue

		public override ZDateTime DateTimeValue
		{
			get { return DateTimeOffsetValue.ToZDateTime(); }
			set { DateTimeOffsetValue = new ZDateTimeOffset(value); }
		}

		[DefaultValue("")]
		[Bindable(true)]
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		public virtual ZDateTimeOffset DateTimeOffsetValue
		{
			get
			{
				if (pushValueRequired)
				{
					PushValue();
				}
				return dateTimeOffsetValue;
			}
			set
			{
				Text = DateTimeOffsetToString(value);
				lastParsedValue = Text;
				dateTimeOffsetValue = value;
			}
		}
		ZDateTimeOffset dateTimeOffsetValue;

		protected override void ResetTimeValueIfNeeded()
		{
			base.ResetTimeValueIfNeeded();
			DateTimeOffsetValue = dateTimeOffsetValue;
		}

		public string DateTimeOffsetToString(ZDateTimeOffset? date, bool hideOffset = false)
		{
			if (!date.HasValue)
			{
				return string.Empty;
			}

			string formatStringCell;
			if (DateTimeFormat == ZDateTimePickerFormat.Short)
			{
				formatStringCell = DateTimeFormatStrings.ShortDateFormat;
			}
			else if (hideOffset)
			{
				formatStringCell = DateTimeFormatStrings.LongTimeFormat;
			}
			else if(DataRegistry.Instance.DisplayUtcOffset)
			{
				formatStringCell = DateTimeFormatStrings.LongTimeFormatIncludingGMT;
			}
			else
			{
				formatStringCell = DateTimeFormatStrings.LongTimeFormat;
			}
			return date.Value.ToString(formatStringCell, CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture);
		}

		public event EventHandler DateTimeOffsetValueChanged;

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			pushValueRequired = true;
			if (DateTimeOffsetValueChanged != null)
			{
				DateTimeOffsetValueChanged(this, e);
			}
		}

		protected internal override void PushValue()
		{
			if (!DateTextBox.Text.Equals(lastParsedValue, StringComparison.OrdinalIgnoreCase))
			{
				dateTimeOffsetValue = Core.StringToDateTimeOffset(DateTextBox.Text); 
				lastParsedValue = Text;
			}
		}

		#endregion

		public new static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZDateTimeOffsetEdit>()
				.Property(nameof(DateTimeOffsetValue), ZDateTimeOffset.Empty)
				.Property(nameof(ReadOnly), true)
				.Property(nameof(IsVisibleForBinding), ZBool.True)
				.Property("ReadOnlyForBinding", true) //Can't use nameof since it is marked Obsolete.
				.Property("ReadOnlyForBindingIsNull", true, false) //Can't use nameof since it is marked Obsolete.
				.Result;
		}

		protected override void SetDateTimeSelected(ZPopupCalendar.DateTimeSelectedEventArgs e)
		{
			SetDateTimeOffsetSelected((ZPopupCalendar.DateTimeOffsetSelectedEventArgs)e);
		}

		protected void SetDateTimeOffsetSelected(ZPopupCalendar.DateTimeOffsetSelectedEventArgs e)
		{
			DateTimeOffsetValue = new ZDateTimeOffset(e.Value, e.OffsetValue);
		}

		protected override void OnBeforeShowingPopupCalendar()
		{
			base.OnBeforeShowingPopupCalendar();

			Calendar.ReturnsDateTimeOffset = true;
			Calendar.HasTimeZoneFindBox = HasTimeZoneFindBox;
		}

		protected internal override DateTime? GetDateTimeForPopupCalendarCore()
		{
			return DateTimeOffsetValue.IsValid ? DateTimeOffsetValue.ToDateTime() : null;
		}

		protected internal override TimeSpan? GetOffsetForPopupCalendar()
		{
			return DateTimeOffsetValue.IsValid ? DateTimeOffsetValue.Offset : Env.Time.GetUtcOffsetBasedOnUtc(Env.CurrentBranch.NKUNLOCO, Env.Time.CurrentUtcDateTime);
		}
	}
	//This class is tested in ZDateEdit.cs.
}
