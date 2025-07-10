using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Registry.Business;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngineCore.Testing
{
	sealed class DeliveryMethodHelperTest : TestCaseWithFactory
	{
		public void TestIsEmailOrEPrint()
		{
			AssertEquals(true, DeliveryMethodHelper.IsEmailOrEPrint(Core.Constants.ContactNotifyModes.Email));
			AssertEquals(true, DeliveryMethodHelper.IsEmailOrEPrint(Core.Constants.ContactNotifyModes.EPrint));
			AssertEquals(false, DeliveryMethodHelper.IsEmailOrEPrint(Core.Constants.ContactNotifyModes.DoNotDeliver));
			AssertEquals(false, DeliveryMethodHelper.IsEmailOrEPrint(Core.Constants.ContactNotifyModes.Electronic));
			AssertEquals(false, DeliveryMethodHelper.IsEmailOrEPrint(Core.Constants.ContactNotifyModes.Fax));
			AssertEquals(false, DeliveryMethodHelper.IsEmailOrEPrint(Core.Constants.ContactNotifyModes.Ftp));
			AssertEquals(false, DeliveryMethodHelper.IsEmailOrEPrint(Core.Constants.ContactNotifyModes.Print));
		}

		public void TestIsEmail()
		{
			Assert(DeliveryMethodHelper.IsEmail(ContactNotifyModes.Email));
			Assert(!DeliveryMethodHelper.IsEmail(ContactNotifyModes.EPrint));
			Assert(!DeliveryMethodHelper.IsEmail(ContactNotifyModes.DoNotDeliver));
			Assert(!DeliveryMethodHelper.IsEmail(ContactNotifyModes.Electronic));
			Assert(!DeliveryMethodHelper.IsEmail(ContactNotifyModes.Fax));
			Assert(!DeliveryMethodHelper.IsEmail(ContactNotifyModes.Ftp));
			Assert(!DeliveryMethodHelper.IsEmail(ContactNotifyModes.Print));
		}

		public void TestGetEmailAttachmentOutOfSizeLimitError()
		{
			AssertEquals(string.Empty, DeliveryMethodHelper.GetEmailAttachmentOutOfSizeLimitError(null));
			AssertEquals(string.Empty, DeliveryMethodHelper.GetEmailAttachmentOutOfSizeLimitError(Array.Empty<IDeliveryEmailAttachment>()));
			AssertEquals(string.Empty, DeliveryMethodHelper.GetEmailAttachmentOutOfSizeLimitError(new IDeliveryEmailAttachment[] { null, null }));

			var repository = new MockRepository(MockBehavior.Default);
			var attachment1 = repository.Create<IDeliveryEmailAttachment>();
			var attachment2 = repository.Create<IDeliveryEmailAttachment>();
			var attachment3 = repository.Create<IDeliveryEmailAttachment>();
			var attachment4 = repository.Create<IDeliveryEmailAttachment>();
			var attachment5 = repository.Create<IDeliveryEmailAttachment>();
			var attachment6 = repository.Create<IDeliveryEmailAttachment>();
			var attachment7 = repository.Create<IDeliveryEmailAttachment>();

			attachment1.Setup(t => t.FileName).Returns("Attachment1.XML");
			attachment1.Setup(t => t.FileSizeInBytes).Returns(1 * 1024 * 1024);
			attachment1.Setup(t => t.ShouldBeAttached).Returns(true);

			attachment2.Setup(t => t.FileName).Returns("Attachment2.XML");
			attachment2.Setup(t => t.FileSizeInBytes).Returns(2 * 1024 * 1024);
			attachment2.Setup(t => t.ShouldBeAttached).Returns(false);

			attachment3.Setup(t => t.FileName).Returns("Attachment3.XML");
			attachment3.Setup(t => t.FileSizeInBytes).Returns(3 * 1024 * 1024);
			attachment3.Setup(t => t.ShouldBeAttached).Returns(true);

			attachment4.Setup(t => t.FileName).Returns("Attachment4.XML");
			attachment4.Setup(t => t.FileSizeInBytes).Returns(4 * 1024 * 1024);
			attachment4.Setup(t => t.ShouldBeAttached).Returns(false);

			attachment5.Setup(t => t.FileName).Returns("Attachment5.XML");
			attachment5.Setup(t => t.FileSizeInBytes).Returns(5 * 1024 * 1024);
			attachment5.Setup(t => t.ShouldBeAttached).Returns(true);

			attachment6.Setup(t => t.FileName).Returns("Attachment6.XML");
			attachment6.Setup(t => t.FileSizeInBytes).Returns(1024 * 1024 * 1024);
			attachment6.Setup(t => t.ShouldBeAttached).Returns(true);

			attachment7.Setup(t => t.FileName).Returns("Attachment7.XML");
			attachment7.Setup(t => t.FileSizeInBytes).Returns(1_000_000_000L * 1024 * 1024);
			attachment7.Setup(t => t.ShouldBeAttached).Returns(true);

			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				IDeliveryEmailAttachment[] attachments1 = { attachment1.Object, attachment3.Object, attachment5.Object };
				AssertEquals(
					$"One or more eDoc files exceeds the 2MB attachment limit and cannot be sent: Attachment3.XML, Attachment5.XML. The limit is defined in the Registry at {((IRegistryItemInternals)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB).Location}.",
					DeliveryMethodHelper.GetEmailAttachmentOutOfSizeLimitError(attachments1));

				IDeliveryEmailAttachment[] attachments2 = { attachment2.Object, attachment4.Object };
				AssertEquals(string.Empty, DeliveryMethodHelper.GetEmailAttachmentOutOfSizeLimitError(attachments2));
			}

			//999 999 999 MB when converted to Bytes will overflow int.MaxValue. This makes sure we cater for the max value this registry supports
			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 999_999_999))
			{
				IDeliveryEmailAttachment[] attachments1 = { attachment6.Object };
				AssertEquals(string.Empty, DeliveryMethodHelper.GetEmailAttachmentOutOfSizeLimitError(attachments1));

				IDeliveryEmailAttachment[] attachments2 = { attachment7.Object };
				AssertEquals(
					$"One or more eDoc files exceeds the 999999999MB attachment limit and cannot be sent: Attachment7.XML. The limit is defined in the Registry at {((IRegistryItemInternals)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB).Location}.",
					DeliveryMethodHelper.GetEmailAttachmentOutOfSizeLimitError(attachments2));
			}
		}
	}
}
