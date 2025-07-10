using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.CN.Business
{
	public class CusCNClassification : AutoCusCNClassification
	{
		public CusCNClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Pivot")]
		public override ZGuid CNC_CI
		{
			get => base.CNC_CI;
			set => base.CNC_CI = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusCNClassificationLookups.EndUseList))]
		public override ZString CNC_EndUse
		{
			get => base.CNC_EndUse;
			set => base.CNC_EndUse = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusCNClassificationLookups.CIQTariffList))]
		public override ZString CNC_CIQTariff
		{
			get => base.CNC_CIQTariff;
			set => base.CNC_CIQTariff = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusCNClassificationLookups.DestDistrictList))]
		public override ZString CNC_DestinationDistrict { get => base.CNC_DestinationDistrict; set => base.CNC_DestinationDistrict = value; }

		[List(nameof(Lookups) + "." + nameof(CusCNClassificationLookups.OrigDistrictList))]
		public override ZString CNC_OriginDistrict { get => base.CNC_OriginDistrict; set => base.CNC_OriginDistrict = value; }

		[List(nameof(Lookups) + "." + nameof(CusCNClassificationLookups.DestRegionList))]
		public override ZString CNC_DestinationRegion { get => base.CNC_DestinationRegion; set => base.CNC_DestinationRegion = value; }

		[List(nameof(Lookups) + "." + nameof(CusCNClassificationLookups.OrigRegionList))]
		public override ZString CNC_OriginRegion { get => base.CNC_OriginRegion; set => base.CNC_OriginRegion = value; }

		[List(nameof(Lookups) + "." + nameof(CusCNClassificationLookups.CIQOriginStateList))]
		public override ZString CNC_OriginState { get => base.CNC_OriginState; set => base.CNC_OriginState = value; }

		[List(nameof(Lookups) + "." + nameof(CusCNClassificationLookups.UNDGPackageTypes))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusCNClassification|CNC_UNPackageMarking", Caption = "UN Markings for Packaging", ShortCaption = "UN Markings for Pack.", FullDescription = "UN Marking for the Packaging of Dangerous Goods")]
		public override ZString CNC_UNPackageMarking { get => base.CNC_UNPackageMarking; set => base.CNC_UNPackageMarking = value; }

		[List(nameof(Lookups) + "." + nameof(CusCNClassificationLookups.TradeUnitQtyList))]
		public override ZString CNC_TradeUnitQty { get => base.CNC_TradeUnitQty; set => base.CNC_TradeUnitQty = value; }

		public CusClassPartPivot Pivot => Factory.Load<CusClassPartPivot>(CNC_CI);

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (Pivot == null || Pivot.IsDeleted)
			{
				Delete();
			}
		}
	}
}
