using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class RemovedCommoditiesLogViewerUserControl : ZUserControl
	{
		RemovedCommoditiesLogCollection removedCommoditiesLogCollection;

		public RemovedCommoditiesLogViewerUserControl()
		{
			InitializeComponent();
			AddControls();
		}

		void AddControls()
		{
			var commodityAssessmentLogPanelUserControl = new CommodityRiskLogPanelUserControl() { Dock = DockStyle.Fill };

			SplitContainer.Panel2.Controls.Add(commodityAssessmentLogPanelUserControl);
			BindingSource.SetBindingMember(commodityAssessmentLogPanelUserControl, "CommodityRiskLogCollection");
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is IComplianceItemRiskStatusProvider provider)
			{
				removedCommoditiesLogCollection = new RemovedCommoditiesLogCollection(provider);

				base.SetDataBinding(removedCommoditiesLogCollection, dataMember);

				removedCommoditiesLogCollection.Factory.Saved -= Factory_Saved;
				removedCommoditiesLogCollection.Factory.Saved += Factory_Saved;
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			ReloadCollectionIfNeeded();
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				ReloadCollectionIfNeeded();
			}
		}

		void ReloadCollectionIfNeeded()
		{
			if (Visible && !IsDisposing && !IsDisposed)
			{
				removedCommoditiesLogCollection?.LoadCollection();
			}
		}
	}
}
