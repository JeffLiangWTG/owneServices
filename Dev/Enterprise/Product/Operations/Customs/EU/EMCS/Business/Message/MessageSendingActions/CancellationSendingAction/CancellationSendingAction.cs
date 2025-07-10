using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class CancellationSendingAction : EMCSMessageSendingAction, IEMCSCancellation, IObsoleteValidation
	{
		public CancellationSendingAction(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		public static class Schema
		{
			public const string Reason = "Reason";
			public const string Information = "Information";

			public const int ReasonMaxLength = 3;
			public const int InformationMaxLength = 350;
		}

		protected override ZString HumanReadableNameCore => ResString.GetMultilingualString("477932bb-213e-41d4-9859-e009b9ad39d4", "Cancellation of EAD");

		[List(nameof(Lookups) + "." + nameof(CancellationSendingActionLookups.ReasonList))]
		[MaxLength(Schema.ReasonMaxLength)]
		[ResourceStringData("32184441-0CEA-4A68-AF06-5A9813823BA9", Caption = "Reason")]
		public ZString Reason
		{
			get => reason;
			set
			{
				if (reason != value)
				{
					SetNonPersistentPropertyValue(ReasonInfo, ref reason, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateReason();
					}
				}
			}
		}
		public ZPropertyInfo ReasonInfo => GetZPropertyInfo(Schema.Reason);
		ZString reason;

		[MaxLength(Schema.InformationMaxLength)]
		[ResourceStringData("CE6AF9F2-59B0-4F08-B3EB-3752429B91EA", Caption = "Information")]
		public ZString Information
		{
			get => information;
			set
			{
				if (information != value)
				{
					SetNonPersistentPropertyValue(InformationInfo, ref information, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateInformation();
					}
				}
			}
		}
		public ZPropertyInfo InformationInfo => GetZPropertyInfo(Schema.Information);
		ZString information;

		#region Lookups

		public CancellationSendingActionLookups Lookups => lookups ?? (lookups = GetNewLookups());
		CancellationSendingActionLookups lookups;
		public CancellationSendingActionLookups GetNewLookups() => new CancellationSendingActionLookups(this);

		#endregion

		#region Validation

		public CancellationSendingActionValidation Validation => new CancellationSendingActionValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion
	}
}
