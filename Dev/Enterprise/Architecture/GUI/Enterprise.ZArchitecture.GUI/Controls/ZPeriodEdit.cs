using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Environment;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(true)]
	public class ZPeriodEdit : ZTextBox, IPastableControl
	{
		public ZPeriodEdit()
		{
			MaxLength = 6;
			InitializeComponent();

			Hotkeys.RegisterHotKey(Keys.F5, () =>
			{
				var env = EnvProxy.Instance;
				Text = env.CurrentCompany.GetPeriod(env.Time.CurrentLocalDateTime).ToString(CultureInfo.CurrentCulture);
			}, Res.GetString("5600a0e2-c985-478b-8fff-ff8be3188918", "Insert company period"));
		}

		public override bool ShouldAggressivelyTruncateText
		{
			get
			{
				return false;
			}
		}

		#region ZInt Support

		protected virtual void Parse(ConvertEventArgs e)
		{
			var s = e.Value != null ? e.Value.ToString() : "";
			if (s.Length != 4 && s.Length != 6)
			{
				e.Value = ZInt.Zero;
			}
			else
			{
				var shouldParse = true;

				if (s.Length == 4)
				{
					string fourDigitYear;
					if (TryConvertTwoDigitYearToFourDigitYear(s.Substring(0, 2), out fourDigitYear))
					{
						s = fourDigitYear + s.Substring(2, 2);
					}
					else
					{
						shouldParse = false;
					}
				}

				if (shouldParse)
				{
					int i;
					e.Value = int.TryParse(s, NumberStyles.None, null, out i) ? new ZInt(i) : ZInt.Zero;
				}
				else
				{
					e.Value = ZInt.Zero;
				}
			}

			if (e.DesiredType == typeof(ZString) && !(e.Value is ZString))
			{
				e.Value = new ZString(e.Value.ToString());
				if (e.Value.ToString() == "0")
				{
					e.Value = new ZString("");
				}
			}
		}

		protected override Type DataSourceType
		{
			get { return typeof(ZInt); }
		}

		protected virtual void Format(ConvertEventArgs e)
		{
			if (e.Value is ZInt)
			{
				var value = (ZInt)e.Value;
				if (value == new ZInt(0))
				{
					e.Value = "";
				}
				else
				{
					e.Value = value.ToString();
				}
			}
			else if (e.Value is ZString && e.DesiredType == typeof(string))
			{
				e.Value = e.Value.ToString();
				if (e.Value.ToString() == "0")
				{
					e.Value = "";
				}
			}
			else
			{
				e.Value = "";
			}
		}

		bool TryConvertTwoDigitYearToFourDigitYear(string inputYear, out string fourDigitYear)
		{
			int year;
			if (int.TryParse(inputYear, NumberStyles.None, null, out year))
			{
				var minYear = ZDateTime.Today.AddYears(-50).Year;
				var maxYear = ZDateTime.Today.AddYears(49).Year;

				var minCentury = minYear.ToString().Substring(0, 2);
				var maxCentury = maxYear.ToString().Substring(0, 2);

				var lastCenturyDate = int.Parse(minCentury + inputYear, NumberStyles.None);
				if (lastCenturyDate >= minYear && lastCenturyDate <= maxYear)
				{
					fourDigitYear = minCentury + inputYear;
					return true;
				}
				else
				{
					fourDigitYear = maxCentury + inputYear;
					return true;
				}
			}
			else
			{
				fourDigitYear = "";
				return false;
			}
		}

		#endregion

		#region Key Handling

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			const uint Backspace = 8;
			const uint CarriageReturn = 13;

			var isValidChar = ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == Backspace || e.KeyChar == CarriageReturn);
			e.Handled = !isValidChar;

			base.OnKeyPress(e);
		}

		#endregion

		#region Size

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Size Size
		{
			get { return base.Size; }
		}

		#endregion

		#region IPastableControl Members

#if !WINZOR

		bool IPastableControl.TryPaste()
		{
			var dataObject = SafeClipboard.GetDataObject();
			if (dataObject != null)
			{
				var value = (string)dataObject.GetData(typeof(string));
				if (value != null)
				{
					int i;

					if (int.TryParse(value, NumberStyles.None, null, out i))
					{
						Text = value;
						return true;
					}
				}
			}

			return false;
		}

#endif

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			var textBinding = DataBindings["Text"];
			if (textBinding != null)
			{
				textBinding.Format += ZPeriodEdit_Format;
				textBinding.Parse += ZPeriodEdit_Parse;
			}
		}

		void ZPeriodEdit_Format(object sender, ConvertEventArgs e)
		{
			Format(e);
		}

		void ZPeriodEdit_Parse(object sender, ConvertEventArgs e)
		{
			Parse(e);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Component Designer generated code

		private Container components;

		private void InitializeComponent()
		{
			components = new Container();
		}
		#endregion
	}
}
