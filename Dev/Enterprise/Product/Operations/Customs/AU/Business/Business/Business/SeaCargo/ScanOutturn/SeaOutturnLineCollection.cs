using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaOutturnLineCollection : OutturnLineCollection
	{
		public SeaOutturnLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override OutturnLineCollection CreateNewOutturnLineCollection()
		{
			return new SeaOutturnLineCollection(Factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SeaOutturnLine();
		}
	}
}
