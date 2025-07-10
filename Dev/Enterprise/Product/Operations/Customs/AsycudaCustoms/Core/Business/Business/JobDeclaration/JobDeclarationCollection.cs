using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("_CustomsTemplate")]
	public partial class JobDeclarationCollection : Customs.Business.BaseJobDeclarationCollection
	{
		public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, companyPkToFilterOn)
		{
		}

		protected override bool AllowNewCore => false;
	}
}
