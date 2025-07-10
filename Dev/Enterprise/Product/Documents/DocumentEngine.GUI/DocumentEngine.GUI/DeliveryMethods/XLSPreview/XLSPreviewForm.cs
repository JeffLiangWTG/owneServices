using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.GUI.DocumentDelivery;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using FlexCel.Core;
using FlexCel.Draw;
using FlexCel.Render;
using FlexCel.XlsAdapter;
using Watermark = Enterprise.RemotePrinting.Engine.Watermark;
#if !WINZOR
using Enterprise.RemoteDesktopServices;
#endif

namespace Enterprise.DocumentEngine.GUI
{
	public partial class XLSPreviewForm : ZForm
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1120:DoNotGetIconsImagesFromRexOrResourcesFile", Justification = "This is a very oooooooooold code")]
		public XLSPreviewForm(Stream xlsStream, DeliveryInfo[] deliveryInfos, IDeliverCapableForm parentForm)
		{
			DeliveryInfos = deliveryInfos;
			XLSStream = xlsStream;
			DeliverForm = parentForm;

			if (deliveryInfos != null)
			{
				var disposableList = new DisposableList(deliveryInfos.Length);
				foreach (var deliveryInfo in deliveryInfos)
				{
					deliveryInfo.SheetNames.ForEach(SheetNames.Add);

					disposableList.Add(deliveryInfo.LockFileContents());
				}
				deliveryInfosLock = disposableList;
			}

			InitializeComponent();

			//if image cannot be shown, detect and correct
			try
			{
				var resources = new System.ComponentModel.ComponentResourceManager(typeof(XLSPreviewForm));
				ZoomPresetButton.Image = (Image)resources.GetObject("ZoomPresetButton.Image");
				ZoomPresetButton.Visible = true;
				ZoomPresetButton.ImageAlign = ContentAlignment.MiddleRight;
			}
			catch (ArgumentException)
			{
				//Parameter is not valid. at System.Drawing.Image.get_FrameDimensionsList() 
				ZoomPresetButton.Image = null;
			}

			PreviewMain.Document = flexCelImgProducer;
			PreviewThumbs.Document = flexCelImgProducer;

			if (deliveryInfos != null && deliveryInfos.Length > 0)
			{
				var info = deliveryInfos[0];

				PreviewMain.IsLocalDocument = info.IsLocalDocument;

				var instructions = info.Instructions;
				if (instructions != null)
				{
					DeliverButton.Visible = OpenInExcelButton.Visible = instructions.Destination != DeliveryInstructionDestination.DocConfigPreview;
				}
				SetSaveAsButtonVisible(info);
			}
			else
			{
				SaveAsButton.Visible = false;
			}

			ZoomUpDown.AllowOverlap(ZoomLabel);
			DocumentPanel.AllowOverlap(SheetsListBox);
			Separator2Panel.AllowOverlap(SheetsListBox);
		}

		void SetSaveAsButtonVisible(DeliveryInfo deliveryInfo)
		{
			var buttonVisibleIfReport = deliveryInfo.DeliveryFormat == DeliveryInfo.DeliveryFormats.Report && Env.Security.SaveAsReportButton.IsAllowed;
			var buttonVisibleIfDocument = deliveryInfo.DeliveryFormat == DeliveryInfo.DeliveryFormats.Document && Env.Security.SaveAsDocumentButton.IsAllowed;
			SaveAsButton.Visible = buttonVisibleIfReport || buttonVisibleIfDocument;
		}

		internal readonly IDeliverCapableForm DeliverForm;
		readonly Stream XLSStream;
		internal readonly DeliveryInfo[] DeliveryInfos;
		readonly IDisposable deliveryInfosLock;

#if DEBUG
		internal Stream XlsStreamForTesting
		{
			get { return XLSStream; }
		}
#endif

		#region Loading

		protected override void OnLoad(EventArgs e)
		{
			try
			{
				isLoading = true;
#if DEBUG
				SetupPostingCalled = true;
#endif
				base.OnLoad(e);
			}
			finally
			{
				isLoading = false;
			}
		}

