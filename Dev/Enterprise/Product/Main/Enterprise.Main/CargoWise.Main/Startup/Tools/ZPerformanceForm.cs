using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common.MemoryManagement;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;

namespace Enterprise
{
	public sealed partial class ZPerformanceForm : ZChildForm
	{
		public ZPerformanceForm(PerformanceStatisticWithForms performanceStatistic)
			: base(performanceStatistic)
		{
			this.performanceStatistic = performanceStatistic;
			makeFactoryReferencesStrong = PersistentFactoryCacheManager.Instance.MakeFactoryReferencesStrong();
			InitializeComponent();

			var bindingEnabled = ObjectFactory.Get<ISystemDataRegistry>().ServiceTaskBusinessObjectBindingEnabled;
			zLabel4.Visible = !bindingEnabled;
			NudgingGrid.Visible = bindingEnabled;
			NudgingTabPage.UseVisualStyleBackColor = bindingEnabled;

			StaticCacheGrid.ContextMenu.MenuItems.Add((NoResString)"Partial Collect", delegate { Reclaim(FlushAction.Partial); });
			StaticCacheGrid.ContextMenu.MenuItems.Add((NoResString)"Full Collect", delegate { Reclaim(FlushAction.Full); });
			StaticCacheGrid.ContextMenu.MenuItems.Add(CreateRefreshMenuItem());
			zGridFactoryStatistics.ContextMenu.MenuItems.Add(CreateRefreshMenuItem());
			OpenedFormsGrid.ContextMenu.MenuItems.Add(CreateRefreshMenuItem());
			NudgingGrid.ContextMenu.MenuItems.Add(CreateRefreshMenuItem());
			UncollectedTypesGrid.ContextMenu.MenuItems.Add(CreateRefreshMenuItem());
		}

		IDisposable makeFactoryReferencesStrong;

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("dba10f6b-4eb0-46c1-b462-7f9ad2425838", "Performance Statistics"); }
		}

		readonly PerformanceStatisticWithForms performanceStatistic;

		void Reclaim(FlushAction action)
		{
			var memoryBeforeReclaim = GC.GetTotalMemory(false);
			var details = Array.ConvertAll(StaticCacheGrid.SelectedElements, x => (StaticCacheDetails)x);
			performanceStatistic.Reclaim(action, details);
			performanceStatistic.RefreshBindingIncludingChildren();
			var memoryAfterReclaim = GC.GetTotalMemory(false);
			decimal reclaimedBytes = memoryBeforeReclaim - memoryAfterReclaim;
			var megabytesReclaimed = reclaimedBytes / 1024 / 1024;
			Globals.Message.Show(
				Res.GetString("b3387e96-7728-4b33-8d34-75926925698e", "{0} megabytes of managed memory were reclaimed.", megabytesReclaimed.ToString("N3")),
				Res.GetString("abaf1d53-52d4-4d09-b85d-4b41f5147cd4", "Memory reclaim"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		ZMenuItem CreateRefreshMenuItem()
		{
			return new ZMenuItem((NoResString)"Refresh", this.refreshToolStripMenuItem_Click, Shortcut.F5);
		}

		void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			performanceStatistic.Load();
		}
	}
}
