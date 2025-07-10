using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	public class WowTestUtil
	{
		#region Constants

		public const string DummyUnmatchOrgCode = "WOWPTX";
		public const string TestEdiTrackDestinationEmail = "email@emails.com.au";

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public string ReadTestCsvFile(string testFileName)
		{
			FileStream fileStr = new FileStream(TestCase.BaseSourcePath + @"Enterprise\ClientExtensions\WOW\ZClientWOW\ZClientWOW.Test\DataManipulation\" + testFileName, FileMode.Open, FileAccess.Read);
			using (fileStr)
			{
				byte[] bytes = new byte[fileStr.Length];
				fileStr.Read(bytes, 0, (int)fileStr.Length);
				return Encoding.ASCII.GetString(bytes);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public static string GetTestFilePath(string testFileName)
		{
			return Path.Combine(TestCase.BaseSourcePath + @"Enterprise\ClientExtensions\WOW\ZClientWOW\ZClientWOW.Test\DataManipulation\", testFileName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public StreamReader GetStreamReaderForTestFile(string testFileName)
		{
			StreamReader reader = null;
			string dataFile = TestCase.BaseSourcePath + @"Enterprise\ClientExtensions\WOW\ZClientWOW\ZClientWOW.Test\DataManipulation\" + testFileName;
			if (File.Exists(dataFile))
			{
				reader = new StreamReader(dataFile);
			}
			return reader;
		}

		public void AssertBusinessPropsEquals(BusinessObject bO, string[] propertyMappings, params object[] values)
		{
			TestCase.AssertEquals("Property names count doesn't match value count", propertyMappings.Length, values.Length);
			for (int i = 0; i < propertyMappings.Length; i++)
			{
				string propName = propertyMappings[i];
				if (!string.IsNullOrEmpty(propName) && values[i] != null)
				{
					TestCase.AssertEquals(propName, values[i], bO[propName]);
				}
			}
		}

		public OrgHeader SetupDummyUnmatchOrgAndNotifyGroup(BusinessObjectFactory factory)
		{
			OrgHeader result = factory.New<OrgHeader>();
			result.OH_Code = DummyUnmatchOrgCode;
			result.MainAddress.OA_Address1 = "x";

			OrgHeader defaultImporter = factory.LoadTop1<OrgHeader>(new ZQuery());
			// set up the data import notification group
			GlbGroup group = factory.LoadTop1<GlbGroup>(
				new ZQuery(GlbGroupSchema.GG_Code, SQLComparisonOperator.Equal, WowConstants.DataImportNotificationGroupCode));
			if (group == null)
			{
				group = factory.New<GlbGroup>();
				group.GG_Code = WowConstants.DataImportNotificationGroupCode;
			}
			GlbStaff newGroupStaff = group.Staff.AddNew();
			newGroupStaff.GS_Code = "on+";
			newGroupStaff.GS_FullName = "ong_staff";
			newGroupStaff.GS_EmailAddress = "splat@splaty.com";

			WowDataRegistry.Instance.ManagingImportsOutputDirectory = "";
			WowDataRegistry.Instance.UnmatchedDataItemsAccount = result.PK;
			WowDataRegistry.Instance.DeclarationImporter = defaultImporter.PK.ToGuid();
			return result;
		}

		public void DeleteAllOrders(BusinessObjectFactory factory)
		{
			WoolworthsOrder[] ordersToDelete = (WoolworthsOrder[])factory.Load(typeof(WoolworthsOrder), new ZQuery());
			foreach (WoolworthsOrder order in ordersToDelete)
			{
				order.Delete();
			}
		}

		public void RunClientDbCreateScripts()
		{
			var scripts = new List<DatabaseObjectCreateScript>();
			scripts.AddRange(ClientHookLoader.Instance.ClientHook.DbSchemaExtensionObjects.TableCreationScripts);
			scripts.AddRange(ClientHookLoader.Instance.ClientHook.DbSchemaExtensionObjects.ViewAndRoutineCreationScripts);
			foreach (var script in scripts)
			{
				try
				{
					Db.Connection.ExecuteNonQuery(script.DropScript);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
				Db.Connection.ExecuteNonQuery(script.CreateScript);
			}
		}

		#region CheckNoChangesAfterRunningImportSecondTime

		/// <summary>
		/// This checks against changing data that has already been set in the database. This also exposes bugs related
		/// to not checking IsCopying when setting another property from some property setter.
		/// </summary>
		public void CheckNoChangesAfterRunningImportSecondTime(
			BusinessObjectFactory factory, WowDataImporter importer, StreamReader data, INotifications notify)
		{
			((INeedDataSet)factory).Data.AcceptChanges();
			TestCase.AssertEquals("No changes initially", false, ((INeedDataSet)factory).Data.HasChanges());
			new WowTestUtil().HookDataChangedEventsToEnsureNoChangesMade(factory);
			try
			{
				ITransactionParticipant[] transactionActions;
				importer.ImportDataToFactory(data, "", notify, SourceInfo.EmptySourceInfo, out transactionActions);
			}
			finally
			{
				new WowTestUtil().UnhookDataChangedEventsToEnsureNoChangesMade(factory);
			}

			VerifyNoChangesToTable(factory, Order.Schema.TableName);
			VerifyNoChangesToTable(factory, OrderLine.Schema.TableName);
			VerifyNoChangesToTable(factory, OrderLineDelivery.Schema.TableName);
			VerifyNoChangesToTable(factory, OrderLineDeliverContainer.Schema.TableName);
			VerifyNoChangesToTable(factory, WoolworthsProduct.Schema.TableName);
		}

		public void HookDataChangedEventsToEnsureNoChangesMade(BusinessObjectFactory factory)
		{
			foreach (DataTable table in ((INeedDataSet)factory).Data.Tables)
			{
				table.RowChanging += new DataRowChangeEventHandler(OnTable_RowChanging);
			}
		}

		public void UnhookDataChangedEventsToEnsureNoChangesMade(BusinessObjectFactory factory)
		{
			foreach (DataTable table in ((INeedDataSet)factory).Data.Tables)
			{
				table.RowChanging -= new DataRowChangeEventHandler(OnTable_RowChanging);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Testing")]
		protected void OnTable_RowChanging(object sender, DataRowChangeEventArgs e)
		{
			// break here to debug unwelcome changes to data (check the stack trace to find where the bodge edit was)
			System.Windows.Forms.Application.DoEvents();
		}

		protected void VerifyNoChangesToTable(BusinessObjectFactory factory, string tableName)
		{
			DataTable table = ((INeedDataSet)factory).Data.Tables[tableName];
			if (table != null)
			{
				foreach (DataRow row in table.Rows)
				{
					if (row.RowState != DataRowState.Unchanged)
					{
						foreach (DataColumn column in table.Columns)
						{
							object current = row[column];
							object original = row[column, DataRowVersion.Original];
							if (!current.Equals(original))
							{
								TestCase.Fail("Table " + table.TableName + " changed on column " + column.ColumnName +
									"; original value is '" + original + "' current value is '" + current + "'");
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
