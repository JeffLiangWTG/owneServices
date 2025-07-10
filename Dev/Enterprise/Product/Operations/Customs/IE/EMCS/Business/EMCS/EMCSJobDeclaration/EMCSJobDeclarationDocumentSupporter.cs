using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSJobDeclarationDocumentSupporter : EU.EMCS.Business.EMCSJobDeclarationDocumentSupporter
	{
		public EMCSJobDeclarationDocumentSupporter(EMCSJobDeclaration jobDeclaration) : base(jobDeclaration)
		{
		}

		protected override ZString WrapperClassFullNamespace => "Enterprise.Customs.IE.EMCS.DocumentWrappers.IEEMCSDeclarationWrapper";

		protected override string WrapperClassAssemblyName => "Enterprise.Customs.IE.EMCS.DocumentWrappers";
	}
}
