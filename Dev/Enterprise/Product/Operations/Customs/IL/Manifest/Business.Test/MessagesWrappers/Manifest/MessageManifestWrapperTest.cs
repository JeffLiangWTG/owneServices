using System;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class MessageManifestWrapperTest : Customs.Business.Testing.DataProviderTestCase<MessageManifestWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When header is null", () => new MessageManifestWrapper(null));
		}

		public void TestNewOrNull()
		{
			AssertNull(MessageManifestWrapper.NewOrNull(null));
			AssertNotNull(MessageManifestWrapper.NewOrNull(asycudaManifestHeader));
		}

		public void TestDeclaration()
		{
			var wrapper = CreateWrapper();
			var declaration = wrapper.Declaration;
			AssertNotNull(nameof(IMessageManifest.Declaration), declaration);
			AssertType<DeclarationWrapper>(declaration);
			AssertSame("Cached", declaration, wrapper.Declaration);
		}

		public void TestRequestContentHeader()
		{
			var wrapper = CreateWrapper();
			AssertNotNull(wrapper.RequestContentHeader);
			AssertType<RequestContentHeaderWrapper>(wrapper.RequestContentHeader);
		}

		protected override MessageManifestWrapper GetProvider() => MessageManifestWrapper.NewOrNull(asycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
			asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_RN_NKCountry = "IL";
			asycudaManifestHeader.AMA_ApplicationCode = "NVC";
			asycudaManifestHeader.AMA_ManifestType = "785";
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction.Dispose();
		}

		IMessageManifest CreateWrapper() => GetProvider();

		IDisposable disposableAction;
		AsycudaManifestHeader asycudaManifestHeader;
	}
}
