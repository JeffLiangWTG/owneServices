using System.ComponentModel;

using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public class ZFilterGrid : ZGrid, IDataGridLayoutIdentifierRoot
	{
		protected internal override sealed bool IsGridLayoutConfigurable
		{
			get { return false; }
		}

		public void SetParentFilterGridModule(ZModule parentFilterGridModule)
		{
			ParentModule = parentFilterGridModule;
			SetModuleId(ParentModule == null ? null : ParentModule.ID);
		}

		public void SetModuleId(ModuleIdentifier id)
		{
			ModuleID = id;
			ColumnLayoutContext = ModuleIDName;
		}

		public string ModuleIDName
		{
			get { return ModuleID == null ? string.Empty : ModuleID.Name; }
		}

		public bool? ShowFormsFromMainThread => (ParentModule as ZFilterModule)?.ShowFormsFromMainThread;

		public ZModule ParentModule { get; private set; }

		public ModuleIdentifier ModuleID { get; private set; }

		/// <summary>
		/// LegacyDataGridLayoutContextKeyProvider needs this to be cached to access StmModuleFilter
		/// However if the key is accessed while disposing, ZGrid.Parent control is not there any more
		/// </summary>
		internal FilterStripBusinessObject FilterBusinessObject { get; private set; }

		protected override void OnAfterDataBound()
		{
			base.OnAfterDataBound();

			var parentFilterStripCtrl = Parent as ZFilterStripControl;
			if (parentFilterStripCtrl != null)
			{
				FilterBusinessObject = parentFilterStripCtrl.FilterBusinessObject;
			}
		}

		#region IDataGridLayoutIdentifierRoot Members

		/// <summary>
		/// If a module is country-specific, then the column layout should be per country. ie Customs Declaration module vs Shipment module
		/// </summary>
		string IDataGridLayoutIdentifierRoot.ID
		{
			get
			{
				if (dataGridLayoutRootID == null && ParentModule != null)
				{
					dataGridLayoutRootID = "";

					var currentCountry = StaticCurrentFetcher.Instance.CurrentCompany.Country.RN_Code;
					var currentCountryModuleInfoThatDoesNotFallBackToGeneral = ZModuleFactory.Instance.GetRegisteredModuleInfo(ParentModule.ID, currentCountry, false);

					if (currentCountryModuleInfoThatDoesNotFallBackToGeneral != null
						&& ZModuleFactory.Instance.GetCountryOverridesRegisteredForModule(ParentModule.ID).Length > 0)
					{
						dataGridLayoutRootID = currentCountry;
					}
				}
				return dataGridLayoutRootID;
			}
		}
		string dataGridLayoutRootID;

		#endregion
	}
}
