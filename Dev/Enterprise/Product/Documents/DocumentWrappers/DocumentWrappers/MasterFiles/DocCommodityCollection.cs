using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocCommodityCollection : DocumentWrapperCollection
	{
		public DocCommodityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocCommodity this[int index]
		{
			get
			{
				return (DocCommodity)base[index];
			}
		}

		//NOTE: This is here bcoz of the Commodity changes in Shipment to PackLines, so that with this we can go 
		//Commodity.Description and actually string-ing up all the commodity codes from packlines.
		public ZString Description
		{
			get
			{
				ZString result = ZString.Empty;
				DocCommodityCollection dummyColl = this;
				foreach (DocCommodity commodity in dummyColl)
				{
					if (!result.Contains(commodity.Description))
					{
						result += !result.IsEmpty ? ", " : "";
						result += commodity.Description;
					}
				}
				return result;
			}
		}

		public ZString Code
		{
			get
			{
				ZString result = ZString.Empty;
				DocCommodityCollection dummyColl = this;
				foreach (DocCommodity commodity in dummyColl)
				{
					if (!result.Contains(commodity.Code))
					{
						result += !result.IsEmpty ? ", " : "";
						result += commodity.Code;
					}
				}
				return result;
			}
		}

		public ZBool ContainsHazardous
		{
			get
			{
				DocCommodityCollection dummyColl = this;
				foreach (DocCommodity commodity in dummyColl)
				{
					if (commodity.Code == Core.Constants.CargoTypes.Hazardous)
					{
						return ZBool.True;
					}
				}
				return ZBool.False;
			}
		}
	}
}
