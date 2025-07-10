using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobRelatedWayBill : AutoJobRelatedWayBill
	{
		public JobRelatedWayBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class Constants
		{
			public static class RelatedWayBillType
			{
				public const string Parent = "PAR";
				public const string Child = "CHI";
			}
		}
	}
}
