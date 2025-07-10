using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.ManifestBase
{
	[SingleObjectAroundARow]
	[RowFetchStrategy(FetchStrategyType = typeof(AsycudaTaxRowFetchStrategy))]
	public class AsycudaTax : AutoAsycudaTax, Integration.Customs.ManifestBase.IAsycudaTax, IClusterKeyWorker
	{
		public AsycudaTax(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly AsycudaTaxTypeDecider TypeDecider = new AsycudaTaxTypeDecider();
		public new AsycudaTaxValidation Validation => base.Validation;

		public new AsycudaTaxLookups Lookups => base.Lookups;

		[List("Lookups.ChargeTypeList")]
		public override ZString AET_ChargeType { get => base.AET_ChargeType; set => base.AET_ChargeType = value; }

		[List("Lookups.MethodOfPaymentList")]
		public override ZString AET_MethodOfPayment { get => base.AET_MethodOfPayment; set => base.AET_MethodOfPayment = value; }

		[List("Lookups.RateOverrideReasonCodeList")]
		public override ZString AET_RateOverrideReasonCode { get => base.AET_RateOverrideReasonCode; set => base.AET_RateOverrideReasonCode = value; }

		[List("Lookups.Currencies")]
		public override ZString AET_RX_NKCurrency { get => base.AET_RX_NKCurrency; set => base.AET_RX_NKCurrency = value; }

		public AsycudaBill Bill => Factory.Load<AsycudaBill>(AET_ABL);

		[RelatedBusinessObject(nameof(Bill))]
		public override ZGuid AET_ABL
		{
			get { return base.AET_ABL; }
			set { base.AET_ABL = value; }
		}

		public AsycudaPackedItem PackedItem => Factory.Load<AsycudaPackedItem>(AET_API_AsycudaPackedItem);

		[RelatedBusinessObject(nameof(PackedItem))]
		public override ZGuid AET_API_AsycudaPackedItem
		{
			get { return base.AET_API_AsycudaPackedItem; }
			set { base.AET_API_AsycudaPackedItem = value; }
		}

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => AET_ABL.IsEmpty ? typeof(AsycudaPackedItem) : typeof(AsycudaBill);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)(AET_ABL.IsEmpty ? AET_API_AsycudaPackedItemInfo : AET_ABLInfo);

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)AET_ClusterKeyInfo;

		#endregion
	}
}

