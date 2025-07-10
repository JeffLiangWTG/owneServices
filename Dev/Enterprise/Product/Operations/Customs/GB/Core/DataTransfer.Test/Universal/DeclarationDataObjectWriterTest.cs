using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	class DeclarationDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestCDS()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSettingCDS = badgeCodeSettings.AddNew();
			badgeCodeSettingCDS.BadgeCode = "ABC";
			badgeCodeSettingCDS.RL_PortCode = "GBLBA";
			badgeCodeSettingCDS.Direction = "IMP";
			badgeCodeSettingCDS.CSPCode = "CCSUK";
			badgeCodeSettingCDS.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk;
			badgeCodeSettingCDS.ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_CustomsProfile = "ABC";
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

				var credentials = new CredentialsSettingCollection();
				var credentialsSetting = credentials.AddNew();
				credentialsSetting.BadgeCode = "ABC";
				credentialsSetting.Username = "VWG";
				credentialsSetting.Password = "123";
				credentialsSetting.Printer = "Printer";

				using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, credentials))
				{
					var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
					var declarationData = writer.GetDataObject(declaration);

					var addinfos = declarationData.AddInfoCollection;
					AssertAddInfo(addinfos, Constants.AddInfo.Keys.GatewayKey, "VWG");
					AssertAddInfo(addinfos, Constants.AddInfo.Keys.GatewayValue, "123");
					AssertAddInfo(addinfos, Constants.AddInfo.Keys.GatewayOutputDevice, "Printer");
				}
			}
		}

		public void TestExportLocationOfGood()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_LocationOfGoods = "BFS";
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertEquals("BFS", declarationData.LocationAtClearance.Code);
		}

		public void TestAdditionalInfoOnEntryInstructions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();
			var cusSupportingInfo = Factory.New<AdditionalInfo>();
			cusSupportingInfo.CSI_ReferenceNumber = "REF1";
			cusSupportingInfo.CSI_Value = 999;
			cusSupportingInfo.CSI_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
			cusSupportingInfo.CSI_ParentID = entryInstruction.PK;
			entryInstruction?.AdditionalInfos.Add(cusSupportingInfo);

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertEquals("REF1", declarationData.EntryInstructionCollection[0].CustomsSupportingInformationCollection[0].ReferenceNumber);
			AssertEquals(999m, declarationData.EntryInstructionCollection[0].CustomsSupportingInformationCollection[0].Value);
		}

		public void TestExportLocationOfGoodForCDS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_Calc_LocationOtherInformationCountry = "GB";
			declaration.JE_Calc_LocationOtherInformationType = "BU";
			declaration.JE_LocationQualifier = "CW";
			declaration.JE_LocationOfGoods = "NOTFORCDS";
			declaration.JE_GoodsLocation = "TESTPLACE0";
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertEquals("GBBUCWTESTPLACE0", declarationData.LocationAtClearance.Code);

			declaration.JE_Calc_LocationOtherInformationCountry = ZString.Empty;
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			declarationData = writer.GetDataObject(declaration);
			AssertEquals("  BUCWTESTPLACE0", declarationData.LocationAtClearance.Code);

			declaration.JE_Calc_LocationOtherInformationType = ZString.Empty;
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			declarationData = writer.GetDataObject(declaration);
			AssertEquals("    CWTESTPLACE0", declarationData.LocationAtClearance.Code);

			declaration.JE_LocationQualifier = ZString.Empty;
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			declarationData = writer.GetDataObject(declaration);
			AssertEquals("      TESTPLACE0", declarationData.LocationAtClearance.Code);

			declaration.JE_Calc_LocationOtherInformationCountry = "GB";
			declaration.JE_Calc_LocationOtherInformationType = "BU";
			declaration.JE_LocationQualifier = "CW";
			declaration.JE_GoodsLocation = ZString.Empty;
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			declarationData = writer.GetDataObject(declaration);
			AssertEquals("GBBUCW", declarationData.LocationAtClearance.Code);
		}

		public void TestGetUniversalDataObjectWriterHelper()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var writer = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var helper = writer.GetUniversalDataObjectWriterHelperExposed(declaration.Factory, declaration.CountryCode);
			Assert(helper is UniversalDataObjectWriterHelper);
		}

		static void AssertAddInfo(IEnumerable<AddInfo> addInfos, ZString key, ZString value)
		{
			var addinfo = addInfos.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
			AssertNotNull(addinfo);
			AssertEquals(value, addinfo.Value);
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}

		class DeclarationDataObjectWriterForTest : DeclarationDataObjectWriter
		{
			public DeclarationDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
			{
			}

			public EU.DataTransfer.Universal.UniversalDataObjectWriterHelper GetUniversalDataObjectWriterHelperExposed(BusinessObjectFactory factory, ZString countryCode) => GetUniversalDataObjectWriterHelper(factory, countryCode);
		}
	}
}
