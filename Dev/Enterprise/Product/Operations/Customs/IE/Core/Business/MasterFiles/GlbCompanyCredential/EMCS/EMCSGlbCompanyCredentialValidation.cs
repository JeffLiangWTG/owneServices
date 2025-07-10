using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public class EMCSGlbCompanyCredentialValidation : GlbExternalPasswordWithCertificateValidation
	{
		public EMCSGlbCompanyCredentialValidation(EMCSGlbCompanyCredential parent) : base(parent)
		{
		}

		protected new EMCSGlbCompanyCredential Parent => (EMCSGlbCompanyCredential)base.Parent;

		protected override bool IsCertificateMandatory => false;

		public override void ValidateDuplicateConstraint()
		{
			if (!Parent.IsInDatabase ||
				Parent.GP_GSInfo.HasChanges ||
				Parent.GP_GGInfo.HasChanges ||
				Parent.GP_GCInfo.HasChanges ||
				Parent.GP_GBInfo.HasChanges ||
				Parent.GP_PasswordTypeInfo.HasChanges ||
				Parent.GP_UserIDInfo.HasChanges)
			{
				var filter = new ZQuery(GlbExternalPasswordSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GS, Parent.GP_GS);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GG, Parent.GP_GG);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GC, Parent.GP_GC);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GB, Parent.GP_GB);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, Parent.GP_PasswordType);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_UserID, Parent.GP_UserID);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_MailBoxID, Parent.GP_MailBoxID);
				if (Parent.Factory.LoadTop1<GlbExternalPassword>(filter) != null)
				{
					Parent.AddRowError(DuplicateCredentialFoundMessage);
				}
			}
		}

		protected override void CheckGP_MailBoxID()
		{
			base.CheckGP_MailBoxID();

			MandatoryValidation.CheckEntered(Parent.GP_MailBoxIDInfo);
			CheckDuplicateMailBoxID(Parent.GP_MailBoxIDInfo);
		}

		void CheckDuplicateMailBoxID(ZPropertyInfo propertyInfo)
		{
			var credentialCollection = GlbCompanyWrapper.Get(Parent.Company)?.EMCSGlbExternalPasswordCollection?.Cast<EMCSGlbCompanyCredential>();

			if (credentialCollection?.Any(x => x.GP_MailBoxID == Parent.GP_MailBoxID && x.PK != Parent.PK) ?? false)
			{
				propertyInfo.AddError(DuplicateMailBoxIDError);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error string")]
		const string DuplicateMailBoxIDError = "Certificate Identifier must be unique.";
	}
}
