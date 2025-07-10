using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	sealed class CusContainerTest : Customs.Business.Testing.BaseCusContainerWithCustomLabelsTestCase
	{
		public void TestConvertBCNToFCX()
		{
			CusContainer destination = Factory.New<CusContainer>();
			CommonContainer source = Factory.New<CommonContainer>();
			AssertEquals(Enterprise.Core.Constants.ContainerModes.FCLMixedShipper, destination.ModeConverter.ConvertFreightToCustoms(
				Enterprise.Core.Constants.ContainerModes.BuyersConsol));
		}

		public void TestContainerCase()
		{
			var container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "crxu1234567";
			AssertEquals("Lowercase should be forced to uppercase", "CRXU1234567", container.CO_ContainerNumber);
		}

		public void TestContainerType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			Assert("NoMessageErrors", !container.CO_FCL_LCL_AIRInfo.HasMessageErrors());

			container.CO_FCL_LCL_AIR = "300";
			Assert("MessageErrors", container.CO_FCL_LCL_AIRInfo.HasMessageErrors());

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			Assert("NoMessageErrors", !container.CO_FCL_LCL_AIRInfo.HasMessageErrors());

			container.CO_FCL_LCL_AIR = "";
			Assert("MessageErrors", container.CO_FCL_LCL_AIRInfo.HasMessageErrors());
		}

		public void TestJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			AssertEquals(declaration, container.JobDeclaration);
		}

		public void TestIsDeclarationPersistent()
		{
			var container = Factory.New<CusContainer>();
			AssertEquals(true, container.IsDeclarationPersistent);
			AssertEquals(true, container.IsSavedByFactory);

			var declaration = Factory.New<JobDeclaration>();
			container.CO_JE = declaration.PK;
			AssertEquals("IsDeclarationPersistent with persistent declaration", true, container.IsDeclarationPersistent);
			AssertEquals("IsSavedByFactory with persistent declaration", true, container.IsSavedByFactory);

			declaration.MakeNonPersistent();
			AssertEquals("IsDeclarationPersistent with non-persistent declaration", false, container.IsDeclarationPersistent);
			AssertEquals("IsSavedByFactory with non-persistent declaration", false, container.IsSavedByFactory);

			declaration.Delete();
			AssertEquals(true, container.IsDeclarationPersistent);
			AssertEquals(true, container.IsSavedByFactory);
		}

		public void TestExportDoesntRequireContainerMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = ZString.Empty;
			Assert("Empty mode for Export is not a message error", !container.CO_FCL_LCL_AIRInfo.HasMessageErrors());
		}

		public void TestEdificeRequiresContainerMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = ZString.Empty;
			Assert("empty mode for edifice is a message error", container.CO_FCL_LCL_AIRInfo.HasMessageErrors());
		}

		public void TestISupportDataImporting()
		{
			BusinessObject testObjectToImport = Factory.New(typeof(CusContainer));
			Assert("Woolies importer requires the class to support ISupportDataImporting", testObjectToImport is ISupportDataImporting);
			((ISupportDataImporting)testObjectToImport).IsImportingData = true;
			AssertEquals("Importing should be set to true", true, ((ISupportDataImporting)testObjectToImport).IsImportingData);
			((ISupportDataImporting)testObjectToImport).IsImportingData = false;
			AssertEquals("Importing should be set to false", false, ((ISupportDataImporting)testObjectToImport).IsImportingData);
		}

		public void TestLastMessageSentWasCancellation()
		{
			var container = Factory.New<CusContainer>();
			AssertEquals(false, container.LastPRAMessageSentWasCancellation);

			EDIMessage message1 = container.PRAMessages.AddNew();
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SCN"; // Cancel Message
			AssertEquals(true, container.LastPRAMessageSentWasCancellation);

			EDIMessage message2 = container.PRAMessages.AddNew();
			message2.EM_ReceiveTransmit = "TRX";
			message2.EM_MessageSubType = "SSM"; // Send Message
			AssertEquals(false, container.LastPRAMessageSentWasCancellation);
		}

		public void TestLastPRAStatus()
		{
			var container = Factory.New<CusContainer>();
			EDIMessage message = container.PRAMessages.AddNew();
			AssertEquals("PRA Cancellation Message Sent but not responded to yet.", container.CurrentPRAStatus);
			message.EM_MessageSubType = "SSM";
			AssertEquals("PRA Submit Message Sent but not responded to yet.", container.CurrentPRAStatus);
		}

		public void TestCO_FCL_LCL_AIR_ForMessaging()
		{
			CusContainer container = CusContainer.New(Factory);
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("BBK should be mapped to B/B", CMRCargoTypes.Codes.BreakBulk, container.CO_FCL_LCL_AIR_ForMessaging);
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("Anything other than BBK should be mapped as is", Core.Constants.ContainerModes.FCL, container.CO_FCL_LCL_AIR_ForMessaging);
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("Anything other than BBK should be mapped as is", Core.Constants.ContainerModes.LCL, container.CO_FCL_LCL_AIR_ForMessaging);
			container.CO_FCL_LCL_AIR = "ZZZ";
			AssertEquals("Anything other than BBK should be mapped as is", "ZZZ", container.CO_FCL_LCL_AIR_ForMessaging);
		}

		public void TestAddInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var container = CusContainer.New(Factory);
			container.CO_JE = declaration.PK;
			container.SealStartNumber = "1";
			container.SealEndNumber = "2";
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var container2 = factory2.Load<CusContainer>(container.PK);
			AssertEquals("1", container2.SealStartNumber);
			AssertEquals("2", container2.SealEndNumber);
		}

		public void TestReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT123456";

			declaration.PlaceHold("IAN TEST READONLY");
			AssertEquals(true, declaration.ReadOnly);
			AssertEquals(true, container.ReadOnly);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals(true, declaration2.ReadOnly);
			var container2 = declaration2.CusContainers[0];
			AssertEquals(true, container2.ReadOnly);

			declaration2.RemoveHold();
			AssertEquals(false, declaration2.ReadOnly);
			AssertEquals(false, declaration2.ReadOnly);

			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var declaration3 = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals(false, declaration3.ReadOnly);
			var container3 = declaration3.CusContainers[0];
			AssertEquals(false, container3.ReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			CusContainer result = declaration.CusContainers.AddNew();
			Customs.Business.Testing.TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(result.Factory);
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			declaration.Importer.OH_FullName = "Test Importer";
			declaration.Importer.MainAddress.OA_Address1 = "Importers Address";
			declaration.Importer.OH_RL_NKClosestPort = "AUSYD";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			CusContainer result = declaration.CusContainers.AddNew();
			return result;
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
		{
			ICustomLabelsProvider result = new BaseCusContainer.CustomLabelsProvider(((CusContainer)bO).Declaration);
			return result;
		}

		#endregion
	}
}
