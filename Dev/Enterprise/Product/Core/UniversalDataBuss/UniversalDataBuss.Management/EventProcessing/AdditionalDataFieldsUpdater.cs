using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management.EventProcessing
{
	class AdditionalDataFieldsUpdater
	{
		internal AdditionalDataFieldsUpdater(UniversalEvent eventDataObject, XmlSessionTracker logger)
		{
			Argument.NotNull(eventDataObject, "eventDataObject");
			this.logger = Argument.NotNull(logger, "logger");
			this.fieldUpdates = new List<FieldUpdater>();
			SetupFieldList(eventDataObject.AdditionalFieldsToUpdateCollection);
		}

		readonly XmlSessionTracker logger;
		readonly List<FieldUpdater> fieldUpdates;

		void SetupFieldList(List<AdditionalFieldToUpdate> additionalFieldsToUpdate)
		{
			if (additionalFieldsToUpdate != null)
			{
				foreach (var additionalFieldToUpdate in additionalFieldsToUpdate)
				{
					if (additionalFieldToUpdate != null && additionalFieldToUpdate.Type.HasValue && additionalFieldToUpdate.Value.HasValue)
					{
						var fieldIdElements = additionalFieldToUpdate.Type.Value.ToString().Split('.');
						if (fieldIdElements.Length >= 2)
						{
							fieldUpdates.Add(new FieldUpdater(fieldIdElements, additionalFieldToUpdate.Value.Value, logger));
						}
					}
				}
			}
		}

		internal void UpdateMatchingDataFields(IBusiness businessObject)
		{
			foreach (var fieldUpdater in fieldUpdates)
			{
				fieldUpdater.Update(businessObject);
			}
		}

		class FieldUpdater
		{
			internal FieldUpdater(string[] fieldIdElements, string fieldValue, XmlSessionTracker logger)
			{
				this.fieldIdElements = fieldIdElements;
				this.fieldValue = fieldValue;
				this.logger = logger;
			}

			readonly string[] fieldIdElements;
			readonly string fieldValue;
			readonly XmlSessionTracker logger;

			internal void Update(IBusiness topLevelBusinessObject)
			{
				var dataTypeName = fieldIdElements[0];
				if (!dataTypeName.Equals(topLevelBusinessObject.GetType().Name, StringComparison.InvariantCultureIgnoreCase)
					&& !dataTypeName.Equals(topLevelBusinessObject.TableName, StringComparison.InvariantCultureIgnoreCase))
				{
					return;
				}

				var businessObject = topLevelBusinessObject;
				for (var index = 1; index < fieldIdElements.Length; index++)
				{
					var propertyInfo = GetPropertyInfoSafe(businessObject.GetType(), fieldIdElements[index]);
					if (propertyInfo == null)
					{
						logger.Log(LogType.Warning, Res.GetString("91ac437a-8402-4b56-8e2f-027196ab3fff", "Field [{0}] could not be found to update with value [{1}] on {2}.", string.Join(".", fieldIdElements, 0, index + 1), fieldValue, topLevelBusinessObject.HumanReadableName));
						return;
					}

					if (index < fieldIdElements.Length - 1)
					{
						if (!typeof(BusinessObject).IsAssignableFrom(propertyInfo.PropertyType))
						{
							logger.Log(LogType.Warning, Res.GetString("e09a1f87-d4f8-4f6b-90f6-83dbc0db2715", "Entity in property [{0}] does not have any child fields to update on {2}.", string.Join(".", fieldIdElements, 0, index + 1), fieldValue, topLevelBusinessObject.HumanReadableName));
							return;
						}
						else
						{
							businessObject = propertyInfo.GetValue(businessObject, null) as BusinessObject;
							if (businessObject == null)
							{
								logger.Log(LogType.Warning, Res.GetString("84f95376-e73c-4815-badd-33bc282c70e0", "No entity present to update in property [{0}] on {2}.", string.Join(".", fieldIdElements, 0, index + 1), fieldValue, topLevelBusinessObject.HumanReadableName));
								return;
							}
						}
					}
					else
					{
						try
						{
							UpdateProperty(topLevelBusinessObject, businessObject, propertyInfo);
						}
						catch (InvalidPropertyTypeException)
						{
							logger.Log(LogType.Warning,
								Res.GetString("4EC95B12-9C44-4BE1-A8AB-4FE6CFDBC289", "Invalid property type [{0}]. The property [{1}] can't be updated.", propertyInfo.PropertyType.FullName, propertyInfo.Name));
						}
					}
				}
			}

			void UpdateProperty(IBusiness topLevelBusinessObject, IBusiness businessObject, PropertyInfo propertyInfo)
			{
				object convertedValue = propertyInfo.TryConvertValueToPropertyType(fieldValue, logger, Res.GetString("30297602-6d7b-4d49-803b-40642e776ae1", "Field [{0}] could not be updated to value [{1}] on {2}", string.Join(".", fieldIdElements), fieldValue, topLevelBusinessObject.HumanReadableName));
				if (convertedValue == null)
				{
					return;
				}
				if (businessObject is IPropertyChecker propertyChecker
					&& !propertyChecker.IsPropertyUpdatableViaXueAdditionalFields(propertyInfo, convertedValue, out var errorMessage))
				{
					logger.Log(LogType.Warning, errorMessage);
					return;
				}

				var length = 0;
				var maxLength = 0;
				if (convertedValue is ZString zStringValue)
				{
					var actualBusinessObject = businessObject as BusinessObject;
					length = zStringValue.Length;
					maxLength = actualBusinessObject?.FindPropertyInfo(propertyInfo.Name).MaxLength ?? int.MaxValue;
				}
				else
				{
					zStringValue = ZString.Empty;
				}

				if (maxLength == -1 || maxLength >= length)
				{
					propertyInfo.SetValue(businessObject, convertedValue, null);
					logger.Log(LogType.Information, Res.GetString("00199938-ca91-4644-b6cd-6494f70e8817", "Field [{0}] has been updated to value [{1}] on {2}.", string.Join(".", fieldIdElements), fieldValue, topLevelBusinessObject.HumanReadableName));
				}
				else
				{
					logger.LogBoth(LogType.Warning, BusinessObject.GetMaximumLengthErrorDescription(propertyInfo.Name, maxLength, convertedValue.ToString(), propertyInfo.GetValue(businessObject).ToString()));

					var truncatedValue = zStringValue.Substring(0, maxLength);
					propertyInfo.SetValue(businessObject, truncatedValue, null);
					logger.Log(LogType.Information, Res.GetString("00199938-ca91-4444-b6cd-6494f70e8817", "Value [{0}] has been truncated to the limit of {1} characters for Field <{2}>.", zStringValue, maxLength, string.Join(".", fieldIdElements)));
				}
			}

			static PropertyInfo GetPropertyInfoSafe(Type type, string currentIdElement)
			{
				PropertyInfo result = null;
				foreach (var candidate in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if (candidate.Name.Equals(currentIdElement, StringComparison.InvariantCultureIgnoreCase) && candidate.GetIndexParameters().Length == 0)
					{
						if (result != null)
						{
							if (result.Name.Equals(currentIdElement, StringComparison.InvariantCulture) && !candidate.Name.Equals(currentIdElement, StringComparison.InvariantCulture))
							{
								continue;
							}

							if (candidate.DeclaringType.IsAssignableFrom(result.DeclaringType))
							{
								continue;
							}
						}

						result = candidate;
					}
				}

				return result;
			}
		}
	}
}
