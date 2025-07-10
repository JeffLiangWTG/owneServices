using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5SMHeader : IImport934_5SMHeader
	{
		ZString ValueDeclarationTemplateNumber { get; }
		IImport5SMFormC FormCData { get; }
		IImport934_5SMFormD FormDData { get; }
		IEnumerable<IImport5SMLine> EntryLines { get; }
	}
}
