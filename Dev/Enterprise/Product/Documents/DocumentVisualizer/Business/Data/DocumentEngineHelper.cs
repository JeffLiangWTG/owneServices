using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentVisualizer.Business
{
	public class DocumentEngineHelper
	{
		public BusinessContext GetBusinessContext(BusinessObject bizo)
		{
			var documentSupportable = bizo as IDocumentSupportable;
			if (documentSupportable != null)
			{
				return documentSupportable.DocumentSupporter.BusinessContext;
			}

			return BusinessContext.INVALID;
		}
	}
}
