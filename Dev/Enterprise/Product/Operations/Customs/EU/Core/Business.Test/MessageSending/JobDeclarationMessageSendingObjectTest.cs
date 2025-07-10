using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObjectForTest))]
	class JobDeclarationMessageSendingObjectTest : Customs.Business.Testing.JobDeclarationMessageSendingObjectTest
	{
		public void TestEntryTypeCaption()
		{
			AssertResourceStringData(JobDeclarationMessageSendingObject.Schema.EntryType, "Entry Type", string.Empty, "Type");
		}

		public void TestEntryStatusDescriptionCaption()
		{
			AssertResourceStringData(JobDeclarationMessageSendingObject.Schema.EntryStatusDescription, "Entry Status Description", "Entry Status Desc.", "Status Desc.");
		}

		public void TestBGMReferenceCaption()
		{
			AssertResourceStringData(JobDeclarationMessageSendingObject.Schema.BGMReference, "Reference Number", "Reference No.", "Ref. No.");
		}

		public override void TestProperties()
		{
			var testItem = (JobDeclarationMessageSendingObjectForTest)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("Entry Type", "IFD (IM)", testItem.EntryType);
				AssertEquals("Entry Status Description", EntryStatusList.Descriptions.AwaitingResponse, testItem.EntryStatusDescription);
				AssertEquals("BGM Reference", "ENT1", testItem.BGMReference);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "IFD";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.AwaitingResponse;
			entryHeader.CH_BGMReference = "ENT1";

			return new JobDeclarationMessageSendingObjectForTest(declaration.CustomsEntryHeaders[0]);
		}

		static void AssertResourceStringData(string propertyName, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
		{
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclarationMessageSendingObject), propertyName);
				AssertEquals("Caption", expectedCaption, resourceStringData.Caption);
				AssertEquals("Medium Caption", expectedMediumCaption, resourceStringData.MediumCaption);
				AssertEquals("Short Caption", expectedShortCaption, resourceStringData.ShortCaption);
			});
		}

		class JobDeclarationMessageSendingObjectForTest : JobDeclarationMessageSendingObject
		{
			public JobDeclarationMessageSendingObjectForTest(CusEntryHeader header)
				: base(header)
			{
			}
		}
	}
}