		internal bool isLoading;

		internal List<SheetName> SheetNames { get; } = new List<SheetName>();

		void XLSPreviewForm_Load(object sender, System.EventArgs e)
		{
			isLoading = true;

			if (XLSStream.Position > 0)
			{
				XLSStream.Position = 0;
			}

			try
			{
				xlsFile = new XlsFile();
				xlsFile.Open(XLSStream);
				xlsFile.Linespacing = Convert.ToDouble(DeliveryInfos[0].LineSpacing);

				ExcelInterface.ScaleDocument(xlsFile);

				SheetsListBox.BeginUpdate();
				try
				{
					for (var sheetNumber = 1; sheetNumber <= xlsFile.SheetCount; sheetNumber++)
					{
						xlsFile.ActiveSheet = sheetNumber;
						if (xlsFile.SheetVisible == TXlsSheetVisible.Visible)
						{
							var sheetName = SheetNames.FirstOrDefault(s => s.StrictName == xlsFile.SheetName);
							if (sheetName != null)
							{
								SheetsListBox.Items.Add(sheetName);
							}
							else
							{
								var newSheetName = new SheetName { StrictName = xlsFile.SheetName, EntireName = xlsFile.SheetName };
								SheetsListBox.Items.Add(newSheetName);
							}
						}
					}
				}
				finally
				{
					SheetsListBox.EndUpdate();
				}

				int sheetCount = 10;
				if (SheetsListBox.Items.Count < 10)
				{
					sheetCount = SheetsListBox.Items.Count;
				}

				if (sheetCount < 3)
				{
					sheetCount = 3;
				}

				ControlDpiScalingHelper.SetHeight(ref SheetsPanel, SheetCaptionPanel.Height + (sheetCount + 1) * SheetsListBox.ItemHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);

				flexCelImgProducer.Workbook = xlsFile;
				SheetsListBox.SelectedIndex = 0;

				LoadFormInfo();

				SetZoomFactor();
				ThumbSplitter.SplitPosition = CheckSplitterLocation(ThumbSplitter.Left);

				HideExcelButtonAndRepositionDeliverButton();
			}
			catch (FlexCelXlsAdapterException ex)
			{
				if (ex.ErrorCode is XlsErr.ErrFileIsNotSupported or XlsErr.ErrExcelInvalid)
				{
					Globals.Message.ShowError(ex.Message);
				}
				else
				{
					throw;
				}
			}
			catch (OutOfMemoryException)
			{
				//handle GDI error
				Globals.Message.ShowError(Res.GetString("d050b1d0-14ba-483a-ae09-8c5a8d881391", "One or more images in this document are corrupt or in a format unsupported by GDI+. Check the excel document and images from the registry, if any."),
					Res.GetString("72eae896-7a0a-47ac-b951-a27dd7069cfc", "Generic GDI+ Error"));
			}
			catch (FlexCelCoreException ex)
			{
				if (ex.ErrorCode == FlxErr.ErrCreatingImage)
				{
					string errorMessage = Res.GetString("ce87db03-5da2-4252-b875-2798a34aaada", @"{0} caught when loading the preview form - 

Your system might be low on memory or system resources, please close all other tasks, exit {1}, come back in and try again.

Error message is: {2}

If problems persist, please contact your system administrator and show them this information:

{3}",
						"FlexCelCoreException",
						Core.Constants.ProductName,
						ex.Message,
						TopLevelExceptionHandler.ResourcesMessage());
					Globals.Message.ShowError(errorMessage, Res.GetString("50a7c440-8c30-404f-a8f1-6b5836fb9d86", "Error"));
					hasFlexCelCoreCreatingImageException = true;
				}
				else if (ex.ErrorCode == FlxErr.ErrFontNotSupported)
				{
					string errorMessage = Res.GetString("D093820B-7543-40A8-BCD8-07B6EDA79BE1", @"{0} caught when loading the preview form - 

Your system may have some problems with a font needed for rendering the document, please fix or uninstall this font before trying to run this document again.

Error message is: {1}", "FlexCelCoreException", ex.Message);
					Globals.Message.ShowError(errorMessage, Res.GetString("73273216-fba3-4710-b6b0-71782bcfc9ef", "Error Rendering Font"));
					hasFlexCelCoreCreatingFontException = true;
				}
				else if (ex.ErrorCode == FlxErr.ErrInvalidNumberOfParams)
				{
					string errorMessage = Res.GetString("31C8156E-E434-43D3-BD9C-AA23EC330A2C", @"{0} caught when loading the preview form - 

Your document template have a formula with invalid number of parameters when rendering the document, please fix or remove this formula before trying to run this document again.

Error message is: {1}", "FlexCelCoreException", ex.Message);
					Globals.Message.ShowError(errorMessage, Res.GetString("DCC50410-9A18-4E58-B1B3-3CA557EEBF6C", "Error Rendering Formula Function"));
					hasFlexCelCoreCreatingInvalidParamNumberException = true;
				}
				else if (ex.ErrorCode == FlxErr.ErrUnexpectedChar)
				{
					string errorMessage = Res.GetString("77F9E35E-D116-4553-8C9E-076BB23541B9", @"{0} caught when loading the preview form - 

Your document template have some invalid characters when rendering the document, please fix or remove this formula before trying to run this document again.

Error message is: {1}", "FlexCelCoreException", ex.Message);
					Globals.Message.ShowError(errorMessage, Res.GetString("DCC50410-9A18-4E58-B1B3-3CA557EEBF6C", "Error Rendering Formula Function"));
					hasFlexCelCoreCreatingInvalidCharacterException = true;
				}
				else
				{
					throw;
				}
			}
			finally
			{
				isLoading = false;
			}
		}

		internal XlsFile xlsFile;
		bool hasFlexCelCoreCreatingImageException;

		bool hasFlexCelCoreCreatingFontException;

		bool hasFlexCelCoreCreatingInvalidParamNumberException;

		bool hasFlexCelCoreCreatingInvalidCharacterException;

		#endregion

		void HideExcelButtonAndRepositionDeliverButton()
		{
			bool allowOpenInExcel = false;

			// only need to check first document to see if this is a Report or Document
			if (DeliveryInfos[0].DeliveryFormat == DeliveryInfo.DeliveryFormats.Report)
			{
				if (Env.Security.OpenReportsInExcel.IsAllowed)
				{
					allowOpenInExcel = true;
				}
			}
			else
			{
				IUser currentUser = EnvProxy.Instance.CurrentUser;
				if (currentUser != null && currentUser.IsDeveloper)
				{
					allowOpenInExcel = true;
				}
				else
				{
					if (DeliveryInfos[0].AllowRawView)
					{
						allowOpenInExcel = true;
					}
				}
			}
			if (!allowOpenInExcel)
			{
				OpenInExcelButton.Visible = false;
			}
		}

		#region Shown

		void XLSPreviewForm_Shown(object sender, EventArgs e)
		{
			if (PreviewMain.Visible && PreviewMain.Enabled)
			{
				ActiveControl = PreviewMain;
			}
			if (hasFlexCelCoreCreatingImageException || hasFlexCelCoreCreatingFontException || hasFlexCelCoreCreatingInvalidParamNumberException || hasFlexCelCoreCreatingInvalidCharacterException)
			{
				Close();
			}

			PreviewMain.AutoScrollMinSize = ControlDpiScalingHelper.NewScaledSize(PreviewMain.AutoScrollMinSize);
			PreviewThumbs.AutoScrollMinSize = ControlDpiScalingHelper.NewScaledSize(PreviewThumbs.AutoScrollMinSize);
		}

		#endregion

		#region Zoom / Resize

		void ZoomUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			SetZoomFactor();
		}

		void SetZoomFactor()
		{
			double zoom = ((double)ZoomUpDown.Value) / 100d;
			PreviewMain.Zoom = zoom;
		}

		void PreviewMain_ZoomChanged(object sender, System.EventArgs e)
		{
			ZoomUpDown.Value = (int)Enterprise.ZArchitecture.Core.Utilities.Round((decimal)(PreviewMain.Zoom * 100f), 0);
		}

		void PresetMenuItem_Click(object sender, System.EventArgs e)
		{
			string zoomStr = ((MenuItem)sender).Text;
			int rZoom = Convert.ToInt32(zoomStr.Substring(0, zoomStr.IndexOf("%")).Trim());
			if (rZoom < ZoomUpDown.Minimum)
			{
				rZoom = (int)ZoomUpDown.Minimum;
			}

			if (rZoom > ZoomUpDown.Maximum)
			{
				rZoom = (int)ZoomUpDown.Maximum;
			}

			ZoomUpDown.Value = rZoom;
		}

		void ZoomPresetButton_Click(object sender, System.EventArgs e)
		{
#if !WINZOR
			ZoomMenu.Show(ZoomPresetButton, ControlDpiScalingHelper.NewScaledPoint(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(ZoomPresetButton.Height)));
#else
			ZoomMenu.Show(ZoomPresetButton, ControlDpiScalingHelper.NewScaledPoint(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(ZoomPresetButton.Height)), false);
#endif
		}

		void ThumbSplitter_SplitterMoving(object sender, SplitterEventArgs e)
		{
			e.SplitX = CheckSplitterLocation(e.SplitX);
		}

		int CheckSplitterLocation(int left)
		{
			var maxAllowedSplitterLeft = this.Width - ThumbSplitter.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(MinPreviewMainPanelWidth + (IntendBetweenFormAndPanel * 2));
			if (left > maxAllowedSplitterLeft)
			{
				return maxAllowedSplitterLeft;
			}
			return left;
		}

		const int MinPreviewMainPanelWidth = 662;
		const int IntendBetweenFormAndPanel = 4;

		void XLSPreviewForm_SizeChanged(object sender, EventArgs e)
		{
			ThumbSplitter.SplitPosition = CheckSplitterLocation(ThumbSplitter.Left);
		}

#endregion

		#region Sheet Navigation

#if DEBUG
		protected virtual
#endif
 void SheetsListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (SheetsListBox.SelectedItem is SheetName sheetName)
			{
				xlsFile.ActiveSheetByName = sheetName.StrictName;
				SaveAsFileName = sheetName.EntireName;
				try
				{
					PreviewMain.InvalidatePreview();
				}
				catch (ExternalException exception) when (exception.IsGdiPlusException())
				{
					exception.ReportWithInvalidCharacters(xlsFile);
					throw;
				}
			}
		}

		internal string SaveAsFileName { get; set; }

		void GoToFirstPageButton_Click(object sender, System.EventArgs e)
		{
			PreviewMain.StartPage = 1;
		}

		void GoToLastPageButton_Click(object sender, System.EventArgs e)
		{
			PreviewMain.StartPage = PreviewMain.TotalPages;
		}

		void GoToPrevPageButton_Click(object sender, System.EventArgs e)
		{
			PreviewMain.StartPage--;
		}

		void GoToNextPageButton_Click(object sender, System.EventArgs e)
		{
			PreviewMain.StartPage++;
		}

		void PreviewMain_StartPageChanged(object sender, System.EventArgs e)
		{
			UpdateCurrentPageTextBox();
		}

		void ChangePage()
		{
			string s = CurrentPageTextBox.Text.Trim();
			int pos = 0;
			while (pos < s.Length && s[pos] >= '0' && s[pos] <= '9')
			{
				pos++;
			}

			if (pos > 0)
			{
				int page = PreviewMain.StartPage;
				try
				{
					page = Convert.ToInt32(s.Substring(0, pos));
				}
				catch (OverflowException)
				{
				}
				catch (FormatException)
				{
				}

				PreviewMain.StartPage = page;
			}
			UpdateCurrentPageTextBox();
		}

#if DEBUG
		public TextBox GetCurrentPageTextBox()
		{
			return CurrentPageTextBox;
		}
#endif

		void CurrentPageTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
			{
				ChangePage();
			}

			if (e.KeyChar == (char)27)
			{
				UpdateCurrentPageTextBox();
			}
		}

