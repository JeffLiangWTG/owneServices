using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Microsoft.SqlServer.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.Business
{
	[Immutable]
	public class StringToBusinessObjectFieldConverter : IStringToBusinessObjectFieldConverter
	{
		protected StringToBusinessObjectFieldConverter()
		{
			fMappingOrgPK = Guid.Empty;
		}

		protected StringToBusinessObjectFieldConverter(ZGuid mappingOrgPK)
		{
			if (!mappingOrgPK.IsValid)
			{
				throw new ZException("Invalid MappingOrgPK");
			}
			this.fMappingOrgPK = mappingOrgPK;
		}

		public static StringToBusinessObjectFieldConverter GetInstance(ZGuid mappingOrgPK)
		{
			return new StringToBusinessObjectFieldConverter(mappingOrgPK);
		}

		public static StringToBusinessObjectFieldConverter InstanceForCurrentCompany
		{
			get
			{
				if (fInstanceForCurrentCompany == null || fInstanceForCurrentCompany.MappingOrgPK != GlbCompany.CurrentCompany.GC_OH_OrgProxy)
				{
					if (!GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsValid)
					{
						throw new ArgumentException("CurrentCompany doesn't have an org proxy!");
					}
					fInstanceForCurrentCompany = new StringToBusinessObjectFieldConverter(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				}
				return fInstanceForCurrentCompany;
			}
		}

		public ZGuid MappingOrgPK
		{
			get
			{
				if (!fMappingOrgPK.IsValid)
				{
					throw new NotImplementedException("You must override MappingOrgPK");
				}
				return fMappingOrgPK;
			}
		}
		readonly ZGuid fMappingOrgPK;

		public bool IsFKGuidColumn(string columnName)
		{
			string[] colNameParts = columnName.Split('_');

			return
				colNameParts.Length >= 2
				&& (colNameParts[0].Length == 2 || colNameParts[0].Length == 3)
				&& (colNameParts[1].Length == 2 || colNameParts[1].Length == 3)
				&& (colNameParts.Length == 2 || (colNameParts[2].Length > 0 && !colNameParts[2].StartsWith("NK")));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Diagnostic Error Text")]
		public BusinessObject LoadBOAndExpectOnly1(BusinessObjectFactory factory, Type bizType, ZQuery filter, string nKStringForErrorMessage, INotifications notifications, bool notifyErrorIfNoneFound)
		{
			var result = factory.Load(bizType, filter);
			if (result.Length >= 2)
			{
				notifications.Notify(new ErrorNotification(ErrorType.MoreThan1NKMatch, "object " + bizType.Name + "; " + nKStringForErrorMessage));
			}
			else if (result.Length == 0 && notifyErrorIfNoneFound)
			{
				notifications.Notify(new ErrorNotification(ErrorType.MissingPKFromNK, "object " + bizType.Name + "; " + nKStringForErrorMessage));
			}
			return (result.Length == 0) ? null : result[0];
		}

		protected virtual ZGuid UnmatchOrgPK
		{
			get { return ZGuid.Empty; }
		}

		protected virtual ZDateTime ParseDateTime(string valueAsString)
		{
			throw new NotImplementedException("This must be implemented for the specific client before use");
		}

		protected virtual ZDateTime ParseDateTime(string valueAsString, out bool setOnBusinessObject)
		{
			setOnBusinessObject = true;
			return ParseDateTime(valueAsString);
		}

		protected virtual ZDateTimeOffset ParseDateTimeOffset(string valueAsString)
		{
			throw new NotImplementedException("This must be implemented for the specific client before use");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		protected virtual ZDateTimeOffset ParseDateTimeOffset(string valueAsString, out bool setOnBusinessObject)
		{
			setOnBusinessObject = true;
			return ParseDateTimeOffset(valueAsString);
		}

		protected virtual ZTime ParseTimeSpan(string valueAsString)
		{
			throw new NotImplementedException("This must be implemented for the specific client before use");
		}

		protected virtual ZTime ParseTimeSpan(string valueAsString, out bool setOnBusinessObject)
		{
			setOnBusinessObject = true;
			return ParseTimeSpan(valueAsString);
		}

		protected virtual ZGeography ParseGeography(string valueAsString)
		{
			throw new NotImplementedException("This must be implemented for the specific client before use");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		protected virtual ZGeography ParseGeography(string valueAsString, out bool setOnBusinessObject)
		{
			setOnBusinessObject = true;
			return ParseGeography(valueAsString);
		}

		#region Mapping PK from NK

		public ZGuid GetPKFromNK(BusinessObjectFactory factory, Type bizType, SchemaColumn naturalKey, ZString nk, INotifications notifications)
		{
			ZGuid result = ZGuid.Empty;
			if (!nk.IsEmpty)
			{
				BusinessObject bo = LoadBOAndExpectOnly1(factory, bizType, new ZQuery(naturalKey, SQLComparisonOperator.Equal, nk), nk, notifications, true);
				result = (bo == null) ? ZGuid.Empty : bo.PK;
			}
			return result;
		}

		public ZGuid GetPKFromNKGivenFKType(BusinessObjectFactory factory, string naturalKeyValue, ForeignKeyType foreignKeyType, INotifications notifications)
		{
			ZGuid result = ZGuid.Empty;
			naturalKeyValue = naturalKeyValue.Trim();
			NKColumnValuePair nkColumnValue = NKColumnValuePair.New(naturalKeyValue, foreignKeyType);

			switch (foreignKeyType)
			{
				case ForeignKeyType.OrganisationMatchWithFullName:
					result = GetPKFromOrgFullName(factory, nkColumnValue.NKColumn, nkColumnValue.Value, notifications);
					break;

				case ForeignKeyType.PortNK:
					result = GetPKFromPortName(factory, nkColumnValue.NKColumn, nkColumnValue.Value, notifications);
					break;

				case ForeignKeyType.ContainerCodeNK:
					result = GetPKFromContainerType(factory, nkColumnValue.NKColumn, nkColumnValue.Value, notifications);
					break;

				case ForeignKeyType.ChargeCodeNK:
					result = GetMappedChargeCode(factory, nkColumnValue.Value, notifications);
					break;

				case ForeignKeyType.WarehouseNK:
					result = GetMappedWarehouse(factory, nkColumnValue.Value, notifications);
					break;

				case ForeignKeyType.CountryNK:
					result = GetPKFromCountry(factory, nkColumnValue.NKColumn, nkColumnValue.Value, notifications, true);
					break;

				case ForeignKeyType.CurrencyNK:
					result = GetPKFromCurrencyCode(factory, nkColumnValue.NKColumn, nkColumnValue.Value, notifications);
					break;

				case ForeignKeyType.RefServiceLevelNK:
					result = GetPKFromServiceLevelCode(factory, nkColumnValue.NKColumn, nkColumnValue.Value, notifications);
					break;

				case ForeignKeyType.IntZoneNK:
					result = GetPKFromIntZoneCode(factory, nkColumnValue.NKColumn, nkColumnValue.Value, notifications);
					break;

				default:
					result = GetPKFromNK(factory, nkColumnValue.BizType, nkColumnValue.NKColumn, nkColumnValue.Value, notifications);
					break;
			}

			return result;
		}

		#endregion

		#region SetPropertyInfoValue / ConvertRawStringToZTypeValue

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "string formatting")]
		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, ZDecimal value, int precision, int scale, INotifications notifications, string errorContext)
		{
			if (value.IsWithinSqlPrecisionAndScale(precision, scale))
			{
				propertyInfo.Value = value;
			}
			else
			{
				string message = string.Format("value = {0}, max = {1}.{2}",
					value,
					new string('9', precision - scale),
					new string('9', scale)
				);

				if (string.IsNullOrEmpty(errorContext))
				{
					notifications.Notify(new ErrorNotification(ErrorType.ValueOverflowError, message));
				}
				else
				{
					notifications.Notify(new ErrorNotification(ErrorType.ValueOverflowError, errorContext + "; " + message));
				}
			}
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, DateTime value)
		{
			var dateTime = (value == DateTime.MinValue) ? ZDateTime.Empty : (ZDateTime)value;
			if (propertyInfo.PropertyType.Equals(typeof(ZDate)))
			{
				propertyInfo.Value = dateTime.Date;
			}
			else
			{
				propertyInfo.Value = dateTime;
			}
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, DateTimeOffset value)
		{
			propertyInfo.Value = (value.Ticks == DateTimeOffset.MinValue.Ticks || value.UtcTicks == DateTimeOffset.MinValue.Ticks) ? ZDateTimeOffset.Empty : value;
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, TimeSpan value)
		{
			propertyInfo.Value = new ZTime(value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded constant")]
		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, SqlGeography value)
		{
			propertyInfo.Value = (value.AsTextZM().ToSqlString().ToString() == "POINT EMPTY") ? ZGeography.Empty : (ZGeography)value;
		}

		public void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string valueAsString, INotifications notifications)
		{
			SetPropertyInfoValue(propertyInfo, valueAsString, ForeignKeyType.None, notifications);
		}

		public void SetPropertyInfoValue(ZPropertyInfo property, string valueAsString, INotifications notifications, string errorContext)
		{
			this.SetPropertyInfoValue(property, valueAsString, ForeignKeyType.None, notifications, "");
		}

		public void SetPropertyInfoValue(ZPropertyInfo property, string valueAsString, ForeignKeyType fKType, INotifications notifications)
		{
			this.SetPropertyInfoValue(property, valueAsString, fKType, notifications, "");
		}

		public void SetPropertyInfoValue(ZPropertyInfo property, string valueAsString, ForeignKeyType fKType, INotifications notifications, string errorContext)
		{
			IZType value = ConvertRawStringToZTypeValue(property.PropertyType, fKType, property.BizObj.Factory, valueAsString, notifications);

			// make sure the business object isn't updated if it doesn't need to be
			if (value != null && !property.Value.Equals(value))
			{
				int maxLength = property.MaxLength;

				// make sure the MaxLength of the field isn't exceeded
				if (value is ZString && maxLength != -1 && value.ToString().Length > maxLength)
				{
					string errorPrefix = string.IsNullOrEmpty(errorContext) ? "" : (errorContext + "; ");
					notifications.Notify(new WarningNotification(WarningType.MaxLengthExceeded, errorPrefix + "value=" + valueAsString));
					property.Value = ((ZString)value.ToString()).SubstringSafe(0, maxLength);
				}
				else
				{
					property.Value = value;
				}
			}
		}

		public IZType ConvertRawStringToZTypeValue(Type propertyType, ForeignKeyType fKType, BusinessObjectFactory factory, string valueAsString, INotifications notifications)
		{
			valueAsString = valueAsString == null ? "" : valueAsString.Trim();

			IZType value = null;

			if (fKType == ForeignKeyType.PortNK)
			{
				value = GetMappedPortName(factory, valueAsString, notifications);
			}
			else if (fKType == ForeignKeyType.PackTypeCodeNK)
			{
				ZString eDICode = GetEDIStringCode(valueAsString, factory, Core.Constants.OrgPatternMatchOverrideRelationships.PackageType);
				value = eDICode.IsEmpty ? new ZString(valueAsString) : eDICode;
			}
			else if (fKType == ForeignKeyType.IncoTermNK)
			{
				ZString eDICode = GetEDIStringCode(valueAsString, factory, Core.Constants.OrgPatternMatchOverrideRelationships.IncoTerm);
				value = eDICode.IsEmpty ? new ZString(valueAsString) : eDICode;
			}
			else if (fKType == ForeignKeyType.CountryNK && propertyType == typeof(ZString))
			{
				value = GetCountryZTypeValue(fKType, factory, valueAsString, notifications);
			}
			else if (fKType == ForeignKeyType.CurrencyNK && (propertyType == typeof(ZString) || propertyType == typeof(ZGuid)))
			{
				value = GetCurrencyZTypeValue(propertyType, fKType, factory, valueAsString, notifications);
			}
			else if (fKType == ForeignKeyType.EventCodeNK)
			{
				ZString eDICode = GetEDIStringCode(valueAsString, factory, Core.Constants.OrgPatternMatchOverrideRelationships.EventCode);
				value = eDICode.IsEmpty ? new ZString(valueAsString) : eDICode;
			}
			else if (fKType == ForeignKeyType.RefServiceLevelNK)
			{
				value = GetServiceLevelZTypeValue(propertyType, fKType, factory, valueAsString, notifications);
			}
			else if (fKType == ForeignKeyType.IntZoneNK)
			{
				value = GetIntZoneZTypeValue(propertyType, fKType, factory, valueAsString, notifications);
			}
			else if (propertyType == typeof(ZString))
			{
				value = new ZString(valueAsString);
			}
			else if (valueAsString.Length > 0)
			{
				if (propertyType == typeof(ZInt))
				{
					try
					{
						value = new ZInt(int.Parse(valueAsString));
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, e.Message));
					}
				}
				else if (propertyType == typeof(ZShort))
				{
					try
					{
						value = new ZShort(short.Parse(valueAsString));
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, e.Message));
					}
				}
				else if (propertyType == typeof(ZByte))
				{
					try
					{
						value = new ZByte(byte.Parse(valueAsString));
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, e.Message));
					}
				}
				else if (propertyType == typeof(ZDecimal))
				{
					try
					{
						valueAsString = valueAsString.Replace("..", "."); // hack due to some donkey decimal in some of the files
						value = new ZDecimal(decimal.Parse(valueAsString));
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, e.Message));
					}
				}
				else if (propertyType == typeof(ZDateTime))
				{
					try
					{
						bool setOnBusinessObject;
						ZDateTime dateToSet = ParseDateTime(valueAsString, out setOnBusinessObject);
						if (setOnBusinessObject)
						{
							value = dateToSet;
						}
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, e.Message));
					}
				}
				else if (propertyType == typeof(ZDate))
				{
					try
					{
						bool setOnBusinessObject;
						ZDateTime dateToSet = ParseDateTime(valueAsString, out setOnBusinessObject);
						if (setOnBusinessObject)
						{
							value = dateToSet.Date;
						}
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, e.Message));
					}
				}
				else if (propertyType == typeof(ZDateTimeOffset))
				{
					try
					{
						bool setOnBusinessObject;
						ZDateTimeOffset dateToSet = ParseDateTimeOffset(valueAsString, out setOnBusinessObject);
						if (setOnBusinessObject)
						{
							value = dateToSet;
						}
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, e.Message));
					}
				}
				else if (propertyType == typeof(ZTime))
				{
					try
					{
						bool setOnBusinessObject;
						ZTime timeToSet = ParseTimeSpan(valueAsString, out setOnBusinessObject);
						if (setOnBusinessObject)
						{
							value = timeToSet;
						}
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, e.Message));
					}
				}
				else if (propertyType == typeof(ZGeography))
				{
					try
					{
						bool setOnBusinessObject;
						ZGeography dateToSet = ParseGeography(valueAsString, out setOnBusinessObject);
						if (setOnBusinessObject)
						{
							value = dateToSet;
						}
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, e.Message));
					}
				}
				else if (propertyType == typeof(ZBool))
				{
					try
					{
						value = ZBool.False;
						if (valueAsString.ToUpper() == "Y" || valueAsString.ToUpper() == "TRUE")
						{
							value = ZBool.True;
						}
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, e.Message));
					}
				}
				else if (propertyType == typeof(ZGuid))
				{
					value = GetPKFromNKGivenFKType(factory, valueAsString, fKType, notifications);
				}
				else
				{
					throw new Exception("Unsupported data type '" + propertyType.FullName);
				}
			}
			return value;
		}

		IZType GetCountryZTypeValue(ForeignKeyType fkType, BusinessObjectFactory factory, string valueAsString,
			INotifications notifications)
		{
			RefCountry country = null;
			NKColumnValuePair nkColumnValue = NKColumnValuePair.New(valueAsString, fkType);
			ZGuid countryGuid = GetPKFromCountry(factory, nkColumnValue.NKColumn, valueAsString, notifications, false);

			if (countryGuid.IsValid)
			{
				country = factory.Load<RefCountry>(countryGuid);
			}

			return country != null ? country.RN_Code : new ZString(valueAsString);
		}

		IZType GetCurrencyZTypeValue(Type propertyType, ForeignKeyType fkType, BusinessObjectFactory factory,
			string valueAsString, INotifications notifications)
		{
			IZType value;
			ZGuid currencyGuid;
			NKColumnValuePair nkColumnValue = NKColumnValuePair.New(valueAsString, fkType);

			if (propertyType == typeof(ZString) && valueAsString.Length == 0)
			{
				currencyGuid = ZGuid.Empty;
			}
			else
			{
				currencyGuid = GetPKFromCurrencyCode(factory, nkColumnValue.NKColumn, valueAsString, notifications);
			}

			RefCurrency currency = null;
			if (currencyGuid.IsValid)
			{
				currency = factory.Load<RefCurrency>(currencyGuid);
			}

			if (propertyType == typeof(ZString))
			{
				value = currency != null ? currency.RX_Code : new ZString(valueAsString);
			}
			else
			{
				value = currency != null ? currency.PK : ZGuid.Empty;
			}
			return value;
		}

		IZType GetServiceLevelZTypeValue(Type propertyType, ForeignKeyType fkType, BusinessObjectFactory factory,
			string valueAsString, INotifications notifications)
		{
			IZType value;
			ZGuid serviceLevelGuid;
			NKColumnValuePair nkColumnValue = NKColumnValuePair.New(valueAsString, fkType);

			if (propertyType == typeof(ZString) && valueAsString.Length == 0)
			{
				serviceLevelGuid = ZGuid.Empty;
			}
			else
			{
				serviceLevelGuid = GetPKFromServiceLevelCode(factory, nkColumnValue.NKColumn, valueAsString, notifications);
			}

			RefServiceLevel serviceLevel = null;
			if (serviceLevelGuid.IsValid)
			{
				serviceLevel = factory.Load<RefServiceLevel>(serviceLevelGuid);
			}

			if (propertyType == typeof(ZString))
			{
				value = serviceLevel != null ? serviceLevel.RS_Code : new ZString(valueAsString);
			}
			else
			{
				value = serviceLevel != null ? serviceLevel.PK : ZGuid.Empty;
			}
			return value;
		}

		IZType GetIntZoneZTypeValue(Type propertyType, ForeignKeyType fkType, BusinessObjectFactory factory, string valueAsString, INotifications notifications)
		{
			IZType value;

			NKColumnValuePair nkColumnValue = NKColumnValuePair.New(valueAsString, fkType);
			ZGuid zoneGuid;

			if (propertyType == typeof(ZString) && valueAsString.Length == 0)
			{
				zoneGuid = ZGuid.Empty;
			}
			else
			{
				zoneGuid = GetPKFromIntZoneCode(factory, nkColumnValue.NKColumn, valueAsString, notifications);
			}

			RefZoneHeader zone = null;
			if (zoneGuid.IsValid)
			{
				zone = factory.Load<RefZoneHeader>(zoneGuid);
			}

			if (propertyType == typeof(ZString))
			{
				value = zone != null ? zone.FZ_Code : new ZString(valueAsString);
			}
			else
			{
				value = zone != null ? zone.PK : ZGuid.Empty;
			}
			return value;
		}

		protected ZString GetMappedPortName(BusinessObjectFactory factory, ZString unmappedPortName, INotifications notifications)
		{
			return GetMappedPortNameFromMatcher(new PortMatcher(factory, unmappedPortName, notifications, MappingOrgPK));
		}

		protected ZString GetMappedPortNameFromMatcher(PortMatcher matcher)
		{
			return matcher.Result;
		}

		protected ZGuid GetMappedChargeCode(BusinessObjectFactory factory, ZString unmappedChargeCode, INotifications notifications)
		{
			return new ChargeCodeMatcher(factory, MappingOrgPK, unmappedChargeCode, notifications).Result;
		}

		protected ZGuid GetMappedWarehouse(BusinessObjectFactory factory, ZString whsValue, INotifications notifications)
		{
			return new WarehouseMatcher(factory, MappingOrgPK, whsValue).Result;
		}

		#endregion

		#region Implementation

		[ThreadStatic] static StringToBusinessObjectFieldConverter fInstanceForCurrentCompany;

		ZString GetEDIStringCode(string foreignCode, BusinessObjectFactory factory, string relationship)
		{
			ZString result = "";

			ZQuery orgFilter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, this.MappingOrgPK);
			orgFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, relationship);
			orgFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, foreignCode);

			OrgPatternMatchOverride orgPatternMatch = factory.LoadTop1<OrgPatternMatchOverride>(orgFilter);

			if (orgPatternMatch != null)
			{
				result = orgPatternMatch.OO_LocalCode;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "string formatting")]
		protected ZGuid GetPKFromOrgFullName(BusinessObjectFactory factory, SchemaColumn naturalKeyColumn, string nKValue, INotifications notifications)
		{
			ZGuid result = ZGuid.Empty;
			if (string.IsNullOrWhiteSpace(nKValue))
			{
				notifications.Notify(new ErrorNotification(ErrorType.MissingPKFromNK, "Empty " + DataBoundResourceStrings.GetColumnDescriptiveName(naturalKeyColumn.TableName, naturalKeyColumn.Name) + " full name"));
			}
			else
			{
				OrgHeader currentOrg = (OrgHeader)factory.Load(typeof(OrgHeader), MappingOrgPK);
				OrgMatchResult match = new OrgMatcher(factory).MatchOrgName(currentOrg, nKValue);

				if (match != null)
				{
					switch (match.MatchType)
					{
						case OrgMatchType.ExactMatch:
						case OrgMatchType.OverrideMatch:
							result = match.MatchingOrg.PK;
							break;
						case OrgMatchType.FilteredNameMatch:
							result = match.MatchingOrg.PK;
							notifications.Notify(new ErrorNotification(
								ErrorType.OrgMatchedFuzzy,
								string.Format("'{0}' evaluated as '{1}'", nKValue, match.MatchingOrg.OH_FullName)));
							break;
						case OrgMatchType.MultipleFilteredNameMatches:
						case OrgMatchType.MultipleExactMatches:
							notifications.Notify(new ErrorNotification(ErrorType.MoreThan1NKMatch, nKValue));
							break;
						case OrgMatchType.NoMatchesFound:
							notifications.Notify(new ErrorNotification(ErrorType.MissingPKFromNK, "Organisation: " + nKValue));
							break;
					}
				}
			}

			if (result.IsEmpty)
			{
				result = UnmatchOrgPK;
			}
			return result;
		}

		protected ZGuid GetPKFromPortName(BusinessObjectFactory factory, SchemaColumn naturalKeyColumn, string nKValue, INotifications notifications)
		{
			ZGuid result = ZGuid.Empty;
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JoinCondition.And, OrgPatternMatchOverrideSchema.OO_OH, SQLComparisonOperator.Equal, this.MappingOrgPK);
			filter.AddToFilter(JoinCondition.And, OrgPatternMatchOverrideSchema.OO_Relationship, SQLComparisonOperator.Equal, Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Port);
			filter.AddToFilter(JoinCondition.And, OrgPatternMatchOverrideSchema.OO_ForeignCode, SQLComparisonOperator.Equal, nKValue);

			OrgPatternMatchOverride match = (OrgPatternMatchOverride)factory.LoadTop1(typeof(OrgPatternMatchOverride), filter);
			if (match != null)
			{
				result = match.OO_LocalGuid;
			}
			else
			{
				result = GetPKFromNK(factory, typeof(RefUNLOCO), naturalKeyColumn, nKValue, notifications);
			}
			return result;
		}

		protected virtual ZGuid GetPKFromCountry(BusinessObjectFactory factory, SchemaColumn naturalKeyColumn, string nKValue, INotifications notifications, bool findMatchinReferenceFile)
		{
			ZGuid result = ZGuid.Empty;

			ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, MappingOrgPK);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Country);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, nKValue);

			OrgPatternMatchOverride match = factory.LoadTop1<OrgPatternMatchOverride>(filter);

			if (match != null)
			{
				result = match.OO_LocalGuid;
			}
			else
			{
				if (findMatchinReferenceFile)
				{
					result = GetPKFromNK(factory, typeof(RefCountry), naturalKeyColumn, nKValue, notifications);
				}
			}

			return result;
		}

		protected virtual ZGuid GetPKFromCurrencyCode(BusinessObjectFactory factory, SchemaColumn naturalKeyColumn, string nKValue, INotifications notifications)
		{
			ZGuid result = ZGuid.Empty;

			ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, MappingOrgPK);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Currency);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, nKValue);

			OrgPatternMatchOverride match = factory.LoadTop1<OrgPatternMatchOverride>(filter);

			if (match != null)
			{
				result = match.OO_LocalGuid;
			}
			else
			{
				RefCurrency currencyFromRefFiles = RefCurrency.LoadFromCurrencyCode(factory, nKValue);
				if (currencyFromRefFiles != null)
				{
					result = currencyFromRefFiles.PK;
				}
				else
				{
					notifications.Notify(new WarningNotification(Res.GetString("6f181ce7-2d99-46c4-8322-0571c4a9416e", "Invalid currency specified - {0}", nKValue)));
				}
			}

			return result;
		}

		protected ZGuid GetPKFromContainerType(BusinessObjectFactory factory, SchemaColumn naturalKeyColumn, string nkValue, INotifications notifications)
		{
			ZGuid result = ZGuid.Empty;
			ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, this.MappingOrgPK);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, nkValue);

			OrgPatternMatchOverride match = factory.LoadTop1<OrgPatternMatchOverride>(filter);
			if (match != null)
			{
				result = match.OO_LocalGuid;
			}
			else
			{
				var refContainer = factory.LoadFromNaturalKey<RefContainer>(naturalKeyColumn, nkValue);
				if (refContainer != null)
				{
					result = refContainer.PK;
				}
				else
				{
					notifications.Notify(new WarningNotification(Res.GetString("27ED8157-6752-4D2E-B31E-964B3C857E8F", "Invalid container type specified - {0}", nkValue)));
				}
			}
			return result;
		}

		protected ZGuid GetPKFromServiceLevelCode(BusinessObjectFactory factory, SchemaColumn naturalKeyColumn, string nKValue, INotifications notifications)
		{
			ZGuid result = ZGuid.Empty;

			ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, MappingOrgPK);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.ServiceLevel);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, nKValue);

			OrgPatternMatchOverride match = factory.LoadTop1<OrgPatternMatchOverride>(filter);

			if (match != null)
			{
				result = match.OO_LocalGuid;
			}
			else
			{
				RefServiceLevel serviceLevelFromRefFiles = factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, nKValue);
				if (serviceLevelFromRefFiles != null)
				{
					result = serviceLevelFromRefFiles.PK;
				}
				else
				{
					notifications.Notify(new WarningNotification(Res.GetString("74c6ca7e-492d-4423-9ceb-ce828736ad0a", "Invalid service level specified - {0}", nKValue)));
				}
			}

			return result;
		}

		protected ZGuid GetPKFromIntZoneCode(BusinessObjectFactory factory, SchemaColumn naturalKeyColumn, string nKValue, INotifications notifications)
		{
			ZGuid result = ZGuid.Empty;

			var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, MappingOrgPK);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.IntZone);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, nKValue);

			var match = factory.LoadTop1<OrgPatternMatchOverride>(filter);

			if (match != null)
			{
				result = match.OO_LocalGuid;
			}
			else
			{
				var zoneTypeQuery = new ZQuery(RefZoneHeaderSchema.FZ_Code, nKValue);
				zoneTypeQuery.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, SQLComparisonOperator.NotEqual, RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean);
				var zone = factory.LoadTop1<RefZoneHeader>(zoneTypeQuery);
				if (zone != null)
				{
					result = zone.PK;
				}
				else
				{
					notifications.Notify(new WarningNotification(Res.GetString("ef1b2470-0978-404e-ba44-e756804fad83", "Invalid zone specified - {0}", nKValue)));
				}
			}

			return result;
		}

		#endregion
	}
}
