using System.Linq;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.CN.Business.Constants.UniversalReferenceConstants;

namespace Enterprise.Customs.CN.Business
{
	public class CusSupportingDocumentLookups : Customs.Business.CusSupportingInfoLookups
	{
		public CusSupportingDocumentLookups(CusSupportingDocument parent) : base(parent)
		{
		}

		public new CusSupportingDocument Parent => (CusSupportingDocument)base.Parent;

		public CodeDescriptionPairList SupportingDocumentsList
		{
			get
			{
				var shipmentType = (Parent.Parent?.Declaration?.IsImport ?? false) ? Universal.RefCusCodeListAttributeTypes.Codes.Import : Universal.RefCusCodeListAttributeTypes.Codes.Export;
				return Factory.GetCachedValue(System.FormattableString.Invariant($"CN_SupportingDocument_UserReadableList_{shipmentType}"), () =>
				{
					var result = new CodeDescriptionPairList();
					var allSupportingDocumentTypes = CNRefCusCodeListTypes.GetCusSupportingDocumentList(Factory, Parent.EffectiveDate);
					allSupportingDocumentTypes.Load();
					foreach (var docType in allSupportingDocumentTypes.OfType<ZZRefCusCodeListCombined>().Where(x => x.HasAttribute(shipmentType) && x.ZZD_Code != Constants.DocumentCodes.CertificateOfOrigin).OrderBy<ZZRefCusCodeListCombined, string>(x => x.GetAttribute(CusCodeListAttributeName.DisplayCode), System.StringComparer.Ordinal)
					)
					{
						result.AddPairIfNotExist(docType.ZZD_Code, System.FormattableString.Invariant($"{docType.GetAttribute(CusCodeListAttributeName.DisplayCode)}.{docType.ZZD_Description}"));
					}
					return result;
				});
			}
		}
	}
}
