using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class GatewayProfitRedistributionConsolValidatorTest : TestCaseWithFactory
	{
		public void TestValidation()
		{
			var consol = testObjectCreator.CreateConsol();
			consol.JK_UniqueConsignRef = "CN00001";
			AssertValidation(consol, false, "CN00001 is not a Gateway Consol.");

			var gatewayConsol = profitShareTestHelper.CreateConsol("CN00002");
			AssertValidation(gatewayConsol, true, null);

			var consolidationProfitShare = Factory.NewWithValidTestData<ConsolidationProfitShare>();
			consolidationProfitShare.CPS_JK = gatewayConsol.PK;
			Factory.Save();

			AssertValidation(gatewayConsol, false, "CN00002 is already processed.");
		}

		public void TestValidation_DuplicateConsol_UsingDifferentLoginCompany()
		{
			var branch1 = profitShareTestHelper.CreateBranch("BSG", "CSG", "SG", "SGSIN");
			var branch2 = profitShareTestHelper.CreateBranch("BIN", "CIN", "IN", "INIXE");
			var branch1Address = branch1.OrgProxy.MainAddress;
			var branch2Address = branch2.OrgProxy.MainAddress;
			var gatewayConsol1PK = profitShareTestHelper.CreateGatewayConsol(branch1Address, branch2Address).PK;
			var gatewayConsol2PK = profitShareTestHelper.CreateGatewayConsol(branch2Address, branch1Address).PK;

			Factory.Save();

			var branch1PK = branch1.PK.ToGuid();
			var branch2PK = branch2.PK.ToGuid();

			AssertValidation(branch1PK, gatewayConsol1PK, true);
			AssertValidation(branch2PK, gatewayConsol1PK, true);
			AssertValidation(branch1PK, gatewayConsol2PK, true);
			AssertValidation(branch2PK, gatewayConsol2PK, true);

			ProcessConsol(branch1PK, gatewayConsol1PK);
			ProcessConsol(branch2PK, gatewayConsol2PK);

			AssertValidation(branch1PK, gatewayConsol1PK, false);
			AssertValidation(branch2PK, gatewayConsol1PK, true);
			AssertValidation(branch1PK, gatewayConsol2PK, true);
			AssertValidation(branch2PK, gatewayConsol2PK, false);

			ProcessConsol(branch1PK, gatewayConsol2PK);
			ProcessConsol(branch2PK, gatewayConsol1PK);

			AssertValidation(branch1PK, gatewayConsol1PK, false);
			AssertValidation(branch1PK, gatewayConsol2PK, false);
			AssertValidation(branch2PK, gatewayConsol1PK, false);
			AssertValidation(branch2PK, gatewayConsol2PK, false);

			void ProcessConsol(Guid branchPK, ZGuid consolPK)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK, Env.CurrentDepartment.PK))
				{
					var consol = Factory.Load<ForwardingConsol>(consolPK);
					using (var consolJob = new JobHeader.Loader(Factory, consol).TryLoadOrCreateWithoutMutexForTestOnly())
					{
						AssertNotNull("Pre-Condition: consol job", consolJob);

						var profitShareRedistribution = Factory.NewWithValidTestData<ProfitShareRedistribution>();
						var consolidationProfitShare = Factory.New<ConsolidationProfitShare>();
						consolidationProfitShare.CPS_PSR = profitShareRedistribution.PK;
						consolidationProfitShare.CPS_RX_NKCurrency = CurrencyCodes.Australia;
						consolidationProfitShare.CPS_JK = consol.PK;
						consolidationProfitShare.CPS_JH_ConsolJob = consolJob.PK;
					}

					Factory.Save();
				}
			}
		}

		void AssertValidation(ForwardingConsol forwardingConsol, bool isValid, string expectedError)
		{
			using (var logger = new TestLogger())
			{
				var validation = new GatewayProfitRedistributionConsolValidator(Factory, forwardingConsol, logger);
				AssertEquals(isValid, validation.IsValid());
				AssertEquals(expectedError, logger.GetLogs(LogType.Error).FirstOrDefault());
			}
		}

		void AssertValidation(Guid branchPK, ZGuid consolPK, bool isValid)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK, Env.CurrentDepartment.PK))
			{
				var consol = Factory.Load<ForwardingConsol>(consolPK);
				AssertValidation(consol, isValid, isValid ? null : $"{consol.JK_UniqueConsignRef} is already processed.");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);
			profitShareTestHelper = new ProfitShareTestHelper(testObjectCreator);
		}

		ProfitShareTestHelper profitShareTestHelper;
		TestObjectCreator testObjectCreator;
	}
}
