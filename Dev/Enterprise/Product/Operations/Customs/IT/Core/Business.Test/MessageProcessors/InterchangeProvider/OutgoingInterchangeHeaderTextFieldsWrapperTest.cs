using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class OutgoingInterchangeHeaderTextFieldsWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("interchange required", () => new OutgoingInterchangeHeaderTextFieldsWrapper(interchange: null, customsInterchangeAndAccountInfo));
		AssertExceptionThrown<ArgumentNullException>("customsInterchangeAndAccountInfo required", () => new OutgoingInterchangeHeaderTextFieldsWrapper(interchange: interchange, null));
	}

	public void TestStaff()
	{
		AssertEquals("Empty Staff", ZString.Empty, fieldsProvider.Staff);

		customsInterchangeAndAccountInfo.Staff = "XXX";
		AssertEquals("Filled Staff", "XXX", fieldsProvider.Staff);
	}

	public void TestNode()
	{
		AssertEquals($"Null {nameof(customsInterchangeAndAccountInfo.Header)}", ZString.Empty, fieldsProvider.Node);

		customsInterchangeAndAccountInfo.Header = CustomsInterchangeHeader.NewForEhubSending("1234", ZString.Empty, ZString.Empty, ZString.Empty, 0, 0);
		AssertEquals("Filled Node", "1234", fieldsProvider.Node);
	}

	public void TestMessageType()
	{
		interchange.EI_InterchangeType = ZString.Empty;
		AssertEquals("Empty Staff", ZString.Empty, fieldsProvider.MessageType);

		interchange.EI_InterchangeType = SADConstants.CustomsInterchangeType.IdocR;
		AssertEquals("Filled MessageType", SADConstants.CustomsInterchangeType.IdocR, fieldsProvider.MessageType);
	}

	public void TestAccountNumber()
	{
		Factory.New<OrgHeader>().OH_Code = "AAA";
		Factory.Save();

		AssertEquals($"Null {nameof(customsInterchangeAndAccountInfo.Account)}", ZString.Empty, fieldsProvider.AccountNumber);

		customsInterchangeAndAccountInfo.Account = new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-AAA", "AAA")
			.Build()[0];

		AssertEquals("Filled AccountNumber", "11111111111-001", fieldsProvider.AccountNumber);
	}

	public void TestCustomsInterchangeHeader()
	{
		AssertEquals($"Null {nameof(customsInterchangeAndAccountInfo.Header)}", ZString.Empty, fieldsProvider.CustomsInterchangeHeader);

		customsInterchangeAndAccountInfo.Header = CustomsInterchangeHeader.NewForEhubSending("1234", ZString.Empty, ZString.Empty, ZString.Empty, 0, 0);
		AssertEquals("Filled CustomsInterchangeHeader", "1234                                                  000 00000", fieldsProvider.CustomsInterchangeHeader);
	}

	protected override void SetUp()
	{
		base.SetUp();
		interchange = Factory.NewWithValidTestData<EDIInterchange>();
		customsInterchangeAndAccountInfo = new CustomsInterchangeAndAccountInfo();
		fieldsProvider = new OutgoingInterchangeHeaderTextFieldsWrapper(interchange, customsInterchangeAndAccountInfo);
	}
	EDIInterchange interchange;
	CustomsInterchangeAndAccountInfo customsInterchangeAndAccountInfo;
	IOutgoingInterchangeHeaderTextFieldsProvider fieldsProvider;
}
