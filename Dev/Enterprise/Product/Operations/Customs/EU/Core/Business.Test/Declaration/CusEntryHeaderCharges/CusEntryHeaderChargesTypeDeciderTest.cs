using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CusEntryHeaderChargesTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var cusEntryHeaderCharges = Factory.New<CusEntryHeaderCharges>();
			AssertEquals("Enterprise.Customs.EU.Business.Declaration.CusEntryHeaderCharges", cusEntryHeaderCharges.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("TR");
			cusEntryHeaderCharges = Factory.New<CusEntryHeaderCharges>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.TR.Business.Declaration.CusEntryHeaderCharges", cusEntryHeaderCharges.GetType().FullName);
		}

		#region Overrides

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cusEntryHeaderCharges = bizO as CusEntryHeaderCharges;
			if (cusEntryHeaderCharges != null)
			{
				cusEntryHeaderCharges.EntryHeader.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryHeaderCharges = entryHeader.Charges.AddNew();

			entryHeaderCharges.C1_ChargeType = "AAA";
			entryHeaderCharges.C1_ChargeAmount = 1000m;

			return entryHeaderCharges;
		}

		protected override Type BaseTypeDecidedType => typeof(CusEntryHeaderCharges);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryHeaderCharges>() },
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

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryHeaderCharges>() },
			};
		}

		#endregion
	}
}
