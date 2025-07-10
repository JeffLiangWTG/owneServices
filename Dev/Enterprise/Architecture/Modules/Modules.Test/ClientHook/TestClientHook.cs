using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class TestClientHook : ClientHook
	{
		public override Clients Client
		{
			get { return ClientOverride; }
		}

		public Clients ClientOverride = Clients.None;

		public override string ClientDisplayName
		{
			get { return "ClientDisplayName"; }
		}

		#region Instance

		public static TestClientHook Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new TestClientHook();
				}
				return fInstance;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Was static before Extract Test.")]
		static TestClientHook fInstance;

		#endregion

		public int InitialisedCount;

		protected override void InitialiseCore()
		{
			InitialisedCount++;
		}

		protected override void UninitialiseCore()
		{
			InitialisedCount--;
		}

		protected override ITableSchema[] GetTableSchemas()
		{
			return new ITableSchema[]
			{
				new TestTableSchema(),
			};
		}

		[WTG.StaticAnalysis.Annotation.Immutable]
		class TestTableSchema : ITableSchema
		{
			#region ITableSchema Members

			public string SqlSchemaName
			{
				get { return "dbo"; }
			}

			public string TableName
			{
				get { return "ClientTestTable"; }
			}

			public SchemaColumnCollection All
			{
				get { return DummyBizoSchema.All; }
			}

			public SchemaPKColumn PK
			{
				get { return new SchemaPKColumn(CargoWise.Schema.Schema.GenericTableSchema, "T9_PK"); }
			}

			public string PkIndexName => null;

			public SchemaColumn GetSchemaColumn(string columnName)
			{
				return ((ITableSchema)DummyBizoSchema.Instance).GetSchemaColumn(columnName);
			}

			public bool AddParameterSuffixForPKReference
			{
				get { return false; }
			}

			#endregion
		}

		#region NewClientControllers

		public ControllerInfo[] NewClientControllersForTest;

		protected override ControllerInfo[] NewClientControllersCore
		{
			get { return NewClientControllersForTest; }
		}

		#endregion

		#region NewClientModules

		public NewClientModuleInfo[] NewClientModulesForTest;

		protected override NewClientModuleInfo[] NewClientModulesCore
		{
			get { return NewClientModulesForTest; }
		}

		#endregion

		#region NewModuleSectionsToAddForClient

		public IModuleSectionAddOn[] NewModuleSectionsToAddForClientForTest;

		protected override IModuleSectionAddOn[] NewModuleSectionsToAddForClientCore
		{
			get { return NewModuleSectionsToAddForClientForTest; }
		}

		public override bool AddNewModuleSectionsAtTopOfTree
		{
			get { return AddNewModuleSectionsAtTopOfTreeIsSet ? OverriddenAddNewModuleSectionsAtTopOfTree : base.AddNewModuleSectionsAtTopOfTree; }
		}

		bool AddNewModuleSectionsAtTopOfTreeIsSet;
		bool OverriddenAddNewModuleSectionsAtTopOfTree;

		public void SetAddNewModuleSectionsAtTopOfTreeIsSet(bool value)
		{
			AddNewModuleSectionsAtTopOfTreeIsSet = true;
			OverriddenAddNewModuleSectionsAtTopOfTree = value;
		}

		#endregion

		public override IExtensionObjects DbSchemaExtensionObjects
		{
			get { return new ExtensionObjects(TableCreateScripts.ToImmutableArray(), ViewAndRoutineCreateScripts.ToImmutableArray()); }
		}

		public List<DatabaseObjectCreateScript> TableCreateScripts { get; } = new List<DatabaseObjectCreateScript>();
		public List<DatabaseViewAndRoutineCreateScript> ViewAndRoutineCreateScripts { get; } = new List<DatabaseViewAndRoutineCreateScript>();
	}
}
