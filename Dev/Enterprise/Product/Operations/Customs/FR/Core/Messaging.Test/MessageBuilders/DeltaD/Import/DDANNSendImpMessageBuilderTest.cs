using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDANNSendImpMessageBuilderTest : DDSendMessageBuilderTest<DDANNSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.ANN;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>", "<Motivation>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>8</codact>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("ANN message doesn't populate DSIComp where Aeroportemb and Aertratag exist.", true);
		}

		public void TestPopulateMotivation()
		{
			var motivation = GenerateMotivation("cancel explanation");
			AssertEquals("cancel explanation", motivation.Motiv);

			var motivation2 = GenerateMotivation("");
			AssertEquals(null, motivation2);
		}

		static CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Import.TMotivation GenerateMotivation(string motivation)
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

			var builder = new DDANNSendImpMessageBuilderForTest(myDecl.Object, new EU.Business.ErrorCollector(), TransactionTypes.Original, 999);

			return builder.PopulateMotivation(myDeclHeader.Object);
		}
	}

	public class DDANNSendImpMessageBuilderForTest : DDANNSendImpMessageBuilder
	{
		public DDANNSendImpMessageBuilderForTest(IDeclarationImportExport declaration
			, EU.Business.ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declaration, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		public new CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Import.TMotivation PopulateMotivation(IHeader motivationHeader)
		{
			return base.PopulateMotivation(motivationHeader);
		}
	}
}
