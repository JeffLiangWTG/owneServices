using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusContainerValidation : Customs.Business.CusContainerValidation
	{
		public CusContainerValidation(CusContainer parent)
			: base(parent)
		{
		}

		public new CusContainer Parent
		{
			get { return (CusContainer)base.Parent; }
		}

		protected override void CheckCO_WeightUQ()
		{
			base.CheckCO_WeightUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CO_WeightUQInfo, Parent.Lookups.WeightUnits);
		}

		protected override string MessageForContainerShouldLinkToOneInvoiceLine
		{
			get { return Res.GetString("2fe41cee-eb66-4af7-bff8-3bd12f52e636", "A container should be linked to at least one package line. Please go to the Packaging tab -> sub tab Packing Details and select appropriate container(s)"); }
		}

		protected override void CheckCO_Seal()
		{
			var parent = Parent;
			base.CheckCO_Seal();
			CheckSealEnteredIfHasAdditionalSeals(parent.CO_SealInfo);
		}

		protected override void CheckCO_SecondSeal()
		{
			var parent = Parent;
			base.CheckCO_SecondSeal();
			CheckSealEnteredIfHasAdditionalSeals(parent.CO_SecondSealInfo);
		}

		void CheckSealEnteredIfHasAdditionalSeals(ZPropertyInfo sealInfo)
		{
			var parent = Parent;
			if (sealInfo.Value.IsEmpty && parent.AdditionalSeals.Count > 0)
			{
				sealInfo.AddError(Res.GetString("8E3CD3C7-5DAD-4A80-AA7E-6D71A813E837", "Please enter {0}", sealInfo.HumanReadableName));
			}
		}
	}
}
