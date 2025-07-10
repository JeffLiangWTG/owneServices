using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	class DocWrapperContext
	{
		internal DocWrapperContext(Dictionary<string, object> constants)
		{
			ReWriteConstants(constants);
		}

		internal void ReWriteConstants(Dictionary<string, object> constants)
		{
			privateConstantsDictionary = constants != null
																		? new Dictionary<string, object>(constants)
																		: new Dictionary<string, object>();

			SetInternalValue(Constants.TemplateDefined.ContactType, value => DocumentContactTypeCode = new ZString(value));
			SetInternalValue(Constants.TemplateDefined.ContactOrganisationPK, value => ContactOrganisationPK = new ZGuid(value));
			SetInternalValue(Constants.TemplateDefined.ReportName, value => ReportName = new ZString(value));
			SetInternalValue(Constants.TemplateDefined.DocumentDirection, value => DocumentDirection = new ZString(value));
			SetInternalValue(Constants.TemplateDefined.MenuTitle, value => MenuTitle = new ZString(value));
			SetInternalValue(Constants.TemplateDefined.MenuItemPK, value => MenuItemPK = new ZGuid(value));
			SetInternalValue(Constants.TemplateDefined.DeliveryMode, value => DocumentDeliveryMode = new ZString(value));
			SetInternalValue(Constants.TemplateDefined.BrandedOrganisationPK, value => BrandedOrganisationPK = new ZGuid(value));
		}

		internal void MergeConstants(Dictionary<string, object> constants)
		{
			if (constants != null && privateConstantsDictionary != null)
			{
				foreach (KeyValuePair<string, object> keyValuePair in constants)
				{
					if (privateConstantsDictionary.ContainsKey(keyValuePair.Key))
					{
						privateConstantsDictionary[keyValuePair.Key] = keyValuePair.Value;
					}
					else
					{
						privateConstantsDictionary.Add(keyValuePair.Key, keyValuePair.Value);
					}
				}
				ReWriteConstants(privateConstantsDictionary);
			}
		}

		object GetTemplateConstantValue(string key)
		{
			return privateConstantsDictionary != null && privateConstantsDictionary.ContainsKey(key) ? privateConstantsDictionary[key] : null;
		}

		public T GetTemplateConstantValue<T>(string key, out bool isFound)
		{
			var result = GetTemplateConstantValue(key);
			isFound = result != null;

			try
			{
				var converter = TypeDescriptor.GetConverter(typeof(T));
				if (isFound && converter != null && converter.CanConvertFrom(result.GetType()))
				{
					return (T)converter.ConvertFrom(result);
				}

				return default(T);
			}
			catch (Exception ex)
			{
				var message = Res.GetString("bfe6cddf-de40-4d29-9e3d-28df4c76cf08", "'{0}' template constant value cannot be converted to target type '{1}'. Actual value is '{2}'", key, typeof(T).ToString(), result);
				throw new DocumentTypeConversionFailedException(message, ex);
			}
		}

		Dictionary<string, object> privateConstantsDictionary;

		void SetInternalValue(string key, Action<object> setter)
		{
			object keyValue = null;
			privateConstantsDictionary?.TryGetValue(key, out keyValue);
			setter(keyValue);
		}
		public ZString DocumentContactTypeCode { get; private set; }
		public ZGuid ContactOrganisationPK { get; private set; }
		public ZGuid BrandedOrganisationPK { get; private set; }
		public ZString ReportName { get; private set; }
		public ZString DocumentDirection { get; private set; }
		public ZString MenuTitle { get; private set; }
		public ZGuid MenuItemPK { get; private set; }
		public ZString DocumentDeliveryMode { get; private set; }
	}
}
