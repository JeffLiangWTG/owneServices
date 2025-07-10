using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.GUI.CashBook;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing.CashBook
{
	public class CashBookTransactionGUIHelperTest : TestCaseWithFactory
	{
		public void TestConfigureColumns_RemoveCommonColumns_ForGovernmentChargeCode()
		{
			var clolumns = new List<string>	{ DirectTransactionLineBase.Schema.AL_GovtChargeCode };
			var directReceipt = TestObjectCreator.CreateCashBookTransaction<DirectReceipt>();

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertGridColumns(clolumns, new List<string>(), directReceipt);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertGridColumns(clolumns, clolumns, directReceipt);
			}
		}

		public void TestConfigureColumns_RemoveCommonColumns_ForPlaceOfSupply()
		{
			var clolumns = new List<string>	{ DirectTransactionLineBase.Schema.AL_PlaceOfSupply };
			var directReceipt = TestObjectCreator.CreateCashBookTransaction<DirectReceipt>();

			AssertEquals(false, PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));
			AssertGridColumns(clolumns, clolumns, directReceipt);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				AssertEquals(true, PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));
				AssertGridColumns(clolumns, new List<string>(), directReceipt);
			}
		}

		public void TestConfigureColumns_RemoveTaxColumns_CommonTaxClolumns()
		{
			var commonTaxClolumns = new List<string>
			{
				DirectTransactionLineBase.Schema.AL_AT,
				DirectTransactionLineBase.Schema.AL_TaxDate,
				DirectTransactionLineBase.Schema.AL_A9_VATClass,
				DirectTransactionLineBase.Schema.AL_OSTaxAmount,
				DirectTransactionLineBase.Schema.AL_LocalTaxAmount
			};

			var directReceipt = TestObjectCreator.CreateCashBookTransaction<DirectReceipt>();
			var directPayment = TestObjectCreator.CreateCashBookTransaction<DirectPayment>();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			directReceipt.Lines[0].AL_AT = TestObjectCreator.FREEVAT.PK;
			directPayment.Lines[0].AL_AT = TestObjectCreator.FREEVAT.PK;
			AssertEquals("Percondition", true, directReceipt.IsTaxed);
			AssertEquals("Percondition", true, directPayment.IsTaxed);
			AssertGridColumns(commonTaxClolumns, new List<string>(), directReceipt);
			AssertGridColumns(commonTaxClolumns, new List<string>(), directPayment);

			directReceipt.Lines[0].AL_AT = ZGuid.Empty;
			directPayment.Lines[0].AL_AT = ZGuid.Empty;
			Factory.ClearCachedValue<ZBool>("IsTaxed:" + directReceipt.PK.ToStringKey());
			Factory.ClearCachedValue<ZBool>("IsTaxed:" + directPayment.PK.ToStringKey());
			AssertEquals("Percondition", false, directReceipt.IsTaxed);
			AssertEquals("Percondition", false, directPayment.IsTaxed);
			AssertGridColumns(commonTaxClolumns, commonTaxClolumns, directReceipt);
			AssertGridColumns(commonTaxClolumns, commonTaxClolumns, directPayment);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AssertGridColumns(commonTaxClolumns, new List<string>(), directReceipt);
			AssertGridColumns(commonTaxClolumns, new List<string>(), directPayment);
		}

		public void TestConfigureColumns_RemoveTaxColumns_ExtraTaxClolumns()
		{
			var extraTaxClolumns = new List<string>
			{
				DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount,
				DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount,
				DirectTransactionLineBase.Schema.AL_OSGSTAmount,
				DirectTransactionLineBase.Schema.AL_LocalGSTAmount
			};

			var directReceipt = TestObjectCreator.CreateCashBookTransaction<DirectReceipt>();
			var directPayment = TestObjectCreator.CreateCashBookTransaction<DirectPayment>();

			AssertEquals("Percondition", false, GlbCompany.CurrentCompany.IsExtraTaxApplicable());
			AssertGridColumns(extraTaxClolumns, extraTaxClolumns, directReceipt);
			AssertGridColumns(extraTaxClolumns, extraTaxClolumns, directPayment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				AssertEquals("Percondition", true, GlbCompany.CurrentCompany.IsExtraTaxApplicable());
				AssertGridColumns(extraTaxClolumns, new List<string>(), directReceipt);
				AssertGridColumns(extraTaxClolumns, new List<string>(), directPayment);
			}
		}

		public void TestConfigureColumns_RemoveTaxColumns_TaxRecoverableColumns()
		{
			var taxRecoverableClolumns = new List<string>
			{
				DirectTransactionLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage,
				DirectTransactionLineBase.Schema.AL_OSTaxAmount_Recoverable,
				DirectTransactionLineBase.Schema.AL_OSTaxAmount_NotRecoverable,
				DirectTransactionLineBase.Schema.AL_LocalTaxAmount_Recoverable,
				DirectTransactionLineBase.Schema.AL_LocalTaxAmount_NotRecoverable
			};

			var directReceipt = TestObjectCreator.CreateCashBookTransaction<DirectReceipt>();
			var directPayment = TestObjectCreator.CreateCashBookTransaction<DirectPayment>();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			directReceipt.Lines[0].AL_AT = TestObjectCreator.FREEVAT.PK;
			directPayment.Lines[0].AL_AT = TestObjectCreator.FREEVAT.PK;
			AssertEquals("Percondition", true, directReceipt.IsTaxed);
			AssertEquals("Percondition", true, directPayment.IsTaxed);
			AssertGridColumns(taxRecoverableClolumns, taxRecoverableClolumns, directReceipt);
			AssertGridColumns(taxRecoverableClolumns, new List<string>(), directPayment);

			directReceipt.Lines[0].AL_AT = ZGuid.Empty;
			directPayment.Lines[0].AL_AT = ZGuid.Empty;
			Factory.ClearCachedValue<ZBool>("IsTaxed:" + directReceipt.PK.ToStringKey());
			Factory.ClearCachedValue<ZBool>("IsTaxed:" + directPayment.PK.ToStringKey());
			AssertEquals("Percondition", false, directReceipt.IsTaxed);
			AssertEquals("Percondition", false, directPayment.IsTaxed);
			AssertGridColumns(taxRecoverableClolumns, taxRecoverableClolumns, directReceipt);
			AssertGridColumns(taxRecoverableClolumns, taxRecoverableClolumns, directPayment);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AssertGridColumns(taxRecoverableClolumns, taxRecoverableClolumns, directReceipt);
			AssertGridColumns(taxRecoverableClolumns, new List<string>(), directPayment);
		}

		public void TestConfigureColumns_ConfigureExtraTaxColumnsCaption()
		{
			var directReceipt = TestObjectCreator.CreateCashBookTransaction<DirectReceipt>();
			var clolumns = new List<string>
			{
				DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount,
				DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount,
				DirectTransactionLineBase.Schema.AL_LocalGSTAmount,
				DirectTransactionLineBase.Schema.AL_OSGSTAmount
			};

			string exTaxCaption = null;
			string gSTCaption = null;
			var additionalAction = new Action<ZGrid>((x) => {
				AssertContains(exTaxCaption + " Amount", x.GetColumnStyle(DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount).CaptionResourceString.ToString());
				AssertContains(exTaxCaption + " Local", x.GetColumnStyle(DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount).CaptionResourceString.ToString());
				AssertContains(gSTCaption + " Local", x.GetColumnStyle(DirectTransactionLineBase.Schema.AL_LocalGSTAmount).CaptionResourceString.ToString());
				AssertContains(gSTCaption + " Amount", x.GetColumnStyle(DirectTransactionLineBase.Schema.AL_OSGSTAmount).CaptionResourceString.ToString());
			});

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				exTaxCaption = "QST";
				gSTCaption = "GST";
				AssertEquals(true, GlbCompany.CurrentCompany.IsExtraTaxApplicable());
				AssertGridColumns(clolumns, new List<string>(), directReceipt, additionalAction);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				exTaxCaption = "SGST";
				gSTCaption = "CGST/IGST";
				AssertEquals(true, GlbCompany.CurrentCompany.IsExtraTaxApplicable());
				AssertGridColumns(clolumns, new List<string>(), directReceipt, additionalAction);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				exTaxCaption = "RET";
				gSTCaption = "IVA";
				AssertEquals(true, GlbCompany.CurrentCompany.IsExtraTaxApplicable());
				AssertGridColumns(clolumns, new List<string>(), directReceipt, additionalAction);
			}
		}

		void AssertGridColumns(IEnumerable<string> testColumnNames, IEnumerable<string> notVisibleColumnNames, DirectTransactionHeaderBase bizO, Action<ZGrid> action = null)
		{
			using (var form = new ZForm(bizO))
			using (var grid = new ZGrid())
			{
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);

				foreach (var testColumnName in testColumnNames)
				{
					switch (testColumnName)
					{
						case DirectTransactionLineBase.Schema.AL_GovtChargeCode:
							grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = testColumnName });
							break;
						case DirectTransactionLineBase.Schema.AL_PlaceOfSupply:
							grid.ColumnStyles.Add(new ZDropEditColumnStyleInfo { ColumnName = testColumnName });
							break;
						case DirectTransactionLineBase.Schema.AL_TaxDate:
							grid.ColumnStyles.Add(new ZDateEditColumnStyleInfo { ColumnName = testColumnName });
							break;
						case DirectTransactionLineBase.Schema.AL_AT:
						case DirectTransactionLineBase.Schema.AL_A9_VATClass:
							grid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo { ColumnName = testColumnName });
							break;
						default:
							grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo { ColumnName = testColumnName });
							break;
					}

					grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo { ColumnName = DirectTransactionLineBase.Schema.AL_LocalExTaxAmount });
				}

				grid.SetDataBinding(bizO, "Lines");
				form.Load += (sender, e) =>
				{
					CashBookTransactionGUIHelper.ConfigureColumns(grid, bizO.AH_TransactionType, bizO);
				};

				form.Show();

				foreach (ZGridColumnInfo columnStyle in grid.ColumnStyles)
				{
					AssertEquals(columnStyle.ColumnName + (notVisibleColumnNames.Contains(columnStyle.ColumnName) ? " Should not be Visible " : " Should be Visible"),
						!notVisibleColumnNames.Contains(columnStyle.ColumnName), grid.Columns.Contains(columnStyle.ColumnName));
				}

				action?.Invoke(grid);
			}
		}

		TestObjectCreator testObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
	}
}
