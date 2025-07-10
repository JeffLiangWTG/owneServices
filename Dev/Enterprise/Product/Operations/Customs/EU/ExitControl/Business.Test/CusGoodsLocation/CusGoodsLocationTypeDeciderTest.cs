using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

sealed class CusGoodsLocationTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
{
	protected override Type BaseTypeDecidedType => typeof(CusGoodsLocation);

	protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusGoodsLocation>();

	protected override BusinessObject GetNewBusinessObjectForLoadTest()
	{
		var header = Factory.GetUcc6ExitHeader();
		var consignment = header.CusExitConsignments.AddNew();
		var report = header.CusExitReports.AddNew();
		report.CER_CXC_Consignment = consignment.PK;

		return report.GoodsLocation;
	}

	protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

	protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

	protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
	{
		var countryTypes = new Dictionary<ZGuid, Type>
		{
			{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusGoodsLocation>() },
		};

		return countryTypes;
	}

	protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
	{
		var report = (CusExitReport)((CusGoodsLocation)bizO).Parent;
		report.Header.Branch.Company.GC_RN_NKCountryCode = countryCode;
	}

	static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
	{
		var countryTypes = new Dictionary<string, Type>
		{
			{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusGoodsLocation>() },
		};

		return countryTypes;
	}
}
