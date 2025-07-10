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
	sealed class CusEntrLineTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestTypeDecider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec = Factory.New<JobDeclaration>();
				AssertEquals("GB", dec.CountryCode);
				var entry = dec.CustomsEntryHeaders.AddNew();
				var entryLine = entry.MergedLines.AddNew();
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				AssertNoExceptionThrown("The reload as GB.CEL of an object previously loaded and cached as EU.CEL did not explode", delegate
				{
					newFactory.Load<CusEntryLine>(entryLine.PK);
					newFactory.Load<Integration.Customs.GB.ICusEntryLine>(entryLine.PK);
				});
			}
		}

		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var cusEntryLine = Factory.New<CusEntryLine>();
			AssertEquals("Enterprise.Customs.EU.Business.Declaration.CusEntryLine", cusEntryLine.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("DE");
			cusEntryLine = Factory.New<CusEntryLine>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.DE.Business.Declaration.CusEntryLine", cusEntryLine.GetType().FullName);
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cusEntryLine = bizO as CusEntryLine;
			if (cusEntryLine != null)
			{
				cusEntryLine.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			return entry.MergedLines.AddNew();
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryLine>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusEntryLine>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryLine>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusEntryLine>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryLine>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryLine>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryLine>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryLine>() },
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
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryLine>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryLine>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryLine>() }
			};
		}

		protected override Type BaseTypeDecidedType => typeof(CusEntryLine);
	}
}
