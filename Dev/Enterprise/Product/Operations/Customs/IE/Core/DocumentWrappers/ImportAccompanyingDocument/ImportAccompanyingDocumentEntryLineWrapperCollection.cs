using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.IE.DocumentWrappers
{
	public class ImportAccompanyingDocumentEntryLineWrapperCollection : DocBaseWrapperCollection
	{
		public ImportAccompanyingDocumentEntryLineWrapperCollection(CusEntryHeader header)
			: base(header.Factory)
		{
			header.AllEntryLines.OrderBy(x => x.CL_LineNumber).ThenBy(x => x.PK).ForEach(x => Add(ImportAccompanyingDocumentEntryLineWrapper.New(header, x)));
		}
	}
}
