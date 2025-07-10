using System;
using System.Globalization;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public class ZoneFilterBuilder
	{
		public ZoneFilterBuilder(Type businessObjectType, ZString zoneName) : this(businessObjectType, zoneName, null)
		{
		}

		public ZoneFilterBuilder(Type businessObjectType, ZString zoneName, SchemaStringColumn postcodeColumn)
		{
			this.businessObjectType = businessObjectType;
			this.zoneName = zoneName;
			this.postcodeColumn = postcodeColumn;
		}

		public void AddToFilter(ZDBOnlyQuery query)
		{
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@ZoneName", zoneName, RateTransportZonesSchema.TZ_ZoneName);
			parameters.Add("@CurrentCompanyOrgProxyPK", GlbCompany.CurrentCompany.GC_OH_OrgProxy, GlbCompanySchema.GC_OH_OrgProxy);

			using (var sql = new StringWriter(CultureInfo.InvariantCulture))
			{
				var existsKeyWord = zoneName == UPEFilterConstants.MetroCountry.Other ? "NOT EXISTS" : "EXISTS";
				var pkColumnName = PKColumn.Name;

				if (postcodeColumn == null)
				{
					sql.WriteLine("{0} IN (", pkColumnName);
					sql.WriteLine(GetTop1_OAPostCodeSql());
					sql.WriteLine("AND {0} ({1} AND {2})", existsKeyWord, GetRateTransportZoneSql(), LinkedConsigneeOrgPostCodeWhereClause);
					sql.WriteLine(")");
				}
				else
				{
					sql.WriteLine("{0} ({1} AND {2})", existsKeyWord, GetRateTransportZoneSql(), UnlinkedConsigneePostCodeWhereClause);
				}

				query.AddFilterAndZSQLParameterCollection(sql.ToString(), parameters);
			}
		}

		string GetTop1_OAPostCodeSql()
		{
			using (var sql = new StringWriter(CultureInfo.InvariantCulture))
			{
				sql.WriteLine("SELECT {0} FROM (", PKColumn.Name);
				sql.WriteLine("       SELECT ROW_NUMBER() OVER(PARTITION BY {0} ORDER BY {1}) as RowNumber, {0}, {2}", PKColumn.Name, OrgAddressCapabilitySchema.Constants.PZ_AddressType, OrgAddressSchema.Constants.OA_PostCode);
				sql.WriteLine("       FROM {0}", TableName);
				sql.WriteLine("            INNER JOIN {0} ON {1}={2}", OrgHeaderSchema.Constants.TableName, JobDeclarationSchema.JE_OH_Importer.Name, OrgHeaderSchema.PK.Name);
				sql.WriteLine("            INNER JOIN {0} ON {1}={2}", OrgAddressSchema.Constants.TableName, OrgAddressSchema.OA_OH.Name, OrgHeaderSchema.PK.Name);
				sql.WriteLine("            INNER JOIN {0} ON ({1}) AND {2}={3}", OrgAddressCapabilitySchema.Constants.TableName, OfficeAddressFilterString, OrgAddressCapabilitySchema.PZ_OA.Name, OrgAddressSchema.PK.Name);
				sql.WriteLine(") InnerTable WHERE RowNumber = 1");
				return sql.ToString();
			}
		}

		string GetRateTransportZoneSql()
		{
			using (var result = new StringWriter(CultureInfo.InvariantCulture))
			{
				result.WriteLine("SELECT 1 FROM {0}", RateTransportProviderSchema.Constants.TableName);
				result.WriteLine("              INNER JOIN {0} ON {1}={2}", RateTransportZonesSchema.Constants.TableName, RateTransportZonesSchema.TZ_TP.Name, RateTransportProviderSchema.PK.Name);
				result.WriteLine("              INNER JOIN {0} ON {1}={2}", RateTransportZoneItemSchema.Constants.TableName, RateTransportZoneItemSchema.TQ_TZ_DomesticZone.Name, RateTransportZonesSchema.PK.Name);
				result.WriteLine("				AND {0}.{1} <> '' AND {0}.{2} <> ''", RateTransportZoneItemSchema.Constants.TableName, RateTransportZoneItemSchema.TQ_FromPostCode.Name, RateTransportZoneItemSchema.TQ_ToPostCode.Name);
				result.WriteLine("WHERE {0}=@CurrentCompanyOrgProxyPK", RateTransportProviderSchema.TP_OH_RelatedParty.Name);
				result.WriteLine("AND {0}", ZoneNameCompareWhereClause);
				return result.ToString();
			}
		}

		string ZoneNameCompareWhereClause
		{
			get
			{
				string result;
				if (zoneName == UPEFilterConstants.MetroCountry.Metro || zoneName == UPEFilterConstants.MetroCountry.Other)
				{
					result = "({ZoneNameColumn} like '%" + UPERateTransportZone.MetroKeyword + "%')";
				}
				else
				{
					result = "{ZoneNameColumn}=@ZoneName";
				}

				return result.Replace("{ZoneNameColumn}", RateTransportZonesSchema.TZ_ZoneName.Name);
			}
		}

		string UnlinkedConsigneePostCodeWhereClause
		{
			get
			{
				return
					GetPadStringWithZerosSql(postcodeColumn.Name) + " >= " + GetPadStringWithZerosSql(RateTransportZoneItemSchema.TQ_FromPostCode.Name) + " AND\r\n" +
					GetPadStringWithZerosSql(postcodeColumn.Name) + " <= " + GetPadStringWithZerosSql(RateTransportZoneItemSchema.TQ_ToPostCode.Name) + "\r\n";
			}
		}

		string LinkedConsigneeOrgPostCodeWhereClause
		{
			get
			{
				return
					GetPadStringWithZerosSql(OrgAddressSchema.Constants.OA_PostCode) + " >= " + GetPadStringWithZerosSql(RateTransportZoneItemSchema.TQ_FromPostCode.Name) + " AND\r\n" +
					GetPadStringWithZerosSql(OrgAddressSchema.Constants.OA_PostCode) + " <= " + GetPadStringWithZerosSql(RateTransportZoneItemSchema.TQ_ToPostCode.Name) + "\r\n";
			}
		}

		string OfficeAddressFilterString
		{
			get
			{
				return string.Format("{0}='{1}' AND {2} = 1", OrgAddressCapabilitySchema.PZ_AddressType.Name, OrgAddressType.Office.Code, OrgAddressCapabilitySchema.PZ_IsMainAddress.Name);
			}
		}

		string GetPadStringWithZerosSql(string columnName)
		{
			return "(replicate('0', 10-LEN(" + columnName + "))+" + columnName + ")";
		}

		string TableName
		{
			get
			{
				if (fTableName == null)
				{
					fTableName = BusinessObjectFactory.GetTableNameFromType(businessObjectType);
				}
				return fTableName;
			}
		}
		string fTableName;

		SchemaGuidColumn PKColumn
		{
			get
			{
				if (fPKColumn == null)
				{
					fPKColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(TableName);
				}
				return fPKColumn;
			}
		}
		SchemaGuidColumn fPKColumn;

		readonly Type businessObjectType;
		readonly ZString zoneName;
		readonly SchemaStringColumn postcodeColumn;
	}
}
