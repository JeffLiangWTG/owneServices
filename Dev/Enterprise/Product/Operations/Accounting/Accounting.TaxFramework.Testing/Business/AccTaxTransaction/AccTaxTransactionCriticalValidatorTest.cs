using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	public class AccTaxTransactionCriticalValidatorTest : TestCaseWithFactory
	{
		public void TestCheckCancelledNonSPRTransactionRealisationDate()
		{
			AccTaxTransaction taxRecord;
			IAccTaxTransactionCriticalValidator validator;

			var nonPTMBasises = new TaxBasisList().Cast<ICodeDescription>().Select(x => x.Code).Where(x => (x != TaxBasisList.PostingOnMatching.Code)).ToArray();
			Assert(nonPTMBasises.Length > 0);
			foreach (var basis in nonPTMBasises)
			{
				setupTest();
				taxRecord.ATT_Basis = basis;
				taxRecord.ATT_IsCancelled = true;
				Assert("Precondition: IsInDatabase", !taxRecord.IsInDatabase);
				AssertRealisationDateForTaxTransaction(expectErrorOnEmptyDate: true);
			}

			setupTest();
			taxRecord.ATT_Basis = TaxBasisList.PostingOnMatching.Code;
			taxRecord.ATT_IsCancelled = true;
			AssertRealisationDateForTaxTransaction(expectErrorOnEmptyDate: false);

			setupTest();
			taxRecord.ATT_Basis = TaxBasisList.PostingOnMatching.Code;
			taxRecord.ATT_IsCancelled = false;
			AssertRealisationDateForTaxTransaction(expectErrorOnEmptyDate: false);

			setupTest();
			taxRecord.ATT_Basis = TaxBasisList.Matching.Code;
			taxRecord.ATT_IsCancelled = true;
			((INeedRow)taxRecord).Row.AcceptChanges();
			Assert("Precondition: IsInDatabase", taxRecord.IsInDatabase);
			Assert("Precondition: ATT_IsCancelledInfo.HasChanges", !taxRecord.ATT_IsCancelledInfo.HasChanges);
			AssertRealisationDateForTaxTransaction(expectErrorOnEmptyDate: false);

			setupTest();
			taxRecord.ATT_Basis = TaxBasisList.Matching.Code;
			((INeedRow)taxRecord).Row.AcceptChanges();
			taxRecord.ATT_IsCancelled = true;
			Assert("Precondition: IsInDatabase", taxRecord.IsInDatabase);
			Assert("Precondition: ATT_IsCancelledInfo.HasChanges", taxRecord.ATT_IsCancelledInfo.HasChanges);
			AssertRealisationDateForTaxTransaction(expectErrorOnEmptyDate: true);

			void AssertRealisationDateForTaxTransaction(bool expectErrorOnEmptyDate)
			{
				taxRecord.ATT_RealisationDate = ZDate.Empty;
				if (expectErrorOnEmptyDate)
				{
					CriticalValidationResultTestHelper.AssertCriticalValidationResult(new TestCaseDefinition_ForSeparateTestsMethods("CancelledTaxTransactionRealisationDateShouldNotBeEmpty", true,
						CriticalValidationErrorType.CancelledTaxTransactionRealisationDateShouldNotBeEmpty, "Canceled transaction has empty realization date.", $"PK = {taxRecord.PK}", "Properties:"),
						validator.CheckCancelledNonSPRTransactionRealisationDate());
				}
				else
				{
					AssertEquals(null, validator.CheckCancelledNonSPRTransactionRealisationDate());
				}

				taxRecord.ATT_RealisationDate = ZDate.Today.AddDays(-2);
				AssertEquals(null, validator.CheckCancelledNonSPRTransactionRealisationDate());
			}

			void setupTest()
			{
				taxRecord = Factory.New<AccTaxTransaction>();
				validator = new AccTaxTransactionCriticalValidator(taxRecord);
			}
		}
	}
}
