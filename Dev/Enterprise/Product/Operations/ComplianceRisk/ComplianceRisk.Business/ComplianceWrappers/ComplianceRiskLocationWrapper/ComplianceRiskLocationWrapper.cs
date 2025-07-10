using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ComplianceRisk.Integration;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList.Descriptions;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskLocationWrapper : NonPersistentBusinessObject
	{
		readonly IComplianceLocation location;

		public ComplianceRiskLocationWrapper(IComplianceLocation location)
		{
			this.location = location;
		}

		#region Properties

		public bool IsSanctioned => location.IsSanctioned;

		public ZGuid Key => location.Key;

		[ResourceStringData("f46b77b6-8984-436c-b6e4-d58801bbd921", Caption = "Country Code")]
		public ZString Location
		{
			get { return location.Code; }
		}

		[ResourceStringData("fa9446ff-28f1-4727-9da9-30f17a3531ea", Caption = "Country Name")]
		public ZString LocationDescription
		{
			get { return location.LocationDescription; }
		}

		[ResourceStringData("87d59ed1-7b50-49b7-b51e-3a8eba4626a9", Caption = "Description")]
		public ZString Description
		{
			get { return location.ParentsDescription; }
		}

		[ResourceStringData("ba64207a-0f1f-42ea-9fe2-21318fc48e6c", Caption = "Risk Status")]
		public ZString RiskStatus => IsSanctioned  ? Blocked : Clear;

		#endregion
	}
}
