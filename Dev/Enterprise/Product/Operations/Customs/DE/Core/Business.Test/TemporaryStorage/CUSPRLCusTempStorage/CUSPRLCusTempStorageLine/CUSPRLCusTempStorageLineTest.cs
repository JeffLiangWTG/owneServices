using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPRLCusTempStorageLine))]
	public class CUSPRLCusTempStorageLineTest : CusTempStorageLineTest<CUSPRLCusTempStorageLine>
	{
		public void TestCarrier()
		{
			var cusprlStorageLine = Factory.New<CUSPRLCusTempStorageLine>();
			var carrierDocAddress = cusprlStorageLine.CarrierDocAddress;
			AssertEquals("DocAddressType", DocAddressType.Carrier, carrierDocAddress.DocAddressType);
		}

		public void TestCarrierDocAddressRequirement_ValidateOrganisationPK_Mandatory()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var dec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			var cusprlStorageLine = dec.CusTempStorageLines.AddNew();
			var propertyInfo = cusprlStorageLine.CarrierDocAddress.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				cusprlStorageLine.CarrierDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("Previous Ref. Type not N355; Transport Document Type not N703, N740", propertyInfo, MandatoryValidation.YouHaveNotEntered);

				storageHeader.SJH_PreviousReferenceType = "N355";
				cusprlStorageLine.CarrierDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("Previous Ref. Type N355; Transport Document Type not N703, N740", propertyInfo, MandatoryValidation.YouHaveNotEntered);

				cusprlStorageLine.TSL_TransportNumberType = "N703";
				cusprlStorageLine.CarrierDocAddress.Validation.ValidateOrganisationPK();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo, MandatoryValidation.YouHaveNotEntered, "Previous Ref. Type: N355; Transport Document Type: N703");

				cusprlStorageLine.TSL_TransportNumberType = "N740";
				cusprlStorageLine.CarrierDocAddress.Validation.ValidateOrganisationPK();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo, MandatoryValidation.YouHaveNotEntered, "Previous Ref. Type: N355; Transport Document Type: N740");

				storageHeader.SJH_PreviousReferenceType = "N360";
				cusprlStorageLine.CarrierDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("Previous Ref. Type not N355; Transport Document Type N740", propertyInfo, MandatoryValidation.YouHaveNotEntered);

				cusprlStorageLine.CarrierDocAddress.Validation.ValidateOrganisationPK();
				cusprlStorageLine.TSL_TransportNumberType = "N703";
				AssertNoMessageErrorContaining("Previous Ref. Type not N355; Transport Document Type N703", propertyInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCarrierDocAddressRequirement_ValidateOrganisationPK_MissingEORINumber()
		{
			const string message = "No EORI code (EOR) exists in the organization registration numbers for organization TEST.";

			var carrierOrg = Factory.GetOrgHeaderWithEori("TEST", "12345", Core.Constants.CountryCodes.Germany);
			var carrierAddress = carrierOrg.MainAddress;
			carrierAddress.Address1 = "Address1";
			carrierAddress.City = "City";
			carrierAddress.Postcode = "2730018";
			carrierAddress.OA_RN_NKCountryCode = "DE";
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_OA_Address = carrierAddress.PK;

			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var dec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			var cusprlStorageLine = dec.CusTempStorageLines.AddNew();
			var propertyInfo = cusprlStorageLine.CarrierDocAddress.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				cusprlStorageLine.CarrierDocAddress.OrganisationPK = carrierOrg.PK;

				AssertNoMessageError("No carrier", propertyInfo, message);

				cusprlStorageLine.CarrierDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Carrier has EORI", propertyInfo, message);

				carrierOrg.GetOrgCusCode("EOR", "12345").Delete();

				cusprlStorageLine.CarrierDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Carrier without EORI", propertyInfo, message);
			});
		}

		public void TestTransportDocumentMaster() => AssertType<TransportDocumentMaster>(Factory.New<CUSPRLCusTempStorageLine>().TransportDocumentMaster);

		public void TestReceptacle()
		{
			var cusprlStorageLine = Factory.New<CUSPRLCusTempStorageLine>();
			var testString = "a".PadRight(34, 'a');
			cusprlStorageLine.Receptacle = testString;
			AssertEquals(testString, cusprlStorageLine.Receptacle);
		}

		public void TestReceptacleCaption()
		{
			var data = DataBoundResourceStrings.GetDataForProperty(this.GetNewCusTempStorageLine().ReceptacleInfo);
			AssertEquals("Receptacle", data.Caption);
		}

		public void TestReceptacle_GenAddOnColumn()
		{
			var cusprlStorageLine = Factory.New<CUSPRLCusTempStorageLine>();
			var testString = "a".PadRight(34, 'a');

			var query = new ZQuery(GenAddOnColumnSchema.XA_Name, "TSL_Receptacle");
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusTempStorageLineSchema.Constants.Prefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentID, cusprlStorageLine.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_Type, "STR");

			CombineAssertions(() =>
			{
				AssertEquals("No GenAddOn", expected: false, Factory.Exists(typeof(GenAddOnColumn), query));
				cusprlStorageLine.Receptacle = testString;
				var genAddOn = Factory.Load<GenAddOnColumn>(query).Single();
				AssertEquals("Column Exists with correct value", testString, genAddOn.XA_Data);
			});
		}

		public void TestReceptacle_MaxLength()
		{
			var cusprlStorageLine = Factory.New<CUSPRLCusTempStorageLine>();
			AssertEquals(35, cusprlStorageLine.ReceptacleInfo.MaxLength);
		}

		public void TestContainerNumber()
		{
			var cusprlStorageLine = Factory.New<CUSPRLCusTempStorageLine>();
			var testString = "1".PadRight(16, '1');
			cusprlStorageLine.ContainerNumber = testString;
			AssertEquals(testString, cusprlStorageLine.ContainerNumber);
		}

		public void TestContainerNumberCaption()
		{
			var data = DataBoundResourceStrings.GetDataForProperty(this.GetNewCusTempStorageLine().ContainerNumberInfo);
			AssertEquals("Container Number", data.Caption);
		}

		public void TestContainerNumber_GenAddOnColumn()
		{
			var cusprlStorageLine = Factory.New<CUSPRLCusTempStorageLine>();
			var testString = "1".PadRight(16, '1');

			var query = new ZQuery(GenAddOnColumnSchema.XA_Name, "TSL_ContainerNumber");
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusTempStorageLineSchema.Constants.Prefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentID, cusprlStorageLine.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_Type, "STR");

			CombineAssertions(() =>
			{
				AssertEquals("No GenAddOn", expected: false, Factory.Exists(typeof(GenAddOnColumn), query));
				cusprlStorageLine.ContainerNumber = testString;
				var genAddOn = Factory.Load<GenAddOnColumn>(query).Single();
				AssertEquals("Column Exists with correct value", testString, genAddOn.XA_Data);
			});
		}

		public void TestContainerNumber_MaxLength()
		{
			var cusprlStorageLine = Factory.New<CUSPRLCusTempStorageLine>();
			AssertEquals(17, cusprlStorageLine.ContainerNumberInfo.MaxLength);
		}

		public void TestCarrierDocAddress()
		{
			var cusprlStorageLine = Factory.New<CUSPRLCusTempStorageLine>();
			var carrierDocAddress = cusprlStorageLine.CarrierDocAddress;

			CombineAssertions(() =>
			{
				AssertNotNull("Created when Null", carrierDocAddress);
				AssertEquals("Type", DocAddressType.Carrier, carrierDocAddress.DocAddressType);
			});
		}

		public void TestTSL_IsModified()
		{
			var modifiers = new List<Action<CusTempStorageLine>>
			{
				l => l.TSL_LineNo = 535,
				l => l.TSL_ReferenceNumber = "11DE11111111111116",
				l => l.TSL_ReferenceNumberLine = 979,
				l => l.TSL_ReferenceNumber2 = "ATB150000010320006001",
				l => l.TSL_ReferenceNumber2Line = 588,
				l => l.TSL_UnionStatus = DEUnionStatusList.Codes.C,
				l => l.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB,
				l => l.TSL_OwnerReferenceNumber = "789013",
				l => l.TSL_GoodsDescription = "GOODS DESCRIPTION 1",
				l => l.TSL_GoodsType = DEGoodsTypeList.Codes.A,
				l => l.TSL_GrossWeight = 35.874m,
				l => l.TSL_RN_NKDepartureCountry = Core.Constants.CountryCodes.Afghanistan,
				l => l.TSL_DestinationPlace = "DESTINATION PLACE 1",
				l => l.TSL_LocationOfGoods = "LOC04",
				l => l.TSL_IsFTZ = false,
				l => l.TSL_CustodianIdentifier = "DE1234567891",
				l => l.TSL_CustodianIdentifierBranchNo = "0002",
				l => l.TSL_GoodsOwnerIdentifier = "DE0987654322",
				l => l.TSL_GoodsOwnerIdentifierBranchNo = "0003",
				l => l.TSL_PackageType = "VT",
				l => l.TSL_PackageQty = 934
			};

			var i = 0;
			foreach (var modifier in modifiers)
			{
				var line = GetNewCusTempStorageLine();
				line.Dec.StorageHeader.SJH_JobReference = "VWG" + i;
				line.Dec.StorageHeader.Customer.OH_Code = "VW" + i++;
				line.TSL_LineNo = 534;
				line.TSL_ReferenceNumber = "11DE11111111111115";
				line.TSL_ReferenceNumberLine = 978;
				line.TSL_ReferenceNumber2 = "ATB150000010320006000";
				line.TSL_ReferenceNumber2Line = 587;
				line.TSL_UnionStatus = DEUnionStatusList.Codes.X;
				line.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ZZZ;
				line.TSL_OwnerReferenceNumber = "789012";
				line.TSL_GoodsDescription = "GOODS DESCRIPTION";
				line.TSL_GoodsType = DEGoodsTypeList.Codes.E;
				line.TSL_GrossWeight = 35.873m;
				line.TSL_RN_NKDepartureCountry = Core.Constants.CountryCodes.FishFromForeignBoats;
				line.TSL_DestinationPlace = "DESTINATION PLACE";
				line.TSL_LocationOfGoods = "LOC03";
				line.TSL_IsFTZ = true;
				line.TSL_CustodianIdentifier = "DE1234567890";
				line.TSL_CustodianIdentifierBranchNo = "0001";
				line.TSL_GoodsOwnerIdentifier = "DE0987654321";
				line.TSL_GoodsOwnerIdentifierBranchNo = "0002";
				line.TSL_PackageType = "VS";
				line.TSL_PackageQty = 937;

				Factory.Save();
				Assert(!line.TSL_IsModified);

				line.Dec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
				Factory.Save();
				Assert(!line.TSL_IsModified);

				modifier.Invoke(line);
				Assert(line.HasChanges);
				Factory.Save();
				Assert(line.TSL_IsModified);

				line.TSL_IsModified = ZBool.False;
				Factory.Save();
				Assert(!line.TSL_IsModified);
			}
		}

		public void TestSequenceNumberEnabled_DecNull()
		{
			var storageLine = Factory.New<CUSPRLCusTempStorageLine>();
			AssertEquals(true, storageLine.SequenceNumberEnabled);
		}

		public void TestSequenceNumberEnabled_ReferenceNumber()
		{
			CombineAssertions(() =>
			{
				var storageLine = GetNewCusTempStorageLine();
				storageLine.Dec.ReferenceNumber = ZString.Empty;
				AssertEquals("Empty", true, storageLine.SequenceNumberEnabled);
				storageLine.Dec.ReferenceNumber = "123";
				AssertEquals("Entered", false, storageLine.SequenceNumberEnabled);
			});
		}

		public void TestTSL_ReferenceNumber()
		{
			var storageLine = Factory.New<CUSPRLCusTempStorageLine>();
			AssertEquals(18, storageLine.TSL_ReferenceNumberInfo.MaxLength);
		}

		public void TestTSL_GrossWeightUQ()
		{
			var storageLine = Factory.New<CUSPRLCusTempStorageLine>();
			AssertEquals(Core.Constants.Weight.Kilograms, storageLine.TSL_GrossWeightUQ);
		}

		public void TestTSL_CustomsStatus()
		{
			var storageLine = Factory.New<CUSPRLCusTempStorageLine>();
			Assert(storageLine.TSL_CustomsStatusInfo.ReadOnly);
		}

		public void TestTSL_LineNo_ReadOnly_DecNull()
		{
			var storageLine = Factory.New<CUSPRLCusTempStorageLine>();
			AssertEquals(true, storageLine.TSL_LineNoInfo.ReadOnly);
		}

		public void TestTSL_LineNo_ReadOnly_ReferenceNumber()
		{
			CombineAssertions(() =>
			{
				var storageLine = GetNewCusTempStorageLine();
				storageLine.TSL_CustomsStatus = ZString.Empty;
				storageLine.Dec.ReferenceNumber = ZString.Empty;
				AssertEquals("Empty", true, storageLine.TSL_LineNoInfo.ReadOnly);
				storageLine.Dec.ReferenceNumber = "123";
				AssertEquals("Entered", false, storageLine.TSL_LineNoInfo.ReadOnly);
			});
		}

		public void TestTSL_LineNo_ReadOnly_CustomsStatus()
		{
			CombineAssertions(() =>
			{
				var storageLine = GetNewCusTempStorageLine();
				storageLine.Dec.ReferenceNumber = "123";
				storageLine.TSL_CustomsStatus = ZString.Empty;
				AssertEquals("Empty", false, storageLine.TSL_LineNoInfo.ReadOnly);
				storageLine.TSL_CustomsStatus = CustomsStatusList.Codes.TST;
				AssertEquals("Entered", true, storageLine.TSL_LineNoInfo.ReadOnly);
			});
		}

		public void TestSequence_Active_RecordRemoved()
		{
			var storageLine1 = GetNewCusTempStorageLine();
			var storageDec = storageLine1.Dec;
			storageDec.ReferenceNumber = ZString.Empty;
			var storageLine2 = storageDec.CusTempStorageLines.AddNew();
			var storageLine3 = storageDec.CusTempStorageLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Ordered 1", 1, storageLine1.TSL_LineNo);
				AssertEquals("Ordered 2", 2, storageLine2.TSL_LineNo);
				AssertEquals("Ordered 3", 3, storageLine3.TSL_LineNo);

				storageLine2.Delete();
				AssertEquals("Reordered 1", 1, storageLine1.TSL_LineNo);
				AssertEquals("Reordered 3", 2, storageLine3.TSL_LineNo);
			});
		}

		public void TestSequence_Inactive_RecordRemoved()
		{
			var storageLine1 = GetNewCusTempStorageLine();
			var storageDec = storageLine1.Dec;
			storageDec.ReferenceNumber = "VALUEDECLAREDBYOTHERENTITY";
			var storageLine2 = storageDec.CusTempStorageLines.AddNew();
			var storageLine3 = storageDec.CusTempStorageLines.AddNew();
			AssertEquals("Ordered 1", 1, storageLine1.TSL_LineNo);
			AssertEquals("Ordered 1", 2, storageLine2.TSL_LineNo);
			AssertEquals("Ordered 1", 3, storageLine3.TSL_LineNo);

			storageLine2.Delete();
			AssertEquals("Same 1", 1, storageLine1.TSL_LineNo);
			AssertEquals("Same 3", 3, storageLine3.TSL_LineNo);

			storageLine2 = storageDec.CusTempStorageLines.AddNew();
			storageLine1.TSL_LineNo = 1;
			storageLine2.TSL_LineNo = 45;
			storageLine3.TSL_LineNo = 64;

			var storageLine4 = storageDec.CusTempStorageLines.AddNew();
			AssertEquals(65, storageLine4.TSL_LineNo);

			storageDec.ReferenceNumber = ZString.Empty;
			storageLine4.Delete();
			AssertEquals(1, storageLine1.TSL_LineNo);
			AssertEquals(2, storageLine2.TSL_LineNo);
			AssertEquals(3, storageLine3.TSL_LineNo);
		}

		public void TestSequence_Inactive_AddedRecord()
		{
			var storageLine1 = GetNewCusTempStorageLine();
			var storageDec = storageLine1.Dec;
			storageDec.ReferenceNumber = "VALUEDECLAREDBYOTHERENTITY";
			var storageLine2 = storageDec.CusTempStorageLines.AddNew();
			var storageLine3 = storageDec.CusTempStorageLines.AddNew();
			storageLine1.TSL_LineNo = 1;
			storageLine2.TSL_LineNo = 45;
			storageLine3.TSL_LineNo = 64;

			var storageLine4 = storageDec.CusTempStorageLines.AddNew();
			AssertEquals(65, storageLine4.TSL_LineNo);
		}

		public void TestSequence_InactiveToActive()
		{
			var storageLine1 = GetNewCusTempStorageLine();
			var storageDec = storageLine1.Dec;
			storageDec.ReferenceNumber = "VALUEDECLAREDBYOTHERENTITY";
			var storageLine2 = storageDec.CusTempStorageLines.AddNew();
			var storageLine3 = storageDec.CusTempStorageLines.AddNew();
			storageLine1.TSL_LineNo = 1;
			storageLine2.TSL_LineNo = 45;
			storageLine3.TSL_LineNo = 64;

			storageDec.ReferenceNumber = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("Reordered 1", 1, storageLine1.TSL_LineNo);
				AssertEquals("Reordered 2", 2, storageLine2.TSL_LineNo);
				AssertEquals("Reordered 3", 3, storageLine3.TSL_LineNo);
			});
		}

		public void TestTSL_ReferenceNumberLine()
		{
			var storageLine = Factory.New<CUSPRLCusTempStorageLine>();
			AssertEquals(3, storageLine.TSL_ReferenceNumberLineInfo.MaxLength);
		}

		public void TestTSL_ReferenceNumber2Line()
		{
			var storageLine = Factory.New<CUSPRLCusTempStorageLine>();
			AssertEquals(5, storageLine.TSL_ReferenceNumber2LineInfo.MaxLength);
		}

		public void TestTSL_TransportNumberType_Caption()
		{
			var storageLine = Factory.New<CUSPRLCusTempStorageLine>();
			AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(storageLine.TSL_TransportNumberTypeInfo).Caption);
		}

		public void TestTSL_TransportNumber_Caption()
		{
			var storageLine = Factory.New<CUSPRLCusTempStorageLine>();
			AssertEquals("Reference Number", DataBoundResourceStrings.GetDataForProperty(storageLine.TSL_TransportNumberInfo).Caption);
		}

		public void TestTransportDocumentMasterIsDeletedWhenLineIsDeleted()
		{
			var line = GetNewCusTempStorageLine(Factory);

			line.TransportDocumentMaster.CSI_Code = "123";

			var transportDocumentMaster = line.TransportDocumentMaster;

			AssertEquals("Precondition", false, line.IsDeleted);
			AssertEquals("Precondition", false, transportDocumentMaster.IsDeleted);

			line.Delete();

			AssertEquals("Deleted", true, line.IsDeleted);
			AssertEquals("Should be deleted with Parent Line.", true, transportDocumentMaster.IsDeleted);
		}

		protected override CUSPRLCusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_OH_Customer = customer.PK;
			var storageDecs = CUSPRLCusTempStorageDec.LoadOrCreate(storageJobHeader);
			return storageDecs.CusTempStorageLines.AddNew();
		}

		protected override Type GetDecType() => typeof(CUSPRLCusTempStorageDec);

		protected override Type GetLookupType() => typeof(CUSPRLCusTempStorageLineLookups);

		protected override Type GetValidationType() => typeof(CUSPRLCusTempStorageLineValidation);
	}
}
