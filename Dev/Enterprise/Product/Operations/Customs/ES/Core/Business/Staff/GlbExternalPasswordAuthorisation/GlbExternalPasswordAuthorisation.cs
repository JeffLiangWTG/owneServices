using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ES.Business
{
	public class GlbExternalPasswordAuthorisation : Enterprise.MasterFiles.Business.GlbExternalPasswordAuthorisation
	{
		public GlbExternalPasswordAuthorisation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new class Schema : Enterprise.MasterFiles.Business.AutoGlbExternalPasswordAuthorisation.Schema
		{
			public const string StaffName = "StaffName";
		}

		#endregion

		#region Properties

		public new GlbExternalPassword ExternalPassword => (GlbExternalPassword)base.ExternalPassword;

		public ZString StaffName => cachedStaffName?.Value ?? (cachedStaffName = new CachedProperty<ZString>(Factory, () => AuthorisedStaff?.GS_FullName ?? ZString.Empty)).Value;
		CachedProperty<ZString> cachedStaffName;

		public bool GEA_GS_AuthorisedStaff_ReadOnly => IsInDatabase && GEA_GS_AuthorisedStaff.IsValid;

		public override ZGuid GEA_GP
		{
			get => base.GEA_GP;
			set
			{
				var externalPassword = ExternalPassword;
				var original = GEA_GP;
				base.GEA_GP = value;
				if (original.IsValid && GEA_GP.IsEmpty && externalPassword != null)
				{
					var message = FormattableString.Invariant($"Authorisation for user {AuthorisedStaffCode} to use certificate {externalPassword.GP_Name} has been withdrawn.");
					externalPassword.Logs.AddNew(Events.AuthorisationWithdrawn, message);
				}
			}
		}

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();

			var externalPassword = ExternalPassword;
			if (!IsInDatabase)
			{
				var message = FormattableString.Invariant($"User {AuthorisedStaffCode} has been authorised to use certificate {externalPassword.GP_Name}.");
				externalPassword.Logs.AddNew(Events.Authorised, message);
			}
		}

		public new GlbExternalPasswordAuthorisationValidation Validation => (GlbExternalPasswordAuthorisationValidation)base.Validation;
		protected override Enterprise.MasterFiles.Business.GlbExternalPasswordAuthorisationValidation GetNewValidation() => new GlbExternalPasswordAuthorisationValidation(this);

		public ZString AuthorisedStaffCode => AuthorisedStaff?.GS_Code ?? GEA_GS_AuthorisedStaff.ToString();
	}
}
