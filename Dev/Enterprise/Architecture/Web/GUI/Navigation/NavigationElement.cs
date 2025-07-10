using System.Web;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;

#if DEBUG
using Enterprise.ZArchitecture.Web.GUI.Testing;
#endif

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class NavigationElement
	{
		#region Constructors

		public NavigationElement(string title, string pagePath)
		{
			this.fTitle = title;
			this.pagePath = pagePath;
		}

		public NavigationElement(string title, string pagePath, BooleanRegistryItem registryUseItem)
			: this(title, pagePath)
		{
			this.RegistryUseItem = registryUseItem;
		}

		public NavigationElement(string title, string pagePath, BooleanRegistryItem registryUseItem, WebSecurityRight securityRight, WebUser currentUser)
			: this(title, pagePath, registryUseItem)
		{
			this.securityRight = securityRight;
			this.CurrentUser = currentUser;
		}

		#endregion Constructors

		#region Properties

		public string Title
		{
			get { return fTitle; }
		}
		readonly string fTitle;

		public string PagePath
		{
			get { return pagePath; }
		}
		readonly string pagePath;

		public string PageParameters
		{
			get;
			set;
		}

		public string URL
		{
			get
			{
				string result = "";
				if (string.IsNullOrEmpty(PageParameters))
				{
					result = PagePath;
				}
				else
				{
					result = ZString.Format("{0}?{1}", PagePath, PageParameters);
				}
				if (AppInstance != null && IsRelativePath(PagePath) && PagePath != "#")
				{
					result = AppInstance.ApplicationRoot + result;
				}
				return result;
			}
		}

		bool IsRelativePath(string pagePath)
		{
			return pagePath.IndexOf("http://") < 0 && pagePath.IndexOf("https://") < 0;
		}

		public bool Visible
		{
			get { return AtLeastOneSubMenuItemIsVisible && RegistryItemIsActivated && (CurrentUser == null || CurrentUser.AreSecurityRightsGranted(SecurityRight)); }
		}

		protected virtual bool AtLeastOneSubMenuItemIsVisible
		{
			get
			{
				bool result = (SubMenuItems.Count == 0);
				foreach (NavigationElement subMenuItem in SubMenuItems)
				{
					bool isReportPage = false;
					if (AppInstance != null && AppInstance.WebAccessManager != null && AppInstance.WebAccessManager.IsReportsPage(subMenuItem.PagePath))
					{
						isReportPage = true;
					}
					if (subMenuItem.Visible && !isReportPage)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public NavigationElementCollection SubMenuItems
		{
			get
			{
				if (fSubMenuItems == null)
				{
					fSubMenuItems = new NavigationElementCollection();
				}
				return fSubMenuItems;
			}
		}
		NavigationElementCollection fSubMenuItems;

		public WebSecurityRight SecurityRight
		{
			get
			{
				return securityRight;
			}
		}

		#endregion

		#region Implementation

		protected virtual ZGlobal AppInstance
		{
			get
			{
				if (fAppInstance == null)
				{
#if DEBUG
					if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
					{
						{
							fAppInstance = GetNewTestGlobal();
						}
					}
					else
#endif
					{
						fAppInstance = (HttpContext.Current != null) ? (ZGlobal)HttpContext.Current.ApplicationInstance : null;
					}
				}
				return fAppInstance;
			}
		}
		ZGlobal fAppInstance;

#if DEBUG

		protected virtual ZGlobal GetNewTestGlobal()
		{
			return new ZTestGlobal();
		}

#endif

		bool RegistryItemIsActivated
		{
			get
			{
				return RegistryUseItem == null || RegistryUseItem.Value;
			}
		}

		readonly BooleanRegistryItem RegistryUseItem;
		readonly WebSecurityRight securityRight;
		readonly WebUser CurrentUser;

		#endregion
	}
}
