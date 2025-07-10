using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class DeletePeriodsFromSetting : NonPersistentBusinessObject<DeletePeriodsFromSettingValidation>, IObsoleteValidation
	{
		public DeletePeriodsFromSetting()
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string DeletePeriodsFrom = "DeletePeriodsFrom";
		}

		#endregion

		#region DeletePeriodsFrom

		public ZDateTime DeletePeriodsFrom
		{
			get { return fDeletePeriodsFrom; }
			set
			{
				if (fDeletePeriodsFrom != value)
				{
					SetNonPersistentPropertyValue(DeletePeriodsFromInfo, ref fDeletePeriodsFrom, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDeletePeriodsFrom();
					}
				}
			}
		}
		ZDateTime fDeletePeriodsFrom;

		public ZPropertyInfo DeletePeriodsFromInfo
		{
			get { return GetZPropertyInfo(Schema.DeletePeriodsFrom); }
		}

		#endregion

		public override DeletePeriodsFromSettingValidation GetNewValidation()
		{
			return new DeletePeriodsFromSettingValidation(this);
		}
	}
}
