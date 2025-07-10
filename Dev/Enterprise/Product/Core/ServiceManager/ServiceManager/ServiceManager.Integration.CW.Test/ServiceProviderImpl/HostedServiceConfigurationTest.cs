using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Business.Testing
{
	class HostedServiceConfigurationTest : TestCaseWithFactory
	{
		public class SatisfiesRequirementsTest : TestCaseWithFactory
		{
			public void TestSatisfiesCountryRequirements()
			{
				var attribute = new HostedServiceAttribute("TT1", "Test", "TST", typeof(object));
				var config = new HostedServiceConfiguration(attribute);
				string? branchCode;
				Assert("No country requirement", config.SatisfiesRequirements(out branchCode));
				AssertNull(branchCode);

				attribute.RequiresCompanyInCountry = Core.Constants.CountryCodes.Barbados;
				Assert("No company in Barbados", !config.SatisfiesRequirements(out branchCode));
				AssertNull(branchCode);

				var newCompany = Factory.New<GlbCompany>();
				newCompany.GC_Code = "~TT";
				newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Barbados;
				var newBranch1 = newCompany.Branches.AddNew();
				newBranch1.GB_Code = "~TB";
				newBranch1.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
				Factory.Save();

				Assert(config.SatisfiesRequirements(out branchCode));
				AssertEquals("Have company and branch in Barbados", newBranch1.GB_Code, branchCode);

				var newBranch2 = newCompany.Branches.AddNew();
				newBranch2.GB_Code = "~TA";
				newBranch2.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
				Factory.Save();

				Assert(config.SatisfiesRequirements(out branchCode));
				AssertEquals("First Barbados branch by alphabetical order", newBranch2.GB_Code, branchCode);

				newBranch2.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Canada)).RL_RN_NKCountryCode;
				Factory.Save();

				Assert(config.SatisfiesRequirements(out branchCode));
				AssertEquals("Ignore Canada branch", newBranch1.GB_Code, branchCode);

				newBranch1.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Germany)).RL_RN_NKCountryCode;
				Factory.Save();

				Assert("No branches in Barbados", !config.SatisfiesRequirements(out branchCode));
				AssertNull(branchCode);
			}

			public void TestSatisfiesCountryRequirements2()
			{
				var attribute = new HostedServiceAttribute("TT1", "Test", "TST", typeof(object));
				var config = new HostedServiceConfiguration(attribute);
				attribute.RequiresCompanyInCountry = Core.Constants.CountryCodes.Barbados + ", " + Core.Constants.CountryCodes.TrinidadAndTobago;

				string? branchCode;
				Assert("No company in Barbados", !config.SatisfiesRequirements(out branchCode));
				AssertNull(branchCode);

				var newCompany = Factory.New<GlbCompany>();
				newCompany.GC_Code = "~TT";
				newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.TrinidadAndTobago;
				var newBranch1 = newCompany.Branches.AddNew();
				newBranch1.GB_Code = "~TB";
				newBranch1.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
				Factory.Save();

				Assert(config.SatisfiesRequirements(out branchCode));
				AssertEquals(newBranch1.GB_Code, branchCode);
			}

			public void TestSatisfiesOtherRequirements()
			{
				var attribute = new HostedServiceAttribute("TT2", "Test", "TST", typeof(TaskForTesting));
				var config = new HostedServiceConfiguration(attribute);
				TaskForTesting.SetSettingsAreCorrect(string.Empty, "2a not satisfied", string.Empty, string.Empty);
				Assert(!config.SatisfiesRequirements());

				TaskForTesting.SetSettingsAreCorrect(string.Empty, string.Empty, string.Empty, string.Empty);
				Assert(config.SatisfiesRequirements());

				attribute.RequiresCompanyInCountry = Core.Constants.CountryCodes.Jamaica;
				string? branchCode;
				Assert("No company in Jamaica", !config.SatisfiesRequirements(out branchCode));
				AssertNull(branchCode);

				var newCompany = Factory.New<GlbCompany>();
				newCompany.GC_Code = "~TT";
				newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Jamaica;
				var newBranch = newCompany.Branches.AddNew();
				newBranch.GB_Code = "~TB";
				newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
				Factory.Save();

				Assert(config.SatisfiesRequirements(out branchCode));
				AssertEquals("Have company and branch in Jamaica", newBranch.GB_Code, branchCode);

				TaskForTesting.SetSettingsAreCorrect("1a not satisfied", string.Empty, string.Empty, string.Empty);
				Assert(!config.SatisfiesRequirements());

				TaskForTesting.SetSettingsAreCorrect(string.Empty, string.Empty, "3a1 not satisfied", "3a2 not satisfied");
				Assert(!config.SatisfiesRequirements());
			}

			[ExpectNoExceptions]
			public void TestRequirementCheckDoesNotTimeOutWhenExpectedToComplete()
			{
				CombineAssertions(() =>
				{
					Test(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), typeof(TaskForTestingwithSlowRequirementSingle));
					Test(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), typeof(TaskForTestingwithSlowRequirementList));
					Test(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(6), typeof(TaskForTestingwithSlowRequirementSingle));
					Test(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(6), typeof(TaskForTestingwithSlowRequirementList));
					Test(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15), typeof(TaskForTestingwithSlowRequirementSingle));
					Test(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15), typeof(TaskForTestingwithSlowRequirementList));
				});

				static void Test(TimeSpan requirementCheckDuration, TimeSpan timeout, Type serviceTaskType)
				{
					// Arrange
					requirementCheckTestDuration = requirementCheckDuration;
					var attribute = new HostedServiceAttribute("TT2", "Test", "TST", serviceTaskType);
					var config = new HostedServiceConfiguration(attribute);

					using (SystemDataRegistry.Instance.ServiceTaskRequirementCheckTimeLimitSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, timeout.Seconds))
					{
						// Act
						var success = config.SatisfiesRequirements();

						// Assert
						Assert("Requirement check completed.", success);
					}
				}
			}

			[ExpectNoExceptions]
			public void TestRequirementCheckDoesNotIncludeDbConnectionInTimeLimit()
			{
				// Arrange
				var timeLimit = TimeSpan.FromSeconds(5);
				var dbConnectionDelay = timeLimit + TimeSpan.FromSeconds(1);
				var attribute = new HostedServiceAttribute("TT2", "Test", "TST", typeof(TaskForTesting));
				TaskForTesting.SetSettingsAreCorrect(string.Empty, string.Empty, string.Empty, string.Empty);
				var config = new HostedServiceConfigurationForTestWithDbConnectionDelay(attribute, (int)dbConnectionDelay.TotalMilliseconds);

				using (SystemDataRegistry.Instance.ServiceTaskRequirementCheckTimeLimitSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, timeLimit.Seconds))
				{
					// Act
					var success = config.SatisfiesRequirements();

					// Assert
					Assert("Requirement check completed.", success);
				}
			}

			public void TestRequirementCheckTimesOutAsExpected()
			{
				CombineAssertions(() =>
				{
					Test(TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(5), typeof(TaskForTestingwithSlowRequirementSingle));
					Test(TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(5), typeof(TaskForTestingwithSlowRequirementList));
					Test(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(8), typeof(TaskForTestingwithSlowRequirementSingle));
					Test(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(8), typeof(TaskForTestingwithSlowRequirementList));
					Test(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(15), typeof(TaskForTestingwithSlowRequirementSingle));
					Test(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(15), typeof(TaskForTestingwithSlowRequirementList));
				});

				static void Test(TimeSpan requirementCheckDuration, TimeSpan timeout, Type serviceTaskType)
				{
					// Arrange
					requirementCheckTestDuration = requirementCheckDuration;
					var attribute = new HostedServiceAttribute("TT2", "Test", "TST", serviceTaskType);
					var config = new HostedServiceConfiguration(attribute);

					using (SystemDataRegistry.Instance.ServiceTaskRequirementCheckTimeLimitSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, timeout.Seconds))
					{
						// Act
						var success = config.SatisfiesRequirements();

						// Assert
						Assert("Requirement check did not complete.", !success);
					}

					AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
					AssertType<ServiceTaskRequirementCheckTimeoutException>(ExceptionReporterTestListener.Instance[0]);

					ExceptionReporterTestListener.Instance.Clear();
					ErrorReporter.Clear(); // so that errors are reported for each test
				}
			}

			public void TestRequirementCheckTimeoutExceptionKeyAndMessageInErrorReport()
			{
				CombineAssertions(() =>
				{
					Test(TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(5), typeof(TaskForTestingwithSlowRequirementSingle));
					Test(TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(6), typeof(TaskForTestingwithSlowRequirementList));
				});

				static void Test(TimeSpan requirementCheckDuration, TimeSpan timeout, Type serviceTaskType)
				{
					// Arrange
					requirementCheckTestDuration = requirementCheckDuration;
					var attribute = new HostedServiceAttribute("TT2", "Test", "TST", serviceTaskType);
					var config = new HostedServiceConfiguration(attribute);

					using (SystemDataRegistry.Instance.ServiceTaskRequirementCheckTimeLimitSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, timeout.Seconds))
					{
						// Act
						var success = config.SatisfiesRequirements();

						// Assert
						Assert("Requirement check did not complete.", !success);
					}

					var expectedKey = $@"Service task {attribute.Code} ({attribute.Description}) timed out on the {serviceTaskType.GetMethods()[0].Name} requirement check ({timeout.TotalSeconds}s limit).";
					var expectedExceptionMessage = expectedKey;

					AssertEquals(expectedKey, ExceptionReporterTestListener.Instance.GetExceptionKey(0));
					AssertContains(expectedExceptionMessage, ExceptionReporterTestListener.Instance[0].Message);

					ExceptionReporterTestListener.Instance.Clear();
					ErrorReporter.Clear();
				}
			}

			[ExpectNoExceptions]
			public void TestRequirementCheckWithNonCriticalExceptionIsHandled()
			{
				// Arrange
				var attribute = new HostedServiceAttribute("TT2", "Test", "TST", typeof(TaskForTestingThrowsNonCriticalException));
				var config = new HostedServiceConfiguration(attribute);

				// Act
				var result = config.SatisfiesRequirements();

				// Assert
				Assert(!result);
				AssertGreaterThan(ExceptionReporterTestListener.Instance.Count, 0);
				foreach (var error in ExceptionReporterTestListener.Instance)
				{
					AssertContains("Operation is not valid due to the current state of the object.", error.Message);
				}

				ExceptionReporterTestListener.Instance.Clear();
			}

			public void TestRequirementCheckWithCriticalExceptionRaises()
			{
				// Arrange
				var attribute = new HostedServiceAttribute("TT2", "Test", "TST", typeof(TaskForTestingThrowsCriticalException));

				// Act
				var config = new HostedServiceConfiguration(attribute);

				// Assert
				var exception = AssertExceptionThrown<Exception>(() => config.SatisfiesRequirements());

				Assert(exception.IsCriticalException());
			}

			public void TestSatisfiesRequirements_CanRunInAnyBranchIsTrue()
			{
				var newCompany = Factory.New<GlbCompany>();
				newCompany.GC_Code = "~TT";
				newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Barbados;
				var newBranch1 = newCompany.Branches.AddNew();
				newBranch1.GB_Code = "~TB";
				newBranch1.GB_RL_NKHomePort = "AAAAA";
				Factory.Save();

				var attribute = new HostedServiceAttribute("TT1", "Test", "TST", typeof(object));
				var config = new HostedServiceConfiguration(attribute);
				attribute.RequiresCompanyInCountry = Core.Constants.CountryCodes.Barbados;

				CombineAssertions(() =>
				{
					AssertEquals("Has company in Barbados, has branch but not in Barbados, CanRunInAnyBranch is false", expected: false, config.SatisfiesRequirements(out var branchCode));

					attribute.CanRunInAnyBranch = true;
					AssertEquals("CanRunInAnyBranch is true", expected: true, config.SatisfiesRequirements(out branchCode));

					attribute.RequiresCompanyInCountry = Core.Constants.CountryCodes.AlandIslands;
					AssertEquals("No Company for AX country", expected: false, config.SatisfiesRequirements(out branchCode));
				});
			}

			public void TestSatisfiesRequirementsWithMissingType()
			{
				var config1 = new HostedServiceConfiguration(new HostedServiceAttribute("TT2", "Test", "TST", typeof(TaskForTesting)));
				TaskForTesting.SetSettingsAreCorrect(string.Empty, string.Empty, string.Empty, string.Empty);
				Assert(config1.SatisfiesRequirements());

				var config2 = new HostedServiceConfiguration(new HostedServiceAttribute { TypeName = "Type1", TypeAssemblyName = "Assembly2" });
				Assert(!config2.SatisfiesRequirements());

				var config3 = new HostedServiceConfiguration(new HostedServiceAttribute { TypeName = "Type1", TypeAssemblyName = Path.GetFileNameWithoutExtension(typeof(TaskForTesting).Assembly.Location) });
				Assert(!config3.SatisfiesRequirements());
			}

			public void TestConfiguredForNudging_NudgedAttributeReturnsTrue_ReturnsTrue()
			{
				TaskForTestingNudgingAttribute.IsNudged = true;
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TN2", "Test", "TST", typeof(TaskForTestingNudgingAttribute)));
				Assert("Class with nudging attribute that returns true should return true", config.IsConfiguredForNudging);
			}

			public void TestConfiguredForNudging_NudgedAttributeReturnsFalse_ReturnsFalse()
			{
				TaskForTestingNudgingAttribute.IsNudged = false;
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TN2", "Test", "TST", typeof(TaskForTestingNudgingAttribute)));
				Assert("Class with nudging attribute that returns false should return false", !config.IsConfiguredForNudging);
			}

			public void TestConfiguredForNudging_NudgedAttributeNotConfigured_ReturnsFalse()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TN1", "Test", "TST", typeof(TaskForTesting)));
				Assert("Class with no nudging attribute should return false", !config.IsConfiguredForNudging);
			}
		}

		public class CheckSatisfiesRequirementsForCurrentBranchTest : TestCaseWithFactory
		{
			public void TestSatisfiesRequirementsOnCurrentBranch()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT2", "Test", "TST", typeof(TaskForTesting)));
				TaskForTesting.SetSettingsAreCorrect(string.Empty, "2a not satisfied", string.Empty, string.Empty);
				var failureDescriptions = config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies());
				AssertEquals(1, failureDescriptions.Length);
				AssertCollectionContains("2a not satisfied", failureDescriptions);

				TaskForTesting.SetSettingsAreCorrect(string.Empty, string.Empty, string.Empty, string.Empty);
				failureDescriptions = config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies());
				AssertEquals(0, failureDescriptions.Length);

				TaskForTesting.SetSettingsAreCorrect("1a not satisfied", string.Empty, string.Empty, string.Empty);
				failureDescriptions = config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies());
				AssertEquals(1, failureDescriptions.Length);
				AssertCollectionContains("1a not satisfied", failureDescriptions);

				TaskForTesting.SetSettingsAreCorrect(string.Empty, string.Empty, "3a1 not satisfied", "3a2 not satisfied");
				failureDescriptions = config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies());
				AssertEquals(2, failureDescriptions.Length);
				AssertCollectionContains("3a1 not satisfied", failureDescriptions);
				AssertCollectionContains("3a2 not satisfied", failureDescriptions);
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsTrue()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = string.Join(", ", Core.Constants.CountryCodes.AlandIslands, Core.Constants.CountryCodes.AntiguaAndBarbuda),
					CanRunInAnyBranch = true,
				});

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"No active company exists for any of the required countries. The configured list of the required countries for the task=AX, AG.",
				}, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()));
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsTrue_RequiresCompanyInCountry_ContainsEmptyCode()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = ",US",
					CanRunInAnyBranch = true,
				});

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"No active company exists for any of the required countries. The configured list of the required countries for the task=,US.",
				}, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()));
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsTrue_RequiresCompanyInCountry_ContainsDuplicateCode()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = "us,US",
					CanRunInAnyBranch = true,
				});

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"No active company exists for any of the required countries. The configured list of the required countries for the task=us,US.",
				}, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()));
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsTrue_RequiresCompanyInCountry_EmptyCodesCombination()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = " , ",
					CanRunInAnyBranch = true,
				});

				AssertEquals(0, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()).Length);
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsFalse_RequiresCompanyInCountry_ContainsEmptyCode()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = ",US",
					CanRunInAnyBranch = false,
				});

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"The current company country=AU was not in the configured list of the required countries for the task=,US.",
					"The current branch country=AU was not in the configured list of the required countries for the task=,US.",
				}, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()));
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsFalse_RequiresCompanyInCountry_ContainsDuplicateCode()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = "us,US",
					CanRunInAnyBranch = false,
				});

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"The current company country=AU was not in the configured list of the required countries for the task=us,US.",
					"The current branch country=AU was not in the configured list of the required countries for the task=us,US.",
				}, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()));
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsFalse_RequiresCompanyInCountry_EmptyCodesCombination()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = " , ",
					CanRunInAnyBranch = false,
				});

				AssertEquals(0, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()).Length);
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsTrue_OnlyOneRequiredCompanyExists()
			{
				var newCompany = Factory.New<GlbCompany>();
				newCompany.GC_Code = "~TT";
				newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				var newBranch1 = newCompany.Branches.AddNew();
				newBranch1.GB_Code = "~TB";
				newBranch1.GB_RL_NKHomePort = "AAAAA";
				Factory.Save();

				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = $"{Core.Constants.CountryCodes.UnitedStates}, {Core.Constants.CountryCodes.PuertoRico}",
					CanRunInAnyBranch = true,
				});

				AssertEquals(0, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()).Length);
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsFalse_OnlyOneRequiredCompanyExists()
			{
				var newCompany = Factory.New<GlbCompany>();
				newCompany.GC_Code = "~TT";
				newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				var newBranch1 = newCompany.Branches.AddNew();
				newBranch1.GB_Code = "~TB";
				newBranch1.GB_RL_NKHomePort = "AAAAA";
				Factory.Save();

				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = $"{Core.Constants.CountryCodes.UnitedStates}, {Core.Constants.CountryCodes.PuertoRico}",
					CanRunInAnyBranch = false,
				});

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"The current company country=AU was not in the configured list of the required countries for the task=US, PR.",
					"The current branch country=AU was not in the configured list of the required countries for the task=US, PR.",
				}, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()));
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsFalse_CurrentCompanyIsInRequiredList()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = $"{Core.Constants.CountryCodes.Australia}, {Core.Constants.CountryCodes.PuertoRico}",
					CanRunInAnyBranch = false,
				});

				AssertEquals(0, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()).Length);
			}

			public void TestCheckSatisfiesRequirementsForCurrentBranch_CanRunInAnyBranchIsFalse_CurrentCompanyIsNotInRequiredList()
			{
				var config = new HostedServiceConfiguration(new HostedServiceAttribute("TT1", "Test", "TST", typeof(object))
				{
					RequiresCompanyInCountry = $"{Core.Constants.CountryCodes.UnitedStates}, {Core.Constants.CountryCodes.PuertoRico}",
					CanRunInAnyBranch = false,
				});

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"The current company country=AU was not in the configured list of the required countries for the task=US, PR.",
					"The current branch country=AU was not in the configured list of the required countries for the task=US, PR.",
				}, config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()));
			}

			[UseSnapshotProtection]
			public void TestDoesNotReadDatabase()
			{
				// Arrange
				var config = new HostedServiceConfiguration(Mock.Of<IHostedServiceAttribute>(serviceAttribute =>
					serviceAttribute.Code == "TST"
					&& serviceAttribute.RequiresCompanyInCountry == Core.Constants.CountryCodes.Russia
					&& !serviceAttribute.CanRunInAnyBranch));

				var activeCompanies = GlbCompany.GetActiveCompanies();
				Db.Connection.CloseConnection();

				// Act
				var result = config.CheckSatisfiesRequirementsForCurrentBranch(activeCompanies);

				// Assert
				CombineAssertions(() =>
				{
					AssertGreaterThan(result.Length, 0);
					AssertEquals(ConnectionState.Closed, Db.Connection.State);
				});
			}

			public void TestNullCurrentCompany()
			{
				// Arrange
				var userContextMock = new Mock<IUserContext>();
				var userContext = Env.CurrentUserContext;
				userContextMock
					.SetupGet(context => context.Branch)
					.Returns(userContext.Branch);

				var config = new HostedServiceConfiguration(new HostedServiceAttribute("Code", "Task", "TST", typeof(object))
				{
					RequiresCompanyInCountry = string.Join(", ",
						GlbBranch.CurrentBranch.Country.RN_Code,
						Core.Constants.CountryCodes.Barbados,
						Core.Constants.CountryCodes.AntiguaAndBarbuda),
				});

				using (Env.SetTemporaryUserContext(userContextMock.Object))
				{
					// Act
					var result = config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()).Single();

					// Assert
					AssertContains("CurrentCompany is not set", result);
					AssertContains($"required countries for the task {config.ServiceAttribute.RequiresCompanyInCountry}.", result, ignoreCase: true);
				}
			}

			public void TestNullCurrentBranch()
			{
				// Arrange
				var userContextMock = new Mock<IUserContext>();
				var userContext = Env.CurrentUserContext;
				userContextMock
					.SetupGet(context => context.Company)
					.Returns(userContext.Company);

				var config = new HostedServiceConfiguration(new HostedServiceAttribute("Code", "Task", "TST", typeof(object))
				{
					RequiresCompanyInCountry = string.Join(", ",
						GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
						Core.Constants.CountryCodes.Barbados,
						Core.Constants.CountryCodes.AntiguaAndBarbuda),
				});

				using (Env.SetTemporaryUserContext(userContextMock.Object))
				{
					// Act
					var result = config.CheckSatisfiesRequirementsForCurrentBranch(GlbCompany.GetActiveCompanies()).Single();

					// Assert
					AssertContains("CurrentBranch is not set", result);
					AssertContains($"required countries for the task {config.ServiceAttribute.RequiresCompanyInCountry}.", result, ignoreCase: true);
				}
			}

			public void TestWrongParamsCall()
			{
				var config = new HostedServiceConfiguration(Mock.Of<IHostedServiceAttribute>(serviceAttribute =>
					serviceAttribute.Code == "TST"
					&& serviceAttribute.RequiresCompanyInCountry == Core.Constants.CountryCodes.Russia
					&& !serviceAttribute.CanRunInAnyBranch));

				CombineAssertions(() =>
				{
					var result = AssertExceptionThrown<ArgumentNullException>(() => config.CheckSatisfiesRequirementsForCurrentBranch(null!));
					AssertEquals("activeCompanies", result.ParamName);
				});
			}
		}

		public class MiscellaneousTest : TestCaseWithFactory
		{
			public void TestGetServiceTaskType()
			{
				var config1 = new HostedServiceConfiguration(new HostedServiceAttribute { TypeName = typeof(TaskForTesting).FullName!, TypeAssemblyName = Path.GetFileNameWithoutExtension(typeof(TaskForTesting).Assembly.Location) });
				AssertEquals(typeof(TaskForTesting), config1.SafeType);

				var config2 = new HostedServiceConfiguration(new HostedServiceAttribute { TypeName = "Type1", TypeAssemblyName = Path.GetFileNameWithoutExtension(typeof(TaskForTesting).Assembly.Location) });
				AssertNull(config2.SafeType);

				var config3 = new HostedServiceConfiguration(new HostedServiceAttribute { TypeName = "Type1", TypeAssemblyName = "Assembly2" });
				AssertNull(config3.SafeType);
			}
		}

		abstract class TaskForTestingBase
		{
			[HostedServiceRequirement]
			public static string GetSettingsAreCorrect1a()
			{
				return settingsAreCorrect[0];
			}

			[HostedServiceRequirement]
			protected static string GetSettingsAreCorrect1b()
			{
				return "Should not be called";
			}

			protected static string[] settingsAreCorrect = new string[2];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test class requires private methods with [HostedServiceRequirements] attribute to confirm incorrect code is handled.")]
		class TaskForTesting : TaskForTestingBase
		{
			[HostedServiceRequirement]
			public static string GetSettingsAreCorrect2a()
			{
				return settingsAreCorrect[1];
			}

			[HostedServiceRequirements]
			public static string[] GetSettingsAreCorrect3a()
			{
				if (string.IsNullOrEmpty(settingsAreCorrect[2]) && string.IsNullOrEmpty(settingsAreCorrect[3]))
				{
					return Array.Empty<string>();
				}

				return new[] { settingsAreCorrect[2], settingsAreCorrect[3] };
			}

			[HostedServiceRequirements]
			static string[] GetSettingsAreCorrect3b()
			{
				return new[] { "Should not be called" };
			}

			[HostedServiceRequirement]
			static string GetSettingsAreCorrect2b()
			{
				return "Should not be called";
			}

			public static void SetSettingsAreCorrect(params string[] settingsAreCorrect_)
			{
				settingsAreCorrect = settingsAreCorrect_;
			}
		}

		class TaskForTestingNudgingAttribute : TaskForTestingBase
		{
			[HostedServiceNudged]
			public static bool CheckIsNudged()
			{
				return IsNudged;
			}

			public static bool IsNudged { get; set; }
		}

		class TaskForTestingThrowsNonCriticalException
		{
			[HostedServiceRequirement]
			public static string NonCriticalExceptionRequirementCheck()
			{
				throw new InvalidOperationException();
			}
		}

		class TaskForTestingThrowsCriticalException
		{
			[HostedServiceRequirement]
			public static string CriticalExceptionRequirementCheck()
			{
				throw new OutOfMemoryException();
			}
		}

		class TaskForTestingwithSlowRequirementList
		{
			[HostedServiceRequirements]
			public static string[] SlowRequirementCheckList()
			{
				Thread.Sleep(requirementCheckTestDuration);

				return Array.Empty<string>();
			}
		}

		class TaskForTestingwithSlowRequirementSingle
		{
			[HostedServiceRequirement]
			public static string SlowRequirementCheckSingle()
			{
				Thread.Sleep(requirementCheckTestDuration);

				return string.Empty;
			}
		}

		class HostedServiceConfigurationForTestWithDbConnectionDelay : HostedServiceConfiguration
		{
			public HostedServiceConfigurationForTestWithDbConnectionDelay(IHostedServiceAttribute serviceAttribute, int delay) : base(serviceAttribute)
			{
				this.delay = delay;
			}

			protected override IDisposable DisposableActionForDbConnection()
			{
				Thread.Sleep(delay);
				return base.DisposableActionForDbConnection();
			}

			readonly int delay;
		}

		static TimeSpan requirementCheckTestDuration = TimeSpan.FromSeconds(5);
	}
}
