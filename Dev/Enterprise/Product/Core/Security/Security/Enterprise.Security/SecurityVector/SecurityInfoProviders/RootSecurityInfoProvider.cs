using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Core.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Provider
{
	class RootSecurityInfoProvider : SecurityInfoProvider
	{
		public RootSecurityInfoProvider(SecurityCore security)
			: base(null, null)
		{
			this.security = security;

			ModuleTree tree = new ModuleTree();
			IModuleTreeLoader loader = ObjectFactory.Get<IModuleTreeLoader>();
			loader.Initialise(tree, security);
			loader.LoadModules();
			security.LoadPrintQueueSecurityCheckPoints();
			security.LoadDocumentTypeSecurityCheckPoints();

			this.tree = tree;
		}

		public override bool IsRoot
		{
			get { return true; }
		}

		public override SecurityCore Security
		{
			get { return security; }
		}

		public override string Name
		{
			get { return string.Empty; }
		}

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			var categoryProviders = new List<CategorySecurityInfoProvider>();
			foreach (ModuleCategory category in tree.Categories.Values)
			{
				if (category.SecurityCheckpoint != null)
				{
					var provider = new CategorySecurityInfoProvider(this, category);
					provider.FetchForGetChildren();
					categoryProviders.Add(provider);
				}
			}
			foreach (var provider in categoryProviders)
			{
				yield return provider;
			}

			yield return new CheckpointSecurityInfoProvider(this, Security.Notes);
			yield return new CheckpointSecurityInfoProvider(this, Security.DocumentsReports);
			yield return new CheckpointSecurityInfoProvider(this, Security.SpecializedRights);
			yield return new CheckpointSecurityInfoProvider(this, Security.Login);
			yield return new CheckpointSecurityInfoProvider(this, Security.EditUserDefinedFilters);
			yield return new CheckpointSecurityInfoProvider(this, Security.PublishGlobalFilterLayouts);
			yield return new CheckpointSecurityInfoProvider(this, Security.PublishGlobalGridColorSchemes);
			yield return new CheckpointSecurityInfoProvider(this, Security.EditAllGlobalColourSchemes);
			yield return new CheckpointSecurityInfoProvider(this, Security.PublishGlobalUniversalCopyTemplates);
			yield return new CheckpointSecurityInfoProvider(this, Security.AutoRefreshModuleGrids);
			yield return new CheckpointSecurityInfoProvider(this, Security.PublishGlobalNoteTemplates);
			yield return new CheckpointSecurityInfoProvider(this, Security.IgnoreMandatoryToRead);
			yield return new CheckpointSecurityInfoProvider(this, Security.SaveDataImportWizardSettings);
			yield return new CheckpointSecurityInfoProvider(this, Security.ContainsInNumbersAndReferences);
		}

		protected override BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory { NameForDebugging = "Security RootSecurityInfoProvider" };
					factory.SuspendValidation();
					factory.RefreshEnabled = false;
					factory.AddFetchHint(new StmMenuItemHint());
					factory.AddFetchHint(new OperationalActionFetchHint());
				}

				return factory;
			}
		}
		BusinessObjectFactory factory;

		class OperationalActionFetchHint : IFetchHint
		{
			public string BuilderKey
			{
				get { return "RootSecurityInfoProviderOperationalActionFetchHint"; }
			}

			public void GenerateQuery(QueryBuilder builder)
			{
				if (builder.IsEmpty)
				{
					builder.Init(GetQuery(), new ZQuery());
				}
			}

			public IQueryHashKey GetHashKeyObject() { return new FetchHint.EnumerableHashObject { BuilderKey }; }

			public ZQuery GetQuery()
			{
				var filter = new ZQuery();
				filter.AddToFilter(StmMenuItemSchema.SU_MenuType, "ACT");
				var subFilter = new ZQuery();
				subFilter.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_GS_NKStaffCode, "");
				subFilter.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_GS_NKStaffCode, Environment.Env.CurrentUser.Initials);
				filter.AddToFilter(subFilter);
				return filter;
			}

			public bool IsDataHintLoaded { get; set; }

			public bool IsNeeded(QueryHistoryProvider historyProvider)
			{
				bool isNeeded = !historyProvider.IsQueryCached(TableName, GetQuery());
				return isNeeded;
			}

			public IEnumerable<SchemaColumn> LoadWithBlobs
			{
				get { return System.Array.Empty<SchemaColumn>(); }
			}

			public string TableName
			{
				get { return StmMenuItemSchema.Constants.TableName; }
			}
		}

		class StmMenuItemHint : IFetchHint
		{
			public string BuilderKey
			{
				get { return "RootSecurityInfoProviderStmMenuItemHint"; }
			}

			public void GenerateQuery(QueryBuilder builder)
			{
				if (builder.IsEmpty)
				{
					builder.Init(GetQuery(), new ZQuery());
				}
			}

			public IQueryHashKey GetHashKeyObject() { return new FetchHint.EnumerableHashObject { string.Empty }; }

			public ZQuery GetQuery()
			{
				var filter = new DocumentZQuery(); // Constants.BusinessContextPrefixes.Reports + moduleID
				filter.AddToFilter(StmMenuItemSchema.SU_IsPublished, true);
				return filter;
			}

			public bool IsDataHintLoaded { get; set; }

			public bool IsNeeded(QueryHistoryProvider historyProvider)
			{
				bool isNeeded = !historyProvider.IsQueryCached(TableName, new DocumentZQuery());
				return isNeeded;
			}

			public IEnumerable<SchemaColumn> LoadWithBlobs
			{
				get { return System.Array.Empty<SchemaColumn>(); }
			}

			public string TableName
			{
				get { return StmMenuItemSchema.Constants.TableName; }
			}
		}

		readonly ModuleTree tree;
		readonly SecurityCore security;
	}
}
