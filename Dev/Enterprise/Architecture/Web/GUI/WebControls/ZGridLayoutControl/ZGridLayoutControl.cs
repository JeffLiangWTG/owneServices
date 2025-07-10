using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.Internal;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZGridLayoutControl : ZButtonPopup, ISelfBindingWebControl
	{
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
		bool ShouldSerializeModuleID()
		{
			return (ModuleID != WebModuleIDs.NotAssigned);
		}

		#endregion ModuleID	

		#region Grid Layout Change Event

		public void HookTextChangedEvent(EventHandler handler)
		{
			TextChanged += handler;
		}

		public void UnhookTextChangedEvent(EventHandler handler)
		{
			TextChanged -= handler;
		}

		#endregion

		#region Overrides

		protected override string IFrameSourcePageName
		{
			get { return "GridLayoutPage.aspx"; }
		}

		protected override Unit PopupWidth
		{
			get { return Unit.Pixel(380); }
		}

		protected override Unit PopupHeight
		{
			get { return Unit.Pixel(200); }
		}

		protected override string ButtonToolTip
		{
			get { return Res.GetString("aefec181-f86b-405a-9b24-af4fa524e672", "Customize Grid Columns"); }
		}

		protected override string ButtonTextCore
		{
			get { return Res.GetString("ed081cfc-c61a-4dbb-a226-ecf8b23590ff", "Customize Columns"); }
		}

		protected override int ButtonControlWidth
		{
			get { return 140; }
		}

		protected override Unit ControlHeight
		{
			get { return Unit.Empty; }
		}

		protected override NameValueCollection AdditionalParameters
		{
			get
			{
				if (fAdditionalParameters == null)
				{
					fAdditionalParameters = base.AdditionalParameters;
					fAdditionalParameters.Add(GridModuleIDKey, ModuleID.ToString());
					fAdditionalParameters.Add(GridCurrentLayoutKey, TextBoxControl.Text);
				}

				return fAdditionalParameters;
			}
		}
		NameValueCollection fAdditionalParameters;

		#endregion

		public const string GridModuleIDKey = "GridLayout_ModuleID";
		public const string GridCurrentLayoutKey = "GridLayout_CurrentLayout";

		#region ISelfBindingWebControl Members

		public override bool IsBindable(object topLevelBizO)
		{
			return false;
		}

		protected override void BindCore(object topLevelBizO)
		{
		}

		public override void UnBind()
		{
		}

		#endregion

		#region Internal Properties

		internal string GetTextFromValueInternal(IZType newValue) => GetTextFromValue(newValue);
		internal IZType GetSelectedValueInternal() => GetSelectedValue();
		internal Unit PopupHeightInternal => PopupHeight;
		internal Unit PopupWidthInternal => PopupWidth;
		internal string DisplayStyleInternal => DisplayStyle;

		#endregion
	}
}
