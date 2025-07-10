using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.AES;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class IE583MessageProvider : ExitReportMessageProvider, IIE583Header, IIE583ExportOperation
	{
		public IE583MessageProvider(CusExitReport exitReport) : base(exitReport)
		{
		}

		#region IIE583Header Members
		public IIE583ExportOperation ExportOperation => this;

		public string CustomsOfficeOfExport => exitReport.CER_OfficeOfExport;

		public string CustomsOfficeOfExitActual => exitReport.CER_OfficeOfExit;

		public IParty ExitCarrier => CachedValueHelper.GetValue(ref exitCarrierCached, () => PartyProvider.New(exitHeader.Carrier));
		CachedValue<IParty> exitCarrierCached;

		public IReadOnlyCollection<IAlternativeEvidence> AlternativeEvidence => alternativeEvidence ?? (alternativeEvidence = exitReport.AlternativeEvidences.Cast<AlternativeEvidence>().Select(e => new AlternativeEvidenceProvider(e)).ToArray());
		IReadOnlyCollection<IAlternativeEvidence> alternativeEvidence;

		public IParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => PartyProvider.New(exitReport.Declarant));
		CachedValue<IParty> declarantCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(exitReport.Representative, exitReport.CER_DeclarantType));
		CachedValue<IRepresentative> representativeCached;
		#endregion

		#region IIE583ExportOperation Members
		public string EnquiryInformationCode => exitReport.CER_EnquiryInformationCode;

		public DateTime ExitDate => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(exitReport.CER_DateTime.ToZDateTime());

		public string MRN => exitConsignment.CXC_MovementReference;
		#endregion
	}
}
