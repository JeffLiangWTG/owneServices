using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using BondedWarehousingHelper = Enterprise.Customs.EU.Business.BondedWarehousingHelper;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing
{
	public class WarehouseCustomsLineDetailsTest : TestCaseWithFactory
	{
		public void TestDeclarationEntryStyle()
		{
			var invoiceLine = new CommercialInvoiceLine();
			var whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertNull("DeclarationEntryStyle should be null if shipment.MessageSubType is not provided.", whsLineDetails.DeclarationEntryStyle);

			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine, shipment => shipment.MessageSubType = new CodeDescriptionPair() { Code = "IM", Description = "IntoWarehouse" } );
			AssertEquals("DeclarationEntryStyle should be captured from shipment.MessageSubType.", "IM", whsLineDetails.DeclarationEntryStyle);
		}

		public void TestEntryInstructionSubStyle()
		{
			var invoiceLine = new CommercialInvoiceLine();
			var whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertNull("EntryInstructionSubStyle should be null as no entry instruction can be retrieved.", whsLineDetails.EntryInstructionSubStyle);

			invoiceLine.EntryInstructionLink = 1;
			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertNull("EntryInstructionSubStyle should be null as no entry instruction can be retrieved.", whsLineDetails.EntryInstructionSubStyle);

			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine, shipment => { shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>() { new EntryInstruction { Link = 1, SubStyle = new CodeDescriptionPair { Code = "A", Description = "Normal Procedure Article" } } }); });
			AssertEquals("EntryInstructionSubStyle should be captured from the linked entryInstruction.CEI_SubStyle", "A", whsLineDetails.EntryInstructionSubStyle);
		}

		public void TestPreviousDocumentNature()
		{
			var invoiceLine = new CommercialInvoiceLine();
			var whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertNull("PreviousDocumentNature should be null as no CustomsSupportingInformation is provided.", whsLineDetails.PreviousDocumentNature);

			invoiceLine.CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>();
			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertNull("PreviousDocumentNature should be null as CustomsSupportingInformation is empty.", whsLineDetails.PreviousDocumentNature);

			invoiceLine.CustomsSupportingInformationCollection.Add(
				new CustomsSupportingInformation
				{
					Category = new CodeDescriptionPair { Code = "SUP", Description = "Supporting Document" },
					Type = new CodeDescriptionPair6Char { Code = "N001", Description = "Doc Example" },
				});
			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertNull("PreviousDocumentNature should be null as CustomsSupportingInformation contains no previous document.", whsLineDetails.PreviousDocumentNature);

			invoiceLine.CustomsSupportingInformationCollection.Add(
				new CustomsSupportingInformation
				{
					Category = new CodeDescriptionPair { Code = "PRE", Description = "Previous Document" },
					Type = new CodeDescriptionPair6Char { Code = "N002", Description = "Doc Example2" },
				});
			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertEquals("PreviousDocumentNature can be captured from the only one previous document.", "N002", whsLineDetails.PreviousDocumentNature);

			invoiceLine.CustomsSupportingInformationCollection.Add(
				new CustomsSupportingInformation
				{
					Category = new CodeDescriptionPair { Code = "PRE", Description = "Previous Document" },
					Type = new CodeDescriptionPair6Char { Code = "N003", Description = "Doc Example3" },
				});
			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine, shipment => shipment.MessageSubType = new CodeDescriptionPair() { Code = "IM", Description = "IntoWarehouse" });
			AssertEquals("PreviousDocumentNature can be captured from shipment.MessageSubType as multiple previous documents are provided.", "IM", whsLineDetails.PreviousDocumentNature);
		}

		public void TestPreviousDocumentReference()
		{
			var invoiceLine = new CommercialInvoiceLine();
			var whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertNull("PreviousDocumentReference should be null as no CustomsSupportingInformation is provided.", whsLineDetails.PreviousDocumentReference);

			invoiceLine.CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>();
			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertNull("PreviousDocumentReference should be null as CustomsSupportingInformation is empty.", whsLineDetails.PreviousDocumentReference);

			invoiceLine.CustomsSupportingInformationCollection.Add(
				new CustomsSupportingInformation
				{
					Category = new CodeDescriptionPair { Code = "SUP", Description = "Supporting Document" },
					ReferenceNumber = "REF0001"
				});
			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertNull("PreviousDocumentReference should be null as CustomsSupportingInformation contains no previous document.", whsLineDetails.PreviousDocumentReference);

			invoiceLine.CustomsSupportingInformationCollection.Add(
				new CustomsSupportingInformation
				{
					Category = new CodeDescriptionPair { Code = "PRE", Description = "Previous Document" },
					ReferenceNumber = "REF0002"
				});
			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertEquals("PreviousDocumentReference can be captured from the only one previous document.", "REF0002", whsLineDetails.PreviousDocumentReference);

			invoiceLine.CustomsSupportingInformationCollection.Add(
				new CustomsSupportingInformation
				{
					Category = new CodeDescriptionPair { Code = "PRE", Description = "Previous Document" },
					ReferenceNumber = "REF0003"
				});
			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine);
			AssertEquals("PreviousDocumentReference can be captured from shipment.MessageSubType as multiple previous documents are provided.", "REFERENCES MULTIPLES", whsLineDetails.PreviousDocumentReference);
		}

		public void TestEstimatedBreakdownData()
		{
			var commercialInvoiceLine = new CommercialInvoiceLine
			{
				EntryLineNumber = 1,
				BondedWarehouseQuantity = 1,
				EntryInstructionLink = 1,
				Procedure = "7100000",
			};

			var whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(commercialInvoiceLine);
			var additionalAddInfos = whsLineDetails.AddInfos;

			Assert("EstimatedDutyBreakdown should not be available when commercialInvoiceLine EstimatedDutyBreakdown add info null or empty.", !additionalAddInfos.Contains("EstimatedDutyBreakdown"));
			Assert("EstimatedVATBreakdown should not be available when commercialInvoiceLine EstimatedDutyBreakdown add info null or empty.", !additionalAddInfos.Contains("EstimatedVATBreakdown"));
			Assert("EstimatedOtherTaxesBreakdown should not be available when commercialInvoiceLine EstimatedDutyBreakdown add info null or empty.", !additionalAddInfos.Contains("EstimatedOtherTaxesBreakdown"));

			commercialInvoiceLine.AddInfoCollection = new List<UniversalAddInfo>(new[]
			{
					new UniversalAddInfo() { Key = BondedWarehousingHelper.Constants.EstimatedDutyBreakdown, Value = "15.00" },
					new UniversalAddInfo() { Key = BondedWarehousingHelper.Constants.EstimatedVATBreakdown, Value = "10.00" },
					new UniversalAddInfo() { Key = BondedWarehousingHelper.Constants.EstimatedOtherTaxesBreakdown, Value = "99.00" },
			});

			whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(commercialInvoiceLine);
			additionalAddInfos = whsLineDetails.AddInfos;
			AssertContains("EstimatedDutyBreakdown=15.00*EstimatedVATBreakdown=10.00*EstimatedOtherTaxesBreakdown=99.00", additionalAddInfos);
		}

		public void TestIsOutward()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var outwardProcedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "42", "71", "C33", "", "IMP", "42P");
			outwardProcedure.ZZ6_IntoWarehouse = "N";
			outwardProcedure.ZZ6_OutOfWarehouse = "Y";

			var inwardProcedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "10", "71", "F61", "", "IMP", "10P");
			inwardProcedure.ZZ6_IntoWarehouse = "Y";
			inwardProcedure.ZZ6_OutOfWarehouse = "N";
			Factory.Save();

			var invoiceLine = new CommercialInvoiceLine
			{
				EntryLineNumber = 1,
				BondedWarehouseQuantity = 1,
				EntryInstructionLink = 1,
				Procedure = "4271C33"
			};

			var shipment = GetOutlineShipmentByCommercialInvoiceLine(invoiceLine);

			var fallbackDetail1 = new WarehouseCustomsFallbackDetailWithEntryInstruction
			{
				CountryCode = Core.Constants.CountryCodes.France,
				EntryInstructionProcedureMap = new Dictionary<ZInt, ZString>
				{
					{ 1, "42" }
				}
			};
			var lineDetails1 = new WarehouseCustomsLineDetailsForTest(Factory, invoiceLine, fallbackDetail1, shipment);
			AssertEquals("4271C33 is outward.", true, lineDetails1.IsOutward);

			var fallbackDetail2 = new WarehouseCustomsFallbackDetailWithEntryInstruction
			{
				CountryCode = Core.Constants.CountryCodes.France,
				EntryInstructionProcedureMap = new Dictionary<ZInt, ZString>
				{
					{ 1, "42P" }
				}
			};
			var lineDetails2 = new WarehouseCustomsLineDetailsForTest(Factory, invoiceLine, fallbackDetail2, shipment);
			AssertEquals("4271C33 is outward.", true, lineDetails2.IsOutward);

			var fallbackDetail3 = new WarehouseCustomsFallbackDetailWithEntryInstruction
			{
				CountryCode = Core.Constants.CountryCodes.France,
				EntryInstructionProcedureMap = new Dictionary<ZInt, ZString>
				{
					{ 1, "10" }
				}
			};
			invoiceLine.Procedure = "1071F61";
			var lineDetails3 = new WarehouseCustomsLineDetailsForTest(Factory, invoiceLine, fallbackDetail3, shipment);
			AssertEquals("1071F61 is not outward.", false, lineDetails3.IsOutward);
		}

		public void TestCustomsRelatedAddInfo()
		{
			const int entryInstructionLink = 1;
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceLine.EntryInstructionLink = entryInstructionLink;
			invoiceLine.SetCustomsSupportingInformationCollection(
				() => new List<CustomsSupportingInformation> {
					new CustomsSupportingInformation() {
						Category = new CodeDescriptionPair() { Code = "PRE" },
						Type = new CodeDescriptionPair6Char() { Code = "AAA" },
						ReferenceNumber = new ZString("111")
					} } );

			var whsLineDetails = GetWarehouseCustomsLineDetailsWithDefaultFallback(invoiceLine, shipment =>
			{
				shipment.GoodsDestination = new ZString("FR");

				var entryInstruction = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance) { Link = entryInstructionLink, Style = "AA" };
				entryInstruction.SetCustomsReferenceCollection(
					() => new List<CustomsReference> {
						new CustomsReference() { Type = new CodeDescriptionPair() { Code = "AUT" }, Reference = "222" } });
				shipment.SetEntryInstructionCollection(() => new List<EntryInstruction> { entryInstruction } );

				var entryHeader = new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { EntryStatus = new EntryStatus() { Code = "101", Description = "XYZ" }, EntryInstructionLink = entryInstructionLink };
				entryHeader.SetAddInfoCollection(
					() => new List<UniversalAddInfo> {
						new UniversalAddInfo() { Key = "GuaranteedAmount", Value = "99" },
						new UniversalAddInfo() { Key = "ENTRYSTATUSDESCRIPTION", Value = "BAE" },
						new UniversalAddInfo() { Key = "EXITDATE", Value = "11/01/2020" } });
				shipment.SetEntryHeaderCollection(() => new List<EntryHeader> { entryHeader });
			});

			var additionalAddInfos = whsLineDetails.AddInfos;
			AssertContains("PREVIOUSDOCCODE=AAA*PREVIOUSDOCREFERENCE=111*COUNTRYOFDESTINATION=FR*GUARANTEEDAMOUNT=99*EXITDATE=11/01/2020*ENTRYSTATUS=XYZ*ENTRYSTATUSDESCRIPTION=BAE*AUTHORIZATION=222", additionalAddInfos);
		}

		WarehouseCustomsLineDetails GetWarehouseCustomsLineDetailsWithDefaultFallback(CommercialInvoiceLine commercialInvoiceLine, Action<Shipment> customizedModification = null)
		{
			var shipment = GetOutlineShipmentByCommercialInvoiceLine(commercialInvoiceLine);
			customizedModification?.Invoke(shipment);

			var fallbackDetail1 = new WarehouseCustomsFallbackDetailWithEntryInstruction { };
			var whsLineDetails = new WarehouseCustomsLineDetails(Factory, commercialInvoiceLine, fallbackDetail1, shipment);

			return whsLineDetails;
		}

		Shipment GetOutlineShipmentByCommercialInvoiceLine(CommercialInvoiceLine commercialInvoiceLine)
		{
			var dataContext = GetDefaultDataContext();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>(new []
								{
									commercialInvoiceLine,
								})))
						})
				},
			};

			return shipment;
		}

		IDataContextDataObject GetDefaultDataContext()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			return dataContext;
		}
	}

	class WarehouseCustomsLineDetailsForTest : WarehouseCustomsLineDetails
	{
		public WarehouseCustomsLineDetailsForTest(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, Customs.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail, Shipment shipment) : base(factory, invoiceLine, fallbackDetail, shipment)
		{
		}

		public new bool IsOutward => base.IsOutward;
	}
}
