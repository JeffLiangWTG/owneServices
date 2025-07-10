using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	[CodeProperty(AccGlobalChargeCodeMapSchema.Constants.YG_Code), DescriptionProperty(AccGlobalChargeCodeMapSchema.Constants.YG_Desc)]
	public abstract class GlobalChargeCodeMap : AutoAccGlobalChargeCodeMap
	{
		public GlobalChargeCodeMap(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoAccGlobalChargeCodeMap.Schema
		{
			public const string YG_APChargeCode = "YG_APChargeCode";
			public const string YG_ARChargeCodes = "YG_ARChargeCodes";
		}

		#endregion

		public static readonly GlobalChargeCodeMapTypeDecider TypeDecider = new GlobalChargeCodeMapTypeDecider();
	}
}

