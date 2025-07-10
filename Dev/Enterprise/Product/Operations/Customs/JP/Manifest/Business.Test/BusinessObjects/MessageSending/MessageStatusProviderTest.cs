using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing;

[TestedType(typeof(MessageStatusProvider))]
sealed class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
{
	public override void TestAllowCancellationMessage()
	{
		Assert(!statusProvider.AllowCancellationMessage(header));
	}

	public override void TestAllowManifestCancellationMessage()
	{
		Assert(!statusProvider.AllowManifestCancellationMessage(header));
	}

	public override void TestAllowModificationMessage()
	{
		Assert(!statusProvider.AllowModificationMessage(header));
	}

	public override void TestAllowOriginalMessage()
	{
		Assert(!statusProvider.AllowOriginalMessage(header));
	}

	public override void TestHasManifestBeenAcceptedByCustoms()
	{
		Assert(!statusProvider.HasManifestBeenAcceptedByCustoms(header));
	}

	public override void TestHasManifestBeenSubmittedToCustoms()
	{
		Assert(!statusProvider.HasManifestBeenSubmittedToCustoms(header));
	}

	public override void TestMessageStatusCanBeReset()
	{
		Assert(!statusProvider.MessageStatusCanBeReset(header));
	}

	public void TestGetMessageStatusList()
	{
		AssertType<JPMessageStatusList>(statusProvider.GetMessageStatusList(Factory, Core.Constants.CountryCodes.Japan));
	}

	public void TestGetRegistrationStatusList()
	{
		AssertType<JPCustomsStatusList>(statusProvider.GetRegistrationStatusListCore(Factory, Core.Constants.CountryCodes.Japan));
	}

	protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider() => statusProvider;

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Japan;
		statusProvider = header.MessageStatusProvider;
	}

	AsycudaManifestHeader header;
	ASYCUDA.Business.MessageStatusProvider statusProvider;
}
