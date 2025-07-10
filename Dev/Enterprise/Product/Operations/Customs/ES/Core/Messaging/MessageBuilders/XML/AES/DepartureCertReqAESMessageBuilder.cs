using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCCSEC_v514.CCCSECV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class DepartureCertReqAESMessageBuilder : AESCommonMessageBuilder<IDepartureCertReqAESMessageDataProvider, Cccsecv1Ent>
	{
		public DepartureCertReqAESMessageBuilder(IDepartureCertReqAESMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CCCSEC";

		protected override Cccsecv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Cccsecv1Ent>();
			if (declaration != null)
			{
				declaration.Cccsec = GetPopulatedCCCSEC();
			}
			return declaration;
		}

		CccsecType GetPopulatedCCCSEC()
		{
			var cCCSEv514 = GetPopulatedMessage<CccsecType>();
			if (cCCSEv514 != null)
			{
				cCCSEv514.ExportOperation = GetPopulatedExportOperationMRN();
			}
			return cCCSEv514;
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
