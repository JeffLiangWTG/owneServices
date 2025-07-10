using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngineCore.DocumentParsing
{
	public class DocumentFieldAttributeFinder : IDocumentFieldAttributeFinder
	{
		public DocumentFieldAttributeFinder()
		{
		}

		#region Find Properties

		/// <summary>
		/// Searches a given class for the DocumentFieldAttribute
		/// </summary>
		/// <param name="ClassNameToSearchForEmailAttributes">Name of class to search</param>
		/// <returns>Collection of all DocumentFieldAttributes found on properties and methods in that class</returns>
		public DocumentFieldDefinitionCollection FindProperties(Type classTypeToSearch)
		{
			DocumentFieldDefinitionCollection emailTagProperties = DocumentFieldAttributeHash[classTypeToSearch.Name];

			if (emailTagProperties == null)
			{
				emailTagProperties = new DocumentFieldDefinitionCollection();

				PropertyInfo[] propertyInfos = classTypeToSearch.GetProperties(BindingFlags.Instance | BindingFlags.Public);
				foreach (PropertyInfo info in propertyInfos)
				{
					DocumentFieldAttribute[] emailAttribute = (DocumentFieldAttribute[])info.GetCustomAttributes(typeof(DocumentFieldAttribute), false);
					if (emailAttribute.Length != 0)
					{
						DocumentFieldDefinition.FieldTypes fieldType;
						if (typeof(DocumentWrapper).IsAssignableFrom(info.PropertyType))
						{
							fieldType = DocumentFieldDefinition.FieldTypes.RelatedDocumentWrapper;
							emailTagProperties.Add(new DocumentFieldDefinition(info.Name, emailAttribute[0].UserVisibleDescription, fieldType, info.PropertyType));
						}
						else if (typeof(BusinessObject).IsAssignableFrom(info.PropertyType))
						{
							fieldType = DocumentFieldDefinition.FieldTypes.RelatedBusinessObject;
							emailTagProperties.Add(new DocumentFieldDefinition(info.Name, emailAttribute[0].UserVisibleDescription, fieldType));
						}
						else
						{
							fieldType = DocumentFieldDefinition.FieldTypes.Property;
							emailTagProperties.Add(new DocumentFieldDefinition(info.Name, emailAttribute[0].UserVisibleDescription, fieldType));
						}
					}
				}

				MethodInfo[] methodInfos = classTypeToSearch.GetMethods(BindingFlags.Instance | BindingFlags.Public);
				foreach (MethodInfo info in methodInfos)
				{
					DocumentFieldAttribute[] emailAttribute = (DocumentFieldAttribute[])info.GetCustomAttributes(typeof(DocumentFieldAttribute), false);
					if (emailAttribute.Length != 0)
					{
						var fieldDefinition = new DocumentFieldDefinition(info.Name, emailAttribute[0].UserVisibleDescription, DocumentFieldDefinition.FieldTypes.Method, info.ReturnType);
						fieldDefinition.AdditionalFieldInfo = "(" + string.Join(",", info.GetParameters().Select(p => "{" + p.Name + "}").ToArray()) + ")";
						emailTagProperties.Add(fieldDefinition);
					}
				}

				DocumentFieldAttributeHash.Add(classTypeToSearch.Name, emailTagProperties);
			}

			return emailTagProperties;
		}

		IDocumentFieldDefinitionCollection IDocumentFieldAttributeFinder.FindProperties(Type objectType)
		{
			return FindProperties(objectType);
		}

		#endregion

		#region DocumentFieldAttributeHash

		internal static DocumentFieldAttributeHashTable DocumentFieldAttributeHash
		{
			get
			{
				if (fDocumentFieldAttributeHash == null)
				{
					fDocumentFieldAttributeHash = new DocumentFieldAttributeHashTable();
				}

				return fDocumentFieldAttributeHash;
			}
		}
		[ThreadStatic]
		static DocumentFieldAttributeHashTable fDocumentFieldAttributeHash;

		public class DocumentFieldAttributeHashTable : DictionaryBase
		{
			public DocumentFieldDefinitionCollection this[string typeName]
			{
				get { return (DocumentFieldDefinitionCollection)Dictionary[typeName]; }
				set { Dictionary[typeName] = value; }
			}

			public void Add(string typeName, DocumentFieldDefinitionCollection listOfDocumentFieldAttributePairs)
			{
				Dictionary.Add(typeName, listOfDocumentFieldAttributePairs);
			}

			public void Remove(string typeName)
			{
				Dictionary.Remove(typeName);
			}

			public bool Contains(string className)
			{
				return Dictionary.Contains(className);
			}
		}

		#endregion
	}
}
