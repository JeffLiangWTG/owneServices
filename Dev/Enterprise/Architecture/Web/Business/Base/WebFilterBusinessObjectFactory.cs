using System;
using System.Data;
using System.IO;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class WebFilterBusinessObjectFactory
	{
		public WebFilterBusinessObjectFactory(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		protected internal readonly BusinessObjectFactory Factory;

		#region Load

		public T Load<T>() where T : FilterBusinessObject
		{
			return (T)Load(typeof(T));
		}

		public FilterBusinessObject Load(Type filterBusinessObjectType)
		{
			FilterBusinessObject filterBizO = GetFilterBusinessObject(filterBusinessObjectType, true);
			filterBizO.OnLoaded();
			filterBizO.Factory.InvalidateCachedProperties();
			return filterBizO;
		}

		#endregion

		#region New

		public T New<T>() where T : FilterBusinessObject
		{
			return (T)New(typeof(T));
		}

		public FilterBusinessObject New(Type filterBusinessObjectType)
		{
			return GetFilterBusinessObject(filterBusinessObjectType, false);
		}

		#endregion

		#region Get Filter Business Object

		protected FilterBusinessObject GetFilterBusinessObject(Type filterBusinessObjectType, bool shouldRestorePersistedFilter)
		{
			FilterBusinessObject result;

			if (filterBusinessObjectType.IsSubclassOf(typeof(FilterStripBusinessObject)))
			{
				result = (FilterBusinessObject)Activator.CreateInstance(filterBusinessObjectType);
			}
			else
			{
				if (!filterBusinessObjectType.IsSubclassOf(typeof(FilterBusinessObject)) || GetAutoType(filterBusinessObjectType) == null)
				{
					throw new ArgumentException("The WebFilterBusinessObjectFactory currently only supports creating and loading FilterBusinessObjects");
				}

				DataRow row = ConstructRowFromProperties(GetAutoType(filterBusinessObjectType));

				Type[] paramTypes = { typeof(DataRow), typeof(Type), typeof(ITypeDeciderContext) };
				object[] methodParams = { row, filterBusinessObjectType, null };
				MethodInfo createBusinessObjectMethod = typeof(BusinessObjectFactory).GetMethod("CreateBusinessObject", BindingFlags.Instance | BindingFlags.NonPublic, null, paramTypes, null);
				result = (FilterBusinessObject)createBusinessObjectMethod.Invoke(Factory, methodParams);

				if (shouldRestorePersistedFilter)
				{
					LoadPreviousFilterIntoRow(filterBusinessObjectType, ((INeedRow)result).Row);
				}

				row.Table.Rows.Add(row); // can't do this till defaults are set by BizO constructor
			}

			return result;
		}

		#endregion

		#region Save

		public void Save(FilterBusinessObject bizO)
		{
			StringWriter writer = new StringWriter();
			((IBusinessObjectInternals)bizO).Row.Table.DataSet.WriteXml(writer, XmlWriteMode.WriteSchema);
			EnvProxy.Instance.Registry.SetFilterCriteria(UserPkToStoreFilterCriteriaAgainst.ToGuid(), bizO.GetType().FullName, writer.ToString());
		}

		#endregion

		#region Retrieve

		protected void LoadPreviousFilterIntoRow(Type typeOfFilterBusinessObject, DataRow filterRow)
		{
			string fromRegistry = EnvProxy.Instance.Registry.GetFilterCriteria(UserPkToStoreFilterCriteriaAgainst.ToGuid(), typeOfFilterBusinessObject.FullName);
			if (!string.IsNullOrEmpty(fromRegistry))
			{
				DataSet loadedData = new DataSet();

				try
				{
					loadedData.ReadXml(new StringReader(fromRegistry), XmlReadMode.ReadSchema);
				}
				catch (Exception e) when (!e.IsCriticalException()) { } // it it's invalid XML, who cares. Ignore stored filter.

				if (loadedData.Tables.Count == 1 && loadedData.Tables[0].Rows.Count == 1)
				{
					DataTable table = loadedData.Tables[0];
					DataRow row = table.Rows[0];

					if (table.Columns.Contains(Version))
					{
						if ((int)row[Version] == VersionNumber)
						{
							foreach (DataColumn loadedColumn in table.Columns)
							{
								if (filterRow.Table.Columns.Contains(loadedColumn.ColumnName))
								{
									object value = row[loadedColumn.ColumnName];
									bool isRightDataType = value.GetType().Equals(filterRow.Table.Columns[loadedColumn.ColumnName].DataType);
									if (value != DBNull.Value && isRightDataType)
									{
										filterRow[loadedColumn.ColumnName] = value;
									}
								}
							}
						}
					}
				}
			}
		}

		ZGuid UserPkToStoreFilterCriteriaAgainst
		{
			get { return (WebEnv.CurrentUser != null) ? WebEnv.CurrentUser.PK : Env.CurrentUser.PK; }
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		Type GetAutoType(Type filterBizOType)
		{
			if (filterBizOType == null)
			{
				return null;
			}
			else
			{
				if (filterBizOType.Name.StartsWith("Auto")
					&& filterBizOType.BaseType != null
					&& (filterBizOType.BaseType.Equals(typeof(FilterBusinessObject)) || filterBizOType.BaseType.Equals(typeof(FilterStripBusinessObject))))
				{
					return filterBizOType;
				}
				else
				{
					return GetAutoType(filterBizOType.BaseType);
				}
			}
		}

		DataRow ConstructRowFromProperties(Type type)
		{
			DataSet data = new DataSet(type.FullName);
			DataTable table = new DataTable(type.FullName);
			data.Tables.Add(table);

			PropertyInfo[] infos = type.GetProperties();
			foreach (PropertyInfo info in infos)
			{
				Type propertyType = null;
				if (typeof(IZType).IsAssignableFrom(info.PropertyType))
				{
					if (typeof(MultilingualString).IsAssignableFrom(info.PropertyType))
					{
						propertyType = typeof(string);
					}
					else
					{
						propertyType = ZDataType.ZTypeToBaseType(info.PropertyType);
					}
				}
				else if (info.PropertyType == typeof(SQLComparisonOperator))
				{
					propertyType = info.PropertyType;
				}

				if (propertyType != null
					&& info.Name != "ShowRelatedNotes"
					&& info.Name != "BizObjNameToShowEventsFor"
					&& info.Name != "HumanReadableName"
					&& info.Name != "HumanReadableShortcutName"
					&& info.Name != "HumanReadableItemCode")
				{
					DataColumn column = table.Columns.Add(info.Name, propertyType);
					column.AllowDBNull = IsNullableType(info.PropertyType);
				}
			}

			table.Columns.Add(Version, typeof(int));

			DataRow newRow = table.NewRow();
			newRow[Version] = VersionNumber;

			return newRow;
		}

		bool IsNullableType(Type type)
		{
			return !(type == typeof(ZString) ||
						type == typeof(ZDecimal) ||
						type == typeof(ZInt) ||
						type == typeof(ZByte) ||
						type == typeof(ZBool));
		}

		const int VersionNumber = 1;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		const string Version = "Version";

		#endregion
	}
}
