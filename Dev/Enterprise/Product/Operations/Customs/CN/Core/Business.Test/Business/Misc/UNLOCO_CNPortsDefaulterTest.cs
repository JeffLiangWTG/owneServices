using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business.Testing
{
	class UNLOCOCNPortsDefaulterTest : TestCaseWithFactory
	{
		public void TestDefaultPort()
		{
			defaulter.DefaultPort();
			AssertEquals(ZString.Empty, declaration.JE_CNPortOfOrigin);
			declaration.JE_RL_NKPortOfLoading = "XXYYY";
			defaulter.DefaultPort();
			AssertEquals("ZZZ000", declaration.JE_CNPortOfOrigin);
			declaration.JE_RL_NKPortOfLoading = "XXX01";
			defaulter.DefaultPort();
			AssertEquals(new ZString("XXX0A"), declaration.JE_CNPortOfOrigin);
			declaration.JE_RL_NKPortOfLoading = "SYXX";
			defaulter.DefaultPort();
			AssertEquals(new ZString("SYR000"), declaration.JE_CNPortOfOrigin);
			declaration.JE_RL_NKPortOfLoading = "SXXX";
			defaulter.DefaultPort();
			AssertEquals(new ZString("SYR000"), declaration.JE_CNPortOfOrigin);
		}

		public void TestDefaultUNLOCO()
		{
			defaulter.DefaultUNLOCO();
			AssertEquals(ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			declaration.JE_CNPortOfOrigin = "XXX0X";
			defaulter.DefaultUNLOCO();
			AssertEquals(ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			declaration.JE_CNPortOfOrigin = "XXX0A";
			defaulter.DefaultUNLOCO();
			AssertEquals("XXX01", declaration.JE_RL_NKPortOfLoading);
			declaration.JE_CNPortOfOrigin = "XXX0B";
			defaulter.DefaultUNLOCO();
			AssertEquals("XXX01", declaration.JE_RL_NKPortOfLoading);
		}

		public void TestRoughChinaPort()
		{
			var query = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, "CHN341");
			query.AddToFilter(new ZQuery(RefLocoMapSchema.RY_RL_NKLocoPort, "CNSHA"));
			query.AddToFilter(new ZQuery(RefLocoMapSchema.RY_SystemUsage, "CUS"));
			query.AddToFilter(new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.China));
			var locoMap = Factory.LoadTop1<RefLocoMap>(query);
			if (locoMap == null)
			{
				locoMap = Factory.New<RefLocoMap>();
				locoMap.RY_LocalPortCode = "CHN341";
				locoMap.RY_RL_NKLocoPort = "CNSHA";
				locoMap.RY_SystemUsage = "CUS";
				locoMap.RY_RN = Core.Constants.CountryGuids.China;
				Factory.Save();
			}

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfLoading = "CNSHA";
			AssertEquals("Precondition：JE_LastPortBeforeEntry", "CNSHA", declaration.JE_LastPortBeforeEntry);
			var defaulter = new UNLOCO_CNPortsDefaulter(Factory, declaration.JE_CNLastPortBeforeEntryInfo, declaration.JE_LastPortBeforeEntryInfo, true);
			defaulter.DefaultPort();
			AssertEquals("JE_CNLastPortBeforeEntry", "CHN000", declaration.JE_CNLastPortBeforeEntry);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var locoMapX = Factory.New<RefLocoMap>();
			locoMapX.RY_LocalPortCode = "XXX0X";
			locoMapX.RY_RL_NKLocoPort = "XXX01";
			locoMapX.RY_SystemUsage = "CUX";
			locoMapX.RY_RN = Core.Constants.CountryGuids.China;
			var locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_LocalPortCode = "XXX0A";
			locoMap.RY_RL_NKLocoPort = "XXX01";
			locoMap.RY_SystemUsage = "CUS";
			locoMap.RY_RN = Core.Constants.CountryGuids.China;
			Factory.Save();
			declaration = Factory.New<JobDeclaration>();
			defaulter = new UNLOCO_CNPortsDefaulter(Factory, declaration.JE_CNPortOfOriginInfo, declaration.JE_RL_NKPortOfLoadingInfo);
		}

		JobDeclaration declaration;
		UNLOCO_CNPortsDefaulter defaulter;
	}
}
