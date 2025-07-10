using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public interface IProfitShareChargeCreator
	{
		void RunPreCreateValidation();

		bool CreateCharges();

		ZString ValidationErrors { get; }
	}

	public abstract class ProfitShareChargeCreator : IProfitShareChargeCreator
	{
		protected int CreatedChargesCount;

		public bool CreateCharges()
		{
			return CreateCharges(true);
		}

		public bool CreateCharges(bool shouldMergeWithSaveChargeResult)
		{
			var chargesUpdatedOrCreated = false;

			CreatedChargesCount = 0;

			if (ProfitShareChargeCodePK != Guid.Empty)
			{
				chargesUpdatedOrCreated = CreateChargesCore();
				AddLogToParent(chargesUpdatedOrCreated, CreatedChargesCount);
				if (chargesUpdatedOrCreated)
				{
					ParentObject?.Factory?.SetContext(BusinessContext.CreatingProfitShareCharge);
				}

				var saveChargeResult = SaveCharges();
				if (shouldMergeWithSaveChargeResult)
				{
					chargesUpdatedOrCreated &= saveChargeResult;
				}
			}

			return chargesUpdatedOrCreated;
		}

		public ZString ValidationErrors => ValidationErrorAccumulator.ToStringWithNewLineBetweenAppends();
		protected readonly ZStringBuilder ValidationErrorAccumulator = new ZStringBuilder();

		public void RunPreCreateValidation()
		{
			RunPreCreationValidationCore();
		}

		protected virtual void RunPreCreationValidationCore()
		{
		}

		protected virtual ZBool SaveCharges()
		{
			return true;
		}

		protected virtual void AddLogToParent(bool chargesUpdatedOrCreated, int createdChargesCount)
		{
			if (chargesUpdatedOrCreated && ParentObject != null)
			{
				var shortLog = string.Format((NoResString)"Profit share charges created: {0}", createdChargesCount);
				ParentObject.GetLogs().AddNew(Events.ProfitShareCalculated, shortLog);  //to be added
			}
		}

		protected abstract bool CreateChargesCore();

		protected abstract BusinessObject ParentObject { get; }

		protected virtual Guid ProfitShareChargeCodePK => AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
	}
}
