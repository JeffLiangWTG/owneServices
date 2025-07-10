using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.UserPortal
{
	public class UserPortalDisclaimerBizO : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UserPortalDisclaimerBizO()
		{
		}

		public static bool ShouldShowDisclaimer
		{
			get
			{
				bool result;
				string filterCriteria = Env.Registry.GetFilterCriteria(shouldShowDisclaimerFilterCriteriaName);
				return !bool.TryParse(filterCriteria, out result) || result;
			}
		}

		public ZBool DoNotShowAgainNextTime
		{
			get { return doNotShowAgainNextTime; }
			set { SetNonPersistentPropertyValue(DoNotShowAgainNextTimeInfo, ref doNotShowAgainNextTime, value); }
		}

		public ZPropertyInfo DoNotShowAgainNextTimeInfo
		{
			get { return GetZPropertyInfo(nameof(DoNotShowAgainNextTime)); }
		}

		public void SaveToRegistry()
		{
			Env.Registry.SetFilterCriteria(shouldShowDisclaimerFilterCriteriaName, (!DoNotShowAgainNextTime).ToString());
		}

		ZBool doNotShowAgainNextTime;
		static readonly string shouldShowDisclaimerFilterCriteriaName = typeof(UserPortalDisclaimerBizO).FullName + "_ShouldShowDisclaimer";
	}
}
