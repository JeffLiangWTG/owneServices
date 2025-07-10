
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceConnectionLookups : AutoLicenceConnectionLookups
	{
		public LicenceConnectionLookups(AutoLicenceConnection parent)
			: base(parent)
		{
		}

		#region Remote Access Methods

		public ReadOnlyCodeDescriptionPairList RemoteAccessMethodsList
		{
			get
			{
				if (remoteAccessMethodsCodeDescriptionPairList == null)
				{
					remoteAccessMethodsCodeDescriptionPairList = new DefaultRemoteAccessMethods();

					var extras = EDIDataRegistry.Instance.RemoteAccessMethodDescriptionList.Value;
					foreach (ICodeDescription extra in extras)
					{
						remoteAccessMethodsCodeDescriptionPairList.Add(extra);
					}

					remoteAccessMethodsCodeDescriptionPairList.Sort();
				}
				return remoteAccessMethodsCodeDescriptionPairList;
			}
		}
		CodeDescriptionPairList remoteAccessMethodsCodeDescriptionPairList;

		#endregion

		#region Companies
		public LicenceCompanyCollection Companies
		{
			get
			{
				var companies = new LicenceCompanyCollection(Factory, ZQueryForCompanies);
				companies.Load();
				return companies;
			}
		}

		ZQuery ZQueryForCompanies
		{
			get
			{
				// SELECT LicenceCompany.* FROM dbo.LicenceCompany
				// JOIN dbo.LicenceDatabase ON LD_LE = LC_LE
				// WHERE LD_PK = Parent.LK_LD;

				var query = new ZDBOnlyQuery(typeof(LicenceCompany));

				var enterpriseJoinSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
				var databaseJoinSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.LD_LE);
				databaseJoinSubQuery.AddToFilter(LicenceDatabaseSchema.PK, ((LicenceConnection)Parent).LK_LD);
				enterpriseJoinSubQuery.AddSubQuery(databaseJoinSubQuery, JoinCondition.And);
				query.AddSubQuery(enterpriseJoinSubQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion
	}
}

