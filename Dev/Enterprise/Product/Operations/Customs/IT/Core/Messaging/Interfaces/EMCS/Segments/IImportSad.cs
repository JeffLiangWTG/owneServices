using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface IImportSadContainer
{
	IEnumerable<IImportSad> ImportSads { get; }
}

public interface IImportSad
{
	ZString ImportSadNumber { get; }
}
