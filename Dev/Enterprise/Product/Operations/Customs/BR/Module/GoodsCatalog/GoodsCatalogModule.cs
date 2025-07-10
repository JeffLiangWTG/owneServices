using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class GoodsCatalogModule : Customs.Module.GoodsCatalogModule, IOperationalActionSupportable
	{
		public GoodsCatalogModule() : base()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override IFilterControl GetNewFilterControl() => new GoodsCatalogFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new BaseCusGoodsCatalogCollection<CusGoodsCatalog>(Factory, GlbCompany.CurrentCompany.PK);

		public OperationalActionSupporter OperationalActionSupporter => new GoodsCatalogOperationalActionSupporter();

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = base.GetNewActionMenuItems().ToList();
			result.Add(new ZMenuItem(ResString.GetMultilingualString("B3584F74-4225-4AC9-9B17-ED0119F031D2", "Download Catalog"), DownloadGoodsCatalogMenu_Click));
			return result.ToArray();
		}

		void DownloadGoodsCatalogMenu_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new DownloadGoodsCatalogForm(new GoodsCatalogDownloadObject(new BusinessObjectFactory())));
		}

		public override bool SupportsWorkflow => true;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GoodsCatalogFilterBusinessObject();
	}
}
