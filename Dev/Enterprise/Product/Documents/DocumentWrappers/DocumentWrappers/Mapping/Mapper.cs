using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Mapping
{
	public abstract class Mapper
	{
		protected Mapper(Type wrapperTypeToMap, bool includeChildrenAndRelatedObjects, bool includeIBODocDataProviders, Type baseObjectType, Type baseCollectionType, List<Type> baseParentTypesToHide, string requiredNameSpacePrefix, List<string> additionalNameSpacePrefixes)
		{
			if (!baseObjectType.IsAssignableFrom(wrapperTypeToMap))
			{
				throw new ArgumentException("You can only Map a DocumentWrappper that descends from the " + baseObjectType.Name + " class.");
			}
			TypesAlreadyProcessed = new List<Type>();
			WrapperTypeToMap = wrapperTypeToMap;
			IncludeChildrenAndRelatedObjects = includeChildrenAndRelatedObjects;
			IncludeIBODocDataProviders = includeIBODocDataProviders;
			BaseObjectType = baseObjectType;
			BaseCollectionType = baseCollectionType;
			BaseParentTypesToHide = baseParentTypesToHide;
			RequiredNameSpacePrefix = requiredNameSpacePrefix;
			AdditionalNameSpacePrefixes = additionalNameSpacePrefixes;
		}
		readonly Type WrapperTypeToMap;
		readonly bool IncludeChildrenAndRelatedObjects;
		readonly bool IncludeIBODocDataProviders;
		readonly Type BaseObjectType;
		readonly Type BaseCollectionType;
		readonly List<Type> BaseParentTypesToHide;
		readonly string RequiredNameSpacePrefix;
		readonly List<string> AdditionalNameSpacePrefixes;
		readonly List<Type> TypesAlreadyProcessed;

		public ZString GetMapAsText()
		{
			List<MapTable> tables = GetMapAsTableList();
			ZStringBuilder result = new ZStringBuilder();
			foreach (MapTable table in tables)
			{
				result.Append(table.ToString());
			}
			return result.ToStringWithDelimiterBetweenAppends("\r\n\r\n\r\n");
		}

		public List<MapTable> GetMapAsTableList()
		{
			if (Tables == null)
			{
				Tables = new List<MapTable>();

				if (IncludeChildrenAndRelatedObjects)
				{
					AppendMapForPropertiesOfThisObjectWithRelatedObjectsAndChildren(WrapperTypeToMap);
				}
				else
				{
					AppendMapForPropertiesOfThisObject(WrapperTypeToMap);
				}
				ThrowExceptionIfErrorsExist(Tables);
			}
			return Tables;
		}
		List<MapTable> Tables;

		void ThrowExceptionIfErrorsExist(List<MapTable> tables)
		{
			ZStringBuilder errors = new ZStringBuilder();
			foreach (MapTable table in tables)
			{
				foreach (ZString error in table.Errors)
				{
					errors.Append("-----------------------**************************-----------------------");
					errors.Append((NoResString)"Table: " + table.TypeTitle);
					errors.Append(error);
				}
			}
			if (!errors.IsEmpty)
			{
				throw new Exception("Error mapping " + WrapperTypeToMap.ToString() + "\r\n" + errors.ToStringWithNewLineBetweenAppends());
			}
		}

		#region Implementation

		void AppendMapForPropertiesOfThisObjectWithRelatedObjectsAndChildren(Type wrapperTypeToMap)
		{
			List<Type> relatedTypes = new List<Type>();
			AppendMapForPropertiesOfThisObjectAndItsChildren(wrapperTypeToMap, relatedTypes);

			foreach (Type relatedTypeToMap in relatedTypes)
			{
				AppendMapForPropertiesOfThisObjectWithRelatedObjectsAndChildren(relatedTypeToMap);
			}
		}

		void AppendMapForPropertiesOfThisObjectAndItsChildren(Type typeToMap, List<Type> relatedTypes)
		{
			List<Type> childCollectionTypes = new List<Type>();
			AppendMapForPropertiesOfThisObject(typeToMap, relatedTypes, childCollectionTypes);
			foreach (Type childCollectionTypeToMap in childCollectionTypes)
			{
				CodeDescriptionPairList indexerValuesList = CustomIndexerListAttribute.GetListForIndexer(childCollectionTypeToMap);
				if (indexerValuesList != null)
				{
					AppendMapForPropertiesOfThisCollection(childCollectionTypeToMap, indexerValuesList);
				}

				System.Reflection.MethodInfo typedIndexerMethodInfo = childCollectionTypeToMap.GetMethod("get_Item", new Type[] { typeof(int) });
				if (typedIndexerMethodInfo != null)
				{
					Type childTypeToMap = typedIndexerMethodInfo.ReturnType;
					AppendMapForPropertiesOfThisObjectAndItsChildren(childTypeToMap, relatedTypes);
				}
			}
		}

		void AppendMapForPropertiesOfThisObject(Type wrapperTypeToMap)
		{
			AppendMapForPropertiesOfThisObject(wrapperTypeToMap, null, null);
		}

		void AppendMapForPropertiesOfThisObject(Type typeToMap, List<Type> relatedTypes, List<Type> childCollectionTypes)
		{
			if (!TypesAlreadyProcessed.Contains(typeToMap))
			{
				GenericWrapperProcessor genericWrapperProcessor = new GenericWrapperProcessor(this);
				ZString typeTitle = genericWrapperProcessor.GetFormattedTypeName(typeToMap);
				ZString defaultFieldName = DefaultFieldAttribute.GetDefaultFieldName(typeToMap);
				bool isGenericWrapperType = BaseObjectType.IsAssignableFrom(typeToMap);
				MapTable table = new MapTable(typeTitle, defaultFieldName, (NoResString)"Name", 38, (NoResString)"Type", 30, !isGenericWrapperType);

				if (BaseObjectType.IsAssignableFrom(typeToMap))
				{
					genericWrapperProcessor.AppendMapLines(typeToMap, false, table, "1", relatedTypes);
					new IZTypeProcessor(this).AppendMapLines(typeToMap, false, table, "2", null);
					new GenericWrapperCollectionProcessor(this).AppendMapLines(typeToMap, true, table, "3", childCollectionTypes);
				}
				else if (IncludeIBODocDataProviders && BODocDataProvider.IsBODocDataProvider(typeToMap))
				{
					new IZTypeProcessor(this).AppendMapLines(typeToMap, false, table, "1", null);
				}

				Tables.Add(table);
				TypesAlreadyProcessed.Add(typeToMap);
			}
		}

		void AppendMapForPropertiesOfThisCollection(Type typeToMap, CodeDescriptionPairList indexerValuesList)
		{
			if (!TypesAlreadyProcessed.Contains(typeToMap))
			{
				ZString typeTitle = new GenericWrapperCollectionProcessor(this).GetFormattedTypeName(typeToMap) + (NoResString)" - Available Types";
				MapTable table = new MapTable(typeTitle, "", (NoResString)"Code", 23, (NoResString)"Description", 45, false);

				foreach (CodeDescriptionPair indexer in indexerValuesList)
				{
					table.AddLine(indexer.Code, indexer.Code, indexer.Description);
				}

				Tables.Add(table);
				TypesAlreadyProcessed.Add(typeToMap);
			}
		}

		interface ICanFormatATypeName
		{
			string GetFormattedTypeName(Type typeRequiringName);
		}

		abstract class MapLineProcessor : ICanFormatATypeName
		{
			protected MapLineProcessor(Mapper mapper)
			{
				Mapper = mapper;
			}
			protected readonly Mapper Mapper;

			public void AppendMapLines(Type typeToMap, bool prependBlankLineIfLinesExist, MapTable table, string sortPrefix, List<Type> relatedTypes)
			{
				bool addedAtLeastOneLine = false;
				foreach (System.Reflection.PropertyInfo propertyInfo in typeToMap.GetProperties())
				{
					Type declaringType = propertyInfo.DeclaringType;
					if (!declaringType.IsAssignableFrom(Mapper.BaseObjectType)
						&& !Mapper.BaseParentTypesToHide.Contains(declaringType)
						&& CanProcess(propertyInfo.PropertyType)
						&& propertyInfo.PropertyType != typeof(ZGuid))
					{
						string fieldName = propertyInfo.Name;
						string fieldType = GetFormattedTypeName(propertyInfo.PropertyType);
						table.AddLine(sortPrefix + GetSortString(fieldName, fieldType), fieldName, fieldType);
						addedAtLeastOneLine = true;

						if (relatedTypes != null)
						{
							Type relatedType = propertyInfo.PropertyType;
							if (!relatedTypes.Contains(relatedType))
							{
								if (Mapper.RequiredNameSpacePrefix == null || relatedType.Namespace.StartsWith(Mapper.RequiredNameSpacePrefix)
									|| (Mapper.AdditionalNameSpacePrefixes != null && typeToMap.Namespace.StartsWith(Mapper.RequiredNameSpacePrefix) && Mapper.AdditionalNameSpacePrefixes.Contains(relatedType.Namespace)))
								{
									relatedTypes.Add(relatedType);
								}
							}
						}
					}
				}
				if (prependBlankLineIfLinesExist && addedAtLeastOneLine)
				{
					table.AddLine(sortPrefix, "", "");
				}
			}

			protected abstract string GetSortString(string name, string type);
			public string GetFormattedTypeName(Type typeRequiringName)
			{
				if (!CanProcess(typeRequiringName))
				{
					throw new ArgumentOutOfRangeException(string.Format("Can only return a Formatted Name for types descending from {0}. (You passed a {1})", TypeOfPropertyToMap.FullName, typeRequiringName.FullName));
				}
				return GetFormattedTypeNameCore(typeRequiringName);
			}
			protected virtual string GetFormattedTypeNameCore(Type typeRequiringName)
			{
				return WrapperTypeNameAttribute.GetName(typeRequiringName);
			}
			protected abstract Type TypeOfPropertyToMap { get; }
			protected internal virtual bool CanProcess(Type typeToProcess)
			{
				return TypeOfPropertyToMap.IsAssignableFrom(typeToProcess);
			}
		}

		class IZTypeProcessor : MapLineProcessor
		{
			public IZTypeProcessor(Mapper mapper)
				: base(mapper)
			{
			}

			protected override string GetSortString(string name, string type)
			{
				return name.PadRight(38) + type;
			}

			protected override string GetFormattedTypeNameCore(Type typeRequiringName)
			{
				if (typeRequiringName.Name.StartsWith("Z"))
				{
					return typeRequiringName.Name.Substring(1);
				}
				else
				{
					return typeRequiringName.Name;
				}
			}

			protected override Type TypeOfPropertyToMap
			{
				get { return typeof(IZType); }
			}
		}

		class GenericWrapperProcessor : MapLineProcessor
		{
			public GenericWrapperProcessor(Mapper mapper)
				: base(mapper)
			{
			}

			protected override string GetSortString(string name, string type)
			{
				return type.PadRight(30) + name;
			}

			protected override Type TypeOfPropertyToMap
			{
				get { return Mapper.BaseObjectType; }
			}

			protected internal override bool CanProcess(Type typeToProcess)
			{
				return
					(Mapper.IncludeIBODocDataProviders && BODocDataProvider.IsBODocDataProvider(typeToProcess))
					|| base.CanProcess(typeToProcess);
			}
		}

		class GenericWrapperCollectionProcessor : MapLineProcessor
		{
			public GenericWrapperCollectionProcessor(Mapper mapper)
				: base(mapper)
			{
			}

			protected override string GetSortString(string name, string type)
			{
				return type.PadRight(30) + name;
			}

			protected override Type TypeOfPropertyToMap
			{
				get { return Mapper.BaseCollectionType; }
			}

			protected internal override bool CanProcess(Type typeToProcess)
			{
				if (base.CanProcess(typeToProcess))
				{
					System.Reflection.MethodInfo typedIndexerMethodInfo = typeToProcess.GetMethod("get_Item", new Type[] { typeof(int) });
					if (typedIndexerMethodInfo != null)
					{
						return new GenericWrapperProcessor(Mapper).CanProcess(typedIndexerMethodInfo.ReturnType);
					}
				}
				return false;
			}
		}
		#endregion
	}
}
