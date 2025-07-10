using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSAddInfoJobDeclarationValidation : EU.EMCS.Business.EMCSAddInfoJobDeclarationValidation
	{
		public EMCSAddInfoJobDeclarationValidation(EMCSAddInfoJobDeclaration parent) : base(parent)
		{
		}

		protected new EMCSJobDeclaration Declaration => (EMCSJobDeclaration)base.Declaration;

		protected override void CheckZG_TransportArrangement()
		{
			base.CheckZG_TransportArrangement();
			if (Declaration.IsConsolidatedDocument() && Parent.ZG_TransportArrangement != EMCSTransportArrangementList.Codes.Consignor)
			{
				Parent.ZG_TransportArrangementInfo.AddMessageError(Res.GetString("482972DB-1591-4B7E-84A8-016411CEA41F", "Transport Arrangement must be 1-Consignor for deferred consolidated declarations."));
			}
		}

		protected override void CheckZG_GuarantorType()
		{
			base.CheckZG_GuarantorType();
			if (Declaration.IsConsolidatedDocument() && Parent.ZG_GuarantorType != EmcsGuarantorTypeList.Codes.Consignor)
			{
				Parent.ZG_GuarantorTypeInfo.AddWarning(Res.GetString("30BAE9C2-E881-47ED-909A-01D71134618F", "If a guarantee has been lodged, code '1' must be entered."));
			}
		}

		protected override ZBool ShouldValidateGuarantorMustBe0WhenDestinationTypeIs1 => ZBool.False;

		protected override ZBool ShouldValidateDispatchReferenceIsMandatory => ZBool.False;
	}
}
