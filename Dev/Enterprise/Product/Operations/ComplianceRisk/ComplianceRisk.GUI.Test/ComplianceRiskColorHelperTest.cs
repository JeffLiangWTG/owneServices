using System.Drawing;
using NUnit.Framework;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskColorHelperTest : TestCase
	{
		public void TestGetColorForRiskStatus()
		{
			AssertEquals(Color.FromArgb(255, 179, 179), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.PotentialRisk));
			AssertEquals(Color.FromArgb(255, 179, 179), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.Blocked));
			AssertEquals(Color.FromArgb(255, 179, 179), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.NotChecked));
			AssertEquals(Color.FromArgb(255, 179, 179), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.Held));
			AssertEquals(Color.FromArgb(255, 179, 179), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.HighRisk));
			AssertEquals(Color.FromArgb(198, 236, 198), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.Clear));
			AssertEquals(Color.FromArgb(198, 236, 198), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.Released));
			AssertEquals(Color.FromArgb(139, 154, 239), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.OverrideClear));
			AssertEquals(Color.FromArgb(255, 179, 179), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.Incomplete));
			AssertEquals(Color.FromArgb(255, 179, 179), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.Unknown));
			AssertEquals(Color.FromArgb(255, 210, 165), ComplianceRiskColorHelper.GetColorForRiskStatus(Codes.PossibleRisk));
			AssertEquals(Color.Empty, ComplianceRiskColorHelper.GetColorForRiskStatus("XXX"));
		}
	}
}
