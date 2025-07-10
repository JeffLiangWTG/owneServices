using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// A find that that binds to a ZGuid property
	/// </summary>
	[ToolboxData("<{0}:ZGuidFindBox runat=server></{0}:ZFindBox>"),
	Designer(typeof(Design.ZTextBoxButtonDesigner))]
	public class ZGuidFindBox : ZFindBox
	{
		public new const string ModuleIDQuery = "ZGuidFindBox_ModuleID";
		public const string GuidContainerIDQuery = "GuidContainerID";

		#region Selected value

		protected override IZType GetSelectedValue()
		{
			ZGuid result = ZGuid.Invalid;

			if (string.IsNullOrEmpty(TextBoxControl.Text))
			{
				return ZGuid.Empty;
			}

			if (!string.IsNullOrEmpty(GuidContainerValue))
			{
				result = new ZGuid(GuidContainerValue);
			}
			if ((!result.IsValid || result.IsEmpty) && List != null)
			{
				result = List.PrimaryKeyFromCode(TextBoxControl.Text);
			}
			return result;
		}

		public delegate string TextFromCustomField(ZGuid value);
		public TextFromCustomField GetTextFromCustomField;

		protected override string GetTextFromValue(IZType newValue)
		{
			string result = null;
			if (newValue is ZGuid)
			{
				ZGuid zGuidValue = (ZGuid)newValue;
				if (zGuidValue.IsValid)
				{
					if (GetTextFromCustomField != null)
					{
						result = GetTextFromCustomField(zGuidValue);
					}
					else
					{
						result = List.CodeFromPrimaryKey(zGuidValue);
					}
				}
				else if (!zGuidValue.IsEmpty)
				{
					result = Res.GetString("5c9d6a4a-ac66-4baa-9b50-467003d1bc07", "Invalid");
				}
				else
				{
					result = "";
				}
			}
			return result;
		}

		protected override NameValueCollection AdditionalParameters
		{
			get
			{
				if (additionalParameters == null)
				{
					additionalParameters = base.AdditionalParameters;
					additionalParameters.Add(GuidContainerIDQuery, GuidContainerID.ToString());
				}

				return additionalParameters;
			}
		}
		NameValueCollection additionalParameters;

		#endregion

		#region GuidContainer

		public ZString GuidContainerID
		{
			get { return "GuidContainer" + ClientID; }
		}

		protected virtual string GuidContainerValue
		{
			get { return Page.Request.Params[GuidContainerID]; }
		}
		#endregion

		#region Overrides

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			if (Page != null)
			{
				Page.ZClientScript.RegisterHiddenField(GuidContainerID, ZString.Empty);
			}
		}

		protected override string ButtonClickHandler
		{
			get
			{
				EnsureChildControls();
				return string.Format("ZTextPopup_ShowGuidPopup('{0}', '{1}', '{2}', '{3}', '{4}');{5}", TextBoxControl.ClientID, ButtonControl.ClientID, PopupID, GuidContainerID, IFrameSourceString, AdditionalButtonClickHandler); // javascript junction text should not be translated
			}
		}

		protected override string OKFunctionName => "ZTextPopup_SetValueAndGuidThenHidePopup";

		#endregion

		#region IBindTo Members

		string fBindTo = "";

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public override string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}

		#endregion

		#region Internal Properties

		internal IZType GetSelectedValueInternal() => GetSelectedValue();
		internal string GetTextFromValueInternal(IZType newValue) => GetTextFromValue(newValue);
		internal string GuidContainerValueInternal => GuidContainerValue;

		#endregion
	}
}
