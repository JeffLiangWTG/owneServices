using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class DocCDSEntryLineWrapperCollection : DocBaseWrapperCollection
	{
		public DocCDSEntryLineWrapperCollection(CusEntryHeader header)
			: base(header.Factory)
		{
			header.AllEntryLines.Cast<CusEntryLine>().ForEach(x => Add(DocCDSEntryLineWrapper.New(x)));
		}
	}
}
