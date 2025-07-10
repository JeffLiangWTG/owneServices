using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class AllocationCodeRetrieverTest : TestCaseWithDocumentFactory
	{
		public void TestRefType()
		{
			AllocationCodeRetriever retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV S00001001]");
			AssertEquals("Should have parsed reftype", Core.Constants.DocManagerCodes.Shipment, retriever.RefType);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV ]");
			AssertEquals("Should still parse reftype without all the info in the file or email subject line", Core.Constants.DocManagerCodes.Shipment, retriever.RefType);

			retriever = new AllocationCodeRetriever(MasterFactory, "[SHP CIV S00001001]");
			AssertEquals("Should parse the reftype without the ediDocManager tag at the front", Core.Constants.DocManagerCodes.Shipment, retriever.RefType);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager] SHP CIV S00001001");
			AssertEquals("Should not parse the reftype if it is outside of square brackets", "", retriever.RefType);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager]");
			AssertEquals("reftype should return empty", true, retriever.RefType.IsEmpty);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager] ZZZ");
			AssertEquals("reftype should return empty - nonexistent type", true, retriever.RefType.IsEmpty);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV S00001001] moo");
			AssertEquals("Should have parsed reftype", Core.Constants.DocManagerCodes.Shipment, retriever.RefType);

			retriever = new AllocationCodeRetriever(MasterFactory, "FW: [ediDocManager SHP CIV S00001001] moo");
			AssertEquals("Should have parsed reftype", Core.Constants.DocManagerCodes.Shipment, retriever.RefType);

			retriever = new AllocationCodeRetriever(MasterFactory, "FW: [ediDocManager SHP CIV4 S00001001]");
			AssertEquals("Should have parsed reftype", Core.Constants.DocManagerCodes.Shipment, retriever.RefType);
		}

		public void TestRefTypeIsCaseInsensitive()
		{
			var retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager shp CIV S00001001]");
			AssertEquals("Should have parsed reftype", Core.Constants.DocManagerCodes.Shipment, retriever.RefType);
		}

		public void TestDocType()
		{
			AllocationCodeRetriever retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV S00001001]");
			AssertEquals("Should have parsed DocType", "CIV", retriever.DocType);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV ]");
			AssertEquals("Should still parse DocType without all the info in the file or email subject line", "CIV", retriever.DocType);

			retriever = new AllocationCodeRetriever(MasterFactory, "[SHP CIV S00001001]");
			AssertEquals("Should parse the DocType without the ediDocManager tag at the front", "CIV", retriever.DocType);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager] SHP CIV S00001001");
			AssertEquals("Should not parse the DocType if it is outside of square brackets", "", retriever.DocType);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager]");
			AssertEquals("DocType should return empty", true, retriever.DocType.IsEmpty);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV S00001001] moo");
			AssertEquals("Should have parsed reftype", "CIV", retriever.DocType);

			retriever = new AllocationCodeRetriever(MasterFactory, "FW: [ediDocManager SHP CIV S00001001] moo");
			AssertEquals("Should have parsed reftype", "CIV", retriever.DocType);

			retriever = new AllocationCodeRetriever(MasterFactory, "FW: [ediDocManager SHP CIV S00001001]");
			AssertEquals("Should have parsed reftype", "CIV", retriever.DocType);
		}

		public void TestRefPK()
		{
			ZQuery query = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001001");
			BusinessObject shipmentS00001001 = (BusinessObject)MasterFactory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(query);

			if (shipmentS00001001 == null)
			{
				shipmentS00001001 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
				shipmentS00001001[JobShipmentSchema.JS_UniqueConsignRef] = "S00001001";
			}

			MasterFactory.Save();

			AllocationCodeRetriever retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV S00001001]");
			AssertEquals("Should have parsed RefPK", shipmentS00001001.PK, retriever.RefPK);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV ]");
			AssertEquals("RefPK will return empty beacuse there isn't enough information to find the PK", ZGuid.Empty, retriever.RefPK);

			retriever = new AllocationCodeRetriever(MasterFactory, "[SHP CIV S00001001]");
			AssertEquals("Should parse the RefPK without the ediDocManager tag at the front", shipmentS00001001.PK, retriever.RefPK);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager] SHP CIV S00001001");
			AssertEquals("Should not parse the RefPK if it is outside of square brackets", ZGuid.Empty, retriever.RefPK);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager]");
			AssertEquals("RefPK should return empty", true, retriever.RefPK.IsEmpty);

			retriever = new AllocationCodeRetriever(MasterFactory, "SHP CIV S12345678");
			AssertEquals("RefPK should return empty because we couldn't find the matching shipment", true, retriever.RefPK.IsEmpty);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "AAA BBB  CCC";
			Factory.Save();

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager ORG POA AAA BBB  CCC ]");
			AssertEquals("Should have parsed RefPK", org.PK, retriever.RefPK);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager ORG POA AAA BBB]  CCC");
			AssertEquals("Should not have parsed RefPK", ZGuid.Empty, retriever.RefPK);

			retriever = new AllocationCodeRetriever(MasterFactory, "FW: [ediDocManager ORG POA AAA BBB  CCC ]");
			AssertEquals("Should have parsed RefPK", org.PK, retriever.RefPK);

			retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager ORG POA AAA BBB  CCC ] qwerty");
			AssertEquals("Should have parsed RefPK", org.PK, retriever.RefPK);
		}

		public void TestVisibleCompanyBranchDepartmentCode()
		{
			AllocationCodeRetriever retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV S00001001]");
			AssertEquals(ZGuid.Empty, retriever.VisibleInfo.VisibleCompanyPK);

			var company = Factory.LoadTop1<GlbCompany>(new ZQuery());
			retriever = new AllocationCodeRetriever(MasterFactory, string.Format("[ediDocManager SHP CIV S00001001 C:{0}]", company.GC_Code));
			AssertEquals("Should have parsed VisibleCompanyPK", company.PK, retriever.VisibleInfo.VisibleCompanyPK);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "AAA BBB  CCC";
			Factory.Save();

			retriever = new AllocationCodeRetriever(MasterFactory, string.Format("[ediDocManager ORG POA AAA BBB  CCC C:{0}]", company.GC_Code));
			AssertEquals("Should have parsed RefPK", org.PK, retriever.RefPK);
			AssertEquals("Should have parsed VisibleCompanyPK", company.PK, retriever.VisibleInfo.VisibleCompanyPK);

			retriever = new AllocationCodeRetriever(MasterFactory, string.Format("[ediDocManager ORG POA AAA BBB  CCC C:{0} B:{1} D:{2} S:DAA]", company.GC_Code,
				GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code));
			AssertEquals("Should have parsed RefPK", org.PK, retriever.RefPK);
			AssertEquals("Should have parsed VisibleCompanyPK", company.PK, retriever.VisibleInfo.VisibleCompanyPK);
			AssertEquals("Should have parsed VisibleBranchPK", GlbBranch.CurrentBranch.PK, retriever.VisibleInfo.VisibleBranchPK);
			AssertEquals("Should have parsed VisibleDepartmentPK", GlbDepartment.CurrentDepartment.PK, retriever.VisibleInfo.VisibleDepartmentPK);
			AssertEquals("Should have parsed DocSource", "DAA", retriever.DocSource);

			string expectedMessage = "The visible company code '***' specified in email subject is invalid.";

			AssertExceptionThrown(typeof(IncorrectVisibleCompanyBranchDepartmentException), expectedMessage,
				() =>
				{
					retriever = new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV S11111 C:***]", true);
				});

			expectedMessage =
				@"The visible branch code '@@@' specified in email subject is invalid.
