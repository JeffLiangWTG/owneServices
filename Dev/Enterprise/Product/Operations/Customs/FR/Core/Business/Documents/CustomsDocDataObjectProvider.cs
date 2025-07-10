using System;
using CargoWise.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Business.Documents.CertificateOfOrigin;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class CustomsDocDataObjectProvider : EU.Business.Documents.DocDataObjects.CustomsDocDataObjectProvider
	{
		protected override EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin GetEURCertificateOfOriginForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => new EURCertificateOfOriginWrapper((Declaration.CusEntryHeader)entryHeader);

		protected override EU.Business.Documents.CertificateOfOrigin.IDV1Certificate GetDV1CertificateForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => new DV1CertificateWrapper((Declaration.CusEntryHeader)entryHeader);

		protected override object GetFromEntryHeader(CusEntryHeader entryHeader, string dataContext, IDocDataObjectParameters parameters)
		{
			using (TemporarilySwitchLanguageIfNeeded(parameters, dataContext))
			{
				switch (dataContext)
				{
					case DataContext.FRPortsCustomsCheckCAED:
						return new EntryCAEDBuilder((Declaration.CusEntryHeader)entryHeader).Build();
					default:
						return base.GetFromEntryHeader(entryHeader, dataContext, parameters);
				}
			}
		}

		IDisposable TemporarilySwitchLanguageIfNeeded(IDocDataObjectParameters parameters, string dataContext)
		{
			return IsTranslationNeeded(parameters, dataContext)
				? Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.French)
				: DisposableAction.NoAction;
		}

		bool IsTranslationNeeded(IDocDataObjectParameters parameters, string dataContext)
		{
			return parameters.DataStoreName == "DV1"
				|| dataContext == DataContext.FRPortsCustomsCheckCAED;
		}
	}
}
