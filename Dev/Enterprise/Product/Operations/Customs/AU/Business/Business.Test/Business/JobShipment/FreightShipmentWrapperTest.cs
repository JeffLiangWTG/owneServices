using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FreightShipmentWrapperTest : TestCaseWithFactory
	{
		public void TestFreightShipmentWrapper()
		{
			AssertNotNull("Wrapper", wrapper);
		}

		public void TestConstructor()
		{
			var shipment = Factory.New<CommonShipment>();
			var hvlvConsignment = Factory.New<IHVLVConsignment>();
			var hvlvConsignmentWrapper = new FreightShipmentWrapper(shipment, null, hvlvConsignment);

			AssertNotNull("HVLV Consignment Line Wrapper", hvlvConsignmentWrapper);
		}

		public void TestLineSequenceCollection()
		{
			var shipment = Factory.New<CommonShipment>();
			var hvlvConsignment = Factory.New<IHVLVConsignment>();

			var shipmentWrapper = new FreightShipmentWrapper(shipment, null);
			var hvlvConsignmentWrapper = new FreightShipmentWrapper(shipment, null, hvlvConsignment);

			CombineAssertions(() =>
			{
				AssertEquals("Shipment Wrapper Type", CusCodeDataTypeList.Codes.CustomsManifestLineSequence, shipmentWrapper.LineSequenceCollection.CY_Type);
				AssertEquals("HVLVConsignment Wrapper Type", CusCodeDataTypeList.Codes.CustomsManifestLineSequence, hvlvConsignmentWrapper.LineSequenceCollection.CY_Type);

				AssertEquals("Shipment Wrapper Master", shipment, shipmentWrapper.LineSequenceCollection.Master);
				AssertEquals("HVLVConsignment Wrapper Master", hvlvConsignment, hvlvConsignmentWrapper.LineSequenceCollection.Master);
			});
		}

		public void TestGetJobDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_JS = wrapper.Shipment.PK;

			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			company.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_GB = branch.PK;
			declaration2.JE_JS = wrapper.Shipment.PK;

			AssertEquals(2, wrapper.GetJobDeclaration().Count());
		}

		public void TestGetExistingCANPermitWhenExemptionExistsAlso()
		{
			var exlvEntryNum = Factory.New<AUCusEntryNumber>();
			exlvEntryNum.CE_EntryIsSystemGenerated = false;
			exlvEntryNum.CE_EntryNum = "";
			exlvEntryNum.CE_EntryType = "EXLV";
			exlvEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			exlvEntryNum.CE_ParentID = wrapper.Shipment.PK;
			exlvEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			exlvEntryNum.CE_Category = "CUS";

			var entryNum = Factory.New<AUCusEntryNumber>();
			entryNum.CE_EntryIsSystemGenerated = false;
			entryNum.CE_EntryNum = "AELRFT77N";
			entryNum.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			entryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			entryNum.CE_ParentID = wrapper.Shipment.PK;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_Category = "CUS";
			Factory.Save();

			// Proof of defect: run GetExistingPermit to see intermittent erroneous results (failures)...
			var specificPermit = wrapper.GetSpecificPermitType(new List<string>(new string[] { CANType.CustomsAuthorityNumber.Code }));
			AssertNotNull(specificPermit);
			AssertEquals(CANType.CustomsAuthorityNumber.Code, specificPermit.CE_EntryType);
			AssertEquals("AELRFT77N", specificPermit.CE_EntryNum);
		}

		public void TestGetSpecificPermitFromList()
		{
			var exlvEntryNum = Factory.New<AUCusEntryNumber>();
			exlvEntryNum.CE_EntryIsSystemGenerated = false;
			exlvEntryNum.CE_EntryNum = "";
			exlvEntryNum.CE_EntryType = "EXLV";
			exlvEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			exlvEntryNum.CE_ParentID = wrapper.Shipment.PK;
			exlvEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			exlvEntryNum.CE_Category = "CUS";

			var entryNum = Factory.New<AUCusEntryNumber>();
			entryNum.CE_EntryIsSystemGenerated = false;
			entryNum.CE_EntryNum = "AELRFT77N";
			entryNum.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			entryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			entryNum.CE_ParentID = wrapper.Shipment.PK;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_Category = "OTH";

			var contingencyCanEntryNum = Factory.New<AUCusEntryNumber>();
			contingencyCanEntryNum.CE_EntryIsSystemGenerated = false;
			contingencyCanEntryNum.CE_EntryNum = "CONTFT77N";
			contingencyCanEntryNum.CE_EntryType = CANType.ContingencyCustomsAuthorityNumber.Code;
			contingencyCanEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			contingencyCanEntryNum.CE_ParentID = wrapper.Shipment.PK;
			contingencyCanEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			contingencyCanEntryNum.CE_Category = "CUS";

			var ecnEntryNum = Factory.New<AUCusEntryNumber>();
			ecnEntryNum.CE_EntryIsSystemGenerated = false;
			ecnEntryNum.CE_EntryNum = "CONTET77N";
			ecnEntryNum.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			ecnEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			ecnEntryNum.CE_ParentID = wrapper.Shipment.PK;
			ecnEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ecnEntryNum.CE_Category = "OTH";

			Factory.Save();

			var specificPermit = wrapper.GetSpecificPermitType(new List<string>(new string[] { CANType.CustomsAuthorityNumber.Code, CANType.ContingencyCustomsAuthorityNumber.Code, CusEntryNumberTypes.Australia.ECN }));
			AssertNotNull(specificPermit);
			AssertEquals(CANType.ContingencyCustomsAuthorityNumber.Code, specificPermit.CE_EntryType);
			AssertEquals("Method should find the only valid CUS permit from the list of codes", "CONTFT77N", specificPermit.CE_EntryNum);
		}

		public void TestGetSpecificPermitParentID()
		{
			AUCusEntryNumber entryNumLinkShipment = Factory.New<AUCusEntryNumber>();
			entryNumLinkShipment.CE_EntryIsSystemGenerated = false;
			entryNumLinkShipment.CE_EntryNum = "JS123";
			entryNumLinkShipment.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			entryNumLinkShipment.CE_ParentTable = CommonShipment.Schema.TableName;
			entryNumLinkShipment.CE_ParentID = wrapper.Shipment.PK;
			entryNumLinkShipment.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumLinkShipment.CE_Category = "OTH";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = wrapper.Shipment.PK;
			AUCusEntryNumber entryNumLinkDeclaration = Factory.New<AUCusEntryNumber>();
			entryNumLinkDeclaration.CE_EntryIsSystemGenerated = false;
			entryNumLinkDeclaration.CE_EntryNum = "JE123";
			entryNumLinkDeclaration.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			entryNumLinkDeclaration.CE_ParentTable = JobDeclaration.Schema.TableName;
			entryNumLinkDeclaration.CE_ParentID = declaration.PK;
			entryNumLinkDeclaration.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumLinkDeclaration.CE_Category = "OTH";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			AUCusEntryNumber entryNumLinkEntryHeader = Factory.New<AUCusEntryNumber>();
			entryNumLinkEntryHeader.CE_EntryIsSystemGenerated = false;
			entryNumLinkEntryHeader.CE_EntryNum = "CH123";
			entryNumLinkEntryHeader.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			entryNumLinkEntryHeader.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumLinkEntryHeader.CE_ParentID = entryHeader.PK;
			entryNumLinkEntryHeader.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumLinkEntryHeader.CE_Category = "OTH";

			Factory.Save();
			AUCusEntryNumber existingPermit = wrapper.GetSpecificPermitType(new List<string>(new string[] { CANType.CustomsAuthorityNumber.Code }));
			AssertNull(existingPermit);

			entryNumLinkShipment.CE_Category = "CUS";
			entryNumLinkDeclaration.CE_Category = "OTH";
			entryNumLinkEntryHeader.CE_Category = "OTH";
			Factory.Save();
			existingPermit = wrapper.GetSpecificPermitType(new List<string>(new string[] { CANType.CustomsAuthorityNumber.Code }));
			AssertEquals("JS123", existingPermit.CE_EntryNum);

			entryNumLinkShipment.CE_Category = "OTH";
			entryNumLinkDeclaration.CE_Category = "CUS";
			entryNumLinkEntryHeader.CE_Category = "OTH";
			Factory.Save();
			existingPermit = wrapper.GetSpecificPermitType(new List<string>(new string[] { CANType.CustomsAuthorityNumber.Code }));
			AssertEquals("JE123", existingPermit.CE_EntryNum);

			entryNumLinkShipment.CE_Category = "OTH";
			entryNumLinkDeclaration.CE_Category = "OTH";
			entryNumLinkEntryHeader.CE_Category = "CUS";
			Factory.Save();
			existingPermit = wrapper.GetSpecificPermitType(new List<string>(new string[] { CANType.CustomsAuthorityNumber.Code }));
			AssertEquals("CH123", existingPermit.CE_EntryNum);
		}

		public void TestGetExistingPermit()
		{
			TestGetExistingPermit(CANType.CustomsAuthorityNumber.Code);
		}

		public void TestGetExistingPermitForCCAN()
		{
			TestGetExistingPermit(CANType.ContingencyCustomsAuthorityNumber.Code);
		}

		public void TestGetExistingPermitForCMRExemptionCode()
		{
			TestGetExistingPermit(CANType.Exemptions.EXSP.Code);
		}

		public void TestESMLineSequence()
		{
			AssertEquals("ESMMainfestLineSequence", null, wrapper.ESMMainfestLineSequence);
			wrapper.ESMLineNumber = 1;
			AssertNotNull("ESMMainfestLineSequence created", wrapper.ESMMainfestLineSequence);
		}

		public void TestESMLineSequenceWhenConsignmentOnMultipleConsols()
		{
			AssertEquals("ESMMainfestLineSequence", null, wrapper.ESMMainfestLineSequence);

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "C00045827";
			var shipment = consol1.Shipments.AddNew();
			Factory.Save();
			AssertEquals("Pre-condition - shipment attached to Consol", shipment.PK, consol1.Shipments[0].PK);

			wrapper = new FreightShipmentWrapper(shipment, consol1);
			wrapper.ESMLineNumber = 5;

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "C00045975";
			consol2.Shipments.Add(shipment);
			Factory.Save();
			AssertEquals("Pre-condition - shipment now attached to second Consol", shipment.PK, consol2.Shipments[0].PK);

			wrapper = new FreightShipmentWrapper(shipment, consol2);
			wrapper.ESMLineNumber = 3;
			AssertNotNull("ESMMainfestLineSequence created", wrapper.ESMMainfestLineSequence);

			wrapper = new FreightShipmentWrapper(shipment, consol1);
			AssertEquals("ESMLineNumber for shipment linked to Consol1 should be 5", 5, wrapper.ESMLineNumber);

			wrapper = new FreightShipmentWrapper(shipment, consol2);
			AssertEquals("ESMLineNumber for shipment linked to Consol2 should be 3", 3, wrapper.ESMLineNumber);
		}

		public void TestESMLineNumber()
		{
			AssertEquals("Pre-condition: ESMLineNumber", 0, wrapper.ESMLineNumber);
			wrapper.ESMLineNumber = 5;
			AssertEquals("ESMLineNumber", 5, wrapper.ESMLineNumber);
		}

		public void TestPreliminaryLineNumberUserUniqueConsolValue()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "C00045827";
			var shipment1 = consol1.Shipments.AddNew();
			var shipment2 = consol1.Shipments.AddNew();
			Factory.Save();
			AssertEquals("Pre-condition - shipment1 attached to Consol", shipment1.PK, consol1.Shipments[0].PK);
			AssertEquals("Pre-condition - shipment2 attached to Consol", shipment2.PK, consol1.Shipments[1].PK);
			AssertEquals("Pre-condition - shipment2.JS_JK_ConsolID", "C00045827", shipment2.JS_JK_ConsolID);

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "C00045975";
			consol2.Shipments.Add(shipment2);
			Factory.Save();
			AssertEquals("Pre-condition - shipment2 now attached to second Consol as well", shipment2.PK, consol2.Shipments[0].PK);
			AssertEquals("Pre-condition - shipment2.JS_JK_ConsolID now is a string of both consols", "C00045827, C00045975", shipment2.JS_JK_ConsolID);

			wrapper = new FreightShipmentWrapper(shipment1, consol1);
			wrapper.CreatePreliminaryManifestLineNumber(5);
			AssertEquals("ESMPreliminaryLineSequence.CY_Data should use Consol1 unique ID", consol1.JK_UniqueConsignRef, wrapper.ESMPreliminaryLineSequence.CY_Data);

			wrapper = new FreightShipmentWrapper(shipment2, consol2);
			wrapper.CreatePreliminaryManifestLineNumber(5);
			AssertEquals("Shipment2 wrapped with Consol2 ESMPreliminaryLineSequence.CY_Data should use Consol2 unique ID", consol2.JK_UniqueConsignRef, wrapper.ESMPreliminaryLineSequence.CY_Data);
		}

		public void TestIsPreliminaryNumberType()
		{
			AssertEquals("IsPreliminaryNumberType", false, wrapper.IsPreliminaryNumberType);
			wrapper.CreatePreliminaryManifestLineNumber(1);
			AssertEquals("IsPreliminaryNumberType", true, wrapper.IsPreliminaryNumberType);
			wrapper.ESMPreliminaryLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.ManifestLineNumber;
			AssertEquals("IsPreliminaryNumberType", false, wrapper.IsPreliminaryNumberType);
		}

		public void TestHasSubManifestLineNumber()
		{
			AssertEquals("HasSubManifestLineNumber", false, wrapper.HasSubManifestLineNumber);
			wrapper.CreatePreliminaryManifestLineNumber(1);
			AssertEquals("HasSubManifestLineNumber", false, wrapper.HasSubManifestLineNumber);
			wrapper.UpdatePreliminaryLineNumberToManifestedLineNumber();
			AssertEquals("HasSubManifestLineNumber", true, wrapper.HasSubManifestLineNumber);
		}

		public void TestCreatePreliminaryManifestLineNumber()
		{
			AssertEquals("LineSequenceCollection count", 0, wrapper.LineSequenceCollection.Count);
			wrapper.CreatePreliminaryManifestLineNumber(5);
			AssertEquals("LineSequenceCollection count", 1, wrapper.LineSequenceCollection.Count);
			AssertEquals("Has PreliminaryLineNumber only", false, wrapper.HasSubManifestLineNumber);

			wrapper.ShipmentManifestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.ManifestLineNumber;
			AssertEquals("Now has SubManifestLineNumber", true, wrapper.HasSubManifestLineNumber);

			wrapper.CreatePreliminaryManifestLineNumber(5); // As in doing an amendment now
			AssertEquals("LineSequenceCollection count - should have Preliminary & Previous Manifested numbers", 2, wrapper.LineSequenceCollection.Count);

			wrapper.CreatePreliminaryManifestLineNumber(5); // As in doing an amendment now
			AssertEquals("Attempting to create PLN again when it already exists should just update the line number  - should still only have Preliminary & Previous Manifested numbers", 2, wrapper.LineSequenceCollection.Count);
		}

		#region Implementation

		void TestGetExistingPermit(ZString entryType)
		{
			AUCusEntryNumber entryNumLinkShipment = Factory.New<AUCusEntryNumber>();
			entryNumLinkShipment.CE_EntryIsSystemGenerated = false;
			entryNumLinkShipment.CE_EntryNum = "JS123";
			entryNumLinkShipment.CE_EntryType = entryType;
			entryNumLinkShipment.CE_ParentTable = CommonShipment.Schema.TableName;
			entryNumLinkShipment.CE_ParentID = wrapper.Shipment.PK;
			entryNumLinkShipment.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumLinkShipment.CE_Category = "OTH";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = wrapper.Shipment.PK;
			AUCusEntryNumber entryNumLinkDeclaration = Factory.New<AUCusEntryNumber>();
			entryNumLinkDeclaration.CE_EntryIsSystemGenerated = false;
			entryNumLinkDeclaration.CE_EntryNum = "JE123";
			entryNumLinkDeclaration.CE_EntryType = entryType;
			entryNumLinkDeclaration.CE_ParentTable = JobDeclaration.Schema.TableName;
			entryNumLinkDeclaration.CE_ParentID = declaration.PK;
			entryNumLinkDeclaration.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumLinkDeclaration.CE_Category = "OTH";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			AUCusEntryNumber entryNumLinkEntryHeader = Factory.New<AUCusEntryNumber>();
			entryNumLinkEntryHeader.CE_EntryIsSystemGenerated = false;
			entryNumLinkEntryHeader.CE_EntryNum = "CH123";
			entryNumLinkEntryHeader.CE_EntryType = entryType;
			entryNumLinkEntryHeader.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumLinkEntryHeader.CE_ParentID = entryHeader.PK;
			entryNumLinkEntryHeader.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumLinkEntryHeader.CE_Category = "OTH";

			Factory.Save();
			AUCusEntryNumber existingPermit = wrapper.GetExistingPermit();
			AssertNull(existingPermit);

			entryNumLinkShipment.CE_Category = "CUS";
			entryNumLinkDeclaration.CE_Category = "OTH";
			entryNumLinkEntryHeader.CE_Category = "OTH";
			Factory.Save();
			existingPermit = wrapper.GetExistingPermit();
			AssertEquals("JS123", existingPermit.CE_EntryNum);

			entryNumLinkShipment.CE_Category = "OTH";
			entryNumLinkDeclaration.CE_Category = "CUS";
			entryNumLinkEntryHeader.CE_Category = "OTH";
			Factory.Save();
			existingPermit = wrapper.GetExistingPermit();
			AssertEquals("JE123", existingPermit.CE_EntryNum);

			entryNumLinkShipment.CE_Category = "OTH";
			entryNumLinkDeclaration.CE_Category = "OTH";
			entryNumLinkEntryHeader.CE_Category = "CUS";
			Factory.Save();
			existingPermit = wrapper.GetExistingPermit();
			AssertEquals("CH123", existingPermit.CE_EntryNum);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var shipment = Factory.New<CommonShipment>();
			wrapper = new FreightShipmentWrapper(shipment, null);
		}

		FreightShipmentWrapper wrapper;
		#endregion
	}
}
