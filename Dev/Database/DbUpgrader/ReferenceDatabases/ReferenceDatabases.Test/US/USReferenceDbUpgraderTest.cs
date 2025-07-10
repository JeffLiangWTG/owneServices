using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	[TestedType(typeof(USReferenceDbUpgrader))]
	sealed class USReferenceDbUpgraderTest : ReferenceDbUpgraderTest<USReferenceDbUpgrader>
	{
		public void TestIsShared()
		{
			Assert(new USReferenceDbUpgrader(upgradeContext, testConnection, logger).SupportsVersionSharedDatabase);
		}

		protected override USReferenceDbUpgrader GetNewReferenceDbUpgrader()
		{
			return new USReferenceDbUpgraderForTesting(upgradeContext, testConnection, logger);
		}

		protected override string[] ExpectedReferenceTables
		{
			get
			{
				return new string[]
				{
					"USCAESResponseCode",
					"USCAffirmationOfCompliance",
					"USCAntiDumpingBondCashIndicator",
					"USCAntiDumpingCase",
					"USCAntiDumpingRate",
					"USCAntiDumpingTariff",
					"USCCarrier",
					"USCChapter",
					"USCCountry",
					"USCFDAProductNumber",
					"USCFIRMS",
					"USCForeignPort",
					"USCGoldPrice",
					"USCImportEstablishment",
					"USCImportEstablishmentAlternateName",
					"USCQuota",
					"USCRegionDistrictPort",
					"USCRule",
					"USCRuleSecondaryTariff",
					"USCRuleSecondaryTariffException",
					"USCScheduleB",
					"USCSection",
					"USCTariff",
					"USCTariffDateRestriction",
					"USCTariffDutyRate",
					"USCTariffQuantity",
					"USCTariffRule",
					"USCTariffRuleException",
					"USCTariffValue",
					"USCTeamSpecialist",
					"USCVisa",
					"USCVisaTariff",
					"USCZipCode",
					"USCDataVersion",
					"USCACCase",
					"USCACCaseRate",
					"USCACCaseTariff",
					"USCACCaseLiqSuspension",
					"USCACCaseEvent",
					"USCACCaseBondCash",
					"USCAMSProductNumber"
				};
			}
		}

		protected override void SetUp()
		{
			upgradeContext = new Mock<IUpgradeContext>().Object;
			base.SetUp();
		}

		IUpgradeContext upgradeContext;
	}
}
