using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.Core.Environment;
#if DEBUG
using Enterprise.Integration;
#endif

namespace Enterprise.ZArchitecture.Modules
{
	[SuppressClassNamesAreUniqueAcrossAssembliesMessage]
	[WTG.StaticAnalysis.Annotation.CodeAlive("Subtypes are loaded dynamically")]
	public abstract class ClientHook : IClientHook, ITableSchemaSource
	{
		#region Initialise / Uninitialise

		public bool IsInitialised
		{
			get { return fInitialised; }
		}

		public void Initialise()
		{
			Initialise(false);
		}

		public void Initialise(bool loggedIn)
		{
			if (fInitialised)
			{
				throw new InvalidOperationException("The client hook is already initialised");
			}
			if (loggedIn || !HasCompanySpecificOverrides)
			{
				InitialiseCore();
				fInitialised = true;
			}
		}

		protected virtual void InitialiseCore()
		{
		}

		public void Uninitialise()
		{
			if (!fInitialised)
			{
				throw new InvalidOperationException("The client hook is already uninitialised");
			}
			UninitialiseCore();
			Overridable.ResetAll();
			fInitialised = false;
		}

		protected virtual void UninitialiseCore()
		{
		}

		bool fInitialised;

		public virtual bool HasCompanySpecificOverrides => false;

		public virtual bool IsCorrectCompanyForOverrides(Guid companyPK)
		{
			return true;
		}

		public virtual void SetupClientSpecificRelationships(object systemSetup)
		{
		}

		public virtual void DeleteClientSpecificOrphans(object logger)
		{
		}

#if DEBUG
		public virtual IClientSpecificArchiveManagerHelper GetClientSpecificArchiveManagerHelper()
		{
			return null;
		}

		public bool? IsCorrectCompanyForOverrides_ForTest;
#endif

		public bool IsUpgrading { get; set; }

		#endregion

		#region Client Code / Name

		public abstract Clients Client { get; }

		public string UniqueId
		{
			get
			{
				var client = Client;
				return client != Clients.None ? client.ToString() : string.Empty;
			}
		}

		public abstract string ClientDisplayName { get; }

		#endregion

		#region Help

		public virtual string HelpWebPage
		{
			get { return ""; }
		}

		#endregion

		#region Security

		public virtual void AddClientSpecificSecurityCheckpointsToSecurityInstance(IZSecurity securityInstance)
		{
		}

		public virtual IEnumerable<ISecurityCheckpoint> GetClientSpecificJobInvoicingCheckpoints(IZSecurity securityInstance)
		{
			return Enumerable.Empty<ISecurityCheckpoint>();
		}

		#endregion

		#region Client Type Deciders

		ITypeDeciderDictionary IClientHook.ClientTypeDeciders
		{
			get { return ClientTypeDeciders; }
		}

		object IClientHook.GetTableSchema(string tableName)
		{
			return GetTableSchema(tableName);
		}

		public virtual ITypeDeciderDictionary ClientTypeDeciders
		{
			get { return null; }
		}

		#endregion

		#region Registry Items

		public virtual IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return null; }
		}

		#endregion

		#region DB Upgrade

		public virtual IExtensionObjects DbSchemaExtensionObjects
		{
			get { return new ExtensionObjects(ImmutableArray<DatabaseObjectCreateScript>.Empty, ImmutableArray<DatabaseViewAndRoutineCreateScript>.Empty); }
		}

		#endregion

		#region Login Functionality

		public virtual bool IsValidLogin(string login, string password)
		{
			return true;
		}

		public virtual string LoginDeniedMessage
		{
			get { return Res.GetString("5fd73f26-26e6-4fe5-99d5-cfba391bb05b", "The user name and/or password is not a valid {0} login.", ClientHookLoader.Instance.ClientHook.ClientDisplayName); }
		}

		#endregion

		#region Controllers

		public ControllerOverrides ControllerOverrides => controllerOverrides ??= GetControllerOverrides();
		ControllerOverrides controllerOverrides;

		protected virtual ControllerOverrides GetControllerOverrides() => new();

