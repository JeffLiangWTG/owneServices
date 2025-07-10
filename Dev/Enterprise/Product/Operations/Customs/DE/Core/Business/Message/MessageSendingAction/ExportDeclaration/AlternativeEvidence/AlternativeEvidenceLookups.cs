using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants.RefCusCodeList.Codes;

namespace Enterprise.Customs.DE.Business
{
	public class AlternativeEvidenceLookups(AlternativeEvidence parent) : JobDeclarationAlternativeEvidenceLookups(parent)
	{
		public override CodeDescriptionPairList AlternativeEvidenceTypeList => Factory.GetCachedValue<AlternativeEvidenceTypeList>();

		public override ICollection TransportDocumentTypeList
		{
			get
			{
				return Factory.GetCachedValue("857E0A56-44B8-45B4-BE68-301B1291321F", () =>
				{
					var result = new CodeDescriptionPairList();
					var list = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, ZDateTime.Today);
					result.AddRange(list);
					result.RemoveCode(Code_9ZZX);
					result.RemoveCode(Code_9ZZY);
					result.Sort();
					return result;
				});
			}
		}
	}
}
