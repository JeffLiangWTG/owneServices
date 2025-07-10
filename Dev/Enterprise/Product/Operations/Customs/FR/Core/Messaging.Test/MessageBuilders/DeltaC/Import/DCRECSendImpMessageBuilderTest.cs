using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCRECSendImpMessageBuilderTest : DCSendMessageBuilderTest<DCRECSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.REC;

		protected override ZString[] ItemsNotContains => new ZString[] { "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>10</codact>", "<Motivation>" };

		public void TestPopulateDCSendMessageBuilderHeaderWithRectification()
		{
			CreateDeclarationMock(MessageType, IsImport);

			var messageHeaderPart = @"<Entete><codact>10</codact><Motivation><motiv>Rectification explanation</motiv></Motivation><refdos>8461132</refdos></Entete>";
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			message = Extensions.GetFlatXml(message);
			AssertContains(messageHeaderPart, message);
		}
	}
}
