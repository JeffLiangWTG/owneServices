using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.AFR.Business
{
	using Helper = VesselDetailValidationHelper;

	public class BlanketVesselChangeValidation : AutoBlanketVesselChangeValidation
	{
		public BlanketVesselChangeValidation(AutoBlanketVesselChange parent) : base(parent)
		{
		}

		protected override void CheckJPM_CarrierCodeNew()
		{
			base.CheckJPM_CarrierCodeNew();

			Helper.CheckCarrierCode(Parent.JPM_CarrierCodeNewInfo, null);

			ValidateNoVesselDetailsChange(Parent.JPM_CarrierCodeNewInfo);
		}

		protected override void CheckJPM_VesselNameNew()
		{
			base.CheckJPM_VesselNameNew();

			Helper.CheckVesselName(Parent.JPM_VesselNameNewInfo, Parent.Vessel);

			ValidateNoVesselDetailsChange(Parent.JPM_VesselNameNewInfo);
		}

		protected override void CheckJPM_RadioCallSignNew()
		{
			base.CheckJPM_RadioCallSignNew();

			Helper.CheckCallSign(Parent.JPM_RadioCallSignNewInfo);

			ValidateNoVesselDetailsChange(Parent.JPM_RadioCallSignNewInfo);
		}

		protected override void CheckJPM_RN_NKCountryOfRegNew()
		{
			base.CheckJPM_RN_NKCountryOfRegNew();

			Helper.CheckCountryOfReg(Parent.JPM_RN_NKCountryOfRegNewInfo);

			ValidateNoVesselDetailsChange(Parent.JPM_RN_NKCountryOfRegNewInfo);
		}

		protected override void CheckJPM_OperatorVoyageNew()
		{
			base.CheckJPM_OperatorVoyageNew();

			ValidateNoVesselDetailsChange(Parent.JPM_OperatorVoyageNewInfo);
		}

		protected override void CheckJPM_VoyageNumberNew()
		{
			base.CheckJPM_VoyageNumberNew();

			Helper.CheckVoyageNumber(Parent.JPM_VoyageNumberNewInfo);

			ValidateNoVesselDetailsChange(Parent.JPM_VoyageNumberNewInfo);
		}

		protected override void CheckJPM_IsDepartureFromRelaxedAreaNew()
		{
			base.CheckJPM_IsDepartureFromRelaxedAreaNew();

			ValidateNoVesselDetailsChange(Parent.JPM_IsDepartureFromRelaxedAreaNewInfo);
		}

		protected override void CheckJPM_PortOfLoadingCodeNew()
		{
			base.CheckJPM_PortOfLoadingCodeNew();

			Helper.CheckLoadingPortCode(Parent.JPM_PortOfLoadingCodeNewInfo, Parent.Loading);

			ValidateNoVesselDetailsChange(Parent.JPM_PortOfLoadingCodeNewInfo);
		}

		protected override void CheckJPM_PortOfLoadingSuffixNew()
		{
			base.CheckJPM_PortOfLoadingSuffixNew();

			Helper.CheckLoadingPortSuffix(Parent.JPM_PortOfLoadingSuffixNewInfo);

			ValidateNoVesselDetailsChange(Parent.JPM_PortOfLoadingSuffixNewInfo);
		}

		protected override void CheckJPM_ETDNew()
		{
			base.CheckJPM_ETDNew();

			Helper.CheckETD(Parent.JPM_ETDNewInfo);

			ValidateNoVesselDetailsChange(Parent.JPM_ETDNewInfo);
		}

		void ValidateNoVesselDetailsChange(ZPropertyInfo propertyInfo)
		{
			if (!Parent.AreVesselDetailsDifferentFromHeader)
			{
				propertyInfo.AddMessageError(ValidationConstants.MessageSending.NoVesselDetailsHaveChanged);
			}
		}

		protected new BlanketVesselChange Parent => (BlanketVesselChange)base.Parent;
	}
}
