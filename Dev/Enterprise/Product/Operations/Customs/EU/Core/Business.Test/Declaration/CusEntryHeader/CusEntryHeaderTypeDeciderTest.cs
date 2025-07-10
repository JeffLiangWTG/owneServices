using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CusEntryHeaderTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestTypeDecider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec = Factory.New<JobDeclaration>();
				AssertEquals("GB", dec.CountryCode);
				var gbEntry = dec.CustomsEntryHeaders.AddNew();
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				AssertNoExceptionThrown("The reload as GB.CEH of an object previously loaded and cached as EU.CEH did not explode", delegate
				{
					newFactory.Load<CusEntryHeader>(gbEntry.PK);
					newFactory.Load<Integration.Customs.GB.ICusEntryHeader>(gbEntry.PK);
				});
			}
		}

		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("Enterprise.Customs.EU.Business.Declaration.CusEntryHeader", cusEntryHeader.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("IT");
			cusEntryHeader = Factory.New<CusEntryHeader>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.IT.Business.Declaration.CusEntryHeader", cusEntryHeader.GetType().FullName);
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cusEntryHeader = bizO as CusEntryHeader;
			if (cusEntryHeader != null)
			{
				cusEntryHeader.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var dec = Factory.New<JobDeclaration>();
			return dec.CustomsEntryHeaders.AddNew();
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryHeader>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryHeader>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusEntryHeader>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryHeader>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusEntryHeader>() },
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusEntryHeader>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryHeader>() }
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryHeader>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryHeader>() }
			};
		}

		protected override Type BaseTypeDecidedType => typeof(CusEntryHeader);
	}
}
