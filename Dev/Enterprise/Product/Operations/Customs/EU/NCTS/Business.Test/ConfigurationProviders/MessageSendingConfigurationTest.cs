using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public abstract class MessageSendingConfigurationAbstractTest<M> : TestCaseWithFactory
		where M : MessageSendingConfiguration, new()
	{
		public abstract void TestMessageTypeList();
		public abstract void TestSetDefaultMessageType();
		public abstract void TestGetNewNctsMessageSendingObjectParent();
		public abstract void TestGetShouldSendDefault();
		public abstract void TestShowJustification();

		protected abstract Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType { get; }
		public void TestValidationDecider()
		{
			AssertType(ExpectedNctsHeaderMessageSendingObjectValidationDeciderType, configuration.GetValidationDecider());
		}

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new M();
		}
		protected M configuration;
	}

	sealed class MessageSendingConfigurationForTest : MessageSendingConfiguration
	{
		protected override void SetDefaultMessageTypeCore(NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
		{
			nctsHeaderMessageSendingObject.MessageType = "123";
		}
	}

	sealed class MessageSendingConfigurationBaseOnlyTest : MessageSendingConfigurationAbstractTest<MessageSendingConfigurationForTest>
	{
		public void TestShouldFillAdditionalWarningsOnSendScreen()
		{
			AssertEquals(expected: false, configuration.ShouldFillAdditionalWarningsOnSendScreen);
		}

		public void TestReleaseRequestCode() => AssertEquals(NCTS5DeparturePhaseList.Codes.ReleaseRequest, configuration.ReleaseRequestCode);

		public void TestShouldHideSendWithAdditionalWarningCheckBox()
		{
			AssertEquals(expected: false, configuration.ShouldHideSendWithAdditionalWarningCheckBox);
		}

		protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(NctsHeaderMessageSendingObjectValidationDecider);

		public override void TestMessageTypeList()
		{
			header.BH_ApplicationCode = Customs.Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("To make sure the Movement is phase4 and not arrival.", true, !header.IsPhase5 && !header.IsArrivalMovement);
			AssertEquals("Empty MessageTypeList by default(phase 4 and departure)", string.Empty, configuration.MessageTypeList(header).CodesAsString);

			header.BH_ApplicationCode = Customs.Common.CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("To make sure the Movement is phase5 and departure.", true, header.IsPhase5 && header.IsDepartureMovement);
			AssertEquals("Include 014, 015, 170 in MessageTypeList when (phase5, departure)", "015, 014, 170", configuration.MessageTypeList(header).CodesAsString);

			var anotherHeader = Factory.New<NctsHeader>();
			anotherHeader.BH_ApplicationCode = Customs.Common.CusInBondApplicationCodeList.Codes.NCTS5;
			anotherHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertSame("Caching", configuration.MessageTypeList(header), configuration.MessageTypeList(anotherHeader));

			header.BH_HeaderType = ZString.Empty;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertEquals("To make sure the Movement is phase5 and not arrival.", true, header.IsPhase5 && header.IsArrivalMovement);
			AssertEquals("Include 007,044 in MessageTypeList when (phase5, arrival)", "007, 044", configuration.MessageTypeList(header).CodesAsString);

			anotherHeader.BH_HeaderType = ZString.Empty;
			anotherHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertSame("Caching", configuration.MessageTypeList(header), configuration.MessageTypeList(anotherHeader));
		}

		public override void TestSetDefaultMessageType()
		{
			var sendingObject = new NctsHeaderMessageSendingObject(header);
			configuration.SetDefaultMessageType(sendingObject);
			AssertEquals("123", sendingObject.MessageType);
		}

		public override void TestGetNewNctsMessageSendingObjectParent()
		{
			AssertType<NctsHeaderMessageSendingObjectParent>(configuration.GetNewNctsHeaderMessageSendingObjectParent(header));
		}

		public override void TestGetShouldSendDefault()
		{
			var sendingObj = new NctsHeaderMessageSendingObject(header);
			Assert(configuration.GetShouldSendDefault(sendingObj));
		}

		public override void TestShowJustification()
		{
			var sendingObj = new NctsHeaderMessageSendingObject(header);
			AssertEquals(expected: true, configuration.ShowJustification(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
