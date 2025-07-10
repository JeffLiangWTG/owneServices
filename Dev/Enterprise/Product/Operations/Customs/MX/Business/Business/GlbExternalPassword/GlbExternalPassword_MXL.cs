using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Business
{
	public class GlbExternalPassword_MXL : GlbExternalPassword
	{
		public GlbExternalPassword_MXL(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implement

		protected override ZString HumanReadableNameCore => Res.GetString("749F0833-01CC-4EE2-97B8-FD0BC44A5369", "Staff License");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.MXL;
		}

		public new GlbExternalPasswordValidation_MXL Validation => (GlbExternalPasswordValidation_MXL)GetNewValidation();

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordValidation_MXL(this);
		}

		protected override GlbExternalPasswordLookups GetNewLookups() => new GlbExternalPasswordLookups_MXL(this);

		public new GlbExternalPasswordLookups_MXL Lookups => (GlbExternalPasswordLookups_MXL)base.Lookups;

		#endregion

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(GlbExternalPasswordLookups_MXL.CustomsFacilities))]
		[ResourceStringData("Enterprise.Customs.MX.Business.GlbExternalPassword_MXL|GP_CertificateAuthority", Caption = "Customs Area")]
		public override ZString GP_CertificateAuthority
		{
			get => base.GP_CertificateAuthority;
			set
			{
				base.GP_CertificateAuthority = value;
				if (GP_UserID_ReadOnly)
				{
					GP_UserID = ZString.Empty;
				}
			}
		}

		[ReadOnlyMember(nameof(GP_UserID_ReadOnly))]
		[MaxLength(4)]
		[ResourceStringData("Enterprise.Customs.MX.Business.GlbExternalPassword_MXL|GP_UserID", Caption = "Number")]
		public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

		bool GP_UserID_ReadOnly => GP_CertificateAuthority.IsEmpty;
	}
}
