using CargoWise.EntityFramework;

namespace Enterprise.Customs.MX.Business
{
	public class JobDeclarationValidation : Customs.Business.BaseJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_LocationOfGoodsInfo);
		}

		protected override void CheckJE_SubLocationOfGoods()
		{
			base.CheckJE_SubLocationOfGoods();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_SubLocationOfGoodsInfo);
		}

		protected override void CheckJE_GoodsOrigin()
		{
			base.CheckJE_GoodsOrigin();
			if (Parent.IsExport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_GoodsOriginInfo);
			}
		}

		protected override void CheckJE_GoodsDestination()
		{
			base.CheckJE_GoodsDestination();
			if (Parent.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_GoodsDestinationInfo);
			}
		}

		protected override void CheckJE_CustomsProfile()
		{
			base.CheckJE_CustomsProfile();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CustomsProfileInfo);
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_MessageSubTypeInfo);
		}
	}
}
