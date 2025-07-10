using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class B2AdjustmentsOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		public B2AdjustmentsOperationalActionMethodApplicator(BusinessObjectFactory factory)
			: base("Insert Date Submitted operational action", factory)
		{
		}

		public static class Schema
		{
			public const string OverrideExisting = "OverrideExisting";
			public const string ForceUpdate = "ForceUpdate";
			public const string SubmissionDate = "SubmissionDate";
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			log.SetSectionProgressMax(targets.Length);

			foreach (var bo in targets)
			{
				var declaration = bo as JobDeclaration;
				if (declaration != null)
				{
					UpdateSubmissionDate(log, declaration);
					log.BumpSectionProgress();
				}
			}
		}

		void UpdateSubmissionDate(IOperationalActionSectionLog log, JobDeclaration declaration)
		{
			var link = GetDeclarationLink(declaration);
			if (!declaration.CA_B2SubmissionDate.IsEmpty && !OverrideExisting)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("ce171761-4ec0-4248-8407-4a6fc27a2490", "Job {0}: Submitted Date can't be updated as it already exists."), link);
			}
			else if (declaration.JE_SystemCreateTimeUtc > SubmissionDate && !ForceUpdate)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("799e85ac-4e3e-471d-aaa3-15db193a8451", "Job {0}: Submitted Date can't be updated as it's earlier than job created date."), link);
			}
			else
			{
				declaration.CA_B2SubmissionDate = SubmissionDate;
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("242d8639-3e46-498b-8b62-7baac9f8739b", "Job {0}: Submitted Date has been updated."), link);
			}
		}

		LogControllerLink GetDeclarationLink(JobDeclaration declaration)
		{
			return new LogControllerLink(declaration.JE_DeclarationReference, ControllerIDs.Customs.CA.B2Adjustments, declaration.PK);
		}

		public ZBool OverrideExisting
		{
			get => overrideExisting;
			set => SetNonPersistentPropertyValue(OverrideExistingInfo, ref overrideExisting, value);
		}
		ZBool overrideExisting;

		public ZPropertyInfo OverrideExistingInfo => GetZPropertyInfo(Schema.OverrideExisting);

		public ZBool ForceUpdate
		{
			get => forceUpdate;
			set => SetNonPersistentPropertyValue(ForceUpdateInfo, ref forceUpdate, value);
		}
		ZBool forceUpdate;

		public ZPropertyInfo ForceUpdateInfo => GetZPropertyInfo(Schema.ForceUpdate);

		public ZDateTime SubmissionDate
		{
			get => submissionDate;
			set
			{
				SetNonPersistentPropertyValue(SubmissionDateInfo, ref submissionDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSubmissionDate();
				}
			}
		}
		ZDateTime submissionDate;

		public ZPropertyInfo SubmissionDateInfo => GetZPropertyInfo(Schema.SubmissionDate);

		public B2AdjustmentsOperationalActionMethodApplicatorValidation Validation => new B2AdjustmentsOperationalActionMethodApplicatorValidation(this);
	}
}
