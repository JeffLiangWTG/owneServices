using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirOutturnLineCollection : OutturnLineCollection
	{
		public AirOutturnLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override OutturnLineCollection CreateNewOutturnLineCollection()
		{
			return new AirOutturnLineCollection(Factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AirOutturnLine();
		}
	}
}
