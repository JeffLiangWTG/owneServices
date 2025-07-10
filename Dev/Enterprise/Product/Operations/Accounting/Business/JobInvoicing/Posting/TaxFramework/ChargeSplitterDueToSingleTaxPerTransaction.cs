using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.TaxFramework
{
	public interface IChargeSplitterDueToSingleTaxPerTransaction
	{
		PostingChargeCollection GetSplitCharges(PostingChargeCollection postingCharges);
	}

	public class ChargeSplitterDueToSingleTaxPerTransaction : IChargeSplitterDueToSingleTaxPerTransaction
	{
		public ChargeSplitterDueToSingleTaxPerTransaction()
		{
			ITaxFrameworkConfigurationHelper configurationHelper = new TaxFrameworkConfigurationHelper();
			taxRecordCollectionValidator_constructorInitializedOnly = new TaxRecordCollectionValidator(configurationHelper);
		}

		PostingChargeCollection IChargeSplitterDueToSingleTaxPerTransaction.GetSplitCharges(PostingChargeCollection postingCharges)
		{
			if (postingCharges.Count > 0)
			{
				var chargesToAddDictionary = new Dictionary<PostingChargeKey, (IReceivablesPostingChargeCollection existingChargesCollection, List<IReceivablesPostingCharge> chargeToMove)>();

				var factory = new ReadOnlyBusinessObjectFactory();

				var company = GlbCompany.CurrentCompany;
				var taxSystemCodes = AccountingMasterFilesRegistry.Instance.TaxSystems.Value.Cast<TaxSystemsConfiguration>().Where(x => x.IsSingleRecordPerARTransactionRequired(company.GC_RN_NKCountryCode)).Select(x => x.Code).ToArray();

				if (taxSystemCodes.Any() && GlbCompany.CurrentCompany.IsEnabledForTaxFrameworkConfiguration(factory))
				{
					foreach (IReceivablesPostingChargeCollection charges in postingCharges)
					{
						var taxParent = new ReceivablesTaxParentFromPostingCharge(charges, factory);
						IReadOnlyCollection<(AccTaxTransaction taxRecord, AccTaxRecordTransactionLinePivot linePivot)> result = null;
						var errorMessage = ZString.Empty;

						(result, errorMessage) = ObjectFactory.Get<ITaxProcessor>().GetEstimatedTaxRecordsByTaxSystemCodes(taxParent, taxSystemCodes);

						if (!errorMessage.IsEmpty)
						{
							throw new InterruptPostingException(errorMessage);
						}

						if (!result.Any())
						{
							continue;
						}

						var taxRecordPKsWithDuplicateTaxCode = TaxRecordCollectionValidator.GetDuplicatedTaxRecordsForSinglePostingTaxSystem(result.Select(x => x.taxRecord).ToHashSet()).Select(x => x.PK).ToHashSet();
						if (!taxRecordPKsWithDuplicateTaxCode.Any())
						{
							continue;
						}

						var taxRecordsWithPivotsHavingDuplicateTaxCodes = (from taxRecords in result
																		   where taxRecordPKsWithDuplicateTaxCode.Contains(taxRecords.taxRecord.PK)
																		   select (taxRecords.taxRecord, taxRecords.linePivot)).ToList();

						var taxLines = ((ITaxRecordParentBase)taxParent).GetLines();
						var groupsByTaxSystemCodeAndLinePK = taxRecordsWithPivotsHavingDuplicateTaxCodes.GroupBy(x => (x.taxRecord.ATT_TaxSystemCode, x.linePivot.ATP_AL_TransactionLine));

						foreach (var group in groupsByTaxSystemCodeAndLinePK)
						{
							if (group.Count() > 1)
							{
								var taxLine = GetTaxLine(taxLines, group.Key.ATP_AL_TransactionLine);
								var chargeCode = taxLine.ChargeCode.AC_Code;
								var taxSystemCode = group.Key.ATT_TaxSystemCode;
								errorMessage = Res.GetString("9234aca4-1ed4-4ba7-b133-e73def792fcb", @"Posting is prevented because Tax defaulting rules for Charge Code '{0}' would create more than one Tax record for the '{1}' Tax System.
Please review the Tax Override rules configured for this charge code.", chargeCode, taxSystemCode);

								throw new InterruptPostingException(errorMessage);
							}
						}

						int counter = 1;
						var groupByTaxRecords = taxRecordsWithPivotsHavingDuplicateTaxCodes.GroupBy(x => x.taxRecord.PK);
						foreach (var groupedByTaxRecord in groupByTaxRecords.Skip(1))
						{
							var newKey = new PostingChargeKey(charges.Key);
							newKey.TaxSystemSplitKey = counter++;

							var chargesToAddList = new List<IReceivablesPostingCharge>();
							foreach (var item in groupedByTaxRecord)
							{
								var taxLine = GetTaxLine(taxLines, item.linePivot.ATP_AL_TransactionLine);
								charges.Remove(taxLine.PostingCharge);

								chargesToAddList.Add(taxLine.PostingCharge);
							}

							chargesToAddDictionary[newKey] = (charges, chargesToAddList);
						}
					}
				}

				foreach (var items in chargesToAddDictionary)
				{
					if (!postingCharges.ContainsKey(items.Key))
					{
						postingCharges.SetCharges(items.Key, PostingChargeDistributor.CreateNewChargesCollection(items.Value.existingChargesCollection));
					}

					foreach (var chargeToMove in items.Value.chargeToMove)
					{
						postingCharges.GetCharges(items.Key).Add(chargeToMove);
					}
				}
			}

			return postingCharges;

			TaxLineFromPostingCharge GetTaxLine(IReadOnlyList<ITaxableTransactionLineBase> taxLines, ZGuid linePK)
			{
				return taxLines.Cast<TaxLineFromPostingCharge>().First(x => x.PK == linePK);
			}
		}

		ITaxRecordCollectionValidator TaxRecordCollectionValidator => taxRecordCollectionValidator_constructorInitializedOnly;
		ITaxRecordCollectionValidator taxRecordCollectionValidator_constructorInitializedOnly;

		#if DEBUG

		public void SubstituteTaxRecordCollectionValidator_ForTestOnly(ITaxRecordCollectionValidator replacement) => taxRecordCollectionValidator_constructorInitializedOnly = replacement;
		public ITaxRecordCollectionValidator TaxRecordCollectionValidator_ExposedForTestOnly => TaxRecordCollectionValidator;

		#endif
	}
}
