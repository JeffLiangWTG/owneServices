using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IDVDCommonDataProvider : IESEDIMessageCollectionProvider
	{
		ZString MRN { get; }
	}

	public interface IDVDCommonPackage : IPackageCommon
	{
		ZInt NumberOfPackages { get; }
	}
}
