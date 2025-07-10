using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD.Testing
{
	class CODMessageEnvelopeWrapperTest : TestCaseWithFactory
	{
		public void TestSchemaID()
		{
			AssertEquals(MessageEnvelopeWrapper.codSchema, wrapper.SchemaID);
		}

		public void TestSchemaVersion()
		{
			AssertEquals(MessageEnvelopeWrapper.deltaDSchemaVersion, wrapper.SchemaVersion);
		}

		public void TestPartnerId()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RepresentativeID = "TESTREPID";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = importer.PK;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsProfile = ZString.Empty;
			declaration.JE_CustomsProfile = "TESTACC";
			AssertEquals("TESTREPID", declaration.CustomsProfileRelatedAccountRepresentativeID);
			AssertEquals("TESTREPID", wrapper.PartnerId);
		}

		public void TestTransactionId()
		{
			entry.CorrelationID = "TESTCORID";
			AssertEquals("TESTCORID", wrapper.TransactionId);
		}

		public void TestNumSeq()
		{
			var message = entry.Messages.AddNew();
			message.IsTransmitMessage = true;
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			var message1 = entry.Messages.AddNew();
			message1.IsTransmitMessage = true;
			message1.EM_MessageType = MessageTypeList.Codes.COD;
			var message2 = entry.Messages.AddNew();
			message2.IsTransmitMessage = true;
			message2.EM_MessageType = MessageTypeList.Codes.COD;
			var message3 = entry.Messages.AddNew();
			message3.IsTransmitMessage = true;
			message3.EM_MessageType = MessageTypeList.Codes.DCG;
			var message4 = entry.Messages.AddNew();
			message4.IsTransmitMessage = true;
			message4.EM_MessageType = MessageTypeList.Codes.COD;
			var message5 = entry.Messages.AddNew();
			message5.IsTransmitMessage = false;
			message5.EM_MessageType = MessageTypeList.Codes.COD;
			AssertEquals((short)3, wrapper.NumSeq);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			wrapper = new CODMessageEnvelopWrapper(entry);
		}

		JobDeclaration declaration;
		CusEntryHeader entry;
		CODMessageEnvelopWrapper wrapper;
	}
}
