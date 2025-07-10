using System;
using System.Windows.Forms;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.Module
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public partial class ResourceStringsFilterControl : ZFilterStripControl
	{
		public ResourceStringsFilterControl(HelpDataStringCollection gridCollection, ResourceStringsFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			FilteredGrid.ContextMenu.Popup += ContextMenu_Popup;
			FilteredGrid.ContextMenu.Collapse += ContextMenu_Collapse;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			if (FilteredGrid.SelectedElements.Length > 0)
			{
				HelpDataString dataString = (HelpDataString)FilteredGrid.SelectedElements[0];

				if (dataString.HD_Language != Business.ResourceStrings.Instance.CurrentLanguage)
				{
					translateResourceMenuItem = new ZMenuItem(ResString.GetMultilingualString("ResourceStrings.Translate", "Translate"), TranslateSelectedResource);
					FilteredGrid.ContextMenu.MenuItems.Add(translateResourceMenuItem);
				}
			}
		}

		void ContextMenu_Collapse(object sender, EventArgs e)
		{
			if (translateResourceMenuItem != null)
			{
				FilteredGrid.ContextMenu.MenuItems.Remove(translateResourceMenuItem);
			}
		}

		MenuItem translateResourceMenuItem;

		void TranslateSelectedResource(object sender, EventArgs e)
		{
			HelpDataString translation = GetTranslation((HelpDataString)FilteredGrid.SelectedElements[0]);
			if (translation != null)
			{
				new HelpDataStringForm(translation).Show();
			}
		}

		internal static HelpDataString GetTranslation(HelpDataString resource)
		{
			var translation = ResourceStringsFactory.Lookup(Business.ResourceStrings.Instance.CurrentLanguage, resource.HD_Code);
			if (translation == null)
			{
				translation = resource.Clone();
				translation.HD_Language = Business.ResourceStrings.Instance.CurrentLanguage;
				translation.HD_IsCheckedOut = true;
			}
			return translation;
		}

		#region Implementation

		protected internal new ResourceStringsFilterBusinessObject FilterBusinessObject
		{
			get { return (ResourceStringsFilterBusinessObject)base.FilterBusinessObject; }
		}

		#endregion
	}
}
