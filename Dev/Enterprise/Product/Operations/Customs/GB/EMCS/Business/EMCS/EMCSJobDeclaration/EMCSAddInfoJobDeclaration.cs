using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class EMCSAddInfoJobDeclaration : EU.EMCS.Business.EMCSAddInfoJobDeclaration
	{
		public EMCSAddInfoJobDeclaration(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		public new EMCSAddInfoJobDeclarationValidation Validation => (EMCSAddInfoJobDeclarationValidation)base.Validation;
		protected override EUEMCSAddInfoValidation GetNewValidation() => new EMCSAddInfoJobDeclarationValidation(this);
	}
}
