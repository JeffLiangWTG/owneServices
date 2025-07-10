using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;

namespace Enterprise.Customs.IT.Business;

public class InterchangeFileNameStrategy : IInterchangeFileNameStrategy
{
	public InterchangeFileNameStrategy(Account account, ZString messageType, BusinessObjectFactory factory)
	{
		Argument.NotNull(account, nameof(account));
		Argument.NotNullOrEmpty(messageType, nameof(messageType));
		Argument.NotNull(factory, nameof(factory));

		FilenameProvider = new CustomsMessageFilenameProvider(account, messageType, factory);
	}

	ICustomsMessageFilenameProvider FilenameProvider { get; }

	ZString IInterchangeFileNameStrategy.GetFileName() => FilenameProvider.GenerateFilename();
}
