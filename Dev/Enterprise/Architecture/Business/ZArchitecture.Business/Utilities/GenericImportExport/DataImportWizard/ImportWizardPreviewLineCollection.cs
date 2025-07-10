using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ImportWizardPreviewLineCollection : NonPersistentBusinessObjectCollection<ImportWizardPreviewLine>, IDynamicBusinessObjectCollection
	{
		public ImportWizardPreviewLineCollection(IEnumerable<IImportPropertyInfo> propertyInfos, Dictionary<string, IList> lists)
		{
			this.properties = GetProperties(propertyInfos, lists);
			template = (ImportWizardPreviewLine)CreateNonPersistentBusinessObject();
		}

		IEnumerable<Tuple<Type, string, DynamicMetaData[]>> GetProperties(IEnumerable<IImportPropertyInfo> propertyInfos, Dictionary<string, IList> lists)
		{
			List<Tuple<Type, string, DynamicMetaData[]>> result = new List<Tuple<Type, string, DynamicMetaData[]>>();
			foreach (IImportPropertyInfo propertyInfo in propertyInfos)
			{
				DynamicMetaData[] metaData;
				IList list;
				if (lists.TryGetValue(propertyInfo.MappingName, out list))
				{
					metaData = new DynamicMetaData[] { DynamicMetaData.ListDataSource(list) };
				}
				else if (propertyInfo.PropertyType.Equals(typeof(ZString)))
				{
					metaData = new DynamicMetaData[] { DynamicMetaData.MaxLength(Int32.MaxValue) };
				}
				else
				{
					metaData = Array.Empty<DynamicMetaData>();
				}

				result.Add(new Tuple<Type, string, DynamicMetaData[]>(propertyInfo.PropertyType, propertyInfo.MappingName, metaData));

				if (propertyInfo.IsMultiControl && !string.IsNullOrEmpty(propertyInfo.FieldTypeColumnName))
				{
					result.Add(new Tuple<Type, string, DynamicMetaData[]>(typeof(string), propertyInfo.FieldTypeColumnName, Array.Empty<DynamicMetaData>()));
				}
			}
			return result.ToArray();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			ImportWizardPreviewLine result = new ImportWizardPreviewLine(properties);
			return result;
		}

		readonly IEnumerable<Tuple<Type, string, DynamicMetaData[]>> properties;

		#region IDynamicBusinessObjectCollection Members

		IDynamicBusinessObject IDynamicBusinessObjectCollection.Template
		{
			get { return template; }
		}
		readonly ImportWizardPreviewLine template;

		#endregion
	}
}
