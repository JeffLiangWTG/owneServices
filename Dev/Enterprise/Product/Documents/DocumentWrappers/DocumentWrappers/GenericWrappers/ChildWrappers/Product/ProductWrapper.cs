using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ProductWrapper : GenericWrapper
	{
		public ProductWrapper(OrgSupplierPart partBO, BusinessObjectFactory factory)
			: base(partBO, factory)
		{
		}

		#region Product

		public OrgSupplierPart Product
		{
			get { return (OrgSupplierPart)WrappedBO; }
		}

		#endregion

		#region Commodity

		public CodeAndDescriptionWrapper Commodity
		{
			get { return commodity ?? (commodity = GetCommodity()); }
		}
		CodeAndDescriptionWrapper commodity;

		CodeAndDescriptionWrapper GetCommodity()
		{
			return Product != null ? new CodeAndDescriptionWrapper(Product.OP_RH_NKCommodityCode, Product.Lookups.CommodityCodes, Factory) : CodeAndDescriptionWrapper.Empty;
		}

		#endregion

		#region UNDGSubstances

		public UNDGSubstanceWrapperCollection UNDGSubstances
		{
			get { return undgSubstances ?? (undgSubstances = GetUNDGSubstances()); }
		}
		UNDGSubstanceWrapperCollection undgSubstances;

		UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			var result = new UNDGSubstanceWrapperCollection(Factory);
			if (Product != null)
			{
				foreach (UNDGDataItem dGDataItem in Product.UNDGs)
				{
					if (dGDataItem.Substance != null)
					{
						result.Add(new UNDGSubstanceWrapper(dGDataItem, Factory));
					}
				}
			}
			return result;
		}

		#endregion

		#region StockUnit

		public ValueAndUnitWrapper StockUnit
		{
			get { return stockUnit ?? (stockUnit = GetStockUnit()); }
		}
		ValueAndUnitWrapper stockUnit;

		ValueAndUnitWrapper GetStockUnit()
		{
			return Product != null ? new ValueAndUnitWrapper(Product.OP_StockKeepingUnitPerPallet, Product.OP_StockKeepingUnit, Product.Lookups.OP_ProductUQ_List, Factory) : ValueAndUnitWrapper.Empty;
		}

		#endregion

		#region Volume

		public VolumeWrapper Volume
		{
			get { return volume ?? (volume = GetVolume()); }
		}
		VolumeWrapper volume;

		VolumeWrapper GetVolume()
		{
			return Product != null ? new VolumeWrapper(Product.OP_Cubic, Product.OP_CubicUQ, Product.Lookups.OP_CubicUQ_List, Factory) : VolumeWrapper.Empty;
		}

		#endregion

		#region Weight

		public WeightWrapper Weight
		{
			get { return weight ?? (weight = GetWeight()); }
		}
		WeightWrapper weight;

		WeightWrapper GetWeight()
		{
			return Product != null ? new WeightWrapper(Product.OP_Weight, Product.OP_WeightUQ, 3, Product.Lookups.OP_WeightUQ_List, Factory) : WeightWrapper.Empty;
		}

		#endregion

		#region NetWeigth

		public WeightWrapper NetWeight
		{
			get { return netWeight ?? (netWeight = GetNetWeight()); }
		}
		WeightWrapper netWeight;

		WeightWrapper GetNetWeight()
		{
			return Product != null ? new WeightWrapper(Product.OP_NetWeight, Product.OP_WeightUQ, 3, Product.Lookups.OP_WeightUQ_List, Factory) : WeightWrapper.Empty;
		}

		#endregion

		#region Dimensions

		public DimensionsWrapper Dimensions
		{
			get { return dimensions ?? (dimensions = GetDimensions()); }
		}
		DimensionsWrapper dimensions;

		DimensionsWrapper GetDimensions()
		{
			return Product != null ? new DimensionsWrapper(Product.OP_Depth, Product.OP_Width, Product.OP_Height, Product.OP_MeasureUQ, Product.Lookups.OP_MeasureUQ_List, Factory) : DimensionsWrapper.Empty;
		}

		#endregion

		#region Notes

		public NoteWrapperCollection Notes
		{
			get { return notes ?? (notes = new NoteWrapperCollection(Product, Factory)); }
		}
		NoteWrapperCollection notes;

		#endregion

		#region Style

		public ProductStyleWrapper Style
		{
			get { return StyleSize.Style; }
		}

		#endregion

		#region StyleSize

		public ProductStyleSizeWrapper StyleSize
		{
			get
			{
				if (styleSize == null)
				{
					var sizeBO = Product != null ? Factory.Load<WhsProductStyleSize>(Product.OP_WSZ_WhsProductStyleSize) : null;
					styleSize = new ProductStyleSizeWrapper(sizeBO, Factory);
				}

				return styleSize;
			}
		}
		ProductStyleSizeWrapper styleSize;

		#endregion

		#region StyleColour

		public ProductStyleColourWrapper StyleColour
		{
			get
			{
				if (styleColour == null)
				{
					var colourBO = Product != null ? Factory.Load<WhsProductStyleColour>(Product.OP_WSC_WhsProductStyleColour) : null;
					styleColour = new ProductStyleColourWrapper(colourBO, Factory);
				}

				return styleColour;
			}
		}
		ProductStyleColourWrapper styleColour;

		#endregion

		#region StyleClassification

		public ProductStyleClassificationWrapper StyleClassification
		{
			get
			{
				if (styleClassification == null)
				{
					var classificationBO = Product != null ? Factory.Load<WhsProductStyleClassification>(Product.OP_WSS_WhsProductStyleClassification) : null;
					styleClassification = new ProductStyleClassificationWrapper(classificationBO, Factory);
				}

				return styleClassification;
			}
		}
		ProductStyleClassificationWrapper styleClassification;

		#endregion

	}
}
