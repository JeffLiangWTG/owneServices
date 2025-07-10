using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using NotificationType = Enterprise.DocumentVisualizer.Core.NotificationType;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public sealed class DocDataObjectValidationProvider : IValidationProvider
	{
		#region IValidationProvider members

		public IEnumerable<ValidationRule> GetValidationRules(IDynamicData validatee)
		{
			if (validatee == null)
			{
				yield break;
			}

			var description = validatee.GetMetaData<string>(MetaDataType.Description);

			var requiredAttribute = validatee.GetAttribute<MandatoryAttribute>();
			if (requiredAttribute != null)
			{
				yield return CreateMandatoryValidationRule(description, requiredAttribute.NotificationType);
			}

			var maxLenghtAttribute = validatee.GetAttribute<MaxLengthAttribute>();
			if (maxLenghtAttribute != null)
			{
				yield return CreateMaxLengthValidationRule(description, maxLenghtAttribute.NotificationType, maxLenghtAttribute.MaxLength);
			}

			if (validatee.Value is IAdHocValidationProvider validationProvider)
			{
				foreach (var message in (validationProvider.Validator?.Invoke() ?? Enumerable.Empty<string>()))
				{
					if (!string.IsNullOrEmpty(message))
					{
						yield return _ =>
						{
							var source = new NotificationSource(validatee.Type.Name);
							return new Notification(source, NotificationType.MessageError, message);
						};
					}
				}
			}

			var zPropertyInfoValidationRule = CreateZPropertyInfoValidationRule(description, validatee);

			if (zPropertyInfoValidationRule != null)
			{
				yield return zPropertyInfoValidationRule;
			}
		}

		#endregion

		#region Validation methods

		static ValidationRule CreateMandatoryValidationRule(string propertyName, NotificationTypes zNotificationType)
		{
			return dynamicData =>
			{
				var message = Res.GetString("9d733b58-399f-4ad5-b5a9-073e50862b16", "Value is required.");

				var text = Convert.ToString(dynamicData.Value, CultureInfo.InvariantCulture);

				if (string.IsNullOrWhiteSpace(text))
				{
					var source = new NotificationSource(propertyName);
					var notificationType = TranslateZNotificationType(zNotificationType);

					return new Notification(source, notificationType, message);
				}

				return null;
			};
		}

		static ValidationRule CreateMaxLengthValidationRule(string propertyName, NotificationTypes zNotificationType, int maxLength)
		{
			return dynamicData =>
			{
				var message = Res.GetString("3e9ee517-4fe1-427c-a921-45fde5b19f26", "Value exceeded max length of {0}.", maxLength);

				var text = Convert.ToString(dynamicData.Value, CultureInfo.InvariantCulture);

				if (text.Length > maxLength)
				{
					var source = new NotificationSource(propertyName);
					var notificationType = TranslateZNotificationType(zNotificationType);

					return new Notification(source, notificationType, message);
				}

				return null;
			};
		}

		static ValidationRule CreateZPropertyInfoValidationRule(string propertyName, IDynamicData validatee)
		{
			if (validatee is DocDataObjectDynamicData docDataObjectDynamicData)
			{
				if (docDataObjectDynamicData.ZPropertyInfo == null)
				{
					return null;
				}

				return _ =>
				{
					foreach (var zNotification in docDataObjectDynamicData.ZPropertyInfo.Notifications.OrderBy(x => x.Type?.Severity ?? int.MaxValue))
					{
						var source = new NotificationSource(propertyName);
						var notificationType = TranslateZNotificationType(zNotification.Type);
						return new Notification(source, notificationType, zNotification.Message);
					}

					return null;
				};
			}

			return null;
		}

		#endregion

		#region Implementation

		static NotificationType TranslateZNotificationType(NotificationTypes notificationType)
		{
			switch (notificationType)
			{
				case NotificationTypes.Error:
					return NotificationType.Error;

				case NotificationTypes.MessageError:
					return NotificationType.MessageError;

				default:
					return NotificationType.Warning;
			}
		}

		static NotificationType TranslateZNotificationType(CargoWise.ComponentModel.INotificationType notificationType)
		{
			if (notificationType == CargoWise.ComponentModel.NotificationType.Error)
			{
				return NotificationType.Error;
			}
			else if (notificationType == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				return NotificationType.MessageError;
			}
			else if (notificationType.EnumValueName == nameof(Core.NotificationType.DeliveryError))
			{
				return NotificationType.DeliveryError;
			}

			return NotificationType.Warning;
		}

		#endregion
	}
}
