using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	public class VanningAddressLookups : JobDocAddressLookups
	{
		public VanningAddressLookups(AutoJobDocAddress parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList GovRegNumTypes
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.JP.Business.VanningAddressLookups.GovRegNumTypes", () =>
				{
					var result = new CodeDescriptionPairList();
					var orgCodes = CustomsCodesList.GetJPCustomsCodesList(Factory);

					result.AddPair(OrgCusCode.CodeTypes.ControlledPremisesID, orgCodes.GetDescriptionFromCode(OrgCusCode.CodeTypes.ControlledPremisesID));
					result.AddPair(OrgCusCode.JapanCodeTypes.LPC, orgCodes.GetDescriptionFromCode(OrgCusCode.JapanCodeTypes.LPC));
					result.AddPair(OrgCusCode.JapanCodeTypes.CIE, orgCodes.GetDescriptionFromCode(OrgCusCode.JapanCodeTypes.CIE));

					return result;
				});
			}
		}

		public new CodeDescriptionPairList State_List => Factory.GetStateList(Parent.E2_RN_NKCountryCode, Parent.E2_RN_NKCountryCodeInfo.HasErrors());

		public IBusinessObjectCollection JPCustomsControlledPremisesCodeList => JPRefCusCodeListTypes.GetJapanBondedAreaCodes(Factory);

		public RefCountryCollection CountryList
		{
			get
			{
				if (countryList == null)
				{
					countryList = new RefCountryCollection(Factory);
					countryList.ApplySort(RefCountrySchema.RN_Desc.Name, ListSortDirection.Ascending);
				}
				return countryList;
			}
		}
		RefCountryCollection countryList;
	}
}
