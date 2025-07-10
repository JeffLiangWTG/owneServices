using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business
{
	public class GlbILStaffExternalPassword : GlbExternalPassword
	{
		public GlbILStaffExternalPassword(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new GlbILStaffExternalPasswordLookups Lookups => (GlbILStaffExternalPasswordLookups)base.Lookups;

		#region GP_CertificateAuthority

		[List(nameof(Lookups) + "." + nameof(GlbILStaffExternalPasswordLookups.CertificateAuthoritiesList))]
		[ResourceStringData("7AB7A557-5957-4ADD-B9FB-82B23FD2DA92", Caption = "Certificate Authority")]
		public override ZString GP_CertificateAuthority { get => base.GP_CertificateAuthority; set => base.GP_CertificateAuthority = value; }

		#endregion GP_CertificateAuthority

		#region GP_UserID

		[ResourceStringData("7BC41198-C2CC-4C30-8BD2-BB4A58A99A83", Caption = "Certificate ID")]
		public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

		#endregion GP_UserID

		#region CurrentDecryptedPassword

		[ResourceStringData("4214DBFB-E0F6-42A1-A98B-C50461173DC6", Caption = "Pin Code")]
		public override ZString CurrentDecryptedPassword { get => base.CurrentDecryptedPassword; set => base.CurrentDecryptedPassword = value; }

		#endregion CurrentDecryptedPassword

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.ILS;
		}

		protected override GlbExternalPasswordLookups GetNewLookups() => new GlbILStaffExternalPasswordLookups(this);
	}
}
