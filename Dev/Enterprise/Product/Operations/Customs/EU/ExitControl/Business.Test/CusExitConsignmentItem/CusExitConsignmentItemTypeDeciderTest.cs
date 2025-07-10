using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitConsignmentItemTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override Type BaseTypeDecidedType => typeof(CusExitConsignmentItem);

		protected override BusinessObject GetNewBusinessObjectForLoadTest() => CusExitConsignmentItemTest.GetNewBusinessObject(Factory).consignmentItem;

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesAndExpectedTypes();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesAndExpectedTypes();
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitConsignmentItem>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitConsignmentItem>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitConsignmentItem>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitConsignmentItem>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitConsignmentItem>() },
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			((CusExitConsignmentItem)bizO).Consignment.Header.Company.GC_RN_NKCountryCode = countryCode;
		}

		Dictionary<string, Type> GetTestCountryCodesAndExpectedTypes()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitConsignmentItem>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitConsignmentItem>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitConsignmentItem>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitConsignmentItem>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitConsignmentItem>() },
			};
		}
	}
}
