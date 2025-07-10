using CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.IT.Business.Documents.DocDataObjects;

public class CustomsDocDataObjectProvider : EU.Business.Documents.DocDataObjects.CustomsDocDataObjectProvider<CusEntryHeader>
{
	protected override EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin GetEURCertificateOfOriginForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters)
	{
		if (parameters.DataStoreName == Eur1CertificateOfOriginFromXmlDataStoreName)
		{
			return GetEURCertificateOfOriginFromXml(entryHeader);
		}
		else if (parameters.DataStoreName == EurMEDCertificateOfOriginFromXmlDataStoreName)
		{
			return GetEURMEDCertificateOfOriginFromXml(entryHeader);
		}
		return new EURCertificateOfOriginWrapper(entryHeader);
	}

	protected override EU.Business.Documents.CertificateOfOrigin.IATRCertificateOfOrigin GetATRCertificateOfOriginForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters)
	{
		if (parameters.DataStoreName == ATRCertificateOfOriginFromXmlDataStoreName)
		{
			return GetATRCertificateOfOriginFromXml(entryHeader);
		}
		return new ITATRCertificateOfOriginWrapper(entryHeader);
	}

	protected override EU.Business.Documents.CertificateOfOrigin.IDV1Certificate GetDV1CertificateForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => new EU.Business.Documents.CertificateOfOrigin.DV1CertificateWrapper(entryHeader);

	#region Implementation

	EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin GetEURCertificateOfOriginFromXml(CusEntryHeader entryHeader)
	{
		var certificateEur1 = entryHeader.GetCertificateOfOriginMessage()
			.EM_MessageText
			.DeserializeToObject<CERTIFICATO_EUR1>();

		return new XmlEURCertificateOfOriginWrapper(entryHeader.Factory, certificateEur1);
	}

	EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin GetEURMEDCertificateOfOriginFromXml(CusEntryHeader entryHeader)
	{
		var certificateEurMed = entryHeader.GetCertificateOfOriginMessage()
			.EM_MessageText
			.DeserializeToObject<CERTIFICATO_EURMED>();

		return new XmlEURCertificateOfOriginWrapper(entryHeader.Factory, certificateEurMed);
	}

	EU.Business.Documents.CertificateOfOrigin.IATRCertificateOfOrigin GetATRCertificateOfOriginFromXml(CusEntryHeader entryHeader)
	{
		var certificateATR = entryHeader.GetCertificateOfOriginMessage()
			.EM_MessageText
			.DeserializeToObject<CERTIFICATO_ATR>();

		return new XmlATRCertificateOfOriginWrapper(entryHeader.Factory, certificateATR);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant Eur1 Certificate of Origin from Xml data store name")]
	const string Eur1CertificateOfOriginFromXmlDataStoreName = "EUR1 Certificate XML";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant Eur MED Certificate of Origin from Xml data store name")]
	const string EurMEDCertificateOfOriginFromXmlDataStoreName = "EURMED Certificate XML";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant ATR Certificate of Origin from Xml data store name")]
	const string ATRCertificateOfOriginFromXmlDataStoreName = "ATR Certificate XML";

	#endregion
}
