using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class ASYCUDAManifestBillModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.ASYCUDA.ManifestBill;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill);

		protected override IFilterControl GetNewFilterControl() => new ASYCUDAManifestBillFilterStripControl(GridCollection, (ASYCUDAManifestBillFilterStrip)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ASYCUDAManifestBillModuleCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ASYCUDAManifestBillFilterStrip();

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());

			if (ViewMenuItem != null)
			{
				ViewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ASYCUDAManifestBillModule.Menu.ViewBill", "View Bill"), HandleViewClick));
				ViewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ASYCUDAManifestBillModule.Menu.ViewManifest", "View Manifest"), HandleViewManifestClick));
			}

			if (EditMenuItem != null)
			{
				EditMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ASYCUDAManifestBillModule.Menu.EditBill", "Edit Bill"), HandleEditClick));
				EditMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ASYCUDAManifestBillModule.Menu.EditManifest", "Edit Manifest"), HandleEditManifestClick));
			}

			return menuItems.ToArray();
		}

		void HandleViewManifestClick(object sender, EventArgs eventArgs)
		{
			var bill = SelectedBusinessObjects.OfType<AsycudaBill>().FirstOrDefault();
			var manifest = bill?.Header;
			if (manifest != null)
			{
				var manifestController = AsycudaModule.GetNewControllerFor(manifest);
				var form = manifestController.ShowViewForm(manifest);
				AsycudaModule.SelectAndShowBill(form, bill);
			}
		}

		void HandleEditManifestClick(object sender, EventArgs eventArgs)
		{
			var bill = SelectedBusinessObjects.OfType<AsycudaBill>().FirstOrDefault();
			var manifest = bill?.Header;
			if (manifest != null)
			{
				var manifestController = AsycudaModule.GetNewControllerFor(manifest);
				var form = manifestController.ShowEditForm(manifest);
				AsycudaModule.SelectAndShowBill(form, bill);
			}
		}

		protected override FilteredGridLoader CreateSearchManager()
			=> new BillModuleLoader(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

		class BillModuleLoader : FilteredGridLoader
		{
			readonly Dictionary<BusinessObjectFactory, IDisposable> factoryWithDisposables;

			public BillModuleLoader(FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
				: this(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements, new Dictionary<BusinessObjectFactory, IDisposable>())
			{
			}

			BillModuleLoader(FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements, Dictionary<BusinessObjectFactory, IDisposable> factoryWithDisposables)
				: base(filterBusinessObject, handler, provider, moduleId, SuspendValidation(factoryWithDisposables, createFactory), typeOfElements)
			{
				this.factoryWithDisposables = factoryWithDisposables;
			}

			static Func<BusinessObjectFactory> SuspendValidation(Dictionary<BusinessObjectFactory, IDisposable> factoryWithDisposables, Func<BusinessObjectFactory> inner)
			{
				return () =>
				{
					var result = inner();
					factoryWithDisposables.Add(result, new DisposableList(new[]
					{
						result.SuspendCustomsValuesFetchHint(typeof(AsycudaBill)),
						result.SuspendCustomsValuesFetchHint(typeof(AsycudaManifestHeader)),
					}));

					return result;
				};
			}

			protected override void OnFactorySwapped(BusinessObjectFactory oldFactory, BusinessObjectFactory newFactory)
			{
				base.OnFactorySwapped(oldFactory, newFactory);
				if (factoryWithDisposables.TryGetValue(oldFactory, out var list))
				{
					list.Dispose();
					factoryWithDisposables.Remove(oldFactory);
				}
			}

			protected override void Dispose(bool isDisposing)
			{
				base.Dispose(isDisposing);

				factoryWithDisposables.Values.ForEach(d => d.Dispose());
				factoryWithDisposables.Clear();
			}
		}

		protected override int MaxRowsToLoad => MaxDisplayRecords;
		internal static int MaxDisplayRecords => ManifestCustomsDataRegistry.Instance.MaximumSearchableManifestBills.Value;

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.AsycudaManifestReporting;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.GlobalManifestBillsWorkflowDescriptorCode; }
		}
	}
}
