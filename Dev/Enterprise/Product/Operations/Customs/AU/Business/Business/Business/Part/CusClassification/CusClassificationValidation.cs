using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusClassificationValidation : Customs.Business.CusClassificationValidation
	{
		public CusClassificationValidation(Classification parent)
			: base(parent)
		{
		}

		public new Classification Parent
		{
			get { return (Classification)base.Parent; }
		}

		#region Overriden Checks

		protected override void CheckCC_TariffNum()
		{
			base.CheckCC_TariffNum();

			if (Parent.CC_ClassificationType == Classification.ClassificationType.EXP)
			{
				if (Parent.ExportTariff?.HasChildren ?? IsIncompleteTariffCode)
				{
					Parent.CC_TariffNumInfo.AddError("The selected tariff is a partial tariff used only for navigation - Please select a complete tariff number.");
				}
				else if (Parent.ExportTariff == null)
				{
					Parent.CC_TariffNumInfo.AddMessageError("This tariff number does not exist.");
				}
			}
			else
			{
				new ImportTariffValidator().Validate(Parent.CC_TariffNumInfo, null, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			}
		}

		bool IsIncompleteTariffCode
		{
			get
			{
				var value = Parent.CC_TariffNum.Trim();
				return !value.IsEmpty && value.Length < 10;
			}
		}

		#endregion
	}
}
