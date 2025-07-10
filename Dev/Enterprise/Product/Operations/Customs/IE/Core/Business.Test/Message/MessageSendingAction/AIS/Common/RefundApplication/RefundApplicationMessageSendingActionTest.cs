using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.IE.Business.Declaration.CusEntryHeader;
using ZPropertyInfoExtensions = Enterprise.Customs.Business.ZPropertyInfoExtensions;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(RefundApplicationMessageSendingAction))]
	sealed class RefundApplicationMessageSendingActionTest : CusEntryHeaderMessageSendingActionTest<RefundApplicationMessageSendingAction>
	{
		public void TestMovementReferenceNumber()
		{
			entryHeader.MovementReferenceNumberSetter("MRN001");
			AssertEquals("MovementReference", "MRN001", sendingAction.MovementReferenceNumber);
		}

		public void TestMovementReferenceNumber_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingAction.MovementReferenceNumberInfo);
			AssertEquals("Movement Reference Number (MRN)", resData.Caption);
			AssertEquals("MRN", resData.ShortCaption);
		}

		public void TestMovementReferenceNumber_Readonly()
		{
			Assert(sendingAction.MovementReferenceNumberInfo.ReadOnly);
		}

		public void TestRefundType_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingAction.RefundTypeInfo);
			AssertEquals("Type", resData.Caption);
		}

		public void TestRefundType_DefaultValue()
		{
			AssertEquals(AISRefundTypeList.Codes.REP, sendingAction.RefundType);
		}

		public void TestOfficeOfDebt_CaptionAndFullDescription()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingAction.OfficeOfDebtInfo);
			AssertEquals("Office of Debt", resData.Caption);
			AssertEquals("Customs office where the debt was notified.", resData.FullDescription);
		}

		public void TestOfficeOfResponsibility_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingAction.OfficeOfResponsibilityInfo);
			AssertEquals("Office of Responsibility", resData.Caption);
			AssertEquals("Customs office responsible for the place where the goods are located.", resData.FullDescription);
		}

		public void TestLegalBasis_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingAction.LegalBasisInfo);
			AssertEquals("Legal Basis", resData.Caption);
		}

		public void TestDescriptionOfGrounds_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingAction.DescriptionOfGroundsInfo);
			AssertEquals("Description of Grounds", resData.Caption);
		}

		public void TestDescriptionOfGrounds_MaxLength()
		{
			AssertEquals(512, sendingAction.DescriptionOfGroundsInfo.MaxLength);
		}

		public void TestDescriptionOfGroundsDefault()
		{
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.LegalBasisCode, "ABC", "ABC Desc");
			Factory.Save();
			var entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var sendingAction = new RefundApplicationMessageSendingAction(entryHeader);

			sendingAction.DescriptionOfGrounds = "XYZ";
			sendingAction.LegalBasis = "ABC";
			AssertEquals("When DescriptionOfGrounds is not empty then its value should not be defaulted to the legal basis description", "XYZ", sendingAction.DescriptionOfGrounds);

			sendingAction.LegalBasis = string.Empty;
			sendingAction.DescriptionOfGrounds = string.Empty;
			sendingAction.LegalBasis = "ABC";
			AssertEquals("When DescriptionOfGrounds is empty then its value should be defaulted to the legal basis description selected", "ABC Desc", sendingAction.DescriptionOfGrounds);
		}

		public void TestBankDetails_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingAction.BankDetailsInfo);
			AssertEquals("Bank Details", resData.Caption);
		}

		public void TestBankDetails_MaxLength()
		{
			AssertEquals(512, sendingAction.BankDetailsInfo.MaxLength);
		}

		public void TestAmount_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingAction.AmountInfo);
			AssertEquals("Amount", resData.Caption);
		}

		public void TestAdditionalInformation_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingAction.AdditionalInformationInfo);
			AssertEquals("Additional Information", resData.Caption);
		}

		public void TestCheckAmount_DecimalAttributes()
		{
			AssertEquals(16, sendingAction.AmountInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			AssertEquals(2, sendingAction.AmountInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestAdditionalInformation_MaxLength()
		{
			AssertEquals(512, sendingAction.AdditionalInformationInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject() => sendingAction;

		protected override Type ExpectedLookupsType => typeof(RefundApplicationMessageSendingActionLookups);

		protected override Type ExpectedSenderType => typeof(RefundApplicationMessageSender);

		protected override Type ExpectedValidationType => typeof(RefundApplicationMessageSendingActionValidation);

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			sendingAction = new RefundApplicationMessageSendingAction(entryHeader);
		}

		CusEntryHeader entryHeader;
		RefundApplicationMessageSendingAction sendingAction;
	}
}
