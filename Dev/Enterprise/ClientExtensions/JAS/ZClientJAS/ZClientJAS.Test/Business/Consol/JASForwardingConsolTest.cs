using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Client.JAS.Business.Testing
{
	public class JASForwardingConsolTest : ForwardingConsolTest
	{
		public void TestTypeDeciderWouldLoadClientSpecificForwardingConsol()
		{
			var consol = Factory.NewWithValidTestData<JASForwardingConsol>();
			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			AssertEquals("JASForwardingConsol should be loaded", typeof(JASForwardingConsol), anotherFactory.Load<CommonConsol>(consol.PK).GetType());
		}

		public void TestAreShipmentJobsClosed()
		{
			JASForwardingShipment shipment1 = (JASForwardingShipment)Consol.Shipments.AddNew();
			JASJob job1 = (JASJob)new Accounting.Business.JobInvoicing.Job.Loader(shipment1).TryCreate();
			job1.JH_ParentID = shipment1.PK;
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipment1.Job.JH_Status = JobHeaderStatus.Closed.Code;
			JASForwardingShipment shipment2 = (JASForwardingShipment)Consol.Shipments.AddNew();
			JASJob job2 = (JASJob)new Accounting.Business.JobInvoicing.Job.Loader(shipment2).TryCreate();
			job2.JH_ParentID = shipment2.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipment2.Job.JH_Status = JobHeaderStatus.Closed.Code;
			Assert("Should be true", Consol.AreShipmentJobsClosed());
			shipment1.Job.JH_Status = JobHeaderStatus.Working.Code;
			Assert("Should be false", !Consol.AreShipmentJobsClosed());
			shipment1.Job.JH_Status = JobHeaderStatus.Closed.Code;
			shipment2.Job.JH_Status = JobHeaderStatus.Complete.Code;
			Assert("Should be false", !Consol.AreShipmentJobsClosed());
		}

		public void TestAreShipmentJobsClosed_NoShipmentJob()
		{
			Assert("No shipments, should be false", !Consol.AreShipmentJobsClosed());
			JASForwardingShipment shipment1 = (JASForwardingShipment)Consol.Shipments.AddNew();
			JASJob job1 = (JASJob)new Accounting.Business.JobInvoicing.Job.Loader(shipment1).TryCreate();
			job1.JH_ParentID = shipment1.PK;
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipment1.Job.JH_Status = JobHeaderStatus.Closed.Code;
			Assert("Should be true", Consol.AreShipmentJobsClosed());
			JASForwardingShipment shipment2 = (JASForwardingShipment)Consol.Shipments.AddNew();
			Assert("Shipment2 does not have a Shipment Job. Should be false", !Consol.AreShipmentJobsClosed());
		}

		public void TestIsSuitableForJXC()
		{
			AssertEquals("No shipments attached", JASForwardingConsol.SuitableForJXCAirOrOceanMessage.NoShipmentAttached, Consol.IsSuitableForJXCAirOrOceanMessage());
			JASForwardingShipment shipment1 = (JASForwardingShipment)Consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("Invalid mode", JASForwardingConsol.SuitableForJXCAirOrOceanMessage.InvalidConsolTransportMode, Consol.IsSuitableForJXCAirOrOceanMessage());
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(JASForwardingConsol.SuitableForJXCAirOrOceanMessage.Suitable, Consol.IsSuitableForJXCAirOrOceanMessage());
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Incompatible mode with shipment 1", JASForwardingConsol.SuitableForJXCAirOrOceanMessage.ConsolAndShipmentsNotCompatible, Consol.IsSuitableForJXCAirOrOceanMessage());
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertEquals(JASForwardingConsol.SuitableForJXCAirOrOceanMessage.Suitable, Consol.IsSuitableForJXCAirOrOceanMessage());
			JASForwardingShipment shipment2 = (JASForwardingShipment)Consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertEquals("Incompatible mode with shipment 2", JASForwardingConsol.SuitableForJXCAirOrOceanMessage.ConsolAndShipmentsNotCompatible, Consol.IsSuitableForJXCAirOrOceanMessage());
		}

		#region Business Object Overrides
		public void TestDocumentSupporter()
		{
			AssertEquals(typeof(JASForwardingConsolDocumentSupporter), Consol.DocumentSupporter.GetType());
		}

		public void TestJASNoteTypes()
		{
			AssertCollectionContains(JASPredefinedNoteTypes.Instance.JXCExportLog, Consol.NoteTypes);
		}

		#endregion
		#region Related Business Objects
		public void TestNewedOrgHeaders()
		{
			Consol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			AssertNotNull(Consol.SendingForwarder);
			Consol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			AssertNotNull(Consol.ReceivingForwarder);
			Consol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			AssertNotNull(Consol.ShippingLine);
		}

		public void TestConsolShipmentCollection()
		{
			AssertEquals(typeof(JASForwardingConsolShipmentCollection), Consol.Shipments.GetType());
		}

		public void TestInheritedConsolSynchronisation() //Test is not specific to JAS. A Defect was reported which occured for any ZClient haviing their own subclass of ForwardingConsol, no functional cahnge - This is just about moving a unit test to a different assembly. 
		{
			AssertNoExceptionThrown("Consol synchroniser should be instantiated", delegate
			{
				var inheritedConsol = Factory.New<JASForwardingConsol>();
				var nctsHeader = Factory.New<Integration.Customs.EU.NCTS.ICusInBondHeader>();
				nctsHeader.SetMovementType("D");
				nctsHeader.BH_ParentID = inheritedConsol.PK;
				nctsHeader.BH_ParentTableCode = inheritedConsol.TablePrefix;
				nctsHeader.Synchronise(true);
			});
		}

		#endregion
		#region class InvalidDomainValidation
		public class InvalidDomainValidation : ZValidation
		{
			public InvalidDomainValidation(Transport transport) : base(transport)
			{
			}

			public InvalidDomainValidation(ForwardingConsol consol) : base(consol)
			{
			}

			public override Type AutoValidationType
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public override void ValidateAll()
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		#endregion
		#region IJXCExportHeader Tests
		public void TestDestOfficeAndNettingCode()
		{
			AssertEquals("Pre-condition", "", Consol.DestOfficeCode);
			AssertEquals("Pre-condition", "", Consol.DestNettingCode);
			SetOfficeCode();
			SetNettingCode();
			Consol.SetDefaultReceivingForwarderAddress(TestOrg);
			AssertEquals("ORG123", Consol.DestOfficeCode);
			AssertEquals("NET123", Consol.DestNettingCode);
		}

		public void TestSendingOfficeAndNettingCode()
		{
			AssertEquals("Pre-condition", "", Consol.SendingOfficeCode);
			AssertEquals("Pre-condition", "", Consol.SendingNettingCode);
			SetOfficeCode();
			SetNettingCode();
			Consol.SetDefaultSendingForwarderAddress(TestOrg);
			AssertEquals("ORG123", Consol.SendingOfficeCode);
			AssertEquals("NET123", Consol.SendingNettingCode);
		}

		public void TestFreightDest()
		{
			IJXCExportHeader header = Consol;
			AssertEquals("", header.FreightDest);
			Consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("AUSYD", header.FreightDest);
			Consol.JK_RL_NKDischargePort = "AUBNE";
			AssertEquals("AUBNE", header.FreightDest);
		}

		#endregion
		#region IJXCImportHeader Tests
		public void TestSetFreightDestination()
		{
			Consol.SetFreightDestination("AUBNE");
			AssertEquals("AUBNE", Consol.JK_RL_NKDischargePort);
		}

		public void TestSetFreightDestination_TrimmedWhenExceedingMaxLength()
		{
			Consol.SetFreightDestination("THIS_IS_WAY_TOOO_LONG");
			AssertEquals("Should be trimmed", "THIS_", Consol.JK_RL_NKDischargePort);
		}

		public void TestSetDestinationForwarder()
		{
			JASOrgHeader newOrg = Factory.NewWithValidTestData<JASOrgHeader>();
			Consol.SetDefaultReceivingForwarderAddress(newOrg);
			Factory.Save();
			Consol.SetDestinationForwarder("ORG123", "NET123");
			AssertEquals("Should not be changed, organisation could not be matched", newOrg.PK, Consol.ReceivingForwarderPK);
			SetNettingCode();
			Factory.Save();
			Consol.SetDestinationForwarder("ORG123", "NET123");
			AssertEquals("Should not be changed, only matching on netting code is not enough", newOrg.PK, Consol.ReceivingForwarderPK);
			SetOfficeCode();
			Factory.Save();
			Consol.SetDestinationForwarder("ORG123", "NET123");
			AssertEquals("Should be changed now", TestOrg.PK, Consol.ReceivingForwarderPK);
		}

		public void TestSetSendingForwarder()
		{
			JASOrgHeader newOrg = Factory.NewWithValidTestData<JASOrgHeader>();
			Consol.SetDefaultSendingForwarderAddress(newOrg);
			Factory.Save();
			Consol.SetSendingForwarder("ORG123", "NET123");
			AssertEquals("Should not be changed, organisation could not be matched", newOrg.PK, Consol.SendingForwarderPK);
			SetNettingCode();
			Factory.Save();
			Consol.SetSendingForwarder("ORG123", "NET123");
			AssertEquals("Should not be changed, only matching on netting code is not enough", newOrg.PK, Consol.SendingForwarderPK);
			SetOfficeCode();
			Factory.Save();
			Consol.SetSendingForwarder("ORG123", "NET123");
			AssertEquals("Should be changed now", TestOrg.PK, Consol.SendingForwarderPK);
		}

		#endregion
		#region JXC Air Or Ocean Message Export
		[TestDate(2006, 1, 1)]
		public void TestUpdateAWBPrinted()
		{
			try
			{
				SetupExportDir();
				Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_MasterBillNum = "081a";
				Consol.UpdateAWBPrinted();
				AssertExportedFile("a.081", JXCConstants.LineTypes.MAWB);
				StmALog lastLog = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_PostedTimeUtc, ZDateTime.Now));
				AssertEquals("MAWB Printed", ForwardingConsol.MAWBPrintedLogReference, lastLog.SL_Reference);
				AssertEquals("FinalMAWBPrintedDate", ZDateTime.Today, Consol.FinalMAWBPrintedDate);
			}
			finally
			{
				ResetExportDir();
			}
		}

		public void TestExportJXCAirOrOceanMessage()
		{
			try
			{
				SetupExportDir();
				Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				Consol.JK_MasterBillNum = "081a";
				Consol.ExportJXCAirOrOceanMessage();
				AssertExportedFile("a.081", JXCConstants.LineTypes.MAWB);
				Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				Consol.JK_MasterBillNum = "081a";
				Consol.ExportJXCAirOrOceanMessage();
				AssertExportedFile("081a", JXCConstants.LineTypes.OMAN);
			}
			finally
			{
				ResetExportDir();
			}
		}

		void AssertExportedFile(string expectedFileName, string expectedFirstBodyLineType)
		{
			string exportedFile = Path.Combine(JASDataRegistry.Instance.JXCOutgoingDirectoryName, expectedFileName);
			Assert("File should be successfully exported", File.Exists(exportedFile));
			using (StreamReader reader = File.OpenText(exportedFile))
			{
				reader.ReadLine();
				Assert("Line should start with '" + expectedFirstBodyLineType + "'", reader.ReadLine().StartsWith(expectedFirstBodyLineType));
			}
		}

		void SetupExportDir()
		{
			JXCOutgoingDirName = JASDataRegistry.Instance.JXCOutgoingDirectoryName;
			string testPath = Path.Combine(Env.TempPath, "___JXCOUTGOINGTESTDIR");
			JASDataRegistry.Instance.JXCOutgoingDirectoryName = testPath;
		}

		void ResetExportDir()
		{
			TempDirectory.DeleteDirectory(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
			JASDataRegistry.Instance.JXCOutgoingDirectoryName = JXCOutgoingDirName;
		}

		string JXCOutgoingDirName;
		#endregion
		#region Implementation
		void SetOfficeCode()
		{
			OrgCusCode cusCode = TestOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.UniversalOfficeCode;
			cusCode.OK_CustomsRegNo = "ORG123";
			AssertEquals("ORG123", TestOrg.CustomsCodes.GetUOC());
		}

		void SetNettingCode()
		{
			OrgCusCode cusCode = TestOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.UniversalNettingCode;
			cusCode.OK_CustomsRegNo = "NET123";
			AssertEquals("NET123", TestOrg.CustomsCodes.GetUNC());
		}

		protected JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
				}

				return fConsol;
			}
		}

		OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>();
					fTestOrg.OH_Code = "ORG123";
				}

				return fTestOrg;
			}
		}

		OrgHeader fTestOrg;
		JASForwardingConsol fConsol;
		#endregion
	}
}
