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
	sealed class CusEntryInstructionTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			AssertEquals("Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction", cusEntryInstruction.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("DE");
			cusEntryInstruction = Factory.New<CusEntryInstruction>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction", cusEntryInstruction.GetType().FullName);
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryInstruction>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryInstruction>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryInstruction>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusEntryInstruction>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryInstruction>() },
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusEntryInstruction>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusEntryInstruction>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryInstruction>() }
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cei = bizO as CusEntryInstruction;
			if (cei != null)
			{
				cei.JobDeclaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryInstruction>() },
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryInstruction>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryInstruction>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryInstruction>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusEntryInstruction>() },
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusEntryInstruction>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusEntryInstruction>() },
				{ Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryInstruction>() }
			};
		}

		protected override Type BaseTypeDecidedType
		{
			get { return typeof(CusEntryInstruction); }
		}
		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			var cei = entryHeader.EntryInstruction;
			return cei;
		}
	}
}
