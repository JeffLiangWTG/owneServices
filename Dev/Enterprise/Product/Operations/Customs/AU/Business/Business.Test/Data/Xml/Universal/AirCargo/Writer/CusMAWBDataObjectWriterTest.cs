using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalConstants = Enterprise.Customs.DataTransfer.Universal.Constants;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestCusMAWBMappings()
		{
			var mawb = SetupCusMAWB(Factory.BOFactory, Factory.New<CusMAWB>(), "MB324242", "CL32423", AirForeignPort1.RL_Code, AirLocalPort1.RL_Code, AirLocalPort2.RL_Code);
			mawb.Notes.AddNew(ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			mawb.Notes.AddNew(ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "HB24";
			hawb1.CS_IsSelfAssessedClearance = ZBool.True;
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB89";
			hawb2.CS_IsSelfAssessedClearance = ZBool.True;
			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "SB243";
			hawb3.CS_MasterHouseBill = "HB89";
			hawb3.CS_CS_MasterHouseBill = hawb2.PK;
			Factory.SaveForTesting();
			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertHVLVManifestContents(mawbData, "MB324242", "CL32423");
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			var hawbData1 = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
			AssertEquals("hawbData1.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData1.MessageSubType.GetCodeAsUpperCase());
			var hawbData2 = mawbData.SubShipmentCollection[1];
			AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
			AssertEquals("hawbData2.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData2.MessageSubType.GetCodeAsUpperCase());

			writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertHVLVManifestContents(mawbData, "MB324242", "CL32423");
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			hawbData1 = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
			AssertEquals("hawbData1.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData1.MessageSubType.GetCodeAsUpperCase());
			hawbData2 = mawbData.SubShipmentCollection[1];
			AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
			AssertEquals("hawbData2.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData2.MessageSubType.GetCodeAsUpperCase());

			mawb.CM_MasterHouseBill = ZString.Empty;
			Factory.SaveForTesting();
			writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertHVLVManifestContents(mawbData, "MB324242", null);
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			hawbData1 = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
			AssertEquals("hawbData1.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData1.MessageSubType.GetCodeAsUpperCase());
			hawbData2 = mawbData.SubShipmentCollection[1];
			AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
			AssertEquals("hawbData2.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData2.MessageSubType.GetCodeAsUpperCase());
		}

		public void TestExportData_SubShipment()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB1";
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "HB1";
			var subHawb1 = mawb.ChildBills.AddNew();
			subHawb1.CS_HAWB = "SB1";
			subHawb1.CS_MasterHouseBill = "HB1";
			subHawb1.CS_CS_MasterHouseBill = hawb1.PK;
			var subHawb2 = mawb.ChildBills.AddNew();
			subHawb2.CS_HAWB = "SB2";
			subHawb2.CS_MasterHouseBill = "HB1";
			subHawb2.CS_CS_MasterHouseBill = hawb1.PK;
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB2";
			var subHawb3 = mawb.ChildBills.AddNew();
			subHawb3.CS_HAWB = "SB3";
			subHawb3.CS_MasterHouseBill = "HB2";
			subHawb3.CS_CS_MasterHouseBill = hawb2.PK;
			var manager = (IShipmentDataContextManager)hawb1.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, hawb1)));
			var mawbData = (Shipment)writer.GetDataObject(hawb1);
			var sources = mawbData.DataContext.DataSourceCollection.ToArray();
			AssertEquals("mawbData.DataContext.DataSourceCollection.Count", 2, sources.Length);
			AssertEquals(nameof(DataContextType.AirManifest), sources[0].Type);
			AssertEquals(nameof(DataContextType.AirManifestLine), sources[1].Type);
			AssertEquals("mawbData.WayBillNumber", "MB1", mawbData.WayBillNumber);
			AssertEquals("mawbData.SubShipmentCollection.Count", 1, mawbData.SubShipmentCollection.Count);
			var hawb1Data = mawbData.SubShipmentCollection[0];
			AssertEquals("hawb1Data.WayBillNumber", "HB1", hawb1Data.WayBillNumber);
			AssertEquals("hawb1Data.SubShipmentCollection.Count", 2, hawb1Data.SubShipmentCollection.Count);
			var subHawb1Data = hawb1Data.SubShipmentCollection[0];
			AssertEquals("subHawb1Data.WayBillNumber", "SB1", subHawb1Data.WayBillNumber);
			AssertEquals("subHawb1Data.AdditionalBillCollection.Count", 1, subHawb1Data.AdditionalBillCollection.Count);
			var subHawb1BillData = subHawb1Data.AdditionalBillCollection[0];
			AssertEquals("subHawb1BillData.ParentBillNumber", "HB1", subHawb1BillData.ParentBillNumber);
			var subHawb2Data = hawb1Data.SubShipmentCollection[1];
			AssertEquals("subHawb2Data.WayBillNumber", "SB2", subHawb2Data.WayBillNumber);
			AssertEquals("subHawb2Data.AdditionalBillCollection.Count", 1, subHawb2Data.AdditionalBillCollection.Count);
			var subHawb2BillData = subHawb2Data.AdditionalBillCollection[0];
			AssertEquals("subHawb2BillData.ParentBillNumber", "HB1", subHawb2BillData.ParentBillNumber);

			manager = (IShipmentDataContextManager)subHawb2.GetUniversalDataContextManager();
			writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, subHawb2)));
			mawbData = (Shipment)writer.GetDataObject(subHawb2);
			sources = mawbData.DataContext.DataSourceCollection.ToArray();
			AssertEquals("mawbData.DataContext.DataSourceCollection.Count", 2, sources.Length);
			AssertEquals(nameof(DataContextType.AirManifest), sources[0].Type);
			AssertEquals(nameof(DataContextType.AirManifestLine), sources[1].Type);
			AssertEquals("mawbData.WayBillNumber", "MB1", mawbData.WayBillNumber);
			AssertEquals("mawbData.SubShipmentCollection.Count", 1, mawbData.SubShipmentCollection.Count);
			hawb1Data = mawbData.SubShipmentCollection[0];
			AssertEquals("hawb1Data.WayBillNumber", "HB1", hawb1Data.WayBillNumber);
			AssertEquals("hawb1Data.SubShipmentCollection.Count", 1, hawb1Data.SubShipmentCollection.Count);
			subHawb2Data = hawb1Data.SubShipmentCollection[0];
			AssertEquals("subHawb2Data.WayBillNumber", "SB2", subHawb2Data.WayBillNumber);
			AssertEquals("subHawb2Data.AdditionalBillCollection.Count", 1, subHawb2Data.AdditionalBillCollection.Count);
			subHawb2BillData = subHawb2Data.AdditionalBillCollection[0];
			AssertEquals("subHawb2BillData.ParentBillNumber", "HB1", subHawb2BillData.ParentBillNumber);

			manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			mawbData = (Shipment)writer.GetDataObject(mawb);
			sources = mawbData.DataContext.DataSourceCollection.ToArray();
			AssertEquals("mawbData.DataContext.DataSourceCollection.Count", 1, sources.Length);
			AssertEquals(nameof(DataContextType.AirManifest), sources[0].Type);
			AssertEquals("mawbData.WayBillNumber", "MB1", mawbData.WayBillNumber);
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			hawb1Data = mawbData.SubShipmentCollection[0];
			AssertEquals("hawb1Data.WayBillNumber", "HB1", hawb1Data.WayBillNumber);
			AssertEquals("hawb1Data.SubShipmentCollection.Count", 2, hawb1Data.SubShipmentCollection.Count);
			subHawb1Data = hawb1Data.SubShipmentCollection[0];
			AssertEquals("subHawb1Data.WayBillNumber", "SB1", subHawb1Data.WayBillNumber);
			AssertEquals("subHawb1Data.AdditionalBillCollection.Count", 1, subHawb1Data.AdditionalBillCollection.Count);
			subHawb1BillData = subHawb1Data.AdditionalBillCollection[0];
			AssertEquals("subHawb1BillData.ParentBillNumber", "HB1", subHawb1BillData.ParentBillNumber);
			subHawb2Data = hawb1Data.SubShipmentCollection[1];
			AssertEquals("subHawb2Data.WayBillNumber", "SB2", subHawb2Data.WayBillNumber);
			AssertEquals("subHawb2Data.AdditionalBillCollection.Count", 1, subHawb2Data.AdditionalBillCollection.Count);
			subHawb2BillData = subHawb2Data.AdditionalBillCollection[0];
			AssertEquals("subHawb2BillData.ParentBillNumber", "HB1", subHawb2BillData.ParentBillNumber);
			var hawb2Data = mawbData.SubShipmentCollection[1];
			AssertEquals("hawb2Data.WayBillNumber", "HB2", hawb2Data.WayBillNumber);
			AssertEquals("hawb2Data.SubShipmentCollection.Count", 1, hawb2Data.SubShipmentCollection.Count);
			var subhawb3Data = hawb2Data.SubShipmentCollection[0];
			AssertEquals("subhawb3Data.WayBillNumber", "SB3", subhawb3Data.WayBillNumber);
			AssertEquals("subhawb3Data.AdditionalBillCollection.Count", 1, subhawb3Data.AdditionalBillCollection.Count);
			var subhawb3BillData = subhawb3Data.AdditionalBillCollection[0];
			AssertEquals("subhawb2BillData.ParentBillNumber", "HB2", subhawb3BillData.ParentBillNumber);
		}

		public void TestCusHAWBDataObjectWriterForCTOCusMawb()
		{
			// CTOCusMawb and CusMAWB in AU both derive from base CusMAWB
			// CusHAWBDataObjectWriter requires a MAWB of type base CusMAWB but was paramatized here with the AU CusMAWB so was crashing when the AU CTOCusMAWB was being passed in.
			// The change needed the base CusMAWB to be fully qualified in the class/method so that both types of AU mawbs could be accessed by the data writer.
			var mawb = SetupCTOCusMAWB(Factory.New<CTOCusMAWB>(), "MB324242", "CL32423");
			mawb.Notes.AddNew(ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			mawb.Notes.AddNew(ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "HB24";
			hawb1.CS_IsSelfAssessedClearance = ZBool.True;
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB89";
			hawb2.CS_IsSelfAssessedClearance = ZBool.True;
			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "SB243";
			hawb3.CS_MasterHouseBill = "HB89";
			hawb3.CS_CS_MasterHouseBill = hawb2.PK;
			Factory.SaveForTesting();
			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertHVLVManifestContents(mawbData, "MB324242", "CL32423");
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			var hawbData1 = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
			AssertEquals("hawbData1.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData1.MessageSubType.GetCodeAsUpperCase());
			var hawbData2 = mawbData.SubShipmentCollection[1];
			AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
			AssertEquals("hawbData2.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData2.MessageSubType.GetCodeAsUpperCase());

			writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertHVLVManifestContents(mawbData, "MB324242", "CL32423");
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			hawbData1 = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
			AssertEquals("hawbData1.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData1.MessageSubType.GetCodeAsUpperCase());
			hawbData2 = mawbData.SubShipmentCollection[1];
			AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
			AssertEquals("hawbData2.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData2.MessageSubType.GetCodeAsUpperCase());

			mawb.CM_MasterHouseBill = ZString.Empty;
			Factory.SaveForTesting();
			writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertHVLVManifestContents(mawbData, "MB324242", null);
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			hawbData1 = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
			AssertEquals("hawbData1.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData1.MessageSubType.GetCodeAsUpperCase());
			hawbData2 = mawbData.SubShipmentCollection[1];
			AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
			AssertEquals("hawbData2.MessageSubType", Business.JobDeclaration.MessageSubType.SelfAssessedClearance, hawbData2.MessageSubType.GetCodeAsUpperCase());
		}

		internal static CusMAWB SetupCusMAWB(BusinessObjectFactory factory, CusMAWB mawb, ZString wayBillNumber, ZString coloadBillNumber, ZString airForeignPortCode, ZString airLocalPort1, ZString airLocalPort2)
		{
			var responsibleParty = GetOrganizationBO_WUFSHIJNB(factory);
			mawb.CM_GB = GlbBranch.CurrentBranch.PK;
			mawb.CM_MAWB = wayBillNumber;
			mawb.CM_MasterHouseBill = coloadBillNumber;
			mawb.CM_FlightNo = "QF123";
			mawb.CM_Folio = "FL324";
			mawb.CM_OH_ResponsibleParty = responsibleParty.PK;
			mawb.CM_ResponsiblePartyID = "RPI2343";
			mawb.CM_RL_NKLoadPort = airForeignPortCode;
			mawb.CM_RL_NKFirstArrivalPort = airLocalPort1;
			mawb.CM_RL_NKDischargePort = airLocalPort2;
			mawb.CM_DepartureDate = new ZDateTime(2012, 6, 4);
			mawb.CM_DateOfFirstArrival = new ZDateTime(2012, 6, 5);
			mawb.CM_ArrivalDate = new ZDateTime(2012, 6, 6);
			return mawb;
		}

		internal static void AssertContents(AdditionalReference additionalReferenceDataObject, ZString referenceNumber, ICodeDescription type)
		{
			AssertNotNull("Precondition: additionalReferenceDataObject", additionalReferenceDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("additionalReferenceDataObject.ReferenceNumber", referenceNumber, additionalReferenceDataObject.ReferenceNumber);
				AssertNotNull("additionalReferenceDataObject.Type", additionalReferenceDataObject.Type);
				AssertEquals("additionalReferenceDataObject.Type.Code", type.Code, additionalReferenceDataObject.Type.Code);
				AssertEquals("additionalReferenceDataObject.Type.Description", type.Description, additionalReferenceDataObject.Type.Description);
			});
		}

		RefUNLOCO AirLocalPort1
		{
			get
			{
				if (airLocalPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort1;
			}
		}
		RefUNLOCO airLocalPort1;

		RefUNLOCO AirLocalPort2
		{
			get
			{
				if (airLocalPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, AirLocalPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort2;
			}
		}
		RefUNLOCO airLocalPort2;

		RefUNLOCO AirForeignPort1
		{
			get
			{
				if (airForeignPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airForeignPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airForeignPort1;
			}
		}
		RefUNLOCO airForeignPort1;

		internal static void AssertContents(Date dateDataObject, DateType type, ZDateTime dateTime, ZBool isEstimate)
		{
			AssertNotNull("Precondition: dateDataObject", dateDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("dateDataObject.Type", type, dateDataObject.Type);
				AssertEquals("dateDataObject.Value", dateTime, dateDataObject.Value);
				AssertEquals("dateDataObject.IsEstimate", isEstimate, dateDataObject.IsEstimate);
			});
		}

		void AssertContents(Note noteData, ZBool isCustomDescription, ZString description, ZString noteText)
		{
			AssertNotNull("Precondition: noteData", noteData);
			CombineAssertions(delegate
			{
				AssertEquals("noteData.IsCustomDescription", isCustomDescription, noteData.IsCustomDescription);
				AssertEquals("noteData.Description", description, noteData.Description);
				AssertEquals("noteData.NoteText", noteText, noteData.NoteText);
			});
		}

		CTOCusMAWB SetupCTOCusMAWB(CTOCusMAWB mawb, ZString wayBillNumber, ZString coloadBillNumber)
		{
			var responsibleParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			mawb.CM_GB = GlbBranch.CurrentBranch.PK;
			mawb.CM_MAWB = wayBillNumber;
			mawb.CM_MasterHouseBill = coloadBillNumber;
			mawb.CM_FlightNo = "QF123";
			mawb.CM_Folio = "FL324";
			mawb.CM_OH_ResponsibleParty = responsibleParty.PK;
			mawb.CM_ResponsiblePartyID = "RPI2343";
			mawb.CM_RL_NKLoadPort = AirForeignPort1.RL_Code;
			mawb.CM_RL_NKFirstArrivalPort = AirLocalPort1.RL_Code;
			mawb.CM_RL_NKDischargePort = AirLocalPort2.RL_Code;
			mawb.CM_DepartureDate = new ZDateTime(2012, 6, 4);
			mawb.CM_DateOfFirstArrival = new ZDateTime(2012, 6, 5);
			mawb.CM_ArrivalDate = new ZDateTime(2012, 6, 6);
			return mawb;
		}

		void AssertHVLVManifestContents(Shipment mawbData, ZString? wayBillNumber, ZString? coloadBillNumber)
		{
			AssertAirCargoMasterContents(mawbData, wayBillNumber, coloadBillNumber, "QF123", "FL324", CodeDescriptionPairForTesting.New(AirForeignPort1.RL_Code, AirForeignPort1.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort2.RL_Code, AirLocalPort2.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort1.RL_Code, AirLocalPort1.RL_PortName), CodeDescriptionPairForTesting.New(GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName));
			AssertNotNull("mawbData.DateCollection", mawbData.DateCollection);
			AssertEquals("mawbData.DateCollection.Count", 3, mawbData.DateCollection.Count);
			AssertContents(mawbData.DateCollection[0], DateType.LoadingDate, new ZDateTime(2012, 6, 4), ZBool.False);
			AssertContents(mawbData.DateCollection[1], DateType.FirstArrivalInCountry, new ZDateTime(2012, 6, 5), ZBool.False);
			AssertContents(mawbData.DateCollection[2], DateType.DischargeDate, new ZDateTime(2012, 6, 6), ZBool.False);
			AssertEquals("mawbData.OrganizationAddressCollection.Count", 1, mawbData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("ResponsibleParty", mawbData.OrganizationAddressCollection[0], AddressTypes.ResponsibleParty);
			AssertEquals("mawbData.AdditionalReferenceCollection.Count", 1, mawbData.AdditionalReferenceCollection.Count);
			AssertContents(mawbData.AdditionalReferenceCollection[0], "RPI2343", CodeDescriptionPairForTesting.New(UniversalConstants.AdditionalReference.EntryType.Codes.ResponsiblePartyID, UniversalConstants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID));
			AssertEquals("mawbData.NoteCollection.Count", 2, mawbData.NoteCollection.Count);
			AssertContents(mawbData.NoteCollection[0], ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			AssertContents(mawbData.NoteCollection[1], ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
		}

		void AssertAirCargoMasterContents(Shipment mawbData, ZString? wayBillNumber, ZString? coloadBillNumber, ZString? flight, ZString? folio, ICodeDescription portOfLoading, ICodeDescription portOfDischarge, ICodeDescription portOfFirstArrival, ICodeDescription branch)
		{
			AssertNotNull("Precondition: mawbData", mawbData);

			CombineAssertions(delegate
			{
				AssertEquals("mawbData.WayBillNumber", wayBillNumber, mawbData.WayBillNumber);
				AssertNotNull("mawbData.WayBillType", mawbData.WayBillType);
				AssertEquals("mawbData.WayBillType.Code", WayBillTypeList.Codes.Master, mawbData.WayBillType.Code);
				AssertEquals("mawbData.WayBillType.Description", WayBillTypeList.Descriptions.Master, mawbData.WayBillType.Description);
				if (coloadBillNumber.HasValue)
				{
					AssertNotNull("mawbData.AdditionalBillCollection", mawbData.AdditionalBillCollection);
					AssertNotNull("mawbData.AdditionalBillCollection should contain masterhouse", mawbData.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber == coloadBillNumber && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.MasterHouse && x.ParentBillNumber == wayBillNumber));
				}
				else if (mawbData.AdditionalBillCollection != null)
				{
					AssertNull("mawbData.AdditionalBillCollection should not contain masterhouse", mawbData.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber == coloadBillNumber && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.MasterHouse && x.ParentBillNumber == wayBillNumber));
				}

				AssertEquals("mawbData.VoyageFlightNo", flight, mawbData.VoyageFlightNo);
				AssertEquals("mawbData.Folio", folio, mawbData.Folio);
				AssertNotNull("mawbData.PortOfLoading", mawbData.PortOfLoading);
				AssertEquals("mawbData.PortOfLoading.Code", portOfLoading.Code, mawbData.PortOfLoading.Code);
				AssertEquals("mawbData.PortOfLoading.Name", portOfLoading.Description, mawbData.PortOfLoading.Name);
				AssertNotNull("mawbData.PortOfDischarge", mawbData.PortOfDischarge);
				AssertEquals("mawbData.PortOfDischarge.Code", portOfDischarge.Code, mawbData.PortOfDischarge.Code);
				AssertEquals("mawbData.PortOfDischarge.Name", portOfDischarge.Description, mawbData.PortOfDischarge.Name);
				AssertNotNull("mawbData.PortOfFirstArrival", mawbData.PortOfFirstArrival);
				AssertEquals("mawbData.PortOfFirstArrival.Code", portOfFirstArrival.Code, mawbData.PortOfFirstArrival.Code);
				AssertEquals("mawbData.PortOfFirstArrival.Name", portOfFirstArrival.Description, mawbData.PortOfFirstArrival.Name);
				AssertNotNull("mawbData.Branch", mawbData.Branch);
				AssertEquals("mawbData.Branch.Code", branch.Code, mawbData.Branch.Code);
				AssertEquals("mawbData.Branch.Name", branch.Description, mawbData.Branch.Name);
			});
		}
	}
}
