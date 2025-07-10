using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.MY.Business.Testing
{
	abstract class PortOperatorTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			CTOAddressPK = CTO.MainAddress.PK;

			notifications = new NotificationBuffer();
			PortOperatorCode.OK_CustomsRegNo = "BKCT";
			CustomsStation.OK_CustomsRegNo = "";
			Berth = "1";
			new PortOperator(Consol, Consol.IsImport()).Validate(notifications);
			Assert("Mandatory Customs Station", notifications.AsString.IndexOf("Mandatory field Customs Station Code") != -1);

			notifications = new NotificationBuffer();
			PortOperatorCode.OK_CustomsRegNo = "";
			CustomsStation.OK_CustomsRegNo = "B10";
			Berth = "1";
			new PortOperator(Consol, Consol.IsImport()).Validate(notifications);
			Assert("Mandatory Port Operator", notifications.AsString.IndexOf("Mandatory field Port Operator Code") != -1);

			notifications = new NotificationBuffer();
			PortOperatorCode.OK_CustomsRegNo = "BKCT";
			CustomsStation.OK_CustomsRegNo = "B10";
			Berth = "";
			new PortOperator(Consol, Consol.IsImport()).Validate(notifications);
			Assert("Mandatory berth number", notifications.AsString.IndexOf("Mandatory field Berth Number not entered") != -1);

			notifications = new NotificationBuffer();
			PortOperatorCode.OK_CustomsRegNo = "BKCT";
			CustomsStation.OK_CustomsRegNo = "B10";
			Berth = "1x";
			new PortOperator(Consol, Consol.IsImport()).Validate(notifications);
			Assert("Invalid berth number", notifications.AsString.IndexOf("Invalid berth number") != -1);

			notifications = new NotificationBuffer();
			Berth = "999";
			new PortOperator(Consol, Consol.IsImport()).Validate(notifications);
			Assert("Berth number too big", notifications.AsString.IndexOf("Berth number too big") != -1);

			notifications = new NotificationBuffer();
			PortOperatorCode.OK_CustomsRegNo = "BKCT";
			CustomsStation.OK_CustomsRegNo = "B10";
			Berth = "99";
			new PortOperator(Consol, Consol.IsImport()).Validate(notifications);
			AssertEquals("All fields valid", false, notifications.HasErrors);
		}

		public void TestPortOperatorCode()
		{
			PortOperatorCode.OK_CustomsRegNo = "BKCT";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			CTOAddressPK = CTO.MainAddress.PK;
			AssertEquals("BKCT", new PortOperator(Consol, ImportExportHelper.IsBranchCountry(Consol.JK_RL_NKDischargePort)).PortOperatorCode);
		}

		public void TestCustomsStation_InTestMode()
		{
			Env.Registry.MYCustoms.IsTestMode = true;
			AssertEquals("H10", new PortOperator(Consol, ImportExportHelper.IsBranchCountry(Consol.JK_RL_NKDischargePort)).CustomsStation);
		}

		public void TestCustomsStationCode()
		{
			CustomsStation.OK_CustomsRegNo = "B10";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			CTOAddressPK = CTO.MainAddress.PK;
			AssertEquals("B10", new PortOperator(Consol, ImportExportHelper.IsBranchCountry(Consol.JK_RL_NKDischargePort)).CustomsStation);
		}

		public void TestPortOperatorAndSCN()
		{
			CTOAddressPK = CTO.MainAddress.PK;
			PortOperatorCode.OK_CustomsRegNo = "BKCT";
			ArrivalOrDepartureReference = "SCN";
			AssertEquals("BKCT:SCN", new PortOperator(Consol, ImportExportHelper.IsBranchCountry(Consol.JK_RL_NKDischargePort)).PortOperatorAndSCN);
		}

		public void TestPortOperatorAndBerth()
		{
			CTOAddressPK = CTO.MainAddress.PK;

			PortOperatorCode.OK_CustomsRegNo = "BKCT";
			Berth = "02";
			AssertEquals("BKCT02", new PortOperator(Consol, ImportExportHelper.IsBranchCountry(Consol.JK_RL_NKDischargePort)).PortOperatorAndBerth);
		}

		#region Implementation

		NotificationBuffer notifications = new NotificationBuffer();

		protected abstract ZString Berth { get; set; }
		protected abstract ZGuid CTOAddressPK { get; set; }
		protected abstract ZString ArrivalOrDepartureReference { get; set; }

		protected ForwardingConsol Consol
		{
			get { return consol ?? (consol = GetNewConsol()); }
		}
		ForwardingConsol consol;

		ForwardingConsol GetNewConsol()
		{
			ForwardingConsol result = GetNewConsolCore();
			return result;
		}

		protected virtual ForwardingConsol GetNewConsolCore()
		{
			ForwardingConsol result = Factory.New<ForwardingConsol>();
			return result;
		}

		OrgHeader CTO
		{
			get
			{
				if (cto == null)
				{
					cto = Factory.New<OrgHeader>();
				}
				return cto;
			}
		}
		OrgHeader cto;

		OrgCusCode CustomsStation
		{
			get
			{
				if (customsStation == null)
				{
					customsStation = CTO.CustomsCodes.AddNew();
					customsStation.OK_CodeType = MalaysiaOrgCusCodeInfo.OrgCusCodes.CustomsStation;
				}
				return customsStation;
			}
		}
		OrgCusCode customsStation;

		OrgCusCode PortOperatorCode
		{
			get
			{
				if (portOperatorCode == null)
				{
					portOperatorCode = CTO.CustomsCodes.AddNew();
					portOperatorCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
				}
				return portOperatorCode;
			}
		}
		OrgCusCode portOperatorCode;

		#endregion
	}

	class PortOperatorTest_ForImport : PortOperatorTest
	{
		protected override ForwardingConsol GetNewConsolCore()
		{
			ForwardingConsol result = base.GetNewConsolCore();

			Transport decoyTransport = result.Transports.AddNew();
			decoyTransport.JW_RL_NKLoadPort = "USLAX";
			decoyTransport.JW_RL_NKDiscPort = "USDEN";

			Transport transport = result.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_Vessel = "Vessel";
			transport.JW_VoyageFlight = "Voyage";
			transport.JW_RL_NKLoadPort = "USDEN";
			transport.JW_RL_NKDiscPort = "MYPKG";
			transport.JW_IsLinked = true;

			Transport decoyTransport2 = result.Transports.AddNew();
			decoyTransport2.JW_RL_NKLoadPort = "MYPKG";
			decoyTransport2.JW_RL_NKDiscPort = "MYPEN";

			Transport decoyTransport3 = result.Transports.AddNew();
			decoyTransport3.JW_RL_NKLoadPort = "MYPEN";
			decoyTransport3.JW_RL_NKDiscPort = "SGSIN";

			result.JK_RL_NKLoadPort = "USDEN";
			result.JK_RL_NKDischargePort = "MYPKG";
			AssertEquals("Consol is import for the test", true, result.IsImport());
			AssertNotNull("Consol.Transports.ImportTransport", result.Transports.ImportTransport);
			AssertNotNull("Consol.Transports.ImportTransport.Sailing", result.Transports.ImportTransport.Sailing);
			return result;
		}

		protected override ZString Berth
		{
			get { return Consol.Transports.ImportTransport.JW_JX_DepartOrArriveBerth; }
			set { Consol.Transports.ImportTransport.Sailing.Destination.JB_Berth = value; }
		}

		protected override ZGuid CTOAddressPK
		{
			get { return Consol.JK_OA_ArrivalCTOAddress; }
			set { Consol.JK_OA_ArrivalCTOAddress = value; }
		}

		protected override ZString ArrivalOrDepartureReference
		{
			get { return Consol.Transports.ImportTransport.JW_JX_DepartOrArriveReference; }
			set { Consol.Transports.ImportTransport.Sailing.Destination.JB_ArrivalReference = value; }
		}
	}

	class PortOperatorTest_ForExport : PortOperatorTest
	{
		protected override ForwardingConsol GetNewConsolCore()
		{
			ForwardingConsol result = base.GetNewConsolCore();

			Transport decoyTransport = result.Transports.AddNew();
			decoyTransport.JW_RL_NKLoadPort = "USLAX";
			decoyTransport.JW_RL_NKDiscPort = "MYPKG";

			Transport decoyTransport2 = result.Transports.AddNew();
			decoyTransport2.JW_RL_NKLoadPort = "MYPKG";
			decoyTransport2.JW_RL_NKDiscPort = "MYPEN";

			Transport transport = result.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_Vessel = "Vessel";
			transport.JW_VoyageFlight = "Voyage";
			transport.JW_RL_NKLoadPort = "MYPEN";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_IsLinked = true;

			Transport decoyTransport3 = result.Transports.AddNew();
			decoyTransport3.JW_RL_NKLoadPort = "USLAX";
			decoyTransport3.JW_RL_NKDiscPort = "USDEN";

			result.JK_RL_NKLoadPort = "MYPEN";
			result.JK_RL_NKDischargePort = "USLAX";
			AssertEquals("Consol export for the test", true, result.IsExport());
			AssertNotNull("Consol.Transports.ExportTransport", result.Transports.ExportTransport);
			AssertNotNull("Consol.Transports.ExportTransport.Sailing", result.Transports.ExportTransport.Sailing);
			return result;
		}

		protected override ZString Berth
		{
			get { return Consol.Transports.ExportTransport.JW_JX_DepartOrArriveBerth; }
			set { Consol.Transports.ExportTransport.Sailing.Origin.JA_Berth = value; }
		}

		protected override ZGuid CTOAddressPK
		{
			get { return Consol.JK_OA_DepartureCTOAddress; }
			set { Consol.JK_OA_DepartureCTOAddress = value; }
		}

		protected override ZString ArrivalOrDepartureReference
		{
			get { return Consol.Transports.ExportTransport.JW_JX_DepartOrArriveReference; }
			set { Consol.Transports.ExportTransport.Sailing.Origin.JA_DepartReference = value; }
		}
	}
}
