using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.CA.Business
{
	[SystemDefinedValues]
	public abstract partial class AutoJobDeclaration
	{
		internal AddInfoJobDeclaration GetAddInfo()
		{
			return AddInfo;
		}
	}
}
