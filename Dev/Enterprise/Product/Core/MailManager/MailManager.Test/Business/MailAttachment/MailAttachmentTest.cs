using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using MailManager;
using NUnit.Framework;

namespace Enterprise.MailManager.Business.Testing
{
	[TestedType(typeof(MailAttachment))]
	sealed class MailAttachmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAttachmentSizeInBytes()
		{
			var myAttachment = Factory.New<MailAttachment>();
			myAttachment.MA_Encoding = "B64";
			myAttachment.MA_Data = Encoding.Default.GetBytes(new string('A', 1024 * 100));

			AssertEquals((double)1024 * 100, myAttachment.AttachmentSizeInBytes);
			AssertEquals("100KB", myAttachment.HumanReadableAttachmentSize);
		}

		public void TestMailAttachment()
		{
			AssertNotNull("MyAttachment", Factory.New<MailAttachment>());
		}

		public void TestEncodingType()
		{
			var myAttachment = Factory.New<MailAttachment>();
			myAttachment.MA_Encoding = "B64";
			AssertEquals("EncodingType", MailAttachment.EncodingEnum.Base64, myAttachment.EncodingType);
			myAttachment.MA_Encoding = "XXX";
			AssertEquals("EncodingType", MailAttachment.EncodingEnum.Unknown, myAttachment.EncodingType);
		}

		public void TestSaveTo()
		{
			var myAttachment = Factory.New<MailAttachment>();
			myAttachment.MA_FileName = "pic.bmp";
			var fileData = resourceRetriever.Value.GetBytes("Enterprise.MailManager.Test.Business.MailItem.TestFiles.pic.bmp");
			myAttachment.MA_Data = new ZBlob(fileData);

			string testFilesDir = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			Directory.CreateDirectory(testFilesDir);
			string fileName = Path.Combine(testFilesDir, myAttachment.MA_FileName);

			AssertEquals("Attachment file does not exist", false, File.Exists(fileName));

			try
			{
				AssertEquals(myAttachment.HumanReadableAttachmentSize, MailAttachment.AttachmentSize(fileData.Length));
				myAttachment.SaveTo(testFilesDir);
				var savedFileData = ReadBytes(fileName);

				AssertEquals("Attachment file exists", true, File.Exists(fileName));
				AssertEquals("Identical content", fileData, savedFileData);

				var otherFileData = resourceRetriever.Value.GetBytes("Enterprise.MailManager.Test.Business.MailItem.TestFiles.ComplexHeader.txt");
				myAttachment.MA_Data = new ZBlob(otherFileData);

				myAttachment.SaveTo(testFilesDir);
				savedFileData = ReadBytes(fileName);

				AssertEquals("File was overriden", otherFileData, savedFileData);
			}
			finally
			{
				File.Delete(fileName);
				Directory.Delete(testFilesDir);
				AssertEquals("Attachment file does not exist", false, File.Exists(fileName));
				AssertEquals("Test directory does not exist", false, Directory.Exists(testFilesDir));
			}
		}

		public void TestSaveTo_IllegalCharacters()
		{
			var myAttachment = Factory.New<MailAttachment>();
			myAttachment.MA_FileName = "pic?:.bmp";
			string testFilesDir = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			Directory.CreateDirectory(testFilesDir);
			string expectedFileName = Path.Combine(testFilesDir, "pic.bmp");

			AssertEquals("Attachment file does not exist", false, File.Exists(expectedFileName));

			try
			{
				myAttachment.SaveTo(testFilesDir);
				AssertEquals("Attachment file exists", true, File.Exists(expectedFileName));
			}
			finally
			{
				File.Delete(expectedFileName);
				Directory.Delete(testFilesDir);
				AssertEquals("Attachment file does not exist", false, File.Exists(expectedFileName));
				AssertEquals("Test directory does not exist", false, Directory.Exists(testFilesDir));
			}
		}

