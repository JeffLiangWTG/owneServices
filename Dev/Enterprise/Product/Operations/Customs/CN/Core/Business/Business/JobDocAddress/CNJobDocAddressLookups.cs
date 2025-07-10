using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CNJobDocAddressLookups : JobDocAddressLookups
	{
		public CNJobDocAddressLookups(CNJobDocAddress parent) : base(parent) { }

		public override CodeDescriptionPairList GovRegNumTypes => Factory.GetCachedValue("Enterprise.Customs.CN.Business.CNJobDocAddressLookups.GovRegNumTypes", GetNewGovRegNumTypes);

		CodeDescriptionPairList GetNewGovRegNumTypes()
		{
			var result = new CodeDescriptionPairList();

			var cnRefCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.China);
			var customsCodeList = new OrgCodeLists().CustomsCodes_List(cnRefCountry);

			foreach (var code in CNJobDocAddress.AllRegNumTypes)
			{
				result.AddPair(code, customsCodeList.GetDescriptionFromCode(code));
			}

			return result;
		}

		public CodeDescriptionPairList OverseasPartyCodes
		{
			get
			{
				if (fOverseasPartyCodes == null)
				{
					fOverseasPartyCodes = GovRegNumTypes.FilterListByCodes(CNJobDocAddress.OverseasPartyTypes);
				}
				return fOverseasPartyCodes;
			}
		}
		CodeDescriptionPairList fOverseasPartyCodes;
	}
}
