using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class GlbExternalPassword_CHR : GlbExternalPassword
{
	public GlbExternalPassword_CHR(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.CHR;
	}

	public ZString GP_CertificateText
	{
		get => base.GP_Certificate.ToAscii();
		set => base.GP_Certificate = ZBlob.FromAscii(value);
	}

	public ZPropertyInfo GP_CertificateTextInfo => GetWrappedZPropertyInfo(nameof(GP_CertificateText), x => GP_CertificateInfo);

	protected override GlbExternalPasswordValidation GetNewValidation() => new GlbExternalPasswordValidation_CHR(this);

	public new GlbExternalPasswordValidation_CHR Validation => (GlbExternalPasswordValidation_CHR)base.Validation;
}
