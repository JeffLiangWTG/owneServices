using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="System.Windows.Forms.CheckBox"/></summary>
	[DesignTimeControlNameGenerator("chk")]
	[DefaultBindingProperty("Checked")]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class KCheckBox : System.Windows.Forms.CheckBox, IVariableLengthCaptionRenderer
	{
		const int DefaultDpiIndependantHeight = 24;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "created")]
		public KCheckBox()
		{
			// The default value is always 24, even on different DPIs.
			if (Height == ControlDpiScalingHelper.MarkAsScaled(DefaultDpiIndependantHeight))
			{
				ControlDpiScalingHelper.SetHeight(this, DefaultDpiIndependantHeight, true);
			}

			CaptionRenderer = new ButtonBaseVariableLengthCaptionRenderer(this);
		}

		[SmartTagVisible]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		#region Checked

		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		[BindingOptions(UpdateDataSourceOnPropertyChange = true)]
		[DefaultValue(false)]
		public new bool Checked
		{
			get { return base.Checked; }
			set { base.Checked = value; }
		}

		#endregion

		#region Binding ReadOnly

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBinding
		{
			get { return ReadOnlyForBindingProperty.ReadOnlyForBinding; }
			set { ReadOnlyForBindingProperty.ReadOnlyForBinding = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		public event EventHandler ReadOnlyForBindingChanged
		{
			add { ReadOnlyForBindingProperty.ReadOnlyForBindingChanged += value; }
			remove { ReadOnlyForBindingProperty.ReadOnlyForBindingChanged -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBindingIsNull
		{
			get { return false; }
			set
			{
				if (value)
				{
					ReadOnlyForBinding = true;
				}
			}
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			ReadOnlyForBindingProperty.OnReadOnlyChanged();
		}

		protected virtual ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
						this,
						delegate
						{ return !Enabled; },
						delegate(bool value)
						{ Enabled = !value; });
				}
				return readOnlyForBindingProperty;
			}
		}
		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;

		#endregion

		#region IVariableLengthCaptionRenderer Members

		string[] IVariableLengthCaptionRenderer.Captions
		{
			get { return Captions; }
			set { Captions = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected string[] Captions
		{
			get { return CaptionRenderer.Captions; }
			set
			{
				CaptionRenderer.Captions = value;
				CaptionRenderer.HasAutoSetToolTip = true;
			}
		}

		bool IVariableLengthCaptionRenderer.IsCaptionOverridden
		{
			get { return IsCaptionOverridden; }
			set { IsCaptionOverridden = value; }
		}

		protected bool IsCaptionOverridden
		{
			get { return CaptionRenderer.IsCaptionOverridden; }
			set { CaptionRenderer.IsCaptionOverridden = value; }
		}

		protected bool ShouldSerializeText() // this will only work if the Text property is overridden in this class
		{
			// has the developer set the text property manually
			return CaptionRenderer.IsCaptionOverridden && !string.IsNullOrEmpty(Text);
		}

		readonly ButtonBaseVariableLengthCaptionRenderer CaptionRenderer;

		#endregion
	}
}
