using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Design;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class BaseUserControl : System.Web.UI.UserControl, IDesignTimeDataSourceType
	{
		public BaseUserControl()
		{
			topLevelDataSourceTypeHelper = new TopLevelDataSourceTypeHelper(this);
		}

		protected new ZPage Page
		{
			get { return base.Page as ZPage; }
			set { base.Page = value; }
		}

		protected BusinessObjectFactory Factory
		{
			get { return Page.Factory; }
		}

		protected WebUser SiteUser
		{
			get { return Page == null ? null : Page.SiteUser; }
		}

		protected ZGlobal ZAppInstance
		{
			get { return Page == null ? null : Page.AppInstance; }
		}

#if DEBUG
		/// <summary>
		/// Test-only property to facilitate testing during postbacks
		/// </summary>
		public new virtual bool IsPostBack
		{
			get { return base.IsPostBack; }
			set { }
		}

		/// <summary>
		/// Hashtable that is being used for test-friendly session-orineted methods
		/// </summary>
		Hashtable TestSessionStorage
		{
			get
			{
				if (fTestSessionStorage == null)
				{
					fTestSessionStorage = new Hashtable();
				}

				return fTestSessionStorage;
			}
		}
		Hashtable fTestSessionStorage;
		#endif

		/// <summary>
		/// Test-friendly method to save something on Session
		/// </summary>
		/// <param name="key">Hash key</param>
		/// <param name="value">Object to persist</param>
		protected void SaveToSession(string key, object value)
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				TestSessionStorage[key] = value;
			}
			else
#endif
			{
				Session[key] = value;
			}
		}

		/// <summary>
		/// Test-friendly method to remove something from session
		/// </summary>
		/// <param name="key">Hash key</param>
		protected void RemoveFromSession(string key)
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				TestSessionStorage.Remove(key);
			}
			else
#endif
			{
				Session.Remove(key);
			}
		}

		/// <summary>
		/// Test-friendly method to get values from session
		/// </summary>
		/// <param name="key">Hash key</param>
		/// <returns>Object that matches the key</returns>
		protected object GetFromSession(string key)
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				return TestSessionStorage[key];
			}
			else
#endif
			{
				return Session[key];
			}
		}

		#region IDesignTimeDataSourceType

		readonly TopLevelDataSourceTypeHelper topLevelDataSourceTypeHelper;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string DataSourceAssemblyName
		{
			get { return topLevelDataSourceTypeHelper.DataSourceAssemblyName; }
			set { topLevelDataSourceTypeHelper.DataSourceAssemblyName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string DataSourceTypeName
		{
			get { return topLevelDataSourceTypeHelper.DataSourceTypeName; }
			set { topLevelDataSourceTypeHelper.DataSourceTypeName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Type DataSourceType
		{
			get { return topLevelDataSourceTypeHelper.DataSourceType; }
		}

		#endregion
	}
}
