namespace Enterprise.Customs.AE.Manifest.Business;

public interface IMessageDetailsProvider
{
	string DocumentCode { get; }

	string DocumentIdentifier { get; }

	string Version { get; }

	string MessageFunction { get; }
}
