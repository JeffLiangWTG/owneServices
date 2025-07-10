using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Module.Testing
{
	public class APEnquiryFilterControlTest : EnquiryFilterControlTest
	{
		protected override EnquiryFilterControl GetTestFilterControl()
		{
			return new APEnquiryFilterControl(new APTransactionHeaderCollection(Factory), new APEnquiryFilterBusinessObject());
		}

		public void TestAvailabilityOfWHTColumns()
		{
			var coll = new APTransactionHeaderCollection(Factory);
			var filterBO = new APEnquiryFilterBusinessObject();

			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());

			using (var control = new APEnquiryFilterControl(coll, filterBO))
			{
				AssertEquals(false, ColumnExistsInTheGrid(control.FilteredGrid, nameof(TransactionHeader.AH_NotionalWHTTax), ResourceStringData.Empty));
				AssertEquals(false, ColumnExistsInTheGrid(control.FilteredGrid, nameof(TransactionHeader.AH_RealizedWHTTax), ResourceStringData.Empty));
			}

			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypes.AccountsPayable, TaxSuperTypeList.StandardPaymentRetention.Code);

			using (var control = new APEnquiryFilterControl(coll, filterBO))
			{
				AssertEquals(true, ColumnExistsInTheGrid(control.FilteredGrid, nameof(TransactionHeader.AH_NotionalWHTTax), ResourceStringData.Empty));
				AssertEquals(true, ColumnExistsInTheGrid(control.FilteredGrid, nameof(TransactionHeader.AH_RealizedWHTTax), ResourceStringData.Empty));
			}
		}

		ZBool ColumnExistsInTheGrid(ZDisplayGrid grid, ZString columnName, ResourceStringData groupName)
		{
			foreach (ZGridColumnInfo columnStyle in grid.ColumnStyles)
			{
				if (columnStyle.ColumnName == columnName && (groupName.IsEmpty() || columnStyle.GroupName.Caption == groupName.Caption))
				{
					return ZBool.True;
				}
			}
			return ZBool.False;
		}
	}
}
