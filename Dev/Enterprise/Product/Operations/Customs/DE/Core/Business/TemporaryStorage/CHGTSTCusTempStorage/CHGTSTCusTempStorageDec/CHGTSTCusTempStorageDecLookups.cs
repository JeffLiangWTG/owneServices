using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGTSTCusTempStorageDecLookups : CusTempStorageDecLookups
	{
		public CHGTSTCusTempStorageDecLookups(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		public new CHGTSTCusTempStorageDec Parent => (CHGTSTCusTempStorageDec)base.Parent;

		public override CodeDescriptionPairList IdentificationIndicatorList => Parent.GetIdentificationIndicatorListExcludingSIN();

		public CodeDescriptionPairList NewCustodianBranchList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var orgHeader = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;
				if (orgHeader != null)
				{
					var list = orgHeader.CustomsCodes.GetOrgCusCodesForCodeAndCountry(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany);
					list.ForEach(x => result.AddPair(x.OK_CustomsRegNo.Left(CHGTSTCusTempStorageDec.Schema.NewCustodianBranchMaxlength), x.PremisesAddress?.AddressCode ?? ZString.Empty));
				}
				return result;
			}
		}
	}
}
