using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecRuleErrorMessageProcessor))]
sealed class EdecRuleErrorMessageProcessorTest : BaseInboundMessageProcessorTest
{
	protected override (string MessageType, Event ExpectedEvent)[] MessageTypes => new[] { (JobMessageTypeList.Codes.Import, Events.DeclarationRejected), (JobMessageTypeList.Codes.Export, Events.DeclarationRejected) };

	protected override ZString ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.RuleError;

	protected override ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger) => new EdecRuleErrorMessageProcessor(logger);

	protected override ZString ExpectedMessageFriendlyName => "Rule Error Message Processor";

	protected override ZString BGMReference => "35253";

	readonly ZString rejectionDate = "2021-10-14";

	readonly ZString rejectionTime = "11:36:24";

	#region Message Response

	protected override ZString ReceivedMessage => $@"<?xml version= ""1.0"" encoding= ""UTF-8""?>
<goodsDeclarationsResponse schemaVersion= ""4.0"" xmlns= ""http://www.e-dec.ch/xml/schema/edecResponse/v4"" xmlns:xsi= ""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation= ""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"">
  <goodsDeclarationRejection>
    <rejectionDate>{rejectionDate}</rejectionDate>
    <rejectionTime>{rejectionTime}</rejectionTime>
    <errors>
      <ruleErrors>
        <traderDeclarationNumber>{BGMReference}</traderDeclarationNumber>
        <traderReference>MM_TEST_2009_eBegleit</traderReference>
        <declarant>
          <traderIdentificationNumber>CHE293274655</traderIdentificationNumber>
          <declarantNumber>72</declarantNumber>
        </declarant>
        <error>
          <ruleName>R109c</ruleName>
          <checkType>Version Check</checkType>
          <reference>goodsDeclaration</reference>
          <referencedElements>
            <referencedElement>/goodsDeclarations/goodsDeclaration/correctionCode</referencedElement>
          </referencedElements>
          <descriptions>
            <description language= ""de"">ungÃ¼ltige Version der Ã¼bermittelten Einfuhrzollanmeldung</description>
            <description language= ""fr"">Version de la dÃ©claration en douane importation transmise non valable</description>
            <description language= ""it"">La versione della dichiarazione doganale d'importazione trasmessa non Ã¨ valida</description>
          </descriptions>
        </error>
        <error>
          <ruleName>R213</ruleName>
          <checkType>Preference Check</checkType>
          <reference>traderItemID:1</reference>
          <referencedElements>
            <referencedElement>/goodsDeclarations/goodsDeclaration/preference</referencedElement>
          </referencedElements>
          <descriptions>
            <description language= ""de"">Wenn der Veranlagungstyp nicht 10 oder 11 ist und die Prüferenzveranlagung beantragt wird (Flag origin/preference = 1) und Ursprungsland (origin/orignCountry) nicht Schweiz oder Lichtenstein ist, muss Praeferenzansatz gem. Stammdaten vorhanden sein</description>
            <description language= ""fr"">Si le type de taxation n'est pas 10 ou 11 et que la taxation de référence de contrôle est demandée (Flag origin/preference = 1) et que le pays d'origine (origin/orignCountry) n'est pas la Suisse ou le Lichtenstein, le taux de référence doit être disponible selon les données de base</description>
            <description language= ""it"">Se il tipo di valutazione non è 10 o 11 e viene richiesta la valutazione di riferimento del test (flag origin/preference = 1) e il paese di origine (origin/orignCountry) non è la Svizzera o il Liechtenstein, l'approccio di riferimento deve essere disponibile secondo i dati anagrafici</description>
          </descriptions>
        </error>
      </ruleErrors>
    </errors>
  </goodsDeclarationRejection>
</goodsDeclarationsResponse>";

	#endregion

	public void TestResponseErrorMessage_Success() => CombineAssertions(() =>
	{
		foreach (var messageType in MessageTypes)
		{
			var factory = new BusinessObjectFactory();

			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessage(factory, messageType.MessageType, MessageSubType, ReceivedMessage, BGMReference);
			entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Amendment;

			ProcessMessage(ediMessage);

			var logEntry = factory.Load<StmALog>(new ZQuery(ZArchitecture.Schema.StmALogSchema.SL_Parent, entryHeader.PK)).FirstOrDefault();

			AssertEquals("CH_Status", CHLogicalStatusList.Codes.Invalid, entryHeader.CH_Status);
			AssertEquals("CH_PhaseStatus", PassarDeclarationPhaseList.Codes.Amendment, entryHeader.CH_PhaseStatus);
			AssertNotNull("CusEntryHeader log", logEntry);
			AssertEquals("Event type", messageType.ExpectedEvent, logEntry.Event);
			AssertEquals("SL_EventTimeOffset", $"{rejectionDate} {rejectionTime}", logEntry.SL_EventTimeOffset.ToString("yyyy-MM-dd HH:mm:ss"));
		}
	});
}
