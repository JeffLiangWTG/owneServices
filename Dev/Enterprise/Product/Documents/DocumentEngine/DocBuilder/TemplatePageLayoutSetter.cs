using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.DocBuilder
{
	class TemplatePageLayoutSetter
	{
		public void SetPageLayout(ExcelWorkSheet workSheet, string pageStyle)
		{
			switch (pageStyle)
			{
				case DocumentConfigPageStyleList.Codes.Landscape:
					workSheet.InsertRows(2, 1);
					workSheet[2, 0] = Constants.ConfigAreaParameters.PageStyleSignature + DocumentConfigPageStyleList.Descriptions.Landscape.GetUnresolvedString();
					workSheet.ParentExcelInterface.SetOrientation(Orientation.Landscape);
					break;

				case DocumentConfigPageStyleList.Codes.Continuous:
					workSheet.InsertRows(2, 1);
					workSheet[2, 0] = Constants.ConfigAreaParameters.PageStyleSignature + DocumentConfigPageStyleList.Descriptions.Continuous.GetUnresolvedString();
					break;

				case DocumentConfigPageStyleList.Codes.ContinuousWithNoMargin:
					workSheet.InsertRows(2, 1);
					workSheet[2, 0] = Constants.ConfigAreaParameters.PageStyleSignature + DocumentConfigPageStyleList.Descriptions.Continuous.GetUnresolvedString();
					workSheet.ParentExcelInterface.SetPrintMargins(new TXlsMargins(0, 0, 0, 0, 0, 0));
					break;

				case DocumentConfigPageStyleList.Codes.Label:
					workSheet.InsertRows(2, 1);
					workSheet[2, 0] = Constants.ConfigAreaParameters.PageStyleSignature + DocumentConfigPageStyleList.Descriptions.Label.GetUnresolvedString();
					workSheet.ParentExcelInterface.SetPaperSize(Constants.CustomPaperSizesInInch.Label4x6Width * 256, Constants.CustomPaperSizesInInch.Label4x6Height * 256);
					workSheet.ParentExcelInterface.SetPrintMargins(new TXlsMargins(0, 0, 0, 0, 0, 0));
					break;
			}
		}
	}
}
