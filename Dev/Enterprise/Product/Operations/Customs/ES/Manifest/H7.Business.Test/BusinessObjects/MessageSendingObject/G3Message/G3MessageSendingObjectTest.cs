using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Manifest.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3MessageSendingObject))]
	public class G3MessageSendingObjectTest : EU.H7.Business.Testing.MessageSendingObjectTest
	{
		public void TestAction()
		{
			AssertEquals("Default Action is G3D when no G3MRN", G3MessageTypes.Codes.G3Declaration, g3DSendingObject.Action);

			bill.G3MovementReferenceNumber = "123";
			var g3DSendingObjectWithG3MRN = new G3MessageSendingObject(bill);
			AssertEquals("Default Action is empty when G3MRN exists", string.Empty, g3DSendingObjectWithG3MRN.Action);

			AssertEquals("Action for revoke message", G3MessageTypes.Codes.G3Revoke, g3RSendingObject.Action);
		}

		public void TestActionReadOnly()
		{
			Assert("Action should be readonly when no G3MRN", g3DSendingObject.ActionInfo.ReadOnly);

			bill.G3MovementReferenceNumber = "123";
			Assert("Action should be editable when G3MRN exists", !g3DSendingObject.ActionInfo.ReadOnly);

			Assert("Action should be readonly for revoke message", g3RSendingObject.ActionInfo.ReadOnly);
		}

		public void TestShouldSend()
		{
			Assert("ShouldSend should be true when no G3MRN", g3DSendingObject.ShouldSend);

			bill.G3MovementReferenceNumber = "123";
			var g3DSendingObjectWithG3MRN = new G3MessageSendingObject(bill);
			Assert("ShouldSend should be false when G3MRN exists", !g3DSendingObjectWithG3MRN.ShouldSend);

			Assert("ShouldSend should be true for revoke message", g3RSendingObject.ShouldSend);
		}

		public void TestRevokeReasonCode()
		{
			AssertContainsExactElementsInAnyOrder(ExpectedRevokeReasonList, g3DSendingObject.RevokeReasonList);
			AssertEquals("default reason code should be G3001", "G3001", g3DSendingObject.RevokeReason);
		}

		public void TestG3LocalReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.G3LocalReferenceNumber = "G3LRN123";

			var messageSendingObject = new G3MessageSendingObject(bill);
			var resourceStringDataAttribute = messageSendingObject.G3LocalReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("Expected value", "G3LRN123", messageSendingObject.G3LocalReferenceNumber);
				AssertEquals("Expection short caption", "LRN (G3)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Expection caption", "G3 Local Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("Expection description", "A system-generated local reference number to uniquely identify each single G3 declaration.", resourceStringDataAttribute.FullDescription);
				Assert("Expection readonly", messageSendingObject.G3LocalReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestG3MovementReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.G3MovementReferenceNumber = "G3MRN123";

			var messageSendingObject = new G3MessageSendingObject(bill);
			var resourceStringDataAttribute = messageSendingObject.G3MovementReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("Expected value", "G3MRN123", messageSendingObject.G3MovementReferenceNumber);
				AssertEquals("Expection short caption", "MRN (G3)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Expection caption", "G3 Movement Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("Expection description", "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.", resourceStringDataAttribute.FullDescription);
				Assert("Expection readonly", messageSendingObject.G3MovementReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestH7MovementReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.H7MovementReferenceNumber = "H7MRN123";

			var messageSendingObject = new G3MessageSendingObject(bill);
			var resourceStringDataAttribute = messageSendingObject.H7MovementReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("Expected value", "H7MRN123", messageSendingObject.H7MovementReferenceNumber);
				AssertEquals("Expection short caption", "MRN (H7)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Expection caption", "H7 Movement Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("Expection description", "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.", resourceStringDataAttribute.FullDescription);
				Assert("Expection readonly", messageSendingObject.H7MovementReferenceNumberInfo.ReadOnly);
			});
		}

		CodeDescriptionPairList ExpectedRevokeReasonList
		{
			get
			{
				return new CodeDescriptionPairList
				{
					new CodeDescriptionPair("G3001", "Shipment not arrived"),
					new CodeDescriptionPair("G3002", "Shipment duplicated"),
					new CodeDescriptionPair("G3003", "Shipment type rectification"),
				};
			}
		}

		protected override CodeDescriptionPairList ExpectedActionList
		{
			get
			{
				return new CodeDescriptionPairList
				{
					new CodeDescriptionPair(G3MessageTypes.Codes.G3Declaration, G3MessageTypes.Descriptions.G3Declaration)
				};
			}
		}

		protected override CodeDescriptionPairList ExpectedSubStyleList => new CodeDescriptionPairList();

		protected override EU.H7.Business.AsycudaBill GetMessageSendingObjectParentBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			return bill;
		}

		protected override EU.H7.Business.MessageSendingObject GetMessageSendingObject(EU.H7.Business.AsycudaBill bill)
		{
			((AsycudaBill)bill).G3MovementReferenceNumber = "123";
			return new G3MessageSendingObject(bill as AsycudaBill, false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			bill.H7MovementReferenceNumber = "1234";

			g3DSendingObject = new G3MessageSendingObject(bill);
			g3RSendingObject = new G3MessageSendingObject(bill, true);
		}

		AsycudaBill bill;
		G3MessageSendingObject g3DSendingObject;
		G3MessageSendingObject g3RSendingObject;
	}
}
