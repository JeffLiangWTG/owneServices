using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(NFeExportForm))]
	class NFeExportFormTest : ZFormBasherTest
	{
		public void TestLoadNFeExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.MovementReferenceNumberSetter("2000010001");
			entryheader1.CH_Status = BRMessageStatusList.Codes.Accepted;

			var entryheader2 = declaration.ActiveEntryHeaders.AddNew();
			entryheader2.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

			using (var form = new NFeExportForm(declaration.NFeExportObject))
			{
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("One NFeExportObjectCollection added", 2, declaration.NFeExportObject.Entries.Count);
					AssertNull("No error", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestEntryHeaderGridColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.MovementReferenceNumberSetter("2000010001");
			entryheader1.CH_Status = BRMessageStatusList.Codes.Accepted;

			var entryheader2 = declaration.ActiveEntryHeaders.AddNew();
			entryheader2.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

			using (var form = new NFeExportForm(declaration.NFeExportObject))
			{
				form.Show();
				var columns = new string[]
				{
					"ReferenceNumber",
					"EntryNumber",
					"EntryNumberIssueDate",
					"ReleaseDate"
				};

				CombineAssertions("Entry Header grid columns validation", () =>
				{
					AssertEquals("Grid Coloumn count should be ", columns.Length, form.HeaderGrid.Columns.Count);
					columns.ForEach(column =>
					{
						Assert($"Entry Header grid should contain column {column}", form.HeaderGrid.Columns[column] != null);
					});
				});
			}
		}

		public void TestInvoiceLineGridColumns()
		{
			var messageTypes = new string[] { BRJobMessageTypeList.Codes.ImportSiscomex, BRJobMessageTypeList.Codes.Import };

			foreach (var messageType in messageTypes)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;

				var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
				entryheader1.MovementReferenceNumberSetter("2000010001");
				entryheader1.CH_Status = BRMessageStatusList.Codes.Accepted;

				var entryheader2 = declaration.ActiveEntryHeaders.AddNew();
				entryheader2.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

				using (var form = new NFeExportForm(declaration.NFeExportObject))
				{
					form.Show();
					var columns = declaration.IsImportOnly ? columnsIMP : columnsISW;

					CombineAssertions($"Invoice line grid columns validation when Message Type is {messageType}", () =>
					{
						AssertEquals("Grid Coloumn count should be ", columns.Length, form.LinesGrid.Columns.Count);
						columns.ForEach(column =>
						{
							Assert($"Invoice Line grid should contain column {column}", form.LinesGrid.Columns[column] != null);
						});
					});
				}
			}
		}

		[TestDate(2022, 9, 27)]
		public void TestExportToExcel()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "Importer";
			importer.OH_Code = "TS1";
			importer.PrimaryRegistrationNumber.Number = "97442770000126";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_DeclarationReference = "B00001011";
			declaration.JE_OH_Importer = importer.PK;

			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			entryheader.MovementReferenceNumberSetter("2000010001");
			entryheader.CH_Status = BRMessageStatusList.Codes.Accepted;

			entryheader.MergedLines.AddNew().InvoiceLines.Add(declaration.Invoices.AddNew().InvoiceLines.AddNew());

			var templateFilePath = Path.Combine(Env.TempPath, "B00001011.xls");
			if (File.Exists(templateFilePath))
			{
				File.Delete(templateFilePath);
			}

			using (var form = new NFeExportForm(declaration.NFeExportObject))
			{
				form.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var saveFileDialog = dialog as SaveFileDialog;
					saveFileDialog.FileName = templateFilePath;
				});

				form.ExportDataButton.PerformClick();
				AssertType<SaveFileDialog>(ZFormModaliser.LastCommonDialogShownDialogForTest);

				Assert("An Excel file should be created", File.Exists(templateFilePath));
				File.Delete(templateFilePath);
			}
		}

		string[] columnsISW => new string[]
		{
			NFeInvoiceLineExportObject.Schema.InvoiceLineNumber,
			NFeInvoiceLineExportObject.Schema.InvoiceNumber,
			NFeInvoiceLineExportObject.Schema.ProductCode,
			NFeInvoiceLineExportObject.Schema.TariffCode,
			NFeInvoiceLineExportObject.Schema.GoodsDescription,
			NFeInvoiceLineExportObject.Schema.SupplierName,
			NFeInvoiceLineExportObject.Schema.ManufacturerName,
			NFeInvoiceLineExportObject.Schema.InvoiceQuantity,
			NFeInvoiceLineExportObject.Schema.InvoiceQuantityUQ,
			NFeInvoiceLineExportObject.Schema.CustomsQuantity,
			NFeInvoiceLineExportObject.Schema.CustomsQuantityUQ,
			NFeInvoiceLineExportObject.Schema.FOBValue,
			NFeInvoiceLineExportObject.Schema.FreightValue,
			NFeInvoiceLineExportObject.Schema.InsuranceValue,
			NFeInvoiceLineExportObject.Schema.CIFValue,
			NFeInvoiceLineExportObject.Schema.DutyTaxRegime,
			NFeInvoiceLineExportObject.Schema.DutyRate,
			NFeInvoiceLineExportObject.Schema.DutyAmount,
			NFeInvoiceLineExportObject.Schema.IPITaxRegime,
			NFeInvoiceLineExportObject.Schema.IPIBaseAmount,
			NFeInvoiceLineExportObject.Schema.IPIRate,
			NFeInvoiceLineExportObject.Schema.IPIAmount,
			NFeInvoiceLineExportObject.Schema.IPISpecialRateUQ,
			NFeInvoiceLineExportObject.Schema.IPISpecialRateQuantity,
			NFeInvoiceLineExportObject.Schema.IPISpecialRateAmount,
			NFeInvoiceLineExportObject.Schema.PISCofinsTaxRegime,
			NFeInvoiceLineExportObject.Schema.PISBaseAmount,
			NFeInvoiceLineExportObject.Schema.PISRate,
			NFeInvoiceLineExportObject.Schema.PISAmount,
			NFeInvoiceLineExportObject.Schema.PISSpecialRateUQ,
			NFeInvoiceLineExportObject.Schema.PISSpecialRateQuantity,
			NFeInvoiceLineExportObject.Schema.PISSpecialRateAmount,
			NFeInvoiceLineExportObject.Schema.CofinsBaseAmount,
			NFeInvoiceLineExportObject.Schema.CofinsRate,
			NFeInvoiceLineExportObject.Schema.CofinsAmount,
			NFeInvoiceLineExportObject.Schema.CofinsSpecialRateUQ,
			NFeInvoiceLineExportObject.Schema.CofinsSpecialRateQuantity,
			NFeInvoiceLineExportObject.Schema.CofinsSpecialRateAmount,
			NFeInvoiceLineExportObject.Schema.ICMSTaxRegime,
			NFeInvoiceLineExportObject.Schema.ICMSLegalBase,
			NFeInvoiceLineExportObject.Schema.ICMSBaseAmount,
			NFeInvoiceLineExportObject.Schema.ICMSRate,
			NFeInvoiceLineExportObject.Schema.ICMSReductionPercentage,
			NFeInvoiceLineExportObject.Schema.ICMSAmount,
			NFeInvoiceLineExportObject.Schema.FCPRate,
			NFeInvoiceLineExportObject.Schema.FCPAmount,
			NFeInvoiceLineExportObject.Schema.SiscomexUsageFee,
			NFeInvoiceLineExportObject.Schema.AntidumpingBaseAmount,
			NFeInvoiceLineExportObject.Schema.AntidumpingRate,
			NFeInvoiceLineExportObject.Schema.AntidumpingAmount,
			NFeInvoiceLineExportObject.Schema.AntidumpingSpecialRateUQ,
			NFeInvoiceLineExportObject.Schema.AntidumpingSpecialRateQuantity,
			NFeInvoiceLineExportObject.Schema.AntidumpingSpecialRateAmount,
			NFeInvoiceLineExportObject.Schema.Addition,
			NFeInvoiceLineExportObject.Schema.Nve,
			NFeInvoiceLineExportObject.Schema.ManufacturerIndicator,
			NFeInvoiceLineExportObject.Schema.IcmsTotalAmountReduction,
			NFeInvoiceLineExportObject.Schema.AfrmmAmount,
			NFeInvoiceLineExportObject.Schema.ImportLicenseFineAmount,
			NFeInvoiceLineExportObject.Schema.EICAmount,
			NFeInvoiceLineExportObject.Schema.OrderNumber,
			NFeInvoiceLineExportObject.Schema.OrderLineNumberAndSubLine,
			NFeInvoiceLineExportObject.Schema.ConcessionActNumber,
			NFeInvoiceLineExportObject.Schema.GrossWeight,
			NFeInvoiceLineExportObject.Schema.GrossWeightUQ,
			NFeInvoiceLineExportObject.Schema.NetWeight,
			NFeInvoiceLineExportObject.Schema.NetWeightUQ
		};

		string[] columnsIMP => new string[]
		{
			NFeInvoiceLineExportObject.Schema.InvoiceLineNumber,
			NFeInvoiceLineExportObject.Schema.InvoiceNumber,
			NFeInvoiceLineExportObject.Schema.ProductCode,
			NFeInvoiceLineExportObject.Schema.TariffCode,
			NFeInvoiceLineExportObject.Schema.GoodsDescription,
			NFeInvoiceLineExportObject.Schema.SupplierName,
			NFeInvoiceLineExportObject.Schema.ManufacturerName,
			NFeInvoiceLineExportObject.Schema.InvoiceQuantity,
			NFeInvoiceLineExportObject.Schema.InvoiceQuantityUQ,
			NFeInvoiceLineExportObject.Schema.CustomsQuantity,
			NFeInvoiceLineExportObject.Schema.CustomsQuantityUQ,
			NFeInvoiceLineExportObject.Schema.FOBValue,
			NFeInvoiceLineExportObject.Schema.FreightValue,
			NFeInvoiceLineExportObject.Schema.InsuranceValue,
			NFeInvoiceLineExportObject.Schema.CIFValue,
			NFeInvoiceLineExportObject.Schema.DutyTaxRegime,
			NFeInvoiceLineExportObject.Schema.DutyRate,
			NFeInvoiceLineExportObject.Schema.DutyAmount,
			NFeInvoiceLineExportObject.Schema.IPITaxRegime,
			NFeInvoiceLineExportObject.Schema.IPIBaseAmount,
			NFeInvoiceLineExportObject.Schema.IPIRate,
			NFeInvoiceLineExportObject.Schema.IPIAmount,
			NFeInvoiceLineExportObject.Schema.IPISpecialRateUQ,
			NFeInvoiceLineExportObject.Schema.IPISpecialRateQuantity,
			NFeInvoiceLineExportObject.Schema.IPISpecialRateAmount,
			NFeInvoiceLineExportObject.Schema.PISCofinsTaxRegime,
			NFeInvoiceLineExportObject.Schema.PISBaseAmount,
			NFeInvoiceLineExportObject.Schema.PISRate,
			NFeInvoiceLineExportObject.Schema.PISAmount,
			NFeInvoiceLineExportObject.Schema.PISSpecialRateUQ,
			NFeInvoiceLineExportObject.Schema.PISSpecialRateQuantity,
			NFeInvoiceLineExportObject.Schema.PISSpecialRateAmount,
			NFeInvoiceLineExportObject.Schema.CofinsBaseAmount,
			NFeInvoiceLineExportObject.Schema.CofinsRate,
			NFeInvoiceLineExportObject.Schema.CofinsAmount,
			NFeInvoiceLineExportObject.Schema.CofinsSpecialRateUQ,
			NFeInvoiceLineExportObject.Schema.CofinsSpecialRateQuantity,
			NFeInvoiceLineExportObject.Schema.CofinsSpecialRateAmount,
			NFeInvoiceLineExportObject.Schema.ICMSTaxRegime,
			NFeInvoiceLineExportObject.Schema.ICMSLegalBase,
			NFeInvoiceLineExportObject.Schema.ICMSBaseAmount,
			NFeInvoiceLineExportObject.Schema.ICMSRate,
			NFeInvoiceLineExportObject.Schema.ICMSReductionPercentage,
			NFeInvoiceLineExportObject.Schema.ICMSAmount,
			NFeInvoiceLineExportObject.Schema.FCPRate,
			NFeInvoiceLineExportObject.Schema.FCPAmount,
			NFeInvoiceLineExportObject.Schema.SiscomexUsageFee,
			NFeInvoiceLineExportObject.Schema.AntidumpingBaseAmount,
			NFeInvoiceLineExportObject.Schema.AntidumpingRate,
			NFeInvoiceLineExportObject.Schema.AntidumpingAmount,
			NFeInvoiceLineExportObject.Schema.AntidumpingSpecialRateUQ,
			NFeInvoiceLineExportObject.Schema.AntidumpingSpecialRateQuantity,
			NFeInvoiceLineExportObject.Schema.AntidumpingSpecialRateAmount,
			NFeInvoiceLineExportObject.Schema.Addition,
			NFeInvoiceLineExportObject.Schema.ManufacturerIndicator,
			NFeInvoiceLineExportObject.Schema.IcmsTotalAmountReduction,
			NFeInvoiceLineExportObject.Schema.AfrmmAmount,
			NFeInvoiceLineExportObject.Schema.EICAmount,
			NFeInvoiceLineExportObject.Schema.OrderNumber,
			NFeInvoiceLineExportObject.Schema.OrderLineNumberAndSubLine,
			NFeInvoiceLineExportObject.Schema.ConcessionActNumber,
			NFeInvoiceLineExportObject.Schema.Permits,
			NFeInvoiceLineExportObject.Schema.Complement,
			NFeInvoiceLineExportObject.Schema.GrossWeight,
			NFeInvoiceLineExportObject.Schema.GrossWeightUQ,
			NFeInvoiceLineExportObject.Schema.NetWeight,
			NFeInvoiceLineExportObject.Schema.NetWeightUQ
		};

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.MovementReferenceNumberSetter("2000010001");
			declaration.Factory.Save();
			return new NFeExportForm(declaration.NFeExportObject);
		}

		#endregion
	}
}
