using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDescAcrManager
{
	public NctsDepartureCargoDescAcrManager(NctsDepartureCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		Argument.NotNull(goodsItem.Header, nameof(goodsItem.Header));
	}
	readonly NctsDepartureCargoDesc goodsItem;

	public void Validate()
	{
		var requestedAcrDocType = GetAcrDocAuthorisation();
		if (!requestedAcrDocType.IsEmpty && !HasAnySupportingDocumentWithAcrAuth(requestedAcrDocType))
		{
			goodsItem.AddRowMessageError(ValidationCaptions.SupportingDocument.GetHasNoAcrCertificate(requestedAcrDocType));
		}
	}

	public void AddSupportingDocumentIfNeeded()
	{
		var acrAuthorisation = GetAcrAuthorisation();
		var docType = GetAcrDocType(acrAuthorisation);
		if (!docType.IsEmpty && !HasAnySupportingDocumentWithAcrAuth(docType))
		{
			var supportingDoc = goodsItem.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = docType;
			supportingDoc.CSI_ReferenceNumber = acrAuthorisation.CPH_Number;
		}
	}

	#region Implementation

	bool HasAnySupportingDocumentWithAcrAuth(ZString requestedAcrDocType) => goodsItem.SupportingDocuments.Cast<NctsSupportingDocument>().Any(x => x.CSI_Code == requestedAcrDocType);

	CusAuthorisationHeader GetAcrAuthorisation()
	{
		var currentConsignorAddress = !goodsItem.Consignor.IsEmpty ? goodsItem.Consignor : goodsItem.Header.Consignor;
		if (currentConsignorAddress.IsEmpty)
		{
			return null;
		}

		return CusAuthorisationHeader.Loader.GetAuthorisationsForAddresses(goodsItem.Factory,
			Core.Constants.CountryCodes.Italy,
			new ZString[] { Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit },
			ZDateTime.Today,
			new ZGuid[] { currentConsignorAddress.E2_OA_Address })
			.FirstOrDefault();
	}

	ZString GetAcrDocType(CusAuthorisationHeader acrAuthorisation)
	{
		return acrAuthorisation?.CusAuthorisationRules
			.Cast<CusAuthorisationRule>()
			.SingleOrDefault(x => x.CPR_RuleCode == ITCusAuthorisationRuleTypeList.Codes.Document)
			?.CPR_ValueFrom ?? ZString.Empty;
	}

	ZString GetAcrDocAuthorisation()
	{
		return GetAcrDocType(GetAcrAuthorisation());
	}

	#endregion
}
