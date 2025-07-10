using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business;

public class CMRRelatedTransaction : CodeDescriptionPair
{
	protected CMRRelatedTransaction(object code, string description)
		: base(code, description)
	{
	}

	public static readonly CMRRelatedTransaction Default = new CMRRelatedTransaction("DEF", "Default value");
	public static readonly CMRRelatedTransaction Yes = new CMRRelatedTransaction("Y", "Yes");
	public static readonly CMRRelatedTransaction No = new CMRRelatedTransaction("N", "No");
}
