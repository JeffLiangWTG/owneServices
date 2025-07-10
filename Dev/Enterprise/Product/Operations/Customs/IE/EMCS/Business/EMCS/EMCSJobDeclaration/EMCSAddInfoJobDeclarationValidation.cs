using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSAddInfoJobDeclarationValidation : EU.EMCS.Business.EMCSAddInfoJobDeclarationValidation
	{
		public EMCSAddInfoJobDeclarationValidation(EMCSAddInfoJobDeclaration parent) : base(parent)
		{
		}

		protected override void CheckZG_CCTMSA()
		{
			base.CheckZG_CCTMSA();

			if (Declaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.DestinationExemptedConsignee && !Parent.ZG_CCTMSA.IsEmpty)
			{
				Parent.ZG_CCTMSAInfo.AddMessageError(Res.GetString("EF7151D8-ECBE-4853-86D8-7ECEC9FF81ED"
					, "Member State should only be entered when Destination Type = 5 - {0}.", EMCSDestinationTypeList.Descriptions.DestinationExemptedConsignee));
			}
		}

		protected new EMCSJobDeclaration Declaration => (EMCSJobDeclaration)base.Declaration;

		protected override ZBool ShouldValidateDispatchReferenceIsMandatory => ZBool.False;
	}
}
