using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlWriting
{
	sealed class DataOverrideManager
	{
		public DataOverrideManager(IDataOverrideProvider provider, XmlBuilder builder)
		{
			this.provider = provider;
			this.builder = Argument.NotNull(builder, "builder");
		}

		readonly IDataOverrideProvider provider;
		readonly XmlBuilder builder;
		readonly IDictionary<IDataObject, IDictionary<string, Override>> queuedOverrides = new Dictionary<IDataObject, IDictionary<string, Override>>();

		struct Override
		{
			public IDataObject DataObject { get; set; }

			public Action<IDataObject, XmlBuilder> Writer { get; set; }
		}

		public void QueueForWritingLaterOnParent(IDataObject dataObject, IDataObject parent, string propertyName, Action<IDataObject, XmlBuilder> writer)
		{
			Argument.NotNull(dataObject, "dataObject");
			Argument.NotNull(parent, "parent");
			Argument.NotNullOrEmpty(propertyName, "propertyName");
			Argument.NotNull(writer, "writer");

			IDictionary<string, Override> overrides = null;

			if (!queuedOverrides.TryGetValue(parent, out overrides))
			{
				overrides = new Dictionary<string, Override>();
				queuedOverrides[parent] = overrides;
			}

			overrides[propertyName] = new Override
			{
				DataObject = dataObject,
				Writer = writer
			};
		}

		public IDisposable OverrideWriter(IDataObject dataObject)
		{
			Argument.NotNull(dataObject, "dataObject");

			if (provider == null)
			{
				return new DisposableAction(() => { });
			}

			var state = provider.GetDataObjectState(dataObject);

			if (state == DataObjectState.Added)
			{
				builder.AddStartElementWithAttributes(ElementConstants.DocOverrideElement, ElementConstants.AdditionAttribute);
			}

			return new DisposableAction(() =>
			{
				if (state == DataObjectState.Removed)
				{
					builder.AddElementWithValueWithAttributes(ElementConstants.DocOverrideElement, string.Empty, ElementConstants.RemovedAttribute);
				}
				else
				{
					var overrideAdded = WriteOverrides(dataObject, state == DataObjectState.Added);
					overrideAdded = WriteQueuedOverrides(dataObject, overrideAdded);

					if (overrideAdded)
					{
						builder.AddEndElement(ElementConstants.DocOverrideElement);
					}
				}
			});
		}

		bool WriteOverrides(IDataObject dataObject, bool hasDocOverrideStartElement)
		{
			var values = provider.GetPropertyOverrides(dataObject);

			foreach (var value in values)
			{
				if (!hasDocOverrideStartElement)
				{
					builder.AddStartElement(ElementConstants.DocOverrideElement);
					hasDocOverrideStartElement = true;
				}

				var formattedValue = SimpleTypeFormatter.GetFormattedValueForWritingToXml(value.Value ?? ZString.Empty, () => int.MaxValue);
				builder.AddElementWithValue(value.Name, formattedValue);
			}

			return hasDocOverrideStartElement;
		}

		bool WriteQueuedOverrides(IDataObject dataObject, bool hasDocOverrideStartElement)
		{
			IDictionary<string, Override> overridesToWrite = null;

			if (!queuedOverrides.TryGetValue(dataObject, out overridesToWrite))
			{
				return hasDocOverrideStartElement;
			}

			foreach (var overrideToWrite in overridesToWrite.Values)
			{
				var propertyOverrides = provider
					.GetPropertyOverrides(overrideToWrite.DataObject)
					.ToDictionary(p => p.Name, p => p.Value);

				if (!propertyOverrides.Any())
				{
					continue;
				}

				var dataObjectType = overrideToWrite.DataObject.GetType();

				var dataObjectToWrite = (IDataObject)Activator.CreateInstance(dataObjectType);

				foreach (var propertyInfo in dataObjectType.GetProperties())
				{
					IZType value;

					if (propertyOverrides.TryGetValue(propertyInfo.Name, out value))
					{
						propertyInfo.SetValue(dataObjectToWrite, value);
					}
					else
					{
						var originalValue = propertyInfo.GetValue(overrideToWrite.DataObject);
						propertyInfo.SetValue(dataObjectToWrite, originalValue);
					}
				}

				if (!hasDocOverrideStartElement)
				{
					builder.AddStartElement(ElementConstants.DocOverrideElement);
					hasDocOverrideStartElement = true;
				}

				overrideToWrite.Writer(dataObjectToWrite, builder);
			}

			return hasDocOverrideStartElement;
		}
	}
}