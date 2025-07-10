using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Supports 2 digit hours (ie format hh:mm)
	/// </summary>
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	public class ZTimeTimeEdit : ZTextBox, IGridControl
	{
		public ZTimeTimeEdit()
		{
			Text = "";
			UpdateWidth();
			FontChanged += ZTimeTimeEdit_FontChanged;
		}

		void ZTimeTimeEdit_FontChanged(object sender, EventArgs e)
		{
			fFixedWidth = -1;
			UpdateWidth();
		}

		public override bool ShouldAggressivelyTruncateText
		{
			get
			{
				return false;
			}
		}

		[DefaultValue(false)]
		public bool AllowNegative
		{
			get { return Core.AllowNegative; }
			set { Core.AllowNegative = value; }
		}

		internal ZString NonSelectedText
		{
			get
			{
				ZString result = Text;
				if (SelectionLength > 0)
				{
					result = result.Substring(0, SelectionStart) + result.SubstringSafe(SelectionStart + SelectionLength);
				}
				return result;
			}
		}

		#region Text, MaxLength

		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength", Enabled = false)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override int MaxLength
		{
			get { return 5; }
			set { throw new NotSupportedException("ZTimeTimeEdit.MaxLength should not be set manually."); }
		}

		#endregion

		#region Parse / Format

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);

			if (!ReadOnly && Text == Core.EmptyText)
			{
				Text = "";
			}

			SelectAll(); // req'd for grid
		}

		void Parse(object sender, ConvertEventArgs e)
		{
			Core.InputText = e.Value.ToString();
			e.Value = Core.GetTimeFromText(Core.InputText);
		}

		void Format(object sender, ConvertEventArgs e)
		{
			var value = e.Value.ToString();
			e.Value = string.IsNullOrEmpty(value) ? Core.EmptyText : value;
		}

		#endregion

		#region Core

		internal ZTimeTimeEditCore Core
		{
			get
			{
				if (fCore == null)
				{
					fCore = GetNewCore();
				}
				return fCore;
			}
		}

		internal virtual ZTimeTimeEditCore GetNewCore()
		{
			return new ZTimeTimeEditCore(this);
		}

		ZTimeTimeEditCore fCore;

		#endregion

		#region Control Size

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			UpdateWidth();
		}

		protected void UpdateWidth()
		{
			if (!IsOnGrid && this.Width != FixedWidth)
			{
				ControlDpiScalingHelper.SetWidth(this, FixedWidth, false);
			}
		}

		protected int FixedWidth
		{
			get
			{
				if (fFixedWidth == -1)
				{
					using (var g = this.CreateGraphics())
					{
						const int Padding = 4;

						var controlSize = g.MeasureString(EmptyZeros, Font);
						fFixedWidth = (int)controlSize.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(Padding);
					}
				}

				return fFixedWidth;
			}
			set { fFixedWidth = value; }
		}

		protected virtual string EmptyZeros
		{
			get { return "00:00"; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool IsOnGrid
		{
			get { return base.IsOnGrid; }
			set
			{
				base.IsOnGrid = value;
				this.BorderStyle = IsOnGrid ? BorderStyle.None : BorderStyle.Fixed3D;
			}
		}

		int fFixedWidth = -1;

		#endregion

		#region Preventing Double-Tabbing Column Skipping Bug

		protected const int TabKey = 9;
		protected override bool ProcessKeyMessage(ref Message m)
		{
			return IsTabbingThroughGridColumn(ref m) || base.ProcessKeyMessage(ref m);
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			return IsTabbingThroughGridColumn(ref m) || base.ProcessKeyPreview(ref m);
		}

		protected bool IsTabbingThroughGridColumn(ref Message m)
		{
			var keyCode = (int)m.WParam;
			return (IsOnGrid && keyCode == TabKey);
		}

		#endregion

		#region PropertyDescriptors

		public new static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZTimeTimeEdit>()
				.Property("Text", "") // Property name
				.Property("ReadOnly", false)
				.Property("ReadOnlyForBinding", false, false)
				.Property("ReadOnlyForBindingIsNull", false, false)
				.Property("MaxLength", 0)
				.Property("IsVisibleForBinding", ZBool.True)
				.Result;
		}

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				var binding = DataBindings[nameof(Text)];
				binding.Parse -= Parse;
				binding.Parse += Parse;
				binding.Format -= Format;
				binding.Format += Format;

				if (IsHandleCreated)
				{
					binding.ReadValue();
				}
			}
		}

		protected override Type DataSourceType
		{
			get { return typeof(ZTime); }
		}

		#endregion

		#region IGridControl Members

		int IGridControl.ButtonWidth
		{
			get { return 0; }
		}

		void IGridControl.ActivateEditControl()
		{
			Focus();
		}

		bool IGridControl.ShownForReadOnly
		{
			get { return false; }
		}

		protected internal virtual int HoursDigitCount => 2;

		#endregion
	}
}
