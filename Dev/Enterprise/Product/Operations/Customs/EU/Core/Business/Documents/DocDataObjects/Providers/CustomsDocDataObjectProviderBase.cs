using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects;

public abstract class CustomsDocDataObjectProvider<TEntryHeader> : ICustomsDocDataObjectProvider
	where TEntryHeader : CusEntryHeader
{
	public object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
	{
		if (parent is TEntryHeader entryHeader)
		{
			return GetFromEntryHeader(entryHeader, dataContext, parameters);
		}

		return null;
	}

	protected virtual object GetFromEntryHeader(TEntryHeader entryHeader, string dataContext, IDocDataObjectParameters parameters)
	{
		switch (dataContext)
		{
			case DataContext.JobDeclaration:
			case DataContext.EURMEDCertificate:
				return GetJobDeclarationDocDataObjectForEntry(entryHeader, parameters);

			case DataContext.ATRCertificate:
				return GetATRCertificateDocDataObjectForEntry(entryHeader, parameters);

			case DataContext.DV1Certificate:
				return GetDV1DocDataObjectForEntry(entryHeader, parameters);
		}

		return null;
	}

	protected abstract IEURCertificateOfOrigin GetEURCertificateOfOriginForEntry(TEntryHeader entryHeader, IDocDataObjectParameters parameters);

	protected abstract IATRCertificateOfOrigin GetATRCertificateOfOriginForEntry(TEntryHeader entryHeader, IDocDataObjectParameters parameters);

	protected abstract IDV1Certificate GetDV1CertificateForEntry(TEntryHeader entryHeader, IDocDataObjectParameters parameters);

	JobDeclarationDocDataObject GetJobDeclarationDocDataObjectForEntry(TEntryHeader entryHeader, IDocDataObjectParameters parameters)
	{
		var certificateOfOrigin = GetEURCertificateOfOriginForEntry(entryHeader, parameters);
		return new JobDeclarationDocDataObject(certificateOfOrigin);
	}

	DV1DocDataObject GetDV1DocDataObjectForEntry(TEntryHeader entryHeader, IDocDataObjectParameters parameters)
	{
		var dV1 = GetDV1CertificateForEntry(entryHeader, parameters);
		return new DV1DocDataObject(dV1);
	}

	ATRCertificateDocDataObject GetATRCertificateDocDataObjectForEntry(TEntryHeader entryHeader, IDocDataObjectParameters parameters)
	{
		var atr = GetATRCertificateOfOriginForEntry(entryHeader, parameters);
		return new ATRCertificateDocDataObject(atr, entryHeader.Factory);
	}
}
