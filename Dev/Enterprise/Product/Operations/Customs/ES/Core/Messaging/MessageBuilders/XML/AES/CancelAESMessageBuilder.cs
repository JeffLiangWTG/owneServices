using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC514C_v514.CC514CV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
public class CancelAESMessageBuilder : AESCommonMessageBuilder<ICancelAESMessageDataProvider, Cc514Cv1Ent>
{
	public CancelAESMessageBuilder(ICancelAESMessageDataProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	protected override ZString GetMessageType() => "CC514C";

	protected override Cc514Cv1Ent GenerateXMLMessage()
	{
		var declaration = GetPopulatedTransactionId<Cc514Cv1Ent>();
		if (declaration != null)
		{
			declaration.Cc514C = GetPopulatedCC514C();
		}
		return declaration;
	}

	Cc514CType GetPopulatedCC514C()
	{
		var cC514Cv514 = GetPopulatedMessage<Cc514CType>();
		if (cC514Cv514 != null)
		{
			cC514Cv514.ExportOperation = GetPopulatedExportOperation();
			cC514Cv514.CustomsOfficeOfExport = GetPopulatedCustomsOffice<CustomsOfficeOfExportType01>(provider.CustomsOfficeOfExport);
			cC514Cv514.Exporter = GetPopulatedAddressInformationIdCommon<ExporterType03>(provider.Exporter);
			cC514Cv514.Declarant = GetPopulatedAddressInformationIdCommon<DeclarantType06>(provider.Declarant);
			cC514Cv514.Representative = GetPopulatedRepresentative(provider.Representative);
		}
		return cC514Cv514;
	}

	ExportOperationType514 GetPopulatedExportOperation()
	{
		var exportOperation = provider.ExportOperation;
		return exportOperation == null ? null : new ExportOperationType514
		{
			Mrn = exportOperation.MRN,
			InvalidationReason = exportOperation.InvalidationReason,
		};
	}

	RepresentativeType03 GetPopulatedRepresentative(ICommonRepresentative representativeProvider)
	{
		var representative = GetPopulatedAddressInformationIdCommon<RepresentativeType03>(representativeProvider);
		if (representative != null)
		{
			representative.Status = representativeProvider.Status;
		}
		return representative;
	}
}
