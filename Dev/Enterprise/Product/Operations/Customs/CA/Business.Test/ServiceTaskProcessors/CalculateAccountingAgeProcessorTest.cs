using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CalculateAccountingAgeProcessorTest : TestCaseWithFactory
	{
		public void TestNewScript()
		{
			var company01 = Factory.NewWithValidTestData<GlbCompany>();
			company01.GC_Code = "CA1";
			company01.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var company02 = Factory.NewWithValidTestData<GlbCompany>();
			company02.GC_Code = "CA2";
			company02.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AIR";
			company01.GC_OH_OrgProxy = org1.PK;
			company02.GC_OH_OrgProxy = org1.PK;
			var branch01 = company01.Branches.AddNew();
			branch01.GB_Code = "AAA";
			var branch02 = company02.Branches.AddNew();
			branch02.GB_Code = "BBB";

			for (var i = 0; i < 50; i++)
			{
				CreateJobDeclaration(JobMessageTypeList.Codes.Export, B3EntryTypeList.Codes.AutomotiveP, new ZDateTime(2016, 3, 10), ZDateTime.Empty, MessageTypeList.Codes.B3CUSDEC, ZString.Empty, branch01);
			}

			for (var i = 0; i < 50; i++)
			{
				CreateJobDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.NoB3, new ZDateTime(2016, 3, 10), ZDateTime.Empty, MessageTypeList.Codes.B3CUSDEC, ZString.Empty, branch01);
			}

			for (var i = 0; i < 50; i++)
			{
				CreateJobDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.AutomotiveP, new ZDateTime(2016, 3, 10), ZDateTime.Empty, MessageTypeList.Codes.ACIForwarderClose, ZString.Empty, branch01);
			}

			for (var i = 0; i < 50; i++)
			{
				CreateJobDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.AutomotiveP, new ZDateTime(2016, 3, 10), ZDateTime.Empty, MessageTypeList.Codes.B3CUSDEC, B3EntryStatusList.Codes.Confirmed, branch01);
			}

			for (var i = 0; i < 50; i++)
			{
				CreateJobDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.AutomotiveP, new ZDateTime(2016, 3, 10), ZDateTime.Empty, MessageTypeList.Codes.B3CUSDEC, B3EntryStatusList.Codes.Accepted, branch01);
			}

			for (var i = 0; i < 50; i++)
			{
				CreateJobDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.AutomotiveP, new ZDateTime(2016, 3, 10), new ZDateTime(2016, 3, 10), MessageTypeList.Codes.B3CUSDEC, ZString.Empty, branch01);
			}

			for (var i = 0; i < 50; i++)
			{
				CreateJobDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.AutomotiveP, new ZDateTime(2016, 3, 10), ZDateTime.Empty, MessageTypeList.Codes.B3CUSDEC, ZString.Empty, branch02);
			}

			for (var i = 0; i < 50; i++)
			{
				CreateJobDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.AutomotiveP, new ZDateTime(2016, 3, 10), ZDateTime.Empty, MessageTypeList.Codes.B3CUSDEC, ZString.Empty, branch01);
			}
			Factory.Save();

			var logger = new DummyLogger();
			var processor01 = new CalculateAccountingAgeProcessorForTest(logger);
			var collection01 = processor01.GetCandidateDeclarationPKsUsingOldScript_Exposed(company01.PK);
			AssertEquals(50, collection01.Count);

			var processor02 = new CalculateAccountingAgeProcessorForTest(logger);
			var collection02 = processor02.GetCandidateDeclarationPKs_Exposed(company01.PK);
			AssertEquals(50, collection02.Count);
		}

		[TestDate(2016, 3, 17)]
		public void TestProcess()
		{
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_Code = "CA1";
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AIR";
			caCompany.GC_OH_OrgProxy = org1.PK;
			var caBranch = caCompany.Branches.AddNew();
			caBranch.GB_Code = "AAA";

			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_Code = "USA";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CHI";
			usCompany.GC_OH_OrgProxy = org2.PK;
			var branch = usCompany.Branches.AddNew();
			branch.GB_Code = "CHI";
			Factory.Save();

			var declaration1 = CreateJobDeclaration("EXP", "AB", new ZDateTime(2016, 3, 10), ZDateTime.Empty, "B3C", "");
			var declaration2 = CreateJobDeclaration("IMP", "NO", new ZDateTime(2016, 3, 10), ZDateTime.Empty, "B3C", "");
			var declaration3 = CreateJobDeclaration("IMP", "AB", ZDateTime.Empty, ZDateTime.Empty, "B3C", "");
			var declaration4 = CreateJobDeclaration("IMP", "AB", new ZDateTime(2016, 3, 10), new ZDateTime(2016, 3, 16), "B3C", "");
			var declaration5 = CreateJobDeclaration("IMP", "AB", new ZDateTime(2016, 3, 10), ZDateTime.Empty, "REL", "");
			var declaration6 = CreateJobDeclaration("IMP", "AB", new ZDateTime(2016, 3, 10), ZDateTime.Empty, "B3C", "CLR");
			var declaration7 = CreateJobDeclaration("IMP", "AB", new ZDateTime(2016, 3, 10), ZDateTime.Empty, "B3C", "");
			var declaration8 = CreateJobDeclaration("IMP", "AB", new ZDateTime(2016, 3, 11), ZDateTime.Empty, "B3C", "");
			var declaration9 = CreateJobDeclaration("IMP", "AB", new ZDateTime(2016, 3, 12), ZDateTime.Empty, "B3C", "");
			var declaration10 = CreateJobDeclaration("IMP", "AB", new ZDateTime(2016, 3, 12), ZDateTime.Empty, "B3C", "");
			var declaration11 = CreateJobDeclaration("IMP", "AB", new ZDateTime(2016, 3, 10), ZDateTime.Empty, "B3C", "CNF");
			var declaration12 = CreateJobDeclaration("IMP", "AB", new ZDateTime(2016, 3, 10), ZDateTime.Empty, "B3C", "", isCSAEntry: true);
			var declaration13 = CreateJobDeclaration("IMP", "AB", new ZDateTime(2016, 3, 10), ZDateTime.Empty, "CAD", "39");
			declaration10.JE_GB = branch.PK;
			Factory.Save();

			var logger = new DummyLogger();
			var processor = new CalculateAccountingAgeProcessor(logger, 3);
			processor.Process(GlbCompany.CurrentCompany.PK);

			Assert("declaration1.CA_AccountingAge should not be set", declaration1.CA_AccountingAge.IsEmpty);
			Assert("declaration2.CA_AccountingAge should not be set", declaration2.CA_AccountingAge.IsEmpty);
			Assert("declaration3.CA_AccountingAge should not be set", declaration3.CA_AccountingAge.IsEmpty);
			Assert("declaration4.CA_AccountingAge should not be set", declaration4.CA_AccountingAge.IsEmpty);
			Assert("declaration5.CA_AccountingAge should not be set", declaration5.CA_AccountingAge.IsEmpty);
			Assert("declaration6.CA_AccountingAge should not be set", declaration6.CA_AccountingAge.IsEmpty);
			Assert("declaration7.CA_AccountingAge should be set", !declaration7.CA_AccountingAge.IsEmpty);
			Assert("declaration8.CA_AccountingAge should be set", !declaration8.CA_AccountingAge.IsEmpty);
			Assert("declaration9.CA_AccountingAge should be set", !declaration9.CA_AccountingAge.IsEmpty);
			Assert("declaration10.CA_AccountingAge should not be set", declaration10.CA_AccountingAge.IsEmpty);
			Assert("declaration11.CA_AccountingAge should not be set", declaration11.CA_AccountingAge.IsEmpty);
			Assert("declaration12.CA_AccountingAge should not be set", declaration12.CA_AccountingAge.IsEmpty);
			Assert("declaration13.CA_AccountingAge should not be set", declaration13.CA_AccountingAge.IsEmpty);
		}

		JobDeclaration CreateJobDeclaration(ZString jE_MessageType, ZString jE_MessageSubType, ZDateTime jE_EntryAuthorisationDate, ZDateTime cA_K84AccountingDate, ZString cH_MessageType, ZString cH_EntryStatus, GlbBranch branch = null, bool isCSAEntry = false)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = jE_MessageType;
			declaration.JE_MessageSubType = jE_MessageSubType;
			declaration.JE_EntryAuthorisationDate = jE_EntryAuthorisationDate;
			declaration.CA_K84AccountingDate = cA_K84AccountingDate;
			declaration.CA_CSAEntry = isCSAEntry;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = cH_MessageType;
			entryHeader.CH_EntryStatus = cH_EntryStatus;
			declaration.CA_AccountingAge = 0;
			if (branch != null)
			{
				declaration.JE_GB = branch.PK;
			}

			return declaration;
		}

		public class CalculateAccountingAgeProcessorForTest : CalculateAccountingAgeProcessor
		{
			public CalculateAccountingAgeProcessorForTest(ILogger serviceLogger) : base(serviceLogger)
			{
			}

			public DynamicBusinessObjectCollection GetCandidateDeclarationPKs_Exposed(ZGuid companyPK)
			{
				return GetCandidateDeclarationPKs(companyPK);
			}

			public DynamicBusinessObjectCollection GetCandidateDeclarationPKsUsingOldScript_Exposed(ZGuid companyPK)
			{
				var factory = new ReadOnlyBusinessObjectFactory();
				var declarationPKs = new DynamicBusinessObjectCollection(factory);
				declarationPKs.Load(GetCandidateDeclarationPKSql(companyPK));
				return declarationPKs;
			}

			string GetCandidateDeclarationPKSql(ZGuid companyPK)
			{
				var querySql = @"
				SELECT 
					JobDeclaration.JE_PK
				FROM
					dbo.JobDeclaration
				INNER JOIN
					(
						SELECT 
							JE_PK 
						FROM 
							dbo.CAJobDeclaration 
						WHERE 
							EXISTS (SELECT 1 FROM dbo.CusEntryHeader WHERE CH_JE = JE_PK AND CH_MessageType = '{0}' AND CH_EntryStatus <> '{1}' AND CH_EntryStatus <> '{2}')
							AND JE_K84AccountingDate IS NULL
					)AS JE ON JE.JE_PK = JobDeclaration.JE_PK
				INNER JOIN dbo.GLBBRANCH ON GB_PK = JobDeclaration.JE_GB AND GB_GC = '{3}'
				WHERE 
				JobDeclaration.JE_MessageType = '{4}' 
				AND JobDeclaration.JE_MessageSubType <> '{5}' 
				AND JobDeclaration.JE_EntryAuthorisationDate IS NOT NULL";

				return ZString.Format(querySql,
					MessageTypeList.Codes.B3CUSDEC,
					B3EntryStatusList.Codes.Accepted,
					B3EntryStatusList.Codes.Confirmed,
					companyPK,
					JobMessageTypeList.Codes.Import,
					B3EntryTypeList.Codes.NoB3
					);
			}
		}
	}
}
