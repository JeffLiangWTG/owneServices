using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRuleValidation : AutoComplianceRuleValidation
	{
		public ComplianceRuleValidation(AutoComplianceRule parent) : base(parent)
		{
		}

		public ZString CurrentCountryCode => Parent.CurrentCountryCode;

		public new ComplianceRule Parent => base.Parent as ComplianceRule;

		protected override void CheckCRU_Origin()
		{
			base.CheckCRU_Origin();

			ListValidation.ErrorIfInvalidCode(Parent.CRU_OriginInfo);
			AddErrorIfInvalid(Parent.CRU_OriginInfo);
		}

		protected override void CheckCRU_Destination()
		{
			base.CheckCRU_Destination();

			ListValidation.ErrorIfInvalidCode(Parent.CRU_DestinationInfo);
			AddErrorIfInvalid(Parent.CRU_DestinationInfo);
		}

		protected override void CheckCRU_HarmonizedCode()
		{
			base.CheckCRU_HarmonizedCode();

			var hCode = Parent.CRU_HarmonizedCode;
			if (!hCode.ToString().All(c => c is >='0' and <= '9')
				|| hCode.Length is 1 or > 6
				|| hCode.StartsWith("00"))
			{
				Parent.CRU_HarmonizedCodeInfo.AddError(Res.GetString("a1828553-2b88-4437-a1e3-4b3d74979de1", "Invalid code entered. A valid Harmonized Code can only contain 2 to 6 digits and cannot start with 00."));
			}

			if (!Parent.CRU_HarmonizedCodeInfo.HasErrors())
			{
				AddErrorIfDuplicate();
			}
		}

		protected override void CheckCRU_RiskStatus()
		{
			base.CheckCRU_RiskStatus();

			MandatoryValidation.CheckEntered(Parent.CRU_RiskStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CRU_RiskStatusInfo, Parent.Lookups.ComplianceRuleRiskStatusCodeList);
		}

		void AddErrorIfInvalid(ZPropertyInfo info)
		{
			if (!info.HasErrors())
			{
				if (Parent.CurrentCountry == null)
				{
					info.AddError(Res.GetString("937cbfe0-9f3c-4d9c-84a1-c51f99352614", "Can't find the Country/Region '{0}', please save the form and reload to add the Compliance Rules.", CurrentCountryCode));
				}
				else if (Parent.CRU_Origin.IsEmpty && Parent.CRU_Destination.IsEmpty)
				{
					info.AddError(Res.GetString("ecddff3b-b3f8-4b51-a89c-70dd4228c92a", "At least one of Origin or Destination country/region should be entered."));
				}
				else if (Parent.CRU_Origin != CurrentCountryCode && Parent.CRU_Destination != CurrentCountryCode)
				{
					info.AddError(Res.GetString("be1531e8-08d5-4f2b-813f-eb27842d960c", "At least one of Origin or Destination country/region should be '{0}'.", CurrentCountryCode));
				}
				else if (Parent.CRU_Origin == Parent.CRU_Destination)
				{
					info.AddError(Res.GetString("49231aa2-8f7f-4f38-bfac-5a03d8af52c1", "Origin and Destination cannot be the same."));
				}
				else
				{
					AddErrorIfDuplicate();
				}
			}
		}

		void AddErrorIfDuplicate()
		{
			var notAllowDuplicateRecordsMessagePrefix = Res.GetString("E4EA6CBA-AB76-4DE5-A9AD-4ECDE5CB57FD", "Duplicate");
			Parent.RemoveRowError(notAllowDuplicateRecordsMessagePrefix, true);

			if (!Parent.HasRowErrors)
			{
				if (Parent.CurrentCountry?.ComplianceRules is ComplianceRuleCollection rules)
				{
					if (rules.Cast<ComplianceRule>().Any(u =>
					u.PK != Parent.PK
					&& u.CRU_Origin == Parent.CRU_Origin
					&& u.CRU_Destination == Parent.CRU_Destination
					&& u.CRU_HarmonizedCode == Parent.CRU_HarmonizedCode
					&& u.CRU_CountryOrGrouping == Parent.CRU_CountryOrGrouping))
					{
						var detailedMessages = new System.Collections.Generic.List<string>();

						if (!Parent.CRU_Origin.IsEmpty)
						{
							detailedMessages.Add(Res.GetString("7B4E824C-55FF-4269-A4D5-8067F1AD82B7", "Origin ({0})", Parent.CRU_Origin));
						}

						if (!Parent.CRU_Destination.IsEmpty)
						{
							detailedMessages.Add(Res.GetString("D5C136C9-104E-4380-9C12-46F3529443C1", "Destination ({0})", Parent.CRU_Destination));
						}

						if (!Parent.CRU_HarmonizedCode.IsEmpty)
						{
							detailedMessages.Add(Res.GetString("78AF46E6-3264-4A03-8097-BA3EEE15F6AD", "Harmonized Code ({0})", Parent.CRU_HarmonizedCode));
						}

						if (detailedMessages.Count > 0)
						{
							Parent.AddRowError(Res.GetString("6BD16EA3-AC92-446B-9858-A232F40D9EC4", "{0} cannot be added.", string.Join(" ", notAllowDuplicateRecordsMessagePrefix, string.Join(" + ", detailedMessages))));
						}
					}
				}
			}
		}
	}
}
