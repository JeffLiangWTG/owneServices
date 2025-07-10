using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.DocumentWrappers.Customs.EU;
using IECusEntryLine = Enterprise.Customs.IE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.IE.DocumentWrappers
{
	sealed class IEDocSADHLine : DocSADHLine
	{
		public static IEDocSADHLine New(IECusEntryLine entryLine, BusinessObjectFactory factory)
		{
			return entryLine == null ? null : new IEDocSADHLine(entryLine, factory);
		}

		IEDocSADHLine(IECusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
		}

		protected override ZString Box44_1ProducedDocumentsCertificatesCore => GetProducedDocumentsCertificatesBuilder().GetProducedDocumentsCertificatesFormatted((EntryHeader.SupportingDocuments ?? Enumerable.Empty<SupportingDocument>()).Concat(EntryLine.SupportingDocuments));
	}
}
