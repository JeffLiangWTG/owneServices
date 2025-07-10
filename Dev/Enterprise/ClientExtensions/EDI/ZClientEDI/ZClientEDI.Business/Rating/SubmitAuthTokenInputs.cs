using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace ZClientEDI.Business.Rating
{
	public class SubmitAuthTokenInputs : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZInt DatabaseNumber { get; set; }

		public ZString EnterpriseCode { get; set; }

		public ZString ServerCode { get; set; }

		public ZString UserCode { get; set; }

		public ZString UserFullName { get; set; }

		ZString userEmail;
		public ZString UserEmail
		{
			get => userEmail;
			set
			{
				userEmail = value;
				UserEmailInfo.RefreshBinding();
			}
		}

		public ZString CompanyCode { get; set; }

		public ZString CompanyName { get; set; }

		public ZBoolDescriptionPairList Roles { get; set; }

		public ZPropertyInfo UserEmailInfo => GetZPropertyInfo(nameof(UserEmail));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ClearAllNotifications();
			EmailAddressValidation.ValidateEmailAddress(UserEmailInfo);
		}
	}
}
