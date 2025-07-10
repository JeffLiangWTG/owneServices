using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class CusPermitHeaderValidation : Customs.Business.BaseCusPermitHeaderValidation
	{
		public CusPermitHeaderValidation(CusPermitHeader parent)
			: base(parent)
		{
		}

		public new CusPermitHeader Parent => (CusPermitHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCPH_FullType();
		}

		public void ValidateCPH_FullType()
		{
			ValidateCalculatedProperty(Parent.CPH_FullTypeInfo);
		}

		protected virtual void CheckCPH_FullType()
		{
			var fullTypeInfo = Parent.CPH_FullTypeInfo;
			MandatoryValidation.CheckEntered(fullTypeInfo);
			ListValidation.ErrorIfInvalidCode(fullTypeInfo);
		}

		protected override void CheckCPH_Type()
		{
		}

		protected override void CheckCPH_SubType()
		{
		}

		protected override void CheckCPH_UnitOfMeasure()
		{
			base.CheckCPH_UnitOfMeasure();

			ListValidation.MessageErrorIfInvalidCode(Parent.CPH_UnitOfMeasureInfo);
		}
	}
}
