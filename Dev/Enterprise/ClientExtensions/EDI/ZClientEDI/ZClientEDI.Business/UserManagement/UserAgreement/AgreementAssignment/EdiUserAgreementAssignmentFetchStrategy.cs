using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementAssignmentFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public EdiUserAgreementAssignmentFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		new protected EdiUserAgreementAssignment BusinessObject => (EdiUserAgreementAssignment)base.BusinessObject;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(BusinessObject.ClientAgreementOrgPK):
					case nameof(BusinessObject.CurrentVersion):
					case nameof(BusinessObject.LastAcceptedDateUtc):
						if (BusinessObject.EAE_ParentTableCode.EqualsIgnoringCase(LicenceEnterpriseSchema.Constants.Prefix))
						{
							Factory.AddFetchHint(typeof(LicenceEnterprise), LicenceEnterpriseSchema.PK, BusinessObject.EAE_ParentID);
						}
						else if (BusinessObject.EAE_ParentTableCode.EqualsIgnoringCase(LicenceDatabaseSchema.Constants.Prefix))
						{
							Factory.AddFetchHint(typeof(LicenceDatabase), LicenceDatabaseSchema.PK, BusinessObject.EAE_ParentID);
						}

						break;
				}
			}
		}
	}
}
