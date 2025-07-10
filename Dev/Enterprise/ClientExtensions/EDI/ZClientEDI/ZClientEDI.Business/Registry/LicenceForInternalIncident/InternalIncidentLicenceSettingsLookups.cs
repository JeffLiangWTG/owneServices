using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class InternalIncidentLicenceSettingsLookups : ZLookups
	{
		public InternalIncidentLicenceSettingsLookups(InternalIncidentLicenceSettings parent)
			: base(parent) { }

		#region Implementation

		protected new InternalIncidentLicenceSettings Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (InternalIncidentLicenceSettings)base.Parent; }
		}

		#endregion

		public LicenceHeaderCollection LicenceList
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
				ZDBOnlySubQuery licCompanySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
				licCompanySubQuery.AddToFilter(LicenceCompanySchema.LC_LE, SQLComparisonOperator.Equal, Parent.LicenceEnterpriseKeys.Cast<LicenceEnterpriseKey>().Select(k => k.LE_PK).ToArray());
				query.AddSubQuery(licCompanySubQuery, JoinCondition.And);

				LicenceHeaderCollection licenceList = new LicenceHeaderCollection(Parent.CurrentFactory, query);

				return licenceList;
			}
		}
	}
}

