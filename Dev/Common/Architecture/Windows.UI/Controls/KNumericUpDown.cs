using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using AppDomainWrappers.Net;
using CargoWise.ComponentModel;

#region Debug
#if DEBUG
using System.Linq;
#endif // DEBUG
#endregion // Debug

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="NumericUpDown"/></summary>
	[DefaultBindingProperty("Value")]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KNumericUpDown : NumericUpDown
	{
		public KNumericUpDown()
		{
#if DEBUG
			var appDomainWrapper = new AppDomainWrapper();
			var architectureAssembly = appDomainWrapper.GetAssemblies().First(assembly => assembly.FullName.StartsWith("Enterprise.ZArchitecture.GUI,"));
			var suppressAttributeType = architectureAssembly.GetType("Enterprise.ZArchitecture.GUI.SuppressDpiAwareBasherAttribute");
			var suppressAttribute = (Attribute)Activator.CreateInstance(suppressAttributeType);

			foreach (Control includedControl in Controls)
			{
				var controlType = includedControl.GetType();
				if (controlType.FullName == "System.Windows.Forms.UpDownBase+UpDownButtons" ||
					controlType.FullName == "System.Windows.Forms.UpDownBase+UpDownEdit")
				{
					TypeDescriptor.AddAttributes(includedControl, new[] { suppressAttribute });
				}
			}
#endif // DEBUG
		}

		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		[BindingMetaDataProperty(MetaDataTypes.DecimalPlaces, "DecimalPlaces")]
		public new decimal Value
		{
			get { return base.Value; }
			set { base.Value = value; }
		}

		bool ShouldSerializeValue()
		{
			return Value != 0;
		}
#pragma warning disable CA1725
		protected override void OnLayout(LayoutEventArgs levent)
		{
			AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}
#pragma warning restore CA1725
		#region Binding ReadOnly

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "ReadOnly")]
		[DefaultValue(false)]
		public new virtual bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				base.ReadOnly = value;
				ReadOnlyForBindingProperty.OnReadOnlyChanged();
			}
		}

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

		ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
						this,
						delegate
						{ return ReadOnly; },
						delegate(bool value)
						{ ReadOnly = value; });
				}
				return readOnlyForBindingProperty;
			}
		}
		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;

		#endregion
	}
}
