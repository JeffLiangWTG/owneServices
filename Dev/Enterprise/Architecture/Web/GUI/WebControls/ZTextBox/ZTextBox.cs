using System;
using System.ComponentModel;
using System.Web.UI;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	[DefaultProperty("Text"), ToolboxData("<{0}:ZTextBox runat=server></{0}:ZTextBox>")]
	public class ZTextBox : ZTextBoxBase
	{
		#region Properties

		[Browsable(true), Category("Behavior"), DefaultValue(CharacterCasing.Normal)]
		public CharacterCasing CharacterCasing
		{
			get
			{
				object fCasing = this.ViewState["CharacterCasing"];
				return (fCasing != null) ? (CharacterCasing)fCasing : CharacterCasing.Normal;
			}
			set
			{
				this.ViewState["CharacterCasing"] = value;
			}
		}

		#endregion Properties

		#region Rendering

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (this.CharacterCasing.Equals(CharacterCasing.Upper))
			{
				this.Attributes.Add("onblur", "javascript:this.value = this.value.toUpperCase();");
			}
			else if (this.CharacterCasing == CharacterCasing.Lower)
			{
				this.Attributes.Add("onblur", "javascript:this.value = this.value.toLowerCase();");
			}
		}

		#endregion Rendering

		#region SelectedValue

		[Browsable(false)]
		protected override IZType SelectedValue
		{
			get { return (ZString)Text; }
			set { Text = (value != null) ? value.ToString() : ""; }
		}

		#endregion SelectedValue
	}

	public enum CharacterCasing
	{
		// Fields
		Lower = 2,
		Normal = 0,
		Upper = 1
	}

	#endregion
}
