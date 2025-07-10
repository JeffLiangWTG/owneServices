using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class ReasonForShortageSendingAction : EMCSMessageSendingAction, IObsoleteValidation
	{
		public ReasonForShortageSendingAction(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		public static class Schema
		{
			public const string GeneralExplanation = "GeneralExplanation";
			public const int GeneralExplanationMaxLength = 350;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("70DC6F4D-7E48-4812-AA03-4D321B2BC718", "Reason for Shortage");

		[ResourceStringData("2ABFAF63-A121-4BAF-9A00-88D425F3F725", Caption = "General Explanation")]
		[MaxLength(Schema.GeneralExplanationMaxLength)]
		public ZString GeneralExplanation
		{
			get => generalExplanation;
			set
			{
				if (generalExplanation != value)
				{
					SetNonPersistentPropertyValue(GeneralExplanationInfo, ref generalExplanation, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateGeneralExplanation();
					}
				}
			}
		}
		ZString generalExplanation;
		public ZPropertyInfo GeneralExplanationInfo => GetZPropertyInfo(Schema.GeneralExplanation);

		#region Validation

		public ReasonForShortageSendingActionValidation Validation => new ReasonForShortageSendingActionValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion
	}
}
