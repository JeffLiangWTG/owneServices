using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	[TestedType(typeof(DeltaIEJobDeclarationMessageSendingObject))]
	public class DeltaIEJobDeclarationMessageSendingObjectTest : Customs.Business.Testing.JobDeclarationMessageSendingObjectTest
	{
		public void TestVOCReason_Captions()
		{
			var captions = DataBoundResourceStrings.GetDataForProperty(typeof(DeltaIEJobDeclarationMessageSendingObject), nameof(DeltaIEJobDeclarationMessageSendingObject.VOCReason)).GetCaptions();
			AssertContainsExactElementsInExactOrder("VOCReason captions", captions, new[] { "Reason", "Reason", "Reason" });
		}

		public void TestChangeAcknowledgementIndicator_Captions()
		{
			var captions = DataBoundResourceStrings.GetDataForProperty(typeof(DeltaIEJobDeclarationMessageSendingObject), nameof(DeltaIEJobDeclarationMessageSendingObject.ChangeAcknowledgementIndicator)).GetCaptions();
			AssertContainsExactElementsInExactOrder("ChangeAcknowledgementIndicator captions", captions, new[] { "Motivation", "Motivation", "Motivation" });
		}

		public void TestChangeAcknowledgementIndicator_MaxLength()
		{
			SetupTestItem();
			AssertEquals("ChangeAcknowledgementIndicator MaxLength should be 5.", 5, testItem.ChangeAcknowledgementIndicatorInfo.MaxLength);
		}

		public void TestVOCReasonReadOnly_ShouldBeFalse_WhenMessageTypeIs413or414()
		{
			SetupTestItem();
			CombineAssertions(() =>
			{
				testItem.MessageType = DeltaIESendMessageSubTypeList.Codes.ImportDeclaration;
				Assert("VOCReason should be readonly for IE415.", testItem.VOCReasonInfo.ReadOnly);

				testItem.MessageType = DeltaIESendMessageSubTypeList.Codes.Invalidation;
				Assert("VOCReason should be editable for IE414.", !testItem.VOCReasonInfo.ReadOnly);

				testItem.MessageType = DeltaIESendMessageSubTypeList.Codes.AmendmentRequest;
				Assert("VOCReason should be editable for IE413.", !testItem.VOCReasonInfo.ReadOnly);
			});
		}

		public void TestChangeAcknowledgementIndicatorReadOnly_ShouldBeFalse_WhenMessageTypeIs413or414()
		{
			SetupTestItem();
			CombineAssertions(() =>
			{
				testItem.MessageType = DeltaIESendMessageSubTypeList.Codes.ImportDeclaration;
				Assert("ChangeAcknowledgementIndicator should be readonly for IE415.", testItem.ChangeAcknowledgementIndicatorInfo.ReadOnly);

				testItem.MessageType = DeltaIESendMessageSubTypeList.Codes.Invalidation;
				Assert("ChangeAcknowledgementIndicator should be editable for IE414.", !testItem.ChangeAcknowledgementIndicatorInfo.ReadOnly);

				testItem.MessageType = DeltaIESendMessageSubTypeList.Codes.AmendmentRequest;
				Assert("ChangeAcknowledgementIndicator should be editable for IE413.", !testItem.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
		}

		public void TestMessageTypeIsNotReadOnly()
		{
			SetupExport();
			SetupTestItem();
			AssertEquals("(Export) Message Type should not be read only", false, testItem.MessageTypeInfo.ReadOnly);

			SetupImport();
			SetupTestItem();
			AssertEquals("(Import) Message Type should not be read only", false, testItem.MessageTypeInfo.ReadOnly);
		}

		public void TestLookups()
		{
			SetupExport();
			SetupTestItem();
			AssertType<DeltaIEJobDeclarationMessageSendingObjectLookups>(testItem.Lookups);

			SetupImport();
			SetupTestItem();
			AssertType<DeltaIEJobDeclarationMessageSendingObjectLookups>(testItem.Lookups);
		}

		public void TestValidation()
		{
			SetupExport();
			SetupTestItem();
			AssertType<DeltaIEJobDeclarationMessageSendingObjectValidation>(testItem.Validation);

			SetupImport();
			SetupTestItem();
			AssertType<DeltaIEJobDeclarationMessageSendingObjectValidation>(testItem.Validation);
		}

		public void TestOperatorRequestReference()
		{
			SetupTestItem();
			AssertEquals("OperatorRequestReference should be equal to entry.CorrelationID - YYYYMMDDHHMMSS", "0123456789" + "-" + ZDateTime.UtcNow.ToString("yyyyMMddhhmmss"), testItem.OperatorRequestReference);
		}

		[TestDate(2025, 03, 19, 09, 44, 30)]
		public void TestDateTime()
		{
			SetupTestItem();
			entry.CH_EntrySubmittedDate = new ZDateTime(2020, 01, 01, 12, 30, 00);
			AssertEquals("DateTime should be equal to current date, not entry CH_EntrySubmittedDate.", new ZDateTime(2025, 03, 19, 09, 44, 30), testItem.DateTime);
		}

		#region Overrides of BusinessObjectBaseTestCase

		public override void TestProperties()
		{
			SetupExport();
			SetupTestItem();

			CombineAssertions(() =>
			{
				AssertEquals("(Export) MovementReferenceNumber", "MRN", testItem.MovementReferenceNumber);
				AssertEquals("(Export) DeclarationType", "11", testItem.DeclarationType);
				AssertEquals("(Export) Description", "Desc", testItem.Description);
				AssertEquals("(Export) Entry Status", "09", testItem.EntryStatus);
				AssertEquals("(Export) Sub Style", "SUB", testItem.SubStyle);
				AssertEquals("(Export) Entry Type", "11 (FRSUB)", testItem.EntryType);
				AssertEquals("(Export) Update", true, testItem.Update);
				AssertEquals("(Export) Message Type", "413", testItem.MessageType);
			});

			SetupImport();
			SetupTestItem();

			CombineAssertions(() =>
			{
				AssertEquals("(Import) MovementReferenceNumber", "MRN", testItem.MovementReferenceNumber);
				AssertEquals("(Import) DeclarationType", "11", testItem.DeclarationType);
				AssertEquals("(Import) Description", "Desc", testItem.Description);
				AssertEquals("(Import) Entry Status", "09", testItem.EntryStatus);
				AssertEquals("(Import) Sub Style", "SUB", testItem.SubStyle);
				AssertEquals("(Import) Entry Type", "11 (FRSUB)", testItem.EntryType);
				AssertEquals("(Import) Update", true, testItem.Update);
				AssertEquals("(Import) Message Type", "413", testItem.MessageType);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			declaration = Factory.New<JobDeclaration>();
			instruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_JE = declaration.PK;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			return new DeltaIEJobDeclarationMessageSendingObject(entry);
		}
		#endregion

		#region Setup
		JobDeclaration declaration;
		CusEntryInstruction instruction;
		CusEntryHeader entry;
		DeltaIEJobDeclarationMessageSendingObjectParent messageSendingObjectParent;
		DeltaIEJobDeclarationMessageSendingObject testItem;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			instruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_JE = declaration.PK;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.CorrelationID = "0123456789";
		}

		protected void SetupExport()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_CustomsOffice = "EOF";
			var customsOffice = declaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
			customsOffice.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
			customsOffice.CY_Data = "ECO";

			SetupGeneral();
		}

		protected void SetupImport()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

			SetupGeneral();
		}

		protected void SetupGeneral()
		{
			instruction.CEI_Style = "11";
			instruction.CEI_SubStyle = "SUB";
			instruction.CEI_Description = "Desc";
			entry.CH_MessageType = "CUS";
			entry.CH_EntryStatus = "09";
			entry.CH_Status = "CLO";
			entry.CH_BGMReference = "REF000000001";
			entry.EntryNumber = "ENT00000001";
			entry.CusEntryNumber.CE_IssueDate = ZDateTime.Empty;
			entry.MovementReferenceNumberSetter("MRN", ZDateTime.BrettsBirthday);
		}

		protected void SetupTestItem()
		{
			messageSendingObjectParent = new DeltaIEJobDeclarationMessageSendingObjectParent(declaration);
			testItem = messageSendingObjectParent.SendingObjectsCollection[0];
			testItem.Update = true;
		}
		#endregion
	}
}
