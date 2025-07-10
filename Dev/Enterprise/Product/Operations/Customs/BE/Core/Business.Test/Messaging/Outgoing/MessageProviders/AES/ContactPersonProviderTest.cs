namespace Enterprise.Customs.BE.Business.Testing;

sealed class ContactPersonProviderTest : Customs.Business.Testing.DataProviderTestCase<ContactPersonProvider>
{
	public void TestName()
	{
		AssertEquals("Wise Tech", provider.Name);
	}

	public void TestPhoneNumber()
	{
		AssertEquals("1234567890", provider.PhoneNumber);
	}

	public void TestEMailAddress()
	{
		AssertEquals("be@Wisetechglobal.com", provider.EMailAddress);
	}

	public void TestInvalidContact()
	{
		var invalidContact = ContactPersonProvider.NewOrNull("", "", "be@Wisetechglobal.com");
		AssertNull(invalidContact);
	}

	protected override ContactPersonProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		provider = ContactPersonProvider.NewOrNull("Wise Tech", "1234567890", "be@Wisetechglobal.com");
	}

	ContactPersonProvider provider;
}
