using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static System.Environment;

namespace Enterprise.ComplianceRisk.Business
{
	public static class ComplianceCheckRequestModelBuilder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
		public const string Export = "export";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
		public const string Import = "import";

		public static ComplianceCheckRequestModel GetRequestModel(BusinessObject hostBusinessEntity, IEnumerable<ComplianceCheckRequestPointPair> pointPairs, ComplianceCommodityDetail[] complianceCommodityDetails)
		{
			var commoditiesInfo = complianceCommodityDetails
				.GroupBy(u => (HarmonizedCode: u.CCD_HarmonizedCode, OriginOfGoods: u.CCD_RN_NKOrigin, GoodsDescription: u.CCD_Description.ToUpperInvariant()))
				.Select(u => (CommodityInfo: u.Key, Origin: u.Key.OriginOfGoods.IsEmpty ? Array.Empty<string>() : new[] { u.Key.OriginOfGoods.ToString() },
								HsDescription: u.FirstOrDefault()?.Description, GoodsDescription: u.FirstOrDefault()?.CCD_Description))
				.OrderBy(u => u.CommodityInfo.HarmonizedCode)
				.ThenBy(u => u.CommodityInfo.OriginOfGoods)
				.ThenBy(u => u.CommodityInfo.GoodsDescription);

			return new ComplianceCheckRequestModel
			{
				Commodities = commoditiesInfo.Select(u => new ComplianceCheckRequestCommodityModel
				{
					HsCode = u.CommodityInfo.HarmonizedCode,
					Origin = u.Origin,
					GoodsDescription = u.GoodsDescription,
					HsCodeDescription = u.HsDescription
				}).ToArray(),
				JobNumber = GetJobNumber(hostBusinessEntity),
				PointPairs = pointPairs?
					.Where(u => u.OriginPoint?.Country != null || u.DestinationPoint?.Country != null)
					.Select(u => new ComplianceCheckRequestPointPairModel
					{
						OriginPoint = u.OriginPoint?.Country is null ? null
							: new ComplianceCheckRequestPointPairLocationModel
							{
								Country = u.OriginPoint.Country,
								MovementDescription = u.OriginPoint.MovementDescription,
								MovementType = Export,
								Unloco = u.OriginPoint.UNLOCO,
							},
						DestinationPoint = u.DestinationPoint?.Country is null ? null
							: new ComplianceCheckRequestPointPairLocationModel
							{
								Country = u.DestinationPoint.Country,
								MovementDescription = u.DestinationPoint.MovementDescription,
								MovementType = Import,
								Unloco = u.DestinationPoint.UNLOCO,
							},
						EstimatedTimeOfArrival = u.EstimatedTimeOfArrival.IsValid ? u.EstimatedTimeOfArrival.ToOffset().ToDateTimeOffset() : DateTimeOffset.MinValue,
						EstimatedTimeOfDeparture = u.EstimatedTimeOfDeparture.IsValid ? u.EstimatedTimeOfDeparture.ToOffset().ToDateTimeOffset() : DateTimeOffset.MinValue,
						Mode = u.Mode,
					}).ToArray(),
				Authentication = GetRequestAuthenticationModel(hostBusinessEntity)
			};
		}

		public static (ComplianceCheckRequestModel RequestModel, ComplianceCommodityDetail[] Commodities) GetRequestModel(ComplianceRiskBusinessObject plugInBizO, ComplianceAssessmentPointPairInfo pointPairInfo)
		{
			var commodities = GetApplicableCommodityDetails(plugInBizO.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>()).ToArray();
			var requestModel = GetRequestModel(plugInBizO.HostBusinessEntity as BusinessObject, pointPairInfo.PointPairs, commodities);
			return (requestModel, commodities);
		}

		public static bool RequestIsValid(ComplianceCheckRequestModel request)
		{
			var isValid = request != null && (request.PointPairs?.Any(u => u.OriginPoint != null || u.DestinationPoint != null) ?? false);

			if (!isValid)
			{
				DeniedPartyScreenerAsync.WriteMessage((NoResString)$"{NewLine}The request is not valid, there is no origin or destination in the request.{NewLine}");
			}

			return isValid;
		}

		public static IEnumerable<ComplianceCommodityDetail> GetApplicableCommodityDetails(IEnumerable<ComplianceCommodityDetail> complianceCommodityDetails)
		{
			return complianceCommodityDetails.Where(u => u.CommodityType != CommodityType.RelatedJobLink && !u.CCD_HarmonizedCode.IsEmpty);
		}

		static ComplianceCheckRequestAuthenticationModel GetRequestAuthenticationModel(BusinessObject hostBusinessEntity)
		{
			var loggedInOrganisationPK = Env.CurrentCompany?.OrganisationPK;
			var loggedInOrgProxy = loggedInOrganisationPK.HasValue ? hostBusinessEntity?.Factory.Load<OrgHeader>(loggedInOrganisationPK.Value) : null;

			return new ComplianceCheckRequestAuthenticationModel
			{
				OrgCode = loggedInOrgProxy?.OH_Code ?? string.Empty,
				StaffCode = GlbStaff.CurrentUser?.GS_Code ?? string.Empty,
				DataBaseNumber = ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber.ToString(CultureInfo.InvariantCulture),
			};
		}

		static string GetJobNumber(BusinessObject hostBusinessEntity) => hostBusinessEntity != null ? CodePropertyAttribute.CodeFromBusinessObject(hostBusinessEntity) : "NewJob";
	}
}
