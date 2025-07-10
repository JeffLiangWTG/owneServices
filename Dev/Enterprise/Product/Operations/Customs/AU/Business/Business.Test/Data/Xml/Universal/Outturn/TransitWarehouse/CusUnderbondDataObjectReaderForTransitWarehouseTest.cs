using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusUnderbondDataObjectReaderForTransitWarehouseTest : DataTransfer.Universal.Testing.DataObjectReaderTestHelper
	{
		[TestDate(2022, 5, 11)]
		public void TestReadIntoBusinessObject_NoHousebillFound_LogMessage()
		{
			var mawb = Factory.New<Customs.Business.CusMAWB>();
			mawb.CM_MAWB = "MB001";
			mawb.CM_ApplicationCode = "CMR";

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentID = mawb.PK;
			underbond.C4_ParentTableCode = "CM";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "HB001"
			};

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new UniversalDataBuss.DataObjects.Universal.EntryType
						{
							Code = AdditionalReferenceTypes.Codes.MasterBill,
							Description = AdditionalReferenceTypes.Descriptions.MasterBill
						},
						ReferenceNumber = "MB001"
					}
				});

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OutturnQty = 1,
					PackQty = 1
				}
			});

			var reader = new CusUnderbondDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			AssertContains("Should create a new Outturn", "Added Outturn Bill HB001 from UniversalShipment.", logger.Logs);

			var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, testUnderbond.PK);
			var outturns = Factory.Load<CusOutturn>(query);
			AssertEquals("Should create an outturn", 1, outturns.Length);
		}

		[TestDate(2022, 5, 11)]
		public void TestReadIntoBusinessObject_PopulateOutturn()
		{
			var mawb = Factory.New<Customs.Business.CusMAWB>();
			mawb.CM_MAWB = "MB001";
			mawb.CM_ApplicationCode = "CMR";

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HB001";

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentID = mawb.PK;
			underbond.C4_ParentTableCode = "CM";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "HB001"
			};

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new UniversalDataBuss.DataObjects.Universal.EntryType
						{
							Code = AdditionalReferenceTypes.Codes.MasterBill,
							Description = AdditionalReferenceTypes.Descriptions.MasterBill
						},
						ReferenceNumber = "MB001"
					}
				});

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OutturnQty = 1,
					PackQty = 1
				}
			});

			var reader = new CusUnderbondDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, testUnderbond.PK);
			var outturns = Factory.Load<CusOutturn>(query);
			AssertEquals("Should create 1 outturn", 1, outturns.Length);
		}

		[TestDate(2022, 5, 11)]
		public void TestReadIntoBusinessObject_PolulateOutturn_WhenCusMAWBIsNull()
		{
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "MB001";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "HB001"
			};

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new UniversalDataBuss.DataObjects.Universal.EntryType
						{
							Code = AdditionalReferenceTypes.Codes.MasterBill,
							Description = AdditionalReferenceTypes.Descriptions.MasterBill
						},
						ReferenceNumber = "MB001"
					}
				});

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OutturnQty = 1,
					PackQty = 1
				}
			});

			var reader = new CusUnderbondDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, testUnderbond.PK);
			var outturns = Factory.Load<CusOutturn>(query);
			AssertEquals("Should create 1 outturn", 1, outturns.Length);
		}

		public void TestThrowException_WhenDataContextTypeIsNotTransitReceive()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			dataContext.AddDataSource(DataContextType.TransitDispatch, "DC001");
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "HB001"
			};

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
			{
				new AdditionalReference
				{
					Type = new UniversalDataBuss.DataObjects.Universal.EntryType
					{
						Code = AdditionalReferenceTypes.Codes.MasterBill,
						Description = AdditionalReferenceTypes.Descriptions.MasterBill
					},
					ReferenceNumber = "MB001"
				}
			});

			var reader = new CusUnderbondDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("This UXML does not come from Transit Receive.", () => reader.ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_WhenMasterBillIsEmpty()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			dataContext.AddDataSource(DataContextType.TransitReceive, "RC001");
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "HB001"
			};

			var reader = new CusUnderbondDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			Assert(testUnderbond == null);
			AssertContains("Should print log", "No Air Cargo Report found with Master Bill.", logger.Logs);
		}

		public void TestPopulateBusinessObject_CreateNewUnderbondgWhenNoMatchUnderbondButMasterBillHasValue_MatchAddress()
		{
			PopulateBusinessObject_CreateNewUnderbondgWhenNoMatchUnderbondButMasterBillHasValueCore(true, true);
		}

		public void TestPopulateBusinessObject_CreateNewUnderbondgWhenNoMatchUnderbondButMasterBillHasValue_NoMatchAddress()
		{
			PopulateBusinessObject_CreateNewUnderbondgWhenNoMatchUnderbondButMasterBillHasValueCore(false, true);
		}

		public void TestPopulateBusinessObject_CreateNewUnderbondgWhenNoMatchUnderbondButMasterBillHasValue_NoMatchCusMAWB()
		{
			PopulateBusinessObject_CreateNewUnderbondgWhenNoMatchUnderbondButMasterBillHasValueCore(true, false);
		}

		void PopulateBusinessObject_CreateNewUnderbondgWhenNoMatchUnderbondButMasterBillHasValueCore(bool hasAddress, bool isMatchMAWB)
		{
			var mawb = Factory.New<Customs.Business.CusMAWB>();
			mawb.CM_MAWB = isMatchMAWB ? "MB001" : "MB002";
			mawb.CM_ApplicationCode = "CMR";

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HB001";

			OrgAddress orgAddressCFS = null;
			OrgAddress orgAddressCTO = null;

			if (hasAddress)
			{
				var orgHeaderCFS = Factory.New<OrgHeader>();
				orgHeaderCFS.OH_Code = "CFS";
				orgAddressCFS = Factory.New<OrgAddress>();
				orgAddressCFS.OA_OH = orgHeaderCFS.PK;
				orgAddressCFS.Address1 = "cfs1";
				orgAddressCFS.OA_Code = "CFS";

				var orgHeaderCTO = Factory.New<OrgHeader>();
				orgHeaderCTO.OH_Code = "CTO";
				orgAddressCTO = Factory.New<OrgAddress>();
				orgAddressCTO.OA_OH = orgHeaderCTO.PK;
				orgAddressCTO.Address1 = "cto1";
				orgAddressCTO.OA_Code = "CTO";
			}

			var addressCFS = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			addressCFS.AddressType = "LocalCartageCFS";
			addressCFS.CompanyName = "CFS";
			addressCFS.AddressShortCode = "CFS";
			addressCFS.OrganizationCode = new ZCodeMappedZString(orgAddressCFS == null ? ZString.Empty : orgAddressCFS.OA_Code);

			var addressCTO = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			addressCTO.AddressType = "LocalCartageCTO";
			addressCTO.CompanyName = "CTO";
			addressCTO.AddressShortCode = "CTO";
			addressCTO.OrganizationCode = new ZCodeMappedZString(orgAddressCTO == null ? ZString.Empty : orgAddressCTO.OA_Code);

			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			dataContext.AddDataSource(DataContextType.TransitReceive, "RC001");
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "HB001"
			};
			shipment.VoyageFlightNo = "VN001";
			shipment.TotalNoOfPiecesLanded = 1;
			shipment.TransportMode = new CodeDescriptionPair() { Code = "AIR", Description = "Air" };
			var now = DateTime.Now;
			shipment.SetDateCollection(() => new List<Date>
			{
				new Date()
				{
					Type = DateType.Arrival,
					IsEstimate = false,
					Value = now
				}
			});

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				addressCFS,
				addressCTO
			});

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
			{
				new AdditionalReference
				{
					Type = new UniversalDataBuss.DataObjects.Universal.EntryType
					{
						Code = AdditionalReferenceTypes.Codes.MasterBill,
						Description = AdditionalReferenceTypes.Descriptions.MasterBill
					},
					ReferenceNumber = "MB0-01"
				}
			});

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OutturnQty = 1,
					PackQty = 1
				}
			});

			var reader = new CusUnderbondDataObjectReaderForTransitWarehouse(shipment, logger, Factory);

			if (!isMatchMAWB)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("No MAWB found with Master Bill MB001.", () => reader.ReadIntoBusinessObject());
			}
			else
			{
				var testUnderbond = reader.ReadIntoBusinessObject();

				var underbondFromQuery = Factory.Load<CusUnderbond>(testUnderbond.PK);
				AssertNotNull("Should create new underbond", underbondFromQuery);
				AssertEquals("Should create new underbond with Master Bill", "MB001", underbondFromQuery.C4_MAWB);
				AssertEquals("Should create new underbond with Arrival Date", now, underbondFromQuery.C4_ArrivalDate);
				AssertEquals("Should create new underbond with Flight No", "VN001", underbondFromQuery.C4_FlightNo);
				AssertEquals("Should create new underbond with Pieces Manifested", 1, underbondFromQuery.C4_PiecesManifested);
				AssertEquals("Should create new underbond with Mode Of Movement", "AIR", underbondFromQuery.C4_ModeOfMovement);
				AssertEquals("Should create new underbond with Movement Reason", "DCL", underbondFromQuery.C4_MovementReason);
				AssertEquals("Should create new underbond with Is Move From Discharge", true, underbondFromQuery.C4_IsMoveFromDischarge);
				AssertEquals("Should create new underbond with ParentID", mawb.PK, underbondFromQuery.C4_ParentID);

				if (hasAddress)
				{
					AssertEquals("Should create new underbond with Origin Address", orgAddressCTO.PK, underbondFromQuery.C4_OA_OriginAddress);
					AssertEquals("Should create new underbond with Destination Address", orgAddressCFS.PK, underbondFromQuery.C4_OA_DestinationAddress);
				}
				else
				{
					AssertEquals("Should create new underbond with Origin Address", ZGuid.Empty, underbondFromQuery.C4_OA_OriginAddress);
					AssertEquals("Should create new underbond with Destination Address", ZGuid.Empty, underbondFromQuery.C4_OA_DestinationAddress);
				}

				var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, testUnderbond.PK);
				var outturns = Factory.Load<CusOutturn>(query);
				AssertEquals("Should create 1 outturn", 1, outturns.Length);
			}
		}

		public void TestReadIntoBusinessObject_WhenMasterBillContainClash()
		{
			var mawb = Factory.New<Customs.Business.CusMAWB>();
			mawb.CM_MAWB = "MB001";
			mawb.CM_ApplicationCode = "CMR";

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HB001";

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentID = mawb.PK;
			underbond.C4_ParentTableCode = "CM";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			dataContext.AddDataSource(DataContextType.TransitReceive, "RC001");
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "HB001"
			};

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
			{
				new AdditionalReference
				{
					Type = new UniversalDataBuss.DataObjects.Universal.EntryType
					{
						Code = AdditionalReferenceTypes.Codes.MasterBill,
						Description = AdditionalReferenceTypes.Descriptions.MasterBill
					},
					ReferenceNumber = "MB0-01"
				}
			});

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OutturnQty = 1,
					PackQty = 1
				}
			});

			var reader = new CusUnderbondDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			var testUnderbond = reader.ReadIntoBusinessObject();

			var underbondFromQuery = Factory.Load<CusUnderbond>(testUnderbond.PK);
			AssertEquals("Should find underbond", underbondFromQuery.PK, underbond.PK);
		}
	}
}
