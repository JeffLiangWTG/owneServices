using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.DataTransfer.Phase4;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Testing
{
	[TestedType(typeof(NctsHeaderDataContextManager))]
	class NctsHeaderDataContextManagerTests : ShipmentDataContextManagerTestCase<NctsHeaderDataContextManager, NctsHeader>
	{
		public void TestSupportsEventsAtLast()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
			AssertEquals("Manages events", true, manager.ManagesEvents());
		}

		public void TestGetShipmentDataObjectWriter()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
			var writingManager = new DataWritingManager(new ActionInfo(null, header));
			var writer = manager.GetShipmentDataObjectWriter(writingManager);
			AssertEquals(typeof(NctsHeaderDataObjectWriter), writer.GetType());
		}

		public void TestGetShipmentDataObjectWriter_Phase5Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
			var writingManager = new DataWritingManager(new ActionInfo(null, header));
			var writer = manager.GetShipmentDataObjectWriter(writingManager);
			AssertEquals(typeof(Phase5.NctsDepartureMovementHeaderDataObjectWriter), writer.GetType());
		}

		public void TestGetShipmentDataObjectWriter_Phase5Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
			var writingManager = new DataWritingManager(new ActionInfo(null, header));
			var writer = manager.GetShipmentDataObjectWriter(writingManager);
			AssertEquals(typeof(Phase5.NctsArrivalMovementHeaderDataObjectWriter), writer.GetType());
		}

		public void TestGetShipmentDataObjectReader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
			var writingManager = new DataWritingManager(new ActionInfo(null, header));
			var writer = manager.GetShipmentDataObjectWriter(writingManager);

			var shipment = (Shipment)writer.GetDataObject(header);
			var reader = ((IShipmentDataContextManagerInternal)manager).GetShipmentDataObjectReader(shipment, new DummyLogger(), Factory);
			AssertEquals(typeof(NctsHeaderDataObjectReader), reader.GetType());
		}

		public void TestGetShipmentDataObjectReader_Phase5Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
			var writingManager = new DataWritingManager(new ActionInfo(null, header));
			var writer = manager.GetShipmentDataObjectWriter(writingManager);

			var shipment = (Shipment)writer.GetDataObject(header);
			var reader = ((IShipmentDataContextManagerInternal)manager).GetShipmentDataObjectReader(shipment, new DummyLogger(), Factory);
			AssertEquals(typeof(Phase5.NctsDepartureMovementHeaderDataObjectReader), reader.GetType());
		}

		public void TestLoadBusinessObjectFromDataTarget_ShipmentRequest_Phase4Arrival()
		{
			var expectedBusinessObject = GetNewBusinessObjectForTesting();
			expectedBusinessObject.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			expectedBusinessObject.BH_JobReference = "NCT00000001";
			expectedBusinessObject.SetMovementType(NctsMovementType.Codes.Arrival);

			var dataContext = DataContextCreator.Create(DataContextType.NctsHeader, contextKey: expectedBusinessObject.BH_JobReference, companyCode: "EDI");
			var shipmentRequest = new ShipmentRequest { DataContext = dataContext };
			var dataTarget = dataContext.DataTargetCollection.First();

			var manager = new NctsHeaderDataContextManager();
			var actualBusinessObject = manager.LoadBusinessObjectFromDataTarget(shipmentRequest, dataTarget, Factory.BOFactory, new DummyLogger());

			AssertSame(expectedBusinessObject, actualBusinessObject);
		}

		public void TestLoadBusinessObjectFromDataTarget_ShipmentRequest_Phase4Departure()
		{
			var expectedBusinessObject = GetNewBusinessObjectForTesting();
			expectedBusinessObject.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			expectedBusinessObject.BH_JobReference = "NCT00000001";
			expectedBusinessObject.SetMovementType(NctsMovementType.Codes.Departure);

			var dataContext = DataContextCreator.Create(DataContextType.NctsHeader, contextKey: expectedBusinessObject.BH_JobReference, companyCode: "EDI");
			var shipmentRequest = new ShipmentRequest { DataContext = dataContext };
			var dataTarget = dataContext.DataTargetCollection.First();

			var manager = new NctsHeaderDataContextManager();
			var actualBusinessObject = manager.LoadBusinessObjectFromDataTarget(shipmentRequest, dataTarget, Factory.BOFactory, new DummyLogger());

			AssertSame(expectedBusinessObject, actualBusinessObject);
		}

		public void TestLoadBusinessObjectFromDataTarget_ShipmentRequest_Phase5Arrival()
		{
			var expectedBusinessObject = GetNewBusinessObjectForTesting();
			expectedBusinessObject.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			expectedBusinessObject.BH_JobReference = "NCT00000001";
			expectedBusinessObject.SetMovementType(NctsMovementType.Codes.Arrival);

			var dataContext = DataContextCreator.Create(DataContextType.NctsHeader, contextKey: expectedBusinessObject.BH_JobReference, companyCode: "EDI");
			var shipmentRequest = new ShipmentRequest { DataContext = dataContext };
			var dataTarget = dataContext.DataTargetCollection.First();

			var manager = new NctsHeaderDataContextManager();
			var actualBusinessObject = manager.LoadBusinessObjectFromDataTarget(shipmentRequest, dataTarget, Factory.BOFactory, new DummyLogger());

			AssertSame(expectedBusinessObject, actualBusinessObject);
		}

		public void TestLoadBusinessObjectFromDataTarget_ShipmentRequest_Phase5Departure()
		{
			var expectedBusinessObject = GetNewBusinessObjectForTesting();
			expectedBusinessObject.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			expectedBusinessObject.BH_JobReference = "NCT00000001";
			expectedBusinessObject.SetMovementType(NctsMovementType.Codes.Departure);

			var dataContext = DataContextCreator.Create(DataContextType.NctsHeader, contextKey: expectedBusinessObject.BH_JobReference, companyCode: "EDI");
			var shipmentRequest = new ShipmentRequest { DataContext = dataContext };
			var dataTarget = dataContext.DataTargetCollection.First();

			var manager = new NctsHeaderDataContextManager();
			var actualBusinessObject = manager.LoadBusinessObjectFromDataTarget(shipmentRequest, dataTarget, Factory.BOFactory, new DummyLogger());

			AssertSame(expectedBusinessObject, actualBusinessObject);
		}

		public void TestGetCountryCode()
		{
			var manager = new NctsHeaderDataContextManagerForTest();
			NctsHeader nullHeader = null;
			NctsHeader gbHeader;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				gbHeader = Factory.New<NctsHeader>();
				AssertEquals("GbHeader Prereq", "GB", gbHeader.CountryCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Null header", "DE", manager.GetCountryCode_Exposed(nullHeader));
					AssertEquals("GB header", "GB", manager.GetCountryCode_Exposed(gbHeader));
				});
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Liechtenstein))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Country Of Jurisdiction is used", Core.Constants.CountryCodes.Switzerland, manager.GetCountryCode_Exposed(nullHeader));
					AssertEquals("GB header", "GB", manager.GetCountryCode_Exposed(gbHeader));
				});
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.EU.NCTS.DataTransfer.Testing.TestFiles.SampleNctsUniversalXml.xml"))
				{
					using (var sr = new StreamReader(stream))
					{
						return sr.ReadToEnd();
					}
				}
			}
		}

		protected override NctsHeader GetNewBusinessObjectForTesting()
		{
			var header = base.GetNewBusinessObjectForTesting();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			return header;
		}

		class NctsHeaderDataContextManagerForTest : NctsHeaderDataContextManager
		{
			public ZString GetCountryCode_Exposed(NctsHeader header) => base.GetCountryCode(header);
		}
	}
}
