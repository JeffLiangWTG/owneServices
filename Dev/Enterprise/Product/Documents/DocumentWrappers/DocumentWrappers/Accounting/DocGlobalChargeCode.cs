using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GlobalChargeCode;

namespace Enterprise.DocumentWrappers
{
	public class DocGlobalChargeCode : DocBaseWrapper
	{
		DocGlobalChargeCode(GlobalChargeCodeMap globalAccChargeCode, BusinessObjectFactory factoryToWrap)
			: base(globalAccChargeCode, factoryToWrap)
		{
		}

		public static DocGlobalChargeCode New(GlobalChargeCodeMap globalChargeCodeMap, BusinessObjectFactory factoryToWrap)
		{
			if (globalChargeCodeMap == null)
			{
				return null;
			}
			else
			{
				return factoryToWrap.GetCachedValue(globalChargeCodeMap.PK.ToStringKey(), () => new DocGlobalChargeCode(globalChargeCodeMap, factoryToWrap), CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		internal GlobalChargeCodeMap GlobalChargeCodeMap
		{
			get { return (GlobalChargeCodeMap)WrappedObject; }
		}

		public ZString GlobalCode
		{
			get { return GlobalChargeCodeMap.YG_Code; }
		}

		public ZString Description
		{
			get { return GlobalChargeCodeMap.YG_Desc; }
		}
	}
}
