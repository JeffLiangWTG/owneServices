using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class GroupCompanyChargeValidationTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCheckLocalChargeCode()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216");
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var fisDepartment = TestObjectCreator.FISDepartment;
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("GLBONLY");
			globalChargeCode.Factory.Save();
			Factory.Save();

			ZGuid groupCompanySellCharge1;
			ZGuid groupCompanySellCharge2;

			using (TestObjectCreator.SwitchEnvToCompany(creditorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly())
			{
				groupCompanySellCharge1 = TestObjectCreator.CreateChargeWithPaymentBasis(job, TestObjectCreator.FRT, debtorCompany.OrgProxy, 100m).PK;

				var localChargeCode = globalChargeCode.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);
				groupCompanySellCharge2 = TestObjectCreator.CreateChargeWithPaymentBasis(job, localChargeCode, debtorCompany.OrgProxy, 100m).PK;
			}

			using (TestObjectCreator.SwitchEnvToCompany(debtorCompany, department: fisDepartment))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly())
			{
				var localChargeCode = globalChargeCode.ChildChargeCodes.Single(c => c.AC_GC == Env.CurrentCompanyPK);
				localChargeCode.Delete();
				Factory.Save();

				var expected = new[] { groupCompanySellCharge1, groupCompanySellCharge2 };
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>();

				AssertContainsExactElementsInAnyOrder("Pre-condition", expected, groupCompanySellCharges.Select(x => x.GroupCompanySellCharge.PK));

				var groupCompanyCharge1 = groupCompanySellCharges.Single(x => x.GroupCompanySellCharge.PK == groupCompanySellCharge1);
				groupCompanyCharge1.Validation.ValidateAll();

				AssertHasError(groupCompanyCharge1.CostCompanyChargeCodePKInfo, "Charge Code FRT in Company CNZ could not be mapped to any Global Charge Code.");

				var groupCompanyCharge2 = groupCompanySellCharges.Single(x => x.GroupCompanySellCharge.PK == groupCompanySellCharge2);
				groupCompanyCharge2.Validation.ValidateAll();

				AssertHasError(groupCompanyCharge2.CostCompanyChargeCodePKInfo, "No mapping was found for Global Charge Code GLBONLY to any Local Charge Codes in this Company.");
			}
		}

		TestObjectCreator TestObjectCreator => new TestObjectCreator(Factory);
	}
}
