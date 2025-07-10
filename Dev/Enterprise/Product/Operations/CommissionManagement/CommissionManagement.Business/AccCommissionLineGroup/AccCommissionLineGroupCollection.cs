using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionLineGroupCollection : ActiveBusinessObjectCollection<AccCommissionLineGroup>
	{
		#region Constructors

		public AccCommissionLineGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccCommissionLineGroupCollection(AccCommissionHeader header)
			: base(header)
		{
		}

		#endregion

		#region Add

		public AccCommissionLineGroup AddNew(AccChargeCode chargeCode)
		{
			var result = AddNew();
			using (result.GetValidationSuspender())
			{
				result.CLG_AC = chargeCode.PK;
			}

			return result;
		}

		#endregion
	}
}
