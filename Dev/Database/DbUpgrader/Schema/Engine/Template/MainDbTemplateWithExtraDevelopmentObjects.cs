#if DEBUG
using System;
using System.IO;
using System.Xml;

using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	/// <summary>
	/// This class extends the standard template database creator.
	/// It is to be used on DEBUG mode to create extra development objects:
	///   - ZArchitecture Dummy Tables
	///   - (add more here)
	/// </summary>
	public class MainDbTemplateWithExtraDevelopmentObjects : MainDbTemplate
	{
		public MainDbTemplateWithExtraDevelopmentObjects(IUpgradeManager manager, string templateDbName)
			: base(manager, templateDbName)
		{
		}

		protected override void CreateExtraDbObjects(DbConnection conn)
		{
			base.CreateExtraDbObjects(conn);
			CreateExtraDevelopmentObjects(conn);
		}

		void CreateExtraDevelopmentObjects(DbConnection conn)
		{
			try
			{
				new DummyTableCreator(conn).CreateZArchitectureDummyTables();
			}
			catch (Exception e)
			{
				throw new Exception("Failed to create extra development objects (DEBUG MODE ONLY)." + System.Environment.NewLine + e.Message, e);
			}
		}

		#region Dummy Tables

		internal class DummyTableCreator
		{
			public DummyTableCreator(DbConnection conn)
			{
				this.connection = conn;
			}

			readonly DbConnection connection;

			public void CreateZArchitectureDummyTables()
			{
				CreateDummyTable();
				CreateDummyDependentTable();
				CreateDummyPivotTable();
				CreateDummyLoggedTable();
			}

			/// <summary>
			/// This method is duplicated (WITH CHANGES) in ZAchitecture solution for testing purposes (DummyTableCreator class).
			/// Any changes must be propagated.
			/// </summary>
			void CreateDummyTable()
			{
				string dbNamePrefix = "";
				string sqlText = String.Format(GetScriptFromExtraDevelopmentObjectFile("DummyBizo"), dbNamePrefix);
				connection.ExecuteNonQuery(sqlText);

				sqlText = GetScriptFromExtraDevelopmentObjectFile("DummyBizoFK");
				connection.ExecuteNonQuery(sqlText);

				sqlText = GetScriptFromExtraDevelopmentObjectFile("DummyBizoIndexes");
				connection.ExecuteNonQuery(sqlText);
			}

			void CreateDummyDependentTable()
			{
				string sqlText = GetScriptFromExtraDevelopmentObjectFile("DummyDependentBizo");
				connection.ExecuteNonQuery(sqlText);
			}

			void CreateDummyPivotTable()
			{
				string sqlText = GetScriptFromExtraDevelopmentObjectFile("DummyPivot");
				connection.ExecuteNonQuery(sqlText);
			}

			void CreateDummyLoggedTable()
			{
				string sqlText = GetScriptFromExtraDevelopmentObjectFile("DummyLogged");
				connection.ExecuteNonQuery(sqlText);
			}

			/// <summary>
			/// This method is duplicated in ZAchitecture solution for testing purposes (DummyTableCreator class).
			/// Any changes in the text file location or contents might break some ZArchitecture tests.
			/// </summary>
			string GetScriptFromExtraDevelopmentObjectFile(string scriptName)
			{
				foreach (XmlNode scriptNode in ExtraDevelopmentObjectsXmlDoc.DocumentElement.ChildNodes)
				{
					if (String.Compare(scriptNode.Name, scriptName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return scriptNode.InnerText;
					}
				}

				throw new ArgumentException("Invalid Script Name: " + scriptName);
			}

			XmlDocument ExtraDevelopmentObjectsXmlDoc
			{
				get
				{
					if (extraDevelopmentObjectsXmlDoc == null)
					{
						XmlDocument tempDocument = new XmlDocument();
						tempDocument.LoadXml(ExtraDevelopmentObjectsXml);
						extraDevelopmentObjectsXmlDoc = tempDocument;
					}

					return extraDevelopmentObjectsXmlDoc;
				}
			}
			XmlDocument extraDevelopmentObjectsXmlDoc;

			internal static string ExtraDevelopmentObjectsXml
			{
				get
				{
					using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.DbUpgrader.Schema.Template.ExtraDevelopmentObjects.xml"))
					using (StreamReader streamReader = new StreamReader(stream))
					{
						return streamReader.ReadToEnd();
					}
				}
			}
		}
		#endregion
	}
}
#endif
