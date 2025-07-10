using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.Integration.Licensing;

namespace Enterprise.ZArchitecture.Core
{
	public interface ILicenceProxy
	{
		ILicenceCheckpoint AlwaysAllow { get; }
		ILicenceCheckpoint Booking { get; }
		ILicenceCheckpoint ContainerManager { get; }
		ILicenceCheckpoint Core { get; }
		int DefaultLicenceGracePeriodInDays { get; }
		ILicenceCheckpoint DocManager { get; }
		ILicenceCheckpoint FaxEngine { get; }
		ILicenceCheckpoint InterfaceConnector { get; }
		ILicenceCheckpoint DataWizard { get; }
		ILicenceCheckpoint ShippingManager { get; }
		ILicenceCheckpoint TranslationFeedback { get; }
		ILicenceCheckpoint Packing { get; }
		ILicenceCheckpoint UniversalCopy { get; }
		ILicenceCheckpoint GetCheckpointFromCode(string code);
		ILicenceCheckpoint[] GetAllCheckpoints();

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		IDisposable UseLanguageLicense(ref string language, LanguageUsageType usageType);
	}
}
