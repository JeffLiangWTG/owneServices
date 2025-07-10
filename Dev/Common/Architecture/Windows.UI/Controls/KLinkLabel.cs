using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel.Design;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="KLabel"/></summary>
	[DesignTimeControlNameGenerator("lbl")]
	[DefaultBindingProperty("Text")]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KLinkLabel : LinkLabel, IVariableLengthCaptionRenderer
	{
		public KLinkLabel()
		{
			CaptionRenderer = new LabelVariableLengthCaptionRenderer(this);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			var result = base.CreateAccessibilityInstance();
			accessibleObjects.Add(result);
			return result;
		}

		readonly List<AccessibleObject> accessibleObjects = new List<AccessibleObject>();

#if !WINZOR
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "private field name is not a resource string")]
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			//WI00457065 - Memory Leak - ZLinkLabel
			//note: technically a Link could have been added to Links, then passed to a LinkAccessibleObject constructor, then Links collection cleared, then this LinkLabel disposed...
			//hopefully this isn't a thing that can happen? if it is, then we'll have to make e.g. an AllLinksEver collection and manage its lifespan correctly
#if NET
			const string ownerFieldName = "<Owner>k__BackingField";
			const string linkCollectionFieldName = "_linkCollection";
			const string ownerControlFieldName = "_ownerControl";
#else
			const string ownerFieldName = "owner";
			const string linkCollectionFieldName = "linkCollection";
			const string ownerControlFieldName = "ownerControl";
#endif

			FieldInfo linkField = null;

			foreach (var link in Links)
			{
				linkField ??= typeof(Link).GetField(ownerFieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				linkField.SetValue(link, null);
			}
			//Links.Clear() has side-effects we don't want (e.g. this.owner.AdjustSize();), so just null it instead to break the connection
			typeof(LinkLabel).GetField(linkCollectionFieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, null);

			FieldInfo ownerControlField = null;
			foreach (var accessibleObject in accessibleObjects)
			{
				ownerControlField ??= typeof(ControlAccessibleObject).GetField(ownerControlFieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				ownerControlField.SetValue(accessibleObject, null);
			}
			accessibleObjects.Clear();
		}
#endif

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Override is required or ShouldSerializeText() test will fail")]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		protected bool ShouldSerializeText() // this will only work if the Text property is overridden in this class
		{
			// has the developer set the text property manually
			return CaptionRenderer.IsCaptionOverridden && !string.IsNullOrEmpty(Text);
		}

		readonly ControlTextVariableLengthCaptionRenderer CaptionRenderer;

		#endregion

#if DEBUG
		public void PerformClick_ForTest()
		{
			InvokeOnClick(this, EventArgs.Empty);
		}
#endif
	}
}
