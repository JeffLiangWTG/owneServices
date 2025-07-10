using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class PartyWrapperTest : DataProviderTestCase<PartyWrapper>
{
	public void TestConstructor() 
	{
		AssertNull(PartyWrapper.New(null));
	}

	public void TestId()
	{
		CombineAssertions(() =>
		{
			var orgHeaderWithEORNumber = GetOrgHeaderWithEORNumber();
			var wrapper = PartyWrapper.New(orgHeaderWithEORNumber);
			AssertEquals("Has EOR number", "NL654321", wrapper.Id);

			var orgHeader = Factory.New<OrgHeader>();
			wrapper = PartyWrapper.New(orgHeader);
			AssertNull("No EOR number", wrapper.Id);
		});
	}

	public void TestName()
	{
		CombineAssertions(() =>
		{
			var orgHeaderWithEORNumber = GetOrgHeaderWithEORNumber();
			var wrapper = PartyWrapper.New(orgHeaderWithEORNumber);
			AssertNull("ID isn't empty", wrapper.Name);

			var orgHeader = WrapperTestHelper.CreateOrgHeader(Factory, "Exporter Full Name", "Exporter", ZString.Empty);
			wrapper = PartyWrapper.New(orgHeader);
			AssertNull("ID is empty", wrapper.Id);
			AssertEquals("Name mapped as ID is empty", "Exporter Full Name", wrapper.Name);
		});
	}

	public void TestAddress()
	{
		CombineAssertions(() =>
		{
			var orgHeaderWithEORNumber = GetOrgHeaderWithEORNumber();
			WrapperTestHelper.CreateAddress(orgHeaderWithEORNumber, "EXP", "Havenweg 3", "Rotterdam", "1079CK");
			var wrapper = PartyWrapper.New(orgHeaderWithEORNumber);
			AssertNull("ID isn't empty", wrapper.Address);

			var orgHeader = WrapperTestHelper.CreateOrgHeader(Factory, ZString.Empty, "Exporter", ZString.Empty);
			WrapperTestHelper.CreateAddress(orgHeader, "EXP", "Havenweg 3", "Rotterdam", "1079CK");
			wrapper = PartyWrapper.New(orgHeader);
			AssertNull("ID is empty and Name is empty", wrapper.Address);

			orgHeader.OH_FullName = "Exporter Full Name";
			wrapper = PartyWrapper.New(orgHeader);
			AssertType<AddressWrapper>("Type", wrapper.Address);
			AssertEquals("Line", "Havenweg 3", wrapper.Address.Line);
		});
	}

	public void TestFunctionCode()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var wrapper = PartyWrapper.New(orgHeader);
		AssertEquals(string.Empty, wrapper.FunctionCode);
	}

	public void TestContact()
	{
		CombineAssertions(() =>
		{
			var orgHeader = Factory.New<OrgHeader>();
			var wrapper = PartyWrapper.New(orgHeader);
			AssertNull("No contact", wrapper.Contact);

			WrapperTestHelper.CreateContact(orgHeader, "Exporter Contact Name", "+3164259513", "exporter@mail.nl");
			wrapper = PartyWrapper.New(orgHeader);
			AssertType<ContactWrapper>("Type", wrapper.Contact);
			AssertEquals("Name", "Exporter Contact Name", wrapper.Contact.Name);
		});
	}

	OrgHeader GetOrgHeaderWithEORNumber() => WrapperTestHelper.CreateOrgHeader(Factory, "Exporter Full Name", "EXPORTER", "654321");

	protected override PartyWrapper GetProvider() => PartyWrapper.New(Factory.New<OrgHeader>());
}
