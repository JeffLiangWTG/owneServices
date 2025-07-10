using System;
using CargoWise.Common;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using SupportingDocumentTypes = Enterprise.Customs.IT.Business.UniversalReferenceConstants.SupportingDocumentTypes;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class PortTaxSupportingDocumentRefresher
{
	public PortTaxSupportingDocumentRefresher(ISupportingDocumentsWithHarbourRateProvider provider)
	{
		Argument.NotNull(provider, nameof(provider));

		harbourRateProvider = Argument.NotNull(provider.HarbourRateProvider, nameof(provider.HarbourRateProvider));
		supportingDocumentsMaster = Argument.NotNull(provider.SupportingDocumentsMaster, nameof(provider.SupportingDocumentsMaster));
	}

	public void RefreshDocument()
	{
		var supportingDocuments = supportingDocumentsMaster.SupportingDocuments;
		supportingDocuments.DeleteAllDocumentsHavingCode(SupportingDocumentTypes.PortTax);

		var harbourRate = harbourRateProvider.HarbourRate;
		if (harbourRate != null)
		{
			supportingDocuments.AddNew(
				SupportingDocumentTypes.PortTax,
				FormattableString.Invariant($"--{harbourRate.ZXF_Port}"));
		}
	}

	readonly IHarbourRateProvider harbourRateProvider;
	readonly ISupportingDocumentsProvider supportingDocumentsMaster;
}
