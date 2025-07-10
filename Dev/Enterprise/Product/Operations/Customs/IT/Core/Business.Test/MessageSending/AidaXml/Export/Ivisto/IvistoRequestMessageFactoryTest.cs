using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class IvistoRequestMessageFactoryTest : TestCaseWithFactory
{
	public void TestCreateIvistoRequestMessage()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		entryHeader.MovementReferenceNumberSetter("MRN12345");
		Factory.NewCusEntryNumber(entryHeader, "CLR", "XYZ123", ZDateTime.Now);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var ivistoRequestMessageFactory = new IvistoRequestMessageFactoryForTest();
			ivistoRequestMessageFactory.CreateIvistoRequestMessage(entryHeader, entryHeader.Factory);

			entryHeader.Messages.Reload(reLoadExistingRows: false);
			var ivistoMessage = entryHeader.Messages.GetLastMessageByType("IVI");
			AssertNotNull("Ivisto Message", ivistoMessage);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}
