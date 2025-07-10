using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderLookups : EU.Business.CusTempStorage.CusTempStorageJobHeaderLookups
	{
		public CusTempStorageJobHeaderLookups(AutoCusTempStorageJobHeader parent) : base(parent)
		{
		}

		public IBusinessObjectCollection GuaranteeList => Factory.GetCachedValue("Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeaderLookups|GuaranteeList",
			() => new CusGuaranteeHeaderCollection(Factory));

		#region CustomsProfileList
		public CodeDescriptionPairList CustomsProfileList => Factory.GetCachedValue(GetCustomsProfileCachedKey(nameof(CustomsProfileList)), () =>
		{
			var result = new CodeDescriptionPairList();
			CusAuthorisationHeaders.ToList().ForEach(x => result.AddPair(x.CPH_Number));
			return result;
		});

		internal CusAuthorisationHeader GetCusAuthorisationHeader(ZString permitNumber) => CusAuthorisationHeaders.Where(x => x.CPH_Number == permitNumber).FirstOrDefault();

		CusAuthorisationHeader[] CusAuthorisationHeaders => Factory.GetCachedValue(GetCustomsProfileCachedKey(nameof(CusAuthorisationHeaders)), () =>
		{
			var result = System.Array.Empty<CusAuthorisationHeader>();
			if ((Parent.IsLADT || Parent.IsIST) && Parent.SJH_OH_Customer.IsValid)
			{
				var appCode = Parent.SJH_AppCode;
				var headers = CusAuthorisationHeader.Loader.GetAuthorisations(Factory, Parent.CountryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, ZDateTime.Today, Parent.SJH_OH_Customer);
				if (headers != null)
				{
					result = headers.Where(y => y.CusAuthorisationRules.Any(rule => rule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.USE && rule.CPR_ValueFrom == appCode)).ToArray();
				}
			}
			return result;
		});

		string GetCustomsProfileCachedKey(string listName) => string.Format(CultureInfo.InvariantCulture, (NoResString)"Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeaderLookups|{0}|{1}|{2}|{3}"
			, listName, Parent.SJH_OH_Customer.ToStringKey(), Parent.IsLADT ? "Y" : "N", Parent.IsIST ? "Y" : "N");
		#endregion

		public override CodeDescriptionPairList PreviousReferenceTypeList => Factory.GetCachedValue<PreviousDocumentCodeList>();

		protected new CusTempStorageJobHeader Parent => base.Parent as CusTempStorageJobHeader;
	}
}
