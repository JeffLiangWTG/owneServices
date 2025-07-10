using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Export;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDANNSendExpMessageBuilderTest : DDSendMessageBuilderTest<DDANNSendExpMessageBuilder>
	{
		protected override ZBool IsImport => false;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.ANN;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>", "<Motivation>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>8</codact>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("Export message doesn't populate Aeroportemb and Aertratag.", true);
		}

		public void TestPopulateMotivation()
		{
			var motivation = GenerateMotivation("cancel explanation");
			AssertEquals("cancel explanation", motivation.Motiv);

			var motivation2 = GenerateMotivation("");
			AssertEquals(null, motivation2);
		}

		static TMotivation GenerateMotivation(string motivation)
		{
			var myDeclHeader = new Mock<IHeader>();
			var myMessageRectificationMotiv = new Mock<IMotivation>();
			myMessageRectificationMotiv.Setup(m => m.NewDestination).Returns(new ZString(""));
			myMessageRectificationMotiv.Setup(m => m.RegularJustification).Returns(new ZString(""));
			myMessageRectificationMotiv.Setup(m => m.Comment).Returns(new ZString(""));
			myMessageRectificationMotiv.Setup(m => m.RegularJustification).Returns(new ZString(""));
			myMessageRectificationMotiv.Setup(m => m.Motivation).Returns(motivation);

			myDeclHeader.Setup(m => m.Motivation).Returns(myMessageRectificationMotiv.Object);

			myDeclHeader.Setup(m => m.ActionCode).Returns(Business.EntryActionCodeList.GetMessageCodeNumber(Business.EntryActionCodeList.Codes.ANN).ToString());

			var myDecl = new Mock<IDeclarationImportExport>();

			var builder = new DDANNSendExpMessageBuilderForTest(myDecl.Object, new EU.Business.ErrorCollector(), TransactionTypes.Original, 999);

			return builder.PopulateMotivation(myDeclHeader.Object);
		}
	}

	public class DDANNSendExpMessageBuilderForTest : DDANNSendExpMessageBuilder
	{
		public DDANNSendExpMessageBuilderForTest(IDeclarationImportExport declaration
			, EU.Business.ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declaration, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		public new TMotivation PopulateMotivation(IHeader motivationHeader)
		{
			return base.PopulateMotivation(motivationHeader);
		}
	}
}
