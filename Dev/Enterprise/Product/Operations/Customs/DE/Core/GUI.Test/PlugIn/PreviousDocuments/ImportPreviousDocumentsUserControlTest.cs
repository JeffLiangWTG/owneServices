using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(Enterprise.Customs.DE.NCTS.Business.NctsPreviousDocument.Schema))]

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class ImportPreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestDocumentGridColumnSize()
		{
			using (var control = new ImportPreviousDocumentsUserControl())
			{
				var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Procedure", 72, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Procedure).Width);
					AssertEquals("CSI_ReferenceNumber", 130, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber).Width);
					AssertEquals("CSI_SubType", 167, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_SubType).Width);
					AssertEquals("CSI_DateOfIssue", 100, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_DateOfIssue).Width);
					AssertEquals("CSI_LineNo", 66, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_LineNo).Width);
					AssertEquals("Status", 105, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.Status).Width);
					AssertEquals("CSI_ReferenceNumber2", 129, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber2).Width);
					AssertEquals("CSI_Description", 160, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Description).Width);
					AssertEquals("FormattedTariff", 106, previousDocumentGrid.GetColumnStyle(NCTS.Business.NctsPreviousDocument.Schema.FormattedTariff).Width);
					AssertEquals("CSI_Quantity", 107, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity).Width);
					AssertEquals("CSI_UnitOfQuantity", 38, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity).Width);
					AssertEquals("CSI_Quantity2", 93, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity2).Width);
					AssertEquals("CSI_UnitOfQuantity2", 38, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity2).Width);
					AssertEquals("CSI_CustomsOffice", 151, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_CustomsOffice).Width);
					AssertEquals("UsualProcessingFlag", 128, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.UsualProcessingFlag).Width);
					AssertEquals("CSI_ItemNumberString", 101, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ItemNumberString).Width);
				});
			}
		}

		public void TestDocumentGridColumnCharacterCasing()
		{
			using (var control = new ImportPreviousDocumentsUserControl())
			{
				var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Procedure", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Procedure).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber", CharacterCasing.Normal, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
					AssertEquals("CSI_SubType", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_SubType).CharacterCasing);
					AssertEquals("Status", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.Status).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber2", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber2).CharacterCasing);
					AssertEquals("CSI_Description", CharacterCasing.Normal, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Description).CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity).CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity2", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity2).CharacterCasing);
					AssertEquals("CSI_CustomsOffice", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_CustomsOffice).CharacterCasing);
				});
			}
		}

		public void TestDocumentsGridColumns()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			var collection = new PreviousDocumentCollection(previousDocument, false);

			using (var control = new ImportPreviousDocumentsUserControl())
			{
				var previousDocumentGrid = (ZGrid)control.Controls.Find("PreviousDocumentsGrid", true).Single();
				previousDocumentGrid.SetDataBinding(collection, ZString.Empty);
				control.Show();

				AssertEquals(16, previousDocumentGrid.ColumnStyles.Count);

				const bool Y = true;
				const bool N = false;
				var expectLibrary = new[]
				{
					new { ReferenceNumber = Y, Procedure = Y, Code = N, SubType = N, DateOfIssue = N, LineNo = N, Status = N, ReferenceNumber2 = N, Description = N, Tariff = N, FormattedTariff = N, Quantity = N, UnitOfQuantity = N, Quantity2 = N, UnitOfQuantity2 = N, AuthorizationNumber = N, UsualProcessingFlag = N, CustomsOffice = N,  Key = PreviousProcedureList.Codes._ATA },
					new { ReferenceNumber = Y, Procedure = Y, Code = N, SubType = N, DateOfIssue = N, LineNo = Y, Status = Y, ReferenceNumber2 = N, Description = Y, Tariff = N, FormattedTariff = N, Quantity = N, UnitOfQuantity = N, Quantity2 = N, UnitOfQuantity2 = N, AuthorizationNumber = N, UsualProcessingFlag = N, CustomsOffice = N,  Key = PreviousProcedureList.Codes._ATAV },
					new { ReferenceNumber = Y, Procedure = Y, Code = N, SubType = Y, DateOfIssue = N, LineNo = Y, Status = N, ReferenceNumber2 = Y, Description = N, Tariff = N, FormattedTariff = N, Quantity = Y, UnitOfQuantity = N, Quantity2 = N, UnitOfQuantity2 = N, AuthorizationNumber = N, UsualProcessingFlag = N, CustomsOffice = N,  Key = PreviousProcedureList.Codes._ATNEU },
					new { ReferenceNumber = Y, Procedure = Y, Code = N, SubType = N, DateOfIssue = N, LineNo = Y, Status = Y, ReferenceNumber2 = N, Description = Y, Tariff = N, FormattedTariff = Y, Quantity = Y, UnitOfQuantity = Y, Quantity2 = Y, UnitOfQuantity2 = Y, AuthorizationNumber = N, UsualProcessingFlag = Y, CustomsOffice = N,  Key = PreviousProcedureList.Codes._ATZL },
					new { ReferenceNumber = N, Procedure = Y, Code = N, SubType = N, DateOfIssue = N, LineNo = N, Status = N, ReferenceNumber2 = N, Description = N, Tariff = N, FormattedTariff = N, Quantity = N, UnitOfQuantity = N, Quantity2 = N, UnitOfQuantity2 = N, AuthorizationNumber = N, UsualProcessingFlag = N, CustomsOffice = N,  Key = PreviousProcedureList.Codes._OHNE },
				}.ToDictionary(x => x.Key, x => x);

				var expectRuleOverrideCodes = new Dictionary<string, string>
				{
					{ "ESUMA", "ATA" },
					{ "GB", "ATA" },
					{ "PUEB", "ATA" },
					{ "T1", "ATA" },
					{ "T2", "ATA" },
					{ "TIR", "ATA" },
					{ "VO", "ATA" },
				};

				CombineAssertions(() =>
				{
					foreach (var code in new PreviousProcedureList().GetAllCodes())
					{
						var expectLibraryCode = expectRuleOverrideCodes.TryGetValue(code, out var expectRuleCodeValue) ? expectRuleCodeValue : expectLibrary.ContainsKey(code) ? code : PreviousProcedureList.Codes._OHNE;
						var expect = expectLibrary[expectLibraryCode];
						control.SetPreviousDocumentsGridColumnsVisible(code);
						AssertEquals($"Code={code}, Field={nameof(expect.ReferenceNumber)} Library={expect.Key}", expect.ReferenceNumber, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ReferenceNumber));
						AssertEquals($"Code={code}, Field={nameof(expect.Procedure)} Library={expect.Key}", expect.Procedure, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Procedure));
						AssertEquals($"Code={code}, Field={nameof(expect.Code)} Library={expect.Key}", expect.Code, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Code));
						AssertEquals($"Code={code}, Field={nameof(expect.SubType)} Library={expect.Key}", expect.SubType, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_SubType));
						AssertEquals($"Code={code}, Field={nameof(expect.DateOfIssue)} Library={expect.Key}", expect.DateOfIssue, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_DateOfIssue));
						AssertEquals($"Code={code}, Field={nameof(expect.LineNo)} Library={expect.Key}", expect.LineNo, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_LineNo));
						AssertEquals($"Code={code}, Field={nameof(expect.Status)} Library={expect.Key}", expect.Status, previousDocumentGrid.Columns.Contains(PreviousDocument.Schema.Status));
						AssertEquals($"Code={code}, Field={nameof(expect.ReferenceNumber2)} Library={expect.Key}", expect.ReferenceNumber2, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ReferenceNumber2));
						AssertEquals($"Code={code}, Field={nameof(expect.Description)} Library={expect.Key}", expect.Description, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Description));
						AssertEquals($"Code={code}, Field={nameof(expect.Tariff)} Library={expect.Key}", expect.Tariff, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Tariff));
						AssertEquals($"Code={code}, Field={nameof(expect.FormattedTariff)} Library={expect.Key}", expect.FormattedTariff, previousDocumentGrid.Columns.Contains(NCTS.Business.NctsPreviousDocument.Schema.FormattedTariff));
						AssertEquals($"Code={code}, Field={nameof(expect.Quantity)} Library={expect.Key}", expect.Quantity, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Quantity));
						AssertEquals($"Code={code}, Field={nameof(expect.UnitOfQuantity)} Library={expect.Key}", expect.UnitOfQuantity, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_UnitOfQuantity));
						AssertEquals($"Code={code}, Field={nameof(expect.Quantity2)} Library={expect.Key}", expect.Quantity2, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Quantity2));
						AssertEquals($"Code={code}, Field={nameof(expect.UnitOfQuantity2)} Library={expect.Key}", expect.UnitOfQuantity2, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_UnitOfQuantity2));
						AssertEquals($"Code={code}, Field={nameof(expect.AuthorizationNumber)} Library={expect.Key}", expect.AuthorizationNumber, previousDocumentGrid.Columns.Contains(PreviousDocument.Schema.AuthorizationNumber));
						AssertEquals($"Code={code}, Field={nameof(expect.UsualProcessingFlag)} Library={expect.Key}", expect.UsualProcessingFlag, previousDocumentGrid.Columns.Contains(PreviousDocument.Schema.UsualProcessingFlag));
						AssertEquals($"Code={code}, Field={nameof(expect.CustomsOffice)} Library={expect.Key}", expect.CustomsOffice, previousDocumentGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_CustomsOffice));
					}
				});
			}
		}

		public void TestAdditionalFieldsShownForProcedure()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			var collection = new PreviousDocumentCollection(previousDocument, false);

			using (var control = new ImportPreviousDocumentsUserControl())
			{
				var previousDocumentGrid = (ZGrid)control.Controls.Find("PreviousDocumentsGrid", true).Single();
				previousDocumentGrid.SetDataBinding(collection, ZString.Empty);
				previousDocumentGrid.ListManager.Position = 0;
				control.Show();

				ControlsHasAndDoesntHaveSubControl(PreviousProcedureList.Codes._ATZL, control, shouldHaveControls: new[] { "PreviousDocumentsImportATZLPanel" }, new[] { "PreviousDocumentsImportATAVPanel" });
				ControlsHasAndDoesntHaveSubControl(PreviousProcedureList.Codes._ATAV, control, shouldHaveControls: new[] { "PreviousDocumentsImportATAVPanel" }, new[] { "PreviousDocumentsImportATZLPanel" });

				var procedureTypesThatShouldShowGridAndProcedure = new[]
				{
					PreviousProcedureList.Codes._ATA,
					PreviousProcedureList.Codes._ESUMA,
					PreviousProcedureList.Codes._GB,
					PreviousProcedureList.Codes._PUEB,
					PreviousProcedureList.Codes._T1,
					PreviousProcedureList.Codes._T2,
					PreviousProcedureList.Codes._TIR,
					PreviousProcedureList.Codes._VO,
					PreviousProcedureList.Codes._ATAV,
					PreviousProcedureList.Codes._ATNEU,
					PreviousProcedureList.Codes._ATZL
				};

				foreach (var procedureTypeThatShouldShowGridAndProcedure in procedureTypesThatShouldShowGridAndProcedure)
				{
					ControlsHasAndDoesntHaveSubControl(procedureTypeThatShouldShowGridAndProcedure, control, shouldHaveControls: new[] { "PreviousDocumentsGrid", "PreviousProcedureDropEdit" });
				}

				var procedureTypesThatShouldJustShowProcedure = new[]
				{
					PreviousProcedureList.Codes._199,
					PreviousProcedureList.Codes._200,
					PreviousProcedureList.Codes._444T1,
					PreviousProcedureList.Codes._444T2,
					PreviousProcedureList.Codes._444TF,
					PreviousProcedureList.Codes._447T1,
					PreviousProcedureList.Codes._447T2,
					PreviousProcedureList.Codes._447TF,
					PreviousProcedureList.Codes._A,
					PreviousProcedureList.Codes._AE,
					PreviousProcedureList.Codes._AV,
					PreviousProcedureList.Codes._ENST2L,
					PreviousProcedureList.Codes._FREIZ,
					PreviousProcedureList.Codes._FV,
					PreviousProcedureList.Codes._MAN,
					PreviousProcedureList.Codes._MO,
					PreviousProcedureList.Codes._OESUMA,
					PreviousProcedureList.Codes._OHNE,
					PreviousProcedureList.Codes._POST,
					PreviousProcedureList.Codes._POUS,
					PreviousProcedureList.Codes._T,
					PreviousProcedureList.Codes._T1CF,
					PreviousProcedureList.Codes._T1DF,
					PreviousProcedureList.Codes._T1IC,
					PreviousProcedureList.Codes._T1IE,
					PreviousProcedureList.Codes._T1IF,
					PreviousProcedureList.Codes._T2AN,
					PreviousProcedureList.Codes._T2CF,
					PreviousProcedureList.Codes._T2DF,
					PreviousProcedureList.Codes._T2F,
					PreviousProcedureList.Codes._T2IC,
					PreviousProcedureList.Codes._T2IE,
					PreviousProcedureList.Codes._T2IF,
					PreviousProcedureList.Codes._T2L,
					PreviousProcedureList.Codes._T2LF,
					PreviousProcedureList.Codes._T2M,
					PreviousProcedureList.Codes._T2SM,
					PreviousProcedureList.Codes._T5,
					PreviousProcedureList.Codes._TRPPVW,
					PreviousProcedureList.Codes._V,
					PreviousProcedureList.Codes._VER321,
					PreviousProcedureList.Codes._VV,
					PreviousProcedureList.Codes._Z,
					PreviousProcedureList.Codes._ZL,
					ZString.Empty.ToString(),
				};

				foreach (var procedureTypeThatShouldJustShowGridAndProcedure in procedureTypesThatShouldJustShowProcedure)
				{
					ControlsHasAndDoesntHaveSubControl(procedureTypeThatShouldJustShowGridAndProcedure, control, shouldHaveControls: new[] { "PreviousProcedureDropEdit" }, shouldntHaveControls: new[] { "PreviousDocumentsGrid" });
				}
			}

			void ControlsHasAndDoesntHaveSubControl(string procedureCode, ImportPreviousDocumentsUserControl control, string[] shouldHaveControls = null, string[] shouldntHaveControls = null)
			{
				control.SetPreviousDocumentsGridColumnsVisible(procedureCode);

				var message = procedureCode;
				if (message == ZString.Empty)
				{
					message = "<Empty String>";
				}

				if (shouldHaveControls != null)
				{
					foreach (var shouldHaveControl in shouldHaveControls)
					{
						AssertEquals(string.Join(": ", message, shouldHaveControl), true, control.Controls.Find(shouldHaveControl, true).SingleOrDefault()?.Visible ?? false);
					}
				}

				if (shouldntHaveControls != null)
				{
					foreach (var shouldntHaveControl in shouldntHaveControls)
					{
						AssertEquals(string.Join(": ", message, shouldntHaveControl), false, control.Controls.Find(shouldntHaveControl, true).SingleOrDefault()?.Visible ?? false);
					}
				}
			}
		}

		[TestDate(2020, 09, 16)]
		public void TestFormattedTariffColumn()
		{
			using (var control = new ImportPreviousDocumentsUserControl())
			{
				var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				var columnStyle = previousDocumentGrid.GetColumnStyle(NCTS.Business.NctsPreviousDocument.Schema.FormattedTariff);
				CombineAssertions(() =>
				{
					AssertType<Universal.GUI.TariffColumnStyleInfo>("ColumnStyleInfo: Type", columnStyle);
					var tariffColumnStyleInfo = columnStyle as Universal.GUI.TariffColumnStyleInfo;
					AssertEquals("ColumnStyleInfo: TariffType", Universal.Constants.TariffTypes.Import, tariffColumnStyleInfo.TariffType);
					AssertEquals("ColumnStyleInfo: GetCountryCode", Core.Constants.CountryCodes.Germany, tariffColumnStyleInfo.GetCountryCode());
					AssertEquals("ColumnStyleInfo: GetDataGrouping", Core.Constants.CountryCodes.Germany, tariffColumnStyleInfo.GetDataGrouping());
					AssertEquals("ColumnStyleInfo: EffectiveDate", new ZDateTime(2020, 09, 16), tariffColumnStyleInfo.GetEffectiveDate.Invoke());
				});
			}
		}

		public void TestImportFromSumARegisterButton()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var collection = new PreviousDocumentCollection(cusEntryInstruction, false);
			collection.AddNew();

			using (var control = new ImportPreviousDocumentsUserControl())
			{
				var previousDocumentGrid = (ZGrid)control.Controls.Find("PreviousDocumentsGrid", true).Single();
				previousDocumentGrid.SetDataBinding(collection, ZString.Empty);
				previousDocumentGrid.ListManager.Position = 0;
				control.Show();

				control.SetPreviousDocumentsGridColumnsVisible(PreviousProcedureList.Codes._ATNEU);
				AssertEquals("ATNEU", true, control.ImportFromSumARegisterButton.Visible);

				control.SetPreviousDocumentsGridColumnsVisible(PreviousProcedureList.Codes._ATAV);
				AssertEquals("Not ATNEU", false, control.ImportFromSumARegisterButton.Visible);
			}
		}

		public void TestImportFromSumARegisterButton_ParentIsNotCusEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var collection = new PreviousDocumentCollection(invoiceLine, false);
			collection.AddNew();

			using (var control = new ImportPreviousDocumentsUserControl())
			{
				var previousDocumentGrid = (ZGrid)control.Controls.Find("PreviousDocumentsGrid", true).Single();
				previousDocumentGrid.SetDataBinding(collection, ZString.Empty);
				previousDocumentGrid.ListManager.Position = 0;
				control.Show();

				control.SetPreviousDocumentsGridColumnsVisible(PreviousProcedureList.Codes._ATNEU);
				AssertEquals(false, control.ImportFromSumARegisterButton.Visible);
			}
		}
	}
}
