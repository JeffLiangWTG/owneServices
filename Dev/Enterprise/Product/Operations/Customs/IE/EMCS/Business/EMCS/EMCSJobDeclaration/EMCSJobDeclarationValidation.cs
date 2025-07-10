using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSJobDeclarationValidation : EU.EMCS.Business.EMCSJobDeclarationValidation
	{
		public EMCSJobDeclarationValidation(EMCSJobDeclaration parent) : base(parent) { }

		EMCSJobDeclaration Declaration
		{
			get { return Parent as EMCSJobDeclaration; }
		}

		protected override void CheckJE_CustomsProfile()
		{
			base.CheckJE_CustomsProfile();
			MandatoryValidation.CheckEntered(Parent.JE_CustomsProfileInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JE_CustomsProfileInfo);
			if (Declaration.CertificateIdentifier is EMCSGlbCompanyCredential certificateIdentifier
				&& certificateIdentifier.GP_PasswordStatus != PasswordStatusList.Codes.Valid)
			{
				Parent.JE_CustomsProfileInfo.AddWarning(Res.GetString("19077417-555F-413E-9ECF-FE3BF34A6AC1", "Certificate status does not appear to be valid."));
			}
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();

			if (Parent.JE_MessageSubType != EMCSDestinationTypeList.Codes.DestinationExport
				&& Parent.CustomsOffices.ContainsCode(EuOfficeCodesTypes.Codes.OfficeOfDelivery))
			{
				Parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("FC7ED968-0E57-401B-9ACF-5A46D340F4F1"
					, "Customs Office ({0}) should only be entered when Destination Type = 6 - {1}.", EuOfficeCodesTypes.Descriptions.OfficeOfDelivery, EMCSDestinationTypeList.Descriptions.DestinationExport));
			}
		}
	}
}
