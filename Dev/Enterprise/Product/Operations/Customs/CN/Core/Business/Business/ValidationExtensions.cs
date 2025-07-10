using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.CN.Business
{
	public static class ValidationExtensions
	{
		#region Add Notifications

		public static void AddNotification(this ZPropertyInfo propertyInfo, string message, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => propertyInfo.AddError(message),
				() => propertyInfo.AddMessageError(message),
				() => propertyInfo.AddWarning(message));
		}

		public static void AddNotificationIfInvalidCode(this ZPropertyInfo propertyInfo, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => ListValidation.ErrorIfInvalidCode(propertyInfo),
				() => ListValidation.MessageErrorIfInvalidCode(propertyInfo),
				() => ListValidation.WarnIfInvalidCode(propertyInfo));
		}

		public static void AddNotificationIfInvalidCode(this ZPropertyInfo propertyInfo, ICodeDescriptionPairList list, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => ListValidation.ErrorIfInvalidCode(propertyInfo, list),
				() => ListValidation.MessageErrorIfInvalidCode(propertyInfo, list),
				() => ListValidation.WarnIfInvalidCode(propertyInfo, list));
		}

		public static void AddNotificationIfInvalidCodeOrEmpty(this ZPropertyInfo propertyInfo, ICodeDescriptionPairList list, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() =>
				{
					MandatoryValidation.CheckEntered(propertyInfo);
					ListValidation.ErrorIfInvalidCode(propertyInfo, list);
				},
				() => ListValidation.MessageErrorIfInvalidCodeOrEmpty(propertyInfo, list),
				() =>
				{
					MandatoryValidation.WarnIfNotEntered(propertyInfo);
					ListValidation.WarnIfInvalidCode(propertyInfo, list);
				});
		}

		public static void AddNotificationIfInvalidCodeOrEmpty(this ZPropertyInfo propertyInfo, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() =>
				{
					MandatoryValidation.CheckEntered(propertyInfo);
					ListValidation.ErrorIfInvalidCode(propertyInfo);
				},
				() => ListValidation.MessageErrorIfInvalidCodeOrEmpty(propertyInfo),
				() =>
				{
					MandatoryValidation.WarnIfNotEntered(propertyInfo);
					ListValidation.WarnIfInvalidCode(propertyInfo);
				});
		}

		public static void AddNotificationIfIsNegative(this ZPropertyInfo propertyInfo, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => MandatoryValidation.CheckNotNegative(propertyInfo),
				() => MandatoryValidation.MessageErrorIfIsNegative(propertyInfo),
				() => MandatoryValidation.WarnIfIsNegative(propertyInfo));
		}

		public static void AddNotificationIfIsNegative(this ZPropertyInfo propertyInfo, ZString propertyDescription, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => MandatoryValidation.CheckNotNegative(propertyInfo, propertyDescription),
				() => MandatoryValidation.MessageErrorIfIsNegative(propertyInfo, propertyDescription),
				() => MandatoryValidation.WarnIfIsNegative(propertyInfo, propertyDescription));
		}

		public static void AddNotificationIfIsZero(this ZPropertyInfo propertyInfo, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => MandatoryValidation.CheckNotZero(propertyInfo),
				() => MandatoryValidation.MessageErrorIfIsZero(propertyInfo),
				() => MandatoryValidation.WarnIfIsZero(propertyInfo));
		}

		public static void AddNotificationIfIsZero(this ZPropertyInfo propertyInfo, ZString propertyDescription, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => MandatoryValidation.CheckNotZero(propertyInfo, propertyDescription),
				() => MandatoryValidation.MessageErrorIfIsZero(propertyInfo, propertyDescription),
				() => MandatoryValidation.WarnIfIsZero(propertyInfo, propertyDescription));
		}

		public static void AddNotificationIfLessThanOrEqualToZero(this ZPropertyInfo propertyInfo, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() =>
				{
					MandatoryValidation.CheckNotZero(propertyInfo);
					MandatoryValidation.CheckNotNegative(propertyInfo);
				},
				() => CompareValidation.MessageErrorIfLessThanOrEqualToZero(propertyInfo),
				() =>
				{
					MandatoryValidation.WarnIfIsZero(propertyInfo);
					MandatoryValidation.WarnIfIsNegative(propertyInfo);
				});
		}

		public static void AddNotificationIfIsEntered(this ZPropertyInfo propertyInfo, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => MandatoryValidation.CheckNotEntered(propertyInfo),
				() => MandatoryValidation.MessageErrorIfIsEntered(propertyInfo),
				() => MandatoryValidation.WarnIfIsEntered(propertyInfo));
		}

		public static void AddNotificationIfNotEntered(this ZPropertyInfo propertyInfo, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => MandatoryValidation.CheckEntered(propertyInfo),
				() => MandatoryValidation.MessageErrorIfNotEntered(propertyInfo),
				() => MandatoryValidation.WarnIfNotEntered(propertyInfo));
		}

		public static void AddNotificationIfNotEntered(this ZPropertyInfo propertyInfo, ZString propertyDescription, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => MandatoryValidation.CheckEntered(propertyInfo, propertyDescription),
				() => MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, propertyDescription),
				() => MandatoryValidation.WarnIfNotEntered(propertyInfo, propertyDescription));
		}

		public static void AddRowNotification(this BusinessObject businessObject, string message, IValidationModeProvider provider)
		{
			AddNotification(provider,
				() => businessObject.AddRowError(message),
				() => businessObject.AddRowMessageError(message),
				() => businessObject.AddRowWarning(message));
		}

		static void AddNotification(IValidationModeProvider provider, Action addError, Action addMessageError, Action addWaring)
		{
			var notificationType = provider.GetNotificationType();
			if (notificationType == NotificationType.Error)
			{
				addError?.Invoke();
			}
			else if (notificationType == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				addMessageError?.Invoke();
			}
			else if (notificationType == NotificationType.Warning)
			{
				addWaring?.Invoke();
			}
		}

		#endregion

		#region ValidationModeProvider

		public static INotificationType GetNotificationType(this IValidationModeProvider provider)
		{
			switch (provider?.ValidationMode)
			{
				case ValidationModes.Preliminary:
					return NotificationType.Warning;
				case ValidationModes.ErrorForTesting:
					return NotificationType.Error;
				default:
					return CargoWise.EntityFramework.NotificationType.MessageError;
			}
		}
	}

	public interface IValidationModeProvider
	{
		ValidationModes ValidationMode { get; }
	}

	public enum ValidationModes
	{
		Full = 0,
		Preliminary = 1,
		ErrorForTesting = 2
	}

	#endregion
}
