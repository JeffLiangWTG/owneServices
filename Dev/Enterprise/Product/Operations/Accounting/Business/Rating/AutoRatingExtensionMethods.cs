using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class AutoRatingExtensionMethods
	{
		const string gatewayKeySuffix = "_GW";

		public static ApportionmentListing GetApportionments(this IJobCostingPlugIn costing, bool isGatewayApportionments = false, IJobCreationErrorHandler jobCreationErrorHandler = null)
		{
			var costingBO = (BusinessObject)costing;
			var key = "ApportionmentListing|" + costingBO.PK.ToString();
			if (isGatewayApportionments)
			{
				key += gatewayKeySuffix;
			}
			return costingBO.Factory.GetCachedValue(key, () =>
			{
				var result = new ApportionmentListing(costingBO.Factory, costing, isGatewayApportionments, jobCreationErrorHandler);
				return result;
			});
		}

		public static void CopyPaymentBasesFromSource(this JobConsolCost target, ZGuid sourcePK)
		{
			var costToImportPaymentBases = target.Factory.Load<JobPaymentBasis>(new ZQuery(JobPaymentBasisSchema.PBS_E6, sourcePK)).ToArray();
			if (costToImportPaymentBases.Any())
			{
				foreach (var pb in costToImportPaymentBases)
				{
					var pbCopy = target.PaymentBases.AddNew();
					pbCopy.PBS_AdapterID = pb.PBS_AdapterID;
					pbCopy.PBS_AdapterType = pb.PBS_AdapterType;
					pbCopy.PBS_ChargeableAmount = pb.PBS_ChargeableAmount;
					pbCopy.PBS_ChargeableDescription = pb.PBS_ChargeableDescription;
					pbCopy.PBS_ChargeableUnit = pb.PBS_ChargeableUnit;
					pbCopy.PBS_FlatRate = pb.PBS_FlatRate;
					pbCopy.PBS_IsCost = pb.PBS_IsCost;
					pbCopy.PBS_MaxRate = pb.PBS_MaxRate;
					pbCopy.PBS_MinRate = pb.PBS_MinRate;
					pbCopy.PBS_PerUnitRate = pb.PBS_PerUnitRate;
					pbCopy.PBS_RX_NKRateCurrency = pb.PBS_RX_NKRateCurrency;
					pbCopy.PBS_RateUnitType = pb.PBS_RateUnitType;
					pbCopy.PBS_RateUnit = pb.PBS_RateUnit;
				}
			}
		}
	}
}
