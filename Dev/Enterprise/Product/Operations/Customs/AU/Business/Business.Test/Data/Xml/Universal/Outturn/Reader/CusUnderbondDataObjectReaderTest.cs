using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UEntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;
using UEntryType = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryType;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusUnderbondDataObjectReaderTest : DataTransfer.Universal.Testing.DataObjectReaderTestHelper
	{
		public void TestLogOutturnsReadyForSendingEvent_OutturnsExist() => CombineAssertions(() =>
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "OB1";
			var houseBill = mawb.FilteredChildBills.AddNew();

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentID = mawb.PK;
			underbond.C4_ParentTableCode = mawb.TablePrefix;

			var outturn = underbond.Outturns.AddNew();
			outturn.C5_HouseBill = "OB1";
			outturn.C5_ParentID = houseBill.PK;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Error message is empty, event AUT added", true, mawb.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == Events.AUOutturnReadyForSendingCode));
			AssertContains("Log information", "Information - Added event 'AUT' for Air Cargo OB1.", logger.Logs);
		});

		public void TestLogOutturnsReadyForSendingEvent_OutturnsDontExist() => CombineAssertions(() =>
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "OB1";
			var houseBill = mawb.FilteredChildBills.AddNew();
			houseBill.CS_HAWB = "HB1";

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentID = mawb.PK;
			underbond.C4_ParentTableCode = mawb.TablePrefix;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};
			shipment.SetSubShipmentCollection(() => new DataObjectList<UShipment>
			{
				new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "HB1",
					WayBillType = new WayBillType
					{
						Code = WayBillTypeList.Codes.House,
						Description = WayBillTypeList.Descriptions.House
					}
				}
			});

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertEquals("Error message is empty, event AUT added", true, mawb.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == Events.AUOutturnReadyForSendingCode));
			AssertContains("Log information", "Information - Added event 'AUT' for Air Cargo OB1.", logger.Logs);
		});

		public void TestDichargeAddress_WhenHasMatchedNewDischargeAddress()
		{
			var dischargeAddress1 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			dischargeAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DI123", Core.Constants.CountryCodes.Australia);

			var dischargeAddress2 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			dischargeAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "OR123", Core.Constants.CountryCodes.Australia);

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			underbond.C4_OA_DischargeAddress = dischargeAddress1.MainAddress.PK;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			shipment.AddOrgAddress(writeManager, dischargeAddress2, Outturn.Constants.AddressType.DischargeAddress);
			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond.PK, testUnderbond.PK);
			AssertEquals("underbond.C4_OA_DischargeAddress", dischargeAddress2.MainAddress.PK, testUnderbond.C4_OA_DischargeAddress);
		}

		public void TestDichargeAddress_WhenHasNoNewDischargeAddress_ExpectEmpty()
		{
			var dischargeAddress = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			dischargeAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DI123", Core.Constants.CountryCodes.Australia);

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			underbond.C4_OA_DischargeAddress = dischargeAddress.MainAddress.PK;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};

			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond.PK, testUnderbond.PK);
			AssertEquals("underbond.C4_OA_DischargeAddress", dischargeAddress.MainAddress.PK, testUnderbond.C4_OA_DischargeAddress);
		}

		public void TestDichargeAddress_WhenHasUnmatchedNewDischargeAddress_ExpectEmpty()
		{
			var dischargeAddress1 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			dischargeAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DI123", Core.Constants.CountryCodes.Australia);

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			underbond.C4_OA_DischargeAddress = dischargeAddress1.MainAddress.PK;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				},
			};
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = Outturn.Constants.AddressType.DischargeAddress
					}
				});

			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond.PK, testUnderbond.PK);
			AssertEquals("underbond.C4_OA_DischargeAddress", ZGuid.Empty, testUnderbond.C4_OA_DischargeAddress);
		}

		public void TestC4_DischargePremiseID_WhenHasMatchedNewDischargeAddressAndHasNoNewDishcargePremise()
		{
			var dischargeAddress1 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			dischargeAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DI123", Core.Constants.CountryCodes.Australia);

			var dischargeAddress2 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			dischargeAddress2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "OR123", Core.Constants.CountryCodes.Australia);

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			underbond.C4_OA_DischargeAddress = dischargeAddress1.MainAddress.PK;
			underbond.C4_DischargePremiseID = "XX123";
			AssertEquals("underbond.C4_DischargePremiseID", "DI123", underbond.C4_DischargePremiseID);
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var orgAddress = shipment.AddOrgAddress(writeManager, dischargeAddress2, Outturn.Constants.AddressType.DischargeAddress);
			orgAddress.AddressOverride = true;

			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond.PK, testUnderbond.PK);
			AssertEquals("underbond.C4_OA_DischargeAddress", dischargeAddress2.MainAddress.PK, testUnderbond.C4_OA_DischargeAddress);
			AssertEquals("underbond.C4_DischargePremiseID", "OR123", testUnderbond.C4_DischargePremiseID);
		}

		public void TestC4_DischargePremiseID_WhenHasUnmatchedNewDischargeAddressAndHasNewDishcargePremise()
		{
			var dischargeAddress1 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			dischargeAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DI123", Core.Constants.CountryCodes.Australia);

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			underbond.C4_OA_DischargeAddress = dischargeAddress1.MainAddress.PK;
			underbond.C4_DischargePremiseID = "XX123";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				},
			};
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new UEntryType
						{
							Code = Outturn.Constants.AdditionalReference.EntryType.Codes.DischargePremiseID,
							Description = Outturn.Constants.AdditionalReference.EntryType.Descriptions.DischargePremiseID
						},
						ContextInformation = Core.Constants.CountryCodes.Australia,
						ReferenceNumber = "OR123"
					}
				});
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = Outturn.Constants.AddressType.DischargeAddress
					}
				});

			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond.PK, testUnderbond.PK);
			AssertEquals("underbond.C4_OA_DischargeAddress", ZGuid.Empty, testUnderbond.C4_OA_DischargeAddress);
			AssertEquals("underbond.C4_DischargePremiseID", "OR123", testUnderbond.C4_DischargePremiseID);
		}

		public void TestC4_DischargePremiseID_WhenHasUnmatchedNewDischargeAddressAndHasNoNewDishcargePremise()
		{
			var dischargeAddress1 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			dischargeAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DI123", Core.Constants.CountryCodes.Australia);

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			underbond.C4_OA_DischargeAddress = dischargeAddress1.MainAddress.PK;
			underbond.C4_DischargePremiseID = "XX123";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				},
			};
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = Outturn.Constants.AddressType.DischargeAddress
					}
				});

			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond.PK, testUnderbond.PK);
			AssertEquals("underbond.C4_OA_DischargeAddress", ZGuid.Empty, testUnderbond.C4_OA_DischargeAddress);
			AssertEquals("underbond.C4_DischargePremiseID", "XX123", testUnderbond.C4_DischargePremiseID);
		}

		public void TestC4_DischargePremiseID_WhenHasNoNewDischargeAddressAndHasNewDishcargePremise()
		{
			var dischargeAddress1 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			dischargeAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DI123", Core.Constants.CountryCodes.Australia);

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			underbond.C4_OA_DischargeAddress = dischargeAddress1.MainAddress.PK;
			underbond.C4_DischargePremiseID = "XX123";
			AssertEquals("underbond.C4_DischargePremiseID", "DI123", underbond.C4_DischargePremiseID);
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				},
			};
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new UEntryType
						{
							Code = Outturn.Constants.AdditionalReference.EntryType.Codes.DischargePremiseID,
							Description = Outturn.Constants.AdditionalReference.EntryType.Descriptions.DischargePremiseID
						},
						ContextInformation = Core.Constants.CountryCodes.Australia,
						ReferenceNumber = "OR123"
					}
				});

			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond.PK, testUnderbond.PK);
			AssertEquals("underbond.C4_OA_DischargeAddress", ZGuid.Empty, testUnderbond.C4_OA_DischargeAddress);
			AssertEquals("underbond.C4_DischargePremiseID", "OR123", testUnderbond.C4_DischargePremiseID);
		}

		public void TestC4_DischargePremiseID_WhenHasNoNewDischargeAddressAndHasNoNewDishcargePremise()
		{
			var dischargeAddress1 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			dischargeAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DI123", Core.Constants.CountryCodes.Australia);

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			underbond.C4_OA_DischargeAddress = dischargeAddress1.MainAddress.PK;
			underbond.C4_DischargePremiseID = "XX123";
			AssertEquals("underbond.C4_DischargePremiseID", "DI123", underbond.C4_DischargePremiseID);
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};

			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond.PK, testUnderbond.PK);
			AssertEquals("underbond.C4_OA_DischargeAddress", dischargeAddress1.MainAddress.PK, testUnderbond.C4_OA_DischargeAddress);
			AssertEquals("underbond.C4_DischargePremiseID", "DI123", testUnderbond.C4_DischargePremiseID);
		}

		public void TestMasterBillMatch_C4_MAWB()
		{
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond.PK, testUnderbond.PK);
		}

		public void TestMasterBillMatch_ParentID()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "OB1";

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentID = mawb.PK;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond.PK, testUnderbond.PK);
		}

		public void TestMasterBillMatch_MultipleMAWBS()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "OB1";

			var underbond1 = Factory.New<CusUnderbond>();
			underbond1.C4_ParentID = mawb.PK;
			underbond1.C4_SendersMessageReference = "ABC";

			var underbond2 = Factory.New<CusUnderbond>();
			underbond2.C4_MAWB = "OB1";
			underbond2.C4_SendersMessageReference = "ABD";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertEquals(underbond2.PK, testUnderbond.PK);
		}

		public void TestImportingData()
		{
			var aplVessel = Factory.New<RefVessel>();
			aplVessel.RV_Code = "APL VESSEL";
			aplVessel.RV_LloydsNumber = "9832343";
			aplVessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Jamaica;
			aplVessel.RV_RadioCallSign = "CALLME";

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				},
				VoyageFlightNo = "F123",
				TotalNoOfPieces = 9,
				TransportMode = new CodeDescriptionPair
				{
					Code = CMRUnderbondModeOfMovement.Codes.Road,
					Description = CMRUnderbondModeOfMovement.Descriptions.Road
				},
				ShipmentType = new CodeDescriptionPair
				{
					Code = CMRUnderbondRequestCodes.Codes.Transshipment,
					Description = CMRUnderbondRequestCodes.Descriptions.Transshipment
				},
				EntryStatus = new EntryStatus
				{
					Code = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived,
					Description = CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalAdviceReceived
				},
			};
			shipment.SetDateCollection(() => new List<Date>
				{
					{ DateType.Arrival, ZBool.False, new ZDateTime(2017, 10, 1) },
					{ DateType.Unpack, ZBool.False,  new ZDateTime(2017, 9, 1) }
				});
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new UEntryType
						{
							Code = Outturn.Constants.AdditionalReference.EntryType.Codes.DischargePremiseID,
							Description = Outturn.Constants.AdditionalReference.EntryType.Descriptions.DischargePremiseID
						},
						ContextInformation = Core.Constants.CountryCodes.Australia,
						ReferenceNumber = "DI123"
					},
					new AdditionalReference
					{
						Type = new UEntryType
						{
							Code = Outturn.Constants.AdditionalReference.EntryType.Codes.OriginPremiseID,
							Description = Outturn.Constants.AdditionalReference.EntryType.Descriptions.OriginPremiseID
						},
						ContextInformation = Core.Constants.CountryCodes.Australia,
						ReferenceNumber = "OR123"
					},
					new AdditionalReference
					{
						Type = new UEntryType
						{
							Code = Outturn.Constants.AdditionalReference.EntryType.Codes.DestinationPremiseID,
							Description = Outturn.Constants.AdditionalReference.EntryType.Descriptions.DestinationPremiseID
						},
						ContextInformation = Core.Constants.CountryCodes.Australia,
						ReferenceNumber = "DE123"
					},
					new AdditionalReference
					{
						Type = new UEntryType
						{
							Code = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
							Description = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID
						},
						ContextInformation = Core.Constants.CountryCodes.Australia,
						ReferenceNumber = "RE123"
					}
				});
			shipment.SetAddInfoCollection(() => new List<UAddInfo>
				{
					new UAddInfo
					{
						Key = Outturn.Constants.AddInfoType.UnderbondBySeaVoyage,
						Value =  "12345"
					},
					new UAddInfo
					{
						Key = Outturn.Constants.AddInfoType.IsMoveFromDischarge,
						Value =  Customs.Business.YesNoList.Codes.Yes
					},
					new UAddInfo
					{
						Key = Outturn.Constants.AddInfoType.UnderbondBySeaVessel,
						Value = aplVessel.RV_Code
					}
				});
			shipment.SetEntryNumberCollection(() => new List<UEntryNumber>
				{
					new UEntryNumber
					{
						Type = new UEntryType
						{
							Code = Outturn.Constants.EntryNumber.EntryType.Codes.UnderbondStatus,
							Description = Outturn.Constants.EntryNumber.EntryType.Descriptions.UnderbondStatus
						},
						CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.Australia },
						EntryStatus = new EntryStatus
						{
							Code = CMRBaseStatuses.Codes.NotSent,
							Description = CMRBaseStatuses.Descriptions.NotSent
						}
					}
				});

			var dischargeAddress = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var originAddress = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var destinationAddress = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);

			dischargeAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DI123", Core.Constants.CountryCodes.Australia);
			originAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "OR123", Core.Constants.CountryCodes.Australia);
			destinationAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DE123", Core.Constants.CountryCodes.Australia);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			shipment.AddOrgAddress(writeManager, dischargeAddress, Outturn.Constants.AddressType.DischargeAddress);
			shipment.AddOrgAddress(writeManager, originAddress, Outturn.Constants.AddressType.OriginAddress);
			shipment.AddOrgAddress(writeManager, destinationAddress, Outturn.Constants.AddressType.DestinationAddress);

			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			var underbond = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("underbond.C4_MAWB", "OB1", underbond.C4_MAWB);
				AssertEquals("underbond.C4_FlightNo", "F123", underbond.C4_FlightNo);
				AssertEquals("underbond.C4_PiecesManifested", 9, underbond.C4_PiecesManifested);
				AssertEquals("underbond.C4_ModeOfMovement", CMRUnderbondModeOfMovement.Codes.Road, underbond.C4_ModeOfMovement);
				AssertEquals("underbond.C4_MovementReason", CMRUnderbondRequestCodes.Codes.Transshipment, underbond.C4_MovementReason);
				AssertEquals("underbond.C4_ArrivalDate", new ZDateTime(2017, 10, 1), underbond.C4_ArrivalDate);
				AssertEquals("underbond.C4_Outurned", new ZDateTime(2017, 9, 1), underbond.C4_Outurned);
				AssertEquals("underbond.UnderbondStatus.Code", CMRBaseStatuses.Codes.NotSent, underbond.UnderbondStatus.Code);
				AssertEquals("underbond.C4_OA_DischargeAddress", dischargeAddress.MainAddress.PK, underbond.C4_OA_DischargeAddress);
				AssertEquals("underbond.C4_OA_OriginAddress", originAddress.MainAddress.PK, underbond.C4_OA_OriginAddress);
				AssertEquals("underbond.C4_OA_DestinationAddress", destinationAddress.MainAddress.PK, underbond.C4_OA_DestinationAddress);
				AssertEquals("underbond.C4_DischargePremiseID", "DI123", underbond.C4_DischargePremiseID);
				AssertEquals("underbond.C4_OriginPremiseID", "OR123", underbond.C4_OriginPremiseID);
				AssertEquals("underbond.C4_DestinationPremiseID", "DE123", underbond.C4_DestinationPremiseID);
				AssertEquals("underbond.C4_ResponsiblePartyID", "RE123", underbond.C4_ResponsiblePartyID);
				AssertEquals("underbond.C4_UnderbondBySeaVoyage", "12345", underbond.C4_UnderbondBySeaVoyage);
				AssertEquals("underbond.C4_IsMoveFromDischarge", ZBool.True, underbond.C4_IsMoveFromDischarge);
				AssertEquals("underbond.C4_UnderbondBySeaVessel", "APL VESSEL", underbond.C4_UnderbondBySeaVessel);
				AssertEquals("underbond.C4_Status", "", underbond.C4_Status);
			});
		}

		public void TestImportingData_AirManifest()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "OB1";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HB1";
			var underbond = mawb.Underbonds.AddNew();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				}
			};
			shipment.SetSubShipmentCollection(() => new DataObjectList<UShipment>
			{
				new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "HB1",
					WayBillType = new WayBillType
					{
						Code = WayBillTypeList.Codes.House,
						Description = WayBillTypeList.Descriptions.House
					}
				}
			});
			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			underbond = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				underbond.Outturns.Reload(true);
				var outturn = underbond.Outturns.Cast<CusOutturn>().Single();
				AssertEquals("outturn.C5_ParentID", hawb.PK, outturn.C5_ParentID);
				AssertEquals("outturn.C5_ParentTableCode", CusHAWBSchema.Constants.Prefix, outturn.C5_ParentTableCode);
			});
		}

		public void TestNotUpdateUnderbondWhenHaveSuccessfulMessage()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "OB1";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HB1";
			var underbond = mawb.Underbonds.AddNew();
			underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Rail;
			underbond.UnderbondStatus.Code = CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived;

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				},
				TransportMode = new CodeDescriptionPair
				{
					Code = CMRUnderbondModeOfMovement.Codes.Road,
					Description = CMRUnderbondModeOfMovement.Descriptions.Road
				},
			};
			shipment.SetSubShipmentCollection(() => new DataObjectList<UShipment>
				{
					new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						WayBillNumber = "HB1",
						WayBillType = new WayBillType
						{
							Code = WayBillTypeList.Codes.House,
							Description = WayBillTypeList.Descriptions.House
						},
						TransportMode = new CodeDescriptionPair
						{
							Code = CMRUnderbondModeOfMovement.Codes.Road,
							Description = CMRUnderbondModeOfMovement.Descriptions.Road
						},
					}
				});
			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			underbond = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				underbond.Outturns.Reload(true);
				var outturn = underbond.Outturns.Cast<CusOutturn>().Single();
				AssertEquals("outturn.C5_ParentID", hawb.PK, outturn.C5_ParentID);
				AssertEquals("outturn.C5_ParentTableCode", CusHAWBSchema.Constants.Prefix, outturn.C5_ParentTableCode);
				AssertEquals("underbond.C4_ModeOfMovement", CMRUnderbondModeOfMovement.Codes.Rail, underbond.C4_ModeOfMovement);
			});
		}

		public void TestUpdateUnderbondWhenMessageWithoutDataSource()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "OB1";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HB1";
			var underbond = mawb.Underbonds.AddNew();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataSource(DataContextType.AirManifest, null);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "OB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.Master,
					Description = WayBillTypeList.Descriptions.Master
				},
				TransportMode = new CodeDescriptionPair
				{
					Code = CMRUnderbondModeOfMovement.Codes.Road,
					Description = CMRUnderbondModeOfMovement.Descriptions.Road
				},
			};
			shipment.SetSubShipmentCollection(() => new DataObjectList<UShipment>
			{
				new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "HB1",
					WayBillType = new WayBillType
					{
						Code = WayBillTypeList.Codes.House,
						Description = WayBillTypeList.Descriptions.House
					},
					TransportMode = new CodeDescriptionPair
					{
						Code = CMRUnderbondModeOfMovement.Codes.Road,
						Description = CMRUnderbondModeOfMovement.Descriptions.Road
					}
				}
			});
			Factory.SaveForTesting();

			var reader = new CusUnderbondDataObjectReader(shipment, logger, Factory);
			underbond = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				underbond.Outturns.Reload(true);
				var outturn = underbond.Outturns.Cast<CusOutturn>().Single();
				AssertEquals("outturn.C5_ParentID", hawb.PK, outturn.C5_ParentID);
				AssertEquals("outturn.C5_ParentTableCode", CusHAWBSchema.Constants.Prefix, outturn.C5_ParentTableCode);
				AssertEquals("underbond.C4_ModeOfMovement", CMRUnderbondModeOfMovement.Codes.Road, underbond.C4_ModeOfMovement);
			});
		}
	}
}
