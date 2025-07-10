using System;
using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using FlexCel.Core;
using FlexCel.Render;

namespace Enterprise.DocumentEngine.GUI.DocBuilder
{
	internal partial class SectionPreviewForm : ZChildForm, ISectionPreviewView
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public SectionPreviewForm()
		{
			InitializeComponent();
		}

		public SectionPreviewForm(SectionPreviewManager manager)
			: base(manager)
		{
			InitializeComponent();
			controller.Bind(manager, this);
		}
		bool isImageTruncated;

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		public void UpdateSectionViews(string documentTitle, IBODocDataProvider[] docDataProviders, ExcelTemplate systemExcelTemplate, ExcelTemplate customizedExcelTemplate, ContactType contactType, DocumentDirection documentDirection)
		{
			isImageTruncated = false;

			UpdateOriginalViewImages(documentTitle, docDataProviders, systemExcelTemplate, contactType, documentDirection);
			UpdateCustomizedViewImages(documentTitle, docDataProviders, customizedExcelTemplate, contactType, documentDirection);
			UpdateViewCheckBoxes();
			UpdateSectionPreviewTruncatedLabel(isImageTruncated);
		}

		public void UpdateZoom(int zoom)
		{
			originalTemplatePictureBox.Zoom = zoom;
			originalRenderPictureBox.Zoom = zoom;
			customizedTemplatePictureBox.Zoom = zoom;
			customizedRenderPictureBox.Zoom = zoom;
		}

		void UpdateOriginalViewImages(string documentTitle, IBODocDataProvider[] docDataProviders, ExcelTemplate excelTemplate, ContactType contactType, DocumentDirection documentDirection)
		{
			var imageRenderer = new ExcelTemplateImageRenderer(excelTemplate, (SectionPreviewManager)this.BusinessEntity);

			var dataViewImage = imageRenderer.GetRenderedDocumentImage(documentTitle, docDataProviders, contactType, documentDirection);
			originalRenderPictureBox.Image = dataViewImage;

			var macroViewImage = imageRenderer.GetMacroViewImage();
			originalTemplatePictureBox.Image = macroViewImage;

			isImageTruncated = isImageTruncated || imageRenderer.IsImageTruncated;
		}

		void UpdateCustomizedViewImages(string documentTitle, IBODocDataProvider[] docDataProviders, ExcelTemplate excelTemplate, ContactType contactType, DocumentDirection documentDirection)
		{
			var imageRenderer = new ExcelTemplateImageRenderer(excelTemplate, (SectionPreviewManager)this.BusinessEntity);

			var dataViewImage = imageRenderer.GetRenderedDocumentImage(documentTitle, docDataProviders, contactType, documentDirection);
			customizedRenderPictureBox.Image = dataViewImage;

			var macroViewImage = imageRenderer.GetMacroViewImage();
			customizedTemplatePictureBox.Image = macroViewImage;

			isImageTruncated = isImageTruncated || imageRenderer.IsImageTruncated;
		}

		void UpdateSectionPreviewTruncatedLabel(bool isImageTruncated)
		{
			if (isImageTruncated)
			{
				sectionPreviewTruncatedLabel.ForeColor = Color.Red;
				sectionPreviewTruncatedLabel.Text = Res.GetString("b7b0a74c-bc17-42fa-ae73-8058d91b8cc2", "This section is too big to be previewed correctly. It may have been truncated / not rendered.");
				sectionPreviewTruncatedLabel.Enabled = true;
				sectionPreviewTruncatedLabel.Visible = true;
			}
			else
			{
				sectionPreviewTruncatedLabel.Text = "";
				sectionPreviewTruncatedLabel.Enabled = false;
				sectionPreviewTruncatedLabel.Visible = false;
			}
		}

		void HandleCheckBoxValueChanged(object sender, EventArgs e)
		{
			if (originalViewCheckBox.Checked && customizedViewCheckBox.Checked)
			{
				mainSplitContainer.Visible = true;
				mainSplitContainer.Panel1Collapsed = false;
				mainSplitContainer.Panel2Collapsed = false;
			}
			else if (originalViewCheckBox.Checked && !customizedViewCheckBox.Checked)
			{
				mainSplitContainer.Visible = true;
				mainSplitContainer.Panel1Collapsed = true;
				mainSplitContainer.Panel2Collapsed = false;
			}
			else if (!originalViewCheckBox.Checked && customizedViewCheckBox.Checked)
			{
				mainSplitContainer.Visible = true;
				mainSplitContainer.Panel1Collapsed = false;
				mainSplitContainer.Panel2Collapsed = true;
			}
			else
			{
				mainSplitContainer.Visible = false;
			}
		}

		class ExcelTemplateImageRenderer
		{
			internal ExcelTemplateImageRenderer(ExcelTemplate excelTemplate, SectionPreviewManager sectionPreviewManager)
			{
				this.excelTemplate = excelTemplate;
				this.sectionPreviewManager = sectionPreviewManager;
			}

			readonly ExcelTemplate excelTemplate;
			readonly SectionPreviewManager sectionPreviewManager;
			public bool IsImageTruncated { get; private set; }
			const int MaxImageHeightForTruncation = 2500;

