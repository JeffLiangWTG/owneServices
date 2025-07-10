using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public delegate object ValueGetter<T>(T parent) where T : BusinessObject;
	public delegate bool ValueSetter<T>(T parent, object value) where T : BusinessObject;

	public class CustomPropertyImplementation<T> : ICustomProperty
		where T : BusinessObject
	{
		public CustomPropertyImplementation(string identifier, Type type, ValueGetter<T> valueGetter = null, ValueSetter<T> valueSetter = null)
			: this(identifier, identifier, type, valueGetter, valueSetter)
		{ }

		public CustomPropertyImplementation(string identifier, string caption, Type type, ValueGetter<T> valueGetter = null, ValueSetter<T> valueSetter = null, bool visible = true)
			: this(identifier, caption, type, valueGetter, valueSetter, visible, Array.Empty<DynamicMetaData>())
		{
		}

		public CustomPropertyImplementation(string identifier, string caption, Type type, ValueGetter<T> valueGetter = null, ValueSetter<T> valueSetter = null, bool visible = true, params DynamicMetaData[] metaData)
			: this(identifier, new DynamicBusinessObjectProperty(type, valueSetter == null, visible, metaData.Union(new[] { DynamicMetaData.Description(new PropertyCaption(caption)) }).ToArray()), valueGetter, valueSetter)
		{
			Argument.NotNull(type, "type");
		}

		public CustomPropertyImplementation(string identifier, DynamicBusinessObjectProperty info, ValueGetter<T> valueGetter = null, ValueSetter<T> valueSetter = null)
		{
			Argument.NotNullOrEmpty(identifier, "identifier");
			Argument.NotNull(info, "info");

			Identifier = identifier;
			Info = info;

			this.valueGetter = valueGetter;
			this.valueSetter = valueSetter;
		}

		public object GetValue(T parent)
		{
			return valueGetter != null ? valueGetter(parent) : null;
		}

		public bool TrySetValue(T parent, object value)
		{
			if (valueSetter != null)
			{
				return valueSetter(parent, value);
			}
			return false;
		}

		public void Validate(T parent) { }

		public string Identifier { get; private set; }

		public DynamicBusinessObjectProperty Info { get; private set; }

		public virtual IEnumerable<ICustomProperty> RelatedProperties { get { return Enumerable.Empty<ICustomProperty>(); } }

		public ICustomColumnDefinition CustomColumnDefinition => throw new NotImplementedException();

		public bool IsDeleted => false;

		readonly ValueGetter<T> valueGetter;
		readonly ValueSetter<T> valueSetter;

		class PropertyCaption : IDescription
		{
			public PropertyCaption(string caption)
			{
				this.caption = caption;
			}

			readonly string caption;

			int IDescription.Count
			{
				get { return 1; }
			}

			string IDescription.GetDescription(int index)
			{
				return caption;
			}

			string IDescription.GetDescription(int index, CultureInfo culture)
			{
				return caption;
			}
		}

		#region ICustomProperty Members

		object ICustomProperty.GetValue(BusinessObject parent)
		{
			return GetValue((T)parent);
		}

		bool ICustomProperty.TrySetValue(BusinessObject parent, object value)
		{
			return TrySetValue((T)parent, value);
		}

		void ICustomProperty.Validate(BusinessObject parent)
		{
			Validate((T)parent);
		}

		#endregion
	}
}
