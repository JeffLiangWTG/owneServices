using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	public interface ISupportMaxCountValidation : IBindingList
	{
		BusinessObjectCollectionMaxCountValidator MaxCountValidator { get; }
	}

	public static class ISupportMaxCountValidationExtensions
	{
		public static void EnableMaxCountValidation(this ISupportMaxCountValidation collection, int maxCount, string messageOverride, bool warnAtHalfway)
		{
			EnableMaxCountValidation(collection, maxCount, warnAtHalfway, NotificationType.Error, messageOverride);
		}

		public static void EnableMaxCountValidationWithMessageError(this ISupportMaxCountValidation collection, int maxCount, bool warnAtHalfway, string messageOverride, Func<int> getMaxCountReduction = null)
		{
			EnableMaxCountValidation(collection, maxCount, warnAtHalfway, NotificationType.MessageError, messageOverride, getMaxCountReduction);
		}

		public static void EnableMaxCountValidation(this ISupportMaxCountValidation collection, int maxCount, bool warnAtHalfway, INotificationType notificationType, string messageOverride, Func<int> getMaxCountReduction = null)
		{
			Argument.NotNull(collection, "collection");
			var validator = Argument.NotNull(collection.MaxCountValidator, "collection.MaxCountValidator");

			validator.MaxCount = maxCount;
			validator.WarnAtHalfway = warnAtHalfway;

			if (!string.IsNullOrEmpty(messageOverride))
			{
				validator.Notification = new Notification(notificationType, messageOverride);
			}

			if (getMaxCountReduction != null)
			{
				validator.GetMaxCountReduction = getMaxCountReduction;
			}
		}
	}
}
