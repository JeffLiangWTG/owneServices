
namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusEquipmentValidation : EU.Business.Declaration.CusEquipmentValidation
	{
		public CusEquipmentValidation(CusEquipment parent) : base(parent)
		{
		}

		public new CusEquipment Parent => (CusEquipment)base.Parent;

		protected override void CheckCEQ_IdentificationNumber()
		{
			base.CheckCEQ_IdentificationNumber();

			var parent = Parent;
			var identificationNumber = parent.CEQ_IdentificationNumber;
			if (!identificationNumber.IsEmpty && parent.Seals.Count == 0)
			{
				parent.CEQ_IdentificationNumberInfo.AddMessageError(SealsAreMandatoryMessageError);
			}
		}

		string SealsAreMandatoryMessageError => Res.GetString("9A1DE7E6-0793-490E-87B6-54BB288D0CF9", "If equipment is used, seals are mandatory. If equipment has not seals, it must not be included in the declaration.");
	}
}
