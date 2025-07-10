using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public class ChangeCategoryPopup : ZButtonPopup
	{
		#region Constants

		public const string DataSourceIndexerQueryStringKey = "changeCategoryDataSourceIndexer";
		public const string FilterStripPKQueryStringKey = "changeCategoryFilterStripPK";

		#endregion

		internal ZGuid filterStripPK;

		#region Overrides

		protected override void BindCore(object dataSource)
		{
			FilterStrip strip = dataSource as FilterStrip;

			if (strip != null)
			{
				filterStripPK = strip.PK;
				ButtonControl.Attributes["Class"] = "FilterStripGroup_" + strip.OrCategory.ToString().ToLower();
			}
			else
			{
				base.BindCore(dataSource);
			}
		}

		protected override string PopupID
		{
			get
			{
				return base.PopupID + (filterStripPK.IsValid ? ("_" + filterStripPK.ToString()) : string.Empty);
			}
		}

		protected override void RenderPopupContainer(HtmlTextWriter writer)
		{
			Page.SaveDataSourceToSession();
			base.RenderPopupContainer(writer);
		}

		protected override NameValueCollection AdditionalParameters
		{
			get
			{
				NameValueCollection result = base.AdditionalParameters;

				if (Page.DataSourceIndexer == ZGuid.Empty
#if DEBUG
					&& !Globals.IsTest
#endif
					)
				{
					Page.SaveDataSourceToSession();
				}

				result.Add(DataSourceIndexerQueryStringKey, Page.DataSourceIndexer.ToString());
				result.Add(FilterStripPKQueryStringKey, filterStripPK.ToString());
				return result;
			}
		}

		#endregion

		#region Resources

		const string OrCategoriesImageResourceFileName = "title_category.jpg";
		const string ClearCategoryImageResourceFileName = "none_category.jpg";
		const string RedCategoryImageResourceFileName = "red_category.jpg";
		const string GreenCategoryImageResourceFileName = "green_category.jpg";
		const string BlueCategoryImageResourceFileName = "blue_category.jpg";
		const string BrownCategoryImageResourceFileName = "brown_category.jpg";
		const string GreyCategoryImageResourceFileName = "grey_category.jpg";

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(OrCategoriesImageResource);
				result.Add(ClearCategoryImageResource);
				result.Add(RedCategoryImageResource);
				result.Add(GreenCategoryImageResource);
				result.Add(BlueCategoryImageResource);
				result.Add(BrownCategoryImageResource);
				result.Add(GreyCategoryImageResource);
				return result;
			}
		}

		#region OrCategoriesImageResource

		public ZWebResource OrCategoriesImageResource
		{
			get
			{
				if (orCategoriesImageResource == null)
				{
					orCategoriesImageResource = new ZWebResource(typeof(ChangeCategoryPopup), OrCategoriesImageResourceFileName, Page, "Enterprise.ZArchitecture.Web.GUI.FilterStrips.ChangeCategoryPopup");
				}
				return orCategoriesImageResource;
			}
		}

		ZWebResource orCategoriesImageResource;

		#endregion

		#region ClearCategoryImageResource

		public ZWebResource ClearCategoryImageResource
		{
			get
			{
				if (clearCategoryImageResource == null)
				{
					clearCategoryImageResource = new ZWebResource(typeof(ChangeCategoryPopup), ClearCategoryImageResourceFileName, Page, "Enterprise.ZArchitecture.Web.GUI.FilterStrips.ChangeCategoryPopup");
				}
				return clearCategoryImageResource;
			}
		}

		ZWebResource clearCategoryImageResource;

		#endregion

		#region RedCategoryImageResource

		public ZWebResource RedCategoryImageResource
		{
			get
			{
				if (redCategoryImageResource == null)
				{
					redCategoryImageResource = new ZWebResource(typeof(ChangeCategoryPopup), RedCategoryImageResourceFileName, Page, "Enterprise.ZArchitecture.Web.GUI.FilterStrips.ChangeCategoryPopup");
				}
				return redCategoryImageResource;
			}
		}

		ZWebResource redCategoryImageResource;

		#endregion

		#region GreenCategoryImageResource

		public ZWebResource GreenCategoryImageResource
		{
			get
			{
				if (greenCategoryImageResource == null)
				{
					greenCategoryImageResource = new ZWebResource(typeof(ChangeCategoryPopup), GreenCategoryImageResourceFileName, Page, "Enterprise.ZArchitecture.Web.GUI.FilterStrips.ChangeCategoryPopup");
				}
				return greenCategoryImageResource;
			}
		}

		ZWebResource greenCategoryImageResource;

		#endregion

		#region BlueCategoryImageResource

		public ZWebResource BlueCategoryImageResource
		{
			get
			{
				if (blueCategoryImageResource == null)
				{
					blueCategoryImageResource = new ZWebResource(typeof(ChangeCategoryPopup), BlueCategoryImageResourceFileName, Page, "Enterprise.ZArchitecture.Web.GUI.FilterStrips.ChangeCategoryPopup");
				}
				return blueCategoryImageResource;
			}
		}

		ZWebResource blueCategoryImageResource;

		#endregion

		#region BrownCategoryImageResource

		public ZWebResource BrownCategoryImageResource
		{
			get
			{
				if (brownCategoryImageResource == null)
				{
					brownCategoryImageResource = new ZWebResource(typeof(ChangeCategoryPopup), BrownCategoryImageResourceFileName, Page, "Enterprise.ZArchitecture.Web.GUI.FilterStrips.ChangeCategoryPopup");
				}
				return brownCategoryImageResource;
			}
		}

		ZWebResource brownCategoryImageResource;

		#endregion

		#region GreyCategoryImageResource

		public ZWebResource GreyCategoryImageResource
		{
			get
			{
				if (greyCategoryImageResource == null)
				{
					greyCategoryImageResource = new ZWebResource(typeof(ChangeCategoryPopup), GreyCategoryImageResourceFileName, Page, "Enterprise.ZArchitecture.Web.GUI.FilterStrips.ChangeCategoryPopup");
				}
				return greyCategoryImageResource;
			}
		}

		ZWebResource greyCategoryImageResource;

		#endregion

		#endregion

		#region Setup

		protected override string IFrameSourcePageName
		{
			get { return "ChangeCategoryPage.aspx"; }
		}

		#endregion

		#region Style

		#region Dimensions

		protected override Unit PopupHeight
		{
			get { return 183; }
		}

		protected override Unit PopupWidth
		{
			get { return 180; }
		}

		protected override int ButtonControlWidth
		{
			get { return ZFilterStripConstants.Controls.AddButtonWidth; }
		}

		#endregion

		#region internal wrapper properties

		protected internal Unit InternalPopupHeight() => PopupHeight;

		protected internal Unit InternalPopupWidth() => PopupWidth;

		protected internal ZGuid InternalDataSourceIndexer() => Page.DataSourceIndexer;

		#endregion

		#region CssClass

		protected override string PopupCssClass
		{
			get
			{
				if (fPopupCssClass == null)
				{
					fPopupCssClass = ZCssHelper.Join(base.PopupCssClass, CssConstants.PopupChangeCategory);
				}
				return fPopupCssClass;
			}
		}

		string fPopupCssClass;

		#endregion

		#endregion

		#region Button Text / ToolTip

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected override string ButtonTextCore
		{
			get { return "▼"; }
		}

		protected override string ButtonToolTip
		{
			get { return Res.GetString("133b3aa6-f83e-4662-ab7c-d209d2fb137e", "Select which group to add this filter to.  Any result matching one of the filters in the group will be returned."); }
		}

		#endregion
	}
}
