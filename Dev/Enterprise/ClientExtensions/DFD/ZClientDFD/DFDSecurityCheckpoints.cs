using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.DFD
{
	class DFDSecurityCheckpoints
	{
		internal static SecurityCheckpoint OrgDSVSpecific
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(OrgDSVSpecificReferenceName)); }
		}

		internal SecurityCheckpoint AddOrgDSVSpecific(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				OrgDSVSpecificReferenceName,
				(NoResString)OrgDSVSpecificDisplayName,
				securityDefinitions.Organisation
				);

			return result;
		}

		const string OrgDSVSpecificReferenceName = "OrgDSVSpecific";
		const string OrgDSVSpecificDisplayName = "DSV Specific";

		internal static SecurityCheckpoint OrgModifyDetailsARAP
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(OrgModifyDetailsARAPReferenceName)); }
		}

		internal SecurityCheckpoint AddOrgModifyDetailsARAP(IZSecurity securityInstance, SecurityCheckpoint parent)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				OrgModifyDetailsARAPReferenceName,
				(NoResString)OrgModifyDetailsARAPDisplayName,
				parent
				);

			return result;
		}

		const string OrgModifyDetailsARAPReferenceName = "OrgModifyDetailsARAP";
		const string OrgModifyDetailsARAPDisplayName = "Modify Main Details on AR/AP";

		internal static SecurityCheckpoint OrgModifyMainAddressARAP
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(OrgModifyMainAddressARAPReferenceName)); }
		}

		internal SecurityCheckpoint AddOrgModifyMainAddressARAP(IZSecurity securityInstance, SecurityCheckpoint parent)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				OrgModifyMainAddressARAPReferenceName,
				(NoResString)OrgModifyMainAddressARAPDisplayName,
				parent
				);

			return result;
		}

		const string OrgModifyMainAddressARAPReferenceName = "OrgModifyMainAddressARAP";
		const string OrgModifyMainAddressARAPDisplayName = "Modify Main Address on AR/AP";

		internal static SecurityCheckpoint OrgModifyPostalAddress
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(OrgModifyPostalAddressReferenceName)); }
		}

		internal SecurityCheckpoint AddOrgModifyPostalAddress(IZSecurity securityInstance, SecurityCheckpoint parent)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				OrgModifyPostalAddressReferenceName,
				(NoResString)OrgModifyPostalAddressDisplayName,
				parent
				);

			return result;
		}

		const string OrgModifyPostalAddressReferenceName = "OrgModifyPostalAddress";
		const string OrgModifyPostalAddressDisplayName = "Modify Postal Address";

		internal static SecurityCheckpoint OrgModifyARAPAddress
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(OrgModifyARAPAddressReferenceName)); }
		}

		internal SecurityCheckpoint AddOrgModifyARAPAddress(IZSecurity securityInstance, SecurityCheckpoint parent)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				OrgModifyARAPAddressReferenceName,
				(NoResString)OrgModifyARAPAddressDisplayName,
				parent
				);

			return result;
		}

		const string OrgModifyARAPAddressReferenceName = "OrgModifyARAPAddress";
		const string OrgModifyARAPAddressDisplayName = "Modify AR/AP Address";

		internal static SecurityCheckpoint OrgLockAddressARAPOP
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(OrgLockAddressARAPOPReferenceName)); }
		}

		internal SecurityCheckpoint AddOrgLockAddressARAPOP(IZSecurity securityInstance, SecurityCheckpoint parent)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				OrgLockAddressARAPOPReferenceName,
				(NoResString)OrgLockAddressARAPOPDisplayName,
				parent
				);

			return result;
		}

		const string OrgLockAddressARAPOPReferenceName = "OrgLockAddressARAPOP";
		const string OrgLockAddressARAPOPDisplayName = "Add Address Capabilities AR/AP/Office/Postal";
	}
}