#if DEBUG
		public ControllerOverrides GetControllerOverrides_ExposedForTest() => GetControllerOverrides();
#endif

		public ControllerInfo[] NewClientControllers
		{
			get { return NewClientControllersCore; }
		}

		protected virtual ControllerInfo[] NewClientControllersCore
		{
			get { return null; }
		}

#endregion

		#region Modules

		public ModuleOverrides ModuleOverrides => moduleOverrides ??= GetModuleOverrides();
		ModuleOverrides moduleOverrides;

		protected virtual ModuleOverrides GetModuleOverrides() => new();

#if DEBUG
		public ModuleOverrides GetGetModuleOverrides_ExposedForTest() => GetModuleOverrides();
#endif

		public NewClientModuleInfo[] NewClientModules
		{
			get { return NewClientModulesCore; }
		}

		protected virtual NewClientModuleInfo[] NewClientModulesCore
		{
			get { return null; }
		}

		public ModuleIdentifier[] NewReportModules => NewReportModulesCore;

		protected virtual ModuleIdentifier[] NewReportModulesCore => Array.Empty<ModuleIdentifier>();

		#endregion

		#region Sections

		public IModuleSectionAddOn[] NewModuleSectionsToAddForClient
		{
			get { return NewModuleSectionsToAddForClientCore; }
		}

		protected virtual IModuleSectionAddOn[] NewModuleSectionsToAddForClientCore
		{
			get { return null; }
		}

		/// <summary>
		/// By default, client specific module sections are added at the bottom of the tree.
		/// Return true if you want these added at the top of the tree instead.
		/// </summary>
		public virtual bool AddNewModuleSectionsAtTopOfTree
		{
			get { return false; }
		}

		#endregion

		#region DocumentEngineCollectionProviders

		public virtual Dictionary<string, Type> DocumentEngineCollectionProviders
		{
			get { return null; }
		}

		#endregion

		#region DocumentEngineCodeDescriptionPairProviders

		public virtual Dictionary<string, Type> DocumentEngineCodeDescriptionPairProviders
		{
			get { return null; }
		}

		#endregion

		#region ITableSchemaSource Members

		public ITableSchema GetTableSchema(string tableName)
		{
			foreach (ITableSchema tableSchema in TableSchemas)
			{
				if (tableSchema.TableName == tableName)
				{
					return tableSchema;
				}
			}
			return null;
		}

		public ITableSchema[] TableSchemas
		{
			get
			{
				if (fTableSchemas == null)
				{
					fTableSchemas = GetTableSchemas();
				}
				return fTableSchemas;
			}
		}

		ITableSchema[] fTableSchemas;

		protected virtual ITableSchema[] GetTableSchemas()
		{
			return Array.Empty<ITableSchema>();
		}

		public string GetParentTableName(string childTableName)
		{
			return ChildTableToParentTableDictionary.GetValueSafe(childTableName);
		}

		protected internal void AddChildToParentTableMapping(string childTableName, string parentTableName)
		{
			ChildTableToParentTableDictionary.Add(childTableName, parentTableName);
		}

		protected internal bool RemoveChildToParentTableMapping(string childTableName)
		{
			return ChildTableToParentTableDictionary.Remove(childTableName);
		}

		readonly Dictionary<string, string> ChildTableToParentTableDictionary = new();

		#endregion

		#region LogSubscriber List

		/// <summary>
		/// Should actually return an IEnumerable of Enterprise.LogWalker.LogSubscriber
		/// The signature here uses ILogSubscriber because Enterprise.LogWalker.LogSubscriber is not visible from ZModules
		/// </summary>
		public virtual IEnumerable<ILogSubscriber> LogSubscribers
		{
			get { return new List<ILogSubscriber>(); }
		}

		#endregion
	}
}
