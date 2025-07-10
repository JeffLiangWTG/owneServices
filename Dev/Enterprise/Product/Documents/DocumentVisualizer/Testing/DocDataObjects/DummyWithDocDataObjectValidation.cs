using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyWithDocDataObjectValidation : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Name

		[DocDataObjects.Mandatory(NotificationTypes.MessageError)]
		public ZString Name
		{
			get => name;
			set => SetNonPersistentPropertyValue(NameInfo, ref name, value);
		}

		ZString name;

		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));

		#endregion

		#region Surname

		[DocDataObjects.MaxLength(NotificationTypes.Warning, 10)]
		public ZString Surname
		{
			get => surname;
			set
			{
				if (SetNonPersistentPropertyValue(SurnameInfo, ref surname, value)
					&& !IsValidationSuspended)
				{
					ValidateSurname();
				}
			}
		}

		ZString surname;

		public ZPropertyInfo SurnameInfo => GetZPropertyInfo(nameof(Surname));

		void ValidateSurname()
		{
			SurnameInfo.ClearAllNotifications();
			if (Surname == "Cat")
			{
				SurnameInfo.Add(new NotificationType(nameof(Core.NotificationType.DeliveryError), 500, false, nameof(Core.NotificationType.DeliveryError)), "Surname should not be Cat.");
			}
		}

		#endregion

		#region Salary

		public ZDecimal Salary
		{
			get => salary;
			set
			{
				if (SetNonPersistentPropertyValue(SalaryInfo, ref salary, value)
					&& !IsValidationSuspended)
				{
					ValidateSalary();
				}
			}
		}

		ZDecimal salary;

		public ZPropertyInfo SalaryInfo => GetZPropertyInfo(nameof(Salary));

		void ValidateSalary()
		{
			SalaryInfo.ClearAllNotifications();

			if (Salary < 0m)
			{
				SalaryInfo.AddError("Salary cannot be negative.");
			}
			else if (Salary > 50000m)
			{
				SalaryInfo.AddMessageError("Salary cannot be more than 50000.");
			}
			else if (Salary > 10000m)
			{
				SalaryInfo.AddWarning("Salary is greater than 10000.");
			}
		}

		#endregion

		#region IdentityCardNumber

		public ZString IdentityCardNumber
		{
			get => identityCardNumber;
			set
			{
				if (SetNonPersistentPropertyValue(IdentityCardNumberInfo, ref identityCardNumber, value) && !IsValidationSuspended)
				{
					ValidateIdentityCardNumber();
				}
			}
		}
		ZString identityCardNumber;

		public ZPropertyInfo IdentityCardNumberInfo => GetZPropertyInfo(nameof(IdentityCardNumber));

		void ValidateIdentityCardNumber()
		{
			IdentityCardNumberInfo.ClearAllNotifications();

			IdentityCardNumberInfo.AddMessageError("Messag error for IdentityCardNumber.");
			IdentityCardNumberInfo.AddWarning("Warning for IdentityCardNumber.");
			IdentityCardNumberInfo.AddError("Error for IdentityCardNumber.");
		}

		#endregion
	}
}
