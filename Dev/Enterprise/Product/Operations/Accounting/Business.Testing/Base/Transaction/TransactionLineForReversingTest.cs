using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.Base.Transaction
{
	[TestedType(typeof(TransactionLineForReversing))]
	public class TransactionLineForReversingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWrappedBusinessEntity()
		{
			var wip = Factory.New<WIP>();
			var wrapper = new TransactionLineForReversing(wip);
			AssertEquals(wip, wrapper.WrappedBusinessEntity);
		}

		public void TestRunPreSaveValidation()
		{
			var wip = Factory.New<WIP>();
			var wrapper = new TransactionLineForReversing(wip);
			Assert("Precondition: WIP has no errors.", !wip.HasErrors);

			wrapper.RunPreSaveValidation();
			Assert("WIP must have errors after wrapper validation.", wip.HasErrors);
			Assert("The wrapper must have errors if WIP has errors.", wrapper.HasErrors);
		}

		[TestDate(2019, 05, 20)]
		public void TestTransactionLineForReversingProperties()
		{
			var objectCreator = new TestObjectCreator(Factory);
			objectCreator.AALSHI.OH_IsDebtor = true;
			objectCreator.ABIGAS.OH_IsCreditor = true;
			var positiveWIP = objectCreator.CreateWIP(objectCreator.Job1, objectCreator.CC1, 1M, "Desc1", 12m, null, objectCreator.AALSHI);
			var positiveACR = objectCreator.CreateAccrual(objectCreator.Job1, objectCreator.CC2, 1M, "Desc1", 789m, null, objectCreator.ABIGAS);
			positiveACR.AL_OSExTaxAmount = 789m;
			var negativeWIP = objectCreator.CreateWIP(objectCreator.Job1, objectCreator.CC3, 1M, "Desc1", -456m, null, objectCreator.AALSHI);
			var negativeACR = objectCreator.CreateAccrual(objectCreator.Job1, objectCreator.CC4, 1M, "Desc1", -123m, null, objectCreator.ABIGAS);
			negativeACR.AL_OSExTaxAmount = -123m;
			Factory.Save();

			var wrapperForPositiveWIP = new TransactionLineForReversing(positiveWIP);
			AssertEquals(ZDateTime.Today, wrapperForPositiveWIP.PostDate);
			AssertEquals(objectCreator.Job1.JH_JobNum, wrapperForPositiveWIP.JobNumber);
			AssertEquals(GlbBranch.CurrentBranch.PK, wrapperForPositiveWIP.BranchPK);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, wrapperForPositiveWIP.DepartmentPK);
			AssertEquals(objectCreator.AALSHI.PK, wrapperForPositiveWIP.OrganizationPK);
			AssertEquals(objectCreator.CC1.PK, wrapperForPositiveWIP.ChargeCodePK);
			AssertEquals(objectCreator.GLHeader1.PK, wrapperForPositiveWIP.GLHeaderPK);
			AssertEquals(12m, wrapperForPositiveWIP.Amount);
			AssertEquals(Env.CurrentUser.FullName, wrapperForPositiveWIP.CreatingUser);
			AssertEquals(ZDateTime.Today, wrapperForPositiveWIP.CreatedDate.Date);
			AssertEquals(ZDateTime.Empty, wrapperForPositiveWIP.ReverseDate);
			AssertEquals(TransactionLineTypes.WIP, wrapperForPositiveWIP.TransactionType);

			var wrapperForPostiveACR = new TransactionLineForReversing(positiveACR);
			AssertEquals(ZDateTime.Today, wrapperForPostiveACR.PostDate);
			AssertEquals(objectCreator.Job1.JH_JobNum, wrapperForPostiveACR.JobNumber);
			AssertEquals(GlbBranch.CurrentBranch.PK, wrapperForPostiveACR.BranchPK);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, wrapperForPostiveACR.DepartmentPK);
			AssertEquals(objectCreator.ABIGAS.PK, wrapperForPostiveACR.OrganizationPK);
			AssertEquals(objectCreator.CC2.PK, wrapperForPostiveACR.ChargeCodePK);
			AssertEquals(objectCreator.GLHeader1.PK, wrapperForPostiveACR.GLHeaderPK);
			AssertEquals(-789m, wrapperForPostiveACR.Amount);
			AssertEquals(Env.CurrentUser.FullName, wrapperForPostiveACR.CreatingUser);
			AssertEquals(ZDateTime.Today, wrapperForPostiveACR.CreatedDate.Date);
			AssertEquals(ZDateTime.Empty, wrapperForPostiveACR.ReverseDate);
			AssertEquals(TransactionLineTypes.Accrual, wrapperForPostiveACR.TransactionType);

			var wrapperForNegativeWIP = new TransactionLineForReversing(negativeWIP);
			AssertEquals(ZDateTime.Today, wrapperForNegativeWIP.PostDate);
			AssertEquals(objectCreator.Job1.JH_JobNum, wrapperForNegativeWIP.JobNumber);
			AssertEquals(GlbBranch.CurrentBranch.PK, wrapperForNegativeWIP.BranchPK);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, wrapperForNegativeWIP.DepartmentPK);
			AssertEquals(objectCreator.AALSHI.PK, wrapperForNegativeWIP.OrganizationPK);
			AssertEquals(objectCreator.CC3.PK, wrapperForNegativeWIP.ChargeCodePK);
			AssertEquals(objectCreator.GLHeader1.PK, wrapperForNegativeWIP.GLHeaderPK);
			AssertEquals(-456m, wrapperForNegativeWIP.Amount);
			AssertEquals(Env.CurrentUser.FullName, wrapperForNegativeWIP.CreatingUser);
			AssertEquals(ZDateTime.Today, wrapperForNegativeWIP.CreatedDate.Date);
			AssertEquals(ZDateTime.Empty, wrapperForNegativeWIP.ReverseDate);
			AssertEquals(TransactionLineTypes.WIP, wrapperForNegativeWIP.TransactionType);

			var wrapperForNegativeACR = new TransactionLineForReversing(negativeACR);
			AssertEquals(ZDateTime.Today, wrapperForNegativeACR.PostDate);
			AssertEquals(objectCreator.Job1.JH_JobNum, wrapperForNegativeACR.JobNumber);
			AssertEquals(GlbBranch.CurrentBranch.PK, wrapperForNegativeACR.BranchPK);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, wrapperForNegativeACR.DepartmentPK);
			AssertEquals(objectCreator.ABIGAS.PK, wrapperForNegativeACR.OrganizationPK);
			AssertEquals(objectCreator.CC4.PK, wrapperForNegativeACR.ChargeCodePK);
			AssertEquals(objectCreator.GLHeader1.PK, wrapperForNegativeACR.GLHeaderPK);
			AssertEquals(123m, wrapperForNegativeACR.Amount);
			AssertEquals(Env.CurrentUser.FullName, wrapperForNegativeACR.CreatingUser);
			AssertEquals(ZDateTime.Today, wrapperForNegativeACR.CreatedDate.Date);
			AssertEquals(ZDateTime.Empty, wrapperForNegativeACR.ReverseDate);
			AssertEquals(TransactionLineTypes.Accrual, wrapperForNegativeACR.TransactionType);
		}

		public void TestReverseDateReadOnly()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var shipment = objectCreator.CreateShipment("S10002");
			var job = objectCreator.CreateJob(shipment);
			var wip = objectCreator.CreateWIP(objectCreator.Job1, objectCreator.CC1, 1M, "Desc1", 100M, null, objectCreator.AALSHI);
			Factory.Save();

			var wrapper = new TransactionLineForReversing(wip);
			Assert("Pre-condition", !wip.HasRowErrors);
			Assert(!wrapper.ReverseDate_ReadOnly);

			wip.AddRowError("Error only for testing");

			Assert("Pre-condition", wip.HasRowErrors);
			Assert(wrapper.ReverseDate_ReadOnly);
		}

		public void TestReverseDateReadOnlyForSecurityRightAllowBackPosting()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var wip = objectCreator.CreateWIP(objectCreator.Job1, objectCreator.CC1, 1M, "Desc1", 100M, null, objectCreator.AALSHI);
			Factory.Save();

			var wrapper = new TransactionLineForReversing(wip);
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Assert("Pre-condition", !wip.HasRowErrors);
			Assert("Pre-condition", !wip.IsEditingReverseDateAllowed);
			Assert(wrapper.ReverseDate_ReadOnly);

			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Assert("Pre-condition", !wip.HasRowErrors);
			Assert("Pre-condition", wip.IsEditingReverseDateAllowed);
			Assert(!wrapper.ReverseDate_ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var wip = Factory.New<WIP>();
			return new TransactionLineForReversing(wip);
		}
	}
}

