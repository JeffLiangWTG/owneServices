using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public abstract class DocumentBrandingCollection : ClientAndAgentBrandingCollection
	{
		public DocumentBrandingCollection()
		{
		}

		public DocumentBrandingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new DocumentBrandingBusinessObject this[int i]
		{
			get { return (DocumentBrandingBusinessObject)Elements[i]; }
		}

		public new DocumentBrandingBusinessObject FindByCode(string code)
		{
			return (DocumentBrandingBusinessObject)base.FindByCode(code);
		}
	}
}
