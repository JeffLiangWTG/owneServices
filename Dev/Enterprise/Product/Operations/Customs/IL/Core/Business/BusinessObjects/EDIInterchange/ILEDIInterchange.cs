using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.eServices.Encryption.Client.Encryptor;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class ILEDIInterchange
		: Messaging.Business.EDIInterchange,
		Integration.Customs.IL.IEDIInterchange,
		IxTMessageAttributeProvider
	{
		public ILEDIInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ILEDIInterchange.ApplicationCodes.ILCustoms;
		}

		Dictionary<string, string> IxTMessageAttributeProvider.GetMessageAttrDictionary()
		{
			var credential = Factory.Load<GlbILStaffExternalPassword>(EI_GP);
			return BuildCertificateDictionary(credential);
		}

		static Dictionary<string, string> BuildCertificateDictionary(GlbILStaffExternalPassword glbILStaffExternalPassword)
		{
			if (glbILStaffExternalPassword == null
				|| glbILStaffExternalPassword.GP_UserID.IsEmpty
				|| glbILStaffExternalPassword.GP_CertificateAuthority.IsEmpty
				|| glbILStaffExternalPassword.GP_CertificateAuthority.IsEmpty)
			{
				return BuildNoneSignatureAttribute();
			}

			var encryptedPassword =
				glbILStaffExternalPassword.CurrentDecryptedPassword.IsEmpty
				? string.Empty
				: EhubClientEncryptor.Encrypt(glbILStaffExternalPassword.CurrentDecryptedPassword);

			var attributes = new Dictionary<string, string>();
			attributes[MessageSignatureProperty.SignatureType] = glbILStaffExternalPassword.GP_CertificateAuthority;
			attributes[MessageSignatureProperty.SignatureUser] = glbILStaffExternalPassword.GP_UserID;
			attributes[MessageSignatureProperty.SignaturePIN] = encryptedPassword;

			return attributes;
		}

		static Dictionary<string, string> BuildNoneSignatureAttribute()
		{
			var attributes = new Dictionary<string, string>();
			attributes[MessageSignatureProperty.SignatureType] = string.Empty;
			return attributes;
		}
	}
}
