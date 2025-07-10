using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Windows.UI;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class WindowPersister : IWindowPersister
	{
		public static void SaveUrlsToRegistry()
		{
			SaveCore(GetOpenFormUrls());
		}

		public static void ClearUrlsFromRegistry()
		{
			SaveCore("");
		}

		static void SaveCore(string uRLs)
		{
			ObjectFactory.Get<ISystemDataRegistry>().WindowPersisterData = uRLs;
		}

		public static void LoadUrlsFromRegistry()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var registry = ObjectFactory.Get<ISystemDataRegistry>();
				if (registry != null)
				{
					OpenFormsFromUrls(registry.WindowPersisterData);
					if (!string.IsNullOrEmpty(registry.WindowPersisterData))
					{
						registry.WindowPersisterData = "";
					}
				}
			}
		}

		public static string GetOpenFormUrls()
		{
			var urls = new List<string>();
#if WINZOR
			var isCWNext = true;
#else
			var isCWNext = CWNextFeatureHelper.IsCWNextEnabled();
#endif
			using (Db.DisposableActionForDbConnection())
			{
				foreach (var form in ZApplication.GetOpenForms())
				{
					var zForm = form as ZForm;
					if (zForm != null && zForm.ControllerID != null)
					{
						var controller = ZControllerFactory.Create(zForm.ControllerID);

						ShowFormUrlHandler handler = null;
						switch (zForm.DisplayMode)
						{
							case ODisplayMode.ReadOnly:
								handler = ShowViewFormUrlHandler.Instance;
								break;
							case ODisplayMode.Delete:
								handler = ShowDeleteFormUrlHandler.Instance;
								break;
							case ODisplayMode.Browse:
							case ODisplayMode.Edit:
								handler = ShowEditFormUrlHandler.Instance;
								break;
						}
						if (handler != null && (controller == null || controller.SupportsHyperlinking))
						{
							urls.Add(handler.Create(zForm.ControllerID, zForm.IdentifierForPersistingForm, zForm.GetFormArgsToPersistOnClose()));
						}
					}
					if (isCWNext && form is EmbeddedModulePopup modulePopup && modulePopup.Module != null)
					{
						urls.Add(ShowModuleUrlHandler.Instance.Create(modulePopup.Module.ID));
					}
				}
			}
			return string.Join(",", urls);
		}

		public static void OpenFormsFromUrls(string urls)
		{
			if (!string.IsNullOrEmpty(urls))
			{
				foreach (var url in urls.Split(','))
				{
					try
					{
						EnterpriseUrlHandlerService.Instance.ExecuteUrlForWindowPersister(url, true);
					}
					catch (EnterpriseUrlHandlerException)
					{
						// Couldn't find the form for the specified object, doesn't matter
					}
				}
			}
		}

		string IWindowPersister.GetOpenFormUrls()
		{
			return WindowPersister.GetOpenFormUrls();
		}
	}
}
