using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5BBLine : IImport5BALine
	{
		ZString AmendDataItemID { get; }
		ZString BeforeDescription { get; }
		ZString AfterDescription { get; }
	}
}
