using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentProductAreaCodeDescriptionPairProvider : DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return EDIDataRegistry.Instance.ProductAreas.Value;
		}
	}
}

