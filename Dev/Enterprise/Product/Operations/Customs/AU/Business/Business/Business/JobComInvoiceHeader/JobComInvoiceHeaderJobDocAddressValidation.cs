using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobComInvoiceHeaderJobDocAddressValidation : JobDocAddressValidation
	{
		public JobComInvoiceHeaderJobDocAddressValidation(AutoJobDocAddress parent, JobComInvoiceHeader header)
			: base(parent)
		{
			this.header = header;
		}

		readonly JobComInvoiceHeader header;

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateE2_Phone_Formatted();
			ValidateE2_Mobile_Formatted();
		}

		#endregion

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (IsAQISDocAddress && !Parent.E2_AddressOverride && Parent.Contact == null)
			{
				ValidateE2_Contact();
			}
		}

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();
			if (IsNeedValidationForAQIS && !Parent.E2_AddressOverride && Parent.HasRealAddress && !Parent.BypassFireEventBeforeChange)
			{
				if (Parent.E2_Phone.IsEmpty && Parent.E2_Mobile.IsEmpty)
				{
					Parent.E2_ContactInfo.AddMessageError(PhoneOrMobileRequiredMessage);
				}
			}
		}

		protected override void CheckE2_Phone_Formatted()
		{
			base.CheckE2_Phone_Formatted();
			if (IsNeedValidationForAQIS && Parent.E2_AddressOverride)
			{
				if (Parent.E2_Phone.IsEmpty && Parent.E2_Mobile.IsEmpty)
				{
					Parent.E2_Phone_FormattedInfo.AddMessageError(PhoneOrMobileRequiredMessage);
				}
			}
		}

		protected override void CheckE2_Mobile_Formatted()
		{
			base.CheckE2_Mobile_Formatted();
			if (IsNeedValidationForAQIS && Parent.E2_AddressOverride)
			{
				if (Parent.E2_Phone.IsEmpty && Parent.E2_Mobile.IsEmpty)
				{
					Parent.E2_Mobile_FormattedInfo.AddMessageError(PhoneOrMobileRequiredMessage);
				}
			}
		}

		bool IsAQISDocAddress => header.IsAQISDocAddressType(Parent.DocAddressType);

		bool IsNeedValidationForAQIS => IsAQISDocAddress && Parent.DocAddressType != MasterFiles.Integration.DocAddressType.AQISLoadingEstablishment;

		string PhoneOrMobileRequiredMessage => ResString.GetMultilingualString("AU|JobDocAddress|08F5FA08-9B92-4D36-9D41-BAF8391BEC80", "Either Phone or Mobile is required");
	}
}
