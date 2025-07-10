using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.AsycudaCustoms.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Constants = Enterprise.Customs.Universal.Constants;
using CusEntryInstruction = Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction;
using CusInBondContainer = Enterprise.Customs.AsycudaCustoms.Business.CusInBondContainer;
using CusInBondMoveHeader = Enterprise.Customs.AsycudaCustoms.Business.CusInBondMoveHeader;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestUnlockGuaranteeManagementOnDisposing()
		{
			var helper = new GuaranteeTestHelper(Factory);
			var guarantee = helper.CreateValidLinkedGuarantee();
			using (var form = new ZForm(guarantee.Instruction.JobDeclaration))
			using (var entryInstructionDetailsUserControl = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(entryInstructionDetailsUserControl);
				form.Show();
			}
			AssertEquals(false, guarantee.Instruction.GuaranteeManagementMutex.HasLock);
		}

		public void TestInitializeEntryInstructionsGrid()
		{
			using (var control = new EntryInstructionDetailsUserControl())
			{
				AssertNotNull("Have CEI_OA_Warehouse column", control.EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_OA_Warehouse));
				AssertNotNull("Have CEI_OA_Warehouse2 column", control.EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_OA_Warehouse2));
				AssertEquals("CEI_DateForDuty column caption", "Date for Duty", control.EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_DateForDuty).CaptionResourceString.Caption);
			}
		}

		public void TestControls()
		{
			using (var form = new ZForm())
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("CEI_DateForDuty control caption", "Date for Duty", control.FindSingle<ZDateEdit>("AssessmentDateEdit").CaptionResourceString.Caption);
					AssertEquals("BondHolderOrganisationControl", false, control.FindSingle<ZOrganisationControl>("BondHolderOrganisationControl").Visible);
					AssertEquals("NewOwnerOrganisationControl", false, control.FindSingle<ZOrganisationControl>("NewOwnerOrganisationControl").Visible);
					AssertEquals("RemoverOrganisationControl", false, control.FindSingle<ZOrganisationControl>("RemoverOrganisationControl").Visible);
					AssertEquals("GuaranteeUserControl", true, control.FindSingle<GuaranteeUserControl>("GuaranteeUserControl").Visible);
					AssertEquals("OtherCustomsInformationControl", true, control.FindSingle<OtherCustomsInformationControl>("OtherCustomsInformationControl").Visible);
				});
			}
		}

		public void TestEntryInstructionsGridColumnOrder()
		{
			var columnsInOrder = new[]
			{
				CusEntryInstruction.Schema.CEI_Style,
				CusEntryInstruction.Schema.CEI_Description,
				CusEntryInstruction.Schema.CEI_OA_Warehouse,
				CusEntryInstruction.Schema.CEI_OA_Warehouse2,
				CusEntryInstruction.Schema.CEI_DateForDuty,
				CusEntryInstruction.Schema.EntryNumber,
			};

			using (var control = new EntryInstructionDetailsUserControl())
			{
				var columnStyles = control.EntryInstructionsGrid.ColumnStyles;
				for (var i = 0; i < columnsInOrder.Length; i++)
				{
					AssertEquals(columnStyles[i].GetPropertyValue("ColumnName"), columnsInOrder[i]);
				}
			}
		}

		public void TestCusInBondPermitsGridColumnOrder()
		{
			var columnsInOrder = new[]
			{
				CusInBondMoveHeader.Schema.BM_Calc_PermitNumber,
				CusInBondMoveHeader.Schema.BM_Calc_IssueDate,
				CusInBondMoveHeader.Schema.BM_ArrivalDate,
				CusInBondMoveHeader.Schema.BM_Calc_ValidityDate,
				CusInBondMoveHeader.Schema.BM_AdditionalText,
				CusInBondMoveHeader.Schema.BM_MonetaryValue,
				CusInBondMoveHeader.Schema.BM_NetWeight,
				CusInBondMoveHeader.Schema.BM_CustomsQuantity,
			};

			using (var control = new EntryInstructionDetailsUserControl())
			{
				var columnList = control.FindSingle<ZGrid>("CusInBondPermitsGrid").ColumnStyles;
				for (var i = 0; i < columnsInOrder.Length; i++)
				{
					AssertEquals(columnList[i].GetPropertyValue("ColumnName"), columnsInOrder[i]);
				}
			}
		}

		public void TestTransitPermitsTabPageTabVisible()
		{
			const string TransitPermitsTabName = "TransitPermitsTabPage";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var transitProcedure1 = helper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "10", "71", "F61", "", "IMP", "10P");
			transitProcedure1.ZZ6_IntoWarehouse = YesNoList.Codes.Yes;
			transitProcedure1.ZZ6_OutOfWarehouse = YesNoList.Codes.No;
			transitProcedure1.ZZ6_IsTransit = YesNoList.Codes.Yes;

			var transitProcedure2 = helper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "11", "71", "F61", "", "IMP", "10P");
			transitProcedure2.ZZ6_IntoWarehouse = YesNoList.Codes.Yes;
			transitProcedure2.ZZ6_OutOfWarehouse = YesNoList.Codes.No;
			transitProcedure2.ZZ6_IsTransit = YesNoList.Codes.Yes;

			var nonTransitProcedure = helper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "12", "71", "F61", "", "IMP", "10P");
			nonTransitProcedure.ZZ6_IntoWarehouse = YesNoList.Codes.No;
			nonTransitProcedure.ZZ6_OutOfWarehouse = YesNoList.Codes.No;
			nonTransitProcedure.ZZ6_IsTransit = YesNoList.Codes.No;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var entryInstructionDetailsUserControl = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(entryInstructionDetailsUserControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertTabPageTabVisible("invisible when empty", entryInstructionDetailsUserControl, TransitPermitsTabName, false);

					var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
					AssertTabPageTabVisible("invisible when no invoice line linked", entryInstructionDetailsUserControl, TransitPermitsTabName, false);

					invoiceLine.JI_CEI = entryInstruction.PK;
					SetInvoiceLineProcedure(invoiceLine, transitProcedure1);

					AssertTabPageTabVisible("visible", entryInstructionDetailsUserControl, TransitPermitsTabName, true);

					SetInvoiceLineProcedure(invoiceLine, nonTransitProcedure);
					AssertTabPageTabVisible("invisible when non-transit procedure", entryInstructionDetailsUserControl, TransitPermitsTabName, false);

					SetInvoiceLineProcedure(invoiceLine, transitProcedure2);
					AssertTabPageTabVisible("visible when new transit procedure", entryInstructionDetailsUserControl, TransitPermitsTabName, true);

					invoiceLine.Delete();
					AssertTabPageTabVisible("invisible when no invoice line", entryInstructionDetailsUserControl, TransitPermitsTabName, false);
				});
			}
		}

		public void TestTransitPermitsContainerGrid()
		{
			using (var control = new EntryInstructionDetailsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("PermitContainersGrid");
				AssertContainsExactElementsInAnyOrder(new[] { CusInBondContainer.Schema.BC_ContainerNum },
					grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestRiskGroupVisible_IfRiskEnabled()
		{
			AssertRiskTabPageTabVisible(true);
		}

		public void TestRiskGroupVisible_IfRiskNotEnabled()
		{
			AssertRiskTabPageTabVisible(false);
		}

		public void TestContextMenu()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var transitProcedure1 = helper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "10", "71", "F61", "", "IMP", "10P");
			transitProcedure1.ZZ6_IntoWarehouse = YesNoList.Codes.Yes;
			transitProcedure1.ZZ6_OutOfWarehouse = YesNoList.Codes.No;
			transitProcedure1.ZZ6_IsTransit = YesNoList.Codes.Yes;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var moveHeader = entryInstruction.CusInBondPermitsHeaders.AddNew();
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "NUMBER";
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "NUMBER1";
			invoiceLine.JI_CEI = entryInstruction.PK;
			SetInvoiceLineProcedure(invoiceLine, transitProcedure1);

			using (var form = new ZForm(declaration))
			using (var entryInstructionDetailsUserControl = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(entryInstructionDetailsUserControl);
				form.Show();

				CombineAssertions(() =>
				{
					var grid = entryInstructionDetailsUserControl.FindSingle<ZGrid>("PermitContainersGrid");
					var menuItem = grid.ContextMenu.MenuItems.FindByText("Add All Containers");
					AssertNotNull(menuItem);
					AssertEquals("containers before added", 0, moveHeader.FirstMoveDetail.Containers.Count);

					entryInstructionDetailsUserControl.FindSingle<ZGrid>("CusInBondPermitsGrid").Select(0);
					menuItem.PerformClick();

					AssertEquals("containers added", 2, moveHeader.FirstMoveDetail.Containers.Count);
					AssertEquals("container added BO", "NUMBER", moveHeader.FirstMoveDetail.Containers[0].BC_ContainerNum);
					AssertEquals("container1 added BO", "NUMBER1", moveHeader.FirstMoveDetail.Containers[1].BC_ContainerNum);
					AssertEquals("container in grid", "NUMBER", grid[0, 0].ToString());
					AssertEquals("container1 in grid", "NUMBER1", grid[1, 0].ToString());
				});
			}
		}

		void AssertRiskTabPageTabVisible(bool isRiskEnabled)
		{
			const string RiskTabName = "RiskTabPage";

			var nonIntoRegimeProcedure = CreateNonIntoRegimeProcedure();
			var procedures = CreateIntoRegimeProcedureHolders();
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.Risk,
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				ZDateTime.Today,
				value: isRiskEnabled))
			{
				for (var procedureIndex = 0; procedureIndex < procedures.Length; ++procedureIndex)
				{
					var procedure = procedures[procedureIndex];

					var declaration = Factory.New<JobDeclaration>();
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;

					using (var form = new ZForm(declaration))
					using (var detailsUserControl = new EntryInstructionDetailsUserControl())
					{
						form.Controls.Add(detailsUserControl);
						form.Show();

						CombineAssertions(() =>
						{
							AssertTabPageTabVisible(
								"invisible when no entry instruction",
								detailsUserControl,
								RiskTabName,
								visible: false);

							var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
							AssertTabPageTabVisible(
								"invisible when no invoice line linked",
								detailsUserControl,
								RiskTabName,
								visible: false);

							invoiceLine.JI_CEI = entryInstruction.PK;
							SetInvoiceLineProcedure(invoiceLine, procedure);

							AssertTabPageTabVisible(
								isRiskEnabled ? "visible when into-regime procedure" : "invisible when into-regime due to risk disabled",
								detailsUserControl,
								RiskTabName,
								visible: isRiskEnabled);

							SetInvoiceLineProcedure(invoiceLine, nonIntoRegimeProcedure);
							AssertTabPageTabVisible(
								isRiskEnabled ? "invisible after switching to a non into-regime procedure" : "invisible when switching to a non into-regime procedure due to risk disabled",
								detailsUserControl,
								RiskTabName,
								visible: false);

							// just switch to another into-regime procedure
							SetInvoiceLineProcedure(invoiceLine, procedures[procedures.Length - procedureIndex - 1]);
							AssertTabPageTabVisible(
								isRiskEnabled ? "visible after switching to a new into-regime procedure" : "invisible when procedure changes due to risk disabled",
								detailsUserControl,
								RiskTabName,
								visible: isRiskEnabled);

							invoiceLine.Delete();
							AssertTabPageTabVisible(
								isRiskEnabled ? "invisible when no invoice line" : "invisible when no invoice line",
								detailsUserControl,
								RiskTabName,
								visible: false);
						});
					}
				}
			}
		}

		public void TestRiskTabPageVisible_IfRiskManagementRecordExists()
		{
			const string RiskTabeName = "RiskTabPage";

			var nonIntoRegimeProcedure = CreateNonIntoRegimeProcedure();
			var procedure = CreateIntoRegimeProcedureHolders()[0];
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Universal.Constants.FunctionalityTypes.Risk,
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				ZDateTime.Today,
				value: true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();

				using (var form = new ZForm(declaration))
				using (var detailsUserControl = new EntryInstructionDetailsUserControl())
				{
					form.Controls.Add(detailsUserControl);
					form.Show();

					CombineAssertions(() =>
					{
						var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

						invoiceLine.JI_CEI = entryInstruction.PK;
						SetInvoiceLineProcedure(invoiceLine, procedure);

						AssertTabPageTabVisible(
							"visible",
							detailsUserControl,
							RiskTabeName,
							visible: true);

						var riskRecord = entryInstruction.RiskManagements.AddNew();

						SetInvoiceLineProcedure(invoiceLine, nonIntoRegimeProcedure);
						AssertTabPageTabVisible(
							"still visible when non into-regime procedure but risk management exists",
							detailsUserControl,
							RiskTabeName,
							visible: true);

						riskRecord.Delete();
						AssertTabPageTabVisible(
							"invisible after risk management record is deleted",
							detailsUserControl,
							RiskTabeName,
							visible: false);
					});
				}
			}
		}

		RefCusProcedure CreateNonIntoRegimeProcedure()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "15", "71", "F61", "", "IMP", "10P");
			procedure.ZZ6_IntoWarehouse = YesNoList.Codes.No;
			procedure.ZZ6_IntoInwardProcessing = YesNoList.Codes.No;
			procedure.ZZ6_IntoOutwardProcessing = YesNoList.Codes.No;
			procedure.ZZ6_IntoTemporaryImport = YesNoList.Codes.No;
			procedure.ZZ6_IntoTemporaryExport = YesNoList.Codes.No;
			procedure.ZZ6_IsTransit = YesNoList.Codes.No;

			return procedure;
		}

		RefCusProcedure[] CreateIntoRegimeProcedureHolders()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "20", "71", "F61", "", "IMP", "10P");
			procedure1.ZZ6_IntoWarehouse = YesNoList.Codes.Yes;

			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "21", "71", "F61", "", "IMP", "10P");
			procedure2.ZZ6_IntoInwardProcessing = YesNoList.Codes.Yes;

			var procedure3 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "22", "71", "F61", "", "IMP", "10P");
			procedure3.ZZ6_IntoOutwardProcessing = YesNoList.Codes.Yes;

			var procedure4 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "23", "71", "F61", "", "IMP", "10P");
			procedure4.ZZ6_IntoTemporaryImport = YesNoList.Codes.Yes;

			var procedure5 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "24", "71", "F61", "", "IMP", "10P");
			procedure5.ZZ6_IntoTemporaryExport = YesNoList.Codes.Yes;

			var procedure6 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "25", "71", "F61", "", "IMP", "10P");
			procedure6.ZZ6_IsTransit = YesNoList.Codes.Yes;

			return new RefCusProcedure[] { procedure1, procedure2, procedure3, procedure4, procedure5, procedure6 };
		}

		static void SetInvoiceLineProcedure(JobComInvoiceLine invoiceLine, RefCusProcedure procedure)
		{
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
		}

		static void AssertTabPageTabVisible(string message, EntryInstructionDetailsUserControl entryInstructionDetailsUserControl, string tabPageName, bool visible)
		{
			var tabControl = entryInstructionDetailsUserControl.FindSingle<ZTabControl>("TabControl");
			var tabPage = tabControl.GetTabPage(tabPageName);
			if (visible)
			{
				AssertEquals(message + "->TabControl.Visible", true, tabControl.Visible);
				AssertNotNull(message + "->" + tabPageName + " exists", tabPage);
				AssertEquals(message + "->" + tabPageName + ".TabVisible", true, tabPage.TabVisible);
			}
			else
			{
				AssertEquals(message + "->TabControl.Visible", tabControl.TabPages.Count > 0, tabControl.Visible);
				AssertNull(message + "->" + tabPageName + " doesn't exist", tabPage);
			}
		}
	}
}
