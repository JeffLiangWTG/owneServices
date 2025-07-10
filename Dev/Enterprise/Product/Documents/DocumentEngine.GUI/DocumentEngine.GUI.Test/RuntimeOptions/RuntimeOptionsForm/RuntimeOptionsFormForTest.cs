using System;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	internal class RuntimeOptionsFormForTest : RuntimeOptionsForm
	{
		public RuntimeOptionsFormForTest(Report report)
			: base(report)
		{
		}

		public RuntimeOptionsFormForTest(Report report, ReportScheduleTask scheduleTask)
			: base(report, scheduleTask)
		{
		}

		public RuntimeOptionsFormForTest(Report report, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, SecurityCheckpoint modifyDocumentCheckPoint, bool isErrorForm = false)
			: base(report, deliveryOptions, instructions, modifyDocumentCheckPoint, isErrorForm)
		{
		}

		public RuntimeOptionsFormForTest(PrintTask printTask, Report report, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, SecurityCheckpoint modifyDocumentCheckPoint)
			: base(printTask, report, deliveryOptions, instructions, modifyDocumentCheckPoint)
		{
		}

		internal readonly ShortcutCreatorForTest ShortcutCreator = new ShortcutCreatorForTest();

		protected override ShortcutCreator GetShortcutCreator()
		{
			return ShortcutCreator;
		}

		public new void PreviewButton_Click(object sender, EventArgs e)
		{
			base.PreviewButton_Click(sender, e);
		}

		protected override void Preview(IDeliverCapableForm parentForm, DeliveryInstructions deliveryInstructions)
		{
			if (!SuppressPreview)
			{
				base.Preview(parentForm, deliveryInstructions);
			}

			LastPreviewedDeliveryInstructions = deliveryInstructions;
		}

		internal bool SuppressPreview;

		internal DeliveryInstructions LastPreviewedDeliveryInstructions;

		public void SetReportDbManagerForTesting(SecondaryServerConnectionDetailsProvider testReportDb)
		{
			reportDbManager = SecondaryServerConnectionProviderProvider.GetProvider(testReportDb);
		}

		public static void ResetSessionOverrideReportDbOption()
		{
			sessionOverrideReportDbOption = null;
		}

		public ISecondaryServerConnectionProvider ReportDbManager_Exposed
		{
			get { return reportDbManager; }
		}
	}
}
