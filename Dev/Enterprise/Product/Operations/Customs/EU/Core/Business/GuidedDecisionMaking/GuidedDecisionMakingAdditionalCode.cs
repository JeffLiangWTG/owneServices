using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingAdditionalCode : AutoGuidedDecisionMakingAdditionalCode
	{
		public GuidedDecisionMakingAdditionalCode(GuidedDecisionMakingBasic guidedDecisionMakingBasic) : base(guidedDecisionMakingBasic.Factory)
		{
			Parent = guidedDecisionMakingBasic;
		}

		public GuidedDecisionMakingBasic Parent { get; }

		public override ZBool IsTicked
		{
			get => base.IsTicked;
			set
			{
				var oldValue = base.IsTicked;
				if (!IsCopying && oldValue != value)
				{
					UpdateSiblings(value);
					Parent?.ClearDocumentConditionsCache();
				}
				base.IsTicked = value;
			}
		}

		void UpdateSiblings(bool value)
		{
			if (Parent != null && !Parent.IsUpdatingSiblingsAdditionalCodes)
			{
				Parent.IsUpdatingSiblingsAdditionalCodes = true;
				Parent.UpdateSiblingsAdditionalCodes(AdditionalCode, ApplicableToType, value);
				Parent.IsUpdatingSiblingsAdditionalCodes = false;
			}
		}

	public override ZString ApplicableToType
		{
			get => base.ApplicableToType;
			set
			{
				if (base.ApplicableToType != value)
				{
					base.ApplicableToType = value;
					ApplicableToDescription = GetApplicableToDescription();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingAdditionalCodeLookups.AdditionalCodesList))]
		public override ZString AdditionalCode
		{
			get => base.AdditionalCode;
			set => base.AdditionalCode = value;
		}

		protected virtual ZString GetApplicableToDescription()
		{
			return ApplicableToType;
		}

		public ZString AdditionalCodeDescription
		{
			get => GetAdditionalCodeDescription();
		}

		protected ZString GetAdditionalCodeDescription()
		{
			return Lookups.CachedListOfAdditionalCodeDescriptions.GetDescriptionFromCode(AdditionalCode);
		}

		public GuidedDecisionMakingAdditionalCodeLookups Lookups => lookups ?? (lookups = GetNewLookups());
		GuidedDecisionMakingAdditionalCodeLookups lookups;

		protected virtual GuidedDecisionMakingAdditionalCodeLookups GetNewLookups() => new GuidedDecisionMakingAdditionalCodeLookups(this);
	}
}
