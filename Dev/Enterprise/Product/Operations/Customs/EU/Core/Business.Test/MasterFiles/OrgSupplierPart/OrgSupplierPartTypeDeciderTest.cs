using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using EUOrgSupplierPart = Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	sealed class OrgSupplierPartTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestLoadingCountrySpecificPartFromDifferentCountry_WI00324523()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var part = Factory.New<Customs.Business.OrgSupplierPart>();
				part.OP_PartNum = "DN324323";
				part.OP_Desc = "PART DESC";
				part.OP_StockKeepingUnit = "NO";
				var pivot = part.PivotsForBinding.AddNew();
				pivot.CI_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				pivot.CI_ChildType = "HTI";
				pivot.CI_TariffNum = "1020304050";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				part = newFactory.Load<Customs.Business.OrgSupplierPart>(part.PK);
				var auPart = (Customs.Business.OrgSupplierPart)newFactory.Load<Integration.Customs.AU.IOrgSupplierPart>(part.PK);
				AssertEquals(0, auPart.PivotsForBinding.Count);
				var auPivot = auPart.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.Australia, auPivot.CI_RN_NKCountry);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.AU.ICusClassPartPivot>(), auPivot.GetType());
				var usPart = (Customs.Business.OrgSupplierPart)newFactory.Load<Integration.Customs.US.IOrgSupplierPart>(part.PK);
				AssertEquals(0, usPart.PivotsForBinding.Count);
				var usPivot = usPart.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, usPivot.CI_RN_NKCountry);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.US.ICusClassPartPivot>(), usPivot.GetType());
				var itPart = (Customs.Business.OrgSupplierPart)newFactory.Load<Integration.Customs.IT.IOrgSupplierPart>(part.PK);
				AssertEquals(0, itPart.PivotsForBinding.Count);
				var itPivot = itPart.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.Italy, itPivot.CI_RN_NKCountry);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.IT.ICusClassPartPivot>(), itPivot.GetType());
			}
		}

		public void TestDeciderProxiesToCustomsCountryPart()
		{
			foreach (var type in new OrgSupplierPartTypeDecider().CountrySpecificTypes)
			{
				AssertTypeForNew(type.CountryCode, type.BusinessObjectType);
			}
			AssertTypeForNew(Core.Constants.CountryCodes.Australia, typeof(EUOrgSupplierPart));

			var auType = ObjectFactory.GetType<Integration.Customs.AU.IOrgSupplierPart>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertType(auType, Factory.New(auType));
				AssertType(auType, Factory.New(typeof(Customs.Business.OrgSupplierPart)));
			}
		}

		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var orgSupplierPart = Factory.New<EUOrgSupplierPart>();
			AssertEquals("Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart", orgSupplierPart.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("DE");
			orgSupplierPart = Factory.New<EUOrgSupplierPart>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.DE.Business.OrgSupplierPart", orgSupplierPart.GetType().FullName);
		}

		protected override Type BaseTypeDecidedType => typeof(EUOrgSupplierPart);

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var orgSupplierPart = Factory.New<EUOrgSupplierPart>();
			orgSupplierPart.OP_PartNum = orgSupplierPart.PK.ToString().Replace("-", "");
			return orgSupplierPart;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Sweden, ObjectFactory.GetType<Integration.Customs.SE.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Denmark, ObjectFactory.GetType<Integration.Customs.DK.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Finland, ObjectFactory.GetType<Integration.Customs.FI.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes._EUTemplateCountryName_, ObjectFactory.GetType<Integration.Customs._EUCustomsTemplate_.IOrgSupplierPart>() },
			};
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Sweden, ObjectFactory.GetType<Integration.Customs.SE.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Denmark, ObjectFactory.GetType<Integration.Customs.DK.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Finland, ObjectFactory.GetType<Integration.Customs.FI.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids._EUTemplateCountryName_, ObjectFactory.GetType<Integration.Customs._EUCustomsTemplate_.IOrgSupplierPart>() },
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			GlbCompany.CurrentCompany.SetCountry(countryCode);
		}

		void AssertTypeForNew(string country, Type expectedType)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				AssertType(expectedType, Factory.New(expectedType));
			}
		}
	}
}
