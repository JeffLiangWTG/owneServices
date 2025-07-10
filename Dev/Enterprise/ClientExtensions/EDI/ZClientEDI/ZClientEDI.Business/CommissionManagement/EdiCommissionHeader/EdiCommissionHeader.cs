using System.Data;
using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class EdiCommissionHeader : AccCommissionHeader
	{
		public EdiCommissionHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region AdditionalInfo

		public EdiCommissionHeaderAdditionalInfo AdditionalInfo
		{
			get
			{
				if (additionalInfo == null)
				{
					var query = new ZQuery(EdiCommissionHeaderAdditionalInfoSchema.ECH_CH0, PK);
					query.FetchOnlyFromLocalCache = !IsInDatabase;

					additionalInfo = Factory.LoadTop1<EdiCommissionHeaderAdditionalInfo>(query);
					if (additionalInfo != null)
					{
						RegisterEditableChildObject(additionalInfo);
					}
				}

				if (additionalInfo != null && additionalInfo.IsDeleted)
				{
					return null;
				}

				return additionalInfo;
			}
		}
		EdiCommissionHeaderAdditionalInfo additionalInfo;

		public EdiCommissionHeaderAdditionalInfo GetOrCreateAdditionalInfo()
		{
			if (AdditionalInfo == null)
			{
				additionalInfo = Factory.New<EdiCommissionHeaderAdditionalInfo>();
				using (additionalInfo.SuspendSettingHasChanges())
				{
					additionalInfo.ECH_CH0 = PK;
				}

				RegisterEditableChildObject(additionalInfo);
			}

			return AdditionalInfo;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (AdditionalInfo != null)
			{
				AdditionalInfo.Delete();
			}

			base.Delete();
		}

		#endregion
	}
}

