using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.MasterFiles.GUI.MDMSupportCertificateRegistryEditor, Enterprise.MasterFiles.GUI")]
	public class MDMSupportCertificateRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SystemToSystemTrustInfo>
	{
		public MDMSupportCertificateRegistryDataType(MDMProductCodes productCode)
		{
			ProductCode = productCode;
		}

		public MDMProductCodes ProductCode { get; }

		protected override void ValidateCore(IRegistryItem registryItem, SystemToSystemTrustInfo proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (IsEmpty(proposedValue))
			{
				return;
			}

#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			if (IsAnyNullOrEmptyString(proposedValue.ClientId, proposedValue.TenantId, proposedValue.OperationId, proposedValue.PrivateKey) || proposedValue.Certificate.Length == 0)
			{
				throw new RegistryValidationException((NoResString)"For a valid MDM Support Certificate, all fields are required, including three Guid values and two files.");
			}

			if (!AreAllValidIDs(proposedValue.ClientId, proposedValue.TenantId, proposedValue.OperationId))
			{
				throw new RegistryValidationException(string.Format((NoResString)"Client ID, Tenant ID and Target {0} ID are all required to be a unique Guid value.", ProductCode));
			}
		}

		bool IsEmpty(SystemToSystemTrustInfo info) => AreAllNullOrEmptyString(info.ClientId, info.TenantId, info.OperationId, info.PrivateKey) && info.Certificate.Length == 0;
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented

		bool AreAllNullOrEmptyString(params string[] values) => values.All(x => string.IsNullOrEmpty(x));

		bool IsAnyNullOrEmptyString(params string[] values) => values.Any(x => string.IsNullOrEmpty(x));

		bool AreAllValidIDs(params string[] values) => AreAllValidGuidString(values) && AreAllUniqueValues(values);

		bool AreAllValidGuidString(params string[] values) => values.All(x => Guid.TryParse(x, out _));

		bool AreAllUniqueValues(params string[] values) => values.Distinct().Count() == values.Length;
	}
}
