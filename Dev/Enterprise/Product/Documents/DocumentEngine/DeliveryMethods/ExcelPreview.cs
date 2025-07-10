using System.Drawing.Printing;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.IO;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	public class ExcelPreview : DeliveryMethod
	{
		internal readonly IDeliverCapableForm Parent;

		public ExcelPreview(IDeliverCapableForm parentForm)
		{
			Parent = parentForm;
		}

		protected override void DeliverCore(INotifications notifications = null)
		{
			DeliveryInfo firstXLSInfo = GetFirstXLSDeliveryInfo();
			if (DeliveryInfos.Count > 0 && firstXLSInfo != null)
			{
				Stream xlsFileContent = firstXLSInfo.FileContents;
				if (DeliveryInfos.Count > 1)
				{
					xlsFileContent = MergeFilesIntoOneXLS();
				}
				DeliverToExcel(xlsFileContent);
			}
		}

		#region Implementation

#if DEBUG
		protected virtual
#endif
 void DeliverToExcel(Stream xlsFileContent)
		{
			var deliveryInfo = DeliveryInfos[0];
			xlsFileContent.Position = 0;

			if (deliveryInfo.DeliveryFormat == DeliveryInfo.DeliveryFormats.Document && deliveryInfo.Instructions.AllowModifyAndPreviewInExcel)
			{
				PreviewInExcel(xlsFileContent);
			}
			else
			{
				try
				{
					PrintTaskUIProvider.ShowPreview(xlsFileContent, DeliveryInfos.ToArray(), Parent);
				}
				catch (InvalidPrinterException ex)
				{
					ErrorReporter.ReportOnce("Flexcel preview failed", "Flexcel preview failed", ex);
					xlsFileContent.Position = 0;
					PreviewInExcel(xlsFileContent);
				}
			}
		}

#if DEBUG
		protected virtual
#endif
 void PreviewInExcel(Stream xlsFileContent)
		{
			string xLSFileNameToShow = Temp.GetTempFileNameWithExtension((NoResString)"xls");
			using (ExcelInterface excel = new ExcelInterface())
			{
				excel.LoadExcelFile(xlsFileContent);
				excel.PreviewInXlWithoutDeletingFile(xLSFileNameToShow);
			}
		}

		#endregion
	}
}
