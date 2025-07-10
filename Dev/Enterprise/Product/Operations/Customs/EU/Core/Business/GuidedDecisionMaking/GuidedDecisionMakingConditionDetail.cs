using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingConditionDetail : AutoGuidedDecisionMakingConditionDetail
	{
		public GuidedDecisionMakingConditionDetail(GuidedDecisionMakingCondition guidedDecisionMakingCondition = null) : base(guidedDecisionMakingCondition?.Factory)
		{
			Parents = new List<GuidedDecisionMakingCondition>();
			if (guidedDecisionMakingCondition != null)
			{
				Parents.Add(guidedDecisionMakingCondition);
			}
		}

		public List<GuidedDecisionMakingCondition> Parents { get; }

		public override ZBool IsSatisfied => IsTicked &&
												((Type.Equals(Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument) && !Reference.IsEmpty)
													|| Type.Equals(Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber));

		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingConditionDetailLookups.ConditionDetailCodesList))]
		[MaxLength(15)]
		public override ZString Code
		{
			get => base.Code;
			set => base.Code = value;
		}

		public override ZString Type
		{
			get => base.Type;
			set
			{
				var oldValue = base.Type;
				base.Type = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateReference();
					Validation.ValidateDateOfIssue();
				}
			}
		}

		public override ZBool IsTicked
		{
			get => base.IsTicked;
			set
			{
				var oldValue = base.IsTicked;
				base.IsTicked = value;
				if (oldValue != value && !IsCopying)
				{
					Parents.ForEach(p => p.IsSatisfiedTextForBindingInfo.RefreshBinding());
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateReference();
					Validation.ValidateDateOfIssue();
				}
			}
		}

		[ReadOnlyMember(nameof(ReferenceReadOnly))]
		public override ZString Reference
		{
			get => base.Reference;
			set
			{
				var oldValue = base.Reference;
				base.Reference = value;
				if (oldValue != value && !IsCopying)
				{
					Parents.ForEach(p => p.IsSatisfiedTextForBindingInfo.RefreshBinding());
				}
			}
		}

		[ReadOnlyMember(nameof(ReferenceReadOnly))]
		public override ZDateTime DateOfIssue
		{
			get => base.DateOfIssue;
			set => base.DateOfIssue = value;
		}

		public bool ReferenceReadOnly => Type.Equals(Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber) || !IsTicked;

		public GuidedDecisionMakingConditionDetailLookups Lookups => lookups ?? (lookups = GetNewLookups());
		GuidedDecisionMakingConditionDetailLookups lookups;

		protected virtual GuidedDecisionMakingConditionDetailLookups GetNewLookups() => new GuidedDecisionMakingConditionDetailLookups(this);
	}
}
