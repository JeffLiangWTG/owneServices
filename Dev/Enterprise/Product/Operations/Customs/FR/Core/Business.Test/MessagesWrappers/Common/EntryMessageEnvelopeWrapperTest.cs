using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	public class EntryMessageEnvelopeWrapperTest : TestCaseWithFactory
	{
		public void TestSchemaID()
		{
			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.IMC);
			AssertEquals(MessageEnvelopeWrapper.deltaCSchemaImport, wrapper.SchemaID);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.EXC);
			AssertEquals(MessageEnvelopeWrapper.deltaCSchemaExport, wrapper.SchemaID);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.IMD);
			AssertEquals(MessageEnvelopeWrapper.deltaDSchemaImport, wrapper.SchemaID);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.EXD);
			AssertEquals(MessageEnvelopeWrapper.deltaDSchemaExport, wrapper.SchemaID);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.DCG);
			AssertEquals(MessageEnvelopeWrapper.deltaDcgSchema, wrapper.SchemaID);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.CIN);
			AssertEquals(MessageEnvelopeWrapper.cinSchema, wrapper.SchemaID);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.ARR);
			AssertEquals(ZString.Empty, wrapper.SchemaID);
		}

		public void TestSchemaVersion()
		{
			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.IMC);
			AssertEquals(MessageEnvelopeWrapper.deltaCSchemaVersion, wrapper.SchemaVersion);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.EXC);
			AssertEquals(MessageEnvelopeWrapper.deltaCSchemaVersion, wrapper.SchemaVersion);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.IMD);
			AssertEquals(MessageEnvelopeWrapper.deltaDSchemaVersion, wrapper.SchemaVersion);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.EXD);
			AssertEquals(MessageEnvelopeWrapper.deltaDSchemaVersion, wrapper.SchemaVersion);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.DCG);
			AssertEquals(MessageEnvelopeWrapper.deltaDSchemaVersion, wrapper.SchemaVersion);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.CIN);
			AssertEquals(ZString.Empty, wrapper.SchemaVersion);

			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.ARR);
			AssertEquals(ZString.Empty, wrapper.SchemaVersion);
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
			entry.CH_SequenceNumber = 18;
			AssertEquals((short)18, wrapper.NumSeq);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			wrapper = new EntryMessageEnvelopeWrapper(entry, MessageSubTypeList.Codes.IMC);
		}

		JobDeclaration declaration;
		CusEntryHeader entry;
		MessageEnvelopeWrapper wrapper;
	}
}
