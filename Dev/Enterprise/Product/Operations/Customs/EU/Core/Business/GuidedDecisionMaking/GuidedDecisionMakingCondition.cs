using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingCondition : AutoGuidedDecisionMakingCondition
	{
		public GuidedDecisionMakingCondition(GuidedDecisionMakingBasic gDMBasic = null) : base(gDMBasic?.Factory)
		{
			Parent = gDMBasic;
		}

		public GuidedDecisionMakingBasic Parent { get; private set; }

		public bool IsImport => Parent.IsImport;

		public override ZBool IsSatisfied => ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().GroupBy(x => x.LogicalGroup).Select(group => group.ToList()).All(x => x.Any(cd => cd.IsSatisfied));

		public override ZBool IsInformationCondition => !InformationValue.IsEmpty;

		public ZString ConditionDescription
		{
			get
			{
				var informationValueText = InformationValue.IsEmpty ? ZString.Empty : (ZString)(Res.GetString("eb02f1dd-a200-4673-a88d-d1017c74c0c6", "\r\nInformation: {0}", InformationValue));

				return Res.GetString("53D2A722-8050-4D06-871F-90E0EAD9ECC3", "{0} - {1}\r\nDescription: {2}{3}",
					ConditionType, ConditionTypeDescription, ConditionSatisfactionType, informationValueText);
			}
		}

		public override ZString IsSatisfiedTextForBinding => IsInformationCondition ? Res.GetString("78260850-c504-4285-9182-5857fe009c6e", "Optional") : IsSatisfied ? "YES" : "NO";

		public GuidedDecisionMakingConditionDetailCollection ConditionDetails => conditionDetails ?? (conditionDetails = new GuidedDecisionMakingConditionDetailCollection(this));
		GuidedDecisionMakingConditionDetailCollection conditionDetails;
	}
}
