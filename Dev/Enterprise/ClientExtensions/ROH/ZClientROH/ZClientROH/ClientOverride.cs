using System;
using System.Collections.Generic;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.Rohlig;
using Enterprise.Client.Rohlig.DocWrappers;
using Enterprise.Client.Rohlig.GUI;
using Enterprise.Client.Rohlig.Module;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
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

		protected override void InitialiseCore()
		{
			ROHMenuItem.Initialise();
			DocROHARInvoice.RegisterThisSubTypeOverride();
			DocROHForwardingShipment.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.Rohlig; }
		}

		public override string ClientDisplayName
		{
			get { return "Rohlig"; }
		}

		public override string HelpWebPage
		{
			get { return ""; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return RohDataRegistry.Instance; }
		}

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleIdentifier aPTransactionID = new ClientOverrideModuleIdentifier(ModuleIDs.APTransaction);
			ClientOverrideModuleInfo aPTransactionInfo = new ClientOverrideModuleInfo(aPTransactionID, typeof(BellinModule).Assembly.FullName, typeof(BellinModule).FullName);
			moduleOverrides.AddModuleOverride(aPTransactionInfo);
			return moduleOverrides;
		}

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
				result.Add(typeof(ForwardingShipment), new TypeDeciderImpl(typeof(RohForwardingShipment)));
				result.Add(typeof(JobDeclaration), new TypeDeciderImpl(typeof(RohJobDeclaration)));
				return new TypeDeciderDictionary(result);
			}
		}

		public override IExtensionObjects DbSchemaExtensionObjects => null;

		#endregion
	}
}
