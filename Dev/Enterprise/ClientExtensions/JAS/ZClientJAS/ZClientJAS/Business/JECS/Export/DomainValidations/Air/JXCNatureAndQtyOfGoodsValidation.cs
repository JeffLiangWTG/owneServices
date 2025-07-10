using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCNatureAndQtyOfGoodsValidation : NatureAndQtyOfGoodsValidation
	{
		public JXCNatureAndQtyOfGoodsValidation(JASNatureAndQtyOfGoods parent)
			: base(parent)
		 {
		 }

		protected override void CheckText()
		{
			base.CheckText();

			if (!Parent.HasErrors)
			{
				ValidationHelper.AddJXCWarningIfNotEntered(Parent.TextInfo);
			}
		}

		protected ValidationHelper ValidationHelper
		{
			get { return validationHelper ?? (validationHelper = new ValidationHelper()); }
		}

		ValidationHelper validationHelper;
	}
}
