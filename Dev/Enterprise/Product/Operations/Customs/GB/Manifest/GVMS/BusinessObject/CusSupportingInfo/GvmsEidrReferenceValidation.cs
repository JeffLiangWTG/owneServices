namespace Enterprise.Customs.GB.GVMS;

public class GvmsEidrReferenceValidation : GvmsItemReferenceValidation
{
	public GvmsEidrReferenceValidation(GvmsEidrReference parent) : base(parent)
	{
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		var parent = (GvmsEidrReference)Parent;
		var header = (AsycudaManifestHeader)parent.Parent;
		if (parent.CSI_Code == GVMSCustomsReference.Codes.UkInternalMarketSchemeEntryInDeclarantsRecordsDeclaration && header.AMA_Nature != GVMSManifestNature.Codes.GBtoNI)
		{
			parent.CSI_CodeInfo.AddMessageError("IMS declarations are only allowed for GB-to-NI (nature G2N) movements");
		}
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		var parent = (GvmsEidrReference)Parent;
		var header = (AsycudaManifestHeader)parent.Parent;
		if (parent.CSI_ReferenceNumber.IsEmpty)
		{
			return;
		}
		if (IsValidateEoriReferencePrefix(parent.CSI_Code, header.AMA_Nature) && !parent.CSI_ReferenceNumber.StartsWith(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes))
		{
			parent.CSI_ReferenceNumberInfo.AddMessageError("For the selected nature and declaration type, only XI-prefixed EORIs are allowed");
		}
	}

	bool IsValidateEoriReferencePrefix(string declarationType, string manifestNature)
	{
		return ((declarationType == GVMSCustomsReference.Codes.EntryInDeclarantsRecord && manifestNature == GVMSManifestNature.Codes.GBtoNI)
			|| declarationType == GVMSCustomsReference.Codes.UkInternalMarketSchemeEntryInDeclarantsRecordsDeclaration);
	}
}
