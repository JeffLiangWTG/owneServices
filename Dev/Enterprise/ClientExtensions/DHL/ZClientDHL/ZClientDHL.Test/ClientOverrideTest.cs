using System;
using System.Collections;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using Enterprise.Client.DHL.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.DHL.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : Enterprise.ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		[ExpectNoExceptions]
		public void TestDbSchemaUpgradeInfo()
		{
			Db.Connection.BeginTransaction();
			try
			{
				ArrayList scripts = new ArrayList();
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.TableCreationScripts);
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.ViewAndRoutineCreationScripts);
				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					try
					{
						ExecuteNonQuery(script.DropScript);
					}
					catch
					{
					}
				}

				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					ExecuteNonQuery(script.CreateScript);
				}

				//				AssertEquals("The create script should create the table", true, ExistsDbObject(UPEClientTables.ClientBISIShipmentHeader.TableName));
				//				AssertEquals("The create script should create the table", true, ExistsDbObject(UPEClientTables.ClientBISIShipmentCharge.TableName));
				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					ExecuteNonQuery(script.DropScript);
				}
				//				AssertEquals("The drop script should drop the table", false, ExistsDbObject(UPEClientTables.ClientBISIShipmentHeader.TableName));
				//				AssertEquals("The drop script should drop the table", false, ExistsDbObject(UPEClientTables.ClientBISIShipmentCharge.TableName));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		void ExecuteNonQuery(string sQL)
		{
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.ExecuteNonQuery();
		}

		public void TestModuleOverrides()
		{
			var overrides = ClientOverride.Instance.ModuleOverrides;
			var typePath = overrides[ModuleIDs.Customs.JobDeclaration, Enterprise.Core.Constants.CountryCodes.NewZealand].TypePath;
			AssertEquals(typeof(NZJobDeclarationModule), Type.GetType(typePath));
		}
	}
}
