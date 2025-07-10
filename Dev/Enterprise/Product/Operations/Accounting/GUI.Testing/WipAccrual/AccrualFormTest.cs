using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.WipAccrual.Testing
{
	[TestedType(typeof(AccrualForm))]
	public class AccrualFormTest : WIPAccrualFormTest
	{
		protected override void BashControl(Control controlToBash)
		{
			base.BashControl(controlToBash);
			if (controlToBash is AccrualForm)
			// Restore original value to pass Validation that reports Developer Exception
			{
				((AccrualForm)controlToBash).WipAccrual.AL_JH = (ZGuid)((AccrualForm)controlToBash).WipAccrual.AL_JHInfo.OriginalValue;
				((AccrualForm)controlToBash).WipAccrual.AL_GC = GlbCompany.CurrentCompany.PK;
				TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(((AccrualForm)controlToBash).WipAccrual);
			}
		}

		protected override Form GetFormToBashCore()
		{
			Business.WIPAccrual.Accrual accrual = Factory.New<Business.WIPAccrual.Accrual>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_APLine = accrual.PK;
			accrual.AL_JH = charge.JR_JH;
			accrual.AL_GC = GlbCompany.CurrentCompany.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual);
			Factory.Save();
			return new AccrualForm(accrual);
		}

		protected override WIPAccrualForm GetForm(ref ICriticalValidation criticalValidation, bool saveInDB = true)
		{
			Business.WIPAccrual.Accrual accrual = Factory.New<Business.WIPAccrual.Accrual>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_APLine = accrual.PK;
			accrual.AL_JH = charge.JR_JH;
			accrual.AL_GC = GlbCompany.CurrentCompany.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual);
			if (saveInDB)
			{
				Factory.Save();
			}

			var bizo = Factory.Load<DummyAccrualCriticalValidationParent>(accrual.PK);
			bizo.AL_Desc = "NEW DESC";
			criticalValidation = bizo.CriticalValidation;

			var form = new AccrualFormTestForCriticalValidationException(bizo);
			return form;
		}

		protected override void ForceDelete(WIPAccrualForm form, bool forcedelete)
		{
			var bizo = (form.BusinessEntity as DummyAccrualCriticalValidationParent);
			bizo.ThrowCriticalValidationError = true;
			bizo.ForceDeleted = forcedelete;
		}

		class AccrualFormTestForCriticalValidationException : AccrualForm
		{
			public AccrualFormTestForCriticalValidationException(DummyAccrualCriticalValidationParent bizo)
				: base(bizo)
			{
			}
		}

		class DummyAccrualCriticalValidationParent : Business.WIPAccrual.Accrual, ISupportCriticalValidation
		{
			public DummyAccrualCriticalValidationParent(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public bool ThrowCriticalValidationError = true;
			public bool ForceDeleted;

			public new void OnSaving()
			{
				base.OnSaving();
				CriticalValidation.RegisterOnSavingCheck();
			}

			public override void Delete()
			{
			}

			public override bool IsDeleted
			{
				get
				{
					return ForceDeleted;
				}
			}

			#region ISupportCriticalValidation Members

			public ICriticalValidation CriticalValidation
			{
				get { return new DummyCriticalValidation(this); }
			}

			#endregion

		}

		class DummyCriticalValidation : CriticalValidation<DummyAccrualCriticalValidationParent>
		{
			public DummyCriticalValidation(DummyAccrualCriticalValidationParent parent)
				: base(parent)
			{
			}

			protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
			{
				if (Parent.ThrowCriticalValidationError)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.DummyErrorKeyForTest, ResString.GetMultilingualString("d6217571-847f-472f-9347-47ec07e7c256", "Test error message."), "E=MC2");
				}
			}

			protected override IEnumerable<CriticalValidationResult> DeletedObjectOnSavingCriticalChecks()
			{
				if (Parent.ThrowCriticalValidationError)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.DummyErrorKeyForTest, ResString.GetMultilingualString("d6217571-847f-472f-9347-47ec07e7c256", "Test error message."), "E=MC2");
				}
			}
		}
	}
}
