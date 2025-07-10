using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class SetExceptionCodeProcessorTest : TestCaseWithFactory
	{
		public void TestNewScript()
		{
			CACustomsDataRegistry.Instance.TimeFrameForExceptionReporting.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 10);
			CACustomsDataRegistry.Instance.B3AcceptedButNotReportedOnDN.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 11);
			CACustomsDataRegistry.Instance.B3NoResponseThreshold.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 12);
			CACustomsDataRegistry.Instance.PostArrivalNotReleased.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 14);
			CACustomsDataRegistry.Instance.PARSNotreleasedOceanThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);
			CACustomsDataRegistry.Instance.PARSNotreleasedAirThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 16);
			CACustomsDataRegistry.Instance.PARSNotReleasedHighwayRailAndOtherThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 17);

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
			Factory.Save();

			for (var i = 0; i < 100; i++)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_EntrySubmittedDate = new ZDateTime(2021, 01, 12);
				declaration.JE_DeclarationReference = "B0000000" + i;
				declaration.FillWithValidTestData();
				declaration.JE_GB = branch01.PK;
				declaration.CA_K84AccountingDate = ZDateTime.Empty;
			}

			for (var i = 0; i < 100; i++)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntrySubmittedDate = new ZDateTime(2011, 01, 12);
				declaration.JE_DeclarationReference = "B0000001" + i;
				declaration.FillWithValidTestData();
				declaration.JE_GB = branch01.PK;
				declaration.CA_K84AccountingDate = ZDateTime.Empty;
			}

			for (var i = 0; i < 100; i++)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntrySubmittedDate = new ZDateTime(2021, 01, 12);
				declaration.JE_DeclarationReference = "B0000002" + i;
				declaration.FillWithValidTestData();
				declaration.JE_GB = branch02.PK;
				declaration.CA_K84AccountingDate = ZDateTime.Empty;
			}

			for (var i = 0; i < 100; i++)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntrySubmittedDate = new ZDateTime(2021, 01, 12);
				declaration.JE_DeclarationReference = "B0000003" + i;
				declaration.FillWithValidTestData();
				declaration.JE_GB = branch01.PK;
				declaration.CA_K84AccountingDate = new ZDateTime(2021, 01, 12);
			}

			for (var i = 0; i < 100; i++)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntrySubmittedDate = new ZDateTime(2021, 01, 12);
				declaration.JE_DeclarationReference = "B0000004" + i;
				declaration.FillWithValidTestData();
				declaration.JE_GB = branch01.PK;
				declaration.CA_K84AccountingDate = ZDateTime.Empty;
			}
			Factory.Save();

			var logger = new DummyLogger();
			var processor01 = new SetExceptionCodeProcessorForTest(logger);
			var collection01 = processor01.GetCandidateDeclarationPKsUsingOldScript_Exposed(company01, new ZDate(2020, 01, 12));
			AssertEquals(100, collection01.Count);

			var processor02 = new SetExceptionCodeProcessorForTest(logger);
			var collection02 = processor02.GetCandidateDeclarationPKs_Exposed(company01, new ZDate(2020, 01, 12));
			AssertEquals(100, collection02.Count);
		}

		[TestDate(2016, 9, 1)]
		public void TestProcess()
		{
			CACustomsDataRegistry.Instance.TimeFrameForExceptionReporting.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 10);
			CACustomsDataRegistry.Instance.B3AcceptedButNotReportedOnDN.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 11);
			CACustomsDataRegistry.Instance.B3NoResponseThreshold.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 12);
			CACustomsDataRegistry.Instance.PostArrivalNotReleased.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 14);
			CACustomsDataRegistry.Instance.PARSNotreleasedOceanThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);
			CACustomsDataRegistry.Instance.PARSNotreleasedAirThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 16);
			CACustomsDataRegistry.Instance.PARSNotReleasedHighwayRailAndOtherThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 17);
			SetupJobDeclarations();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = "B3C";
			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			declaration.CA_K84StatementDate = ZDateTime.Empty;
			var message = entryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageType = "B3C";
			message.EM_MessageSubType = "CLR";
			message.EM_SystemCreateTimeUtc = new ZDateTime(2016, 9, 1).AddDays(-12);

			Factory.Save();
			var logger = new DummyLogger();
			var processor = new SetExceptionCodeProcessor(logger);
			processor.Process(glbCompany);
			AssertEquals(CAExceptionCodeList.Codes.EntryLodgedAndAcceptedNotReportedONDN, declaration.CA_DeclarationException);
			AssertEquals("Information - Set Exception Code 010 to declaration B00000001.", logger.Last());

			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Error;
			Factory.Save();
			logger = new DummyLogger();
			processor = new SetExceptionCodeProcessor(logger);
			processor.Process(glbCompany);
			AssertEquals(ZString.Empty, declaration.CA_DeclarationException);
			AssertEquals("Information - Clear Exception Code for declaration B00000001.", logger.Last());
		}

		void SetupJobDeclarations()
		{
			glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "CA1";
			glbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AIR";
			glbCompany.GC_OH_OrgProxy = org1.PK;
			var caBranch1 = glbCompany.Branches.AddNew();
			caBranch1.GB_Code = "AAA";
			var caBranch2 = glbCompany.Branches.AddNew();
			caBranch2.GB_Code = "BBB";

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.FillWithValidTestData();
			declaration.JE_GB = caBranch1.PK;
			declaration.JE_EntrySubmittedDate = new ZDateTime(2016, 9, 1);
			Factory.Save();
		}
		GlbCompany glbCompany;
		JobDeclaration declaration;

		public class SetExceptionCodeProcessorForTest : SetExceptionCodeProcessor
		{
			public SetExceptionCodeProcessorForTest(ILogger serviceLogger) : base(serviceLogger)
			{
			}

			public DynamicBusinessObjectCollection GetCandidateDeclarationPKs_Exposed(GlbCompany company, ZDate effectiveDate)
			{
				return GetCandidateDeclarationPKs(company, effectiveDate);
			}

			public DynamicBusinessObjectCollection GetCandidateDeclarationPKsUsingOldScript_Exposed(GlbCompany company, ZDate effectiveDate)
			{
				var factory = new ReadOnlyBusinessObjectFactory();
				var declarationPKs = new DynamicBusinessObjectCollection(factory);
				declarationPKs.Load(GetCandidateDeclarationPKOldSql(company.PK, effectiveDate));
				return declarationPKs;
			}

			string GetCandidateDeclarationPKOldSql(ZGuid companyCode, ZDate effectiveDate)
			{
				var querySql = @"
				SELECT
					JobDeclaration.JE_PK
				FROM 
					dbo.CAJobDeclaration AS JobDeclaration
					INNER JOIN dbo.GLBBRANCH ON GB_PK = JobDeclaration.JE_GB AND GB_GC = '{0}'
				WHERE 
					JobDeclaration.JE_MessageType <> '{1}' 
					AND JobDeclaration.JE_EntrySubmittedDate IS NOT NULL 
					AND JobDeclaration.JE_EntrySubmittedDate > '{2}'
					AND JobDeclaration.JE_K84AccountingDate IS NULL";

				return ZString.Format(querySql,
					companyCode,
					JobMessageTypeList.Codes.Export,
					effectiveDate.ToString("yyyyMMdd", CultureInfo.CurrentCulture)
					);
			}
		}
	}
}
