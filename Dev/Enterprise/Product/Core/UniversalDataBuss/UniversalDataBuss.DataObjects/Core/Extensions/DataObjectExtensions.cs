using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class DataObjectExtensions
	{
		public static bool HasRecipientRole(this TopLevelDataObject dataObject, RecipientRoleType type)
		{
			var result = false;
			var dataContext = dataObject != null ? dataObject.DataContext : null;
			if (dataContext != null && dataContext.RecipientRoleCollection != null)
			{
				result = dataContext.RecipientRoleCollection.Any(x => x.Code.HasValue && x.Code.Value == type);
			}
			return result;
		}

		public static bool HasRecipientRoleAndService(this TopLevelDataObject dataObject, RecipientRoleType type, ServiceCodeType service)
		{
			return dataObject?.DataContext?.RecipientRoleCollection?.Any(x => x.Code.HasValue && x.Code.Value == type && x.ServiceCode.HasValue && x.ServiceCode.Value == service) ?? false;
		}

		public static ZString GetVesselName(this Shipment shipment, BusinessObjectFactory factory = null)
		{
			if (shipment != null)
			{
				return GetVesselNameFallingBackToLloydsNumberLookup(shipment.VesselName, shipment.LloydsIMO, factory);
			}

			return ZString.Empty;
		}

		public static ZString GetVesselName(this TransportLeg transportLeg, BusinessObjectFactory factory = null)
		{
			if (transportLeg != null)
			{
				return GetVesselNameFallingBackToLloydsNumberLookup(transportLeg.VesselName, transportLeg.VesselLloydsIMO, factory);
			}

			return ZString.Empty;
		}

		static ZString GetVesselNameFallingBackToLloydsNumberLookup(ZString? vesselName, ZString? lloydsIMO, BusinessObjectFactory factory)
		{
			var vesselNameValue = vesselName.GetValueOrDefault();
			if (!vesselNameValue.IsEmpty)
			{
				return vesselNameValue;
			}
			else
			{
				var lloydsIMOValue = lloydsIMO.GetValueOrDefault();
				if (!lloydsIMOValue.IsEmpty)
				{
					if (factory == null)
					{
						factory = new BusinessObjectFactory();
					}

					return ObjectFactory.New<IRefVesselNameFinder>().FindVesselNameByLloydsNumber(lloydsIMOValue, factory);
				}
			}

			return ZString.Empty;
		}

		// UNLOCO is not a ICodeNameDataObject so that it cannot use the ICodeDataObject extension methods. This is to force this method to be used.
		public static ZString GetUNLOCOAsUpperCase(this UNLOCO dataObject, BusinessObjectFactory factory)
		{
			var code = dataObject == null ? ZString.Empty : dataObject.Code.GetValueOrDefault().ToUpper();
			return factory.GetUNLocoFromSuppliedLocationCode(code);
		}

		public static bool TryGetUNLOCOAsUpperCase(this UNLOCO dataObject, BusinessObjectFactory factory, out ZString code)
		{
			code = ZString.Empty;
			bool result = false;

			if (dataObject != null && dataObject.Code.HasValue)
			{
				code = dataObject.GetUNLOCOAsUpperCase(factory);
				result = true;
			}

			return result;
		}

		public static ZString GetCodeAsUpperCase(this ICodeDataObject dataObject)
		{
			return dataObject == null ? ZString.Empty : dataObject.Code.GetValueOrDefault().ToUpper();
		}

		public static ZString? GetNullableCodeAsUpperCase(this ICodeDataObject dataObject)
		{
			return dataObject != null && dataObject.Code.HasValue ? dataObject.GetCodeAsUpperCase() : (ZString?)null;
		}

		public static bool TryGetCodeAsUpperCase(this ICodeDataObject dataObject, out ZString code)
		{
			code = ZString.Empty;
			bool result = false;

			if (dataObject != null && dataObject.Code.HasValue)
			{
				code = dataObject.GetCodeAsUpperCase();
				result = true;
			}

			return result;
		}

		public static string ToStringContents(this ICodeNameDataObject data)
		{
			return data == null ? null : string.IsNullOrEmpty(data.Name) ? (string)data.Code : data.Code + " - " + data.Name;
		}

		public static string ToStringContents(this UNLOCO data)
		{
			return data == null ? null : string.IsNullOrEmpty(data.Name) ? (string)data.Code : data.Code + " - " + data.Name;
		}

		public static string ToStringContents(this ICodeDescriptionDataObject data)
		{
			return data == null ? null : string.IsNullOrEmpty(data.Description) ? (string)data.Code : data.Code + " - " + data.Description;
		}

		public static string ToStringContents<T>(this IEnumerable<T> data, Converter<T, string> converter)
		{
			return data == null ? null : string.Join("\r\n", data.Select(x => converter(x)));
		}

		public static T GetCustomAttribute<T>(this Type type) where T : Attribute
		{
			return GetCustomAttribute<T>(type, true);
		}

		public static T GetCustomAttribute<T>(this Type type, bool isMandatory) where T : Attribute
		{
			var schemaAttributes = type.GetCustomAttributes(typeof(T), false);

			if (schemaAttributes.Length == 1)
			{
				return (T)schemaAttributes[0];
			}

			if (isMandatory && schemaAttributes.Length == 0)
			{
				throw new XmlProcessingException(type, "All DataObjects must have the " + typeof(T).Name + " applied.");
			}

			return null;
		}

		public static T FirstOrDefault<T>(this IEnumerable<T> dataObjectCollection, params ZString[] codes)
			where T : ICodeDataObject
		{
			var result = default(T);
			if (dataObjectCollection != null)
			{
				if (codes == null || codes.Length == 0)
				{
					result = dataObjectCollection.FirstOrDefault(x => true);
				}
				else
				{
					foreach (var code in codes)
					{
						result = dataObjectCollection.FirstOrDefault(x => x.GetCodeAsUpperCase() == code);
						if (result != null)
						{
							break;
						}
					}
				}
			}

			return result;
		}

		public static OrganizationAddress FirstOrDefault(this IEnumerable<OrganizationAddress> organizationAddressCollection, params ZString[] addressTypes)
		{
			OrganizationAddress result = null;
			if (organizationAddressCollection != null)
			{
				if (addressTypes == null || addressTypes.Length == 0)
				{
					result = organizationAddressCollection.FirstOrDefault(x => true);
				}
				else
				{
					foreach (var addressType in addressTypes)
					{
						result = organizationAddressCollection.FirstOrDefault(x => x != null && x.AddressType.GetValueOrDefault() == addressType);
						if (result != null)
						{
							break;
						}
					}
				}
			}
			return result;
		}

		public static Date FirstOrDefault(this List<Date> list, DateType dateType, ZBool isEstimate)
		{
			return list.FirstOrDefault(x => x.Type.GetValueOrDefault() == dateType && x.IsEstimate.GetValueOrDefault() == isEstimate);
		}

		public static Date Add(this List<Date> list, DateType dateType, ZBool isEstimate, ZDateTime date)
		{
			var result = Date.New(dateType, isEstimate, date);
			list.Add(result);
			return result;
		}

		public static CustomizedField Add(this List<CustomizedField> list, ZString key, IZType value)
		{
			var result = CustomizedField.New(key, value);
			list.Add(result);
			return result;
		}

		public static IDataObject Clone(this IDataObject dataObject)
		{
			Type type = dataObject.GetType();
			var properties = type.GetProperties();

			Object result = type.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, dataObject, null);

			foreach (var property in properties)
			{
				if (property.CanWrite)
				{
					property.SetValue(result, property.GetValue(dataObject, null), null);
				}
			}

			return (IDataObject)result;
		}

		public static bool EqualsDateObject(this IDataObject dataObjectX, IDataObject dataObjectY)
		{
			bool result = true;

			if (object.ReferenceEquals(dataObjectX, dataObjectY))
			{
				result = true;
			}
			else if (dataObjectX == null || dataObjectY == null)
			{
				result = false;
			}
			else
			{
				Type typeX = dataObjectX.GetType();
				Type typeY = dataObjectY.GetType();

				result = typeX.Equals(typeY);

				if (result)
				{
					var properties = typeX.GetProperties();

					foreach (var property in properties)
					{
						if (property.CanRead)
						{
							var propertyType = property.PropertyType;
							var valueX = property.GetValue(dataObjectX);
							var valueY = property.GetValue(dataObjectY);

							if (object.ReferenceEquals(valueX, valueY))
							{
								result = true;
							}
							else if (valueX == null || valueY == null)
							{
								result = false;
							}
							else if ((typeof(IDataObject).IsAssignableFrom(propertyType)))
							{
								result = EqualsDateObject((IDataObject)valueX, (IDataObject)valueY);
							}
							else if (propertyType.IsGenericType)
							{
								var genericType = propertyType.GetGenericTypeDefinition();

								if (genericType == typeof(Nullable<>))
								{
									result = object.Equals(valueX, valueY);
								}
								else if (typeof(IList).IsAssignableFrom(propertyType))
								{
									var typeOfContent = propertyType.GetGenericArguments()[0];

									if (typeof(IDataObject).IsAssignableFrom(typeOfContent))
									{
										var listX = (IList)valueX;
										var listY = (IList)valueY;

										result = listX.Count == listY.Count;
										if (result)
										{
											var candidates = listY.Cast<IDataObject>().ToList();

											foreach (IDataObject value in listX)
											{
												var matchedDataObject = candidates.FirstOrDefault(v => value.EqualsDateObject(v));
												result = matchedDataObject != null;
												if (!result)
												{
													break;
												}

												candidates.Remove(matchedDataObject);
											}
										}
									}
								}
							}

							if (!result)
							{
								break;
							}
						}
					}
				}
			}

			return result;
		}

		public static IDictionary<ZString, List<T>> GetOrCreateCodeDictionary<T>(this List<T> collection, ref IDictionary<ZString, List<T>> dictionary, Func<T, ICodeDataObject> getCodeDataObject)
			where T : IDataObject => collection.GetOrCreateCodeDictionary(ref dictionary, x => getCodeDataObject(x).GetCodeAsUpperCase(), key => !key.IsEmpty);

		public static IDictionary<TKey, List<T>> GetOrCreateCodeDictionary<TKey, T>(this List<T> collection, ref IDictionary<TKey, List<T>> dictionary, Func<T, TKey> getKey, Func<TKey, bool> shouldAdd = null)
			where T : IDataObject
		{
			if (dictionary == null)
			{
				dictionary = new Dictionary<TKey, List<T>>();
				if (collection != null)
				{
					foreach (var item in collection)
					{
						var key = getKey(item);
						if (shouldAdd == null || shouldAdd(key))
						{
							var list = dictionary.GetOrAdd(key, () => new List<T>());
							list.Add(item);
						}
					}
				}
			}
			return dictionary;
		}

		#region Has Values Helpers

		public static bool HasTypeOnly(this OrganizationAddress organizationAddress)
		{
			return OnlyHasValuesIn(organizationAddress, "AddressType", "AddressOverride");
		}

		public static bool HasTypeAndCodeOnly(this OrganizationAddress organizationAddress)
		{
			return OnlyHasValuesIn(organizationAddress, "AddressType", "AddressOverride", "OrganizationCode");
		}

		static bool OnlyHasValuesIn(IDataObject organizationAddress, params string[] propertyNames)
		{
			if (organizationAddress == null)
			{
				return true;
			}

			return organizationAddress.GetType().GetProperties()
				.All((property) => propertyNames.Contains(property.Name) || property.InstanceIsNullOrEmpty(organizationAddress));
		}

		static bool InstanceIsNullOrEmpty(this PropertyInfo property, IDataObject organizationAddress)
		{
			var propertyValue = property.GetValue(organizationAddress, null);
			if (propertyValue == null)
			{
				return true;
			}

			if (propertyValue is IZType propertyValueAsIZType)
			{
				return propertyValueAsIZType.IsEmpty;
			}

			if (propertyValue is OrganizationAddressState organizationAddressState)
			{
				return organizationAddressState.IsEmpty();
			}

			return false;
		}

		#endregion

		#region RemoveAddressFromBizo

		public static bool RemoveAddressFromBizo(this OrganizationAddress addressDataObject, IXmlImportLogger logger, BusinessObject bizo, params SchemaColumn[] addressColumns)
		{
			var isToRemove = IsToRemoveAddress(addressDataObject);

			if (isToRemove && bizo != null)
			{
				foreach (var addressColumn in addressColumns)
				{
					SetColumnValueNull(bizo, addressColumn);
				}

				logger.Log(LogType.Information, Res.GetString("355173f9-b0c0-40a3-94de-bc8f64cf4daa", "Set '{0}' to Empty.", addressDataObject.AddressType.GetValueOrDefault()));
			}

			return isToRemove;
		}

		static bool IsToRemoveAddress(OrganizationAddress addressDataObject)
		{
			return addressDataObject.HasTypeOnly();
		}

		static void SetColumnValueNull(BusinessObject bizo, SchemaColumn schemaColumn)
		{
			if (schemaColumn != null)
			{
				bizo[schemaColumn] = null;
			}
		}

		#endregion
	}
}
