using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public static class BusinessObjectXmlHelper
	{
		public static void SerialiseXmlColumns(BusinessObject serialisable)
		{
			foreach (var column in serialisable.XmlSerialisedColumns)
			{
				var rootElementName = string.IsNullOrWhiteSpace(column.RootElementName) ? column.XmlColumnName : column.RootElementName;
				var root = new XElement(rootElementName);
				var doc = new XDocument(root);

				foreach (var xmlColumnMember in GetXmlColumnStrategies(serialisable, serialisable, column))
				{
					AddElement(serialisable, serialisable, column, xmlColumnMember, root);
				}

				ZString xml;
				if (doc.Root.HasElements)
				{
					xml = new ZString(doc.ToString());
				}
				else
				{
					xml = ZString.Empty;
				}

				SetXmlColumnPropertyValue(serialisable, column, xml);
			}
		}

		public static void SetXmlColumnPropertyValue(BusinessObject serialisable, XmlColumnSpecification column, ZString xml)
		{
			var propertyInfo = GetXmlColumnPropertyInfo(serialisable, column);
			if (propertyInfo != null)
			{
				propertyInfo.Value = xml;
			}
			else if (column.XmlColumnProperty != null)
			{
				column.XmlColumnProperty.SetValue(serialisable, xml, null);
			}
		}

		static void AddElement(BusinessObject serialisable, BusinessObject bizo, XmlColumnSpecification column, IXmlColumnAccessStrategy xmlColumnMember, XElement root)
		{
			if (typeof(INonPersistentBusinessObjectCollection).IsAssignableFrom(xmlColumnMember.ColumnType))
			{
				var collection = (IEnumerable)xmlColumnMember.GetValue(bizo);
				if (collection != null)
				{
					var collectionElement = new XElement(xmlColumnMember.Name);

					foreach (BusinessObject item in collection)
					{
						var itemElement = GenerateXElementFromBizo(serialisable, item, column);

						collectionElement.Add(itemElement);
					}

					root.Add(collectionElement);
				}
			}
			else if (typeof(IBusiness).IsAssignableFrom(xmlColumnMember.ColumnType))
			{
				var businessObjectElement = new XElement(xmlColumnMember.Name);
				GenerateXElementFromBizoProperties(businessObjectElement, serialisable, (BusinessObject)xmlColumnMember.GetValue(bizo), column);
				root.Add(businessObjectElement);
			}
			else
			{
				var value = xmlColumnMember.GetValue(bizo);
				if (value is ZDateTime)
				{
					var dateTime = (ZDateTime)value;
					value = dateTime.IsValid ? dateTime.SqlFormat : ZString.Empty;
				}
				else if (value is ZDate)
				{
					var date = (ZDate)value;
					value = date.IsValid ? new ZDateTime(date).SqlFormat : ZString.Empty;
				}
				else if (value is ZDateTimeOffset)
				{
					var dateTimeOffset = (ZDateTimeOffset)value;
					value = dateTimeOffset.IsValid ? dateTimeOffset.SqlFormat : ZString.Empty;
				}
				else if (value is ZTime)
				{
					var time = (ZTime)value;
					value = time.IsValid ? time.SqlFormat : ZString.Empty;
				}
				else if (value is ZGeography)
				{
					var geography = (ZGeography)value;
					//value = geography.IsValid ? (ZString)geography.AsText() : (ZString)ZGeography.Empty.ToString();
					value = geography.IsValid ? (ZString)geography.AsText() : ZString.Empty;
				}

				AddNewElement(root, xmlColumnMember.Name, value, xmlColumnMember);
			}
		}

		static XElement GenerateXElementFromBizo(BusinessObject serialisable, BusinessObject bizo, XmlColumnSpecification column)
		{
			var elementName = bizo.GetType().FullName.Replace('+', '-');
			var itemElement = new XElement(elementName);

			GenerateXElementFromBizoProperties(itemElement, serialisable, bizo, column);

			return itemElement;
		}

		static void GenerateXElementFromBizoProperties(XElement itemElement, BusinessObject serialisable, BusinessObject bizo, XmlColumnSpecification column)
		{
			if (bizo != null)
			{
				foreach (var xmlColumnMember in GetXmlColumnStrategies(serialisable, bizo, column))
				{
					AddElement(serialisable, bizo, column, xmlColumnMember, itemElement);
				}
			}
		}

		static void AddNewElement(XElement element, string name, object value, IXmlColumnAccessStrategy xmlColumnMember)
		{
			if (!xmlColumnMember.ColumnAttribute.SerialiseDefaultValues)
			{
				var zType = value as IZType;
				if (zType != null)
				{
					if (AreDefaultValueAndColumnValueEmpty(zType, xmlColumnMember) || AreDefaultValueAndColumnValueSame(zType, xmlColumnMember))
					{
						return;
					}
				}
			}

			if (value is ZBlob)
			{
				value = string.Join(",", (byte[])(ZBlob)value);
			}
			element.Add(new XElement(name, value));
		}

		static bool AreDefaultValueAndColumnValueEmpty(IZType zType, IXmlColumnAccessStrategy xmlColumnMember)
		{
			return xmlColumnMember.DefaultValue == null && zType.IsDefault;
		}

		static bool AreDefaultValueAndColumnValueSame(IZType zType, IXmlColumnAccessStrategy xmlColumnMember)
		{
			return xmlColumnMember.DefaultValue != null && zType.Equals(xmlColumnMember.GetDefaultValueAsZType());
		}

		public static void DeserialiseXmlColumns(BusinessObject serialisable)
		{
			foreach (var column in serialisable.XmlSerialisedColumns)
			{
				var xml = GetXmlColumnPropertyValue(serialisable, column);
				using (var reader = !string.IsNullOrWhiteSpace(xml) ? XmlReader.Create(new StringReader(xml)) : null)
				{
					DeserialiseXmlColumns(serialisable, serialisable, reader, column);
				}
			}
		}

		public static ZString GetXmlColumnPropertyValue(BusinessObject serialisable, XmlColumnSpecification column)
		{
			var propertyInfo = GetXmlColumnPropertyInfo(serialisable, column);
			if (propertyInfo != null)
			{
				return (ZString)propertyInfo.Value;
			}
			else if (column.XmlColumnProperty != null)
			{
				return (ZString)column.XmlColumnProperty.GetValue(serialisable, null);
			}
			else
			{
				throw new ArgumentException("Could not find the property " + column.XmlColumnName);
			}
		}

		static void DeserialiseXmlColumns(BusinessObject parent, BusinessObject serialisable, XmlReader reader, XmlColumnSpecification column)
		{
			var accessorDictionary = GetXmlColumnStrategies(parent, serialisable, column).ToDictionary(s => s.Name);

			if (reader != null)
			{
				DeserialiseXmlColumnsFromRootElement(accessorDictionary, parent, serialisable, reader, column);
			}

			foreach (var columnAccessor in accessorDictionary.Values.Where(d => d.DefaultValue != null))
			{
				SetValue(columnAccessor.DefaultValue, columnAccessor, serialisable);
			}
		}

		static void DeserialiseXmlColumnsFromRootElement(IDictionary<string, IXmlColumnAccessStrategy> accessorDictionary, BusinessObject parent, BusinessObject serialisable, XmlReader reader, XmlColumnSpecification column)
		{
			bool hasBeenAdvancedToNextElementByRead = false;
			reader.Read(); // Read into first item of tree to avoid parsing the root element as a property.
			while (hasBeenAdvancedToNextElementByRead || reader.Read())
			{
				hasBeenAdvancedToNextElementByRead = false;

				var propertyName = reader.LocalName;
				IXmlColumnAccessStrategy columnAccessor = null;
				if (accessorDictionary.TryGetValue(propertyName, out columnAccessor))
				{
					if (reader.IsStartElement())
					{
						if (accessorDictionary.ContainsKey(columnAccessor.Name))
						{
							accessorDictionary.Remove(columnAccessor.Name);
						}

						if (typeof(IZType).IsAssignableFrom(columnAccessor.ColumnType))
						{
							SetValue(reader, columnAccessor, serialisable);
							hasBeenAdvancedToNextElementByRead = true;
						}
						else if (typeof(INonPersistentBusinessObjectCollectionInternal).IsAssignableFrom(columnAccessor.ColumnType))
						{
							if (columnAccessor.GetValue(serialisable) is INonPersistentBusinessObjectCollectionInternal collection)
							{
								ResetCollection(collection);
								collection.InitialiseCollectionForDeserialisation();

								var nonPersistentBizoSubtree = reader.ReadSubtree();
								nonPersistentBizoSubtree.Read(); // Read into first item of tree to avoid parsing the root element as a property.
								nonPersistentBizoSubtree.Read(); // Read into first item of tree to avoid parsing the root element as a property.
								while (!nonPersistentBizoSubtree.EOF)
								{
									if (nonPersistentBizoSubtree.IsStartElement())
									{
										var element = XElement.ReadFrom(nonPersistentBizoSubtree) as XElement;
										if (element != null)
										{
											BusinessObject item = collection.GetItemToDeserialise(element) ?? collection.AddNew();											

											if (item != null)
											{
												using (item.SuspendSettingHasChanges())
												{
													DeserialiseXmlColumns(parent, item, element.CreateReader(), column);
													collection.Add(item);
												}
											}
										}
									}
									else
									{
										nonPersistentBizoSubtree.Read();
									}
								}
							}
						}
						else if (typeof(IBusiness).IsAssignableFrom(columnAccessor.ColumnType))
						{
							var businessObject = columnAccessor.GetValue(serialisable) as BusinessObject;
							var subtree = reader.ReadSubtree();

							if (businessObject == null && !columnAccessor.ColumnType.IsAbstract)
							{
								businessObject = (BusinessObject)Activator.CreateInstance(columnAccessor.ColumnType);
							}
							if (businessObject != null)
							{
								DeserialiseXmlColumns(parent, businessObject, subtree, column);
							}
						}
					}
				}
			}
		}

		static void ResetCollection(INonPersistentBusinessObjectCollection collection)
		{
			if (collection.Count != 0)
			{
				collection.RemoveAndDeleteAll();
			}

			collection.HasChanges = false;
		}

		static void SetValue(XmlReader reader, IXmlColumnAccessStrategy columnAccessor, BusinessObject businessObject)
		{
			SetValue(reader.ReadElementContentAsObject(), columnAccessor, businessObject);
		}

		internal static void SetValue(object rawValue, IXmlColumnAccessStrategy columnAccessor, BusinessObject businessObject)
		{
			if (typeof(IZType).IsAssignableFrom(columnAccessor.ColumnType))
			{
				IZType value;
				var stringValue = rawValue as string ?? string.Empty;

				if (columnAccessor.ColumnType == typeof(ZString))
				{
					// https://stackoverflow.com/a/1793962
					// According to the above answer, the XML spec requires \n as the newline signifier, but in many .NET controls that causes newlines to not be shown.

					value = new ZString(stringValue.Replace("\n", "\r\n"));
				}
				else if (columnAccessor.ColumnType == typeof(ZBlob))
				{
					value = new ZBlob(stringValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => byte.Parse(s, CultureInfo.InvariantCulture)).ToArray());
				}
				else if (string.IsNullOrEmpty(stringValue))
				{
					value = columnAccessor.GetDefaultValueAsZType() ?? ZDataType.ZTypeToEmptyValue(columnAccessor.ColumnType);
				}
				else
				{
					value = ZDataType.ObjectToZType(columnAccessor.ColumnType, rawValue);
				}

				columnAccessor.SetValue(businessObject, value);
			}
		}

		internal static IEnumerable<IXmlColumnAccessStrategy> GetXmlColumnStrategies(BusinessObject businessObject)
		{
			return GetXmlColumnProperties(businessObject, businessObject, null, true).Select(i => new XmlPropertyStrategy(i.PropertyInfo, i.ColumnAttribute));
		}

		static IEnumerable<IXmlColumnAccessStrategy> GetXmlColumnStrategies(BusinessObject parent, BusinessObject entity, XmlColumnSpecification columnSpec)
		{
			IXmlColumnAccessStrategy[] strategies = null;
			var cacheAccessor = Tuple.Create(parent.GetType(), entity.GetType(), columnSpec);
			var cache = AccessorsWithXmlColumnAttributePerType;

			if (!cache.TryGetValue(cacheAccessor, out strategies))
			{
				strategies = GetXmlColumnProperties(parent, entity, columnSpec).Select(i => new XmlPropertyStrategy(i.PropertyInfo, i.ColumnAttribute)).ToArray();
				cache.Add(cacheAccessor, strategies);
			}

			return strategies;
		}

		internal static IEnumerable<(PropertyInfo PropertyInfo, XmlColumnPropertyAttribute ColumnAttribute)> GetXmlColumnProperties(BusinessObject parent, BusinessObject entity, XmlColumnSpecification columnSpec, bool ignoreColumnSpec = false)
		{
			return FilterProperties(entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance), typeof(XmlColumnPropertyAttribute), parent, columnSpec, ignoreColumnSpec);
		}

		static IEnumerable<(T, XmlColumnPropertyAttribute)> FilterProperties<T>(IEnumerable<T> members, Type attributeType, BusinessObject parent, XmlColumnSpecification columnSpec, bool ignoreColumnSpec)
			where T : MemberInfo
		{
			return
				from member in members
				where member != null
				let attribute = GetAttribute(member, attributeType)
				where attribute != null && (ignoreColumnSpec || attribute.XmlColumnName == columnSpec.XmlColumnName || attribute.XmlColumnName == null && parent.XmlSerialisedColumns.Count() == 1)
				select (member, attribute);
		}

		static XmlColumnPropertyAttribute GetAttribute(MemberInfo member, Type attributeType)
		{
			try
			{
				return Attribute.GetCustomAttribute(member, attributeType, inherit: true) as XmlColumnPropertyAttribute;
			}
			catch (AmbiguousMatchException)
			{
				return member.GetCustomAttribute(attributeType, inherit: false) as XmlColumnPropertyAttribute;
			}
		}

		internal static ZPropertyInfo GetXmlColumnPropertyInfo(BusinessObject bizo, XmlColumnSpecification column)
		{
			return bizo.ZPropertyInfoHash.ContainsKey(column.XmlColumnName) ? bizo.ZPropertyInfoHash[column.XmlColumnName] : null;
		}

		#region Cache

		static XmlColumnAccessStrategyCache AccessorsWithXmlColumnAttributePerType
		{
			get { return accessorsWithXmlColumnAttributePerType ?? (accessorsWithXmlColumnAttributePerType = new XmlColumnAccessStrategyCache()); }
		}

		[ThreadStatic]
		static XmlColumnAccessStrategyCache accessorsWithXmlColumnAttributePerType;

		#endregion
	}
}
