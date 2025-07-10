using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GBGuaranteeValidation : GuaranteeForDeclarationValidation
	{
		public GBGuaranteeValidation(GBGuarantee parent)
		   : base(parent)
		{
		}

		protected new GBGuarantee Parent => (GBGuarantee)base.Parent;

		protected override void CheckPW_Password()
		{
			base.CheckPW_Password();

			if (Parent.Declaration != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.PW_PasswordInfo);
			}
		}

		protected override void CheckPW_BondNumber()
		{
			base.CheckPW_BondNumber();
			if (Parent.Declaration != null && Parent.PW_BondType == Parent.Declaration.ApplicationExtender.GuaranteeBondType)
			{
				if (!Parent.PW_BondNumber.IsEmpty & !Parent.PW_BondNumber2.IsEmpty)
				{
					Parent.PW_BondNumberInfo.AddMessageError(guaranteeWarning);
				}
			}
		}

		protected override void CheckPW_BondNumber2()
		{
			base.CheckPW_BondNumber2();
			if (Parent.Declaration != null && Parent.PW_BondType == Parent.Declaration.ApplicationExtender.GuaranteeBondType)
			{
				if (!Parent.PW_BondNumber2.IsEmpty & !Parent.PW_BondNumber.IsEmpty)
				{
					Parent.PW_BondNumber2Info.AddMessageError(guaranteeWarning);
				}
			}
		}
		const string guaranteeWarning = "Please set only the reference or the GRN field but not both.";
		readonly string holderWarning = BrandingFactory.Instance.ProductName + " was unable to determine which of Other Guarantee Reference and GRN to set automatically. Please ensure that the guarantee type and tax method of payment fields are set such to support this automation. Alternatively, remove the value from this field and instead directly set the Other Guarantee Reference or the GRN field.";

		protected override void CheckPW_HolderIdentification()
		{
			base.CheckPW_HolderIdentification();
			if (!Parent.PW_HolderIdentification.IsEmpty && Parent.PW_BondNumber.IsEmpty && Parent.PW_BondNumber2.IsEmpty)
			{
				Parent.PW_HolderIdentificationInfo.AddMessageError(holderWarning);
			}
		}
	}
}
