using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.CommissionManagement.Business
{
	public abstract class CommissionCreator : ICommissionCreator
	{
		#region Constructor

		protected CommissionCreator(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		#endregion

		public readonly BusinessObjectFactory Factory;

		#region CreateCommissions

		public void CreateCommissions()
		{
			CreateCommissions(new CreateCommissionContext());
		}

		public abstract void CreateCommissions(CreateCommissionContext context);

		#endregion
	}
}
