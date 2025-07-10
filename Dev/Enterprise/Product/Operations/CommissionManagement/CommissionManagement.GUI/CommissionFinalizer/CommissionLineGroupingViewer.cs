using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.GUI
{
	public static class CommissionLineGroupingViewer
	{
		public static IZForm ShowViewForm(ICommissionLineGrouping grouping)
		{
			BusinessObject entityToShow;
			IZForm form = null;
			var controller = GetController(grouping, out entityToShow);
			if (grouping.SourceTableCode == JobHeaderSchema.Constants.Prefix && entityToShow == null)
			{
				Globals.Message.Show(Res.GetString("098896ed-84f5-4e70-a064-90020fc7ff38", "Cannot view form as the Job has been archived"),
						Res.GetString("a95d078a-c990-4ba4-9348-9d2b43972f71", "Job Profit/Loss"),
						MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else if (controller != null)
			{
				form = controller.ShowViewForm(entityToShow);
			}

			return form;
		}

		static ZController GetController(ICommissionLineGrouping grouping, out BusinessObject entityToShow)
		{
			switch (grouping.SourceTableCode)
			{
				case AccTransactionHeaderSchema.Constants.Prefix:
					entityToShow = grouping.Transaction as BusinessObject;
					return AccountingControllerCreator.GetNewController(grouping.Transaction as AccTransactionHeader);

				case JobHeaderSchema.Constants.Prefix:
					entityToShow = grouping.Job;
					return ZControllerFactory.Create(ControllerIDs.JobManagement);
			}

			entityToShow = null;
			return null;
		}
	}
}
