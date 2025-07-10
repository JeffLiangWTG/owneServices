using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	[TestedType(typeof(EdiAccountVerificationWarningTestForm))]
	public class EdiAccountVerificationWarningTest : ZFormBasherTest
	{
		public class EdiAccountVerificationWarningTestForm : EdiAccountVerificationWarning
		{
			public EdiAccountVerificationWarningTestForm(EDIOrgContact ediOrgContact)
				: base(ediOrgContact)
			{ }

			public ZDisplayGrid GetGrid() => ContactGrid;
		}

		EDIOrgContact GetEdiOrgContact()
		{
			var orgHeader = Factory.New<EDIOrgHeader>();
			orgHeader.OH_FullName = "Test organization INC";
			orgHeader.OH_Code = "TOINC";

			var contact = Factory.New<EDIOrgContact>();
			contact.OC_OH = orgHeader.PK;

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var ediCustomerUserAccount = Factory.New<EdiCustomerUserAccount>();

			ediCustomerUserAccount.EUA_LD = licence.Database.PK;
			ediCustomerUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
			ediCustomerUserAccount.EUA_OC_WebAccessContact = contact.PK;
			ediCustomerUserAccount.EUA_UserID = "150";

			Factory.Save();

			return contact;
		}

		protected override Form GetFormToBashCore()
		{
			var contact = GetEdiOrgContact();
			return new EdiAccountVerificationWarningTestForm(contact);
		}

		public void TestGridHasProperColumns()
		{
			var contact = GetEdiOrgContact();
			using (var form = new EdiAccountVerificationWarningTestForm(contact))
			{
				form.Show();

				var grid = form.GetGrid();
				var columnStyles = grid.ColumnStyles;

				AssertEquals(columnStyles.Count, 8);
				AssertEquals("ContactGrid.ColumnStyles[0]", ((ZGridColumnInfo)columnStyles[0]).ColumnName, "ContactRelationshipStatus");
				AssertEquals("ContactGrid.ColumnStyles[1]", ((ZGridColumnInfo)columnStyles[1]).ColumnName, "Product");
				AssertEquals("ContactGrid.ColumnStyles[2]", ((ZGridColumnInfo)columnStyles[2]).ColumnName, "LicenceType");
				AssertEquals("ContactGrid.ColumnStyles[3]", ((ZGridColumnInfo)columnStyles[3]).ColumnName, "ServerCode");
				AssertEquals("ContactGrid.ColumnStyles[4]", ((ZGridColumnInfo)columnStyles[4]).ColumnName, "IsActive");
				AssertEquals("ContactGrid.ColumnStyles[5]", ((ZGridColumnInfo)columnStyles[5]).ColumnName, "UserID");
				AssertEquals("ContactGrid.ColumnStyles[6]", ((ZGridColumnInfo)columnStyles[6]).ColumnName, "FullName");
				AssertEquals("ContactGrid.ColumnStyles[7]", ((ZGridColumnInfo)columnStyles[7]).ColumnName, "Email");
			}
		}
	}
}
