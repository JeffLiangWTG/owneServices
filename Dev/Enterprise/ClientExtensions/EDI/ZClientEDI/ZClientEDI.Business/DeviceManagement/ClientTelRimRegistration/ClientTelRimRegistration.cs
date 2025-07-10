using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientTelRimRegistration : AutoClientTelRimRegistration
	{
		public ClientTelRimRegistration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			clientCompany = new Lazy<ClientCompany>(() =>
			{
				return factory.LoadTop1<ClientCompany>(new ZQuery(ClientCompanySchema.PK, TRR_LCC_ClientCompany)) ?? throw new InvalidOperationException($"{nameof(ClientCompany)} could not be found");
			});

			clientDevice = new Lazy<ClientDeviceHeader>(() =>
			{
				return factory.LoadTop1<ClientDeviceHeader>(new ZQuery(DmgDeviceHeaderSchema.PK, TRR_CDH_ClientDeviceHeader)) ?? throw new InvalidOperationException($"{nameof(ClientDevice)} could not be found");
			});

			organisationCustomCodes = new Lazy<OrgCusCode>(() =>
			{
				return factory.LoadTop1<OrgCusCode>(new ZQuery(OrgCusCodeSchema.PK, TRR_OK_OrgCusCode)) ?? throw new InvalidOperationException($"{nameof(OrganisationCustomCodes)} could not be found");
			});
		}

		[List("Lookups.Schemes")]
		public override ZString TRR_EnrolmentScheme
		{
			get => base.TRR_EnrolmentScheme;
			set => base.TRR_EnrolmentScheme = value;
		}

		public ClientCompany ClientCompany => clientCompany.Value;

		public ClientDeviceHeader ClientDevice => clientDevice.Value;

		public OrgCusCode OrganisationCustomCodes => organisationCustomCodes.Value;

		readonly Lazy<ClientCompany> clientCompany;
		readonly Lazy<ClientDeviceHeader> clientDevice;
		readonly Lazy<OrgCusCode> organisationCustomCodes;
	}
}
