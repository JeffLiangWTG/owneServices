using CargoWise.Common;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	public class AccountingLogger : XmlSessionTracker
	{
		public AccountingLogger(ISimpleLogger taskLogger, ITopLevelDataObject topLevelDataObject) : base(taskLogger)
		{
			this.TopLevelDataObject = Argument.NotNull(topLevelDataObject, "ITopLevelDataObject topLevelDataObject");
		}
	}
}