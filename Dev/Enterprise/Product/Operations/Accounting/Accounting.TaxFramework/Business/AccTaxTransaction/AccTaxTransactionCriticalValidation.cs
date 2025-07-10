using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxTransactionCriticalValidation : CriticalValidation<AccTaxTransaction>
	{
		public AccTaxTransactionCriticalValidation(AccTaxTransaction parent) : base(parent)
		{
			validator = ObjectFactory.Get<ITaxFrameworkDependencyFactory>().GetAccTaxTransactionCriticalValidator(parent);
		}

		readonly IAccTaxTransactionCriticalValidator validator;

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			yield return validator.CheckAmounts();
			yield return validator.CheckCancelledNonSPRTransactionRealisationDate();
			yield return validator.CheckLinkedGLMovements();
			yield return validator.CheckConsistencyWithPivotsData();
		}

		protected override IEnumerable<CriticalValidationResult> AfterSavingCriticalChecks()
		{
			foreach (var result in base.AfterSavingCriticalChecks())
			{
				yield return result;
			}

			yield return validator.CheckHasSkippedDataRefreshBusUpdate();
		}
	}
}
