using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestMessageSendingObjectParentValidation))]
	sealed class ManifestMessageSendingObjectParentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckExportPath()
		{
			var message = "Please select a valid directory path to export the message flat file.";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var context = new MessageSendingContext();
			header.SetCurrentMessageSendingContext(context);

			void ValidateExportPath(bool hasError, SendTarget target, string path)
			{
				context.SendTarget = target;

				var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
				messageSendingObjectParent.ExportPath = path;
				messageSendingObjectParent.Validation.ValidateExportPath();

				if (hasError)
				{
					AssertHasError($"Should has error when the target is {target} and the export path does not exist.", messageSendingObjectParent.ExportPathInfo, message);
				}
				else
				{
					AssertNoErrors($"Should not have any errors when the target is {target} or the export path exists.", messageSendingObjectParent.ExportPathInfo);
				}
			}

			CombineAssertions(() =>
			{
				using var dir = new TempDirectory();
				ValidateExportPath(false, SendTarget.Normal, dir.DirectoryName);
				ValidateExportPath(false, SendTarget.FlatFile, dir.DirectoryName);
				ValidateExportPath(true, SendTarget.FlatFile, string.Empty);
				ValidateExportPath(true, SendTarget.FlatFile, @"S:\InvalidDirForCheckExportPath");
			});
		}

		public void TestCheckEndSendMessage()
		{
			var expectedMessageError = "END should only be used if all house bills under the same master bill have either been registered or included in the current message.";

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			header.AMA_TransportMode = TransportTypeList.Codes.Air;

			var bill = header.Bills.AddNew();
			bill.ABL_MessageStatus = JPMessageStatusList.Codes.Acknowledged;
			header.Bills.AddNew();
			header.Bills.AddNew();

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext()))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObject2 = sendingObjectParent.SendingObjectsCollection[2];
				sendingObject2.ShouldSend = false;

				sendingObjectParent.EndSendMessage = true;
				AssertHasMessageError(sendingObjectParent.EndSendMessageInfo, expectedMessageError);

				sendingObject2.ShouldSend = true;
				AssertNoMessageError(sendingObjectParent.EndSendMessageInfo, expectedMessageError);
			}
		}
	}
}
