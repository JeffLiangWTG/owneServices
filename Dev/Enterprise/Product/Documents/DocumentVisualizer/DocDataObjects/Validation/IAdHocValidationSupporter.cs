using System;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IAdHocValidationSupporter
	{
		bool AddValidationRule(string propertyName, Core.NotificationType notificationType, Func<bool> validationRule, string errorMessage);

		void Validate(params string[] propertyNames);
		void ValidateAll();
		void ValidateAllIncludingChildren();
	}
}