		void CurrentPageTextBox_Enter(object sender, System.EventArgs e)
		{
			CurrentPageTextBox.SelectAll();
		}

		void CurrentPageTextBox_Leave(object sender, System.EventArgs e)
		{
			ChangePage();
		}

		void UpdateCurrentPageTextBox()
		{
			CurrentPageTextBox.Text = Res.GetString("30a62b79-fb91-447b-a75e-15df42c53fd4", "{0} of {1} (Doc. {2} of {3})", PreviewMain.StartPage, PreviewMain.TotalPages, SheetsListBox.SelectedIndex + 1, SheetsListBox.Items.Count);
		}

		#endregion

		#region Open in Excel

		void OpenInExcelButton_Click(object sender, System.EventArgs e)
		{
			OpenFileInExcel();
		}

		protected virtual void OpenFileInExcel()
		{
			string fileName = Temp.GetTempFileNameWithExtension(ExcelInterface.GetExtensionForExcel(xlsFile.FileFormatWhenOpened));

			try
			{
				XLSStream.CopyToFileViaFlexCel(fileName);

				if (!Globals.IsTest) // don't start Excel during test runs
				{
					FileOpener.Open(fileName);
				}
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				Globals.Message.ShowError(Res.GetString("480f1f5b-9b7a-45b6-8b02-4108be17a701", "Error Opening Excel File: {0}", exception.Message));

				try
				{
					File.Delete(fileName);
				}
				catch { }
			}
		}

