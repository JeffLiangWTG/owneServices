using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class WebBusinessObjectAnalyzer
	{
		#region Methods

		public bool IsNewAndDefault(BusinessObject bO)
		{
			return IsNewAndDefaultCore(bO);
		}

		#endregion

		#region Implementation

		protected virtual bool IsNewAndDefaultCore(BusinessObject bO)
		{
			return !(bO.HasChanges || bO.IsInDatabase || bO.IsDeleted);
		}

		#endregion
	}
}
