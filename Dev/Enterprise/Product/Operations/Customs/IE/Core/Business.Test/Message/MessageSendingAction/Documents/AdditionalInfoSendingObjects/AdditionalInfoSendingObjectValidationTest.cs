using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class AdditionalInfoSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckDocumentType()
		{
			var targetInfo = sendingObject.DocumentTypeInfo;

			parentAction.ShouldSend = ZBool.False;
			sendingObject.DocumentType = string.Empty;
			validation.ValidateAll();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			parentAction.ShouldSend = ZBool.True;
			validation.ValidateAll();
			AssertHasErrorContaining("DocumentType should have a validation.", targetInfo, MandatoryValidation.MustBeEntered);

			sendingObject.DocumentType = "9002";
			AssertNoErrors("DocumentType passed.", targetInfo);
		}

		public void TestCheckDocumentInformation()
		{
			var targetInfo = sendingObject.DocumentInformationInfo;

			parentAction.ShouldSend = ZBool.False;
			validation.ValidateAll();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			parentAction.ShouldSend = ZBool.True;
			sendingObject.DocumentInformation = "Document Information";
			sendingObject.DocumentInformation = string.Empty;
			AssertHasErrorContaining("DocumentInformation should have a validation.", sendingObject.DocumentInformationInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCCQualifier()
		{
			var targetInfo = sendingObject.CCQualifierInfo;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: euGroup);
			helper.CreateCusCodeType("CL010", "CL010");
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL010", "ZZ", "EU 1", yesterday, tomorrow);
			Factory.Save();

			parentAction.ShouldSend = ZBool.False;
			sendingObject.CCQualifier = "XY";
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);
			sendingObject.CCQualifier = string.Empty;

			parentAction.ShouldSend = ZBool.True;
			sendingObject.CCQualifier = "XY";
			AssertHasMessageErrorContaining("CCQualifier should have a validation.", targetInfo, ListValidation.InvalidCodeMessageError.ToString());

			sendingObject.CCQualifier = "ZZ";
			AssertNoErrors("CCQualifier validation passed.", targetInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var requestedDocument = entryHeader.EntryInstruction.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			parentAction = new UploadDocumentsSendingAction(entryHeader);
			sendingObject = new AdditionalInfoSendingObject(entryHeader, parentAction, requestedDocument);
			validation = sendingObject.Validation;
		}

		UploadDocumentsSendingAction parentAction;
		AdditionalInfoSendingObject sendingObject;
		AdditionalInfoSendingObjectValidation validation;
		(EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) testBizObjs;
	}
}
