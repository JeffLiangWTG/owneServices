using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Messaging.Wrappers;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class GbCDSExportEntryHeaderWrapperTests : TestCaseWithFactory
	{
		public void TestICDSRequestMessageDataProvider()
		{
			var entry = CreateSampleEntryHeader(Factory);
			var provider = new GbCDSExportEntryHeaderWrapper(entry);
			CombineAssertions(() =>
			{
				AssertEquals("MUCR", provider.MasterUniqueConsignmentReference);
				AssertEquals("UCRREFERENCEPLACEHOLDER3A3C3059CEBF44D08B4E1DE9048AA854", provider.DeclarationUniqueConsignmentReference);
				AssertEquals("GBAUQFLOCATION", ((IUkCinvWrapper)provider).LocationOfGoods);
				AssertEquals(new ZDateTime(2018, 6, 6), provider.DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises);
				AssertEquals(new ZDateTime(2018, 6, 7), provider.DateAndTimeTheGoodsWillBeLeavingLCPPremises);
				AssertEquals("06Jun0000", provider.MovementReference);
				AssertEquals("SUB", provider.ShedCode);
				AssertEquals("", provider.MasterOpt);
				AssertEquals("Flight", provider.TransportIdentityAtTheBorderBox21);
				AssertEquals("4", provider.TransportModeAtTheBorderBox25);
				AssertEquals("GB", provider.TransportNationalityAtTheBorderBox21);
			});
		}

		public static CusEntryHeader CreateSampleEntryHeader(BusinessObjectFactory factory)
		{
			var dec = factory.New<Business.Declaration.JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_DeclarationReference = "DEC123";
			dec.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");
			dec.JE_CustomsProfile = "ABC";

			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var abc = badgeCodeSettings.AddNew();
			abc.BadgeCode = "ABC";
			abc.RL_PortCode = "GBLBA";
			abc.Direction = "IMP";
			abc.CSPCode = "CCSUK";
			abc.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk;
			abc.ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
			var credential = new CredentialsSetting
			{
				BadgeCode = "ABC",
				Printer = "Location",
				Company = "Role"
			};
			var credentials = GBCustomsDataRegistry.Instance.Credentials.GetValueWithoutFallback(dec.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
			credentials.Add(credential);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(dec.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, credentials);

			dec.JE_Calc_LocationOtherInformationCountry = "GB";
			dec.JE_Calc_LocationOtherInformationType = "AU";
			dec.JE_LocationQualifier = "QF";
			dec.JE_GoodsLocation = "LOCATION";
			dec.ZG_LCPDepart = new ZDateTime(2018, 6, 7);
			dec.ZG_LCPInspect = new ZDateTime(2018, 6, 6);
			dec.JE_SubLocationOfGoods = "GBSUB";

			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_VoyageFlightNo = "Flight";
			dec.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.UnitedKingdom;
			var cei = dec.CustomsEntryInstructions.AddNew();
			dec.JE_ExportDate = new ZDateTime(2018, 8, 8);

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MasterUCR = "MUCR";
			entryHeader.CH_CEI_Instruction = cei.PK;
			var ucrnumber = factory.New<CusEntryNumber>();
			ucrnumber.CE_ParentID = entryHeader.PK;
			ucrnumber.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			ucrnumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			ucrnumber.CE_EntryNum = "DUCR/1";

			return entryHeader;
		}

		public static CusEntryHeader CreateSampleEntryHeaderWithBGMReference(BusinessObjectFactory factory, ZString bgmReference)
		{
			var declarationMock = factory.NewMoq<Business.Declaration.JobDeclaration>();
			var dec = declarationMock.Object;
			var cei = dec.CustomsEntryInstructions.AddNew();
			dec.JE_ExportDate = new ZDateTime(2023, 4, 11);

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MasterUCR = "MUCR";
			entryHeader.CH_BGMReference = bgmReference;
			entryHeader.CH_CEI_Instruction = cei.PK;
			var ucrnumber = factory.New<CusEntryNumber>();
			ucrnumber.CE_ParentID = entryHeader.PK;
			ucrnumber.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			ucrnumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			ucrnumber.CE_EntryNum = "DUCR/1";

			return entryHeader;
		}
	}
}
