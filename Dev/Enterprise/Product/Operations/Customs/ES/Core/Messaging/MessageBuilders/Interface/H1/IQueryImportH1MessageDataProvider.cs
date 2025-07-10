using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IQueryImportH1MessageDataProvider : IH1ImportCommonDataProvider
{
	IH1CommonMRN DataProviderMRN { get; }
	ZString CustomsRegistrationNumber { get; }
	ZString ATC { get; }
}
