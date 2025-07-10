using System;
using System.Drawing;
using System.Reflection;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZTextPopupTest : ZTextBoxButtonTest
	{
		public virtual void TestAdditionalButtonClickHandler()
		{
			AssertEquals(String.Empty, Popup.AdditionalButtonClickHandlerInternal);
		}

		public void TestPopupID()
		{
			AssertEquals(ExpectedPopupID, Popup.PopupIDInternal);
		}

		public virtual void TestBackgroundColor()
		{
			AssertEquals(Color.Empty, Popup.PopupBackgroundColorInternal);
		}

		public void TestButtonBackground()
		{
			AssertEquals("BackgroundStyle", ExpectedButtonBackgroundStyle, Popup.ButtonBackgroundStyleInternal);
		}

		public void TestResourceNames()
		{
			PropertyInfo property = ResourceContainerType.GetProperty("Resources");
			ZWebResourceCollection resources = (ZWebResourceCollection)property.GetValue(Control, null);

			for (int i = 0; i < ExpectedResourceNames.Length; i++)
			{
				bool found = false;
				for (int j = 0; j < resources.Count; j++)
				{
					if (ExpectedResourceNames[i] == GetResourceName(resources[j].FileName))
					{
						found = true;
						break;
					}
				}
				AssertEquals(true, found);
			}
		}

		protected string GetResourceName(string resourceFileName)
		{
			return resourceFileName.Substring(resourceFileName.LastIndexOf("/") + 1);
		}

		public override void TestClickHandlerAssignment()
		{
			AssertEquals(string.Format("ZTextPopup_ShowPopup('ctl01_TextBox', '{0}');", ExpectedPopupID) + Popup.AdditionalButtonClickHandlerInternal, Popup.ButtonClickHandlerInternal);
		}

		protected virtual string ExpectedPopupID
		{
			get { return string.Format("ZTextPopup{0}_{1}", Popup.GetType().Name, "1"); }
		}

		protected virtual string ExpectedButtonBackgroundStyle
		{
			get { return string.Empty; }
		}

		public abstract void TestPopupDimension();

		#region Implementation

		protected ZTextPopup Popup
		{
			get { return (ZTextPopup)this.TextBoxButton; }
		}

		protected virtual Type ResourceContainerType
		{
			get { return typeof(ZTextPopup); }
		}

		protected virtual string[] ExpectedResourceNames
		{
			get { return new string[] { "opener.js" }; }
		}

		#endregion Implementation
	}
}