		#endregion

		#region Save As

		void SaveAsButton_Click(object sender, EventArgs e)
		{
			var info = GetSaveAsInfo();
			if (info != null)
			{
				var savedFileFullName = info.DisplayFileName;
				var deliveryMethod = new DeliveryMethod();
				DeliveryInfos.ForEach(deliveryMethod.AddFile);
				var firstDeliveryInfo = deliveryMethod.Infos[0];

				using (var stream = info.FileStream)
				using (var excelInterface = new ExcelInterface())
				{
					var mergedStream = deliveryMethod.FileCount > 1 ? deliveryMethod.MergeFilesIntoOneXLS() : firstDeliveryInfo.FileContents;
					try
					{
						DocumentSaveHelper.SaveStream(excelInterface, mergedStream, info.Type, stream, firstDeliveryInfo);
					}
					catch (Exceptions.ExcelLimitationForThisFileFormatException ex) when (info.Type == SaveAsFileType.Xls)
					{
						stream.Dispose();
						if (File.Exists(savedFileFullName))
						{
							File.Delete(savedFileFullName);
						}
						if (ReportErrorManagement.ExcelLimitationsHelper.Messages.ShowTooManyForExcel2003WithFormatSwitchQuestion(ex) != ZDialogResult.Yes)
						{
							return;
						}
						savedFileFullName = Path.ChangeExtension(info.DisplayFileName, nameof(SaveAsFileType.Xlsx));
						using (var newFileStream = File.Create(savedFileFullName))
						{
							DocumentSaveHelper.SaveStream(excelInterface, mergedStream, SaveAsFileType.Xlsx, newFileStream, firstDeliveryInfo);
						}
					}
				}

				Globals.Message.Show(Res.GetString("56A730BE-2570-4E57-8678-4C1A5EF1ACDD", "{0} has been successfully saved.", savedFileFullName));
			}
		}

#if DEBUG
		internal virtual
#endif
		SaveAsFileInfo GetSaveAsInfo()
		{
			return new DocumentSaveHelper().GetSaveAsFileInfo(null, SaveAsFileName);
		}

