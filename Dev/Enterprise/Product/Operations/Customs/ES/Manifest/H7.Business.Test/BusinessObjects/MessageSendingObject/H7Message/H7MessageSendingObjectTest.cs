using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(H7MessageSendingObject))]
	sealed class H7MessageSendingObjectTest : EU.H7.Business.Testing.MessageSendingObjectTest
	{
		public void TestSetDefaultData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.G3MovementReferenceNumber = "123";
			bill.H7MovementReferenceNumber = ZString.Empty;
			var h7MessageSendingObjectWithoutMRN = new H7MessageSendingObject(bill);
			AssertEquals("Bill with no H7MRN should default action to H7Declaration", DeclarationMessageTypeList.Codes.H7Declaration, h7MessageSendingObjectWithoutMRN.Action);

			bill.H7MovementReferenceNumber = "H7MRN";
			var h7MessageSendingObjectWithtMRN = new H7MessageSendingObject(bill);
			AssertEquals("Bill with H7MRN should default action to empty", string.Empty, h7MessageSendingObjectWithtMRN.Action);
		}

		public void TestActionListChangesWithMRN()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.G3MovementReferenceNumber = "123";
			bill.H7MovementReferenceNumber = ZString.Empty;
			var h7MessageSendingObjectWithoutMRN = new H7MessageSendingObject(bill);

			var actionListCodes = h7MessageSendingObjectWithoutMRN.ActionList.GetAllCodes();
			CombineAssertions(() =>
			{
				AssertEquals("Bill with no MRN should only have 1 action", 1, actionListCodes.Length);
				Assert("Bill with no MRN should only have H7Declaration action available", actionListCodes.Contains(DeclarationMessageTypeList.Codes.H7Declaration));
			});

			bill.H7MovementReferenceNumber = "123";
			var h7MessageSendingObjectWithMRN = new H7MessageSendingObject(bill);

			actionListCodes = h7MessageSendingObjectWithMRN.ActionList.GetAllCodes();
			CombineAssertions(() =>
			{
				AssertEquals("Bill with no MRN should have 3 actions", 3, actionListCodes.Length);
				Assert("Bill with no MRN should have H7ReExport available", actionListCodes.Contains(DeclarationMessageTypeList.Codes.H7ReExport));
				Assert("Bill with no MRN should have H7Cancellation available", actionListCodes.Contains(DeclarationMessageTypeList.Codes.H7Cancellation));
				Assert("Bill with no MRN should have H7Query available", actionListCodes.Contains(DeclarationMessageTypeList.Codes.H7Query));
			});
		}

		public void TestDefaultOperationCodeOnReexportMessageType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var messageSendingObject = new H7MessageSendingObject(bill);

			messageSendingObject.Action = DeclarationMessageTypeList.Codes.H7ReExport;
			AssertEquals("Message sending object Reexport type should default operation code", OperationCodeList.Codes.InvalidateH7WithExsEtd, messageSendingObject.OperationCode);
		}

		public void TestEmptyOperationCodeOnNonReexportMessageType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var messageSendingObject = new H7MessageSendingObject(bill);

			messageSendingObject.Action = DeclarationMessageTypeList.Codes.H7Declaration;
			AssertEquals("Message sending object Non-Reexport type should be empty", ZString.Empty, messageSendingObject.OperationCode);
		}

		public void TestOperationCodeReadonlyOnNonReexportMessageType()
		{
			{
				var header = Factory.New<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				var messageSendingObject = new H7MessageSendingObject(bill);

				messageSendingObject.Action = DeclarationMessageTypeList.Codes.H7ReExport;
				AssertEquals("Message sending object Reexport type should be editable", false, messageSendingObject.OperationCodeInfo.ReadOnly);

				messageSendingObject.Action = DeclarationMessageTypeList.Codes.H7Declaration;
				AssertEquals("Message sending object Non-Reexport type should be readonly", true, messageSendingObject.OperationCodeInfo.ReadOnly);
			}
		}

		public void TestG3LocalReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.G3LocalReferenceNumber = "G3LRN123";

			var messageSendingObject = new H7MessageSendingObject(bill);
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

			var messageSendingObject = new H7MessageSendingObject(bill);
			var resourceStringDataAttribute = messageSendingObject.G3MovementReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("Expected value", "G3MRN123", messageSendingObject.G3MovementReferenceNumber);
				AssertEquals("Expection short caption", "MRN (G3)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Expection caption", "G3 Movement Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("Expection description", "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.", resourceStringDataAttribute.FullDescription);
				Assert("Expection readonly", messageSendingObject.G3MovementReferenceNumberInfo.ReadOnly);
				Assert("Expection readonly", messageSendingObject.H7MovementReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestH7MovementReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.H7MovementReferenceNumber = "H7MRN123";

			var messageSendingObject = new H7MessageSendingObject(bill);
			var resourceStringDataAttribute = messageSendingObject.H7MovementReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("Expected value", "H7MRN123", messageSendingObject.H7MovementReferenceNumber);
				AssertEquals("Expection short caption", "MRN (H7)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Expection caption", "H7 Movement Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("Expection description", "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.", resourceStringDataAttribute.FullDescription);
			});
		}

		protected override CodeDescriptionPairList ExpectedActionList
		{
			get
			{
				var actionList = new CodeDescriptionPairList();
				actionList.AddPair(DeclarationMessageTypeList.Codes.H7Cancellation, DeclarationMessageTypeList.Descriptions.H7Cancellation);
				actionList.AddPair(DeclarationMessageTypeList.Codes.H7Query, DeclarationMessageTypeList.Descriptions.H7Query);
				actionList.AddPair(DeclarationMessageTypeList.Codes.H7ReExport, DeclarationMessageTypeList.Descriptions.H7ReExport);

				return actionList;
			}
		}

		protected override CodeDescriptionPairList ExpectedSubStyleList => new CodeDescriptionPairList();

		protected override CodeDescriptionPairList ExpectedOperationCodeList => new OperationCodeList();

		protected override ZString ExpectedActionForSendingCustomsDeclaration => DeclarationMessageTypeList.Codes.H7Declaration;

		protected sealed override MessageSendingObject GetMessageSendingObject(EU.H7.Business.AsycudaBill bill)
		{
			((AsycudaBill)bill).H7MovementReferenceNumber = "1234";
			return new H7MessageSendingObject((AsycudaBill)bill);
		}

		protected override EU.H7.Business.AsycudaBill GetMessageSendingObjectParentBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			return bill;
		}
	}
}
