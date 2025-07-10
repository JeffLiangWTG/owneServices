using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA
{
	public partial class LIQSubmissionDataColumns : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LIQSubmissionDataColumns(BusinessObjectFactory factory, AccComplianceReport report) : base(factory)
		{
			Argument.NotNull(report, nameof(report));
			ComplianceReport = report;
			Status = AccTaxReturn.Status.Saved;

			ReasonHolder1 = new AdjustmentReasonHolder(() => Adjustments.Box1_TotalVatBaseReceivables) { HasChanges = false };
			ReasonHolder2 = new AdjustmentReasonHolder(() => Adjustments.Box2_TotalVatReceivables) { HasChanges = false };
			ReasonHolder3 = new AdjustmentReasonHolder(() => Adjustments.Box3_TotalVatBasePayables) { HasChanges = false };
			ReasonHolder4 = new AdjustmentReasonHolder(() => Adjustments.Box4_TotalVatPayablesRecoverable) { HasChanges = false };
			ReasonHolder5 = new AdjustmentReasonHolder(() => Adjustments.Box5_TotalVatPayablesNotRecoverable) { HasChanges = false };
			ReasonHolder7 = new AdjustmentReasonHolder(() => Adjustments.Box7_BalancePreviousPeriod) { HasChanges = false };
		}

		public void PopulateDataFromReport()
		{
			if (!IsSubmitted)
			{
				var helper = new LIQSubmissionDataHelper(ComplianceReport);
				helper.SetValues(ComputedByCW1);

				UpdateValuesToSubmit();
			}
		}

		public AccComplianceReport ComplianceReport { get; }

		#region Columns

		public LIQSubmissionData ComputedByCW1
		{
			get
			{
				if (computedByCW1 == null)
				{
					computedByCW1 = new LIQSubmissionData(Factory);
				}

				return computedByCW1;
			}
		}
		LIQSubmissionData computedByCW1;

		public LIQSubmissionData Adjustments
		{
			get
			{
				if (adjustments == null)
				{
					adjustments = new LIQSubmissionData(Factory);
					RegisterEditableChildObject(adjustments);

					adjustments.Box1_TotalVatBaseReceivablesInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box2_TotalVatReceivablesInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box3_TotalVatBasePayablesInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box4_TotalVatPayablesRecoverableInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box5_TotalVatPayablesNotRecoverableInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box7_BalancePreviousPeriodInfo.ValueChanged += AdjustmentsEvent;
				}
				return adjustments;
			}
		}
		LIQSubmissionData adjustments;

		void AdjustmentsEvent(object sender, EventArgs e)
		{
			UpdateValuesToSubmit();
		}

		public LIQSubmissionData ValuesToSubmit
		{
			get
			{
				if (valuesToSubmit == null)
				{
					valuesToSubmit = new LIQSubmissionData(Factory);
				}

				return valuesToSubmit;
			}
		}
		LIQSubmissionData valuesToSubmit;

		#endregion

		public void UpdateValuesToSubmit()
		{
			ValuesToSubmit.Box1_TotalVatBaseReceivables = ComputedByCW1.Box1_TotalVatBaseReceivables + Adjustments.Box1_TotalVatBaseReceivables;
			ValuesToSubmit.Box2_TotalVatReceivables = ComputedByCW1.Box2_TotalVatReceivables + Adjustments.Box2_TotalVatReceivables;
			ValuesToSubmit.Box3_TotalVatBasePayables = ComputedByCW1.Box3_TotalVatBasePayables + Adjustments.Box3_TotalVatBasePayables;
			ValuesToSubmit.Box4_TotalVatPayablesRecoverable = ComputedByCW1.Box4_TotalVatPayablesRecoverable + Adjustments.Box4_TotalVatPayablesRecoverable;
			ValuesToSubmit.Box5_TotalVatPayablesNotRecoverable = ComputedByCW1.Box5_TotalVatPayablesNotRecoverable + Adjustments.Box5_TotalVatPayablesNotRecoverable;
			ValuesToSubmit.Box7_BalancePreviousPeriod = ComputedByCW1.Box7_BalancePreviousPeriod + Adjustments.Box7_BalancePreviousPeriod;
		}

		public ZString Status { get; set; }

		public AdjustmentReasonHolder ReasonHolder1 { get; }

		public AdjustmentReasonHolder ReasonHolder2 { get; }

		public AdjustmentReasonHolder ReasonHolder3 { get; }

		public AdjustmentReasonHolder ReasonHolder4 { get; }

		public AdjustmentReasonHolder ReasonHolder5 { get; }

		public AdjustmentReasonHolder ReasonHolder7 { get; }

		public ZString CompanyName => ComplianceReport.Company.CompanyName;

		public ZString CompanyAddress1 => ComplianceReport.Company.Address1;

		public ZString CompanyAddress2 => ComplianceReport.Company.Address2;

		public ZString CompanyCountry => ComplianceReport.Company.Country.Description;

		public ZString CompanyCity => ComplianceReport.Company.City;

		public ZString CompanyPostCode => ComplianceReport.Company.Postcode;

		public ZString CompanyState => ComplianceReport.Company.State;

		public ZDate PeriodStartDate => ComplianceReport.ACR_DateFrom;

		public ZDate PeriodEndDate => ComplianceReport.ACR_DateTo;

		[ReadOnly(true)]
		public ZDate ReturnDueDate { get; set; }

		public ZString TimeStamp => ZDateTime.Now.ToString();

		public ZString NameOfLoggedInUserOrLoginID => GlbStaff.CurrentUser.GS_FullName;

		public ZString ReceiptReferenceReceivedFromHMRC { get; set; }

		public ZString PeriodKey { get; set; }

		public bool TooOldWarning { get; private set; }

		public ZBool Declaration { get; set; }

		public ZInt PageToAP { get; set; }

		public ZInt PageToAR { get; set; }

		public ZInt PageToLiquidazione { get; set; }

		public ZInt PageFromAP
		{
			get
			{
				return pageFromAP;
			}
			set
			{
				if (pageFromAP != value)
				{
					SetNonPersistentPropertyValue(PageFromAPInfo, ref pageFromAP, value);
				}
				if (!IsValidationSuspended)
				{
					ValidatePageFrom();
				}
			}
		}
		ZInt pageFromAP;

		public ZPropertyInfo PageFromAPInfo
		{
			get { return GetZPropertyInfo(nameof(PageFromAP)); }
		}

		public ZInt PageFromAR
		{
			get
			{
				return pageFromAR;
			}
			set
			{
				if (pageFromAR != value)
				{
					SetNonPersistentPropertyValue(PageFromARInfo, ref pageFromAR, value);
				}
				if (!IsValidationSuspended)
				{
					ValidatePageFrom();
				}
			}
		}
		ZInt pageFromAR;

		public ZPropertyInfo PageFromARInfo
		{
			get { return GetZPropertyInfo(nameof(PageFromAR)); }
		}

		public ZInt PageFromLiquidazione
		{
			get
			{
				return pageFromLiquidazione;
			}
			set
			{
				if (pageFromLiquidazione != value)
				{
					SetNonPersistentPropertyValue(PageFromLiquidazioneInfo, ref pageFromLiquidazione, value);
				}
				if (!IsValidationSuspended)
				{
					ValidatePageFrom();
				}
			}
		}
		ZInt pageFromLiquidazione;

		public ZPropertyInfo PageFromLiquidazioneInfo
		{
			get { return GetZPropertyInfo(nameof(PageFromLiquidazione)); }
		}

		void ValidatePageFrom()
		{
			var errorText = Res.GetString("7B66EE87-933A-48A3-AC2F-C9DE2C3F3522", "Please enter a starting page number, before proceeding with printing and archiving the report, the page number must be greater than 0.");
			var warningText = Res.GetString("37946D26-BC9A-44C5-B9D9-A8889D4FD049", "The page number should be consecutive to the last printed page of the previous period.");

			PageFromARInfo.ClearAllNotifications();
			PageFromAPInfo.ClearAllNotifications();
			PageFromLiquidazioneInfo.ClearAllNotifications();

			if (pageFromAR <= 0)
			{
				PageFromARInfo.AddError(errorText);
			}
			else if (pageFromAR <= MinPageFromAR)
			{
				PageFromARInfo.AddWarning(warningText);
			}

			if (pageFromAP <= 0)
			{
				PageFromAPInfo.AddError(errorText);
			}
			else if (pageFromAP <= MinPageFromAP)
			{
				PageFromAPInfo.AddWarning(warningText);
			}

			if (pageFromLiquidazione <= 0)
			{
				PageFromLiquidazioneInfo.AddError(errorText);
			}
			else if (pageFromLiquidazione <= MinPageFromLiquidazione)
			{
				PageFromLiquidazioneInfo.AddWarning(warningText);
			}
		}

		public override bool ReadOnly
		{
			get { return false; }
			set { base.ReadOnly = value; }
		}

		FunctionalitySuspender SaveOnlyAdjustmentData =>
			saveOnlyAdjustmentData ?? (saveOnlyAdjustmentData = new FunctionalitySuspender());

		FunctionalitySuspender saveOnlyAdjustmentData;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			var saveOnlyAdjustments = !SaveOnlyAdjustmentData.IsSuspended && !IsGroupMemberSubmission;
			CreateOrUpdateTaxReturn(saveOnlyAdjustments);

			base.OnFactorySavingBeforeTransactionCore();
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidatePageFrom();
			base.RunPreSaveValidationCore();
		}

		public ZInt MinPageFromAR { get; set; }

		public ZInt MinPageFromAP { get; set; }

		public ZInt MinPageFromLiquidazione { get; set; }

		public void SetMinPageFrom(ZInt pageFromAR, ZInt pageFromAP, ZInt pageFromLiquidazione)
		{
			MinPageFromLiquidazione = pageFromLiquidazione;
			MinPageFromAR = pageFromAR;
			MinPageFromAP = pageFromAP;
		}
	}
}
