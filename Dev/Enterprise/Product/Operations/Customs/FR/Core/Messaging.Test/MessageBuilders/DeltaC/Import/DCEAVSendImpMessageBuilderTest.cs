using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCEAVSendImpMessageBuilderTest : DCSendMessageBuilderTest<DCEAVSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.EAV;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>6</codact>", "<Declaration><Liquidations>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("EVA message doesn't populate <Gen> where Aeroportemb and Aertratag exist.", true);
		}

		public void TestPopulateLiquidation()
		{
			ZString liquidationItem1 = @"<LiquidationArticle><numart>1</numart><TaxationDetail><codtax>U165</codtax><typtax>0</typtax><quotax>3.1</quotax><asstax>200</asstax><montanttax>6</montanttax><statutLiquidation>1</statutLiquidation></TaxationDetail></LiquidationArticle>";
			ZString liquidationItem2 = @"<LiquidationArticle><numart>2</numart><TaxationDetail><UniSpe><unispe>DTN</unispe><qualifunispe>1</qualifunispe><nbrunispe>50</nbrunispe></UniSpe><codtax>G065</codtax><typtax>0</typtax><quotax>20</quotax><asstax>50</asstax><montanttax>10</montanttax><codeport>810</codeport><statutLiquidation>2</statutLiquidation></TaxationDetail></LiquidationArticle>";

			CreateDeclarationMock(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			message = Extensions.GetFlatXml(message);

			AssertContains("The generated message shows the export declaration tax & fees data correctly.", liquidationItem1, message);
			AssertContains("The generated message shows the export declaration tax & fees data correctly.", liquidationItem2, message);
		}
	}
}
