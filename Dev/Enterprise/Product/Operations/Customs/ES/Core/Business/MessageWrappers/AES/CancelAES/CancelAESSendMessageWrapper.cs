using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CancelAESSendMessageWrapper : AESCommonSendMessageWrapper, ICancelAESMessageDataProvider
{
	public CancelAESSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData, ReasonForCancellation reasonForCancellation) : base(cusEntryHeader, certificateData)
	{
		this.reasonForCancellation = Argument.NotNull(reasonForCancellation, nameof(reasonForCancellation));
	}
	readonly ReasonForCancellation reasonForCancellation;

	public ICancelAESExportOperation ExportOperation => exportOperation ?? (exportOperation = new CancelAESExportOperationWrapper(entryHeader, reasonForCancellation));
	CancelAESExportOperationWrapper exportOperation;

	public ZString CustomsOfficeOfExport => OfficeOfExport;

	public IPartyIdProvider Exporter => exporter ?? (exporter = AESCommonExporterWrapper.New(declaration.Supplier));
	AESCommonExporterWrapper exporter;

	public IPartyIdProvider Declarant => declarant ?? (declarant = AESCommonDeclarantWrapper.New(declaration));
	AESCommonDeclarantWrapper declarant;

	public ICommonRepresentative Representative => representative ?? (representative = CommonRepresentativeWrapper.New(declaration));
	CommonRepresentativeWrapper representative;
}
