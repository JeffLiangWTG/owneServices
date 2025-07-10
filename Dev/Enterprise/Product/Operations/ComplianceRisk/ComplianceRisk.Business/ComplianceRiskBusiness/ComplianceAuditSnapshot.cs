using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceAuditSnapshot
	{
		public List<Party> Parties { get; set; } = [];
		public List<Country> Countries { get; set; } = [];
		public List<Commodity> Commodities { get; set; } = [];
		public ComplianceJobDirection ComplianceJobDirection { get; set; } = new();
		public OverrideDecision OverrideDecision { get; set; } = new();
		public ZBool IsComplianceCommodityRiskProvider { get; set; } = true;
		public ZBool IsCompliancePartyRiskProvider { get; set; } = true;
		public ZBool IsComplianceLocationRiskProvider { get; set; } = true;
		public string PartyRisk { get; set; }
		public string LocationRisk { get; set; }
		public string CommodityRisk { get; set; }
	}

	public class Party
	{
		public Guid PK { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
		public string TableCode { get; set; }
		public string Status { get; set; }
		public Guid LogPK { get; set; }
	}

	public class Country
	{
		public string Code { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsSanctioned { get; set; }
		public Guid LogPK { get; set; }
	}

	public class Commodity
	{
		public string Code { get; set; }
		public string Conditions { get; set; }
		public string HsCodeDescription { get; set; }
		public string RiskStatus { get; set; }
		public string NomenclatureCondition { get; set; }
		public string SpecificCondition { get; set; }
		public string Source { get; set; }
		public string CommoditySource { get; set; }
		public string Notes { get; set; }
		public string GoodsDescription { get; set; }
		public string OriginOfGoods { get; set; }
		public bool IsAssessmentInitiated { get; set; }
		public DateTime DateAddedUtc { get; set; }

		internal static Commodity GetCommodity(string code, string conditions, string hsCodeDescription, string riskStatus, string source, string commoditySource, string notes, string goodsDescription, string originOfGoods, bool isAssessmentInitiated, DateTime dateAddedUtc, string nomenclatureCondition, string specificCondition)
		{
			return new Commodity
			{
				Code = code,
				Conditions = conditions,
				HsCodeDescription = hsCodeDescription,
				RiskStatus = riskStatus,
				NomenclatureCondition = nomenclatureCondition,
				SpecificCondition = specificCondition,
				Source = source,
				CommoditySource = commoditySource,
				Notes = notes,
				GoodsDescription = goodsDescription,
				OriginOfGoods = originOfGoods,
				IsAssessmentInitiated = isAssessmentInitiated,
				DateAddedUtc = dateAddedUtc
			};
		}
	}

	public class ComplianceJobDirection
	{
		public bool IsInternational { get; set; }
		public string Direction { get; set; }
	}

	public class OverrideDecision
	{
		public string Code { get; set; }
		public string Description { get; set; }
		public string Reason { get; set; }
	}
}
