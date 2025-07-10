using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DepotCusUnderbondCusOutturnHeaderCollection : DependentBusinessObjectCollection<CusUnderbond, CusOutturnHeader>
	{
		public DepotCusUnderbondCusOutturnHeaderCollection(CusOutturnHeader header)
			: base(header)
		{
		}
	}
}
