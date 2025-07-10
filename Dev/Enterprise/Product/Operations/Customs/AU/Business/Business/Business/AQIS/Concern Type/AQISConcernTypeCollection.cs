
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISConcernTypeCollection : AQISSingleValueCollection
	{
		public AQISConcernTypeCollection(BusinessObjectFactory factory, JobDeclaration declaration)
			: base(factory)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public new AQISConcernType this[int index]
		{
			get { return (AQISConcernType)(Elements[index]); }
		}

		public new AQISConcernType AddNew()
		{
			return (AQISConcernType)base.AddNew();
		}

		public JobDeclaration Declaration
		{
			get { return declaration; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AQISConcernType(Factory);
		}

		public bool ContainsConcernType(ZString concernTypeCode)
		{
			foreach (AQISConcernType concernType in this)
			{
				if (concernType.Code == concernTypeCode)
				{
					return true;
				}
			}
			return false;
		}
	}
}
