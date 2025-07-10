using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCAESC_v514.CCAESCV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class QueryAESMessageBuilder : AESCommonMessageBuilder<IQueryAESMessageDataProvider, Ccaescv1Ent>
	{
		public QueryAESMessageBuilder(IQueryAESMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CCAESC";

		protected override Ccaescv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Ccaescv1Ent>();
			if (declaration != null)
			{
				declaration.Ccaesc = GetPopulatedCCAESC();
			}
			return declaration;
		}

		CcaescType GetPopulatedCCAESC()
		{
			var cCAESv514 = GetPopulatedMessage<CcaescType>();
			if (cCAESv514 != null)
			{
				cCAESv514.ExportOperation = GetPopulatedExportOperationMRN();
			}
			return cCAESv514;
		}

		ConsultaExportacionType GetPopulatedExportOperationMRN()
		{
			var exportOperation = provider.ExportOperation;
			return exportOperation == null ? null : new ConsultaExportacionType
			{
				Mrn = exportOperation.MRN,
			};
		}
	}
}
