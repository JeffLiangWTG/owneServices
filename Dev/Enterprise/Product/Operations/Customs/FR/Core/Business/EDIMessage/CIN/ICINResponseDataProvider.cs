using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public interface ICINResponseDataProvider
	{
		ZString MessageID { get; }
		ZBool Success { get; }
	}
}
