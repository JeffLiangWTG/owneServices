using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.ComponentModel.Design;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="KGroupBox"/></summary>
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KGroupBox : GroupBox, IVariableLengthCaptionRenderer
	{
		public KGroupBox()
		{
			CaptionRenderer = new GroupBoxVariableLengthCaptionRenderer(this);
		}

		#region Designer Smart Tags

		[SmartTagVisible]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		protected bool ShouldSerializeText() // this will only work if the Text property is overridden in this class
		{
			return CaptionRenderer.IsCaptionOverridden && !string.IsNullOrEmpty(Text);
		}

		[SmartTagVisible]
		public override AnchorStyles Anchor
		{
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}

		#endregion

		#region ControlCollection

		protected override Control.ControlCollection CreateControlsInstance()
		{ return new ControlCollection(this); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public new class ControlCollection : AutoDisposeControlCollection
		{
			public ControlCollection(KGroupBox owner)
				: base(owner)
			{
			}
		}

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
			set { CaptionRenderer.Captions = value; }
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

		readonly GroupBoxVariableLengthCaptionRenderer CaptionRenderer;

		#endregion
	}
}
