using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.DevTools
{
	class BusinessObjectsStackTraceXmlTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public string Name
		{
			get { return "Business Objects stack trace as XML"; }
		}

		public bool AddAsButton
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public void Show(Form form)
		{
			var zWinForm = form as ZForm;
			IBusiness business;
			string message;

			if (zWinForm == null || (business = zWinForm.DataSource as IBusiness) == null || business.Factory == null)
			{
				message = "Unable to find a factory.";
			}
			else
			{
				message = GetXml(((IBusinessObjectFactoryInternals)business.Factory).AllBusinessObjects);
			}

			Globals.Message.ShowInformation(message, "Business Objects stack trace as XML");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Create dummy data")]
		string GetXml(IReadOnlyList<BusinessObject> allBusinessObjects)
		{
			var writer = new StringWriter();
			using (var businessObjectCreationTable = GetBusinessObjectCreationTable())
			using (var dataSet = new DataSet())
			{
				dataSet.Locale = CultureInfo.InvariantCulture;
				dataSet.Tables.Add(businessObjectCreationTable);

				foreach (var bizObj in allBusinessObjects)
				{
					var tablePrefix = bizObj.TablePrefix;
					var propertyInfoHash = bizObj.ZPropertyInfoHash;
					var values = new List<object>(7);
					values.Add(bizObj.GetType().Name);
					values.Add(bizObj.PK.ToGuid());
					AddColumnValue(values, propertyInfoHash, $"{tablePrefix}_SystemCreateTimeUtc");
					AddColumnValue(values, propertyInfoHash, $"{tablePrefix}_SystemCreateUser");
					AddColumnValue(values, propertyInfoHash, $"{tablePrefix}_SystemLastEditTimeUtc");
					AddColumnValue(values, propertyInfoHash, $"{tablePrefix}_SystemLastEditUser");
					values.Add(((IBusinessObjectInternals)bizObj).CreationStackTrace);
					businessObjectCreationTable.Rows.Add(values.ToArray());
				}

				dataSet.WriteXml(writer, XmlWriteMode.WriteSchema);
			}
			return writer.ToString();
		}

		void AddColumnValue(List<object> values, ZPropertyInfoHashtable propertyInfoHash, string propertyName)
		{
			if (propertyInfoHash.GetPropertySafe(propertyName) is ZPropertyInfo info)
			{
				values.Add(info.Value.ToString());
			}
			else
			{
				values.Add(DBNull.Value);
			}
		}

		#region XML Table

		DataTable GetBusinessObjectCreationTable()
		{
			var ruleRelationTable = new DataTable(XMLTableName);
			ruleRelationTable.Columns.Add(TableColumnBusinesObjectType, typeof(string));
			ruleRelationTable.Columns.Add(TableColumnPK, typeof(Guid));
			ruleRelationTable.Columns.Add(TableColumnSystemCreateTimeUtc, typeof(string));
			ruleRelationTable.Columns.Add(TableColumnSystemCreateUser, typeof(string));
			ruleRelationTable.Columns.Add(TableColumnSystemLastEditTimeUtc, typeof(string));
			ruleRelationTable.Columns.Add(TableColumnSystemLastEditUser, typeof(string));
			ruleRelationTable.Columns.Add(TableColumnSystemStackTrace, typeof(string));
			return ruleRelationTable;
		}

		const string TableColumnBusinesObjectType = "BusinesObjectType";
		const string TableColumnPK = "PK";
		const string TableColumnSystemCreateTimeUtc = "SystemCreateTimeUtc";
		const string TableColumnSystemCreateUser = "SystemCreateUser";
		const string TableColumnSystemLastEditTimeUtc = "SystemLastEditTimeUtc";
		const string TableColumnSystemLastEditUser = "SystemLastEditUser";
		const string TableColumnSystemStackTrace = "StackTrace";
		const string XMLTableName = "BusinessObjectCreation";

		#endregion
	}
}
