using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Registry.AccountsManagement.Testing;

[TestedType(typeof(AccountManagementUserControl))]
sealed class AccountManagementUserControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity()
	{
		return new AccountCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
	}

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
	{
		return (((AccountManagementUserControl)control).ReadOnly);
	}

	public void TestAccountGridColumns()
	{
		using (var accountManagementUserControl = new AccountManagementUserControl())
		{
			accountManagementUserControl.Show();

			var accountGrid = (ZGrid)accountManagementUserControl.Controls.Find("AccountsGrid", false)[0];
			CombineAssertions("Accounts Grid should contains these columns", () =>
			{
				AssertNotNull("Account Number", accountGrid.GetColumnStyle("AccountNumber"));
				AssertNotNull("Account Password", accountGrid.GetColumnStyle("AccountPassword"));
				AssertNotNull("Account Password Expiration Date", accountGrid.GetColumnStyle("AccountPasswordExpirationDate"));
				AssertNotNull("Account Status", accountGrid.GetColumnStyle("AccountStatus"));
				AssertNotNull("Account Status Display", accountGrid.GetColumnStyle("AccountStatusDisplay"));
				AssertNotNull("Account Certificate State", accountGrid.GetColumnStyle("AccountCertificateState"));
				AssertNotNull("Account Certificate Password", accountGrid.GetColumnStyle("AccountCertificatePassword"));
				AssertNotNull("Account Certificate Password ExpirationDate", accountGrid.GetColumnStyle("AccountCertificateExpirationDate"));
				AssertNotNull("Account Certificate Status Display", accountGrid.GetColumnStyle("AccountCertificateStatusDisplay"));
				AssertNotNull("Account Node", accountGrid.GetColumnStyle("AccountNode"));
				AssertNotNull("Account Range Start", accountGrid.GetColumnStyle("AccountRangeStart"));
				AssertNotNull("Account Range End", accountGrid.GetColumnStyle("AccountRangeEnd"));
			});
		}
	}

	public void TestExciseNumbersGridColumns()
	{
		using (var accountManagementUserControl = new AccountManagementUserControl())
		{
			accountManagementUserControl.Show();

			var exciseNumbersGrid = (ZGrid)accountManagementUserControl.Controls.Find("ExciseNumbersGrid", true)[0];
			CombineAssertions("ExciseNumbersGrid should contains these columns", () =>
			{
				AssertNotNull("Excise Number", exciseNumbersGrid.GetColumnStyle("Number"));
			});
		}
	}

	public void TestAccountDetailsGridColumns()
	{
		using (var accountManagementUserControl = new AccountManagementUserControl())
		{
			accountManagementUserControl.Show();

			var accountDetailsGrid = (ZGrid)accountManagementUserControl.Controls.Find("AccountDetailsGrid", true)[0];
			CombineAssertions("AccountDetailsGrid should contains these columns", () =>
			{
				AssertNotNull("Internal Code", accountDetailsGrid.GetColumnStyle("InternalCode"));
				AssertNotNull("DeclarantCode", accountDetailsGrid.GetColumnStyle("DeclarantCode"));
				AssertNotNull("Authorized User", accountDetailsGrid.GetColumnStyle("AuthorizedUser"));
			});
		}
	}
}