		#endregion

		#region Deliver

		void DeliverButton_Click(object sender, System.EventArgs e)
		{
			if (!isLoading)
			{
				Deliver();
			}
		}

		void Deliver()
		{
			DeliveryInstructions deliveryInstructions = DeliveryInfos[0].Instructions;
			if (deliveryInstructions != null && deliveryInstructions.DeliveryOptions == AllowedDeliveryOptions.PreviewOnly)
			{
				Globals.Message.ShowError(Res.GetString("85c81d4c-d725-4aea-a319-00a5262630a5", "The document you are previewing has been configured to allow preview only. This means that you cannot deliver this document."));
			}
			else if (DeliverForm == null || DeliverForm.IsFormClosed)
			{
				Globals.Message.ShowError(Res.GetString("7fdf7b98-6dab-40d7-a798-c5e18b084669", "You can not deliver because you have closed the parent form. Please run this report again."));
			}
			else
			{
				if (deliveryInstructions != null)
				{
					deliveryInstructions.IsProceedingToDelivery = true;
				}

				Close();
				DeliverForm.Deliver(deliveryInstructions);
			}
		}

		#endregion

		#region Close

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (isLoading)
			{
				e.Cancel = true;
			}
			else
			{
				base.OnClosing(e);
			}
		}

		internal void CloseButton_Click(object sender, System.EventArgs e)
		{
			if (!isLoading)
			{
				Close();
			}
		}

		void XLSPreviewForm_Closed(object sender, System.EventArgs e)
		{
			PersistFormInfo();
		}

		void PersistFormInfo()
		{
			DocumentsDataRegistry.Instance.DocumentPreviewFormLayout.UserValue =
				new DocumentPreviewFormLayout(ThumbPanelHidden, ThumbPanelSize, (int)ZoomUpDown.Value);
		}

		bool ThumbPanelHidden;
		int ThumbPanelSize;

		void LoadFormInfo()
		{
			if (DocumentsDataRegistry.Instance.DocumentPreviewFormLayout.IsSetByCurrentUser)
			{
				DocumentPreviewFormLayout layout = DocumentsDataRegistry.Instance.DocumentPreviewFormLayout.UserValue;

				int rZoom = layout.ZoomValue;
				if (rZoom < ZoomUpDown.Minimum)
				{
					rZoom = (int)ZoomUpDown.Minimum;
				}

				if (rZoom > ZoomUpDown.Maximum)
				{
					rZoom = (int)ZoomUpDown.Maximum;
				}

				ZoomUpDown.Value = rZoom;

				ThumbPanelHidden = layout.IsThumbnailPanelVisible;

				ThumbPanelSize = layout.ThumbnailPanelPixelWidth;
				if (ThumbPanelSize < 0)
				{
					ThumbPanelSize = 0;
				}

				if (ThumbPanelSize > 500)
				{
					ThumbPanelSize = 500; //Avoid a too big panel.
				}

				UpdateThumbButton();
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (flexCelImgProducer != null)
				{
					flexCelImgProducer.Dispose();
				}

				if (DeliveryInfos != null)
				{
					DeliveryInstructions deliveryInstructions = DeliveryInfos[0].Instructions;

					if (deliveryInstructions != null && !deliveryInstructions.IsProceedingToDelivery && deliveryInstructions.DocPack != null)
					{
						deliveryInstructions.DocPack.Dispose();
					}
				}

				if (deliveryInfosLock != null)
				{
					deliveryInfosLock.Dispose();
				}
			}
			base.Dispose(disposing);
#if !WINZOR
			if (ObjectFactory.Get<TerminalService>().IsRemoteAppSession && DeliverForm is Form parentForm)
			{
				parentForm?.Activate();
			}
#endif
		}

		#endregion

		#region Mouse movement

		void PreviewMain_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				PreviewMain.Cursor = Cursors.NoMove2D;
				Dragging = true;
			}
			else if (e.Button == MouseButtons.Right)
			{
				PreviewMain.Cursor = Cursors.SizeAll;
				Zooming = true;
			}

			if (Dragging || Zooming)
			{
				PreviewMain.Capture = true;
				MouseX = e.X;
				MouseY = e.Y;
				ScrollInitial = PreviewMain.AutoScrollPosition;
				ZoomInitial = PreviewMain.Zoom;
			}
		}

		int MouseX;
		int MouseY;
		Point ScrollInitial;
		double ZoomInitial;
		bool Dragging;
		bool Zooming;

		void PreviewMain_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Dragging = false;
			Zooming = false;
			PreviewMain.Capture = false;
			PreviewMain.Cursor = Cursors.Default;
		}

		void PreviewMain_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (Dragging)
			{
				var unscaledX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ScrollInitial.X - MouseX + e.X);
				var unscaledY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(ScrollInitial.Y - MouseY + e.Y);

				PreviewMain.AutoScrollPosition = ControlDpiScalingHelper.NewScaledPoint(-(unscaledX), -(unscaledY));
			}
			else
				if (Zooming)
			{
				double distance = -MouseX + e.X + -MouseY + e.Y;
				int rZoom = (int)(ZoomInitial * 100 + distance / 10);
				rZoom = rZoom / (int)ZoomUpDown.Increment * (int)ZoomUpDown.Increment; //Make n step zoom growth.
				if (rZoom < ZoomUpDown.Minimum)
				{
					rZoom = (int)ZoomUpDown.Minimum;
				}

				if (rZoom > ZoomUpDown.Maximum)
				{
					rZoom = (int)ZoomUpDown.Maximum;
				}

				ZoomUpDown.Value = rZoom;
			}
		}

		#endregion

		#region Thumb panel

		void UpdateThumbButton()
		{
			if (ThumbPanelHidden)
			{
				ControlDpiScalingHelper.SetWidth(ref ThumbsPanel, ThumbSplitter.MinSize, false);
				HidThumbsButton.Text = ">";
			}
			else
			{
				var scaled50 = ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
				if (ThumbPanelSize < scaled50)
				{
					ThumbPanelSize = scaled50;
				}

				ControlDpiScalingHelper.SetWidth(ref ThumbsPanel, ThumbPanelSize, false);
				HidThumbsButton.Text = "<";
			}
		}

		void HidThumbsButton_Click(object sender, System.EventArgs e)
		{
			ThumbPanelHidden = !ThumbPanelHidden;
			UpdateThumbButton();
		}

		void ThumbsPanel_Resize(object sender, System.EventArgs e)
		{
			if (ThumbsPanel.Width == ThumbSplitter.MinSize)
			{
				ThumbPanelHidden = true;
				PagesPanel.Visible = false;
				SheetsSplitter.Visible = false;
				SheetsListBox.Visible = false;
				PreviewThumbs.Visible = false;
				UpdateThumbButton();
			}
			else
			{
				ThumbPanelHidden = false;
				PagesPanel.Visible = true;
				SheetsSplitter.Visible = true;
				SheetsListBox.Visible = true;
				PreviewThumbs.Visible = true;
				ThumbPanelSize = ThumbsPanel.Width;
				UpdateThumbButton();
			}
		}

		#endregion

		#region Watermark

		void flexCelImgProducer_AfterPaint(object sender, ImgPaintEventArgs e)
		{
			var graphics = ((TGdipUIGraphics)e.Graphics).Handle;
			DrawWatermark(graphics, new SizeF((float)e.PageBounds.Width, (float)e.PageBounds.Height));
		}

		protected virtual void DrawWatermark(Graphics graphics, SizeF size)
		{
			Watermark watermark = GetWatermark();

			if (watermark != null)
			{
				watermark.Draw(graphics, size);
			}
		}

		protected internal Watermark GetWatermark()
		{
			DeliveryInfo deliveryInfo = null;
			if (SheetsListBox.SelectedItem is SheetName sheetName)
			{
				deliveryInfo = DeliveryInfos.FirstOrDefault(d => d.SheetNames.Exists(s => s.StrictName == sheetName.StrictName));
			}

			return deliveryInfo != null ? deliveryInfo.Watermark : DeliveryInfos[0].Watermark;
		}

		#endregion
	}
}
