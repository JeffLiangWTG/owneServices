using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class StmDataCollection : ActiveBusinessObjectCollection<StmData>
	{
		public StmDataCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory)
		{
			this.AdditionalFilter = additionalFilter;
		}
	}
}
