using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class AlertOrRejectReason : NonPersistentBusinessObject<AlertOrRejectReasonValidation>, IAlertOrRejectReason
	{
		public AlertOrRejectReason(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[ResourceStringData("FA8C956B-9590-4DA0-BBA0-87F2A24FFE59", Caption = "Reason")]
		[List(nameof(Lookups) + "." + nameof(AlertOrRejectReasonLookups.ReasonList))]
		[MaxLength(3)]
		public ZString Reason
		{
			get { return reason; }
			set
			{
				SetNonPersistentPropertyValue(ReasonInfo, ref reason, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReason();
				}
			}
		}
		ZString reason;

		public ZPropertyInfo ReasonInfo => GetZPropertyInfo(nameof(Reason));

		[ResourceStringData("8EE4E50D-1B96-4335-B13A-20C92E60B273", Caption = "Information")]
		[MaxLength(350)]
		public ZString Information
		{
			get { return information; }
			set
			{
				SetNonPersistentPropertyValue(InformationInfo, ref information, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateInformation();
				}
			}
		}
		ZString information;

		public ZPropertyInfo InformationInfo => GetZPropertyInfo(nameof(Information));

		public AlertOrRejectReasonLookups Lookups => lookups ?? (lookups = GetNewLookups());
		AlertOrRejectReasonLookups lookups;

		public AlertOrRejectReasonLookups GetNewLookups() => new AlertOrRejectReasonLookups(this);

		public override AlertOrRejectReasonValidation GetNewValidation() => new AlertOrRejectReasonValidation(this);

		protected override ZString HumanReadableNameCore => Res.GetString("DFAE9AA2-43B0-4523-B272-567C9ABDD756", "Alert or Reject Reason");
	}
}
