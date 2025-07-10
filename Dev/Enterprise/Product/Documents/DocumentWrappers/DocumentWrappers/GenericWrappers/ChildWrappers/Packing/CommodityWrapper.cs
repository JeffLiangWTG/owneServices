using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("CodeAndDescription"), WrapperTypeName("Commodity")]
	public class CommodityWrapper : GenericWrapper
	{
		public static class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
		}

		public CommodityWrapper(RefCommodityCode commodity, BusinessObjectFactory factory)
			: base(commodity, factory)
		{
			commodityBO = commodity ?? Factory.GetNull<RefCommodityCode>();
		}

		public ZString Code
		{
			get { return commodityBO.RH_Code; }
		}

		public ZString Description
		{
			get { return commodityBO.RH_DescriptionMultilingual; }
		}

		public ZString CodeAndDescription
		{
			get { return ZString.Format("{0} ({1})", Code, Description); }
		}

		#region Implementation

		readonly RefCommodityCode commodityBO;

		#endregion
	}
}