			internal Image GetMacroViewImage()
			{
				Image result = null;

				if (excelTemplate != null)
				{
					using (var stream = excelTemplate.GetAsTemplateStream())
					{
						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(stream);
							result = GetImage(excelInterface, 6, 1, excelInterface.WorkSheets[0].RowCount - 1, 80);
						}
					}
				}

				return result;
			}

			internal Image GetRenderedDocumentImage(string documentTitle, IBODocDataProvider[] docDataProviders, ContactType contactType, DocumentDirection documentDirection)
			{
				Image result = null;

				if (excelTemplate != null && docDataProviders != null && docDataProviders.Length > 0)
				{
					using (var documentPack = new DocumentPack())
					{
						documentPack.Language = sectionPreviewManager.Language;

						using (var report = new Report(documentPack, excelTemplate, new DataProviderList(docDataProviders), documentTitle,
							contactType, null, documentDirection, false))
						{
							report.IsReportForSectionPreview = true;
							report.PrepareForRender();

							using (var stream = new MemoryStream())
							{
								report.Save(stream);
								using (var excelInterface = new ExcelInterface())
								{
									try
									{
										excelInterface.LoadExcelFile(stream);
									}
									catch (ExcelInterfaceException)
									{
										return null;
									}
									result = GetImage(excelInterface);
								}
							}
						}
					}
				}

				return result;
			}

			Image GetImage(ExcelInterface excelInterface, int printRangeTop = 0, int printRangeLeft = 0, int printRangeBottom = 0, int printRangeRight = 0)
			{
				Image result = null;

				var workSheet = excelInterface.WorkSheets[0];
				if (workSheet.RowCount > 0 && workSheet.ColumnCount > 0 && printRangeTop <= printRangeBottom && printRangeLeft <= printRangeRight)
				{
					var rowRangeHeight = workSheet.GetRowRangeHeight(0, workSheet.RowCount - 1);

					if (rowRangeHeight > 0)
					{
						var excelFile = excelInterface.Xls;

						using (var export = new FlexCelImgExport(excelFile))
						{
							RemovePrintMargins(excelFile);
							excelFile.PrintHCentered = false;
							var sectionHeight = rowRangeHeight / ExcelMetrics.RowMultDisplay(excelFile) + 100;
							export.PageSize = new TPaperDimensions((NoResString)"Section Previews", 2000, sectionHeight);
							export.Resolution = 100;
							export.PrintRangeTop = printRangeTop;
							export.PrintRangeLeft = printRangeLeft;
							export.PrintRangeBottom = printRangeBottom;
							export.PrintRangeRight = printRangeRight;

							using (var imageStream = new MemoryStream())
							{
								try
								{
									SaveAsImageToStreamRetryingWithTruncation(this, export, imageStream, sectionHeight);
									result = new Bitmap(imageStream);
								}
								catch (Exception e)
								{
									if (e.IsCriticalException())
									{
										throw;
									}
									this.IsImageTruncated = true;
									result = new Bitmap(1, 1);
								}
							}
						}
					}
					else
					{
						result = new Bitmap(1, 1);
					}
				}
				else
				{
					result = new Bitmap(1, 1);
				}

				return result;
			}

			static void SaveAsImageToStreamRetryingWithTruncation(ExcelTemplateImageRenderer renderer, FlexCelImgExport export, Stream stream, double sectionHeight)
			{
				try
				{
					export.SaveAsImage(stream, ImageExportType.Tiff, ImageColorDepth.Color256);
				}
				catch
				{
					if (sectionHeight > ControlDpiScalingHelper.ScaleToCurrentDpiY(MaxImageHeightForTruncation))
					{
						renderer.IsImageTruncated = true;
						ControlDpiScalingHelper.SetHeight(export.PageSize, MaxImageHeightForTruncation, true);
						export.SaveAsImage(stream, ImageExportType.Tiff, ImageColorDepth.Color256);
					}
					else
					{
						throw;
					}
				}
			}

			static void RemovePrintMargins(ExcelFile excelFile)
			{
				var printMargins = new TXlsMargins(0, 0, 0, 0, 0, 0);
				excelFile.SetPrintMargins(printMargins);
			}
		}

		void SectionPreviewForm_Shown(object sender, EventArgs e)
		{
			UpdateViewCheckBoxes();
		}

		void UpdateViewCheckBoxes()
		{
			if (originalTemplatePictureBox.Image != null && customizedTemplatePictureBox.Image != null)
			{
				originalViewCheckBox.Checked = false;
				customizedViewCheckBox.Checked = true;

				SetViewCheckBoxesVisible(true);
			}
			else
			{
				SetViewCheckBoxesVisible(false);

				originalViewCheckBox.Checked = originalTemplatePictureBox.Image != null;
				customizedViewCheckBox.Checked = customizedTemplatePictureBox.Image != null;
			}
		}

		void SetViewCheckBoxesVisible(bool visible)
		{
			viewLabel.Visible = visible;
			originalViewCheckBox.Visible = visible;
			customizedViewCheckBox.Visible = visible;
		}
	}
}
