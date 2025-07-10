using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public partial class PaymentApprovalDocumentSupporter : DocumentSupporter
	{
		public PaymentApprovalDocumentSupporter(PaymentApprovalBase approval)
			: base(approval)
		{
			this.Approval = approval;
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.PaymentApproval; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == DataContext.GenericFreightJob)
			{
				if (commandBeingRun.SU_MenuName == PaymentBatchListingMenuName)
				{
					return Approval.PaymentBatch != null
						? DocumentWrapperFactory.GenerateGenericWrappers(DataContext.GenericFreightJob, Approval.PaymentBatch)
						: Array.Empty<DocumentWrapper>();
				}

				return Approval.IsPosted && Approval.TransactionHeader != null
					? DocumentWrapperFactory.GenerateGenericWrappers(DataContext.GenericFreightJob, Approval.TransactionHeader)
					: DocumentWrapperFactory.GenerateGenericWrappers(DataContext.GenericFreightJob, Approval);
			}
			else
			{
				return new DocumentWrapper[] { Approval.IsPosted && Approval.TransactionHeader != null
					? DocumentWrapperFactory.CreateWrapper(DataContext.APPayment, Approval.TransactionHeader)
					: DocumentWrapperFactory.CreateWrapper(DataContext.PaymentApproval, Approval) };
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded Document Menu Names")]
		public const string PaymentBatchListingMenuName = "Payment Batch Listing";

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.TransactionHeader, DataContext.GenericFreightJob };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeader orgHeader = Factory.Load<OrgHeader>(Approval.AV_OH);
			return new OrgHeaderContact(orgHeader, null);
		}

		readonly PaymentApprovalBase Approval;
	}
}
