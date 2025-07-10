using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.WCB;
using Enterprise.Client.WCB.GUI;
using Enterprise.Client.WCB.Module;
using Enterprise.Customs.AU.Declaration.Business;
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
			get
			{
				if (fInstance == null)
				{
					fInstance = new ClientOverride();
				}
				return fInstance;
			}
		}
		[ThreadStatic]
		static ClientOverride fInstance;

		#endregion

		#region IClientHook Members
		protected override void InitialiseCore()
		{
			WCBMenu.Initialise();
		}

		public override Clients Client
		{
			get { return Clients.WCB; }
		}

		public override string ClientDisplayName
		{
			get { return "Watson Curro Burke Menta"; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return WCBDataRegistry.Instance; }
		}

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleIdentifier moduleID = new ClientOverrideModuleIdentifier(ModuleIDs.Customs.JobDeclaration);
			ClientOverrideModuleInfo moduleInfo = new ClientOverrideModuleInfo(moduleID, typeof(DeclarationModuleOverride).Assembly.FullName, typeof(DeclarationModuleOverride).FullName, Core.Constants.CountryCodes.Australia);
			moduleOverrides.AddModuleOverride(moduleInfo);
			return moduleOverrides;
		}

		#endregion

		#endregion

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (fClientTypeDeciders == null)
				{
					Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
					result.Add(typeof(CusEntryLine), new TypeDeciderImpl(typeof(WCBCusEntryLine)));
					result.Add(typeof(JobDeclaration), new TypeDeciderImpl(typeof(JobDeclarationWithFixedInvHeads)));
					result.Add(typeof(JobComInvoiceHeader), new TypeDeciderImpl(typeof(InvHeadWithFixedInvLines)));
					fClientTypeDeciders = new TypeDeciderDictionary(result);
				}
				return fClientTypeDeciders;
			}
		}
		ITypeDeciderDictionary fClientTypeDeciders;
	}
}
