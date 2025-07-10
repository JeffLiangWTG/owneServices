using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Integration.Services;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.Business.Services
{
	public class DashUtils : IDashUtils
	{
		const string PortalPrefix = "DIN";
		const string formFlowId = "4cf2f4339cda4e7fb8316f6cf823df83";

		public string GetCorrectionToolUrl(string docPK)
		{
			if (!Guid.TryParse(docPK, out var docPKGuid))
			{
				return string.Empty;
			}

			var factory = new BusinessObjectFactory();
			var documentQuery = new ZQuery()
				.AddToFilter(new ZQuery(DashDocumentSchema.DDD_DocID, new ZGuid(docPKGuid)))
				.AddToFilter(new ZQuery(DashDocumentSchema.DDD_IsObsolete, 0));
			var document = factory.LoadTop1<DashDocument>(documentQuery);

			if (document == null)
			{
				return string.Empty;
			}

			var portalsUri = GlowRegistry.Instance.GlowPortalsUri.Value.TrimEnd('/');

			return FormattableString.Invariant($"{portalsUri}/{PortalPrefix}/Desktop#/formFlow/{formFlowId}/{document.PK}");
		}
	}
}
