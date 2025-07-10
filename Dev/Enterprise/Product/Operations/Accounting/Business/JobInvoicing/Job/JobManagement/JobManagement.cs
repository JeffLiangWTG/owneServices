using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobManagement : JobHeader
	{
		JobProfitLossCalculation JobProfitLossCalculation => jobProfitLossCalculation ??= new JobProfitLossCalculation(this);
		JobProfitLossCalculation jobProfitLossCalculation;

		public JobManagement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public int LocalDecimals => GlbCompany.CurrentCompany.LocalCurrency.Decimals;

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal JH_TotalProfitRevenueMargin => JobProfitLossCalculation.TotalMargin;

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalAccrual => JobProfitLossCalculation.TotalAccrual;

		public ZPropertyInfo TotalAccrualInfo => GetZPropertyInfo(nameof(TotalAccrual));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalCost => JobProfitLossCalculation.TotalCost;

		public ZPropertyInfo TotalCostInfo => GetZPropertyInfo(nameof(TotalCost));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalLineAmount => JobProfitLossCalculation.TotalLineAmount;

		public ZPropertyInfo TotalLineAmountInfo => GetZPropertyInfo(nameof(TotalLineAmount));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalRevenue => JobProfitLossCalculation.TotalRevenue;

		public ZPropertyInfo TotalRevenueInfo => GetZPropertyInfo(nameof(TotalRevenue));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalWIP => JobProfitLossCalculation.TotalWIP;

		public ZPropertyInfo TotalWIPInfo => GetZPropertyInfo(nameof(TotalWIP));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalMargin => JobProfitLossCalculation.TotalMargin;

		public ZPropertyInfo TotalMarginInfo => GetZPropertyInfo(nameof(TotalMargin));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal RevRecognized => JobProfitLossCalculation.RevRecognized;

		public ZPropertyInfo RevRecognizedInfo => GetZPropertyInfo(nameof(RevRecognized));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal RevNotRecognized => JobProfitLossCalculation.RevNotRecognized;

		public ZPropertyInfo RevNotRecognizedInfo => GetZPropertyInfo(nameof(RevNotRecognized));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CstRecognized => JobProfitLossCalculation.CstRecognized;

		public ZPropertyInfo CstRecognizedInfo => GetZPropertyInfo(nameof(CstRecognized));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CstNotRecognized => JobProfitLossCalculation.CstNotRecognized;

		public ZPropertyInfo CstNotRecognizedInfo => GetZPropertyInfo(nameof(CstNotRecognized));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal WipRecognized => JobProfitLossCalculation.WipRecognized;

		public ZPropertyInfo WipRecognizedInfo => GetZPropertyInfo(nameof(WipRecognized));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal WipNotRecognized => JobProfitLossCalculation.WipNotRecognized;

		public ZPropertyInfo WipNotRecognizedInfo => GetZPropertyInfo(nameof(WipNotRecognized));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AcrRecognized => JobProfitLossCalculation.AcrRecognized;

		public ZPropertyInfo AcrRecognizedInfo => GetZPropertyInfo(nameof(AcrRecognized));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AcrNotRecognized => JobProfitLossCalculation.AcrNotRecognized;

		public ZPropertyInfo AcrNotRecognizedInfo => GetZPropertyInfo(nameof(AcrNotRecognized));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ProfitLossRecognized => JobProfitLossCalculation.ProfitLossRecognized;

		public ZPropertyInfo ProfitLossRecognizedInfo => GetZPropertyInfo(nameof(ProfitLossRecognized));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ProfitLossNotRecognized => JobProfitLossCalculation.ProfitLossNotRecognized;

		public ZPropertyInfo ProfitLossNotRecognizedInfo => GetZPropertyInfo(nameof(ProfitLossNotRecognized));

		#region RevenueRecognitionDates

		public ZString RevenueRecognitionDates => Job.GetRevenueRecognitionDatesAsString(RevenueRecognitionCollection);

		ActiveBusinessObjectCollection<JobChargeRevRecognition> RevenueRecognitionCollection
		{
			get
			{
				return revenueRecognition ?? (revenueRecognition = new ActiveBusinessObjectCollection<JobChargeRevRecognition>(this));
			}
		}
		ActiveBusinessObjectCollection<JobChargeRevRecognition> revenueRecognition;

		public ZPropertyInfo RevenueRecognitionDatesInfo => GetZPropertyInfo(nameof(RevenueRecognitionDates));

		#endregion

		#region Parent Job Number

		public ZString ParentJobNumber
		{
			get
			{
				var result = ZString.Empty;

				if (JH_ParentTableCode == JobCartageSchema.Constants.Prefix)
				{
					var cartage = Factory.Load<CommonCartage>(JH_ParentID);
					result = cartage?.ParentJobNumber ?? ZString.Empty;
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		protected override bool IsChargesCollectionLoaded => true;

		#endregion
	}
}
