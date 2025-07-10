using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public static class ValidationExtensions
	{
		#region Warnings

		public static bool AddWarning(this ZPropertyInfo info, Func<bool> validationRule, string errorMessage)
		{
			if (validationRule != null
				&& info?.BizObj is IAdHocValidationSupporter supporter)
			{
				return supporter.AddValidationRule(info.Name, Core.NotificationType.Warning, validationRule, errorMessage);
			}

			return false;
		}

		public static void AddWarningIfEmpty(this ZPropertyInfo info, string errorMessage = null)
		{
			if (string.IsNullOrWhiteSpace(errorMessage))
			{
				errorMessage = ValueIsRequired;
			}

			info.AddWarning(() => info.Value.IsEmpty, errorMessage);
		}

		#endregion

		#region Message Errors

		public static bool AddMessageError(this ZPropertyInfo info, Func<bool> validationRule, string errorMessage)
		{
			if (validationRule != null
				&& info?.BizObj is IAdHocValidationSupporter supporter)
			{
				return supporter.AddValidationRule(info.Name, Core.NotificationType.MessageError, validationRule, errorMessage);
			}

			return false;
		}

		public static void AddMessageErrorIfEmpty(this ZPropertyInfo info, string errorMessage = null)
		{
			if (string.IsNullOrWhiteSpace(errorMessage))
			{
				errorMessage = ValueIsRequired;
			}

			info.AddMessageError(() => info.Value.IsEmpty, errorMessage);
		}

		#endregion

		#region Delivery Errors

		public static bool AddDeliveryError(this ZPropertyInfo info, Func<bool> validationRule, string errorMessage)
		{
			if (validationRule != null
				&& info?.BizObj is IAdHocValidationSupporter supporter)
			{
				return supporter.AddValidationRule(info.Name, Core.NotificationType.DeliveryError, validationRule, errorMessage);
			}

			return false;
		}

		#endregion

		#region Errors

		public static bool AddError(this ZPropertyInfo info, Func<bool> validationRule, string errorMessage)
		{
			if (validationRule != null
				&& info?.BizObj is IAdHocValidationSupporter supporter)
			{
				return supporter.AddValidationRule(info.Name, Core.NotificationType.Error, validationRule, errorMessage);
			}

			return false;
		}

		public static void AddErrorIfEmpty(this ZPropertyInfo info, string errorMessage = null)
		{
			if (string.IsNullOrWhiteSpace(errorMessage))
			{
				errorMessage = ValueIsRequired;
			}

			info.AddError(() => info.Value.IsEmpty, errorMessage);
		}

		#endregion

		#region Implementation

		static string ValueIsRequired => Res.GetString("6ef066a9-1743-4219-9eda-73fcbe9b168d", "Value is required.");

		#endregion
	}
}
