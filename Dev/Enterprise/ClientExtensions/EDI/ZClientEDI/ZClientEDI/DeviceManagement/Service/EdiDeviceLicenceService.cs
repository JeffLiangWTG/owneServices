using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Services.ServiceHost;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Service
{
	public class EdiDeviceLicenceService : IDeviceLicenceService
	{
		public IEnumerable<CodeDescriptionPair> GetEnterpriseCodeList()
		{
			var collection = new LicenceEnterpriseCollectionForEntCodeFilter(Factory);
			return collection.Select(x => new CodeDescriptionPair(x.LE_EnterpriseCode.ToString(), x.OrganisationName.ToString()));
		}

		public IEnumerable<CodeDescriptionPair> GetServerCodeList(string enterpriseCode)
		{
			if (string.IsNullOrEmpty(enterpriseCode))
			{
				return Enumerable.Empty<CodeDescriptionPair>();
			}

			var collection = new LicenceDatabaseGlobalCollection(Factory);
			return collection
				.Where(x => x.EnterpriseCode == enterpriseCode)
				.Select(x => new CodeDescriptionPair(x.LD_ServerCode.ToString(), x.AddressAsString.ToString()));
		}

		public Guid GetCustomer(string enterpriseCode, string serverCode)
		{
			if (string.IsNullOrEmpty(enterpriseCode) || string.IsNullOrEmpty(serverCode))
			{
				return Guid.Empty;
			}

			var licenceQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
			licenceQuery.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, serverCode);

			var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
			enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode);

			licenceQuery.AddSubQuery(enterpriseSubQuery, JoinCondition.And);

			var licence = Factory.LoadTop1<LicenceDatabase>(licenceQuery);
			var organisation = licence?.LicEnterprise.Organisation;
			if (organisation != null
				&& !organisation.PK.IsEmpty
				&& organisation.PK.IsValid)
			{
				return organisation.PK.ToGuid();
			}

			return Guid.Empty;
		}

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory
		{
			get
			{
				return factory ?? (factory = new BusinessObjectFactory());
			}
		}
	}
}
