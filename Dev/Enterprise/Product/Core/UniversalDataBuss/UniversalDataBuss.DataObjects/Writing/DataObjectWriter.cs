using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public abstract class DataObjectWriter<T, U>
		where T : BusinessObject
		where U : IDataObject, new()
	{
		protected DataObjectWriter(IDataWritingManager writeManager)
		{
			this.writeManager = Argument.NotNull(writeManager, "IDataWritingManager writeManager");
		}

		protected IDataWritingManager writeManager;
		public U GetDataObject(T sourceBO)
		{
			var dataObject = PopulateDataObject(sourceBO);

			writeManager.NotifyExported(dataObject, sourceBO);

			PopulateWorkflowCustomFields(sourceBO, ref dataObject);

			PopulateAttachedDocuments(sourceBO, dataObject);

			InsertParents(sourceBO, ref dataObject);

			return dataObject;
		}

		protected bool HasRecipientRole(RecipientRoleType type)
		{
			var result = false;
			var action = writeManager.Action;
			if (action != null && action.RecipientRoleDetails != null && action.RecipientRoleDetails.Length > 0)
			{
				result = action.RecipientRoleDetails.Any(x => x.Type == type);
			}
			return result;
		}

		protected virtual void InsertParents(T sourceBO, ref U dataObject) { }

		protected abstract U PopulateDataObject(T sourceBO);

		protected virtual void PopulateWorkflowCustomFields(T sourceBO, ref U dataObject)
		{
			var customizedFieldContainer = dataObject as ICustomizedFieldContainer;
			if (customizedFieldContainer != null)
			{
				PopulateWorkflowCustomFields(sourceBO, customizedFieldContainer);
			}
		}

		protected void PopulateWorkflowCustomFields(T sourceBO, ICustomizedFieldContainer customizedFieldContainer)
		{
			var userDefinedValues = GetAllUserDefinedValues(sourceBO, customizedFieldContainer.CustomizedFieldCollection);
			if (userDefinedValues != null)
			{
				foreach (var customValue in userDefinedValues)
				{
					if (customizedFieldContainer.CustomizedFieldCollection == null)
					{
						customizedFieldContainer.SetWriterStrategy(writeManager.WriterStrategy);
						if (!customizedFieldContainer.SetCustomizedFieldCollection(() => new List<CustomizedField>()))
						{
							break;
						}
					}

					customizedFieldContainer.CustomizedFieldCollection.Add(CustomizedField.New(customValue.PropertyName, customValue.Value));
				}
			}
		}

		protected virtual IEnumerable<IPropertyValue> GetUserDefinedValues(T sourceBO)
		{
			return null;
		}

		protected V PopulateValue<V>(V existingData, bool keepExistingData, Func<V> newData)
		{
			return keepExistingData && existingData != null ? existingData : newData();
		}

		protected List<W> ProcessCollection<V, W>(IEnumerable sourceCollection, DataObjectWriter<V, W> writer, bool deDuplicate = false)
			where V : BusinessObject
			where W : IDataObject, new()
		{
			var result = new List<W>();
			ProcessCollectionCore(sourceCollection, writer, deDuplicate, result);
			return result.Count == 0 ? null : result;
		}

		protected DataObjectList<W> ProcessCollection<V, W>(IEnumerable sourceCollection, DataObjectWriter<V, W> writer, CollectionContent contentType, bool deDuplicate = false)
			where V : BusinessObject
			where W : IDataObject, new()
		{
			var result = new DataObjectList<W>();
			result.Content = contentType;
			ProcessCollectionCore(sourceCollection, writer, deDuplicate, result);
			return result.Count == 0 ? null : result;
		}

		void PopulateAttachedDocuments(T businessObject, U dataObject)
		{
			if (dataObject is ITopLevelDataObject topLevelDataObject)
			{
				ObjectFactory.Get<IUniversalXmlContentFilterApplicator>().ExportAttachedDocuments(businessObject, topLevelDataObject, writeManager.Action);
			}
		}

		void ProcessCollectionCore<V, W>(IEnumerable sourceCollection, DataObjectWriter<V, W> writer, bool deDuplicate, ICollection<W> result)
			where V : BusinessObject
			where W : IDataObject, new()
		{
			foreach (V sourceBO in sourceCollection)
			{
				if (!deDuplicate || !writeManager.PKAlreadyExported(sourceBO.PK))
				{
					var dataObject = writer.GetDataObject(sourceBO);
					if (dataObject != null)
					{
						result.Add(dataObject);
						if (deDuplicate)
						{
							writeManager.AddPK(sourceBO.PK);
						}
					}
				}
			}
		}

		protected void MergeInAdditionalShipments(UniversalShipment parentShipment, BusinessObject[] additionalShipmentBOs)
		{
			var dataContext = parentShipment.DataContext as Universal._2011_11.DataContext ?? throw new InvalidOperationException("MergeInAdditionalShipments functionality should only ever be used with the 2011/11 namespace version.");

			foreach (var additionalShipmentBO in additionalShipmentBOs)
			{
				if (additionalShipmentBO != null && !writeManager.PKAlreadyExported(additionalShipmentBO.PK))
				{
					writeManager.AddPK(additionalShipmentBO.PK);

					var dataContextManager = additionalShipmentBO.GetUniversalDataContextManager() as IShipmentDataContextManager ?? throw new InvalidOperationException("additionalShipmentBOs must have the UniversalDataContextAttribute applied against them so that a UniversalDataContextManager can be obtained. Type passed in was: " + additionalShipmentBO.GetType().FullName);

					var shipmentWriter = dataContextManager.GetShipmentDataObjectWriter(writeManager);
					if (shipmentWriter != null)
					{
						var shipment = shipmentWriter.GetDataObject(additionalShipmentBO) as UniversalShipment ?? throw new InvalidOperationException("GetDataObject(additionalShipmentBO) should always return a UniversalShipment.");

						MergeInAdditionalShipment(parentShipment, shipment);
					}
				}
			}
		}

		void MergeInAdditionalShipment(UniversalShipment parentShipment, UniversalShipment shipment)
		{
			if (parentShipment.SubShipmentCollection == null)
			{
				parentShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(new[] { shipment }));
			}
			else
			{
				var shipments = parentShipment.SubShipmentCollection;
				var matchingShipment = shipments.FirstOrDefault(match => match.DataContext.GetDataSourceKey() == shipment.DataContext.GetDataSourceKey());
				if (matchingShipment == null)
				{
					shipments.Add(shipment);
				}
				else
				{
					var mergingShipmentDataContext = shipment.DataContext as Universal._2011_11.DataContext ?? throw new InvalidOperationException("MergeInAdditionalShipment functionality should only ever be used with the 2011/11 namespace version.");

					var matchingShipmentDataContext = matchingShipment.DataContext as Universal._2011_11.DataContext ?? throw new InvalidOperationException("MergeInAdditionalShipment functionality should only ever be used with the 2011/11 namespace version.");

					foreach (var mergingShipmentDataSource in mergingShipmentDataContext.DataSourceCollection)
					{
						AddReferenceIfNotAlreadyThere(matchingShipmentDataContext.DataSourceCollection, mergingShipmentDataSource.Type, mergingShipmentDataSource.Key);
					}

					if (shipment.SubShipmentCollection != null)
					{
						foreach (var childShipment in shipment.SubShipmentCollection)
						{
							MergeInAdditionalShipment(matchingShipment, childShipment);
						}
					}
				}
			}
		}

		IEnumerable<IPropertyValue> GetAllUserDefinedValues(T sourceBO, IEnumerable<CustomizedField> existingCustomFields)
		{
			var userDefinedValues = GetUserDefinedValues(sourceBO) ?? Enumerable.Empty<IPropertyValue>();
			var result = userDefinedValues.ToList();

			var existingCustomFieldKeys = existingCustomFields != null ? existingCustomFields.Select(o => o.Key.GetValueOrDefault()) : Enumerable.Empty<ZString>();
			var existingCustomFieldHash = new HashSet<ZString>(existingCustomFieldKeys);

			foreach (var property in GetAllCustomPropertiesWithDefaultValue(sourceBO))
			{
				if (!userDefinedValues.Any(p => p.PropertyName == property.PropertyName) && !existingCustomFieldHash.Contains(property.PropertyName))
				{
					result.Add(property);
					//if a custom field in workflow template has no value and its name is same as a custom field's caption name in registry,
					//it will not be added to the list
				}
			}

			return result;
		}

		protected virtual IEnumerable<IPropertyValue> GetAllCustomPropertiesWithDefaultValue(T sourceBO) => GetAllCustomPropertiesWithDefaultValue(sourceBO as ICustomFieldProvider);

		protected IEnumerable<IPropertyValue> GetAllCustomPropertiesWithDefaultValue(ICustomFieldProvider customFieldProvider)
		{
			if (customFieldProvider == null)
			{
				yield break;
			}

			IDynamicBusinessObject customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			var ext = ObjectFactory.Get<IDynamicBusinessObjectExtensionsInstance>();

			foreach (string propertyName in customBusinessObject.PropertyNames)
			{
				var property = customBusinessObject.GetProperty(propertyName);
				if (property != null)
				{
					var (description, partdescription) = ext.DescriptionsFor(property);
					if (description != null)
					{
						//Use partdescription as it adds PART1, PART2, etc. to combobox custom fields, and is the same otherwise.
						yield return new PropertyValue(partdescription, GetDefaultValue(property.Type));
					}
				}
			}
		}

		IZType GetDefaultValue(Type type)
		{
			return (IZType)Activator.CreateInstance(type);
		}

		static void AddReferenceIfNotAlreadyThere(List<Universal._2011_11.DataSource> references, ZString? type, ZString? key)
		{
			if (references.Find(r => r.Type == type && r.Key == key) == null)
			{
				references.Add(new Universal._2011_11.DataSource() { Type = type, Key = key });
			}
		}

		protected void ReplaceOverFlowExceptionWithDataObjectValidationException(string failureMessage, Action actions)
		{
			try
			{
				actions();
			}
			catch (OverflowException)
			{
				throw new DataObjectValidationException(failureMessage);
			}
		}
	}
}
