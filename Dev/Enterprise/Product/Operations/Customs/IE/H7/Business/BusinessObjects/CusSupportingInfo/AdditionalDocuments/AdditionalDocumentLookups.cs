using System.Collections;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AdditionalDocumentLookups : EU.H7.Business.AdditionalDocumentLookups
	{
		public AdditionalDocumentLookups(AdditionalDocument parent)
			: base(parent)
		{
		}

		public override ICollection CodeList => GetCodeList();

		ICollection GetCodeList()
		{
			ICollection result;

			var codeType = Parent.CSI_SubType.ToString() switch
			{
				AdditionalInfoSubTypeList.Codes.AdditionalInformation => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportAddDocAdditionalInformation,
				AdditionalInfoSubTypeList.Codes.AdditionalReference => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection,
				AdditionalInfoSubTypeList.Codes.TransportDocument => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportAddDocTransportContract,
				_ => string.Empty
			};

			if (!codeType.IsNullOrEmpty() && !Parent.DataGrouping.IsEmpty)
			{
				result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Parent.Factory, Parent.DataGrouping, codeType, ZDateTime.Today);
			}
			else
			{
				result = new CodeDescriptionPairList();
			}

			return result;
		}
	}
}
