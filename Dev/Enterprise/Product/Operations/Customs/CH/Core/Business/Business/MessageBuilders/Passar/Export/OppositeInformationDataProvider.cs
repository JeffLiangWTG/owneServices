using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class OppositeInformationDataProvider : IOppositeInformation
{
	public static OppositeInformationDataProvider New(JobDeclaration declaration) => declaration == null ? null : new OppositeInformationDataProvider(declaration);

	OppositeInformationDataProvider(JobDeclaration declaration)
	{
		this.declaration = declaration;
	}
	readonly JobDeclaration declaration;

	public string ReferenceNumber => !declaration.JE_OwnerRef.IsEmpty ? declaration.JE_OwnerRef : declaration.JE_DeclarationReference.ReturnNullIfEmpty();

	public string Detail => null;

	public string Text => CustomsMessageHelper.PartnerTopic;
}
