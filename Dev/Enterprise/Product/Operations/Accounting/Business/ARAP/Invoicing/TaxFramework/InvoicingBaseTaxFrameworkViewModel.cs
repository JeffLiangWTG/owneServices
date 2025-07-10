using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public class InvoicingBaseTaxFrameworkViewModel : IInvoicingBaseTaxFrameworkViewModel
	{
		internal InvoicingBaseTaxFrameworkViewModel(InvoicingBase invoice)
		{
			this.parent = invoice;
		}
		readonly InvoicingBase parent;

		ZString IInvoicingBaseTaxFrameworkViewModel.ValidateForPosting()
		{
			var errorMessage = ZString.Empty;
			errorMessage += ValildateIsOtherTaxesCalculatedBeforePosting();

			if (errorMessage.IsEmpty)
			{
				TrackHasChanges(false);
			}

			return errorMessage;
		}

		ZString ValildateIsOtherTaxesCalculatedBeforePosting()
		{
			var errorMessage = ZString.Empty;
			if (TaxRecordParent.ShouldCalculateTaxTransactions && !TaxRecordParent.IsTaxTransactionsCalculatedBeforePosting)
			{
				errorMessage = ResString.GetMultilingualString("C67AD9A4-FD10-474E-ADED-D6D6D0780E69", "Tax Transactions have to be calculated on the invoice before posting. Please retry posting after calculating Tax Transactions.");
			}
			return errorMessage;
		}

		public void TrackHasChanges(bool shouldTrackHasChanges)
		{
			if (shouldTrackHasChanges)
			{
				parent.HasChangesChanged += OnHasChangesChanged;
			}
			else
			{
				parent.HasChangesChanged -= OnHasChangesChanged;
			}
		}

		void OnHasChangesChanged(object sender, EventArgs e)
		{
			if (!IsTrackingHasChangesSuspended && e is HasChangesChangedEventArgs hasChangeArgs)
			{
				var originator = hasChangeArgs.ObjectThatWasChanged as IBusiness;
				if (originator != null && (originator is InvoicingBase || originator is InvoicingLineBaseCollection || originator is InvoicingLineBase) && !hasChangeArgs.ChangeInEditableChildCollection)
				{
					using (SuspendTrackingHasChanges)
					{
						var message = new ZStringBuilder();
						message.Append((NoResString)"Unexpected changes are detected on Invoice. Invoice changes are not allowed after Tax Transactions are calculated as they can make Tax Transactions calculations invalid.");
						message.Append(parent.GetAllPropertyValues());
						ErrorReporter.ReportOnce("InvoicingBaseTaxFrameworkViewModel_TrackHasChanges_1", message.ToStringWithNewLineBetweenAppends());
						ObjectFactory.Get<ITaxProcessor>().DeleteTaxesNotInDB(TaxRecordParent);
					}
				}
			}
		}

		bool IsTrackingHasChangesSuspended
		{
			get
			{
				return TrackingHasChangesSuspender.IsSuspended || parent.Factory.HasAnyOfContexts(new[] { BusinessContext.MakingChangesToOtherTaxes, BusinessContext.MakingChangesNotAffectingTaxRecordParent });
			}
		}

		public IDisposable SuspendTrackingHasChanges => TrackingHasChangesSuspender.GetSuspender();

		FunctionalitySuspender TrackingHasChangesSuspender => trackingHasChangesSuspender ?? (trackingHasChangesSuspender = new FunctionalitySuspender());
		FunctionalitySuspender trackingHasChangesSuspender;

		InvoicingBaseTaxRecordParent TaxRecordParent => TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(parent);
	}
}
