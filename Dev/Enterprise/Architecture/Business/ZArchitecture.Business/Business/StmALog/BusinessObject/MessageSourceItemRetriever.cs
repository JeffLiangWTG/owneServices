using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public static class MessageSourceItemRetriever
	{
		public static KeyDataPairCollection GetSourceItems(BusinessObjectFactory factory, IXmlEventValueObject valueObject)
		{
			Argument.NotNull(factory, "factory");

			return valueObject != null ? GetSourceItems(factory, valueObject.Context, valueObject.DataContext) :
				new KeyDataPairCollection(factory);
		}

		public static KeyDataPairCollection GetSourceItems(BusinessObjectFactory factory, MessageValueObject valueObject)
		{
			Argument.NotNull(factory, "factory");

			KeyDataPairCollection result = null;

			if (valueObject != null)
			{
				result = GetSourceItems(factory, valueObject.ContextList, valueObject.DataContext);
				AddPairIfNotEmpty(result, Res.GetString("09c6bccb-c276-434c-9d71-2c9e657bc998", "Sender ID"), valueObject.Sender);
				AddPairIfNotEmpty(result, Res.GetString("c94b5da9-7f21-4a57-85c3-f1aec722a089", "Recipient ID"), valueObject.Recipient);
			}

			return result ?? new KeyDataPairCollection(factory);
		}

		static KeyDataPairCollection GetSourceItems(BusinessObjectFactory factory, IXmlEventValueObjectContextValueList contextList, IDataContextDataObject dataObject)
		{
			var result = new KeyDataPairCollection(factory);
			if (contextList != null)
			{
				foreach (var item in contextList.Values)
				{
					var key = item.Key.Description.IsEmpty ? item.Key.Type.InsertSpacesIntoPascalCasing() : item.Key.Description;
					AddPairIfNotEmpty(result, key, item.Value.ToString());
				}
			}

			var dataContext = dataObject;
			if (dataContext != null)
			{
				var contextKeyValuePairs = dataContext.ContextKeyValuePairs;
				if (contextKeyValuePairs != null)
				{
					foreach (var pair in contextKeyValuePairs)
					{
						AddPairIfNotEmpty(result, pair.Key, pair.Value);
					}
				}
			}

			return result;
		}

#if DEBUG
		public
#endif
 static void AddPairIfNotEmpty(KeyDataPairCollection result, ZString key, ZString data)
		{
			if (!data.IsEmpty && data.Length <= 2000)
			{
				var pair = result.AddNew();
				pair.Key = key.Substring(0, pair.KeyInfo.MaxLength);
				pair.Data = data;
			}
		}
	}
}
