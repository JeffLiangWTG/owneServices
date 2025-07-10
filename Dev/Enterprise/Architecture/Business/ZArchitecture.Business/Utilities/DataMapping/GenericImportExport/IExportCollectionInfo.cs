using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.DataMapping
{
	public interface IExportCollectionInfo
	{
		BusinessObjectFactory Factory { get; }
		IEnumerable<BusinessObject> BusinessObjects { get; }
		IEnumerable<RowType> RowTypes { get; }
	}

	public class RowType : IEquatable<RowType>
	{
		public RowType(MultilingualString name, Type type, IEnumerable<IImportPropertyInfo> properties)
		{
			Name = name.ToString();
			Type = type;
			Properties = properties;
			if (name is ResourceString resourceString)
			{
				Identifier = resourceString.ResourceKey;
				NameInEnglish.Add(resourceString.ToString(SharedConstants.Languages.English));
				NameInEnglish.Add(resourceString.ToString(SharedConstants.Languages.EnglishBritish));
				NameInEnglish.Add(resourceString.ToString(SharedConstants.Languages.EnglishAmerican));
			}
			else
			{
				Identifier = name.ToString();
			}
		}

		public string Name { get; private set; }
		public Type Type { get; private set; }
		public IEnumerable<IImportPropertyInfo> Properties { get; private set; }
		public string Identifier { get; }
		public HashSet<string> NameInEnglish = new HashSet<string>();

		public bool Equals(RowType other)
		{
			return other != null && Type.Equals(other.Type);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as RowType);
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode();
		}
	}

	public class ExportCollectionInfoImpl : IExportCollectionInfo, IEnumerable
	{
		public ExportCollectionInfoImpl(BusinessObjectFactory factory, IEnumerable<BusinessObject> businessObjects)
		{
			Factory = factory;
			BusinessObjects = businessObjects;
		}

		public void Add(MultilingualString name, Type type, IEnumerable<IImportPropertyInfo> properties)
		{
			rowTypes.Add(new RowType(name, type, properties));
		}

		public BusinessObjectFactory Factory { get; private set; }
		public IEnumerable<BusinessObject> BusinessObjects { get; private set; }

		public IEnumerable<RowType> RowTypes
		{
			get { return rowTypes; }
		}
		readonly List<RowType> rowTypes = new List<RowType>();

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return rowTypes.GetEnumerator();
		}

		#endregion
	}
}
