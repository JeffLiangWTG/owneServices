using System;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_820;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class MessageManifestQueryWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageManifestQuery>
	{
		public void TestNewOrNull()
		{
			AssertNull("When messageSendingObject is null", MessageManifestQueryWrapper.NewOrNull(null));
			AssertNotNull("When messageSendingObject is not null", MessageManifestQueryWrapper.NewOrNull(new AsycudaManifestQueryMessageSendingObject(asycudaManifestHeader)));
		}

		public void TestCargoIdentifier()
		{
			AssertNotNull("CargoIdentifier", Provider.CargoIdentifier);
			AssertType<ManifestQueryCargoIdentifierWrapper>(Provider.CargoIdentifier);
		}

		public void TestContainerNumber()
		{
			AssertNull("ContainerNumber", Provider.ContainerNumber);
		}

		public void TestDocumentNumber()
		{
			AssertNull("DocumentNumber", Provider.DocumentNumber);
		}

		public void TestEntrySiteId()
		{
			AssertNull("EntrySiteId", Provider.EntrySiteId);
		}

		public void TestExitSiteId()
		{
			AssertNull("ExitSiteId", Provider.ExitSiteId);
		}

		public void TestFromEntryExitDateTime()
		{
			AssertNull("FromEntryExitDateTime", Provider.FromEntryExitDateTime);
		}

		public void TestReferenceNum()
		{
			AssertNull("ReferenceNum", Provider.ReferenceNum);
		}

		public void TestReferenceType()
		{
			AssertNull("ReferenceType", Provider.ReferenceType);
		}

		public void TestRequestContentHeader()
		{
			AssertNotNull("RequestContentHeader", Provider.RequestContentHeader);
			AssertType<RequestContentHeaderWrapper>(Provider.RequestContentHeader);
		}

		public void TestToEntryExitDateTime()
		{
			AssertNull("ToEntryExitDateTime", Provider.ToEntryExitDateTime);
		}

		protected override IMessageManifestQuery GetProvider()
		{
			return MessageManifestQueryWrapper.NewOrNull(new AsycudaManifestQueryMessageSendingObject(asycudaManifestHeader));
		}

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

		IDisposable disposableAction;
		AsycudaManifestHeader asycudaManifestHeader;
	}
}


