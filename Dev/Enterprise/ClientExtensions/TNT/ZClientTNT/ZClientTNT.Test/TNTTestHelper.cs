using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.TNT.Testing
{
	class TNTTestHelper
	{
		public TNTTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public void SetupNADDataImportRegistries()
		{
			TNTDataRegistry.Instance.NADFileExtension = "NADUPD";
			TNTDataRegistry.Instance.NADFileSourceDirectory = AddTempFolder(Path.Combine(Env.TempPath, "incoming"));
			TNTDataRegistry.Instance.NADFileProcessedDirectory = AddTempFolder(Path.Combine(Env.TempPath, "archive"));
		}

		public void SetupQuantumDataImportRegistries()
		{
			TNTDataRegistry.Instance.QuantumFileSourceDirectory = AddTempFolder(Path.Combine(Env.TempPath, "import"));
			TNTDataRegistry.Instance.QuantumFileProcessedDirectory = AddTempFolder(Path.Combine(Env.TempPath, "processed"));
			TNTDataRegistry.Instance.TNTReplyDirectory = AddTempFolder(Path.Combine(Env.TempPath, "reply"));
		}

		public void SetupAirCargoResponseExportRegistries()
		{
			TNTDataRegistry.Instance.AirCargoResponseExportHighWaterMark = ZDateTime.Now.AddMinutes(-10);
			TNTDataRegistry.Instance.CustomsStatusCode = CreateCustomsStatusCode();
			TNTDataRegistry.Instance.TNTReplyDirectory = AddTempFolder(Path.Combine(Env.TempPath, "aircargoreply"));
		}

		public CodeDescriptionPairList CreateCustomsStatusCode()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(CustomsEntryStatus.CargoCleared.Code, "Y");
			result.AddPair("DOC", "Y");
			result.AddPair(CustomsEntryStatus.ClearSAC.Code, "S");
			result.AddPair(CustomsEntryStatus.DeclarationWorkComplete.Code, "N");

			return result;
		}

		List<String> TempTestFolders
		{
			get { return tempTestFolders ?? (tempTestFolders = new List<string>()); }
		}
		List<String> tempTestFolders;

		public string AddTempFolder(string folderFullName)
		{
			TempTestFolders.Add(folderFullName);
			if (!Directory.Exists(folderFullName))
			{
				Directory.CreateDirectory(folderFullName);
			}
			return folderFullName;
		}

		public void TidyUp()
		{
			foreach (string folderFullName in TempTestFolders)
			{
				TempDirectory.DeleteDirectory(folderFullName);
			}
		}

		public void CopySourceFileToImportDirectory(string sourceFile, string destFile)
		{
			File.Copy(sourceFile, destFile, true);
			File.SetAttributes(destFile, FileAttributes.Normal);
		}

		public void CreateNotificationGroupForSendingEmails()
		{
			GlbGroup notificationGroup = Factory.New<GlbGroup>();
			notificationGroup.GG_Code = TNTConstants.DataImportNotificationGroupCode;
			GlbStaff staff = notificationGroup.Staff.AddNew();
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_Code = "ZAC";
			Factory.Save();
		}

		public string NADTestDataPath
		{
			get { return @"Enterprise\ClientExtensions\TNT\ZClientTNT\ZClientTNT.Test\DataManipulation\NADInterface\TestFiles\"; }
		}

		readonly BusinessObjectFactory Factory;
	}
}
