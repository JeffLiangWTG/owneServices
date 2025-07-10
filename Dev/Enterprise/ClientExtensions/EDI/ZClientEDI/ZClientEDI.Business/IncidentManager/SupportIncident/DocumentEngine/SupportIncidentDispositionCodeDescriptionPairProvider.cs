using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentDispositionCodeDescriptionPairProvider : DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();

			var lookups = new SupportIncidentLookups(new BusinessObjectFactory());
			foreach (CodeDescriptionPair stage in new SupportIncidentCategoriesList())
			{
				foreach (CodeDescriptionPair status in lookups.StatusList)
				{
					foreach (CodeDescriptionPair criticality in new IncidentApprovalLookups(null).CriticalityList)
					{
						foreach (ICodeDescription product in lookups.ProductList)
						{
							var dispositions = lookups.GetStatusDispositionList(stage.Code, status.Code, criticality.Code, product.Code, activeOnly: true);
							foreach (ICodeDescription disposition in dispositions)
							{
								string key = stage.Code + "_" + status.Code + "_" + criticality.Code + "_" + product.Code + "_" + disposition.Code;
								result.AddPair(key, disposition.Description);
							}
						}
					}
				}
			}

			return result;
		}
	}
}

