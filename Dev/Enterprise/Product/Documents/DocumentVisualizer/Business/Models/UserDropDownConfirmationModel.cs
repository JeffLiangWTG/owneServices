using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.Models
{
	public sealed class UserDropDownConfirmationModel : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UserDropDownConfirmationModel(ICodeDescriptionPairList optionsList)
		{
			Argument.NotNull(optionsList, nameof(optionsList));
			OptionsList = optionsList;
		}

		[List("OptionsList")]
		public ZString Option
		{
			get => option;
			set
			{
				if (SetNonPersistentPropertyValue(OptionInfo, ref option, value)
					&& !IsValidationSuspended)
				{
					ValidateOption();
				}
			}
		}

		ZString option;

		public ZPropertyInfo OptionInfo => GetZPropertyInfo(nameof(Option));

		public ICodeDescriptionPairList OptionsList { get; }

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			this.ValidateOption();
		}

		public void ValidateOption()
		{
			OptionInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(OptionInfo);

			if (!OptionInfo.HasNotifications())
			{
				ListValidation.ErrorIfInvalidCode(OptionInfo);
			}
		}

		#endregion
	}
}