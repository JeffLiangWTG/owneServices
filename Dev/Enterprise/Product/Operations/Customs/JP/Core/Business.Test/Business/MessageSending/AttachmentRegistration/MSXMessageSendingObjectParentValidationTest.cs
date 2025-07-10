using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(MSXMessageSendingObjectParentValidation))]
sealed class MSXMessageSendingObjectParentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckExportPath()
	{
		var message = "Please select a valid directory path to export the message flat file.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
		declaration.ActiveEntryHeaders.AddNew();

		var messageSendingContext = new MessageSendingContext();
		declaration.SetCurrentMessageSendingContext(messageSendingContext);

		void ValidateExportPath(bool hasError, SendTarget target, string path)
		{
			messageSendingContext.SendTarget = target;

			var messageSendingObjectParent = new MSXMessageSendingObjectParent(declaration);
			messageSendingObjectParent.ExportPath = path;
			messageSendingObjectParent.Validation.ValidateExportPath();

			if (hasError)
			{
				AssertHasError($"Should has error when the target is {target} and the export path is not existing.", messageSendingObjectParent.ExportPathInfo, message);
			}
			else
			{
				AssertNoErrors($"Should not have any errors when the target is {target} and the export path is existing.", messageSendingObjectParent.ExportPathInfo);
			}
		}

		CombineAssertions(() =>
		{
			using (var dir = new TempDirectory())
			{
				ValidateExportPath(false, SendTarget.Normal, dir.DirectoryName);
				ValidateExportPath(false, SendTarget.FlatFile, dir.DirectoryName);
				ValidateExportPath(true, SendTarget.FlatFile, string.Empty);
				ValidateExportPath(true, SendTarget.FlatFile, @"S:\InvalidDirForCheckExportPath");
			}
		});
	}
}
