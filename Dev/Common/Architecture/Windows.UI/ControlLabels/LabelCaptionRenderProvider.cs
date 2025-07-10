using System.ComponentModel;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// An IExtenderProvider component that allows LabelCaptionRenderer extensions to be
	/// configured at design time.
	/// <see>CargoWise.Windows.UI.LabelCaptionRenderer</see>
	/// </summary>
	[ProvideProperty("LabelCaptionAlignment", typeof(Control))]
	[ProvideProperty("LabelCaptionVisible", typeof(Control))]
	[ProvideProperty("LabelTop", typeof(Control))]
	public class LabelCaptionRenderProvider : Component, IExtenderProvider
	{
		#region LabelCaptionAlignment

		/// <summary>
		/// Get the alignment where the label caption will be rendered.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		[Category(DesignerConstants.Category)]
		[Description("The alignment where the label caption will be rendered.")]
		[Browsable(true)]
		//[DefaultValue(LabelCaptionAlignment.Default)] // handled in ShouldSerializeLabelCaptionAlignment
		public LabelCaptionAlignment GetLabelCaptionAlignment(Control control)
		{
			ILabelCaptionRenderer renderer = GetRenderer(control);
			return renderer == null ? LabelCaptionAlignment.Default : renderer.Alignment;
		}

		/// <summary>
		/// Set the alignment where the label caption will be rendered.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void SetLabelCaptionAlignment(Control control, LabelCaptionAlignment value)
		{
			ILabelCaptionRenderer renderer = GetRenderer(control);
			if (renderer != null)
			{
				renderer.Alignment = value;
			}
		}

		protected bool ShouldSerializeLabelCaptionAlignment(Control control)
		{
			LabelCaptionAlignment inheritedValue = (LabelCaptionAlignment)InheritedPropertyValues.GetInheritedValue(control, "LabelCaptionAlignment", LabelCaptionAlignment.Default);
			LabelCaptionAlignment currentValue = GetLabelCaptionAlignment(control);
			return inheritedValue != currentValue;
		}

		#endregion

		#region LabelTop

		/// <summary>
		/// Get the top of the label relative to the control when the label is left aligned.
		/// </summary>
		[Category(DesignerConstants.Category)]
		[Description("When the label is aligned to the left of the control, this property specifies where the top of the label sits. If the default value of -1 is specified, the label will be placed at the vertical center of the control.")]
		[Browsable(true)]
		//[DefaultValue(-1)] // handled in ShouldSerializeLabelTop
		public int GetLabelTop(Control control)
		{
			ILabelCaptionRenderer renderer = GetRenderer(control);
			return renderer == null ? -1 : renderer.LabelTop;
		}

		/// <summary>
		/// Set the top of the label relative to the control when the label is left aligned.
		/// </summary>
		public void SetLabelTop(Control control, int value)
		{
			ILabelCaptionRenderer renderer = GetRenderer(control);
			if (renderer != null)
			{
				renderer.LabelTop = ControlDpiScalingHelper.ScaleToCurrentDpiY(value);
			}
		}

		protected bool ShouldSerializeLabelTop(Control control)
		{
			int inheritedValue = (int)InheritedPropertyValues.GetInheritedValue(control, "LabelTop", -1);
			int currentValue = GetLabelTop(control);
			return inheritedValue != currentValue;
		}

		#endregion

		#region LabelCaptionVisible

		/// <summary>
		/// Get whether the label caption is visible (rendered).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		[Category(DesignerConstants.Category)]
		[Description("Controls whether the label caption is visible (rendered).")]
		[Browsable(true)]
		//[DefaultValue(true)] // handled in ShouldSerializeLabelCaptionVisible
		public bool GetLabelCaptionVisible(Control control)
		{
			ILabelCaptionRenderer renderer = GetRenderer(control);
			return renderer != null && renderer.Visible;
		}

		/// <summary>
		/// Set whether the label caption is visible (rendered).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void SetLabelCaptionVisible(Control control, bool value)
		{
			ILabelCaptionRenderer renderer = GetRenderer(control);
			if (renderer != null)
			{
				renderer.Visible = value;
			}
		}

		protected bool ShouldSerializeLabelCaptionVisible(Control control)
		{
			bool inheritedValue = (bool)InheritedPropertyValues.GetInheritedValue(control, "LabelCaptionVisible", true);
			bool currentValue = GetLabelCaptionVisible(control);
			return inheritedValue != currentValue;
		}

		#endregion

		#region Site

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				InheritedPropertyValues.NotifySiteChanged(this);
			}
		}

		#endregion

		#region IExtenderProvider

		public virtual bool CanExtend(object extendee)
		{
			return GetRenderer(extendee as Control) != null;
		}

		#endregion

		#region Implementation

		readonly InheritedExtendedPropertyValueMemory InheritedPropertyValues = new InheritedExtendedPropertyValueMemory();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		protected ILabelCaptionRenderer GetRenderer(Control control)
		{
			IExtendedControl extendedControl = control as IExtendedControl;
			ILabelCaptionRenderer renderer = extendedControl == null ? null : extendedControl.Extensions.Get<ILabelCaptionRenderer>();
			return renderer;
		}

		#endregion
	}
}
