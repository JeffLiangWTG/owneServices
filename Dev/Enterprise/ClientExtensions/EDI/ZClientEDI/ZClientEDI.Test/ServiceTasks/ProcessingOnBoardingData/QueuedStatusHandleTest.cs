using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using NUnit.Framework;

namespace ZClientEDI.Test.ServiceTasks.ProcessingOnBoardingData
{
	[TestedType(typeof(QueuedStatusHandler))]
	internal class QueuedStatusHandleTest : TestCaseWithFactory
	{
		public void TestHandle()
		{
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Queued;

			ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "ABC";
			ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = "1234567";

			Factory.Save();

			AssertEquals(OnBoardingStatuses.Codes.Queued, ediTokenAuthOnBoardingData.TOD_Status);
			AssertEquals(ZString.Empty, ediTokenAuthOnBoardingData.TOD_StagingPRLink);

			new QueuedStatusHandleForTesting(ediTokenAuthOnBoardingData).Handle();

			AssertEquals(OnBoardingStatuses.Codes.StagingPullRequest, ediTokenAuthOnBoardingData.TOD_Status);
			AssertNotNull(ediTokenAuthOnBoardingData.TOD_StagingPRLink);
		}

		public void TestCompanyCode()
		{
			var licenseEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenseEnterprise.LE_EnterpriseCode = "EDI";
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.EnterprisePK = licenseEnterprise.PK;
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_IM = incident.PK;
			Factory.Save();
			var queuedStatusHandler = new QueuedStatusHandleForTesting(ediTokenAuthOnBoardingData);
			AssertEquals(queuedStatusHandler.CompanyCodeForTest, "EDI");

			var loadedOnBoardingData = new BusinessObjectFactory().Load<EdiTokenAuthOnBoardingData>(ediTokenAuthOnBoardingData.PK);
			queuedStatusHandler = new QueuedStatusHandleForTesting(loadedOnBoardingData);
			AssertNotNullOrEmpty(queuedStatusHandler.CompanyCodeForTest);
			AssertEquals("EDI",queuedStatusHandler.CompanyCodeForTest);
		}

		class QueuedStatusHandleForTesting : QueuedStatusHandler
		{
			public QueuedStatusHandleForTesting(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData) : base(ediTokenAuthOnBoardingData)
			{
			}

			public string CompanyCodeForTest => CompanyCode;

			protected override string CreatePRAndMerge(string subFolder, bool isRemove = false)
			{
				return "http://crikey.wtg.zone/";
			}
		}
	}
}
