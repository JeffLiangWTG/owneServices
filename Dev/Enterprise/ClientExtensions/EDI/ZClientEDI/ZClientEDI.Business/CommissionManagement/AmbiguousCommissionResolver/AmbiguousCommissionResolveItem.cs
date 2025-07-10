using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class AmbiguousCommissionResolveItem : AutoAmbiguousCommissionResolveItem
	{
		internal AmbiguousCommissionResolveItem()
		{
		}

		public AmbiguousCommissionResolveItem(AccAmbiguousCommission ambiguousCommission)
			: base(ambiguousCommission.Factory)
		{
			this.ambiguousCommission = ambiguousCommission;
			base.SelectedAgreementPk = ambiguousCommission.AC0_CA0_SelectedAgreement;
		}

		#region Properties

		#region AmbiguousCommission

		public AccAmbiguousCommission AmbiguousCommission
		{
			get { return ambiguousCommission; }
		}
		readonly AccAmbiguousCommission ambiguousCommission;

		#endregion

		#region SelectedAgreementPk

		[List("ComissionAgreements")]
		public override ZGuid SelectedAgreementPk
		{
			get { return base.SelectedAgreementPk; }
			set { base.SelectedAgreementPk = value; }
		}

		public OrgCommissionAgreement SelectedAgreement
		{
			get { return Factory.Load<OrgCommissionAgreement>(SelectedAgreementPk); }
		}

		#endregion

		#region ResolutionProvided

		public ZBool ResolutionProvided
		{
			get { return ambiguousCommission.AC0_CA0_SelectedAgreement != SelectedAgreementPk; }
		}

		#endregion

		#region Invoice

		public AccTransactionHeader Invoice
		{
			get { return ambiguousCommission.Source; }
		}

		public InvoicingBase InvoicingBase
		{
			get { return ambiguousCommission != null ? Factory.Load<InvoicingBase>(ambiguousCommission.AC0_AH_Source) : null; }
		}

		#endregion

		#endregion

		#region Matching Overall Items

		public NonActiveCommissionAgreementOverallItemCollection MatchingOverallItems
		{
			get
			{
				if (matchingOverallItems == null)
				{
					matchingOverallItems = new NonActiveCommissionAgreementOverallItemCollection(Factory);
					ReloadMatchingOverallItems();
				}

				return matchingOverallItems;
			}
		}
		NonActiveCommissionAgreementOverallItemCollection matchingOverallItems;

		void ReloadMatchingOverallItems()
		{
			var invoice = InvoicingBase;
			if (invoice == null)
			{
				return;
			}

			var overallItemsResponsibleForLineChargeCodes = new HashSet<ViewCommissionAgreementOverallItem>(
				ResponsibleOverallItemsForInvoiceLineChargeCodesFinder.GetResponsibleOverallItems(invoice, AmbiguousCommission.AC0_CommissionStream),
				BusinessObjectEqualityComparer<ViewCommissionAgreementOverallItem>.PKOnlyComparer);

			using (matchingOverallItems.SuspendListChanged())
			{
				matchingOverallItems.RemoveAll();
				matchingOverallItems.AddRange(overallItemsResponsibleForLineChargeCodes);
			}
		}

		#endregion

		#region Lists

		public OrgCommissionAgreementCollection ComissionAgreements
		{
			get { return comissionAgreements ?? (comissionAgreements = new OrgCommissionAgreementCollection(Factory)); }
		}
		OrgCommissionAgreementCollection comissionAgreements;

		#endregion
	}
}
