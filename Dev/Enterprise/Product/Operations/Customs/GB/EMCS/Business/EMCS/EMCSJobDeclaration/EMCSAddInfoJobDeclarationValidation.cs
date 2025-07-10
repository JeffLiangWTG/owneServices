using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class EMCSAddInfoJobDeclarationValidation : EU.EMCS.Business.EMCSAddInfoJobDeclarationValidation
	{
		public EMCSAddInfoJobDeclarationValidation(EMCSAddInfoJobDeclaration parent) : base(parent) { }

		protected new EMCSJobDeclaration Declaration => (EMCSJobDeclaration)base.Declaration;

		protected override ZBool ShouldValidateDispatchReferenceIsMandatory => ZBool.False;
	}
}
