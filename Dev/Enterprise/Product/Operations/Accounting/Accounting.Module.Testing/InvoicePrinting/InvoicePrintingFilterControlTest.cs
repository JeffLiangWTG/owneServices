using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class InvoicePrintingFilterControlTest : TestCase
	{
		ZBool ColumnExistsInTheGrid(ZDisplayGrid grid, ZString columnName, ResourceStringData groupName)
		{
			IEnumerator columnEnum = grid.ColumnStyles.GetEnumerator();
			while (columnEnum.MoveNext())
			{
				ZGridColumnInfo column = (ZGridColumnInfo)columnEnum.Current;
				if (column.ColumnName == columnName && (groupName.IsEmpty() || column.GroupName.Equals(groupName)))
				{
					return ZBool.True;
				}
			}
			return ZBool.False;
		}

		[UseSnapshotProtection]
		public void TestManageColumnOrdering()
		{
			TransactionHeaderCollection gridCollection = new TransactionHeaderCollection(Factory);
			APTransactionFilterStripBusinessObject aPFilterBusinessObject = new APTransactionFilterStripBusinessObject();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should exists for Peru company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should exists for VietNam company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should exists for China company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia))
			using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should exists for Indonesia company", ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (TransactionFilterStripControl filterControl = new TransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				Assert("The Compliance SubType column should be deleted for non-Peru company", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}
		}

		[UseSnapshotProtection]
		public void TestRemoveTaxInvoiceColumn()
		{
			AssertVisibleForTaxInvoiceNumColumn("TW", true);
			AssertVisibleForTaxInvoiceNumColumn("CN", true);
			AssertVisibleForTaxInvoiceNumColumn("AU", false);
		}

		void AssertVisibleForTaxInvoiceNumColumn(string country, bool visible)
		{
			AccTransactionHeaderCollection accTH = new AccTransactionHeaderCollection(Factory);
			InvoicePrintingFilterBusinessObject filterBizO = new InvoicePrintingFilterBusinessObject();
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(country);
				GlbCompany.CurrentCompany.Factory.Save();
				using (ZForm form = new ZForm())
				{
					using (InvoicePrintingFilterControl filterIP = new InvoicePrintingFilterControl(accTH, filterBizO))
					{
						form.Controls.Add(filterIP);
						form.Show();
						Application.DoEvents();
						bool isVisible = false;

						foreach (ZGridColumnInfo column in filterIP.FilteredGrid.ColumnStyles)
						{
							if (column.ColumnName == AccTransactionHeaderSchema.AH_TransactionReference.Name)
							{
								isVisible = true;
								break;
							}
						}
						AssertEquals(visible, isVisible);
					}
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestAssertTaxColumnsAreVisible()
		{
			AssertTaxColumnsAreVisible(true);
		}

		public void TestAssertTaxColumnsAreNotVisible()
		{
			AssertTaxColumnsAreVisible(false);
		}

		void AssertTaxColumnsAreVisible(bool isGSTRegistered)
		{
			AccTransactionHeaderCollection transaction = new AccTransactionHeaderCollection(Factory);
			InvoicePrintingFilterBusinessObject filterBO = new InvoicePrintingFilterBusinessObject();
			bool oldGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (ZForm form = new ZForm())
				{
					using (InvoicePrintingFilterControl control = new InvoicePrintingFilterControl(transaction, filterBO))
					{
						form.Controls.Add(control);
						form.Show();
						Application.DoEvents();
						bool isVisibleAH_OSTaxAmount = false;
						bool isVisibleAH_LocalTaxAmount = false;

						foreach (ZGridColumnInfo column in control.FilteredGrid.ColumnStyles)
						{
							if (column.ColumnName == TransactionHeader.Schema.AH_OSTaxAmount && !column.IsUnavailable)
							{
								isVisibleAH_OSTaxAmount = true;
							}
							else if (column.ColumnName == TransactionHeader.Schema.AH_LocalTaxAmount && !column.IsUnavailable)
							{
								isVisibleAH_LocalTaxAmount = true;
							}
						}

						AssertEquals("AH_OSTaxAmount Visible", isGSTRegistered, isVisibleAH_OSTaxAmount);
						AssertEquals("AH_LocalTaxAmount", isGSTRegistered, isVisibleAH_LocalTaxAmount);
					}
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = oldGSTRegistered;
			}
		}

		public void TestOutstandingAmountCaption()
		{
			var transaction = new AccTransactionHeaderCollection(Factory);
			var filterBO = new InvoicePrintingFilterBusinessObject();

			using (var form = new ZForm())
			using (var control = new InvoicePrintingFilterControl(transaction, filterBO))
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var grid = control.FilteredGrid;
				var columnName = "AH_OutstandingAmount";
				var columnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
				AssertNotNull(columnInfo);
				AssertNull(columnInfo.CaptionResourceString.Caption);
				AssertEquals("Outstanding Amount", grid.Columns[columnName].ColumnStyle.HeaderText);
			}
		}

		public void TestTaxBranchColumnVisibilityDependsOnPresentationProvider()
		{
			var transaction = new AccTransactionHeaderCollection(Factory);
			var filterBO = new InvoicePrintingFilterBusinessObject();
			var taxBranchColumn = TransactionHeader.Schema.AH_GB_TaxBranch;
			var presentationProviderMock = new Mock<IInvoicePrintingControlPresentationProvider>();
			var accountingPresentationProviderFactoryMock = new Mock<IAccountingPresentationProviderFactory>();
			accountingPresentationProviderFactoryMock.Setup(x => x.GetInvoicePrintingControlPresentationProvider()).Returns(presentationProviderMock.Object);
			ObjectFactory.Substitute(accountingPresentationProviderFactoryMock.Object);

			AssertTaxBranchColumnVisibility(true);
			AssertTaxBranchColumnVisibility(false);

			void AssertTaxBranchColumnVisibility(bool isTaxBranchColumnVisible)
			{
				presentationProviderMock.Setup(x => x.IsTaxBranchColumnAvailable()).Returns(isTaxBranchColumnVisible);
				using (ZForm form = new ZForm())
				{
					using (var control = new InvoicePrintingFilterControl(transaction, filterBO))
					{
						{
							form.Controls.Add(control);
							form.Show();
							Application.DoEvents();
							AssertEquals($"{taxBranchColumn} column should be added to grid", isTaxBranchColumnVisible, !control.FilteredGrid.GetColumnStyle(TransactionHeader.Schema.AH_GB_TaxBranch).IsUnavailable);
						}
					}
				}
			}
		}

		public void TestTaxBranchColumnIsVisibilityDependsTaxBranchColumnAvailability()
		{
			var transaction = new AccTransactionHeaderCollection(Factory);
			var filterBO = new InvoicePrintingFilterBusinessObject();
			var taxBranchColumn = TransactionHeader.Schema.AH_GB_TaxBranch;
			GetMockInvoicePrintingControlPresentationProvider().Setup(x => x.IsTaxBranchColumnAvailable()).Returns(true);
			using (ZForm form = new ZForm())
			{
				using (var control = new InvoicePrintingFilterControl(transaction, filterBO))
				{
					{
						form.Controls.Add(control);
						form.Show();
						Application.DoEvents();
						Assert($"{taxBranchColumn} column should be visible as default", control.Grid.GetColumnStyle(taxBranchColumn).IsVisible);
					}
				}
			}
		}
		Mock<IInvoicePrintingControlPresentationProvider> GetMockInvoicePrintingControlPresentationProvider()
		{
			var presentationProviderMock = new Mock<IInvoicePrintingControlPresentationProvider>();
			var accountingPresentationProviderFactoryMock = new Mock<IAccountingPresentationProviderFactory>();
			accountingPresentationProviderFactoryMock.Setup(x => x.GetInvoicePrintingControlPresentationProvider()).Returns(presentationProviderMock.Object);
			ObjectFactory.Substitute(accountingPresentationProviderFactoryMock.Object);
			return presentationProviderMock;
		}

		#region Factory

		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}
		BusinessObjectFactory factory;

		#endregion
	}
}
