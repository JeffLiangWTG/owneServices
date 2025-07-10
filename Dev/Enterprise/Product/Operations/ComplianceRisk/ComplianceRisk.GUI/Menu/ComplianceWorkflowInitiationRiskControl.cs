using System.Drawing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class ComplianceWorkflowInitiationRiskControl : ZUserControl
	{
		public ComplianceWorkflowInitiationRiskControl()
		{
			InitializeComponent();
		}

		public void SetRiskInitiationInfo(ComplianceRiskTypes riskType, ComplianceRiskStatus complianceRiskStatus)
		{
			BackColor = Color.FromArgb(244, 246, 247);
			RiskFactor.BorderStyle = System.Windows.Forms.BorderStyle.None;
			RiskReason.BorderStyle = System.Windows.Forms.BorderStyle.None;
			RiskDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;

			switch (riskType)
			{
				case ComplianceRiskTypes.Parties:
					RiskFactor.Text = ResString.GetMultilingualString("C4AB84EC-9E7E-4285-B887-80775364038F", "Parties");
					RiskDescription.Text = GetRiskDescription(complianceRiskStatus.COR_PartyRisk);
					RiskReason.Text = GetRiskReason(riskType, complianceRiskStatus.COR_PartyRisk);
					RiskDescription.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_PartyRisk);
					break;
				case ComplianceRiskTypes.Locations:
					RiskFactor.Text = ResString.GetMultilingualString("94279630-C2AD-47A9-A0A4-9A0F84B5D0C3", "Locations");
					RiskDescription.Text = GetRiskDescription(complianceRiskStatus.COR_LocationRisk);
					RiskReason.Text = GetRiskReason(riskType, complianceRiskStatus.COR_LocationRisk);
					RiskDescription.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_LocationRisk);
					break;
				case ComplianceRiskTypes.Commodities:
					RiskFactor.Text = ResString.GetMultilingualString("2D8DFAFB-6B49-40F8-9149-898791BEBF89", "Commodities");
					RiskDescription.Text = GetRiskDescription(complianceRiskStatus.COR_CommodityRisk);
					RiskReason.Text = GetRiskReason(riskType, complianceRiskStatus.COR_CommodityRisk);
					RiskDescription.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_CommodityRisk);
					break;
				case ComplianceRiskTypes.Assessment:
					RiskFactor.Text = ResString.GetMultilingualString("1FA3E080-5808-48E1-9E9B-78D16243C8E8", "Compliance Assessment");
					RiskDescription.Text = GetRiskDescription(ZString.Empty);
					RiskReason.Text = ResString.GetMultilingualString("C7DC83EC-A773-4137-BE6E-8588942BD99B", "A compliance assessment has not been performed.");
					RiskDescription.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.PotentialRisk);
					break;
				default:
					RiskFactor.Text = ResString.GetMultilingualString("C6988DA3-6177-44CE-8757-3590ECD4AF3A", "Risk Factors");
					RiskDescription.Text = ResString.GetMultilingualString("61F66AF7-4189-483F-A04E-9FA0B4138F34", "Risk");
					RiskReason.Text = ResString.GetMultilingualString("57C2E7F6-0B0B-4184-8E5D-5E6C21AB975F", "Reason");

					RiskFactor.BackColor = Color.FromArgb(244, 246, 247);
					RiskDescription.BackColor = Color.FromArgb(244, 246, 247);
					RiskReason.BackColor = Color.FromArgb(244, 246, 247);

					RiskFactor.IsFontBold = true;
					RiskDescription.IsFontBold = true;
					RiskReason.IsFontBold = true;
					break;
			}
		}

		MultilingualString GetRiskDescription(string riskStatus) => riskStatus switch
		{
			Codes.Clear => Descriptions.Clear,
			Codes.PotentialRisk => Descriptions.PotentialRisk,
			Codes.Incomplete => Descriptions.Incomplete,
			Codes.Unknown => Descriptions.Unknown,
			Codes.HighRisk => Descriptions.HighRisk,
			Codes.PossibleRisk => Descriptions.PossibleRisk,
			Codes.Blocked => Descriptions.Blocked,
			_ => ResString.GetMultilingualString("40091202-E0C0-41E8-8D8D-5318E73FCA02", "High risk"),
		};

		string GetRiskReason(ComplianceRiskTypes riskType, ZString riskStatus)
		{
			switch (riskType)
			{
				case ComplianceRiskTypes.Parties:
					switch (riskStatus)
					{
						case Codes.Clear:
							return string.Empty;
						case Codes.Blocked:
							return ResString.GetMultilingualString("28DCEDC4-AC59-47C3-8910-877120E5E741", "One or more parties have been matched to a denied party.");
						default:
							return ResString.GetMultilingualString("EF3BC495-76F8-482C-AC77-00D7B6EBD14E", "One or more parties need screening or require a review.");
					}
				case ComplianceRiskTypes.Locations:
					return riskStatus == Codes.Clear ? string.Empty : ResString.GetMultilingualString("EEA5A617-5C30-4C86-85BD-B75711BE6D82", "One or more locations have been embargoed.");
				case ComplianceRiskTypes.Commodities:
					switch (riskStatus)
					{
						case Codes.Clear:
							return ResString.GetMultilingualString("6F2AFD10-25C8-451B-AF29-43BCC015C2E6", "All HS codes have been cleared.");
						case Codes.Incomplete:
							return ResString.GetMultilingualString("EF0634EC-FE78-44E2-A5C6-B31ECE079BC9", "No HS codes are entered on the job.");
						case Codes.Unknown:
							return ResString.GetMultilingualString("D464D8BF-740C-494A-8A11-F5E7D42D7F9D", "Commodity risk has not been checked.");
						case Codes.PossibleRisk:
							return ResString.GetMultilingualString("D8C0575E-6B9D-4C08-A69D-0C0E693A65CC", "One or more HS codes may have some risk.");
						case Codes.Blocked:
							return ResString.GetMultilingualString("C6CCAAEB-CC55-4663-94C7-0DE65BE54480", "One or more HS codes cannot proceed.");
						default:
							return ResString.GetMultilingualString("AE497C9F-E433-492F-BACA-2390AD0C122D", "One or more HS codes have conditions.");
					}
				default:
					return null;
			}
		}
	}
}
