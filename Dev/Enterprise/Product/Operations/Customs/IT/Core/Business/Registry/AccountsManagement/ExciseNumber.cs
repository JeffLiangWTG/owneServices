using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Registry;

public class ExciseNumber : NonPersistentBusinessObject
{
	public ExciseNumber(Account account, BusinessObjectFactory factory) : base(factory)
	{
		Account = Argument.NotNull(account, nameof(account));
		Argument.NotNull(factory, nameof(factory));
	}

	public Account Account { get; }

	public static class Schema
	{
		public const string Number = "Number";
		public const int NumberMaxLength = 13;
	}

	#region Properties

	[MaxLength(Schema.NumberMaxLength)]
	public ZString Number
	{
		get { return number; }
		set
		{
			var oldValue = Number;
			SetNonPersistentPropertyValue(NumberInfo, ref number, value);
			if (oldValue != value && !IsValidationSuspended)
			{
				Validation.ValidateNumber();
			}
		}
	}
	ZString number;

	public ZPropertyInfo NumberInfo => GetZPropertyInfo(Schema.Number);

	#endregion

	#region Validation

	ExciseNumberValidation Validation => new ExciseNumberValidation(this);

	protected override void RunPreSaveValidationCore()
	{
		Validation.ValidateAll();
		base.RunPreSaveValidationCore();
	}

	#endregion
}
