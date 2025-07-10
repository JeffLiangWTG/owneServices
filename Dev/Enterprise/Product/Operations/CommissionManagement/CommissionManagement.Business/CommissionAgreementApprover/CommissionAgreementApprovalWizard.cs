using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionAgreementApprovalWizard : AutoCommissionAgreementApprovalWizard
	{
		#region Constructor

		public CommissionAgreementApprovalWizard(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldOverwriteOldCommission = true;
			ShouldAddToCalculationQueue = true;
			FromDate = FromDate_DefaultValue;
		}

		#endregion

		#region Properties

		#region From Type

		[List("Lookups.FromTypes")]
		public override ZString FromType
		{
			get { return base.FromType; }
			set
			{
				if (FromType != value)
				{
					base.FromType = value;
					FromDate = FromDate_DefaultValue;
				}
			}
		}

		public ZBool FromTypeRequiresDate
		{
			get { return FromType == CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate; }
		}

		#endregion

		#region From Date

		protected bool FromDate_ReadOnly
		{
			get { return !FromTypeRequiresDate; }
		}

		public ZDateTime FromDateWithFallback
		{
			get { return FromDate.IsEmpty ? ZDateTime.MinSmallDateTimeValue : FromDate; }
		}

		ZDateTime FromDate_DefaultValue
		{
			get
			{
				var registryDefault = (ZDateTime)OrganisationRegistry.Instance.DefaultSpecifiedBackdate.Value;
				return registryDefault.IsValid ? registryDefault : ZDateTime.Empty;
			}
		}

		#endregion

		#region Error Messages

		public List<ZString> ErrorMessages
		{
			get { return errorMessages ?? (errorMessages = new List<ZString>(1)); }
		}
		List<ZString> errorMessages;

		#endregion

		public bool HasIncludedItems
		{
			get
			{
				return CommissionAgreementApprovalItems.Any(x => x.IsInclude);
			}
		}

		#endregion

		public ActionType Action = ActionType.NotSpecified;

		#region Approve

		public void Approve(Progress progress)
		{
			ErrorMessages.Clear();
			var agreementApprovalItems = CommissionAgreementApprovalItems.Where(x => x.IsInclude);
			var context = GetCreateCommissionContext(agreementApprovalItems);

			var approver = GetCommissionAgreementApprover();
			using (var manager = ((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			using (CommissionAgreementApprovalItemCollection.TemporarilySuspendRefreshCollection())
			{
				try
				{
					if (ShouldAddToCalculationQueue)
					{
						approver.ApproveAndQueue(context, progress);
					}
					else
					{
						approver.ApproveAndCreateCommissions(context, progress);
					}

					manager.CommitTransaction();
				}
				catch (OrgCommissionAgreementBeingProcessedException)
				{
					ErrorMessages.Add(Res.GetString("14155679-c617-417e-a9f0-3a90cb08f621", "Agreement is being processed by service task or was changed by another user. Try operation later."));
				}
				catch (CommissionAgreementApprovalException ex)
				{
					ErrorMessages.Add(ex.UserFriendlyMessage);
				}
				catch (CommissionAgreementApprovalWizardException ex)
				{
					ErrorMessages.Add(ex.Message);
				}
			}

			ReloadCommissionAgreementApprovalItems();
		}

		protected virtual CommissionAgreementApprover GetCommissionAgreementApprover()
		{
			return CommissionAgreementApprover.New(Factory);
		}

		#region CommissionContext

		protected virtual CreateCommissionContext GetCreateCommissionContext(IEnumerable<CommissionAgreementApprovalItem> agreementApprovalItems)
		{
			var context = new CreateCommissionContext();
			context.FromDate = FromDate.IsEmpty ? ZDateTime.MinSmallDateTimeValue : FromDate;
			context.OverwriteOldValues = ShouldOverwriteOldCommission;
			context.AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(agreementApprovalItems.Select(x => x.CommissionAgreement));
			return context;
		}

		#endregion

		#endregion

		#region Disapprove

		public void Disapprove()
		{
			var agreementApprovalItems = CommissionAgreementApprovalItems.Where(x => x.IsInclude).ToArray();
			foreach (var approvalItem in agreementApprovalItems)
			{
				var currentAgreement = approvalItem.CommissionAgreement;
				var parentAgreement = currentAgreement.ParentVersion;
				var reverseParent = currentAgreement.IsDraftCreatedFromOrganisationMerge;
				currentAgreement.DisapproveDraft();
				if (parentAgreement != null && reverseParent)
				{
					var newReverseDraft = parentAgreement.CreateDraft(true);
					newReverseDraft.Reverse();
				}
			}

			Factory.Save();

			ReloadCommissionAgreementApprovalItems();
		}

		#endregion

		#region Related Business Objects

		#region CommissionAgreementApprovalItems

		[ChildEditable]
		public CommissionAgreementApprovalItemCollection CommissionAgreementApprovalItemCollection
		{
			get
			{
				if (commissionAgreementApprovalItems == null)
				{
					commissionAgreementApprovalItems = GetCommissionAgreementApprovalItemCollection(this);
					RegisterEditableChildObject(commissionAgreementApprovalItems);
					ReloadCommissionAgreementApprovalItems();
				}

				return commissionAgreementApprovalItems;
			}
		}
		CommissionAgreementApprovalItemCollection commissionAgreementApprovalItems;

		public IEnumerable<CommissionAgreementApprovalItem> CommissionAgreementApprovalItems
		{
			get { return CommissionAgreementApprovalItemCollection.Cast<CommissionAgreementApprovalItem>(); }
		}

		protected virtual CommissionAgreementApprovalItemCollection GetCommissionAgreementApprovalItemCollection(CommissionAgreementApprovalWizard wizard)
		{
			return new CommissionAgreementApprovalItemCollection(wizard);
		}

		void ReloadCommissionAgreementApprovalItems()
		{
			if (commissionAgreementApprovalItems != null)
			{
				commissionAgreementApprovalItems.Load(UnapprovedCommissionAgreements);
			}
		}

		#endregion

		#region UnapprovedCommissionAgreements

		public virtual OrgCommissionAgreementCollection UnapprovedCommissionAgreements
		{
			get
			{
				if (unapprovedCommissionAgreements == null)
				{
					var requiresApprovalQuery = new ZQuery(OrgCommissionAgreementSchema.CA0_LastApprovedDateUtc, null);
					unapprovedCommissionAgreements = new OrgCommissionAgreementCollection(Factory, requiresApprovalQuery);
				}

				return unapprovedCommissionAgreements;
			}
		}
		OrgCommissionAgreementCollection unapprovedCommissionAgreements;

		#endregion

		#endregion

		#region Lookups

		public CommissionAgreementApprovalWizardLookups Lookups
		{
			get { return lookups ?? (lookups = GetNewLookups()); }
		}

		protected virtual CommissionAgreementApprovalWizardLookups GetNewLookups()
		{
			return new CommissionAgreementApprovalWizardLookups(this);
		}

		CommissionAgreementApprovalWizardLookups lookups;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("9f50ab73-3938-4a6c-bb18-97bf8b90ff6f", "Commission Agreement Approval"); }
		}

		#endregion

		public enum ActionType
		{
			NotSpecified,
			Approve,
			Disapprove
		}
	}

	[Serializable]
	public class CommissionAgreementApprovalWizardException : InvalidOperationException
	{
		public CommissionAgreementApprovalWizardException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected CommissionAgreementApprovalWizardException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
