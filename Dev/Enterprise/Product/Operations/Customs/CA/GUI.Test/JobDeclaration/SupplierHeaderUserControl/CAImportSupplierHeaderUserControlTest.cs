using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CAImportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceTabControlMinimumSize()
		{
			using (var control = new CAImportSupplierHeaderUserControl())
			{
				AssertEquals(994, control.InvoiceTabControl.MinimumSize.Width);
				AssertEquals(330, control.InvoiceTabControl.MinimumSize.Height);
			}
		}

		public void TestGridId()
		{
			using (var control = new CAImportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutTr/Cac4tViR0a1W3XKS7zQ==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestDefaultLayout()
		{
			#region Expected Column Sequence

			var expectedColumnList = new List<string>
																 {
																	 AutoJobComInvoiceHeader.Schema.JZ_InvoiceNumber,
																	"SupplierDocumentaryAddress+OrganisationPK",
																	"SupplierDocumentaryAddress+E2_OA_Address",
																	AutoJobComInvoiceHeader.Schema.JZ_IncoTerm,
																	AutoJobComInvoiceHeader.Schema.JZ_IncoTermPlace,
																	AutoJobComInvoiceHeader.Schema.JZ_InvoiceAmount,
																	AutoJobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency,
																	AutoJobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate,
																	AutoJobComInvoiceHeader.Schema.InvoiceLineTotal,
																	JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
																	AutoJobComInvoiceHeader.Schema.CA_RL_NKLastPort,
																	AutoJobComInvoiceHeader.Schema.JZ_ValuationDateOverride,
																	AutoJobComInvoiceHeader.Schema.CA_TreatmentCode,
																	AutoJobComInvoiceHeader.Schema.CA_ValueForDutyCode,
																	AutoJobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin,
																	AutoJobComInvoiceHeader.Schema.JZ_RW_NKOriginState,
																	AutoJobComInvoiceHeader.Schema.CA_RN_NKExport,
																	AutoJobComInvoiceHeader.Schema.CA_USStateOfExport,
																	AutoJobComInvoiceHeader.Schema.CA_TradeZone,
																	AutoJobComInvoiceHeader.Schema.CA_USPortOfExit,
																	AutoJobComInvoiceHeader.Schema.JZ_PaymentDate,
																	AutoJobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill,
																	AutoJobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice
																 };

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				var control = (CAImportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl;
				AssertEquals("Context is set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);

				ZGrid grid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				grid.ResetColumns();
				Assert("ColumnList count", expectedColumnList.Count <= control.InvoiceHeadersBoundGrid.ColumnStyles.Count);

				for (int i = 0; i < expectedColumnList.Count; i++)
				{
					AssertEquals("ColumnName", expectedColumnList[i], ((ZGridColumnInfo)control.InvoiceHeadersBoundGrid.ColumnStyles[i]).ColumnName);
					AssertEquals("IsVisible", true, ((ZGridColumnInfo)control.InvoiceHeadersBoundGrid.ColumnStyles[i]).IsVisible);
				}
			}
		}

		public void TestCA_DDPDeductDutyOnlyVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				var control = (CAImportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl;
				var dDPDeductDutyCheckBox = form.Controls.Find("DDPDeductDutyOnlyCheckBox", true)[0] as ZCheckBox;
				AssertEquals("DDPDeductDutyOnly CheckBox should not be shown", false, dDPDeductDutyCheckBox.Visible);

				declaration.Invoices[0].JZ_IncoTerm = "DDP";
				AssertEquals("DDPDeductDutyOnly CheckBox should  be shown", true, dDPDeductDutyCheckBox.Visible);
			}
		}

		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var control = new CAImportSupplierHeaderUserControl())
			{
				form.Controls.Add(control);
				form.Show();
			}

			var propertyInfoStorageField = declaration.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
			var propertyInfoStorage = propertyInfoStorageField.GetValue(declaration.Factory);

			var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);

			var dictionaryField = valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var dictionary = (Dictionary<string, Dictionary<BusinessObject, EventHandler>>)dictionaryField.GetValue(valueChangedDictionary);

			foreach (var dictionaryOfSubcriber in dictionary)
			{
				foreach (var subcriber in dictionaryOfSubcriber.Value)
				{
					var subcriberTarget = subcriber.Value.Target;
					if (subcriberTarget is DeclarationValueChangedAnnouncer)
					{
						var onValueChangedField = subcriberTarget.GetType().BaseType.GetField("OnValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
						var onValueChanged = onValueChangedField.GetValue(subcriberTarget);
						var onValueChangedTarget = ((EventHandler)onValueChanged).Target;
						Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is CAImportSupplierHeaderUserControl));
					}
					else
					{
						Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is CAImportSupplierHeaderUserControl));
					}
				}
			}
		}

		public void TestAvailableColumnsContainsPackageLines()
		{
			var newColumnList = new List<string>
			{
				JobComInvoiceHeader.Schema.FirstPackageIsLinked,
				JobComInvoiceHeader.Schema.FirstPackageNumber,
				JobComInvoiceHeader.Schema.FirstPackageQty
			};
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				var control = (CAImportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl;

				ZGrid grid = control.JobComInvoiceHeadersBoundGrid.InnerGrid;
				grid.ResetColumns();
				var availableColumnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
				foreach (var cloumn in newColumnList)
				{
					Assert(availableColumnNames.Contains(cloumn));
				}
				for (int i = 0; i < availableColumnNames.Count(); i++)
				{
					if (newColumnList.Contains(((ZGridColumnInfo)control.InvoiceHeadersBoundGrid.ColumnStyles[i]).ColumnName))
					{
						AssertEquals("IsVisible", false, ((ZGridColumnInfo)control.InvoiceHeadersBoundGrid.ColumnStyles[i]).IsVisible);
					}
				}
			}
		}
	}
}
