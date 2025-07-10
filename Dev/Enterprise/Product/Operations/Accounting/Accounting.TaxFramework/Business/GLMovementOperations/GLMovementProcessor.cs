using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface IGLMovementProcessor
	{
		void CreateGLMovements(params AccTaxTransaction[] taxRecords);
		void DeleteGLMovementsNotInDB(params AccTaxTransaction[] taxRecords);
	}

	class GLMovementProcessor : IGLMovementProcessor
	{
		public GLMovementProcessor()
		{
			glMovementCreator_constructorInitializedOnly = new GLMovementCreator();
		}

		void IGLMovementProcessor.CreateGLMovements(params AccTaxTransaction[] taxRecords)
		{
			foreach (var taxRecord in taxRecords)
			{
				if (taxRecord.ATT_LocalTaxAmount != 0)
				{
					CreatePostingGLMovements(taxRecord);
					CreateRealisationGLMovements(taxRecord);
				}
			}
		}

		void IGLMovementProcessor.DeleteGLMovementsNotInDB(params AccTaxTransaction[] taxRecords)
		{
			LoadGLMovements(taxRecords).Where(x => !x.IsInDatabase).ForEach(x => x.Delete());
		}

		static AccTaxGLMovement[] LoadGLMovements(params AccTaxTransaction[] taxRecords)
		{
			if (taxRecords.Length == 0)
			{
				return System.Array.Empty<AccTaxGLMovement>();
			}

			var query = new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxRecords.Select(x => x.PK));
			if (taxRecords.All(x => !x.IsInDatabase))
			{
				query.FetchOnlyFromLocalCache = true;
			}

			return taxRecords[0].Factory.Load<AccTaxGLMovement>(query);
		}

		void CreatePostingGLMovements(AccTaxTransaction taxRecord)
		{
			if (!taxRecord.IsInDatabase)
			{
				if (taxRecord.ATT_Basis == TaxBasisList.Posting.Code)
				{
					GLMovementCreator.CreateNormalRecord(taxRecord, GLMovementDateSource.TaxRecordPostDate);
				}
				else if (taxRecord.ATT_Basis == TaxBasisList.Matching.Code)
				{
					GLMovementCreator.CreatePendingRecord(taxRecord);
				}
			}
		}

		void CreateRealisationGLMovements(AccTaxTransaction taxRecord)
		{
			if (taxRecord.ATT_RealisationDate.IsValid && (!taxRecord.IsInDatabase || taxRecord.ATT_RealisationDateInfo.HasChanges))
			{
				if (taxRecord.ATT_Basis == TaxBasisList.Matching.Code)
				{
					GLMovementCreator.CreateRealisedRecord(taxRecord);
				}
				else if (taxRecord.ATT_Basis == TaxBasisList.PostingOnMatching.Code)
				{
					GLMovementCreator.CreateNormalRecord(taxRecord, GLMovementDateSource.TaxRecordRealisationDate);
				}
			}
		}

		IGLMovementCreator GLMovementCreator => glMovementCreator_constructorInitializedOnly;
		IGLMovementCreator glMovementCreator_constructorInitializedOnly;

#if DEBUG
		public void SubstituteGLMovementCreator_ForTestOnly(IGLMovementCreator replacement) => glMovementCreator_constructorInitializedOnly = replacement;
		public IGLMovementCreator GLMovementCreator_ExposedForTestOnly => GLMovementCreator;
#endif
	}
}
