using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Shared;
using Res = Enterprise.ZArchitecture.Web.GUI.Res;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class DateFormatterWebServiceMethod : WebServiceMethod<DateFormatterParameters>
	{
		#region Overrides

		protected override void ExecuteCore(DateFormatterParameters parameters, WebServiceResponse response)
		{
			ZDateTimePickerFormat dateTimeFormat = GetDateFormatType(parameters.DateFormatType);
			if (!string.IsNullOrEmpty(parameters.DateValue))
			{
				ZDateTime dateTimeValue = WebDateTimeFormatter.GetParsedDate(parameters.DateValue.ToUpper(), dateTimeFormat);
				if (!dateTimeValue.IsValid || dateTimeValue.IsEmpty)
				{
					response.Add(new SetFocusResponseToken(parameters.DateControlID));
					response.Add(new ShowErrorResponseToken(Res.GetString("2fab6e68-590e-4153-839b-2362c794a098", "Please enter valid date!")));
				}
				else
				{
					response.Add(new UpdateValueResponseToken(parameters.DateControlID, WebDateTimeFormatter.GetFormattedDate(dateTimeValue, DateFormatType)));
					if (!string.IsNullOrEmpty(parameters.TimeControlID) && string.IsNullOrEmpty(parameters.TimeValue))
					{
						response.Add(new UpdateValueResponseToken(parameters.TimeControlID, "00:00"));
					}
				}
			}
		}

		protected override string GetMethodName()
		{
			return "FormatDate";
		}

		#endregion

		#region Properties

		[DefaultValue(ZDateTimePickerFormat.Short)]
		public ZDateTimePickerFormat DateFormatType { get; set; }

		#endregion

		#region Implementation

		ZDateTimePickerFormat GetDateFormatType(string dateFormatTypeString)
		{
			if (!string.IsNullOrEmpty(dateFormatTypeString))
			{
				if (Enum.IsDefined(typeof(ZDateTimePickerFormat), dateFormatTypeString))
				{
					return (ZDateTimePickerFormat)Enum.Parse(typeof(ZDateTimePickerFormat), dateFormatTypeString, true);
				}
			}
			return ZDateTimePickerFormat.Short;
		}

		#endregion

	}
}
