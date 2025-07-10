using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UEntryType = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryType;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnDataObjectReaderTest : DataTransfer.Universal.Testing.DataObjectReaderTestHelper
	{
		public void TestImportingData()
		{
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB1",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.House,
					Description = WayBillTypeList.Descriptions.House
				},
				ShipmentType = new CodeDescriptionPair
				{
					Code = CMROutturnResultType.Codes.NilDiscrepancy,
					Description = CMROutturnResultType.Descriptions.NilDiscrepancy
				},
				OuterPacks = 9,
				OuterPacksPackageType = new PackageType
				{
					Code = CMRPackageTypes.Codes.Crate,
					Description = CMRPackageTypes.Descriptions.Crate
				},
				TotalNoOfPacks = 10,
				EntryStatus = new EntryStatus
				{
					Code = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement,
					Description = CMRConsolidatedCargoStatuses.Descriptions.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement
				},
			};
			shipment.SetAddInfoCollection(() => new List<UAddInfo>
				{
					new UAddInfo
					{
						Key = Outturn.Constants.AddInfoType.IsDamage,
						Value = Customs.Business.YesNoList.Codes.Yes
					},
					new UAddInfo
					{
						Key = Outturn.Constants.AddInfoType.IsPillage,
						Value = Customs.Business.YesNoList.Codes.Yes
					}
				});
			shipment.SetNoteCollection(() => new DataObjectList<Note>(new[]
				{
					new Note
					{
						Description = Outturn.Constants.Note.Descriptions.GoodsDescription,
						IsCustomDescription = ZBool.True,
						NoteText = "Cuckoo Squeakers"
					}
				}));
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new UEntryType
						{
							Code = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
							Description = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID
						},
						ContextInformation = Core.Constants.CountryCodes.Australia,
						ReferenceNumber = "RE123"
					},
				});

			Factory.SaveForTesting();

			var cusMAWB = Factory.New<CusMAWB>();
			var underbond = cusMAWB.Underbonds.AddNew();
			underbond.C4_ResponsiblePartyID = "RE123";
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_HAWB = "HB1";
			var reader = new CusOutturnDataObjectReader(shipment, logger, Factory, underbond);
			var outturn = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("outturn.C5_ParentID", cusHAWB.PK, outturn.C5_ParentID);
				AssertEquals("outturn.C5_ParentTableCode", CusHAWBSchema.Constants.Prefix, outturn.C5_ParentTableCode);
				AssertEquals("outturn.C5_C4_Underbond", underbond.PK, outturn.C5_C4_Underbond);
				AssertEquals("outturn.C5_OutturnResultType", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);
				AssertEquals("outturn.C5_OuterPacks", 9, outturn.C5_OuterPacks);
				AssertEquals("outturn.C5_OuterPackUnits", CMRPackageTypes.Codes.Crate, outturn.C5_OuterPackUnits);
				AssertEquals("outturn.C5_PackagesOutturned", 10, outturn.C5_PackagesOutturned);
				AssertEquals("outturn.C5_DamageIndicator", ZBool.True, outturn.C5_DamageIndicator);
				AssertEquals("outturn.C5_PillageIndicator", ZBool.True, outturn.C5_PillageIndicator);
				AssertEquals("outturn.C5_GoodsDescription", "Cuckoo Squeakers", outturn.C5_GoodsDescription);
				AssertEquals("outturn.C5_CustomsStatus", "", outturn.C5_CustomsStatus);
			});
		}

		public void TestImportingData_NoMatchingHouseBill()
		{
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB2",
				WayBillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.House,
					Description = WayBillTypeList.Descriptions.House
				},
				GoodsValue = 10.0,
				TotalWeight = 100.0,
				TotalWeightUnit = new UnitOfWeight
				{
					Code = Core.Constants.Weight.Kilograms,
					Description = "Kilograms"
				}
			};
			shipment.SetNoteCollection(() => new DataObjectList<Note>(new[]
				{
					new Note
					{
						Description = Outturn.Constants.Note.Descriptions.GoodsDescription,
						IsCustomDescription = ZBool.True,
						NoteText = "Cuckoo Squeakers"
					}
				}));

			Factory.SaveForTesting();

			var cusMAWB = Factory.New<CusMAWB>();
			var underbond = cusMAWB.Underbonds.AddNew();
			underbond.C4_ResponsiblePartyID = "RE123";
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_HAWB = "HB1";
			var reader = new CusOutturnDataObjectReader(shipment, logger, Factory, underbond);
			var outturn = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				var housebill = cusMAWB.ChildBills.Cast<CusHAWB>().Single(x => x.CS_HAWB == "HB2");
				AssertEquals("cusMAWB.ChildBills.Count", 2, cusMAWB.ChildBills.Count);
				AssertEquals("housebill.CS_GoodsDescription", "Cuckoo Squeakers", housebill.CS_GoodsDescription);
				AssertEquals("housebill.CS_GoodsValue", 10.0m, housebill.CS_GoodsValue);
				AssertEquals("housebill.CS_Weight", 100.0m, housebill.CS_Weight);
				AssertEquals("housebill.CS_WeightUQ", "KG", housebill.CS_WeightUQ);
			});
		}
	}
}
