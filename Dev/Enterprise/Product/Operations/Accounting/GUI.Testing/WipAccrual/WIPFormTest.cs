using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.WipAccrual.Testing
{
	[TestedType(typeof(WIPForm))]
	public class WIPFormTest : WIPAccrualFormTest
	{
		protected override void BashControl(Control controlToBash)
		{
			base.BashControl(controlToBash);
			if (controlToBash is WIPForm)
			// Restore original value to pass Validation that reports Developer Exception
			{
				((WIPForm)controlToBash).WipAccrual.AL_JH = (ZGuid)((WIPForm)controlToBash).WipAccrual.AL_JHInfo.OriginalValue;
				TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(((WIPForm)controlToBash).WipAccrual);
			}
		}

		protected override Form GetFormToBashCore()
		{
			Business.WIPAccrual.WIP wip = Factory.New<Business.WIPAccrual.WIP>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = wip.PK;
			wip.AL_JH = charge.JR_JH;
			wip.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			return new WIPForm(wip);
		}

		protected override WIPAccrualForm GetForm(ref ICriticalValidation criticalValidation, bool saveInDB = true)
		{
			Business.WIPAccrual.WIP wip = Factory.New<Business.WIPAccrual.WIP>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = wip.PK;
			wip.AL_JH = charge.JR_JH;
			wip.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wip);

			if (saveInDB)
			{
				Factory.Save();
			}

			var bizo = Factory.Load<DummyWIPCriticalValidationParent>(wip.PK);
			bizo.AL_Desc = "NEW DESC";
			criticalValidation = bizo.CriticalValidation;

			var form = new WIPFormTestForCriticalValidationException(bizo);
			return form;
		}

		protected override void ForceDelete(WIPAccrualForm form, bool forcedelete)
		{
			var bizo = (form.BusinessEntity as DummyWIPCriticalValidationParent);
			bizo.ThrowCriticalValidationError = true;
			bizo.ForceDeleted = forcedelete;
		}

		class WIPFormTestForCriticalValidationException : WIPForm
		{
			public WIPFormTestForCriticalValidationException(DummyWIPCriticalValidationParent bizo)
				: base(bizo)
			{
			}
		}

		class DummyWIPCriticalValidationParent : Business.WIPAccrual.WIP, ISupportCriticalValidation
		{
			public DummyWIPCriticalValidationParent(BusinessObjectFactory factory, System.Data.DataRow row)
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

		class DummyCriticalValidation : CriticalValidation<DummyWIPCriticalValidationParent>
		{
			public DummyCriticalValidation(DummyWIPCriticalValidationParent parent)
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
