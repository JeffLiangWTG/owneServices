//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCAAddInfoValidation
//
//    This class should be used for overriding validation in AutoCAAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.DataRegistry.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CAAddInfoValidation : AutoCAAddInfoValidation
	{
		public CAAddInfoValidation(AutoCAAddInfo parent)
			: base(parent)
		{
			if (!parent.GetType().IsSubclassOf(typeof(AddInfo)))
			{
				throw new ArgumentException("Parent is not a subclass of AddInfo");
			}
		}

		public new AddInfo Parent
		{
			get { return (AddInfo)base.Parent; }
		}

		public ZDateTime GetEffectiveDateForDutyRate()
		{
			ZDateTime result;
			var parentParent = Parent.Parent;
			if (parentParent is JobDeclaration declaration)
			{
				result = declaration.EffectiveDutyDate;
			}
			else if (parentParent is JobComInvoiceHeader invoiceHeader)
			{
				result = invoiceHeader.EffectiveDateForDutyRate;
			}
			else if (parentParent is JobComInvoiceLine invoiceLine)
			{
				result = invoiceLine.EffectiveDateForDutyRate;
			}
			else
			{
				result = ZDateTime.Now;
			}
			return result;
		}

		protected override void CheckCA_JobReadyForPost()
		{
			base.CheckCA_JobReadyForPost();

			var declaration = Parent.Parent as JobDeclaration;
			if (Parent.CA_JobReadyForPost && declaration != null)
			{
				AccountingIntegrationOptions options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetFallBackValueAtAllLevels(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);

				if (options.PreApprovalBillingJob)
				{
					IAccountingAP_ARInvoiceQuery query = GetAP_ARInvoiceQuery();

					ZGuid[] customsChargeCodes = new Registry.EntryChargeTypeList().GetAllChargeCodePKsOf(declaration.Branch.GB_GC);

					AP_ARInvoiceQueryResult queryResult = query.GetPostingDetails(declaration, customsChargeCodes, options.APPostDSB, options.ARPostDSB);

					if (queryResult.NumberOfARInvoicesToBeIssued > 1)
					{
						Parent.CA_JobReadyForPostInfo.AddWarning(string.Format(CultureInfo.InvariantCulture, MoreThanOneARInvoicesWillBeIssued, queryResult.NumberOfARInvoicesToBeIssued));
					}

					if (queryResult.NumberOfUnpostedARCharges == 0 && queryResult.NumberOfUnpostedAPCharges == 0)
					{
						Parent.CA_JobReadyForPostInfo.AddWarning(NoChargesToBePosted);
					}
					else
					{
						if (queryResult.NumberOfUnpostedARCharges != queryResult.NumberOfUnpostedARChargesReadyForPosting)
						{
							Parent.CA_JobReadyForPostInfo.AddWarning(UnmatchedARCharges);
						}

						if (queryResult.NumberOfUnpostedAPCharges != queryResult.NumberOfUnpostedAPChargesReadyForPosting)
						{
							Parent.CA_JobReadyForPostInfo.AddWarning(UnmatchedAPCharges);
						}
					}
				}
			}
		}

		protected override void CheckCA_ProductionDate()
		{
			base.CheckCA_ProductionDate();

			if (Parent.CA_ProductionDate > ZDateTime.Today)
			{
				Parent.CA_ProductionDateInfo.AddMessageError(ManufactureDateCannotBeInTheFuture);
			}
		}

		protected override void CheckCA_OriginalAccountingDateIsValidZDateTimeRange()
		{
			var parentParent = Parent.Parent;
			if (parentParent is JobDeclaration declaration && (declaration.IsB2Adjustments || declaration.IsB3X))
			{
				var limits = new TypeValidationLimits() { PastYearsBeforeWarning = 4 };
				TypeValidation.CheckValidZDateTimeRange(Parent.CA_OriginalAccountingDateInfo, limits);
			}
		}

		#region PGA Indicators

		protected override void CheckCA_CFIAInd()
		{
			base.CheckCA_CFIAInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_CFIAIndInfo, Parent.Lookups.CA_PGAIndicatorList);
		}

		protected override void CheckCA_CNSCInd()
		{
			base.CheckCA_CNSCInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_CNSCIndInfo, Parent.Lookups.CA_PGAIndicatorList);
		}

		protected override void CheckCA_DFOInd()
		{
			base.CheckCA_DFOInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_DFOIndInfo, Parent.Lookups.CA_PGAIndicatorList);
		}

		protected override void CheckCA_ECCCInd()
		{
			base.CheckCA_ECCCInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_ECCCIndInfo, Parent.Lookups.CA_PGAIndicatorList);
		}

		protected override void CheckCA_GACInd()
		{
			base.CheckCA_GACInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_GACIndInfo, Parent.Lookups.CA_PGAIndicatorList);
		}

		protected override void CheckCA_HCInd()
		{
			base.CheckCA_HCInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_HCIndInfo, Parent.Lookups.CA_PGAIndicatorList);
		}

		protected override void CheckCA_NRCanInd()
		{
			base.CheckCA_NRCanInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_NRCanIndInfo, Parent.Lookups.CA_PGAIndicatorList);
		}

		protected override void CheckCA_PHACInd()
		{
			base.CheckCA_PHACInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_PHACIndInfo, Parent.Lookups.CA_PGAIndicatorList);
		}

		protected override void CheckCA_TCInd()
		{
			base.CheckCA_TCInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_TCIndInfo, Parent.Lookups.CA_PGAIndicatorList);
		}

		#endregion

		public const string MoreThanOneARInvoicesWillBeIssued = "{0} AR invoices will be created. Both Debtor and Invoice Type on all AR Charges should be identical to ensure that only 1 AR Invoice is created";
		public const string UnmatchedARCharges = "Some AR charges on the Billing job will not be posted. Please ensure that the Revenue information is entered for these charges (Amount and Debtor etc).";
		public const string UnmatchedAPCharges = "Some AP charges on the Billing job will not be posted. Please ensure that the Cost information is entered for these charges (AP Invoice Number, Invoice Date and Payment Date etc).";
		public const string NoChargesToBePosted = "No charges were found for posting. Please check the data you entered on the Billing tab.";
		public const string PlaceOfDirectShipmentCannotBeCanada = "Place of Direct shipment cannot be Canada";
		public const string ManufactureDateCannotBeInTheFuture = "Manufacture Date can’t be a date in the future. Please enter a date equal to or less than today";

#if DEBUG
		public IAccountingAP_ARInvoiceQuery AP_ARInvoiceQueryForTesting;
#endif
		IAccountingAP_ARInvoiceQuery GetAP_ARInvoiceQuery()
		{
			IAccountingAP_ARInvoiceQuery result = ObjectFactory.Get<IAccountingAP_ARInvoiceQuery>();
#if DEBUG
			if (AP_ARInvoiceQueryForTesting != null)
			{
				result = AP_ARInvoiceQueryForTesting;
			}
#endif

			return result;
		}
	}
}
