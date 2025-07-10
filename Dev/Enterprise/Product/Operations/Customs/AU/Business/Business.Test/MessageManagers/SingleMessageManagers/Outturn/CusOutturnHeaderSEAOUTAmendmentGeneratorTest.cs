using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnHeaderSEAOUTAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierChangesIfVoyageChanges()
		{
			var header = (CusOutturnHeader)GetSavedBizo();
			AssertEquals(false, new CusOutturnHeaderSEAOUTAmendmentGeneratorForTest(header).UniqueIdentifierBeingChanged);
			header.C6_VoyageNum = "123";
			AssertEquals(true, new CusOutturnHeaderSEAOUTAmendmentGeneratorForTest(header).UniqueIdentifierBeingChanged);
		}

		public void TestUniqueIdentifierChangesIfVesselChanges()
		{
			var header = (CusOutturnHeader)GetSavedBizo();
			AssertEquals(false, new CusOutturnHeaderSEAOUTAmendmentGeneratorForTest(header).UniqueIdentifierBeingChanged);
			header.C6_VesselName = "ADMIRALENGRACHT";
			AssertEquals(true, new CusOutturnHeaderSEAOUTAmendmentGeneratorForTest(header).UniqueIdentifierBeingChanged);
		}

		public void TestUniqueIdentifierChangesIfEstablishmentChanges()
		{
			var header = (CusOutturnHeader)GetSavedBizo();
			AssertEquals(false, new CusOutturnHeaderSEAOUTAmendmentGeneratorForTest(header).UniqueIdentifierBeingChanged);
			header.C6_OutturningPremiseID = "123";
			AssertEquals(true, new CusOutturnHeaderSEAOUTAmendmentGeneratorForTest(header).UniqueIdentifierBeingChanged);
		}

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new CusOutturnHeaderSEAOUTAmendmentGeneratorForTest(bizo as CusOutturnHeader);

		protected override Type ExpectedMessageType => typeof(CMRSEAOUTMessage);

		protected override void AssertCommon(EDIMessage message)
		{
			AssertEquals("MessageType", ExpectedMessageType, message.GetType());
			AssertEquals("EM_LinkedObject", typeof(CusOutturnHeader), message.EM_LinkedObject.GetType());
		}

		protected override BusinessObject GetSavedBizo()
		{
			var header = CusOutturnHeader.New(Factory);
			header.Factory.Save();
			return header;
		}

		sealed class CusOutturnHeaderSEAOUTAmendmentGeneratorForTest : CusOutturnHeaderSEAOUTAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public CusOutturnHeaderSEAOUTAmendmentGeneratorForTest(CusOutturnHeader header) : base(header)
			{
			}

			internal new bool UniqueIdentifierBeingChanged => base.UniqueIdentifierBeingChanged;
			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
