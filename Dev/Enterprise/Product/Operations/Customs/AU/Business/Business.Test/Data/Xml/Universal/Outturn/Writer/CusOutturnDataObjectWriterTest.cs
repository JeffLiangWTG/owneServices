using System;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestMappings()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("AA33N", "67094168242");
			using (FreightDataRegistry.Instance.OuturnResponsiblePartyIDOverride.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, codeDescriptionPairList))
			{
				var underbond = Factory.New<CusUnderbond>();
				underbond.C4_MAWB = "OB1";
				var destinationAddress = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				destinationAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "AA33N", Core.Constants.CountryCodes.Australia);
				underbond.C4_OA_DestinationAddress = destinationAddress.MainAddress.PK;
				underbond.C4_DestinationPremiseID = "AA33N";
				Factory.SaveForTesting();

				var outturn = Factory.New<CusOutturn>();
				outturn.C5_HouseBill = "HB1";
				outturn.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
				outturn.C5_DamageIndicator = ZBool.True;
				outturn.C5_PillageIndicator = ZBool.True;
				outturn.C5_OuterPacks = 9;
				outturn.C5_OuterPackUnits = CMRPackageTypes.Codes.Crate;
				outturn.C5_PackagesOutturned = 10;
				outturn.C5_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
				outturn.C5_GoodsDescription = "Cuckoo Squeakers";

				underbond.Outturns.Add(outturn);

				Factory.SaveForTesting();

				var writer = new CusUnderbondDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, underbond)));
				var underbondData = writer.GetDataObject(underbond);

				CombineAssertions(delegate
				{
					AssertEquals("underbondData.WayBillNumber", "OB1", underbondData.WayBillNumber);

					AssertEquals(1, underbondData.SubShipmentCollection.Count);
					var outturnData = underbondData.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == "HB1");
					AssertNotNull("HB1", outturnData);
					AssertEquals("OuterPacks", 9, outturnData.OuterPacks);
					AssertEquals("OuterPacksPackageType.Code", CMRPackageTypes.Codes.Crate, outturnData.OuterPacksPackageType.Code.GetValueOrDefault());
					AssertEquals("OuterPacksPackageType.Description", CMRPackageTypes.Descriptions.Crate, outturnData.OuterPacksPackageType.Description.GetValueOrDefault());
					AssertEquals("TotalNoOfPacks", 10, outturnData.TotalNoOfPacks);
					AssertEquals("EntryStatus.Code", CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement, outturnData.EntryStatus.Code.GetValueOrDefault());
					AssertEquals("EntryStatus.Description", CMRConsolidatedCargoStatuses.Descriptions.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement, outturnData.EntryStatus.Description.GetValueOrDefault());

					AssertAdditionalReference(outturnData.AdditionalReferenceCollection[0], "67094168242",
						CodeDescriptionPairForTesting.New(
							DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
							DataTransfer.Universal.Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID));

					AssertAddInfo("IsDamage", outturnData, Outturn.Constants.AddInfoType.IsDamage, "Y");
					AssertAddInfo("IsPillage", outturnData, Outturn.Constants.AddInfoType.IsPillage, "Y");
					AssertNote("GoodsDescription", outturnData, Outturn.Constants.Note.Descriptions.GoodsDescription, "Cuckoo Squeakers");
				});
			}
		}

		static void AssertAddInfo(ZString message, UShipment shipment, ZString addInfoType, ZString expectedValue)
		{
			var addinfo = shipment.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == addInfoType);
			AssertNotNull(message, addinfo);
			AssertEquals(message, expectedValue, addinfo.Value);
		}

		static void AssertAdditionalReference(AdditionalReference additionalReferenceDataObject, ZString referenceNumber, ICodeDescription type, string contextInformation = Core.Constants.CountryCodes.Australia)
		{
			AssertNotNull("Precondition: additionalReferenceDataObject", additionalReferenceDataObject);
			AssertEquals("additionalReferenceDataObject.ContextInformation", contextInformation, additionalReferenceDataObject.ContextInformation);
			AssertEquals("additionalReferenceDataObject.ReferenceNumber", referenceNumber, additionalReferenceDataObject.ReferenceNumber);
			AssertNotNull("additionalReferenceDataObject.Type", additionalReferenceDataObject.Type);
			AssertEquals("additionalReferenceDataObject.Type.Code", type.Code, additionalReferenceDataObject.Type.Code);
			AssertEquals("additionalReferenceDataObject.Type.Description", type.Description, additionalReferenceDataObject.Type.Description);
		}

		static void AssertNote(ZString message, UShipment shipment, ZString descripton, ZString expectedNoteText, bool expectedIsCustomDescription = true)
		{
			var addinfo = shipment.NoteCollection.FirstOrDefault(x => x.Description.GetValueOrDefault() == descripton);
			AssertNotNull(message, addinfo);
			AssertEquals(message, expectedNoteText, addinfo.NoteText.GetValueOrDefault());
			AssertEquals(message, expectedIsCustomDescription, addinfo.IsCustomDescription.GetValueOrDefault());
		}
	}
}
