using System;
using CargoWise.Common;
using Enterprise.DocumentEngine.GUI.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI
{
	public class ShowReportUrlHandler : ReportUrlHandler
	{
		protected ShowReportUrlHandler()
		{
		}

		public new static ShowReportUrlHandler Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ShowReportUrlHandler();
				}
				return instance;
			}
		}

		[ThreadStatic]
		static ShowReportUrlHandler instance;

		protected override string ExpectedCommandText
		{
			get { return "ShowReport"; }
		}

		protected override void ProcessReport(Report report, ReportPrintSet reportSet, QueryString queryString)
		{
			var pack = new DocumentPack(report.MenuItem);
			pack.Add(report);
			var task = new PrintTask();
			task.Add(pack);

			var form = new RuntimeOptionsForm(task, report, AllowedDeliveryOptions.All, new DeliveryInstructions(pack), null);
#if DEBUG
			ShownFormForTest = form;
#endif
			form.Show();
		}

#if DEBUG
		internal RuntimeOptionsForm ShownFormForTest;
#endif
	}
}
