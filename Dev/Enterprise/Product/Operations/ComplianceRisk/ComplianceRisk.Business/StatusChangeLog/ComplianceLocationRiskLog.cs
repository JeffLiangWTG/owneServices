using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceLocationRiskLog : NonPersistentBusinessObject
	{
		readonly Country country;
		readonly string locationRisk;

		public ComplianceLocationRiskLog(Country country, string locationRisk)
		{
			this.country = country;
			this.locationRisk = locationRisk;
		}

		public ZString Location => country.Code;

		[ResourceStringData("63f4d22c-6ad2-462d-a436-a56cbb36fd7e", Caption = "Country")]
		public ZString LocationDescription => country.Name;

		[ResourceStringData("8a64ce98-9ae7-4a20-89ec-7cdae4a281e2", Caption = "Description")]
		public ZString Description => country.Description;

		// When compliance location risk equal to Blocked, means new status apply to the compliance location risk, risk status on location line should be Blocked
		[ResourceStringData("7feca50a-c7f6-46e1-9b11-6ee616e9db4c", Caption = "Risk Status")]
		public ZString RiskStatus => country.IsSanctioned ? (locationRisk == Codes.Blocked ? Descriptions.Blocked : Descriptions.PotentialRisk) : Descriptions.Clear;
	}
}
