using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.DFD;
using Enterprise.Client.DFD.Business.Organisations;
using Enterprise.Client.DFD.Module;
using Enterprise.Core.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		protected ClientOverride()
		{
		}

		#region Instance
		public static ClientOverride Instance
		{
			get { return instance ?? (instance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride instance;
		#endregion

		#region IClientHook Members

		public override Clients Client
		{
			get { return Clients.DFD; }
		}

		public override string ClientDisplayName
		{
			get { return "DFDS Transport (Hong Kong) Ltd"; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return DFDDataRegistry.Instance; }
		}

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleIdentifier arTransactionID = new ClientOverrideModuleIdentifier(ModuleIDs.ARTransaction);
			ClientOverrideModuleInfo arTransactionInfo = new ClientOverrideModuleInfo(arTransactionID, typeof(DFDARTransactionModule).Assembly.FullName, typeof(DFDARTransactionModule).FullName);
			moduleOverrides.AddModuleOverride(arTransactionInfo);

			ClientOverrideModuleIdentifier organisationID = new ClientOverrideModuleIdentifier(ModuleIDs.Organisation);
			ClientOverrideModuleInfo organisationInfo = new ClientOverrideModuleInfo(organisationID, typeof(DFDOrganisationModule).Assembly.FullName, typeof(DFDOrganisationModule).FullName);
			moduleOverrides.AddModuleOverride(organisationInfo);

			ClientOverrideModuleIdentifier productID = new ClientOverrideModuleIdentifier(ModuleIDs.SupplierPart);
			ClientOverrideModuleInfo productInfo = new ClientOverrideModuleInfo(productID, typeof(DFDOrgSupplierPartModule).Assembly.FullName, typeof(DFDOrgSupplierPartModule).FullName, Core.Constants.CountryCodes.UnitedStates);
			moduleOverrides.AddModuleOverride(productInfo);
			return moduleOverrides;
		}

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (clientTypeDeciders == null)
				{
					Dictionary<Type, ITypeDecider> list = new Dictionary<Type, ITypeDecider>();
					list.Add(typeof(OrgHeader), new TypeDeciderImpl(typeof(DFDOrgHeader)));
					list.Add(typeof(OrgAddress), new TypeDeciderImpl(typeof(DFDOrgAddress)));
					clientTypeDeciders = new TypeDeciderDictionary(list);
				}
				return clientTypeDeciders;
			}
		}
		ITypeDeciderDictionary clientTypeDeciders;

		public override void AddClientSpecificSecurityCheckpointsToSecurityInstance(IZSecurity securityInstance)
		{
			var dfdCheckpoints = new DFDSecurityCheckpoints();
			var orgDSVSpecific = dfdCheckpoints.AddOrgDSVSpecific(securityInstance);
			dfdCheckpoints.AddOrgModifyDetailsARAP(securityInstance, orgDSVSpecific);
			dfdCheckpoints.AddOrgModifyMainAddressARAP(securityInstance, orgDSVSpecific);
			dfdCheckpoints.AddOrgModifyPostalAddress(securityInstance, orgDSVSpecific);
			dfdCheckpoints.AddOrgModifyARAPAddress(securityInstance, orgDSVSpecific);
			dfdCheckpoints.AddOrgLockAddressARAPOP(securityInstance, orgDSVSpecific);
		}

		#endregion
	}
}
