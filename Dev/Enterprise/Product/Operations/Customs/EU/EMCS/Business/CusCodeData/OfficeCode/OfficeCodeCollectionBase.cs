using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business;

public abstract class OfficeCodeCollection : EuOfficeCodeCollection
{
	protected OfficeCodeCollection(EMCSJobDeclaration master) : base(master)
	{
	}
}
