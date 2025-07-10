using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class HarbourFeeEntryHeaderCalculationManager : IDutyCalculationManager
	{
		readonly BusinessObjectFactory factory;

		public HarbourFeeEntryHeaderCalculationManager(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		public void Calculate(CusEntryHeader[] entries)
		{
			CalculateCore(new TypedEnumerable<IHeaderFeeData>(entries));
		}

		#region IDutyCalculationManager Members

		void IDutyCalculationManager.Calculate(IEnumerable<IHeaderFeeData> entries)
		{
			CalculateCore(entries);
		}

		void CalculateCore(IEnumerable<IHeaderFeeData> entries)
		{
			var entry = GetFirstEntry(entries.Cast<CusEntryHeader>());
			if (entry != null)
			{
				ClearEntryFeesSystemCalculatedStashingUserEnteredValue(entry);
				foreach (var entryHeader in entries.Except(entry))
				{
					ClearEntryFees((CusEntryHeader)entryHeader);
				}
				var firstEntryHeader = (IHeaderFeeData)entry;

				var declaration = entry.Declaration;
				var effectiveValuationDate = entry.EffectiveValuationDate;
				var chargePaymentOrDestinationID = UniversalReferenceDataHelper.GetChargePaymentOrDestinationID(factory, declaration);

				if (!chargePaymentOrDestinationID.IsNullOrEmpty())
				{
					var harbourRates = new RefHarbourRate.Loader(factory).Load(declaration.JE_MessageType, declaration.GetDefaultDataGroupingCode(), declaration.JE_ContainerMode, effectiveValuationDate, chargePaymentOrDestinationID, declaration.IsContainerised);
					
					var entryHeaderWrapper = new HarbourFeeEntryHeaderWrapper(declaration);
					foreach (var harbourRate in harbourRates)
					{
						var entryRate = new HarbourFeeUniversalRateData(entryHeaderWrapper, harbourRate.ZXF_RateFormula);
						var fee = entryRate.ValueForDuty;

						if (fee > 0)
						{
							var harbourFeeCode = UniversalReferenceDataHelper.GetLocalHarbourFeeCode(factory, declaration, harbourRate, effectiveValuationDate);
							if (!EntryLineHasFeeForThisCode(entry, harbourFeeCode))
							{
								firstEntryHeader.SetFeeResult(harbourFeeCode, fee);
								var charge = entry.Charges.GetChargeWithThisCode(harbourFeeCode);
								userEnteredStashManager.Apply(charge.UserEnteredStashSource);
							}
						}
					}
				}
			}
		}

		CusEntryHeader GetFirstEntry(IEnumerable<CusEntryHeader> entries) => entries.OrderBy(x => x, new EntryComparer()).FirstOrDefault();

		class EntryComparer : Customs.Business.CusEntryHeader.BaseCusEntryComparer, IComparer<CusEntryHeader>
		{
			public int Compare(CusEntryHeader x, CusEntryHeader y) => base.Compare(x, y);
		}

		void ClearEntryFees(CusEntryHeader entry)
		{
			entry.Charges.RemoveAndDeleteAll();
		}

		bool EntryLineHasFeeForThisCode(CusEntryHeader entry, string rateCode) => entry.Charges.HasOverrideFeeOfGivenCode(rateCode, RateOverrideReasonList.Codes.Override);

		void ClearEntryFeesSystemCalculatedStashingUserEnteredValue(CusEntryHeader entry)
		{
			foreach (var charge in entry.Charges.Cast<CusEntryHeaderCharges>().Where(f => ShouldStash(f)).ToList())
			{
				userEnteredStashManager.Stash(charge.UserEnteredStashSource);
				entry.Charges.RemoveAndDelete(charge);
			}
		}

		ZBool ShouldStash(CusEntryHeaderCharges charge) => charge.C1_RateOverrideReasonCode.IsEmpty;

		readonly EU.Business.Declaration.UserEnteredStashManager userEnteredStashManager = new EU.Business.Declaration.UserEnteredStashManager();
		#endregion
	}
}
