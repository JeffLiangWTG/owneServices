using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using ECB = Enterprise.Customs.Business;

namespace Enterprise.Client.TIP.Testing
{
	public class ProductWrapperTest : TestCaseWithFactory
	{
		public void TestGetProductLine()
		{
			var notify = new NotificationBuffer();
			CMRTariffRatePeriodSnapshot tariffRate = Factory.New<CMRTariffRatePeriodSnapshot>();
			tariffRate.TT_RateNumber = "001";
			tariffRate.TT_PreferenceSchemeType = "GEN";
			tariffRate.TT_TariffClassificationNumber = "22030031";
			tariffRate.TT_StartDate = ZDateTime.Now.AddDays(-50);
			tariffRate.TT_CalculationType = Constants.DutyCalcTypes.Calc;
			tariffRate.TT_CustomsValueRate = 1m;
			CMRTreatmentRatePeriodSnapshot treatment = Factory.New<CMRTreatmentRatePeriodSnapshot>();
			treatment.TP_Code = "DTR";
			treatment.TP_RateNumber = "001";
			treatment.TP_PreferenceSchemeType = "GEN";
			treatment.TP_StartDate = ZDateTime.Now.AddDays(-50);
			treatment.TP_CalculationType = Constants.DutyCalcTypes.Calc;
			treatment.TP_CustomsValueRate = 0.5m;
			OrgHeader owner = Factory.New<OrgHeader>();
			owner.OH_Code = "PARTOWNER";
			AUOrgSupplierPart part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = "TESTPART";
			OrgPartRelation orgPart = part.RelatedOrganisations.AddNew();
			orgPart.OU_OH = owner.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AUCClass importTariff = Factory.New<AUCClass>();
			importTariff.UJ_Code = "2203.00.31 15";
			importTariff.UJ_UQ1 = "LA";
			importTariff.UJ_UQ2 = "L";
			Classification importClass = Factory.New<Classification>();
			importClass.CC_ClassificationType = Classification.ClassificationType.IMP;
			importClass.CC_TariffNum = importTariff.UJ_Code;
			importClass.CC_LookupCode = "I9999988888";
			importClass.AddInfo.ZA_ORG = Core.Constants.CountryCodes.Germany;
			importClass.AddInfo.ZA_PRF = "E";
			importClass.AddInfo.ZA_TreatmentCode_Hidden = "DTR";
			importClass.AddInfo.ZA_InstrumentType_Hidden = "DN";
			importClass.AddInfo.ZA_InstrumentCode_Hidden = "111111";
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ECB.ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = importClass.PK;
			ProductWrapper wrapper = new ProductWrapper();
			AssertEquals("Import line ", new ZString("TESTPART,I9999988888,2203.00.31 15,DE,E,DTR,DN,111111,0.5"), wrapper.GetProductLine(part, false, owner.PK, notify));
			AssertEquals("Export line", ZString.Empty, wrapper.GetProductLine(part, true, owner.PK, notify));
			importPivot.AddInfo.ZA_ORG = Core.Constants.CountryCodes.NewZealand;
			importPivot.AddInfo.ZA_PRF = "P";
			importPivot.AddInfo.ZA_TreatmentCode_Hidden = "TRT";
			importPivot.AddInfo.ZA_InstrumentType_Hidden = "BL";
			importPivot.AddInfo.ZA_InstrumentCode_Hidden = "001452";
			notify.Clear();
			AssertEquals("Import line ", new ZString("TESTPART,I9999988888,2203.00.31 15,NZ,P,TRT,BL,001452,0"), wrapper.GetProductLine(part, false, owner.PK, notify));
			Classification exportClass = Factory.New<Classification>();
			exportClass.CC_ClassificationType = Classification.ClassificationType.EXP;
			exportClass.CC_TariffNum = "11111111";
			exportClass.CC_LookupCode = "I0000000000";
			var exportPivot = part.PivotsForBinding.AddNew();
			exportPivot.CI_ChildType = ECB.ClassificationTypeList.Codes.HTE;
			exportPivot.CI_CC = exportClass.PK;
			AssertEquals("Export line", new ZString("TESTPART,I0000000000,1111.11.11,,,,,,"), wrapper.GetProductLine(part, true, owner.PK, notify));
			part.PivotsForBinding.Remove(exportPivot);
			AssertEquals("Export line", ZString.Empty, wrapper.GetProductLine(part, true, owner.PK, notify));
			AssertContains($"Cannot Export Product '{part.OP_PartNum}'- Classification of type 'HTE' is not found.", notify.AsString);
		}
	}
}
