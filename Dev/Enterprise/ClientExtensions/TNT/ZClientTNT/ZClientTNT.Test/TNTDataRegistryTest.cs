using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	[TestedType(typeof(TNTDataRegistry))]
	public class TNTDataRegistryTest : RegistryItemSetTestCase<TNTDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals(20, AllItems.Count);
			AssertVisible(ItemSet.QuantumFileSourceDirectoryRaw);
			AssertVisible(ItemSet.QuantumFileProcessedDirectoryRaw);
			AssertVisible(ItemSet.QuantumFileSourceDirectoryForManualImportRaw);
			AssertVisible(ItemSet.QuantumFileProcessedDirectoryForManualImportRaw);
			AssertVisible(ItemSet.INDFileImportConsignmentValueThresholdItem);
			AssertVisible(ItemSet.AirCargoResponseExportHighWaterMarkItem);
			AssertVisible(ItemSet.NADFileSourceDirectoryRaw);
			AssertVisible(ItemSet.NADFileProcessedDirectoryRaw);
			AssertVisible(ItemSet.NADFileExtensionRaw);
			AssertVisible(ItemSet.TNTReplyDirectoryRaw);
			AssertVisible(ItemSet.IQDownFileSourceDirectoryRaw);
			AssertVisible(ItemSet.IQDownFileProcessedDirectoryRaw);
			AssertVisible(ItemSet.OutTurnFileSourceDirectoryRaw);
			AssertVisible(ItemSet.OutTurnFileProcessedDirectoryRaw);
			AssertVisible(ItemSet.CustomsStatusCodeRaw);
			AssertVisible(ItemSet.XXXFileSourceDirectoryRaw);
			AssertVisible(ItemSet.XXXFileProcessedDirectoryRaw);
			AssertVisible(ItemSet.DeclarationCustomsResponseExportDirectoryRaw);
			AssertVisible(ItemSet.DeclarationEventsForCustomsResponseItem);
			AssertVisible("LastEDNReturnTimeRaw");
		}

		#region IQDown
		public void TestIQDownFileSourceDirectory()
		{
			ZString previousValue = ItemSet.IQDownFileSourceDirectory;
			try
			{
				ItemSet.IQDownFileSourceDirectory = ZString.Empty;
				AssertEquals("IQDownFileSourceDirectory should be empty", ZString.Empty, ItemSet.IQDownFileSourceDirectory);
				ZString newDirectory = @"blah\blhah\ ";
				ItemSet.IQDownFileSourceDirectory = newDirectory;
				AssertEquals("IQDownFileSourceDirectory should be '" + newDirectory + "'", newDirectory, ItemSet.IQDownFileSourceDirectory);
			}
			finally
			{
				ItemSet.IQDownFileSourceDirectory = previousValue;
			}
		}

		public void TestIQDownFileProcessedDirectory()
		{
			ZString previousValue = ItemSet.IQDownFileProcessedDirectory;
			try
			{
				ItemSet.IQDownFileProcessedDirectory = ZString.Empty;
				AssertEquals("IQDownFileProcessedDirectory should be empty", ZString.Empty, ItemSet.IQDownFileProcessedDirectory);
				ZString newDirectory = @"blah\blhah\ ";
				ItemSet.IQDownFileProcessedDirectory = newDirectory;
				AssertEquals("IQDownFileProcessedDirectory should be '" + newDirectory + "'", newDirectory, ItemSet.IQDownFileProcessedDirectory);
			}
			finally
			{
				ItemSet.IQDownFileProcessedDirectory = previousValue;
			}
		}

		public void TestAirCargoResponseExportHighWaterMark()
		{
			SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime());
			AssertEquals("registry default value", new ZDateTime(SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value), ItemSet.AirCargoResponseExportHighWaterMarkItem.DefaultValue);
			ZDateTime highwaterMark = new ZDateTime(2009, 10, 10, 22, 34, 10);
			ItemSet.AirCargoResponseExportHighWaterMarkItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, highwaterMark.ToDateTime());
			AssertEquals("registry value", highwaterMark, ItemSet.AirCargoResponseExportHighWaterMark);
		}

		#endregion
		#region OutTurn
		public void TestOutTurnSourceDirectory()
		{
			ZString previousValue = ItemSet.OutTurnFileSourceDirectory;
			try
			{
				ItemSet.OutTurnFileSourceDirectory = ZString.Empty;
				AssertEquals("IQDownFileProcessedDirectory should be empty", ZString.Empty, ItemSet.OutTurnFileSourceDirectory);
				ZString newDirectory = @"blah\blhah\ ";
				ItemSet.OutTurnFileSourceDirectory = newDirectory;
				AssertEquals("IQDownFileProcessedDirectory should be '" + newDirectory + "'", newDirectory, ItemSet.OutTurnFileSourceDirectory);
			}
			finally
			{
				ItemSet.OutTurnFileSourceDirectory = previousValue;
			}
		}

		#endregion
		#region CustomsStatusCode
		public void TestCustomsStatusCode()
		{
			ReadOnlyCodeDescriptionPairList previousValue = ItemSet.CustomsStatusCode;
			try
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				ItemSet.CustomsStatusCode = list;
				AssertEquals("Customs Status Code List is empty", 0, ItemSet.CustomsStatusCode.Count);
				list.AddPair("CLC", "Y");
				ItemSet.CustomsStatusCode = list;
				AssertEquals("Customs Status Code List has one Pair", 1, ItemSet.CustomsStatusCode.Count);
				AssertEquals("Code/Status", "CLC", ItemSet.CustomsStatusCode[0].Code);
				AssertEquals("Description/ClearedForDelivery", "Y", ItemSet.CustomsStatusCode[0].Description);
			}
			finally
			{
				ItemSet.CustomsStatusCode = previousValue;
			}
		}

		#endregion
		#region XXX
		public void TestXXXFileSourceDirectory()
		{
			ZString previousValue = ItemSet.XXXFileSourceDirectory;
			try
			{
				ItemSet.XXXFileSourceDirectory = ZString.Empty;
				AssertEquals("XXXFileSourceDirectory should be empty", ZString.Empty, ItemSet.XXXFileSourceDirectory);
				ZString newDirectory = @"blah\blhah\ ";
				ItemSet.XXXFileSourceDirectory = newDirectory;
				AssertEquals("XXXFileSourceDirectory should be '" + newDirectory + "'", newDirectory, ItemSet.XXXFileSourceDirectory);
			}
			finally
			{
				ItemSet.XXXFileSourceDirectory = previousValue;
			}
		}

		public void TestXXXFileProcessedDirectory()
		{
			ZString previousValue = ItemSet.XXXFileProcessedDirectory;
			try
			{
				ItemSet.XXXFileProcessedDirectory = ZString.Empty;
				AssertEquals("XXXFileProcessedDirectory should be empty", ZString.Empty, ItemSet.XXXFileProcessedDirectory);
				ZString newDirectory = @"blah\blhah\ ";
				ItemSet.XXXFileProcessedDirectory = newDirectory;
				AssertEquals("XXXFileProcessedDirectory should be '" + newDirectory + "'", newDirectory, ItemSet.XXXFileProcessedDirectory);
			}
			finally
			{
				ItemSet.XXXFileProcessedDirectory = previousValue;
			}
		}

		#endregion
		#region DeclarationCustomsResponse
		public void TestDeclarationCustomsResponseDirectory()
		{
			AssertEquals("DeclarationCustomsResponseExportDirectory default value", ZString.Empty, ItemSet.DeclarationCustomsResponseExportDirectory);
			ItemSet.DeclarationCustomsResponseExportDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
			AssertEquals("DeclarationCustomsResponseExportDirectory default value", Env.TempPath, ItemSet.DeclarationCustomsResponseExportDirectory);
		}

		public void TestDeclarationEventsForCustomsResponseItem()
		{
			AssertEquals("No events set yet", 0, ItemSet.DeclarationEventsForCustomsResponseItem.Value.Count);
			EventRegistryBusinessObjectCollection collection = new EventRegistryBusinessObjectCollection();
			EventRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.Code = Events.CustomsEntryStatus.Code;
			bizObj.Reference = "ABC";
			Guid currentCompanyGuid = GlbCompany.CurrentCompany.PK.ToGuid();
			ItemSet.DeclarationEventsForCustomsResponseItem.SetValue(currentCompanyGuid, Guid.Empty, Guid.Empty, collection);
			EventRegistryBusinessObjectCollection collection1 = new EventRegistryBusinessObjectCollection();
			EventRegistryBusinessObject bizObj1 = collection1.AddNew();
			bizObj1.Code = Events.CustomsCommenced.Code;
			bizObj1.Reference = "CDE";
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbCompany company = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, currentCompanyGuid));
			ItemSet.DeclarationEventsForCustomsResponseItem.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, collection1);
			EventRegistryBusinessObjectCollection loadedCollection = ItemSet.DeclarationEventsForCustomsResponseItem.GetValueWithoutFallback(currentCompanyGuid, Guid.Empty, Guid.Empty);
			Assert(loadedCollection.ContainsCode(Events.CustomsEntryStatus.Code, "ABC"));
			AssertEquals("The other company should not contain CES event code", true, !loadedCollection.ContainsCode(Events.CustomsCommenced.Code, "CDE"));
		}

		#endregion
		#region TestDataRegistryRaw
		public void TestIQDownFileSourceDirectoryRaw()
		{
			DataRegistryDirectoryTest(ItemSet.IQDownFileSourceDirectoryRaw);
		}

		public void TestIQDownFileProcessedDirectoryRaw()
		{
			DataRegistryDirectoryTest(ItemSet.IQDownFileProcessedDirectoryRaw);
		}

		public void TestOutTurnSourceDirectoryRaw()
		{
			DataRegistryDirectoryTest(ItemSet.OutTurnFileSourceDirectoryRaw);
		}

		public void TestINDFileImportConsignmentValueThreshold()
		{
			AssertEquals("Consignment value threshold (Default Value):", 400.00m, ItemSet.INDFileImportConsignmentValueThresholdItem.DefaultValue);
			AssertEquals("Consignment value threshold (Storage):", RegistryStorageFlags.System, ItemSet.INDFileImportConsignmentValueThresholdItem.Storage);
			TNTDataRegistry.Instance.INDFileImportConsignmentValueThreshold = 499.12m;
			AssertEquals("Consignment value threshold:", 499.12m, ItemSet.INDFileImportConsignmentValueThresholdItem.Value);
		}

		public void TestOutTurnProcessedDirectoryRaw()
		{
			DataRegistryDirectoryTest(ItemSet.OutTurnFileProcessedDirectoryRaw);
		}

		public void DataRegistryDirectoryTest(StringRegistryItem dataRegistryDirectoryRaw)
		{
			string previousValue = dataRegistryDirectoryRaw.Value;
			try
			{
				dataRegistryDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
				AssertEquals("DataRegistry Directory Should be Empty", ZString.Empty, dataRegistryDirectoryRaw.Value);
				string newDirectory = @"blah\blhah\ ";
				dataRegistryDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDirectory);
				AssertEquals("DataRegistry Directory should be '" + newDirectory + "'", newDirectory, dataRegistryDirectoryRaw.Value);
			}
			finally
			{
				dataRegistryDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, previousValue);
			}
		}

		#endregion
		#region TestIsVisible
		public void TestIsVisible()
		{
			GlbBranch.CurrentBranch.SetCountry(Core.Constants.CountryCodes.Australia);
			IRegistryItem[] items = ItemSet.GetAllItems();
			foreach (IRegistryItem item in items)
			{
				ZString itemName = (ZString)item.Name;
				if (itemName == "INDFileImportConsignmentValueThreshold")
				{
					Assert("NZ Consignment Value Threshold Registry should not be visible in Australia", !item.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid()));
				}
				else if (itemName.Contains("ForManualImport"))
				{
					Assert("NZ Manual ImportQuantum Data Registry should be visible in Australia", item.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid()));
				}
				else
				{
					Assert("Other TNTDataRegistry Items should be visible in Australia", item.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid()));
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				foreach (IRegistryItem item in items)
				{
					ZString itemName = (ZString)item.Name;
					if (itemName == "INDFileImportConsignmentValueThreshold" || itemName.Contains("Quantum") && itemName != "QuantumFileProcessedDirectory" && itemName != "QuantumFileSourceDirectory")
					{
						Assert("NZ Quantum Data Registry Should be Visible in New Zealand", item.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid()));
					}
					else
					{
						Assert("Data Registry should not be visible in New Zealand because it is not a Quantum Reigstry", !item.IsVisible(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid()));
					}
				}
			}
		}

		#endregion
		#region TestThrowExceptionIfNotInAustralia
		[ExpectNoExceptions()]
		public void TestExceptionIsNotThrown()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("CDE", "Description");
			ItemSet.CustomsStatusCode = list;
			AssertEquals(1, ItemSet.CustomsStatusCode.Count);
			ItemSet.IQDownFileProcessedDirectory = "blah";
			AssertEquals("blah", ItemSet.IQDownFileProcessedDirectory);
			ItemSet.IQDownFileSourceDirectory = "blah";
			AssertEquals("blah", ItemSet.IQDownFileSourceDirectory);
			ItemSet.LastEDNReturnTime = new ZDateTime(2005, 1, 16);
			AssertEquals(new ZDateTime(2005, 1, 16), ItemSet.LastEDNReturnTime);
			ItemSet.NADFileExtension = "blah";
			AssertEquals("blah", ItemSet.NADFileExtension);
			ItemSet.NADFileProcessedDirectory = "blah";
			AssertEquals("blah", ItemSet.NADFileProcessedDirectory);
			ItemSet.NADFileSourceDirectory = "nad";
			AssertEquals("nad", ItemSet.NADFileSourceDirectory);
			ItemSet.OutTurnFileProcessedDirectory = "outturn";
			AssertEquals("outturn", ItemSet.OutTurnFileProcessedDirectory);
			ItemSet.OutTurnFileSourceDirectory = "outturn";
			AssertEquals("outturn", ItemSet.OutTurnFileSourceDirectory);
			ItemSet.QuantumFileProcessedDirectory = "quantum";
			AssertEquals("quantum", ItemSet.QuantumFileProcessedDirectory);
			ItemSet.QuantumFileSourceDirectory = "quantum";
			AssertEquals("quantum", ItemSet.QuantumFileSourceDirectory);
			ItemSet.TNTReplyDirectory = "reply";
			AssertEquals("reply", ItemSet.TNTReplyDirectory);
			ItemSet.XXXFileProcessedDirectory = "xxx";
			AssertEquals("xxx", ItemSet.XXXFileProcessedDirectory);
			ItemSet.XXXFileSourceDirectory = "xxx";
			AssertEquals("xxx", ItemSet.XXXFileSourceDirectory);
			//GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.QuantumFileProcessedDirectoryForManualImport = "nzquantum";
			AssertEquals("nzquantum", ItemSet.QuantumFileProcessedDirectoryForManualImport);
			ItemSet.QuantumFileSourceDirectoryForManualImport = "nzquantum";
			AssertEquals("nzquantum", ItemSet.QuantumFileSourceDirectoryForManualImport);
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForTNTReply()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.TNTReplyDirectory = "";
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForCustomsStatusCode()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.CustomsStatusCode = new CodeDescriptionPairList();
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForIQDownFileProcessed()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.IQDownFileProcessedDirectory = "";
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForIQDownFileSource()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.IQDownFileSourceDirectory = "";
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForLastEDNReturnTime()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.LastEDNReturnTime = ZDateTime.Now;
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForNADFileExtension()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.NADFileExtension = "";
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForNADFileProcessed()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.NADFileProcessedDirectory = "";
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForNADFileSource()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.NADFileSourceDirectory = "";
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForOutTurnFileProcessed()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.OutTurnFileProcessedDirectory = "";
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForOutTurnFileSource()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.OutTurnFileSourceDirectory = "";
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForXXXFileProcessed()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.XXXFileProcessedDirectory = "";
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestExceptionThrownForXXXFileSource()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			ItemSet.XXXFileSourceDirectory = "";
		}
		#endregion
	}
}
