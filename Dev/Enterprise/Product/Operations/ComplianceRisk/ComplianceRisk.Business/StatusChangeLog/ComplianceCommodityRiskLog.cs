using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceCommodityRiskLog : NonPersistentBusinessObject
	{
		readonly Commodity commodity;

		public ComplianceCommodityRiskLog(Commodity commodity)
		{
			this.commodity = commodity;
		}

		public ZString RiskStatus => commodity.IsAssessmentInitiated ? commodity.RiskStatus : ZString.Empty;

		public ZString Notes => commodity.Notes;

		[ResourceStringData("7feca50a-c7f6-46e1-9b11-6ee616e9db4c", Caption = "Risk Status")]
		public ZString RiskStatusDescription
		{
			get
			{
				return (string)RiskStatus switch
				{
					Codes.Clear => Descriptions.Clear,
					Codes.Released => Descriptions.Released,
					Codes.PotentialRisk => Descriptions.PotentialRisk,
					Codes.NotChecked => Descriptions.NotChecked,
					Codes.Blocked => Descriptions.Blocked,
					Codes.PossibleRisk => Descriptions.PossibleRisk,
					Codes.Unknown => Descriptions.Unknown,
					Codes.HighRisk => Descriptions.HighRisk,
					Codes.NotAssessed => Descriptions.NotAssessed,
					_ => Descriptions.AssessmentNotInitialized
				};
			}
		}

		[ResourceStringData("eaf51b8f-42ea-4706-891e-6b7e90601b9e", Caption = "Harmonized Code")]
		public ZString Code => commodity.Code;

		[ResourceStringData("1700EE4A-4ECE-4C3D-BE61-D2FC826158E1", Caption = "HS Code Description")]
		public ZString HsCodeDescription => commodity.HsCodeDescription;

		[ResourceStringData("693b6687-f055-4f61-9a25-3d6cbb8312b9", Caption = "Conditions")]
		public ZString Conditions => commodity.Conditions;

		[ResourceStringData("594470c5-240a-4c8a-87ff-312e9dfc0216", Caption = "Job Number")]
		public ZString Source => commodity.Source;

		[ResourceStringData("d261ab5d-39ac-404e-974f-476e6a40918d", Caption = "Nomenclature Alerts")]
		public ZString NomenclatureCondition => commodity.NomenclatureCondition;

		[ResourceStringData("1c732586-652b-4971-9fb2-78ad6bd84d6c", Caption = "Tariff Alerts")]
		public ZString SpecificCondition => commodity.SpecificCondition;

		[ResourceStringData("60A4B20C-C86B-487D-BCC6-B608F474D104", Caption = "Commodity Source")]
		public ZString CommoditySource => commodity.CommoditySource;

		[ResourceStringData("B2B3482C-82BB-444A-AB72-4ED7C523A39C", Caption = "Goods Description")]
		public ZString GoodsDescription => commodity.GoodsDescription;

		[ResourceStringData("8ADCA1DC-CDEF-476A-B3BA-893DE15D7094", Caption = "Origin Of Goods")]
		public ZString OriginOfGoods => commodity.OriginOfGoods;

		[ResourceStringData("0F021956-EAEA-4055-A811-BBD5653645BC", Caption = "Date Added (UTC)")]
		public ZDateTime DateAddedUtc => commodity.DateAddedUtc;
	}
}
