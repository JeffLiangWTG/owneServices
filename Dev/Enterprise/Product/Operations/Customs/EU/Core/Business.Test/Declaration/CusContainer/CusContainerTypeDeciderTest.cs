using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CusContainerTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var cusContainer = Factory.New<CusContainer>();
			AssertEquals("Enterprise.Customs.EU.Business.Declaration.CusContainer", cusContainer.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("DE");
			cusContainer = Factory.New<CusContainer>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.DE.Business.Declaration.CusContainer", cusContainer.GetType().FullName);
		}

		protected override Type BaseTypeDecidedType => typeof(CusContainer);

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var dec = Factory.New<JobDeclaration>();
			return dec.CusContainers.AddNew();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusContainer>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusContainer>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusContainer>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.EU.ICusContainer>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusContainer>() },
				{ Core.Constants.CountryGuids.Latvia, typeof(CusContainer) }
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cusContainer = bizO as CusContainer;
			if (cusContainer != null)
			{
				cusContainer.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusContainer>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusContainer>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusContainer>() },
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.EU.ICusContainer>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusContainer>() },
				{ Core.Constants.CountryCodes.Latvia, typeof(CusContainer) }
			};
		}
	}
}
