using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	sealed class CusExitConsignmentPivotTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override Type BaseTypeDecidedType => typeof(CusExitConsignmentPivot);

		protected override BusinessObject GetNewBusinessObjectForLoadTest() => CusExitConsignmentPivotTest.GetNewBusinessObject(Factory).pivot;

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesAndExpectedTypes();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesAndExpectedTypes();

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitConsignmentPivot>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitConsignmentPivot>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitConsignmentPivot>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitConsignmentPivot>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitConsignmentPivot>() },
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			((CusExitConsignmentPivot)bizO).ConsignmentItem.Consignment.Header.Company.GC_RN_NKCountryCode = countryCode;
		}

		Dictionary<string, Type> GetTestCountryCodesAndExpectedTypes()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitConsignmentPivot>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitConsignmentPivot>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitConsignmentPivot>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitConsignmentPivot>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitConsignmentPivot>() },
			};
		}
	}
}
