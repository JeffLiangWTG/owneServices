using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture
{
	[ImmutableObject(true)]
	public class ShowModuleUrlHandler : UrlHandler, IShowModuleUrlHandler
	{
		public Action<Form> OnFormInitialized { get; set; }
		protected ShowModuleUrlHandler()
		{
		}

		public static ShowModuleUrlHandler Instance
		{
			get { return instance ?? (instance = new ShowModuleUrlHandler()); }
		}
		static ShowModuleUrlHandler instance;

		public string Create(ModuleIdentifier moduleID) => Create(moduleID.ID.ToString());

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query string for Enterprise URL Service")]
		public string Create(string moduleID)
		{
			var queryString = new QueryString();
			queryString.Add("Command", ExpectedCommandText);
			queryString.Add("ModuleID", moduleID);
			queryString.Add("Hash", CreateQueryStringSecurityHash(queryString));

			return GetUrlFromQueryString(queryString);
		}

		#region UrlHandler Overrides

		protected override string ExpectedCommandText
		{
			get { return "ShowModule"; }
		}

		protected override bool HandleCore(QueryString queryString)
		{
			var moduleIDAsString = queryString["ModuleID"];
			var result = false;
			if (moduleIDAsString != null)
			{
				var moduleID = ZModuleFactory.Instance.GetRegisteredIdentifierByName(moduleIDAsString);
				result = HandleShowModule(moduleID);
			}

			return result;
		}

		protected internal override string[] GetQueryNamesSecuredBySecurityHash(QueryString queryString)
		{
			return new string[] { "LicenceCode", "ModuleID" };
		}

		#endregion

		#region Show Module

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		bool HandleShowModule(ModuleIdentifier moduleIdentifier)
		{
			var result = false;
			if (moduleIdentifier == null)
			{
				throw new EnterpriseUrlHandlerException("Module type is unknown to the current version of " + Constants.ProductName + ".");
			}
			else
			{
				var module = ZModuleFactory.Instance.Create(moduleIdentifier);
				if (module != null)
				{
					var form = (Form)module.ShowPopup();
					OnFormInitialized?.Invoke(form);
#if !WINZOR
					if (form != null)
					{
						Application.DoEvents();
						ForceFormToActivate(form);
					}
#endif
				}

				result = true;
			}

			return result;
		}

		#endregion
	}
}
