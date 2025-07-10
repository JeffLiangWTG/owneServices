using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class ReasonForCancellation : NonPersistentBusinessObject
	{
		public ReasonForCancellation(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static class Schema
		{
			public const string Code = "Code";
			public const string Reason = "Reason";
		}

		public ReasonForCancellationValidation Validation => new ReasonForCancellationValidation(this);

		public ReasonForCancellationLookups Lookups => new ReasonForCancellationLookups(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		[List(nameof(Lookups) + "." + nameof(ReasonForCancellationLookups.ReasonForCancellationList))]
		public ZString Code
		{
			get => fCode;
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref fCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCode();
				}
			}
		}
		ZString fCode;
		public ZPropertyInfo CodeInfo => GetZPropertyInfo(Schema.Code);

		public ZString Reason
		{
			get => fReason;
			set
			{
				SetNonPersistentPropertyValue(ReasonInfo, ref fReason, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReason();
				}
			}
		}
		ZString fReason;
		public ZPropertyInfo ReasonInfo => GetZPropertyInfo(Schema.Reason);
	}
}
