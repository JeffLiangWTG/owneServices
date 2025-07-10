using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureMovementHeader))]
	sealed class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAllowPermitProcessingMembers()
		{
			var (header, departureMovement) = GetNewBusinessObject(Factory);
			var iAllowPermitProcessing = departureMovement as IAllowPermitProcessing;
			AssertEquals(header.PermitValueDecimalPlaceCount, iAllowPermitProcessing.PermitValueDecimalPlaceCount);
			AssertEquals(5, iAllowPermitProcessing.PermitQuantityDecimalPlaceCount);
			AssertTotalNumberOfPackages(header, departureMovement);
			departureMovement.BM_PaperlessInbondNum = "1";
			AssertEquals("1", iAllowPermitProcessing.GetPermitReference());
			AssertEquals(0, iAllowPermitProcessing.GetPermitReferenceNumberLine());
			AssertGetPermitRecords();
			header.LocalReferenceNumber = "LRN123";
			AssertEquals("NCTS departure LRN123", iAllowPermitProcessing.GetPermitComment(null));
		}

		void AssertTotalNumberOfPackages(NctsHeader header, NctsDepartureMovementHeader movementHeader)
		{
			var bulkType = Factory.SetupBulkCusCode();
			var bill = header.Bills.AddNew();
			var goodsItem1 = bill.GoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 20;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_UnitCount = 30;
			var goodsItem2 = bill.GoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 50;

			var goodsItem3 = bill.GoodsItems.AddNew();
			var package4 = goodsItem3.Packages.AddNew();
			package4.B5_UnitCount = 0;
			package4.B5_UnitType = bulkType;

			var goodsItem4 = bill.GoodsItems.AddNew();
			var package5 = goodsItem4.Packages.AddNew();
			package5.B5_UnitCount = 10;
			package5.B5_UnitType = bulkType;
			AssertEquals(102, (movementHeader as IAllowPermitProcessing).PackageCount);
			AssertEquals((header as IAllowPermitProcessing).PackageCount, (movementHeader as IAllowPermitProcessing).PackageCount);
		}

		void AssertGetPermitRecords()
		{
			var (cusGuaranteeHeader, nctsGuarantee) = NctsTestDataProvider.CreateNctsGuaranteeWithCusGuaranteeHeader(Factory);
			var nctsHeader = nctsGuarantee.NctsHeader;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "LRN123";
			AssertPermitRecords(movementHeader, 1, "NCTS Guarantee added");

			var unlinkedCusGuaranteeHeader = NctsTestDataProvider.CreateCusGuaranteeHeader(Factory, "1234", ZGuid.Empty);
			unlinkedCusGuaranteeHeader.AddTransaction("LRN123", "Test 1", "1234", "", -10000, 0, "PND");
			Factory.Save();
			AssertPermitRecords(movementHeader, 2, "Transaction matches LRN");

			unlinkedCusGuaranteeHeader.AddTransaction("LRN456", "Test 1", "1234", "", -10000, 0, "PND");
			Factory.Save();
			AssertPermitRecords(movementHeader, 2, "2nd transaction does not match LRN");
		}

		void AssertPermitRecords(NctsDepartureMovementHeader movementHeader, int expectedCount, string message = "")
		{
			var records = (movementHeader as IAllowPermitProcessing).GetPermitRecords();
			AssertEquals(message, expectedCount, records.Count);
		}

		public void TestIMessageAttacheeMembers()
		{
			var (header, departureMovement) = GetNewBusinessObject(Factory);
			IMessageAttachee messageAttachee = departureMovement;
			AssertEquals("PK", departureMovement.PK, messageAttachee.PK);
			AssertEquals("TableName", departureMovement.TableName, messageAttachee.TableName);
			AssertEquals("TablePrefix", departureMovement.TablePrefix, messageAttachee.TablePrefix);
			var branch = header.Company.Branches.AddNew();
			header.BH_GB = branch.PK;
			AssertSame("Branch", branch, messageAttachee.Branch);
			AssertNull("CustomsAgent", messageAttachee.CustomsAgent);
			AssertSame("RelatedJob", header, messageAttachee.RelatedJob);
			AssertSame("Factory", Factory, messageAttachee.Factory);
			departureMovement.BM_MessageStatus = "SNT";
			AssertEquals("LogicalStatus", "SNT", messageAttachee.LogicalStatus);
			departureMovement.BM_CustomsStatus = "CLR";
			AssertEquals("EntryStatus", "CLR", messageAttachee.EntryStatus);
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";
			AssertEquals("MovementReferenceNumber", "MRN123", messageAttachee.MovementReferenceNumber);
			departureMovement.Messages.AddNew();
			departureMovement.Messages.AddNew();
			AssertContainsExactElementsInAnyOrder("Messages", departureMovement.Messages, messageAttachee.Messages);
		}

		public void TestMessages()
		{
			var (header, departureMovement) = GetNewBusinessObject(Factory);
			var messages = departureMovement.Messages;
			AssertEquals(departureMovement, messages.Master);
		}

		public void TestLookups()
		{
			(_, var departureMovement) = GetNewBusinessObject(Factory);
			AssertType<NctsDepartureMovementHeaderLookups>(departureMovement.Lookups);
		}

		public void TestValidation()
		{
			(_, var departureMovement) = GetNewBusinessObject(Factory);
			AssertType<NctsDepartureMovementHeaderValidation>(departureMovement.Validation);
		}

		public void TestGoodsLocation()
		{
			AssertType<CusGoodsLocation>(GetNewBusinessObject(Factory).departureMovement.GoodsLocation);
		}

		public void TestBM_TransportAtDeparture_Uppercase()
		{
			(_, var departureMovement) = GetNewBusinessObject(Factory);
			departureMovement.BM_TransportAtDeparture = "aBc";
			AssertEquals(departureMovement.BM_TransportAtDeparture, "ABC");
		}

		public void TestBM_TransportAtDepartureTrailer1RegNo_Uppercase()
		{
			(_, var departureMovement) = GetNewBusinessObject(Factory);
			departureMovement.BM_TransportAtDepartureTrailer1RegNo = "aBc";
			AssertEquals(departureMovement.BM_TransportAtDepartureTrailer1RegNo, "ABC");
		}

		public void TestBM_TransportAtDepartureTrailer2RegNo_Uppercase()
		{
			(_, var departureMovement) = GetNewBusinessObject(Factory);
			departureMovement.BM_TransportAtDepartureTrailer2RegNo = "aBc";
			AssertEquals(departureMovement.BM_TransportAtDepartureTrailer2RegNo, "ABC");
		}

		public void TestBM_TOLCarrierID_Uppercase()
		{
			(_, var departureMovement) = GetNewBusinessObject(Factory);
			departureMovement.BM_TOLCarrierID = "aBc";
			AssertEquals(departureMovement.BM_TOLCarrierID, "ABC");
		}

		public void TestGoodsItems()
		{
			var header = NctsHeaderTest.GetNewBusinessObject(Factory);
			var movementHeader = header.MovementHeader;
			AssertType<NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(movementHeader.GoodsItems);
		}

		public void TestBM_ExportTransportMode_Phase5()
		{
			var header = NctsHeaderTest.GetNewBusinessObject(Factory);
			var movementHeader = header.MovementHeader;
			NCTSTestHelper.AssertCaptionsAndFullDescription(movementHeader.BM_ExportTransportModeInfo, NctsHeader.Phase5CaptionKey, "Transport Border (Active Border)", string.Empty, "Transport Border", "[19 03 001 000] Transport Mode at Border");
		}

		public void TestBM_ActiveBorderIdentificationType_Phase5()
		{
			var header = NctsHeaderTest.GetNewBusinessObject(Factory);
			var movementHeader = header.MovementHeader;
			NCTSTestHelper.AssertCaptionsAndFullDescription(movementHeader.BM_ActiveBorderIdentificationTypeInfo, NctsHeader.Phase5CaptionKey, "Type of Identification", "Type of ID", string.Empty, "[19 08 061 000] Type of Identification");
		}

		public void TestDefaultDepartureLocationCodeFromCusAuthorisationIfBlank()
		{
			var (_, departureMovement) = GetNewBusinessObject(Factory);
			departureMovement.IsSimplifiedNctsProcedure = true;

			var header = departureMovement.Header;
			header.Principal.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			var configuration = header.Configuration.LocationOfGoodsFromAuthorisationDefaulterConfiguration;
			CombineAssertions(() =>
			{
				AssertEquals("Pre-requisite: Configuration IsDefaultingEnabled", expected: true, configuration.IsDefaultingEnabled);
				AssertEquals("Pre-requisite: Configuration QualifierCode", CusGoodsLocationQualifierList.Codes.UnLocode, configuration.QualifierCode);
				AssertEquals("Pre-requisite: Configuration TypeCode", CusGoodsLocationTypeList.Codes.AuthorizedPlace, configuration.TypeCode);
			});

			var cusAuthorisationUsage = departureMovement.CusAuthorizationUsages.AddNew();
			cusAuthorisationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			cusAuthorisationUsage.AGC_Number = "AR10001";
			cusAuthorisationUsage.AGC_OH_Owner = GlbCompany.CurrentCompany.OrgProxy.PK;

			var cusAuthorisationHeader = Factory.New<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			cusAuthorisationHeader.CPH_Number = "AR10001";
			cusAuthorisationHeader.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;

			var goodsLocation = departureMovement.GoodsLocation;

			var rule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			rule.CPR_ValueFrom = "A000";
			departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
			AssertDepartureGoodsLocation(goodsLocation, configuration.QualifierCode, configuration.TypeCode, rule.CPR_ValueFrom);
		}

		void AssertDepartureGoodsLocation(CusGoodsLocation goodsLocation, ZString qualifier, ZString type, ZString code, bool isCustomsOffice = false)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Goods Location Qualifier", qualifier, goodsLocation.CGL_Qualifier);
				AssertEquals("Goods Location Type", type, goodsLocation.CGL_Type);
				if (isCustomsOffice)
				{
					AssertEquals("Goods Location Customs Office", code, goodsLocation.CGL_CustomsOffice);
				}
				else
				{
					AssertEquals("Goods Location UNLOCODE", code, goodsLocation.CGL_AdditionalIdentifier);
				}

				var displayText = new ZStringBuilder().AppendIfNotEmpty(qualifier).AppendIfNotEmpty(type).AppendIfNotEmpty(code).ToStringWithDelimiterBetweenAppends(";").TrimEnd(';');
				AssertEquals("Goods Location Display Text", displayText, goodsLocation.DisplayText);
			});
		}

		public void TestGuarantees()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertType<NctsGuaranteeCollection<NctsGuarantee>>(nctsHeader.MovementHeader.Guarantees);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).departureMovement;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).departureMovement;

		public static (NctsHeader header, NctsDepartureMovementHeader departureMovement) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = NctsHeaderTest.GetNewBusinessObject(factory);
			var departureMovement = header.MovementHeader;
			return (header, departureMovement);
		}
	}
}
