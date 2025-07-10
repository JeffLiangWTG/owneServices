using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;
using ZPropertyInfoExtensions = Enterprise.Customs.Business.ZPropertyInfoExtensions;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DocumentsSendingAction))]
	class DocumentsSendingActionTest : CusEntryHeaderMessageSendingActionTest<DocumentsSendingAction>
	{
		public void TestMovementReference()
		{
			entryHeader.MovementReferenceNumberSetter("MRN001");
			AssertEquals("MovementReference", "MRN001", documentsSendingAction.MovementReference);
		}

		public void TestMovementReference_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(documentsSendingAction.MovementReferenceInfo);
			AssertEquals("MovementReferenceInfo caption", "Movement Reference Number", resData.Caption);
			AssertEquals("MovementReferenceInfo ShortCaption", "MRN", resData.ShortCaption);
		}

		public void TestLocalReference()
		{
			entryHeader.CH_BGMReference = "LRN001";
			AssertEquals("LocalReference", "LRN001", documentsSendingAction.LocalReference);
		}

		public void TestLocalReference_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(documentsSendingAction.LocalReferenceInfo);
			AssertEquals("LocalReferenceInfo caption", "Local Reference Number", resData.Caption);
			AssertEquals("LocalReferenceInfo ShortCaption", "LRN", resData.ShortCaption);
		}

		protected override Type ExpectedLookupsType => typeof(DocumentsSendingActionLookups);

		protected override Type ExpectedSenderType => typeof(DocumentsSender);

		protected override Type ExpectedValidationType => typeof(DocumentsSendingActionValidation);

		protected override BusinessObject GetNewBusinessObject() => documentsSendingAction;

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			documentsSendingAction = new DocumentsSendingAction(entryHeader);
		}

		CusEntryHeader entryHeader;
		DocumentsSendingAction documentsSendingAction;
	}
}