The visible department code '###' specified in email subject is invalid.";

			AssertExceptionThrown(typeof(IncorrectVisibleCompanyBranchDepartmentException), expectedMessage,
				() =>
				{
					retriever = new AllocationCodeRetriever(MasterFactory, string.Format("[ediDocManager SHP CIV S11111 C:{0} B:@@@ D:###]", company.GC_Code), true);
				});
		}

		public void TestInfoTooLongForFieldsDocType()
		{
			bool exceptionCaught = false;
			try
			{
				// codes in wrong order
				AllocationCodeRetriever retriever = new AllocationCodeRetriever(MasterFactory, "[SHP S00001001 CIV]");
			}
			catch (AllocationCodeFormatException ex)
			{
				exceptionCaught = true;
				Assert("Should say warning about the doc type code", ex.Message.IndexOf("DocType: S00001001") != -1);
			}

			AssertEquals("Exception should have been thrown for catching further up the stack", true, exceptionCaught);
		}

		public void TestInfoTooLongForFields()
		{
			bool exceptionCaught = false;
			try
			{
				// codes in wrong order
				AllocationCodeRetriever retriever = new AllocationCodeRetriever(MasterFactory, "[S00001001 SHP CIV]");
			}
			catch (AllocationCodeFormatException ex)
			{
				exceptionCaught = true;
				Assert("Should say warning about the reference code", ex.Message.IndexOf("Reference code: S00001001") != -1);
			}

			AssertEquals("Exception should have been thrown for catching further up the stack", true, exceptionCaught);
		}

		public void TestThrowExceptionOnInvalidPK()
		{
			bool exceptionCaught = false;
			try
			{
				AllocationCodeRetriever retriever = new AllocationCodeRetriever(MasterFactory, "[SHP CIV S12345678]", true);
			}
			catch (AllocationCodeFormatException ex)
			{
				exceptionCaught = true;
				Assert("Should say warning about invalid uniqueid", ex.Message.IndexOf("unique ID") != -1);
			}

			AssertEquals("Exception should have been thrown", true, exceptionCaught);
		}

		[ExpectNoExceptions]
		public void TestDoNotThrowExceptionOnInvalidPK()
		{
			AllocationCodeRetriever retriever = new AllocationCodeRetriever(MasterFactory, "[SHP CIV S12345678]", false);
		}

		public void TestProcessAllocationInfo_ShouldThrowWhenNoMatchingCompanyFound()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DEA";
			Factory.Save();

			AssertExceptionThrown(typeof(AllocationCodeFormatException), () => new AllocationCodeRetriever(MasterFactory, "[ediDocManager BOO:SHP CIV S12345678]", false));
			AssertNoExceptionThrown(() => new AllocationCodeRetriever(MasterFactory, "[ediDocManager SHP CIV S12345678]", false));
			AssertExceptionThrown(typeof(AllocationCodeFormatException), () => new AllocationCodeRetriever(MasterFactory, "[ediDocManager DEA:SHP CIV S12345678]", false));
		}
	}
}
