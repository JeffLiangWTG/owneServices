using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public class CustomPropertyContainer : CustomPropertyContainer<BusinessObject>
	{
		public CustomPropertyContainer(IComparer<ICustomProperty> customPropertyComparer = null)
			: base(customPropertyComparer)
		{
		}
	}

	public class CustomPropertyContainer<T> : IActiveCustomPropertyContainer
		where T : BusinessObject
	{
		public CustomPropertyContainer(IComparer<ICustomProperty> customPropertyComparer = null)
		{
			customProperties = new SortedSet<ICustomProperty>(customPropertyComparer ?? DefaultComparer);
		}

		public IEnumerable<ICustomProperty> CustomProperties
		{
			get { return customProperties; }
		}

		#region AddCustomProperty

		public void AddCustomProperty(string identifier, object constantValue)
		{
			AddCustomProperty(new CustomPropertyImplementation<T>(identifier, identifier, constantValue.GetType(), parent => constantValue));
		}

		public void AddCustomProperty(string identifier, string caption, object constantValue)
		{
			AddCustomProperty(new CustomPropertyImplementation<T>(identifier, caption, constantValue.GetType(), parent => constantValue));
		}

		public void AddCustomProperty(string identifier, Type type, ValueGetter<T> valueGetter = null, ValueSetter<T> valueSetter = null)
		{
			AddCustomProperty(new CustomPropertyImplementation<T>(identifier, identifier, type, valueGetter, valueSetter));
		}

		public void AddCustomProperty(string identifier, string caption, Type type, bool visible, ValueGetter<T> valueGetter = null, ValueSetter<T> valueSetter = null)
		{
			AddCustomProperty(new CustomPropertyImplementation<T>(identifier, caption, type, valueGetter, valueSetter, visible));
		}

		public void AddCustomProperty(string identifier, string caption, Type type, ValueGetter<T> valueGetter = null, ValueSetter<T> valueSetter = null)
		{
			AddCustomProperty(new CustomPropertyImplementation<T>(identifier, caption, type, valueGetter, valueSetter));
		}

		public void AddCustomProperty(string identifier, string caption, Type type, ValueGetter<T> valueGetter = null, ValueSetter<T> valueSetter = null, params ICustomAddOnRule[] rules)
		{
			rules = Array.FindAll(rules, rule => rule.CanBeApplied(type));

			List<DynamicMetaData> metadata = new List<DynamicMetaData>();
			metadata.Add(DynamicMetaData.Description(new Description(caption)));
			foreach (ICustomAddOnRule rule in rules)
			{
				metadata.AddRange(rule.GetMetaData());
			}

			AddCustomProperty(identifier, caption, type, valueGetter, valueSetter, visible: true, metaData: metadata.ToArray());
		}

		public void AddCustomProperty(string identifier, string caption, Type type, ValueGetter<T> valueGetter = null, ValueSetter<T> valueSetter = null, bool visible = true, params DynamicMetaData[] metaData)
		{
			AddCustomProperty(new CustomPropertyImplementation<T>(identifier, caption, type, valueGetter, valueSetter, visible, metaData));
		}

		public void AddCustomProperty(string identifier, DynamicBusinessObjectProperty info, ValueGetter<T> valueGetter = null, ValueSetter<T> valueSetter = null)
		{
			AddCustomProperty(new CustomPropertyImplementation<T>(identifier, info, valueGetter, valueSetter));
		}

		public void AddCustomProperty(ICustomProperty property)
		{
			if (!customProperties.Contains(property))
			{
				customProperties.Add(property);
				OnPropertysSetChanged();
			}
		}

		class Description : IDescription
		{
			public Description(string description)
			{
				this.description = description;
			}

			public int Count { get { return 1; } }

			public string GetDescription(int index, CultureInfo culture)
			{
				return description;
			}

			public string GetDescription(int index)
			{
				return description;
			}

			readonly string description;
		}

		public ICustomProperty this[string identifier]
		{
			get { return this.FindPropertyByIdentifier(identifier); }
		}

		#endregion

		public void RemoveCustomProperty(string identifier)
		{
			ICustomProperty property = this.FindPropertyByIdentifier(identifier);
			if (property != null)
			{
				customProperties.Remove(property);
				OnPropertysSetChanged();
			}
		}

		public void ClearCustomProperties()
		{
			if (customProperties.Count > 0)
			{
				customProperties.Clear();
				OnPropertysSetChanged();
			}
		}

		#region PropertysSetChanged

		void OnPropertysSetChanged()
		{
			if (propertysSetChangedSuspendCounter == 0 && PropertySetChanged != null)
			{
				PropertySetChanged(this, EventArgs.Empty);
			}
		}

		public IDisposable SuspendpropertysSetChanged()
		{
			propertysSetChangedSuspendCounter++;
			return new DisposableAction(
				() =>
				{
					propertysSetChangedSuspendCounter--;
					OnPropertysSetChanged();
				});
		}

		readonly SortedSet<ICustomProperty> customProperties;
		int propertysSetChangedSuspendCounter;
		public event EventHandler PropertySetChanged;

		#endregion

		IComparer<ICustomProperty> DefaultComparer => new CustomPropertyComparer();
	}
}
