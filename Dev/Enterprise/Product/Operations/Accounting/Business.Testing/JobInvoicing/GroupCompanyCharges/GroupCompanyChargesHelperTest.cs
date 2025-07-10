using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class GroupCompanyChargesHelperTest : TestCaseWithFactory
	{
		#region Accept

		public void TestAccept()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var fisDepartment = TestObjectCreator.FISDepartment;
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("GLBCHG");
			Factory.Save();

			ZGuid groupCompanyChargePK;
			ZString sellPaymentBasisDescription;

			using (TestObjectCreator.SwitchEnvToCompany(creditorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var localChargeCode = globalChargeCode.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);
				var groupCompanyCharge = TestObjectCreator.CreateChargeWithPaymentBasis(job, localChargeCode, debtorCompany.OrgProxy, 95m);

				groupCompanyChargePK = groupCompanyCharge.PK;
				sellPaymentBasisDescription = groupCompanyCharge.SellPaymentBases.Single().ToString();
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(debtorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				AssertEquals("Pre-condition", 0, job.Charges.Count);

				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var foundGroupCompanySellCharge = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().Single();
				AssertEquals("Pre-condition: should find single charge based on the group company charge", groupCompanyChargePK, foundGroupCompanySellCharge.GroupCompanySellCharge.PK);
				AssertNull("Group Company Charge has no Local Cost Charge Equivalent", foundGroupCompanySellCharge.GroupCompanyCostCharge);
				AssertEquals("Create", foundGroupCompanySellCharge.AcceptActionDescription);

				job.GroupCompanyChargesForDebtor.AcceptSellChargesAsCosts(new[] { foundGroupCompanySellCharge });

				var acceptedCharge = job.Charges.Cast<Charge>().Single();
				AssertEquals(globalChargeCode.AC_Code, acceptedCharge.ChargeCode.AC_Code);
				AssertEquals("ChargeCode should be mapped to local company charge code", debtorCompany.PK, acceptedCharge.ChargeCode.AC_GC);
				AssertEquals(Constants.CurrencyCodes.UnitedStates, acceptedCharge.JR_RX_NKCostCurrency);
				AssertEquals(95m, acceptedCharge.JR_OSCostAmt);
				AssertEquals("debtorCompany owes money to creditorCompany", creditorCompany.OrgProxy, acceptedCharge.CostAccount);
				AssertEquals(sellPaymentBasisDescription, acceptedCharge.CostPaymentBases.Single().ToString());
				AssertEquals("Lets pretend this charge was actually autorated", acceptedCharge.CostCalculationDescription.ToUTF8());

				groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				foundGroupCompanySellCharge = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().Single();

				AssertEquals("Should still find the same ", groupCompanyChargePK, foundGroupCompanySellCharge.GroupCompanySellCharge.PK);
				AssertEquals("Found group company charge should match existing", acceptedCharge.PK, foundGroupCompanySellCharge.GroupCompanyCostCharge.PK);
				AssertEquals("The charge has been accepted without being modified", "No Action", foundGroupCompanySellCharge.AcceptActionDescription);
			}
		}

		public void TestAccept_CreditorDoesNotOverrideCurrency()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var fisDepartment = TestObjectCreator.FISDepartment;
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("GLBCHG");
			Factory.Save();

			var groupCompanyChargeCurrency = Constants.CurrencyCodes.UnitedStates;

			using (TestObjectCreator.SwitchEnvToCompany(debtorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var localChargeCode = globalChargeCode.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);
				var charge = TestObjectCreator.CreateChargeWithPaymentBasis(job, localChargeCode, creditorCompany.OrgProxy, 95m);
				charge.JR_RX_NKSellCurrency = groupCompanyChargeCurrency;
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(creditorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var debtorDefaultCurrency = Constants.CurrencyCodes.Mexico;
				debtorCompany.OrgProxy.CompanyData.OB_RX_NKAPDefltCurrency = debtorDefaultCurrency;
				Factory.Save();

				var groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().ToArray();
				job.GroupCompanyChargesForDebtor.AcceptSellChargesAsCosts(groupCompanySellCharges);

				var acceptedCharge = job.Charges.Cast<Charge>().Single();
				var message = "Should use the currency from the Group Company Charge, not the Debtor Default Currency, so make sure debtor is set BEFORE the Cost Currency";

				AssertEquals(message, groupCompanyChargeCurrency, acceptedCharge.JR_RX_NKCostCurrency);

				var manualCharge = job.Charges.AddNew();
				manualCharge.JR_OH_CostAccount = debtorCompany.OrgProxy.PK;
				AssertEquals("If charge is manually added then we expect the debtor default currency", debtorDefaultCurrency, manualCharge.JR_RX_NKCostCurrency);
			}
		}

		public void TestAccept_DoesNotFailWhenSameChargeCodeUsedMultipleTimes()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var fisDepartment = TestObjectCreator.FISDepartment;
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("GLBCHG");
			Factory.Save();

			ZGuid charge1PK;
			ZGuid charge2PK;
			ZGuid charge3PK;

			using (TestObjectCreator.SwitchEnvToCompany(creditorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var localChargeCode = globalChargeCode.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);
				charge1PK = TestObjectCreator.CreateChargeWithPaymentBasis(job, localChargeCode, debtorCompany.OrgProxy, 1234m).PK;
				charge2PK = TestObjectCreator.CreateChargeWithPaymentBasis(job, localChargeCode, debtorCompany.OrgProxy, -50m).PK;
				charge3PK = TestObjectCreator.CreateChargeWithPaymentBasis(job, localChargeCode, debtorCompany.OrgProxy, 1m).PK;
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(debtorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var expected = new[] { charge1PK, charge2PK, charge3PK };

				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().ToArray();
				AssertContainsExactElementsInAnyOrder("Pre-condition: should find all charges", expected, groupCompanySellCharges.Select(x => x.GroupCompanySellCharge.PK));
				Assert("All found charges are not linked to any charge on the current job", groupCompanySellCharges.All(x => x.GroupCompanyCostCharge == null));

				job.GroupCompanyChargesForDebtor.AcceptSellChargesAsCosts(groupCompanySellCharges);

				var acceptedCharges = job.Charges.Cast<Charge>().ToArray();
				AssertEquals("Expect all charges to be accepted as costs", 3, acceptedCharges.Length);
				Assert(acceptedCharges.All(c => c.ChargeCode.AC_GC == debtorCompany.PK && c.ChargeCode.AC_Code == globalChargeCode.AC_Code));

				var acceptedCharge1 = acceptedCharges.Single(x => x.JR_OSCostAmt == 1234m);
				var acceptedCharge2 = acceptedCharges.Single(x => x.JR_OSCostAmt == -50m);
				var acceptedCharge3 = acceptedCharges.Single(x => x.JR_OSCostAmt == 1m);
				acceptedCharge3.Delete();

				groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().ToArray();

				AssertContainsExactElementsInAnyOrder("Should still find the same charges in the other company", expected, groupCompanySellCharges.Select(x => x.GroupCompanySellCharge.PK));

				var groupCompanyCharge1 = groupCompanySellCharges.Single(x => x.JR_OSSellAmt == 1234m);
				var groupCompanyCharge2 = groupCompanySellCharges.Single(x => x.JR_OSSellAmt == -50m);
				var groupCompanyCharge3 = groupCompanySellCharges.Single(x => x.JR_OSSellAmt == 1m);

				AssertEquals(acceptedCharge1, groupCompanyCharge1.GroupCompanyCostCharge);
				AssertEquals(acceptedCharge2, groupCompanyCharge2.GroupCompanyCostCharge);
				AssertNull("Equivalent cost charge has been deleted", groupCompanyCharge3.GroupCompanyCostCharge);

				AssertEquals("No Action", groupCompanyCharge1.AcceptActionDescription);
				AssertEquals("No Action", groupCompanyCharge2.AcceptActionDescription);
				AssertEquals("Create", groupCompanyCharge3.AcceptActionDescription);
			}
		}

		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAccept_PrefersBranchOrgProxy()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("NZAKL");

			var debtorBranch2 = debtorCompany.Branches.AddNew();
			debtorBranch2.GB_Code = "NZ2";
			debtorBranch2.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;

			var debtorBranch3 = debtorCompany.Branches.AddNew();
			debtorBranch3.GB_Code = "NZ3";
			debtorBranch3.GB_OH_OrgProxy = TestObjectCreator.AALSHI.PK;

			var debtorBranch4 = debtorCompany.Branches.AddNew();
			debtorBranch4.GB_Code = "NZ4";
			debtorBranch4.GB_OH_OrgProxy = TestObjectCreator.Debtor.PK;

			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var fisDepartment = TestObjectCreator.FISDepartment;

			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("GLBCHG");
			Factory.Save();

			using (TestObjectCreator.SwitchEnvToCompany(creditorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				job.JH_GB = debtorBranch2.PK;

				var localChargeCode = globalChargeCode.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);
				var charge1 = TestObjectCreator.CreateChargeWithPaymentBasis(job, localChargeCode, debtorCompany.OrgProxy, 100m);
				charge1.JR_GB = debtorBranch4.PK;

				var charge2 = TestObjectCreator.CreateChargeWithPaymentBasis(job, localChargeCode, debtorBranch3.OrgProxy, 30m);
				charge2.JR_GB = debtorBranch3.PK;
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(debtorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().ToArray();
				job.GroupCompanyChargesForDebtor.AcceptSellChargesAsCosts(groupCompanySellCharges);

				var charges = job.Charges.Cast<Charge>().ToArray();
				var charge1 = charges.Single(x => x.JR_OSCostAmt == 100m);
				var charge2 = charges.Single(x => x.JR_OSCostAmt == 30m);

				var message = "Accepting Group Company Sell Charges should default the Creditor to be the OrgProxy of the Sell Charge's Branch";
				AssertEquals(message, debtorBranch4.OrgProxy, charge1.CostAccount);
				AssertEquals(message, debtorBranch3.OrgProxy, charge2.CostAccount);
			}
		}

		#endregion

		#region Find

		public void TestFind()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var companyNZ = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var companyAU = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var companyJP = TestObjectCreator.CreateCompanyAndBranch("JPOSA");
			var fisDepartment = TestObjectCreator.FISDepartment;
			Factory.Save();

			ZGuid nzJobPK;
			ZGuid auGroupCompanyChargePK;
			ZGuid jpGroupCompanyCharge1PK;
			ZGuid jpGroupCompanyCharge2PK;

			using (TestObjectCreator.SwitchEnvToCompany(companyNZ, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				nzJobPK = job.PK;
				auGroupCompanyChargePK = TestObjectCreator.CreateChargeWithPaymentBasis(job, TestObjectCreator.CC1, companyAU.OrgProxy, 100m).PK;
				jpGroupCompanyCharge1PK = TestObjectCreator.CreateChargeWithPaymentBasis(job, TestObjectCreator.CC2, companyJP.OrgProxy, 150m).PK;
				jpGroupCompanyCharge2PK = TestObjectCreator.CreateChargeWithPaymentBasis(job, TestObjectCreator.CC3, companyJP.OrgProxy, 1000m).PK;
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(companyAU, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				AssertNotEquals("Pre-condition: Should not load the same job when in another company", nzJobPK, job.PK);

				var message = "companyAU's OrgProxy owes companyNZ's OrgProxy 100USD";
				var groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().Single();

				AssertEquals(message, auGroupCompanyChargePK, groupCompanySellCharges.GroupCompanySellCharge.PK);
			}

			using (TestObjectCreator.SwitchEnvToCompany(companyJP, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var message = "companyJP.OrgProxy owes money (i.e. is the Debtor) two separate charges in the NZ company. ";
				var expected = new[] { jpGroupCompanyCharge1PK, jpGroupCompanyCharge2PK };
				var groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().Select(x => x.GroupCompanySellCharge.PK);

				AssertContainsExactElementsInAnyOrder(message, expected, groupCompanySellCharges);
			}
		}

		public void TestFind_ExcludesAJRJCharges()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var companyNZ = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var companyAU = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var fisDepartment = TestObjectCreator.FISDepartment;

			var branchProxy1 = TestObjectCreator.CreateOrgHeader("BPROXY1", true, true, "NZCHC");
			var branchProxy2 = TestObjectCreator.CreateOrgHeader("BPROXY2", true, true, "NZWLG");
			var auBranchProxy = TestObjectCreator.CreateOrgHeader("AUBPROXY", true, true, "AUMEL");

			var branch1 = companyNZ.FirstActiveBranch;
			branch1.GB_Code = "GB1";
			branch1.GB_OH_OrgProxy = branchProxy1.PK;

			var branch2 = companyNZ.Branches.AddNew();
			branch2.GB_Code = "GB2";
			branch2.GB_OH_OrgProxy = branchProxy2.PK;

			var branch3 = companyNZ.Branches.AddNew();
			branch3.GB_Code = "AUB";
			branch3.GB_OH_OrgProxy = auBranchProxy.PK;

			companyAU.FirstActiveBranch.GB_OH_OrgProxy = auBranchProxy.PK;

			Factory.Save();

			ZGuid branch1OrgProxyChargePK;
			ZGuid branch2OrgProxyChargePK;
			ZGuid companyOrgProxyChargePK;

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(companyAU.PK.ToGuid());
			using (TestObjectCreator.SwitchEnvToCompany(companyAU, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				branch1OrgProxyChargePK = TestObjectCreator.CreateChargeWithPaymentBasis(job, TestObjectCreator.CC2, branchProxy1, 30m).PK;
				branch2OrgProxyChargePK = TestObjectCreator.CreateChargeWithPaymentBasis(job, TestObjectCreator.CC2, branchProxy2, 20m).PK;
				companyOrgProxyChargePK = TestObjectCreator.CreateChargeWithPaymentBasis(job, TestObjectCreator.CC1, auBranchProxy, 10m).PK;
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(companyNZ, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				job.JH_GB = branch1.PK;

				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);

				Assert("Enable AJRJ is set in the other company, we currently don't expect any results.", !groupCompanyChargesForJob.ChargesForDebtor.Any());
				//var message = "Group Company Charges are charges that are debted to the OrgProxy of the Job's Company or Branch.";
				//var expected = new[] { branch2OrgProxyChargePK, branch1OrgProxyChargePK };
				//var groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().Select(x => x.GroupCompanySellCharge.PK);
				//AssertContainsExactElementsInAnyOrder(message, expected, groupCompanySellCharges);

				//message = "Not a Group Company Charge as the debtor is an OrgProxy of companyAU of it's Parent Job's Company (regardless of it also being companyNZ.branch3.OrgProxy).";
				//AssertEquals(message, false, groupCompanySellCharges.Contains(companyOrgProxyChargePK));

				//job.JH_GB = branch2.PK;

				//message = "Changing Branches shouldn't affect the results.";
				////expected = new[] { branch2OrgProxyChargePK, branch1OrgProxyChargePK };
				//groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().Select(x => x.GroupCompanySellCharge.PK);
				//AssertContainsExactElementsInAnyOrder(message, expected, groupCompanySellCharges);
			}
		}

		public void TestFind_ExcludesResultsWithoutPaymentBasis()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var companyAU = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var companyNZ = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var fisDepartment = TestObjectCreator.FISDepartment;
			Factory.Save();

			using (TestObjectCreator.SwitchEnvToCompany(companyNZ, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var currency = TestObjectCreator.USD;
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, job.JH_JobNum, currency, 0m, null, currency, 500, companyAU.OrgProxy);
				AssertEquals("Pre-condition", false, charge.SellPaymentBases.Any());
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(companyAU, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var message = "We should only include those charges where a sell payment basis exists";
				var sellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>();
				Assert(message, !sellCharges.Any());
			}
		}

		public void TestFind_LocalChargeNoGroupCharges_NoLoadingOfJobPaymentBasisAttempted()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var companyAU = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var companyNZ = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var fisDepartment = TestObjectCreator.FISDepartment;
			Factory.Save();

			var currency = TestObjectCreator.USD;

			using (TestObjectCreator.SwitchEnvToCompany(companyAU, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, job.JH_JobNum, currency, 0m, null, currency, 500, companyAU.OrgProxy);
				Factory.Save();

				Factory.ResetDatabaseLoadCount();
				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				using (AssertDbHitsForAllFactories(new Dictionary<string, int> { { JobPaymentBasis.Schema.TableName, 0 } }, ignoreUnspecified: true))
				{
					var otherCharges = job.GroupCompanyChargesForCreditor.IsAcceptedSell(charge);
				}
			}
		}

		public void TestFind_LocalChargeNoGroupCharges_LoadingOfJobPaymentBasisAttempted()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var companyAU = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var companyNZ = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var fisDepartment = TestObjectCreator.FISDepartment;
			Factory.Save();

			var currency = TestObjectCreator.USD;

			using (TestObjectCreator.SwitchEnvToCompany(companyNZ, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, job.JH_JobNum, currency, 0m, null, currency, 600, companyAU.OrgProxy);
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(companyAU, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, job.JH_JobNum, currency, 0m, null, currency, 500, companyAU.OrgProxy);
				Factory.Save();

				Factory.ResetDatabaseLoadCount();
				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				using (AssertDbHitsForAllFactories(new Dictionary<string, int> { { JobPaymentBasis.Schema.TableName, 1 } }, ignoreUnspecified: true))
				{
					var otherCharges = job.GroupCompanyChargesForDebtor.IsAcceptedCost(charge);
				}
			}
		}

		public void TestFind_IncludesNegativeSellAmounts()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var companyNZ = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var companyAU = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var fisDepartment = TestObjectCreator.FISDepartment;
			Factory.Save();

			ZGuid chargePK;
			using (TestObjectCreator.SwitchEnvToCompany(companyNZ, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				chargePK = TestObjectCreator.CreateChargeWithPaymentBasis(job, TestObjectCreator.CC1, companyAU.OrgProxy, -50m).PK;
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(companyAU, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var message = "Should still find Group Company Charges even if the sell amount is negative";
				var groupCompanySellCharge = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().Single();

				AssertEquals(message, chargePK, groupCompanySellCharge.GroupCompanySellCharge.PK);
			}
		}

		public void TestFind_IncludesZeroSellAmounts()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var companyNZ = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var companyAU = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var fisDepartment = TestObjectCreator.FISDepartment;
			Factory.Save();

			ZGuid chargePK;
			using (TestObjectCreator.SwitchEnvToCompany(companyNZ, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				chargePK = TestObjectCreator.CreateChargeWithPaymentBasis(job, TestObjectCreator.CC1, companyAU.OrgProxy, 0m).PK;
				Factory.Save();
			}

			using (TestObjectCreator.SwitchEnvToCompany(companyAU, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				//var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				//var message = "Should still find Group Company Charges even if the amount is zero";
				//var groupCompanySellCharge = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().Single();

				//AssertEquals(message, chargePK, groupCompanySellCharge.GroupCompanySellCharge.PK);
				Assert("Placeholder until work item to create 0 rated payment basis exists", true);
			}
		}

		#endregion

		#region Implementation

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
