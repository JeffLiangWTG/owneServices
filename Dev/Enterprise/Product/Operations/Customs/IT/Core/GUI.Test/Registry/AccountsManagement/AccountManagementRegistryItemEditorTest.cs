using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Registry.AccountsManagement.Testing;

[TestedType(typeof(AccountManagementRegistryItemEditor))]
sealed class AccountManagementRegistryItemEditorTest : RegistryItemEditorTestCase
{
	#region Implementation

	protected override RegistryItemEditor GetEditor()
	{
		return new AccountManagementRegistryItemEditor(RegistryItem.DataType, null, null);
	}

	protected override bool GetEditorPaneEnabledState(Control editorPane)
	{
		return !((AccountManagementUserControl)editorPane).ReadOnly;
	}

	protected override Type GetExpectedEditorPaneType()
	{
		return typeof(AccountManagementUserControl);
	}

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
	{
		return new AccountCollectionRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, new AccountCollection());
	}

	protected override object[] GetValidRegistryValues()
	{
		var bizObject = new AccountCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
		return new object[] { bizObject };
	}

	public override void TestEditorPaneLayout()
	{
		var editor = GetEditor();
		using (var control = editor.NewWinFormsEditorPane())
		{
			editor.SetEditorPaneLayout(control, 666, 333);
			AssertEquals("should have the correct width", 666, control.Width);
			AssertEquals("should have the correct height", 333, control.Height);
			AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, control.Anchor);
		}
	}

	#endregion

	public void TestSystemCreateTimeShouldBeChangeAfterUpdate()
	{
		Factory.New<OrgHeader>().OH_Code = "AA";
		Factory.New<OrgHeader>().OH_Code = "ZZ";
		Factory.Save();

		var accounts = new AccountCollection();
		var accountLine1 = accounts.AddNew();
		accountLine1.AccountNumber = "01234567890-123";
		accountLine1.AccountPassword = "abc123456";
		accountLine1.AccountPasswordExpirationDate = new ZDate(2020, 01, 01);
		accountLine1.AccountCertificatePassword = "def123456";
		accountLine1.AccountCertificateExpirationDate = new ZDate(2020, 01, 01);
		var account1Detail1 = accountLine1.AccountDetails.AddNew();
		accountLine1.AccountNode = "1111";
		account1Detail1.InternalCode = "1111-AA";
		account1Detail1.DeclarantCode = "AA";
		account1Detail1.AuthorizedUser = "00000-001";
		var accountDetail2 = accountLine1.AccountDetails.AddNew();
		accountDetail2.InternalCode = "1111-ZZ";
		accountDetail2.DeclarantCode = "ZZ";
		accountDetail2.AuthorizedUser = "00000-002";

		accountLine1.EmcsNotificationEnabled = true;
		accountLine1.ExciseNumbers.AddNew().Number = "IT00PD0000000";
		accountLine1.ExciseNumbers.AddNew().Number = "IT00MI0000000";
		accountLine1.ExciseNumbers.AddNew().Number = "IT00VE0000000";

		var regItem = new RegistryItemTagForTest(ITAccountsManagementRegistry.Instance.AccountsManagement)
		{
			CompanyPKForTest = GlbCompany.CurrentCompany.PK.ToGuid(),
			BranchPKForTest = Guid.Empty,
			DepartmentPKForTest = Guid.Empty,
			NewValue = accounts,
			IsChanged = true,
			HasValue = true
		};

		regItem.SaveAllValues();

		var zQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
		zQuery.OrderBy = EDIInterchangeSchema.EI_InterchangeNum.Name + " DESC";
		zQuery.AddToFilter(EDIInterchangeSchema.EI_To, "eHub");
		var interchange = Factory.LoadTop1<EDIInterchange>(zQuery);

		var systemCreateTimeUtc = interchange.EI_SystemCreateTimeUtc;
		interchange.Delete();

		accountLine1.AccountNode = "2222";

		regItem.NewValue = accounts;
		regItem.IsChanged = true;
		regItem.HasValue = true;

		regItem.SaveAllValues();

		interchange = Factory.LoadTop1<EDIInterchange>(zQuery);
		AssertNotEquals("EI_SystemCreateTimeUtc", systemCreateTimeUtc, interchange.EI_SystemCreateTimeUtc);
	}
}
