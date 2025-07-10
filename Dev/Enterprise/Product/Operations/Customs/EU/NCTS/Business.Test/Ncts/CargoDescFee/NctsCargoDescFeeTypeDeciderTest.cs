using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public class NctsCargoDescFeeTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override Type EUType => typeof(NctsCargoDescFee);

		protected override Type BaseTypeDecidedType => typeof(NctsCargoDescFee);

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "C#@";
			company.GC_Name = "COMP TEST";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B#@";
			branch.GB_BranchName = "BRANCH TEST";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_GB = branch.PK;
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			return goodsItem.Fees.AddNew();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests() => new Dictionary<string, Type>
		{
			{ Enterprise.Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.INctsCargoDescFee>() },
			{ Enterprise.Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.INctsCargoDescFee>() },
			{ Enterprise.Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.INctsCargoDescFee>() }
		};

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Enterprise.Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.INctsCargoDescFee>() },
				{ Enterprise.Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.INctsCargoDescFee>() },
				{ Enterprise.Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.INctsCargoDescFee>() }
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			((NctsCargoDescFee)bizO).Parent.Header.Branch.Company.GC_RN_NKCountryCode = countryCode;
		}
	}
}
