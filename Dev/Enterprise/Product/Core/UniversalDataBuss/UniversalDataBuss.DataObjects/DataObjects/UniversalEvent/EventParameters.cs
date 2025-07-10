namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using CargoWise.ComponentModel;
	using CargoWise.Types;
	using Enterprise.UniversalDataBuss.Integration;
	using Enterprise.ZArchitecture.Business;
	using Codes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

	[XsdSchema(Placement.Inner)]
	public class EventParameters : IDataObject
	{
		#region Properties

		[MaxLength(256)]
		[EventParameter(Codes.Location)]
		public ZString? Location { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.Facility)]
		public ZString? Facility { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.Department)]
		public ZString? Department { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.Service)]
		public ZString? Service { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.Reason)]
		public ZString? Reason { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.MessageType)]
		public ZString? MessageType { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.MessageSubType)]
		public ZString? MessageSubType { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.Old)]
		public ZString? Old { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.New)]
		public ZString? New { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.Type)]
		public ZString? Type { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.Name)]
		public ZString? Name { get; set; }

		[EventParameter(Codes.Partial)]
		public ZInt? Partial { get; set; }

		[EventParameter(Codes.Total)]
		public ZInt? Total { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.VoyageFlightNumber)]
		public ZString? VoyageFlightNumber { get; set; }

		[EventParameter(Codes.FlightDate)]
		public ZDateTime? FlightDate { get; set; }

		[EventParameter(Codes.EstimatedTimeOfArrival)]
		public ZDateTime? EstimatedTimeOfArrival { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.EquipmentReferenceNumber)]
		public ZString? EquipmentReferenceNumber { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.CustomsReferenceNumber)]
		public ZString? CustomsReferenceNumber { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.ReferenceNumber)]
		public ZString? ReferenceNumber { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.RequestNumber)]
		public ZString? RequestNumber { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.ExternalDocumentType)]
		public ZString? ExternalDocumentType { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.ReceiptNumber)]
		public ZString? ReceiptNumber { get; set; }

		[EventParameter(Codes.Quantity)]
		public ZInt? Quantity { get; set; }

		[EventParameter(Codes.InnerPackQuantity)]
		public ZInt? InnerPackQuantity { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.Mode)]
		public ZString? TransportMode { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.Status)]
		public ZString? Status { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.Score)]
		public ZString? Score { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.EventCode)]
		public ZString? EventCode { get; set; }

		[MaxLength(256)]
		[EventParameter(Codes.From)]
		public ZString? From { get; set; }

		#endregion

		public static string GetCodeByName(string name)
		{
			EnsurePropertiesCacheInitialized();

			string code;
			propertyNameToCodeMap.TryGetValue(name, out code);

			return code;
		}

		public static PropertyInfo GetPropertyByCode(string code)
		{
			EnsurePropertiesCacheInitialized();

			PropertyInfo property;
			codeToPropertyMap.TryGetValue(code, out property);

			return property;
		}

		static void EnsurePropertiesCacheInitialized()
		{
			if (codeToPropertyMap == null)
			{
				codeToPropertyMap = new Dictionary<string, PropertyInfo>();
				propertyNameToCodeMap = new Dictionary<string, string>();

				var properties = typeof(EventParameters).GetProperties();
				foreach (var property in properties)
				{
					var attribute = (EventParameterAttribute)property.GetCustomAttributes(typeof(EventParameterAttribute), false).FirstOrDefault();

					codeToPropertyMap[attribute.Code] = property;
					propertyNameToCodeMap[property.Name] = attribute.Code;
				}
			}
		}

		[ThreadStatic]
		static Dictionary<string, PropertyInfo> codeToPropertyMap;

		[ThreadStatic]
		static Dictionary<string, string> propertyNameToCodeMap;

		public static IDictionary<string, string> GetEventParameters(EventParameters eventParameters, string referenceFallback = null)
		{
			Dictionary<string, string> result;
			if (referenceFallback != null)
			{
				result = new Dictionary<string, string>(StmALog.GetParametersFromReference(referenceFallback));
			}
			else
			{
				result = new Dictionary<string, string>();
			}

			if (eventParameters != null)
			{
				EnsurePropertiesCacheInitialized();

				foreach (var property in codeToPropertyMap)
				{
					var value = property.Value.GetValue(eventParameters);

					if (value != null)
					{
						result.Add(property.Key, StmALog.SerializeParameterValue((IZType)value).Trim());
					}
				}
			}
			return result;
		}

		public static ZString? GetEventParameter(string code, EventParameters eventParameters, string referenceFallback = "")
		{
			ZString? result = null;

			var parameters = GetEventParameters(eventParameters, referenceFallback);
			if (parameters != null && parameters.TryGetValue(code, out var parameter))
			{
				result = parameter;
			}

			return result;
		}

		public object GetEventParameter(string code)
		{
			if (codeToPropertyMap.TryGetValue(code, out var propertyInfo))
			{
				return propertyInfo.GetValue(this);
			}
			return null;
		}
	}
}