		public void TestSaveTo_ReturnPath()
		{
			var myAttachment = Factory.New<MailAttachment>();
			myAttachment.MA_FileName = "2_1.zip";

			var tempDir = Env.TempPath;
			string testFilesDir = Path.Combine(tempDir, ZGuid.BrettsGuid.ToString() + ".tmp");
			Directory.CreateDirectory(testFilesDir);
			string filepath = Path.Combine(testFilesDir, myAttachment.MA_FileName);

			AssertEquals("Attachment file does not exist", false, File.Exists(filepath));

			try
			{
				var returnDirectoryPath = myAttachment.SaveTo(testFilesDir);

				AssertEquals("Attachment file exists",
					tempDir + "20dd961b-3e62-40e5-b60a-b1312b70f5ee.tmp\\2_1.zip",
					returnDirectoryPath);
			}
			finally
			{
				File.Delete(filepath);
				Directory.Delete(testFilesDir);
				AssertEquals("Attachment file does not exist", false, File.Exists(filepath));
				AssertEquals("Test directory does not exist", false, Directory.Exists(testFilesDir));
			}
		}

		public void TestHumanReadableAttachmentSize()
		{
			var mail = Factory.NewWithValidTestData<MailItem>();
			mail.MI_Direction = MailDirection.Transmit;

			var attachment = mail.MailAttachments.AddNew();
			attachment.MA_FileName = "Sample.pdf";
			var fileData = resourceRetriever.Value.GetBytes("Enterprise.MailManager.Test.Business.MailItem.TestFiles.Sample.pdf");
			attachment.MA_Data = new ZBlob(fileData);
			Factory.Save();

			var expectedAttachmentSize = MailAttachment.AttachmentSize(fileData.Length);
			AssertEquals(expectedAttachmentSize, attachment.HumanReadableAttachmentSize);
		}

		public void TestAttachmentSize()
		{
			AssertEquals("0B", MailAttachment.AttachmentSize(0));
			AssertEquals("1B", MailAttachment.AttachmentSize(1));
			AssertEquals("1023B", MailAttachment.AttachmentSize(1024 - 1));
			AssertEquals("1KB", MailAttachment.AttachmentSize(1024));
			AssertEquals("1.5KB", MailAttachment.AttachmentSize(1024.0 * 1.5));
			AssertEquals("1023KB", MailAttachment.AttachmentSize(1024 * 1023));
			AssertEquals("1MB", MailAttachment.AttachmentSize(1024 * 1024));
			AssertEquals("1.5MB", MailAttachment.AttachmentSize(1024.0 * 1024.0 * 1.5));
			AssertEquals("1023MB", MailAttachment.AttachmentSize(1024 * 1024 * 1023));
			AssertEquals("1GB", MailAttachment.AttachmentSize(1024 * 1024 * 1024 + 1));
			AssertEquals("1.5GB", MailAttachment.AttachmentSize(1024.0 * 1024.0 * 1024.0 * 1.5));
			AssertEquals("1023GB", MailAttachment.AttachmentSize(1024L * 1024L * 1024L * 1023L));
			AssertEquals("1024GB", MailAttachment.AttachmentSize(1024L * 1024L * 1024L * 1024L));
			AssertEquals("1536GB", MailAttachment.AttachmentSize((1024.0 * 1024.0 * 1024.0 * 1024.0 + 1.0) * 1.5));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var item = factory.New<MailItem>();
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_LastAttemptDateTime = ZDateTime.UtcNow;
			item.MI_ReceivedDateTime = ZDateTime.UtcNow;
			item.MI_SendDateTime = ZDateTime.UtcNow;

			return item.MailAttachments.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		byte[] ReadBytes(string filename)
		{
			using (var inputStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				var inputBytes = new byte[inputStream.Length];
				inputStream.Read(inputBytes, 0, inputBytes.Length);
				return inputBytes;
			}
		}
	}
}
