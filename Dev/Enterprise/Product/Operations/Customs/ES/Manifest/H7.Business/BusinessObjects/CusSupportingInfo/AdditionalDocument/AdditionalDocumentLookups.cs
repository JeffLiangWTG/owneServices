using System.Collections;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AdditionalDocumentLookups : EU.H7.Business.AdditionalDocumentLookups
	{
		public AdditionalDocumentLookups(AdditionalDocument parent)
			: base(parent)
		{
		}

		public override ICollection CodeList => Factory.GetCachedValue("Enterprise.Customs.ES.Manifest.H7.Business.AdditionalDocumentLookups", () =>
		{
			var result = new CodeDescriptionPairList();

			var codeType = Parent.CSI_SubType.ToString() switch
			{
				AdditionalInfoSubTypeList.Codes.TransportDocument => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ESG3TransportDocumentCode,
				_ => string.Empty
			};

			if (!codeType.IsNullOrEmpty() && !Parent.DataGrouping.IsEmpty)
			{
				var collection = ZZRefCusCodeListCombined.Loader.Load(Parent.Factory, CountryCodes.Spain, codeType, ZDateTime.Today);
				collection.ForEach(x => result.AddPairIfNotExist(x.ZZD_Code, x.ZZD_Description));
			}

			return result;
		});
	}
}
