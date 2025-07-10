using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class QueryOnGuaranteeSendingActionValidation : ZValidation
	{
		const string QueryIdentifier_1 = "1";
		const string QueryIdentifier_4 = "4";

		public QueryOnGuaranteeSendingActionValidation(QueryOnGuaranteeSendingAction parent) : base(parent)
		{
			Parent = parent;
		}
		public QueryOnGuaranteeSendingAction Parent { get; }

		public override Type AutoValidationType => typeof(QueryOnGuaranteeSendingActionValidation);

		public override void ValidateAll()
		{
			ValidateQueryIdentifier();
			ValidatePeriodFrom();
			ValidatePeriodTo();
		}

		public void ValidateQueryIdentifier()
		{
			ValidateCalculatedProperty(Parent.QueryIdentifierInfo);
		}

		public void ValidatePeriodFrom()
		{
			ValidateCalculatedProperty(Parent.PeriodFromInfo);
		}

		public void ValidatePeriodTo()
		{
			ValidateCalculatedProperty(Parent.PeriodToInfo);
		}

		protected void CheckQueryIdentifier()
		{
			var parent = Parent;
			if (!parent.IsValidationSuspended)
			{
				var info = parent.QueryIdentifierInfo;
				if (info.Value.IsEmpty)
				{
					info.AddMessageError(Res.GetString("00E7660A-BCF7-4445-ABEF-79C305D91244", "Please provide {0}.", info.HumanReadableName));
				}
				else if (!(parent.QueryIdentifier == QueryIdentifier_1 || parent.QueryIdentifier == QueryIdentifier_4)
						&& parent.AllGuarantees.Where(p => p.ShouldSend).Any(p => p.GuaranteeType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor || p.GuaranteeType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher))
				{
					info.AddMessageError(Res.GetString("919F641E-2284-4EDB-848E-95753AC43D18", "{0} should be 1 or 4, when the Guarantee type is 2 or 4.", info.HumanReadableName));
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(info);
				}
			}
		}

		protected void CheckPeriodFrom()
		{
			if (!Parent.IsValidationSuspended)
			{
				var info = Parent.PeriodFromInfo;
				if (!info.Value.IsEmpty && !info.Value.IsValid)
				{
					info.AddMessageError(Res.GetString("53C86D5D-7397-42D7-85BD-16BCE507D661", "Please enter a valid date."));
				}
			}
		}

		protected void CheckPeriodTo()
		{
			if (!Parent.IsValidationSuspended)
			{
				var info = Parent.PeriodToInfo;
				var periodFromInfo = Parent.PeriodFromInfo;
				if (periodFromInfo.Value.IsEmpty != info.Value.IsEmpty)
				{
					info.AddMessageError(Res.GetString("179279B7-F955-4B36-A1A1-B69D736838E7", "Both {0} date and {1} date must be entered or blank.", periodFromInfo.HumanReadableName, info.HumanReadableName));
				}
				else if (!info.Value.IsEmpty && !info.Value.IsValid)
				{
					info.AddMessageError(Res.GetString("950393AA-99CD-4715-A6F0-F2E0A7D5AB20", "Please enter a valid date."));
				}
				else if (periodFromInfo.Value.CompareTo(info.Value) > 0)
				{
					info.AddMessageError(Res.GetString("E5A424FD-BE2C-450E-B091-BCC9670C2F38", "{0} date must be after {1} date.", info.HumanReadableName, periodFromInfo.HumanReadableName));
				}
			}
		}
	}
}
