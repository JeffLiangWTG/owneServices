using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingVAT : AutoGuidedDecisionMakingVAT
	{
		public GuidedDecisionMakingVAT(GuidedDecisionMakingBasic guidedDecisionMakingBasic) : base(guidedDecisionMakingBasic.Factory)
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
					Parent?.ClearDocumentConditionsCache();
				}
				base.IsTicked = value;
			}
		}

		public ZString DisplayText
		{
			get
			{
				var additionalCode = AdditionalCode;
				var additionalCodeDescription = AdditionalCodeDescription;
				return $"{VATCode} - {Description} - {(VATRateValue.ToString("P2"))}{System.Environment.NewLine}" +
					$"{(additionalCode.IsEmpty ? (additionalCodeDescription.IsEmpty ? ZString.Empty : new ZString($"{additionalCodeDescription}{System.Environment.NewLine}")) : additionalCodeDescription.IsEmpty ? (new ZString($"{additionalCode}{System.Environment.NewLine}")) : (new ZString($"{additionalCode} - {additionalCodeDescription}{System.Environment.NewLine}")))}" +
					$"{Category}";
			}
		}
	}
}
