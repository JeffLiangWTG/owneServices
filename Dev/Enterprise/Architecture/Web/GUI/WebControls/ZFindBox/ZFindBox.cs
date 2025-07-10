using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Internal;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Find box control
	/// </summary>
	[ToolboxData("<{0}:ZFindBox runat=server></{0}:ZFindBox>"),
	Designer(typeof(Design.ZTextBoxButtonDesigner))]
	public class ZFindBox : ZTextIFramePopup
	{
		public const string ModuleIDQuery = "ZFindBox_ModuleID";

		#region ModuleID

		[Category(ZGUIConstants.DesignerCategory)]
		[Editor(typeof(WebModuleIDEditor), typeof(UITypeEditor))]
		public virtual WebModuleID ModuleID
		{
			get { return fModuleID; }
			set { fModuleID = value; }
		}
		protected WebModuleID fModuleID = WebModuleIDs.NotAssigned;

		// Do *not* remove this method!
		// .NET will use this method to determine whether or not to serialize the ModuleID property value.
		internal bool ShouldSerializeModuleID()
		{
			return (ModuleID != WebModuleIDs.NotAssigned);
		}

		#endregion ModuleID

		#region Overrides

		protected override void BindCore(object dataSource)
		{
			if (!string.IsNullOrEmpty(BindToList))
			{
				List = ZPropertyAccessor.Get(dataSource, BindToList) as IFindBoxListProvider;
			}

			base.BindCore(dataSource);
		}

		protected internal IFindBoxListProvider List;

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			TextBoxControl.CharacterCasing = CharacterCasing.Upper;
		}

		protected override string PopupCssClass
		{
			get
			{
				string result = base.PopupCssClass;
				if (WebModuleIDs.IsFilterStripModule(ModuleID))
				{
					result = CssConstants.ZFindBoxWithFilterStrips;
				}
				return result;
			}
		}

		#endregion Overrides

		#region IBindToList Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToList))]
		public string BindToList
		{
			get { return fBindToList; }
			set { fBindToList = value; }
		}
		string fBindToList = "";

		#endregion

		#region Properties

		#region Selected value

		protected override IZType GetSelectedValue()
		{
			ZString result = TextBoxControl.Text.Trim(Whitespace);
			if (!result.IsEmpty && List != null && string.IsNullOrEmpty(List.DescriptionFromCode(result)))
			{
				result = List.NearestMatchCore(result, List.AutoCompleteOnCommit).Item1;
				TextBoxControl.Text = result;
			}
			return result;
		}

		protected override string GetTextFromValue(IZType newValue)
		{
			string result = null;
			if (newValue is ZString)
			{
				result = ((ZString)newValue).Trim(Whitespace);
			}
			return result;
		}

		#endregion

		protected override string IFrameSourcePageName
		{
			get { return "ZFilterPage.aspx"; }
		}

		protected override Type IFrameSourcePageContainerType
		{
			get { return typeof(ZFindBox); }
		}

		protected override Unit PopupHeight
		{
			get { return new Unit(355); }
		}

		protected override Unit PopupWidth
		{
			get { return new Unit(700); }
		}

		[Browsable(true)]
		[Category("Appearance")]
		public new int MaxLength
		{
			get
			{
				EnsureChildControls();
				return TextBoxControl.MaxLength;
			}
			set
			{
				EnsureChildControls();
				TextBoxControl.MaxLength = value;
			}
		}

		NameValueCollection fAdditionalParameters;
		protected override NameValueCollection AdditionalParameters
		{
			get
			{
				if (fAdditionalParameters == null)
				{
					fAdditionalParameters = base.AdditionalParameters;
					fAdditionalParameters.Add(ModuleIDQuery, ModuleID.ToString());
				}

				return fAdditionalParameters;
			}
		}

		protected override string PopupID
		{
			get { return String.Format("{0}_{1}", base.PopupID, ModuleID); }
		}

		#endregion Properties

		#region Internal Properties

		internal Unit PopupWidthInternal => PopupWidth;
		internal Unit PopupHeightInternal => PopupHeight;
		internal void CreateChildControlsInternal() => CreateChildControls();

		#endregion
	}
}
