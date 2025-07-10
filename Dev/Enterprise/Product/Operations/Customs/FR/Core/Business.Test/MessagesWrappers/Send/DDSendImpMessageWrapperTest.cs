using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Customs.FR.Messaging.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Send.Testing
{
	public class DDSendImpMessageWrapperTest : DCSendMessageWrapperTest
	{
		protected override IDeclarationImportExport GetMessageWrapper(MessageSending.DeltaGJobDeclarationMessageSendingObject objectToSend, ErrorCollector itemErrorCollector) => new DDSendImpMessageWrapper(objectToSend, itemErrorCollector);
		protected override ZString SchemaID => "MessageDecImp";
		protected override ZString SchemaVersion => "18122012";
		protected override ZString Application => "DELTAD";
		protected override ZString DeltaMode => OrgCusAccountDeltaGTypeList.Codes.G2;
		protected override ZString MessageType => EU.Business.MessageTypeList.Codes.Import;
		protected override ZString MessageSubType => MessageSubTypeList.Codes.IMD;
		protected override ZInt ExpectedLiquidationCount => 3;

		public override void TestCusProcedureType()
		{
			var declaration = CreateJobDeclaration();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);

			CombineAssertions(() =>
			{
				AssertCusProcedureType<VALProcedureWrapper>(messageObject, EntryActionCodeList.Codes.VAL);
				AssertCusProcedureType<D2MProcedureWrapper>(messageObject, EntryActionCodeList.Codes.D2M);
				AssertCusProcedureType<ANTProcedureWrapper>(messageObject, EntryActionCodeList.Codes.ANT);
				AssertCusProcedureType<MDVProcedureWrapper>(messageObject, EntryActionCodeList.Codes.MDV);
				AssertCusProcedureType<RPSProcedureWrapper>(messageObject, EntryActionCodeList.Codes.RPS);
				AssertCusProcedureType<MDAProcedureWrapper>(messageObject, EntryActionCodeList.Codes.MDA);
				AssertCusProcedureType<VAAProcedureWrapper>(messageObject, EntryActionCodeList.Codes.VAA);
				AssertCusProcedureType<ANNProcedureWrapper>(messageObject, EntryActionCodeList.Codes.ANN);
				AssertCusProcedureType<RECProcedureWrapper>(messageObject, EntryActionCodeList.Codes.REC);
				AssertCusProcedureType<INVProcedureWrapper>(messageObject, EntryActionCodeList.Codes.INV);
			});
		}
	}
}
