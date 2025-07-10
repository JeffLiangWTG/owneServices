using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class WARRETMessageBuilderTest : CMRMessageBuilderAbstractTest
	{
		public void TestOriginalWarehouseReleaseMessage()
		{
			var expectedMessage = CMRExportMessagesTestData.WARRETOriginal;
			var builder = new WARRETMessageBuilder(declaration);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("MessageString", expectedMessage, builder.MessageText);
		}

		public void TestReplaceWarehouseReleaseMessage()
		{
			var expectedMessage = CMRExportMessagesTestData.WARRETReplace;
			var builder = new WARRETMessageBuilder(declaration);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			AssertEquals("MessageString", expectedMessage, builder.MessageText);
		}

		public void TestUsingEntryLineToBuildMessage()
		{
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("Precondition: One Customs Entry Header created", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("Precondition: EntryLine number", 1, declaration.ActiveEntryHeaders[0].MergedLines.Count);

			var entryNumber = AUCusEntryNumber.LoadEntryNumber(Factory, declaration.PK, JobDeclaration.Schema.TableName, Core.Constants.CountryCodes.Australia, false).First();
			entryNumber.Parent = declaration.EntryHeader;
			var expectedMessage = CMRExportMessagesTestData.WARRETOriginal;
			var builder = new WARRETMessageBuilder(declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault());
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("Message should work like before", expectedMessage, builder.MessageText);
		}

		protected override CMRMessageBuilder GetMessageBuilderToTest() => new WARRETMessageBuilder(declaration);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000001";
			SetDepotCode(declaration, "9172E");
			SetWarehouseCode(declaration, "9119P");
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.DeclarationNumber = "AAACFLYXE";
			AddInvoiceLine(declaration, "2208.40.00", 100m, "LA", "RUM");
			declaration.JE_EstimatedDeliveryOrPickup = new ZDateTime(2007, 6, 24, 22, 29, 0);
		}
		JobDeclaration declaration;
	}
}
