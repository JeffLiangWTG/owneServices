using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace ZClientEDI.Business.Registry
{
	public class TcaRimEnrollmentSchemeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TcaRimEnrollmentSchemeCollection>
	{
		protected override TcaRimEnrollmentSchemeCollection DeserialiseCore(byte[] value)
		{
			var collection = base.DeserialiseCore(value);
			return collection;
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, TcaRimEnrollmentSchemeCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var activeCodes = proposedValue
				.GetCodeDescriptionPairList()
				.GetAllCodes();

			var query = new ZQuery();
			foreach (var code in activeCodes)
			{
				query.AddToFilter(ClientTelRimRegistrationSchema.TRR_EnrolmentScheme, SQLComparisonOperator.NotEqual, code);
			}

			var factory = new BusinessObjectFactory();
			if (factory.Exists(typeof(ClientTelRimRegistration), query))
			{
				throw new RegistryValidationException("Scheme is still in use by registrations, cannot remove enrollment scheme");
			}
		}
	}
}
