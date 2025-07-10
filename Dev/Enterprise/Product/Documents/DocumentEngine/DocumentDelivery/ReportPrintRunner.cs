using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine
{
	public static class ReportPrintRunner
	{
		public static void DeliverWithNotifyModes(string reportName, IBODocDataProvider dataWrapper, CodeDescriptionPairList notifyModesList)
		{
			using (var printTask = new PrintTask())
			using (var documentPack = new DocumentPack())
			using (var report = CreateReport(documentPack, reportName, dataWrapper))
			{
				documentPack.Add(report);
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.OverrideNotifyModesList(notifyModesList);
				printTask.RunWithPartialInstructions(AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);
			}
		}

		public static Report CreateReport(DocumentPack documentPack, string reportName, IBODocDataProvider dataWrapper)
		{
			var excelTemplate = ExcelTemplateRetriever.GetTemplate(reportName, Core.Constants.DataContext.None, null);
			return new Report(documentPack, excelTemplate, dataWrapper, reportName, null, DocumentDirection.ANY, false);
		}
	}
}
