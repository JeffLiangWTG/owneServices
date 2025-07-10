using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitHeaderCollection<T> : Enterprise.Customs.ExitControlBase.Business.CusExitHeaderCollection<T>
		where T : CusExitHeader
	{
		public CusExitHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, GetApplicationCodeFilter(filter))
		{
		}

		public CusExitHeaderCollection(BusinessObjectFactory factory)
			: this(factory, filter: null)
		{
		}

		static ZQuery GetApplicationCodeFilter(ZQuery filter)
		{
			var result = new ZQuery(CusExitHeaderSchema.CXH_ApplicationCode, Common.CusExitHeaderApplicationCodeList.Codes.ExitControl);
			if (filter != null)
			{
				result.AddToFilter(filter);
			}
			return result;
		}
	}
}
