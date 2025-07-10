using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.MessageSending.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class MotivationWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestMotivationWrapperConstructor()
		{
			Activator.CreateInstance(typeof(MotivationWrapper), new object[] { null });
		}

		public void TestMessageSendingDeltaMotivationProperties()
		{
			var deltaHelper = new DeltaMessageSenderHelper();

			var entry = deltaHelper.CreateEntryDeclarationForTest(true);

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.REC;
			var errCollector = new EU.Business.ErrorCollector();

			string motivationPart = "MOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIO";
			motivationPart += "MOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIO";
			motivationPart += "MOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMOTIVATIONMO";

			string commentPart = "COMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTC";
			commentPart += "COMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTC";
			commentPart += "COMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMENTCOMMEN";

			AssertExceptionThrown(typeof(CargoWise.EntityFramework.MaxLengthExceededException), () => { sendingObject.VOCReason = motivationPart + commentPart + "BLABLA"; });

			sendingObject.VOCReason = motivationPart + commentPart;

			sendingObject.ChangeAcknowledgementIndicator = Enterprise.Customs.FR.Business.ReasonCodeList.Codes.C173;
			sendingObject.ReplacementDeclarationType = Enterprise.Customs.FR.Business.ReplacementDeclarationTypeList.Codes.IST;

			motivationWrapperItem = new MotivationWrapper(sendingObject);

			AssertEquals(motivationWrapperItem.vocReasonMotivationLengthPart, motivationWrapperItem.Motivation.Length);
			AssertEquals(motivationPart, motivationWrapperItem.Motivation);
			AssertEquals(motivationWrapperItem.vocReasonMotivationLengthPart, motivationWrapperItem.Comment.Length);
			AssertEquals(commentPart, motivationWrapperItem.Comment);
			AssertEquals(Enterprise.Customs.FR.Business.ReasonCodeShortList.Descriptions.C173, motivationWrapperItem.RegularJustification);
			AssertEquals(Enterprise.Customs.FR.Business.ReplacementDeclarationTypeList.Descriptions.IST, motivationWrapperItem.NewDestination);

			sendingObject.VOCReason = motivationPart;
			motivationWrapperItem = new MotivationWrapper(sendingObject);

			AssertEquals("", motivationWrapperItem.Comment);
			CargoWise.Common.ErrorReporter.Clear();
		}
		MotivationWrapper motivationWrapperItem;
	}
}
