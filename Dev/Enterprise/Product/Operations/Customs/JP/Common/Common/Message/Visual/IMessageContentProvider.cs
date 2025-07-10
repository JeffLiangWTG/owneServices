using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public interface IMessageContentProvider
{
	BusinessObjectFactory Factory { get; }

	ZString ProcedureCode { get; }

	byte[] GetMessageData();
}
