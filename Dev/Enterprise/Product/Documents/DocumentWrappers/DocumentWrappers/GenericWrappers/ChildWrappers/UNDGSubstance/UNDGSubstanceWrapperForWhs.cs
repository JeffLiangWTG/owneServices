using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("UNNumberWithVariant"), WrapperTypeName("UNDGSubstance")]
	public class UNDGSubstanceWrapperForWhs : UNDGSubstanceWrapper
	{
		public UNDGSubstanceWrapperForWhs(UNDGDataItem dgData, BusinessObjectFactory factory, ZDecimal quantity)
			: base(dgData, factory)
		{
			var part = (dgData == null)
				? Factory.GetNull<OrgSupplierPart>()
				: Factory.Load<OrgSupplierPart>(dgData.DI_ParentID);
			if (part != null || dgData == null)
			{
				FallbackProduct = part;
			}
			else
			{
				throw new ArgumentException("UNDG data item should be linked to a Product.");
			}

			Quantity = quantity;
		}

		readonly OrgSupplierPart FallbackProduct;
		readonly ZDecimal Quantity;

		#region GetWeight

		protected override WeightWrapper GetWeight()
		{
			return (DGData == null)
				? WeightWrapper.Empty
				: (DGData.DI_DGWeight != 0m)
					? new WeightWrapper(DGData.DI_DGWeight * Quantity, DGData.DI_UnitOfWeight, UNDGDataItemSchema.DI_DGWeight.Scale, DGData.Lookups.WeightUnits, Factory)
					: new WeightWrapper(FallbackProduct.OP_Weight * Quantity, FallbackProduct.OP_WeightUQ, OrgSupplierPartSchema.OP_Weight.Scale, DGData.Lookups.WeightUnits, Factory);
		}

		#endregion

		#region GetVolume

		protected override VolumeWrapper GetVolume()
		{
			return (DGData == null)
				? VolumeWrapper.Empty
				: (DGData.DI_DGVolume != 0m)
					? new VolumeWrapper(DGData.DI_DGVolume * Quantity, DGData.DI_UnitOfVolume, UNDGDataItemSchema.DI_DGVolume.Scale, DGData.Lookups.VolumeUnits, Factory)
					: new VolumeWrapper(FallbackProduct.OP_Cubic * Quantity, FallbackProduct.OP_CubicUQ, OrgSupplierPartSchema.OP_Cubic.Scale, DGData.Lookups.VolumeUnits, Factory);
		}

		#endregion
	}
}
