using System;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE013AndIE015ConsignmentItemProviderTest : Customs.Business.Testing.DataProviderTestCase<IE013AndIE015ConsignmentItemProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsDepartureCargoDesc missing", () => new IE013AndIE015ConsignmentItemProvider(null));
			});
		}
		protected override IE013AndIE015ConsignmentItemProvider GetProvider() => new IE013AndIE015ConsignmentItemProvider(cargoDesc);

		public void TestGoodsItemNumber()
		{
			cargoDesc.BY_LineNo = 1;
			AssertEquals("GoodsItemNumber", (short)1, Provider.GoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			cargoDesc.BY_DeclarationGoodsItemNumber = 3;
			AssertEquals("DeclarationGoodsItemNumber", 3, Provider.DeclarationGoodsItemNumber);
		}

		public void TestDeclarationType()
		{
			cargoDesc.BY_Type = "T1";
			AssertEquals("DeclarationType", "T1", Provider.DeclarationType);
		}

		public void TestCountryOfDispatch()
		{
			cargoDesc.BY_RN_NKCountryOfDispatch = "CN";
			AssertEquals("CountryOfDispatch", "CN", Provider.CountryOfDispatch);
		}

		public void TestCountryOfDestination()
		{
			cargoDesc.BY_RN_NKCountryOfDestination = "AU";
			AssertEquals("CountryOfDestination", "AU", Provider.CountryOfDestination);
		}

		public void TestCountryOfDestinationUseBM_RL_NKDestinationPort()
		{
			cargoDesc.BY_RN_NKCountryOfDestination = ZString.Empty;
			nctsHeader.MovementHeader.BM_RL_NKDestinationPort = "AU";
			AssertEquals("CountryOfDestination", "AU", Provider.CountryOfDestination);
		}

		public void TestReferenceNumberUCR()
		{
			cargoDesc.BY_CommercialReferenceNumber = "REF123";
			AssertEquals("ReferenceNumberUCR", "REF123", Provider.ReferenceNumberUCR);
		}

		public void TestConsignee()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Org2";
			var consignee = cargoDesc.Consignee;
			consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("Consignee", "Org2", Provider.Consignee.Name);
		}

		public void TestAdditionalSupplyChainActors()
		{
			cargoDesc.CusSupplyChainActorReferences.AddNew().CFR_Code = "C1";
			cargoDesc.CusSupplyChainActorReferences.AddNew().CFR_Code = "C2";
			AssertEquals("AdditionalSupplyChainActors", 2, Provider.AdditionalSupplyChainActors.Count);
			var actors = Provider.AdditionalSupplyChainActors.ToArray();
			AssertEquals("Role", "C1", actors[0].Role);
			AssertEquals("Role", "C2", actors[1].Role);
		}

		public void TestCommodity()
		{
			cargoDesc.BY_Description = "Desc";
			AssertNotNull("Commodity", Provider.Commodity);
			AssertEquals("GoodsDescription", "Desc", Provider.Commodity.GoodsDescription);
		}

		public void TestPackages()
		{
			cargoDesc.Packages.AddNew().B5_UnitType = "A";
			cargoDesc.Packages.AddNew().B5_UnitType = "B";
			var packages = Provider.Packages.ToArray();
			AssertEquals("Packages", 2, packages.Length);
			AssertEquals("PackageType", "A", packages[0].PackageType);
			AssertEquals("PackageType", "B", packages[1].PackageType);
		}

		public void TestPreviousDocuments_MergeWhenTransitionPeriod()
		{
			NCTSConditionalFunctionalityTestHelper.CombineAssertionsInPhase5TransitionPeriod(() =>
			{
				cargoDesc.PreviousDocuments.AddNew().CSI_Code = "PRE1";
				cargoDesc.PreviousDocuments.AddNew().CSI_Code = "PRE2";
				bill.PreviousDocuments.AddNew().CSI_Code = "PRE2";
				bill.PreviousDocuments.AddNew().CSI_Code = "PRE4";
				var previousDocuments = Provider.PreviousDocuments.ToArray();
				AssertEquals("PreviousDocuments should have items from both header and item levels.", 3, previousDocuments.Length);
				AssertEquals("PRE1", true, previousDocuments.Any(item => item.Type == "PRE1"));
				AssertEquals("PRE2", true, previousDocuments.Any(item => item.Type == "PRE2"));
				AssertEquals("PRE4", true, previousDocuments.Any(item => item.Type == "PRE4"));
			});
		}

		public void TestPreviousDocuments_ItemLevelOnlyWhenOutOfTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				cargoDesc.PreviousDocuments.AddNew().CSI_Code = "PRE1";
				cargoDesc.PreviousDocuments.AddNew().CSI_Code = "PRE2";
				bill.PreviousDocuments.AddNew().CSI_Code = "PRE2";
				bill.PreviousDocuments.AddNew().CSI_Code = "PRE4";
				var previousDocuments = Provider.PreviousDocuments.ToArray();
				AssertEquals("PreviousDocuments should have items from item levels only.", 2, previousDocuments.Length);
				AssertEquals("Type", "PRE1", previousDocuments[0].Type);
				AssertEquals("Type", "PRE2", previousDocuments[1].Type);
			}
		}

		public void TestSupportingDocuments_MergeWhenTransitionPeriod()
		{
			NCTSConditionalFunctionalityTestHelper.CombineAssertionsInPhase5TransitionPeriod(() =>
			{
				var doc1 = cargoDesc.SupportingDocuments.AddNew();
				doc1.CSI_Code = "C673";
				doc1.CSI_ReferenceNumber = "ABCD1234";
				doc1.CSI_ItemNumber = 1;
				doc1.CSI_ReferenceNumber2 = "Information 1";

				var doc2 = cargoDesc.SupportingDocuments.AddNew();
				doc2.CSI_Code = "C651";
				doc2.CSI_ReferenceNumber = "WXYZ6789";
				doc2.CSI_ItemNumber = 2;
				doc2.CSI_ReferenceNumber2 = "Information 2";

				var doc3 = bill.SupportingDocuments.AddNew();
				doc3.CSI_Code = "C651";
				doc3.CSI_ReferenceNumber = "WXYZ6789";
				doc3.CSI_ItemNumber = 2;
				doc3.CSI_ReferenceNumber2 = "Information 2";

				var doc4 = bill.SupportingDocuments.AddNew();
				doc4.CSI_Code = "C651";
				doc4.CSI_ReferenceNumber = "WXYZ1234";
				doc4.CSI_ItemNumber = 4;
				doc4.CSI_ReferenceNumber2 = "Information 4";

				var provider = GetProvider();

				AssertEquals("Supporting Document Count", 3, provider.SupportingDocuments.Count);
				var documents = provider.SupportingDocuments.ToArray();

				AssertEquals(true, documents.Any(item => item.Type == "C673" && item.Reference == "ABCD1234" && item.LineItemNumber == 1 && item.ComplementOfInformation == "Information 1"));
				AssertEquals(true, documents.Any(item => item.Type == "C651" && item.Reference == "WXYZ6789" && item.LineItemNumber == 2 && item.ComplementOfInformation == "Information 2"));
				AssertEquals(true, documents.Any(item => item.Type == "C651" && item.Reference == "WXYZ1234" && item.LineItemNumber == 4 && item.ComplementOfInformation == "Information 4"));
			});
		}

		public void TestSupportingDocuments_ItemLevelOnlyWhenOutOfTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				cargoDesc.SupportingDocuments.AddNew().CSI_Code = "SUP1";
				cargoDesc.SupportingDocuments.AddNew().CSI_Code = "SUP2";
				bill.SupportingDocuments.AddNew().CSI_Code = "SUP3";
				bill.SupportingDocuments.AddNew().CSI_Code = "SUP4";
				var supportingDocuments = Provider.SupportingDocuments.ToArray();
				AssertEquals("Supporting documents should have items from line level only.", 2, supportingDocuments.Length);
				AssertEquals("SUP1", true, supportingDocuments.Any(item => item.Type == "SUP1"));
				AssertEquals("SUP2", true, supportingDocuments.Any(item => item.Type == "SUP2"));
			}
		}

		public void TestTransportDocuments_MergeWhenTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertNull("Transport Documents", Provider.TransportDocuments.FirstOrDefault());

				SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA1", "TRA_REF");
				SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA2", "TRA_REF");
				SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA2", "TRA_REF");
				SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA3", "TRA_REF");
				SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA4", "TRA_REF1");

				SetupTransportDocument(bill.AdditionalDocuments.AddNew(), "TRA3", "TRA_REF");
				SetupTransportDocument(bill.AdditionalDocuments.AddNew(), "TRA4", "TRA_REF1");
				SetupTransportDocument(bill.AdditionalDocuments.AddNew(), "TRA4", "TRA_REF2");
				SetupTransportDocument(bill.AdditionalDocuments.AddNew(), "TRA5", "TRA_REF");

				var transportDocuments = GetProvider().TransportDocuments.ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("count", 6, transportDocuments.Length);
					AssertDocument(transportDocuments[0], "TRA1", "TRA_REF");
					AssertDocument(transportDocuments[1], "TRA2", "TRA_REF");
					AssertDocument(transportDocuments[2], "TRA3", "TRA_REF");
					AssertDocument(transportDocuments[3], "TRA4", "TRA_REF1");
					AssertDocument(transportDocuments[4], "TRA4", "TRA_REF2");
					AssertDocument(transportDocuments[5], "TRA5", "TRA_REF");
				});
			}
		}

		public void TestTransportDocuments_ItemLevelOnlyWhenOutOfTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA4", "TRA_REF1");
				SetupTransportDocument(cargoDesc.AdditionalInfos.AddNew(), "TRA3", "TRA_REF3");
				SetupTransportDocument(bill.AdditionalDocuments.AddNew(), "TRA3", "TRA_REF3");
				var transportDocuments = GetProvider().TransportDocuments.ToArray();
				AssertEquals("count", 1, transportDocuments.Length);
				AssertDocument(transportDocuments[0], "TRA3", "TRA_REF3");
			}
		}

		void AssertDocument(IDocument document, string expectedType, string expectedReference)
		{
			AssertEquals("Type", expectedType, document.Type);
			AssertEquals("Reference", expectedReference, document.Reference);
		}

		void SetupTransportDocument(AdditionalInfo info, ZString code, ZString referenceNumber)
		{
			info.CSI_SubType = "TRA";
			info.CSI_Code = code;
			info.CSI_ReferenceNumber = referenceNumber;
		}

		public void TestAdditionalReferences_MergeWhenTransitionPeriod()
		{
			NCTSConditionalFunctionalityTestHelper.CombineAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertNull("Additional References", Provider.AdditionalReferences.FirstOrDefault());
				SetupAdditionalDocuments(cargoDesc);
				var additionalReference = GetProvider().AdditionalReferences.FirstOrDefault();
				AssertEquals("Type", "REF1", additionalReference.Type);
				AssertEquals("Reference", "REF_REF", additionalReference.Reference);
			});
		}

		public void TestAdditionalReferences_ItemLevelOnlyWhenOutOfTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				SetupAdditionalDocuments(cargoDesc);

				var billAdditionalReferences = bill.AdditionalDocuments.AddNew();
				billAdditionalReferences.CSI_SubType = "REF";
				billAdditionalReferences.CSI_Code = "REF";
				billAdditionalReferences.CSI_ReferenceNumber = "BILL_REF1";

				var additionalReferences = GetProvider().AdditionalReferences.ToArray();
				AssertEquals("Item level only", 1, additionalReferences.Length);
				var additionalReference = additionalReferences[0];
				AssertEquals("Type", "REF1", additionalReference.Type);
				AssertEquals("Reference", "REF_REF", additionalReference.Reference);
			}
		}

		public void TestAdditionalInformations_MergeWhenTransitionPeriod()
		{
			NCTSConditionalFunctionalityTestHelper.CombineAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertNull("Additional Informations", Provider.AdditionalInformations.FirstOrDefault());
				SetupAdditionalDocuments(cargoDesc);
				var additionalInformation = GetProvider().AdditionalInformations.FirstOrDefault();
				AssertEquals("Code", "INF1", additionalInformation.Code);
				AssertEquals("Text", "INF_REF", additionalInformation.Text);
			});
		}

		public void TestAdditionalInformations_ItemLevelOnlyWhenOutOfTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				SetupAdditionalDocuments(cargoDesc);

				var additionalDocument = bill.AdditionalDocuments.AddNew();
				additionalDocument.CSI_SubType = "INF";
				additionalDocument.CSI_Code = "INF2";
				additionalDocument.CSI_Description = "INF_REF2";

				var additionalInformation = GetProvider().AdditionalInformations.FirstOrDefault();
				AssertEquals("Code", "INF1", additionalInformation.Code);
				AssertEquals("Text", "INF_REF", additionalInformation.Text);
			}
		}

		void SetupAdditionalDocuments(NctsDepartureCargoDesc cargo)
		{
			var additionalDocument = cargo.AdditionalInfos.AddNew();
			additionalDocument.CSI_SubType = "INF";
			additionalDocument.CSI_Code = "INF1";
			additionalDocument.CSI_Description = "INF_REF";
			var additionalReference = cargo.AdditionalInfos.AddNew();
			additionalReference.CSI_SubType = "REF";
			additionalReference.CSI_Code = "REF1";
			additionalReference.CSI_ReferenceNumber = "REF_REF";
		}

		public void TestMethodOfPayment()
		{
			cargoDesc.BY_TransportChargesMethodOfPayment = "1";
			AssertEquals("MethodOfPayment", "1", Provider.MethodOfPayment);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			bill = nctsHeader.Bills.AddNew();
			cargoDesc = bill.GoodsItems.AddNew();
		}

		NctsDepartureCargoDesc cargoDesc;
		NctsBill bill;
		NctsHeader nctsHeader;
	}
}
