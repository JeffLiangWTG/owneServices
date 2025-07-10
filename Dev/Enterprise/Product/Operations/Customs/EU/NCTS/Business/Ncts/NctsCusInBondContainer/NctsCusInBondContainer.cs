using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsCusInBondContainer : BaseCusInBondContainer, Integration.Customs.EU.NCTS.INctsCusInBondContainer
	{
		protected NctsCusInBondContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public new static readonly NctsCusInBondContainerTypeDecider TypeDecider = new NctsCusInBondContainerTypeDecider();

		#endregion
	}
}
