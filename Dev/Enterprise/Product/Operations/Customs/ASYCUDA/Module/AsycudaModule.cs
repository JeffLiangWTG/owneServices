using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class AsycudaModule : ZFilterGridModule, IOperationalActionSupportable
	{
		protected ApplicationBusinessProvider provider;
		protected ZString countryCode;
		protected bool createVOC;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController result = GetNewControllerFor(selectedBusinessObject);
			var asycudaController = result as ASYCUDAManifestController;
			if (asycudaController != null)
			{
				asycudaController.Provider = this.provider;
				asycudaController.CountryCode = this.countryCode;
				asycudaController.CreateVOC = this.createVOC;
			}
			return result;
		}

		internal static ZController GetNewControllerFor(BusinessObject businessObject)
		{
			ZController result = null;

			var manifestHeader = businessObject as AsycudaManifestHeader;
			var consol = manifestHeader?.Consol;
			if (consol == null || ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.Value)
			{
				result = ZControllerFactory.Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest);
			}
			else
			{
				result = ZControllerFactory.Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestConsol);
			}

			return result;
		}

		internal static void SelectAndShowBill(IZForm form, AsycudaBill bill)
		{
			var consolForm = form as ConsolForm;
			if (consolForm != null)
			{
				var plugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest);
				if (plugIn.Enabled)
				{
					consolForm.PlugIns.SelectPlugInTabPage(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest);
					plugIn.OnGUIShown();
					(plugIn.UserControl as AsycudaManifestMainControl)?.SelectAndShowBill(bill.PK);
				}
			}
			else
			{
				var manifestform = form as ManifestForm;
				manifestform?.SelectAndShowBill(bill.PK);
			}
		}

		public AsycudaModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			var newVOCItem = GetVOCMenu();
			var newNVCItem = GetNVCMenu();
			int newMenuItemPosition = 0;
			if (newVOCItem != null)
			{
				menuItems.Insert(newMenuItemPosition++, newVOCItem);
			}
			if (newNVCItem != null)
			{
				menuItems.Insert(newMenuItemPosition++, newNVCItem);
			}
			return menuItems.ToArray();
		}

		protected virtual MenuItem GetNVCMenu()
		{
			return GetNewMenuItem(
				Res.GetData("AsycudaModuleGrid.NewNVC", "&New Forwarder Manifest"),
				ShowNewNVCManifestForm,
				GetNVCApplicationBusinessProviders(Factory).ToList(),
				ApplicationCodeTypeList.Codes.Consolidator
			);
		}

		public static IEnumerable<ApplicationBusinessProvider> GetNVCApplicationBusinessProviders(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AsycudaModule.NVCC_ApplicationBusinessProviders", () =>
			{
				var result = new List<ApplicationBusinessProvider>();
				result.AddRange(ZZDatabaseValidationHelper.GetNVCApplicationBusinessProviders(factory));
				result.RemoveAll(x => x.CountryCodes.Contains(Core.Constants.CountryCodes.Singapore));
				return result;
			});
		}

		protected virtual MenuItem GetVOCMenu()
		{
			return GetNewMenuItem(
				Res.GetData("AsycudaModuleGrid.NewVOC", "&New Carrier Manifest"),
				ShowNewVOCManifestForm,
				GetVOCApplicationBusinessProviders(Factory).ToList(),
				ApplicationCodeTypeList.Codes.ShippingLine
			);
		}

		public static IEnumerable<ApplicationBusinessProvider> GetVOCApplicationBusinessProviders(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AsycudaModule.VOCC_ApplicationBusinessProviders", () =>
			{
				var result = new List<ApplicationBusinessProvider>();
				result.AddRange(ZZDatabaseValidationHelper.GetVOCApplicationBusinessProviders(factory));
				result.RemoveAll(x => x.CountryCodes.Contains(Core.Constants.CountryCodes.Singapore));
				return result;
			});
		}

		protected delegate void ShowNewManifestForm(ApplicationBusinessProvider provider, ZString countrycode);
		protected MenuItem GetNewMenuItem(ResourceStringData menuText, ShowNewManifestForm showNewForm, IList<ApplicationBusinessProvider> providers, ZString manifestStyle)
		{
			MenuItem newItem = null;
			if (providers.Any())
			{
				var countryMenuItemPairs = new Dictionary<string, List<ZMenuItem>>();
				var countrycode = GlbCompany.CurrentCompany.Country;

				providers.Select(provider => (Provider: provider, Tuple: provider.GetManifestDescriptions(Factory, provider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty, manifestStyle), type => true)))
					.SelectMany(x => x.Tuple.Select(d => (x.Provider, CountryCode: d.CountryCode, Description: d.Description)))
					.OrderBy(x => x.Description)
					.ForEach(info =>
					{
						var key = info.CountryCode;
						var newMenuItem = new ZMenuItem(MakeSafeForMenu(info.Description), (sender, e) => showNewForm(info.Provider, key));

						if (countryMenuItemPairs.TryGetValue(key, out var menuItems))
						{
							menuItems.Add(newMenuItem);
							var countryName = RefCountry.LoadFromCountryCode(Factory, key)?.Description;
							if (countryName.HasValue)
							{
								menuItems.ForEach(x => x.Text = x.Text.Replace(countryName, ZString.Empty).Replace("-", ZString.Empty).Trim());
							}
						}
						else
						{
							countryMenuItemPairs.Add(key, new List<ZMenuItem>() { newMenuItem });
						}
					});

				if (countryMenuItemPairs.ContainsKey(countrycode.Code))
				{
					var defaultProvider = providers.FirstOrDefault(provider => provider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty, manifestStyle).Contains(countrycode.Code));
					newItem = new ZMenuItem(menuText, (sender, e) => showNewForm(defaultProvider, countrycode.Code), IconTypes.NewButtonActive, IconTypes.NewButtonRest);
					ConfigMenuItem(countrycode.Code, countryMenuItemPairs[countrycode.Code].Select(c => c.CloneMenu() as ZMenuItem).ToList());
					newItem.MenuItems.Add("-");
				}
				else
				{
					newItem = new ZMenuItem(menuText, IconTypes.NewButtonActive, IconTypes.NewButtonRest);
				}

				countryMenuItemPairs.ForEach(countryMenuItemPair =>
				{
					ConfigMenuItem(countryMenuItemPair.Key, countryMenuItemPair.Value);
				});

				void ConfigMenuItem(string countryCode, List<ZMenuItem> menuItems)
				{
					MenuItem subMenu = null;

					if (menuItems.Any())
					{
						if (menuItems.IsCountMoreThan(1))
						{
							var refCountry = RefCountry.LoadFromCountryCode(Factory, countryCode);
							subMenu = newItem.MenuItems.Add(MakeSafeForMenu(refCountry.Description), menuItems.ToArray());
						}
						else
						{
							subMenu = menuItems[0];
							newItem.MenuItems.Add(subMenu);
						}

#if DEBUG
						if (subMenu != null)
						{
							TypeDescriptor.AddAttributes(menuItems[0], new SuppressFormsLocalizedTestAttribute());
						}
#endif
					}
				}
			}

			return newItem;
		}

		string MakeSafeForMenu(string dirty)
		{
			return Regex.Replace(dirty, "([^&])&([^&])", "$1&&$2");  // KMenuItem will 'fix' && to become & and then it will not appear (S&&S shows as SS); we will ask for KMenuItem to be fixed and then this module will be ready for it. 
		}

		protected void ShowNewNVCManifestForm(ApplicationBusinessProvider provider, ZString countryCode)
		{
			try
			{
				this.provider = provider;
				this.countryCode = countryCode;
				ShowNewForm();
			}
			finally
			{
				this.provider = null;
				this.countryCode = ZString.Empty;
			}
		}

		protected void ShowNewVOCManifestForm(ApplicationBusinessProvider provider, ZString countryCode)
		{
			try
			{
				this.provider = provider;
				this.countryCode = countryCode;
				createVOC = true;
				ShowNewForm();
			}
			finally
			{
				this.provider = null;
				this.countryCode = ZString.Empty;
				createVOC = false;
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AsycudaFilterStrip();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AsycudaFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var collection = new AsycudaManifestModuleCollection(Factory);
			return collection;
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.ASYCUDA.Manifest; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AsycudaManifestReporting; }
		}
		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}
		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return AsycudaManifestWorkflowDescriptor.Constants.Code; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var baseItems = base.GetNewActionMenuItems();

			var newItems = GlobalManifestActionMenuFactory.GetCountryMenuItem(Factory)
				.Where(m => m.Precondition())
				.Select(i => (MenuItem)i.GetNewMenuItem());

			return newItems.Any()
				? baseItems.Concat(newItems).ToArray()
				: baseItems;
		}

		public OperationalActionSupporter OperationalActionSupporter => new AsycudaManifestOperationalActionSupporter();
	}
}
