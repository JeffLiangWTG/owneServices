using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;
public class EvvSignatureDocumentWrapper : DocumentWrapper
{
	public static EvvSignatureDocumentWrapper New(CHEDIMessage message, BusinessObjectFactory factory) => new EvvSignatureDocumentWrapper(message, factory);

	protected EvvSignatureDocumentWrapper(CHEDIMessage message, BusinessObjectFactory factory) : base(message.EM_LinkedObject, factory)
	{
		responseData = message.MessageDetail as IEvvCommonProvider;
		signatureParser = new SignatureParser(message.EM_MessageText);
	}
	readonly IEvvCommonProvider responseData;
	readonly SignatureParser signatureParser;

	public ZString DocumentNumber => responseData?.DocumentNumber ?? ZString.Empty;

	public ZString DocumentVersion => responseData?.DocumentVersion ?? ZString.Empty;

	public ZString DocumentType => responseData?.DocumentType ?? ZString.Empty;

	public ZString RequestorTraderIdentificationNumber => responseData?.RequestorTraderIdentificationNumber ?? ZString.Empty;

	public ZString SignatureValidationText => GetValidationResultText(signatureParser?.SignatureValidationResult ?? false);

	public ZString CertificateDateValidationText => GetValidationResultText(signatureParser?.CertificateDateValidationResult ?? false);

	public ZString CertificateRevocationListValidationText => GetValidationResultText(signatureParser?.CertificateRevocationListValidationResult ?? false);

	public ZString CertificateChainValidationText => GetValidationResultText(signatureParser?.CertificateChainValidationResult ?? false);

	public ZString ValidationSummaryText => GetValidationResultText(signatureParser?.ValidationSummaryResult ?? false);

	ZString GetValidationResultText(bool result) => result ? ValidationOK : ValidationNotOK;

	static string ValidationOK => Res.GetString("C7055C4E-0CCB-48CD-B3F3-A1F730778441", "OK");
	static string ValidationNotOK => Res.GetString("3AAB8DFB-019E-417B-BA6C-9E175B923CA5", "not OK");
}
