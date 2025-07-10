using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	[CodeProperty(GlbExternalPassword.Schema.GP_MailBoxID), DescriptionProperty(GlbExternalPassword.Schema.GP_MailBoxID)]
	public class EMCSGlbCompanyCredential : GlbExternalPasswordWithCertificate, IxTMessageAttributeProvider
	{
		public EMCSGlbCompanyCredential(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Password

		[BusinessObjectTestExclude]
		[ResourceStringData("434D7562-33BD-4CA0-93FB-3BBAA1CA7E7F", Caption = "Certificate Password")]
		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set
			{
				base.CurrentDecryptedCertificatePassphrase = ExternalPasswordHelper.GetHashedPassword(value);
			}
		}

		#endregion

		#region Password Status

		[ResourceStringData("E98DBD59-0FF4-40D1-AF45-9C43BE518FE5", Caption = "Certificate Status")]
		public ZString PasswordStatus => Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

		[ReadOnly(true)]
		public override ZString GP_PasswordStatus { get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

		#endregion

		#region GP_MailBoxID

		[ReadOnlyMember(nameof(IsMailBoxIDInUse))]
		[MaxLength(mailBoxIDMaxLength)]
		[ResourceStringData("40903453-BC20-4073-81F8-C4F87C348B3B", Caption = "Certificate Identifier")]
		public override ZString GP_MailBoxID { get => base.GP_MailBoxID; set => base.GP_MailBoxID = value; }

		bool IsMailBoxIDInUse
		{
			get
			{
				if (!GP_MailBoxID.IsEmpty && IsInDatabase)
				{
					var query = new ZQuery();
					query.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, EMCSJobDeclaration.EMCSApplicationCode);
					query.AddToFilter(JobDeclarationSchema.JE_CustomsProfile, GP_MailBoxID);
					query.AddToFilter(JobDeclarationSchema.JE_CustomsProfile, SQLComparisonOperator.NotEqual, string.Empty);
					query.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddMonths(-3));
					return Factory.Load<EMCSJobDeclaration>(query).Length != 0;
				}
				else
				{
					return false;
				}
			}
		}

		const int mailBoxIDMaxLength = 35;

		#endregion

		#region GP_ExpireDate

		[ResourceStringData("922215B2-0DCA-4DC3-99E6-F2514561ECE1", Caption = "Expiry Date")]
		public override ZDateTime GP_ExpiryDate { get => base.GP_ExpiryDate; set => base.GP_ExpiryDate = value; }

		#endregion

		public override void OnSaving()
		{
			if (IsInDatabase && (!GP_MailBoxIDInfo.OriginalValue.Equals(GP_MailBoxID) || !GP_CertificateInfo.OriginalValue.Equals(GP_Certificate)))
			{
				TransactionIDManager.SetTransactionNumbersAsUsed(Company, PK, CusTransactionNumberTypeList.Codes.IECustomsEMCS);
			}
			base.OnSaving();
		}

		protected override void ClearDataDefaultedFromCertificate()
		{
			GP_ExpiryDate = ZDateTime.Empty;
		}

		protected override void DefaultDataFromCertificate(X509Certificate2 certificate)
		{
			GP_ExpiryDate = certificate.NotAfter;
		}

		public override bool CanDelete => !IsMailBoxIDInUse;

		protected override GlbExternalPasswordValidation GetNewValidation() => new EMCSGlbCompanyCredentialValidation(this);

		public new EMCSGlbCompanyCredentialValidation Validation => (EMCSGlbCompanyCredentialValidation)GetNewValidation();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.IEM;
		}

		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			var t = (IxTMessageAttributeProvider)this;
			return t.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificatePEM();			
		}

		protected override bool IsCertificateValid => GP_ExpiryDate.IsInTheFutureUtc() && base.IsCertificateValid;
	}
}
