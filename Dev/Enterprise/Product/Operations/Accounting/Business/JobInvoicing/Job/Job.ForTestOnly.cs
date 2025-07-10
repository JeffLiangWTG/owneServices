#if DEBUG

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class Job
	{
		public void RunPreSaveValidationCore_ForTestOnly() => RunPreSaveValidationCore();

		public void ApplyRevenueRecognitionDateForWholeJob_ForTestOnly(ZDateTime dateForRevenueRecognitionOptionOnlyAccepted, string revenueRecognitionOptionToExclude, out bool areREVandCSTLinesUpdated)
			=> ApplyRevenueRecognitionDateForWholeJob(dateForRevenueRecognitionOptionOnlyAccepted, revenueRecognitionOptionToExclude, out areREVandCSTLinesUpdated);

		public IAccChargeTypeOverride GetChargeTypeOverrideInformation_ForTestOnly(AccChargeCode chargeCode) => GetChargeTypeOverrideInformation(chargeCode);

		public ZString GetChargeType_ForTestOnly(AccChargeCode chargeCode) => GetChargeType_ForTestOnly(chargeCode);

		public DepartmentChooser DepartmentChooser_ForTestOnly => DepartmentChooser;

		public JobChargeRevRecognition GetOrCreateJobChargeRevRecognition_ForTestOnly(ZString recognitionType, ZDateTime recognitionDate) => GetOrCreateJobChargeRevRecognition(recognitionType, recognitionDate);

		public JobChargeRevRecognition UpdateJobChargeRevRecognition_ForTestOnly(ZString recognitionType, ZDateTime recognitionDate) => UpdateJobChargeRevRecognition(recognitionType, recognitionDate);

		public ZDateTime CalculateRevenueRecognitionDate_ForTestOnly(IRevenueRecognition revenueRecognition) => CalculateRevenueRecognitionDate(revenueRecognition);

		public IRevenueRecognition GetRevenueRecognitionOption_ForTestOnly(AccChargeCode chargeCode) => GetRevenueRecognitionOption(chargeCode);

		public IRevenueRecognition GetRevenueRecognitionOption_ForTestOnly(AccChargeCode chargeCode, string revenueRecognitionOptionOnlyAccepted = null)
			=> GetRevenueRecognitionOption(chargeCode, revenueRecognitionOptionOnlyAccepted);

		public ZDateTime GetRevenueRecognitionDate_ForTestOnly(ZString revenueRecognitionType, bool getOldJobDatesOnly = false) => GetRevenueRecognitionDate(revenueRecognitionType, getOldJobDatesOnly);

		public IRevenueRecognition GetRevenueRecognitionOption_CreateStubIfRecognitionOptionIsNotExist_ForTestOnly(AccChargeCode chargeCode, ZString revenueRecognitionOptionOnlyAccepted)
			=> GetRevenueRecognitionOption_CreateStubIfRecognitionOptionIsNotExist(chargeCode, revenueRecognitionOptionOnlyAccepted);

		public ZDateTime OffsetRevenueRecognitionDate_ForTestOnly(ZDateTime revenueRecognitionDate, IRevenueRecognition revenueRecognition)
			=> OffsetRevenueRecognitionDate(revenueRecognitionDate, revenueRecognition);

		public Dictionary<string, bool> ChangedNotInSuspendedMode_ForTestOnly => ChangedNotInSuspendedMode;

		public JobClosureConfiguration JobClosureConfiguration_ForTestOnly
		{
			set
			{
				jobClosureConfiguration = value;
			}
		}

		public ZDateTime TryToGetReOpenRestirctionDate_ForTestOnly() => TryToGetReOpenRestirctionDate();

		public ConcurrentDictionary<string, IAccChargeTypeOverride> ChargeTypeCache_ForTestOnly => ChargeTypeCache;

		public void AddGenericExchangeRatesFromSource_ForTestOnly(IExchangeRateSource source) => AddGenericExchangeRatesFromSource(source);

		public void ClearAllUnpostedCharges_ForTestOnly() => ClearAllUnpostedCharges();

		public ZGuid ReopenedUserPK_ForTestOnly
		{
			get
			{
				return ReopenedUserPK;
			}

			set
			{
				ReopenedUserPK = value;
			}
		}

		public ZDecimal JH_TotalTaxExpense_ForTestOnly => JH_TotalTaxExpense;

		public ZDecimal JH_TotalTaxExpensePosted_ForTestOnly => JH_TotalTaxExpensePosted;

		public ZDecimal JH_TotalTaxExpensePostedExcludingDSB_ForTestOnly => JH_TotalTaxExpensePostedExcludingDSB;

		public ZDecimal JH_TotalTaxExpenseExcludingDSB_ForTestOnly => JH_TotalTaxExpenseExcludingDSB;

		public ZDecimal JH_ProfitLossExcludingDSB_ForTestOnly => JH_ProfitLossExcludingDSB;

		[BusinessObjectTestExclude]
		public ChargeCollection ChargesField_ForTestOnly
		{
			set
			{
				fCharges = value;
			}
		}

		public IRevenueRecognition FindRevenueRecognition_ForTestOnly(IEnumerable<BusinessObject> revenueRecognitions, string jobType, string direction, string mode, string broker)
			=> FindRevenueRecognition(revenueRecognitions, jobType, direction, mode, broker);

		public void ApplyRevenueRecognitionDate_ForTestOnly(
			BaseCharge charge,
			ZString revenueRecognitionOptionOnlyAccepted,
			ZDateTime dateForRevenueRecognitionOptionOnlyAccepted,
			bool updateREVandCSTReverseDate,
			bool useStubIfRecognitionOptionIsNotExist,
			bool canDeferRecognition,
			AccChargeCode chargeCode,
			out bool areREVandCSTLinesUpdated)
		{
			ApplyRevenueRecognitionDate(
			charge,
			revenueRecognitionOptionOnlyAccepted,
			dateForRevenueRecognitionOptionOnlyAccepted,
			updateREVandCSTReverseDate,
			useStubIfRecognitionOptionIsNotExist,
			canDeferRecognition,
			chargeCode,
			out areREVandCSTLinesUpdated);
		}

		public bool IsRevenueRecognitionShouldBeDeferred_ForTestOnly(string optionCode) => IsRevenueRecognitionShouldBeDeferred(optionCode);

		public List<Tuple<ICommissionableTransaction, IAccCommissionHeader[]>> GetNonReversedCommissionHeaders_ForTestOnly() => GetNonReversedCommissionHeaders();

		public IDisposable GetSetJobDefaultsInProgress_ForTestOnly() => GetSetJobDefaultsInProgress();
	}
}

#endif
