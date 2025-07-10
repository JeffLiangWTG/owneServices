using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using FlexCel.Render;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI
{
	sealed class ExcelPreviewTest : TestCaseWithFactory
	{
		public void TestVisibilityOfExcelButtonForDocument()
		{
			infos[0].DeliveryFormat = DeliveryInfo.DeliveryFormats.Document;
			AssertEquals("Current user should be a developer", true, EnvProxy.Instance.CurrentUser.IsDeveloper);
			using (var xlsStream = PreviewTesterStream)
			{
				infos[0].AllowRawView = false;
				var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, infos);
				xlsForm.Show();
				AssertEquals("XlsForm.OpenInExcelButton.Visible should be true", true, xlsForm.OpenInExcelButton.Visible);
				xlsForm.Close();
			}

			using (var xlsStream = PreviewTesterStream)
			{
				infos[0].AllowRawView = true;
				var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, infos);
				xlsForm.Show();
				AssertEquals("XlsForm.OpenInExcelButton.Visible should be true", true, xlsForm.OpenInExcelButton.Visible);
				xlsForm.Close();
			}

			var testGroup = Factory.New<GlbGroup>();
			testGroup.GG_Code = "BAM";
			testGroup.GG_IsActive = true;
			var testStaff = Factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_LoginName = "HighLander.test1";
			var testLink = Factory.New<GlbGroupLink>();
			testLink.GK_GS = testStaff.PK;
			testLink.GK_GG = testGroup.PK;
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "SDF";
			otherCompany.GC_Name = "SDF Company";
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "OPI";
			Factory.Save();

			using (Env.SetTemporaryUserContext("HighLander.test1", otherBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Current user should not be a developer", false, EnvProxy.Instance.CurrentUser.IsDeveloper);

				using (var xlsStream = PreviewTesterStream)
				{
					infos[0].AllowRawView = false;
					var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, infos);
					xlsForm.Show();
					AssertEquals("XlsForm.OpenInExcelButton.Visible should be false", false, xlsForm.OpenInExcelButton.Visible);
					xlsForm.Close();
				}

				using (var xlsStream = PreviewTesterStream)
				{
					infos[0].AllowRawView = true;
					var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, infos);
					xlsForm.Show();
					AssertEquals("XlsForm.OpenInExcelButton.Visible should be true", true, xlsForm.OpenInExcelButton.Visible);
					xlsForm.Close();
				}
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestTableFormatting()
		{
			infos[0].DeliveryFormat = DeliveryInfo.DeliveryFormats.Report;
			using (var xlsStream = resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TestTableFormatting.xlsx"))
			{
				var xlsForm = new XLSPreviewForm(xlsStream, infos, null);
				xlsForm.Show();
				System.Windows.Forms.Application.DoEvents();
				xlsForm.Close();
			}
		}

		public void TestVisibilityOfExcelButtonForReport()
		{
			infos[0].DeliveryFormat = DeliveryInfo.DeliveryFormats.Report;
			AssertEquals("Current user should be a developer", true, EnvProxy.Instance.CurrentUser.IsDeveloper);
			using (var xlsStream = PreviewTesterStream)
			{
				infos[0].AllowRawView = false;
				var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, infos);
				xlsForm.Show();
				AssertEquals("XlsForm.OpenInExcelButton.Visible should be true", true, xlsForm.OpenInExcelButton.Visible);
				xlsForm.Close();
			}

			using (var xlsStream = PreviewTesterStream)
			{
				infos[0].AllowRawView = true;
				var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, infos);
				xlsForm.Show();
				AssertEquals("XlsForm.OpenInExcelButton.Visible should be true", true, xlsForm.OpenInExcelButton.Visible);
				xlsForm.Close();
			}

			var testGroup = Factory.New<GlbGroup>();
			testGroup.GG_Code = "BAM";
			testGroup.GG_IsActive = true;
			var testStaff = Factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_LoginName = "HighLander.test1";
			var testLink = Factory.New<GlbGroupLink>();
			testLink.GK_GS = testStaff.PK;
			testLink.GK_GG = testGroup.PK;
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "SDF";
			otherCompany.GC_Name = "SDF Company";
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "OPI";
			Factory.Save();

			using (Env.SetTemporaryUserContext("HighLander.test1", otherBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Env.Security.OpenReportsInExcel.IsAllowed = true;
				AssertEquals("Current user should not be a developer", false, EnvProxy.Instance.CurrentUser.IsDeveloper);

				using (var xlsStream = PreviewTesterStream)
				{
					infos[0].AllowRawView = false;
					var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, infos);
					xlsForm.Show();
					AssertEquals("XlsForm.OpenInExcelButton.Visible should be true", true, xlsForm.OpenInExcelButton.Visible);
					xlsForm.Close();
				}

				using (var xlsStream = PreviewTesterStream)
				{
					infos[0].AllowRawView = true;
					var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, infos);
					xlsForm.Show();
					AssertEquals("XlsForm.OpenInExcelButton.Visible should be true", true, xlsForm.OpenInExcelButton.Visible);
					xlsForm.Close();
				}
			}
		}

		public void TestHandleFlexCelException()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);

			using (var xlsStream = PreviewTesterStream)
			using (var xlsForm = new XLSPreviewFormWithFlexCelException(xlsStream, new DeliveryInfo[] { info }))
			{
				xlsForm.Show();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(@$"FlexCelCoreException caught when loading the preview form - 

Your system might be low on memory or system resources, please close all other tasks, exit {BrandingFactory.Instance.ProductName}, come back in and try again.

Error message is: Error in Flexcel when creating an image

If problems persist, please contact your system administrator and show them this information:"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestHandleFlexCelCreatingFontException()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);

			using (var xlsStream = PreviewTesterStream)
			using (var xlsForm = new XLSPreviewFormWithFlexCelCreatingFontException(xlsStream, new DeliveryInfo[] { info }))
			{
				xlsForm.Show();
				AssertEquals(@"FlexCelCoreException caught when loading the preview form - 

Your system may have some problems with a font needed for rendering the document, please fix or uninstall this font before trying to run this document again.

Error message is: Error in Flexcel when creating an font", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestHandleFlexCelCreatingInvalidParamNumberException()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);

			using (var xlsStream = PreviewTesterStream)
			using (var xlsForm = new XLSPreviewFormWithFlexCelInvalidParamNumberException(xlsStream, new DeliveryInfo[] { info }))
			{
				xlsForm.Show();
				AssertEquals(@"FlexCelCoreException caught when loading the preview form - 

Your document template have a formula with invalid number of parameters when rendering the document, please fix or remove this formula before trying to run this document again.

Error message is: Error in Flexcel when rendering a formula", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[RequiresSTA]
		public void TestCulture()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);

			info.IsLocalDocument = true;
			using (var xlsStream = PreviewTesterStream)
			using (var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, new DeliveryInfo[] { info }))
			{
				AssertEquals(true, xlsForm.PreviewMain.IsLocalDocument);
			}

			info.IsLocalDocument = false;
			using (var xlsStream = PreviewTesterStream)
			using (var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, new DeliveryInfo[] { info }))
			{
				AssertEquals(false, xlsForm.PreviewMain.IsLocalDocument);
			}

			using (var xlsStream = PreviewTesterStream)
			using (var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, null))
			{
				AssertEquals(false, xlsForm.PreviewMain.IsLocalDocument);
			}
		}

		[RequiresSTA]
		public void TestLineSpacing()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);

			using (var xlsStream = PreviewTesterStream)
			using (var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, new DeliveryInfo[] { info }))
			{
				xlsForm.Show();
				AssertEquals("Default Linespacing", (double)1, xlsForm.xlsFile.Linespacing);
			}

			info.LineSpacing = 3.5m;
			using (var xlsStream = PreviewTesterStream)
			using (var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, new DeliveryInfo[] { info }))
			{
				xlsForm.Show();
				AssertEquals("XlsForm.Xls.Linespacing", 3.5, xlsForm.xlsFile.Linespacing);
			}
		}

		public void TestFormCaption()
		{
			using (var previewTesterStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(previewTesterStream, infos, null))
			{
				testForm.Show();
				AssertEquals("Preview", testForm.Text);
				AssertEquals("Preview", testForm.FormCaption);
				AssertEquals("Preview", testForm.FormHeading);
			}
		}

		public void TestOpenReportInExcelSecurity()
		{
			var allowOpenReportsInExcel = Env.Security.OpenReportsInExcel;
			bool originalSecurityValue = allowOpenReportsInExcel.IsAllowed;

			using (var xlsStream = PreviewTesterStream)
			{
				var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, infos);

				allowOpenReportsInExcel.IsAllowed = false;
				try
				{
					xlsForm.Show();
					AssertEquals("XlsForm.OpenInExcelButton.Visible should be false", false, xlsForm.OpenInExcelButton.Visible);
				}
				finally
				{
					xlsForm.Close();
				}
			}

			using (var xlsStream = PreviewTesterStream)
			{
				var xlsForm = new XLSPreviewFormWithNoStartingExcel(xlsStream, infos);

				allowOpenReportsInExcel.IsAllowed = true;
				try
				{
					xlsForm.Show();
					AssertEquals("XlsForm.OpenInExcelButton.Visible should be true", true, xlsForm.OpenInExcelButton.Visible);
				}
				finally
				{
					xlsForm.Close();
					allowOpenReportsInExcel.IsAllowed = originalSecurityValue;
				}
			}
		}

		public void TestChangeZoomFactor()
		{
			using (var previewTesterStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(previewTesterStream, infos, null))
			{
				testForm.Show();

				testForm.ZoomUpDown.Value = 100;
				AssertEquals(1.0d, testForm.PreviewMain.Zoom);

				testForm.ZoomUpDown.Value = 80;
				AssertEquals(0.8d, testForm.PreviewMain.Zoom);
			}
		}

		public void TestFormResize()
		{
			using (var previewTesterStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(previewTesterStream, infos, null))
			{
				testForm.Show();

				testForm.Height = 0;
				testForm.Width = 0;

				AssertEquals(testForm.MinimumSize.Height, testForm.Height);
				AssertEquals(testForm.MinimumSize.Width, testForm.Width);
			}
		}

		public void TestSheetNavigation()
		{
			using (var previewTesterStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(previewTesterStream, infos, null))
			{
				testForm.Show();
				testForm.SheetsListBox.SelectedIndex = 0;
				AssertEquals("Sheet1", testForm.xlsFile.SheetName);
				testForm.SheetsListBox.SelectedIndex = 1;
				AssertEquals("Sheet2", testForm.xlsFile.SheetName);
				testForm.SheetsListBox.SelectedIndex = 2;
				AssertEquals("Sheet3", testForm.xlsFile.SheetName);
			}
		}

		public void TestPageNavigation()
		{
			using (var previewTesterStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(previewTesterStream, infos, null))
			{
				testForm.Show();
				testForm.ZoomUpDown.Value = 200;  //With small zoom, we might not be able to go to the last page.
				testForm.Height = 300;  //Also to verify we move to the last page.
				testForm.GoToFirstPageButton.PerformClick();
				AssertEquals(1, testForm.PreviewMain.StartPage);
				testForm.GoToNextPageButton.PerformClick();
				AssertEquals(2, testForm.PreviewMain.StartPage);
				testForm.GoToLastPageButton.PerformClick();
				AssertEquals(8, testForm.PreviewMain.StartPage);
				testForm.GoToPrevPageButton.PerformClick();
				AssertEquals(7, testForm.PreviewMain.StartPage);
			}
		}

		public void TestHideThumbs()
		{
			using (var previewTesterStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(previewTesterStream, infos, null))
			{
				testForm.Show();
				testForm.ThumbsPanel.Width = 150;
				testForm.HidThumbsButton.PerformClick();
				AssertEquals(testForm.ThumbSplitter.MinSize, testForm.ThumbsPanel.Width);
				testForm.HidThumbsButton.PerformClick();
				AssertEquals(testForm.ThumbsPanel.Width, 150);
			}
		}

		public void TestPersistence()
		{
			using (var xlsStream = PreviewTesterStream)
			using (var testForm1 = new XLSPreviewForm(xlsStream, infos, null))
			{
				testForm1.Show();
				testForm1.ZoomUpDown.Value = 133;
				testForm1.ThumbsPanel.Width = 202;
				testForm1.HidThumbsButton.PerformClick();
				testForm1.Close();
			}

			using (var xlsStream = PreviewTesterStream)
			using (var testForm2 = new XLSPreviewForm(xlsStream, infos, null))
			using (var testForm = new XLSPreviewForm(xlsStream, infos, null))
			{
				testForm2.Show();
				AssertEquals((int)testForm2.ZoomUpDown.Value, 133);
				AssertEquals(testForm2.ThumbsPanel.Width, testForm.ThumbSplitter.MinSize);
				testForm2.HidThumbsButton.PerformClick();
				AssertEquals(testForm2.ThumbsPanel.Width, 202);
				testForm2.Close();
			}
		}

		public void TestFailedDeliver()
		{
			using (var previewTesterStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(previewTesterStream, infos, null))
			{
				testForm.Show();
				testForm.DeliverButton.PerformClick();
				AssertEquals("You can not deliver because you have closed the parent form. Please run this report again.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var instructions = new DeliveryInstructions();

				using (var form = new DocDeliveryForm(instructions, Env.Security.None))
				{
					using (var xlsStream = PreviewTesterStream)
					using (var testForm1 = new XLSPreviewForm(xlsStream, infos, form))
					{
						testForm1.Show();
						testForm1.DeliverButton.PerformClick();
						AssertEquals(testForm1.Visible, false);
						AssertEquals("You have not specified any recipients.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestDeliver_PreviewOnly()
		{
			var instructions = new DeliveryInstructions();
			instructions.DeliveryOptions = AllowedDeliveryOptions.PreviewOnly;
			infos[0].Instructions = instructions;

			using (var form = new DocDeliveryForm(instructions, Env.Security.None))
			{
				using (var xlsStream = PreviewTesterStream)
				using (var testForm1 = new XLSPreviewForm(xlsStream, infos, form))
				{
					testForm1.Show();
					testForm1.DeliverButton.PerformClick();
					AssertEquals("Preview form still visible", testForm1.Visible, true);
					AssertEquals("The document you are previewing has been configured to allow preview only. This means that you cannot deliver this document.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDeliver_CloseDeliverFormBeforePreview()
		{
			var instructions = new DeliveryInstructions();
			infos[0].Instructions = instructions;

			using (var form = new DocDeliveryForm(instructions, Env.Security.None))
			{
				using (var xlsStream = PreviewTesterStream)
				using (var testForm1 = new XLSPreviewForm(xlsStream, infos, form))
				{
					bool exception = false;
					testForm1.Show();
					form.Close();
					try
					{
						testForm1.CloseButton.PerformClick();
					}
					catch
					{
						exception = true;
					}
					Assert("No exception raised while closing preview if deliver form had already been closed", !exception);
				}
			}
		}

		[RequiresSTA]
		public void TestWorkingDeliver()
		{
			var instructions = new DeliveryInstructions();
			instructions.DocumentPackCount = 1;
			Enterprise.MasterFiles.Business.DocDeliveryContact testContact;
			testContact = new Enterprise.MasterFiles.Business.DocDeliveryContact(Factory);
			testContact.Name = "Zubin";
			testContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			testContact.DeliveryAddress = "+61290251199";
			instructions.Recipients.Add(testContact);

			var testReport = new Report(new DocumentPack(), null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			instructions.DeliverablesToBePrinted.Add(testReport);

			using (var form = new DocDeliveryForm(instructions, Env.Security.None))
			{
				using (var xlsStream = PreviewTesterStream)
				using (var testForm1 = new XLSPreviewForm(xlsStream, infos, form))
				{
					testForm1.Show();
					testForm1.DeliverButton.PerformClick();
					AssertEquals(testForm1.Visible, false);
					AssertNull("Error NOT displayed", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(DialogResult.OK, ((DocDeliveryForm)testForm1.DeliverForm).DialogResult);
				}
			}
		}

		public void TestAfterPaintIsAssignedAndWatermarkIsDrawn()
		{
			info.ShowDraftWatermark = true;
			using (var xlsStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(xlsStream, infos, null))
			{
				var reflectionInfo = typeof(FlexCelImgExport).GetField("AfterPaint", BindingFlags.NonPublic | BindingFlags.Instance);
				FlexCel.Render.PaintEventHandler value = (FlexCel.Render.PaintEventHandler)reflectionInfo.GetValue(testForm.flexCelImgProducer);
				AssertNotNull("Event handler should be hooked up to AfterPaint", value);
				testForm.Show();

				using (var img = new Bitmap(10, 10))
				{
					img.SetPixel(5, 5, Color.White);
					var g = Graphics.FromImage(img);
					var rectangle = new RectangleF(new PointF(0, 0), new SizeF(1, 1));
					value.DynamicInvoke(new object[] { testForm, new FlexCel.Render.ImgPaintEventArgs(g, rectangle, 1, 1, 1) });
					Assert("Colour should not be white, should have watermark drawn", img.GetPixel(5, 5) != Color.White);
				}
			}
		}

		[RequiresSTA]
		public void TestDeliveryInfoIsSet()
		{
			using (var previewTesterStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(previewTesterStream, infos, null))
			{
				AssertEquals(infos, testForm.DeliveryInfos);
			}
		}

		[ExpectNoExceptions]
		public void TestGetWatermark()
		{
			foreach (var info in infos)
			{
				info.ShowDraftWatermark = true;
			}

			using (var excelStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(excelStream, infos, null))
			{
				testForm.Show();
				testForm.xlsFile.ActiveSheet = 1;
				testForm.GetWatermark();
				System.Windows.Forms.Application.DoEvents();

				testForm.xlsFile.ActiveSheet = 2;
				testForm.GetWatermark();
				System.Windows.Forms.Application.DoEvents();

				testForm.xlsFile.ActiveSheet = 3;
				testForm.GetWatermark();
				System.Windows.Forms.Application.DoEvents();
			}
		}

		[ExpectNoExceptions]
		public void TestGetWatermarkWithMultipleSheetsAndSingleDeliveryInfo()
		{
			var infos = new DeliveryInfo[] { info };
			info.ShowDraftWatermark = true;

			using (var excelStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(excelStream, infos, null))
			{
				testForm.Show();
				testForm.SheetsListBox.SelectedIndex = 0;
				testForm.GetWatermark();
				System.Windows.Forms.Application.DoEvents();

				testForm.SheetsListBox.SelectedIndex = 1;
				testForm.GetWatermark();
				System.Windows.Forms.Application.DoEvents();

				testForm.SheetsListBox.SelectedIndex = 2;
				testForm.GetWatermark();
				System.Windows.Forms.Application.DoEvents();
			}
		}

		public void TestGetMultipleWatermarksWhenLastDeliveryInfoDoesNotHaveWatermark()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			{
				var info1 = CreateDeliveryInfo("Sheet1", "watermark01");
				var info2 = CreateDeliveryInfo("Sheet2", "watermark02");
				var info3 = CreateDeliveryInfo("Sheet3", string.Empty);

				var infos = new[] { info1, info2, info3 };

				using (var excelStream = PreviewTesterStream)
				using (var form = new XLSPreviewForm(excelStream, infos, null))
				{
					form.Show();
					AssertEquals(3, form.SheetNames.Count);

					form.SheetsListBox.SelectedIndex = 0;
					AssertEquals("Sheet1", form.xlsFile.SheetName);
					AssertEquals("watermark01", form.GetWatermark().AsText);

					form.SheetsListBox.SelectedIndex = 1;
					AssertEquals("Sheet2", form.xlsFile.SheetName);
					AssertEquals("watermark02", form.GetWatermark().AsText);

					form.SheetsListBox.SelectedIndex = 2;
					AssertEquals("Sheet3", form.xlsFile.SheetName);
					AssertNull(form.GetWatermark());
				}
			}
		}

		DeliveryInfo CreateDeliveryInfo(string sheetName, string watermark)
		{
			var report = new Report(new DocumentPack(), null);
			report.CustomWatermarkText = (NoResString)watermark;
			var info = report.GetDeliveryInfo(false);
			info.SheetNames.Add(new SheetName { StrictName = sheetName, EntireName = sheetName + " With Loooooooooooooooooooong suffix" });

			if (!string.IsNullOrEmpty(watermark))
			{
				AssertEquals(watermark, info.Watermark.AsText);
			}
			else
			{
				AssertNull(info.Watermark);
			}

			return info;
		}

		public void TestGetMultipleWatermarksWhenFirstDeliveryInfoDoesNotHaveWatermark()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			{
				var info1 = CreateDeliveryInfo("Sheet1", string.Empty);
				var info2 = CreateDeliveryInfo("Sheet2", "watermark01");
				var info3 = CreateDeliveryInfo("Sheet3", "watermark02");

				var infos = new[] { info1, info2, info3 };

				using (var excelStream = PreviewTesterStream)
				using (var form = new XLSPreviewForm(excelStream, infos, null))
				{
					form.Show();
					AssertEquals(3, form.SheetNames.Count);

					form.SheetsListBox.SelectedIndex = 0;
					AssertEquals("Sheet1", form.xlsFile.SheetName);
					AssertNull(form.GetWatermark());

					form.SheetsListBox.SelectedIndex = 1;
					AssertEquals("Sheet2", form.xlsFile.SheetName);
					AssertEquals("watermark01", form.GetWatermark().AsText);

					form.SheetsListBox.SelectedIndex = 2;
					AssertEquals("Sheet3", form.xlsFile.SheetName);
					AssertEquals("watermark02", form.GetWatermark().AsText);
				}
			}
		}

		#region TestNotClosesWhileIsLoading

		public void TestNotClosesWhileIsLoading()
		{
			AssertNotClosesWhileIsLoading(form => form.Close());
			AssertNotClosesWhileIsLoading(form => form.CloseButton_Click(form, EventArgs.Empty));
		}

		public void AssertNotClosesWhileIsLoading(Action<XLSPreviewForm> action)
		{
			using (var excelStream = PreviewTesterStream)
			using (var testForm = new XLSPreviewForm(excelStream, infos, null))
			{
				testForm.Show();
				Assert("Precondition", !testForm.IsDisposed);
				Assert("Precondition", testForm.Visible);

				testForm.isLoading = true;
				action(testForm);
				Assert("Should not be disposed", !testForm.IsDisposed);
				Assert("Should not be closed", testForm.Visible);

				testForm.isLoading = false;
				action(testForm);
				Assert("Should be disposed", testForm.IsDisposed);
				Assert("Should be closed", !testForm.Visible);
			}
		}

		#endregion

		public void TestXlsPreviewFormWithLongSheetName()
		{
			var info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report);
			info1.SheetNames.Add(new SheetName { StrictName = "Sheet1", EntireName = "Sheet1 With Loooooooooooooooooooong suffix" });
			var info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report);
			info2.SheetNames.Add(new SheetName { StrictName = "Sheet2", EntireName = "Sheet2 With Loooooooooooooooooooong suffix" });
			var info3 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report);
			info3.SheetNames.Add(new SheetName { StrictName = "Sheet3", EntireName = "Sheet3 With Loooooooooooooooooooong suffix" });

			var infos = new[] { info1, info2, info3 };

			using (var xlsStream = PreviewTesterStream)
			using (var form = new XLSPreviewForm(xlsStream, infos, null))
			{
				form.Show();

				AssertEquals(3, form.SheetNames.Count);
				AssertEquals("Sheet1 With Loooooooooooooooooooong suffix", form.SheetsListBox.Items[0].ToString());
				AssertEquals("Sheet2 With Loooooooooooooooooooong suffix", form.SheetsListBox.Items[1].ToString());
				AssertEquals("Sheet3 With Loooooooooooooooooooong suffix", form.SheetsListBox.Items[2].ToString());

				form.SheetsListBox.SelectedIndex = 0;
				AssertEquals("Sheet1", form.xlsFile.SheetName);
				form.SheetsListBox.SelectedIndex = 1;
				AssertEquals("Sheet2", form.xlsFile.SheetName);
				form.SheetsListBox.SelectedIndex = 2;
				AssertEquals("Sheet3", form.xlsFile.SheetName);
				AssertEquals($"In the preview form, the file name should be \nSheet3 With Loooooooooooooooooooong suffix \n but it is \n{form.SaveAsFileName}.", "Sheet3 With Loooooooooooooooooooong suffix", form.SaveAsFileName);
			}
		}

		DeliveryInfo info;
		DeliveryInfo[] infos;

		protected override void SetUp()
		{
			base.SetUp();
			info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report);
			infos = new DeliveryInfo[] { info, info, info, info };
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly));

		Stream PreviewTesterStream => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.PreviewTester.xls");

		sealed class XLSPreviewFormWithNoStartingExcel : XLSPreviewForm
		{
			public XLSPreviewFormWithNoStartingExcel(Stream xLSStream, DeliveryInfo[] deliveryInfos)
				: base(xLSStream, deliveryInfos, null)
			{
			}

			protected override void OpenFileInExcel()
			{
				// Do nothing as this starts Excel.
			}
		}

		sealed class XLSPreviewFormWithFlexCelException : XLSPreviewForm
		{
			public XLSPreviewFormWithFlexCelException(Stream xLSStream, DeliveryInfo[] deliveryInfos)
				: base(xLSStream, deliveryInfos, null)
			{
			}

			protected override void SheetsListBox_SelectedIndexChanged(object sender, System.EventArgs e)
			{
				throw new FlexCelCoreException("Error in Flexcel when creating an image", FlxErr.ErrCreatingImage);
			}
		}

		sealed class XLSPreviewFormWithFlexCelCreatingFontException : XLSPreviewForm
		{
			public XLSPreviewFormWithFlexCelCreatingFontException(Stream xLSStream, DeliveryInfo[] deliveryInfos)
				: base(xLSStream, deliveryInfos, null)
			{
			}

			protected override void SheetsListBox_SelectedIndexChanged(object sender, System.EventArgs e)
			{
				throw new FlexCelCoreException("Error in Flexcel when creating an font", FlxErr.ErrFontNotSupported);
			}
		}

		sealed class XLSPreviewFormWithFlexCelInvalidParamNumberException : XLSPreviewForm
		{
			public XLSPreviewFormWithFlexCelInvalidParamNumberException(Stream xLSStream, DeliveryInfo[] deliveryInfos)
				: base(xLSStream, deliveryInfos, null)
			{
			}

			protected override void SheetsListBox_SelectedIndexChanged(object sender, System.EventArgs e)
			{
				throw new FlexCelCoreException("Error in Flexcel when rendering a formula", FlxErr.ErrInvalidNumberOfParams);
			}
		}
	}
}
