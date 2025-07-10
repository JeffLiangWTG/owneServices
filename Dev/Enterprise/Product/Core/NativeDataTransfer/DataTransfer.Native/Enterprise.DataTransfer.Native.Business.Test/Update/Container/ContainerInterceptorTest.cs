using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.Testing
{
	public class ContainerInterceptorTest : TestCaseWithFactory
	{
		public void TestProcessNativeXml_INSERT_SameContainerCodeAlreadyExists()
		{
			var refContainer = CreateRefContainer("TS001", Constants.TransportModes.Sea, "001");
			Factory.Save();

			ProcessNativeXml(EntityAction.INSERT, ZGuid.NewZGuid(), refContainer.RC_Code, "002");

			AssertContains("The specified container code TS001 already exists.", GetLogs());
		}

		public void TestProcessNativeXml_UPDATE_SameContainerCodeAlreadyExists()
		{
			var refContainer1 = CreateRefContainer("TS001", Constants.TransportModes.Sea, "001");
			var refContainer2 = CreateRefContainer("TS002", Constants.TransportModes.Sea, "002");
			Factory.Save();

			ProcessNativeXml(EntityAction.UPDATE, refContainer1.PK, refContainer2.RC_Code, "001");

			AssertContains("The specified container code TS002 already exists.", GetLogs());
		}

		public void TestProcessNativeXml_UPDATE_TwoSameContainerCodeAlreadyExist_CodeHasNotBeenModified()
		{
			var refContainer1 = CreateRefContainer("TS001", Constants.TransportModes.Sea, "001");
			var refContainer2 = CreateRefContainer("TS001", Constants.TransportModes.Sea, "002");
			Factory.Save();

			ProcessNativeXml(EntityAction.UPDATE, refContainer1.PK, refContainer1.RC_Code, "003");

			refContainer1.Reload();
			refContainer2.Reload();
			AssertEquals("RefContainer - 0 inserts, 1 updates, 0 deletes", GetLogs());
			AssertEquals(refContainer1.RC_IATARateClass, "003");
			AssertEquals(refContainer2.RC_IATARateClass, "002");
		}

		public void TestProcessNativeXml_MERGE_SameContainerCodeAlreadyExists_TryToUpdateExistsContainer()
		{
			var refContainer1 = CreateRefContainer("TS001", Constants.TransportModes.Sea, "001");
			var refContainer2 = CreateRefContainer("TS002", Constants.TransportModes.Sea, "002");
			Factory.Save();

			ProcessNativeXml(EntityAction.MERGE, refContainer1.PK, refContainer2.RC_Code, "001");

			AssertContains("The specified container code TS002 already exists.", GetLogs());
		}

		public void TestProcessNativeXml_MERGE_SameContainerCodeAlreadyExists_TryToInsertNewContainer()
		{
			var refContainer = CreateRefContainer("TS001", Constants.TransportModes.Sea, "001");
			Factory.Save();

			ProcessNativeXml(EntityAction.MERGE, ZGuid.NewZGuid(), refContainer.RC_Code, "002");

			AssertContains("The specified container code TS001 already exists.", GetLogs());
		}

		void ProcessNativeXml(EntityAction action, ZGuid pk, string code, string iataRateClass)
		{
			var importXml = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>CARED_IT</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Container version=""2.0"">
      <RefContainer Action=""{action}"">
        <PK>{pk}</PK>
        <Code>{code}</Code>
        <ShippingMode>SEA</ShippingMode>
        <Description>twenty foot general purpose container</Description>
        <Length>20.000</Length>
        <Height>8.500</Height>
        <Width>8.000</Width>
        <ContainerType>DRY</ContainerType>
        <ISOType>22G0</ISOType>
        <TareWeight>2350.000</TareWeight>
        <GrossWeight>30480.000</GrossWeight>
        <CubicCapacity>0.000</CubicCapacity>
        <StorageClass>20F</StorageClass>
        <HandlingRateClass>20GP</HandlingRateClass>
        <FreightRateClass>20GP</FreightRateClass>
        <IATARateClass>{iataRateClass}</IATARateClass>
        <USContainerCode></USContainerCode>
        <TEU>3.00</TEU>
        <IsActive>true</IsActive>
        <IsHighCube>false</IsHighCube>
        <HasTynes>false</HasTynes>
        <HasVents>false</HasVents>
        <IsIso>true</IsIso>
        <ISOEquipmentSizeTypeCode></ISOEquipmentSizeTypeCode>
        <IsSystem>true</IsSystem>
        <Contour></Contour>
        <InsideHeight>0.000</InsideHeight>
        <InsideLength>0.000</InsideLength>
        <InsideWidth>0.000</InsideWidth>
        <NetWeight>28130.000</NetWeight>
      </RefContainer>
    </Container>
  </Body>
</Native>";

			importHandler.Import(new MemoryStream(Encoding.Default.GetBytes(importXml)));
		}

		#region Implementation

		RefContainer CreateRefContainer(string code, string shippingMode, string iataRateClass)
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = code;
			refContainer.RC_ShippingMode = shippingMode;
			refContainer.RC_IATARateClass = iataRateClass;

			return refContainer;
		}

		string GetLogs() => string.Join("\r\n", dummyLogger.Buffer.Logs().Select(log => log.Message).ToArray());

		void ErrorOccur(XElement source, Exception ex) => dummyLogger.Error("Test error: " + ex.Message);

		AncillaryImportServices sessionServices;
		ImportHandler importHandler;
		MemoryLogger dummyLogger;

		protected override void SetUp()
		{
			base.SetUp();

			sessionServices = new AncillaryImportServices();
			dummyLogger = sessionServices.Logger as MemoryLogger;
			importHandler = new ImportHandler(sessionServices)
			{
				ErrorOccur = ErrorOccur
			};
		}

		#endregion
	}
}
