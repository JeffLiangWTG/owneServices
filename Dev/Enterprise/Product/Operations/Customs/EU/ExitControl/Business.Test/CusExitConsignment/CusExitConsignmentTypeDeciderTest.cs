using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitConsignmentTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitConsignment>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitConsignment>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitConsignment>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitConsignment>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitConsignment>() },
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesAndExpectedTypes();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesAndExpectedTypes();
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			(bizO as CusExitConsignment).Header.Company.GC_RN_NKCountryCode = countryCode;
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
			var consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_Status = "REJ";
			return consignment;
		}

		protected override Type BaseTypeDecidedType => typeof(CusExitConsignment);

		Dictionary<string, Type> GetTestCountryCodesAndExpectedTypes()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitConsignment>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitConsignment>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitConsignment>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitConsignment>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitConsignment>() },
			};
		}
	}
}
