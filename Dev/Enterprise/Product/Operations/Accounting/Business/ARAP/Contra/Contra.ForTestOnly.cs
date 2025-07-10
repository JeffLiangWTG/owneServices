#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP
{
	public partial class Contra
	{
		public static Contra CreateContra_ForTestOnly(BusinessObjectFactory factory)
		{
			return new Contra(factory);
		}

		public void InitialiseNew_ForTestOnly()
		{
			InitialiseNew();
		}
	}
}

#endif
