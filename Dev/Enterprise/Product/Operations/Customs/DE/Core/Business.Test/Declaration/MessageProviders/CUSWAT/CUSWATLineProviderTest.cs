using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CUSWATLineProvider))]
	sealed class CUSWATLineProviderTest : ImportDecLineProviderAbstractTest<CUSWATLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSWATLineProvider(null));
		}

		public void TestArticleNumber()
		{
			invoiceLine.JI_PartNo = "ABC123";
			AssertEquals("ABC123", Provider.ArticleNumber);
		}

		public void TestDepartureCountry()
		{
			invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Indonesia;
			AssertEquals(Core.Constants.CountryCodes.Indonesia, Provider.DepartureCountry);
		}

		public void TestDecisiveDate()
		{
			invoiceLine.JI_CustomDate1 = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, Provider.DecisiveDate);
		}

		public void TestDecisiveDate_Null()
		{
			invoiceLine.JI_CustomDate1 = ZDateTime.Empty;
			AssertNull(Provider.DecisiveDate);
		}

		public void TestContainerFlag()
		{
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC00001";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC00002";
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Last().IsForInvoiceLine = true;
			AssertEquals(true, Provider.ContainerFlag);
		}

		public void TestContainerFlag_NoContainerNumber()
		{
			declaration.CusContainers.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().First().IsForInvoiceLine = true;
			AssertEquals("There's a linked container but it doesn't have ContainerNumber", false, Provider.ContainerFlag);
		}

		public void TestContainerFlag_NoLinkedContainers()
		{
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC00001";
			AssertEquals("No container linked to the invoice line", false, Provider.ContainerFlag);
		}

		public void TestContainerFlag_MultipleLinkedContainers()
		{
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC00001";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC00002";
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().ForEach(x => x.IsForInvoiceLine = true);
			AssertEquals("There are multiple containers linked to the invoice line", true, Provider.ContainerFlag);
		}

		public void TestContainerIdentificationNumbers()
		{
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC00001";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC00002";
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Last().IsForInvoiceLine = true;
			AssertArrayEqualsByElements(new[] { "ABC00002" }, Provider.ContainerIdentificationNumbers.ToArray());
		}

		public void TestContainerIdentificationNumbers_NoContainerNumber()
		{
			declaration.CusContainers.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().First().IsForInvoiceLine = true;
			AssertEquals("There's a linked container but it doesn't have ContainerNumber", false, Provider.ContainerIdentificationNumbers.Any());
		}

		public void TestContainerIdentificationNumbers_NoLinkedContainers()
		{
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC00001";
			AssertEquals("No container linked to the invoice line", false, Provider.ContainerIdentificationNumbers.Any());
		}

		public void TestContainerIdentificationNumbers_MultipleLinkedContainers()
		{
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC00001";
			declaration.CusContainers.AddNew();
			declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC00002";
			declaration.CusContainers.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().ForEach(x => x.IsForInvoiceLine = true);
			AssertArrayEqualsByElements("There are multiple containers linked to the invoice line", new[] { "ABC00001", "ABC00002" }, Provider.ContainerIdentificationNumbers.ToArray());
		}

		public void TestInwardMovementAmount()
		{
			invoiceLine.JI_BondedWhsQuantity = 23.5m;
			invoiceLine.JI_BondedWhsUnitQty = "KGMG";
			var amount = Provider.InwardMovementAmount;
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 23.5m, amount.Quantity);
				AssertEquals("MeasurementUnit", "KGM", amount.MeasurementUnit);
				AssertEquals("Qualifier", "G", amount.Qualifier);
			});
		}

		public void TestRequestedPreferentialTreatment()
		{
			invoiceLine.JI_PrimaryPreference = "123";
			AssertEquals("123", Provider.RequestedPreferentialTreatment);
		}

		public void TestInwardMovementDepartureCustomsWarehouseReferenceNumber()
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var doc = entryInstruction.PreviousDocuments.AddNew();
			doc.CSI_ReferenceNumber = "REF001";

			CombineAssertions(() =>
			{
				AssertNull("Prev doc is not linked to invoice line", Provider.InwardMovementDepartureCustomsWarehouseReferenceNumber);

				doc.CSI_ItemNumber = invoiceLine.JI_LineNo;
				AssertEquals("Prev doc is linked to invoice line", "REF001", GetProvider().InwardMovementDepartureCustomsWarehouseReferenceNumber);
			});
		}

		public void TestInwardMovementDepartureCustomsWarehouseSequenceNumber()
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var doc = entryInstruction.PreviousDocuments.AddNew();
			doc.CSI_LineNo = 1;

			CombineAssertions(() =>
			{
				AssertEquals("Prev doc is not linked to invoice line", 0, Provider.InwardMovementDepartureCustomsWarehouseSequenceNumber);

				doc.CSI_ItemNumber = invoiceLine.JI_LineNo;
				AssertEquals("Prev doc is linked to invoice line", 1, GetProvider().InwardMovementDepartureCustomsWarehouseSequenceNumber);
			});
		}

		public void TestInwardMovementDepartureCustomsWarehouseAccessViaAtlasFlag()
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var doc = entryInstruction.PreviousDocuments.AddNew();
			doc.Status = true;

			CombineAssertions(() =>
			{
				AssertEquals("Prev doc is not linked to invoice line", false, Provider.InwardMovementDepartureCustomsWarehouseAccessViaAtlasFlag);

				doc.CSI_ItemNumber = invoiceLine.JI_LineNo;
				AssertEquals("Prev doc is linked to invoice line", true, GetProvider().InwardMovementDepartureCustomsWarehouseAccessViaAtlasFlag);
			});
		}

		public void TestInwardMovementDepartureCustomsWarehouseUsualProcessingFlag()
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var doc = entryInstruction.PreviousDocuments.AddNew();
			doc.UsualProcessingFlag = true;

			CombineAssertions(() =>
			{
				AssertEquals("Prev doc is not linked to invoice line", false, Provider.InwardMovementDepartureCustomsWarehouseUsualProcessingFlag);

				doc.CSI_ItemNumber = invoiceLine.JI_LineNo;
				AssertEquals("Prev doc is linked to invoice line", true, GetProvider().InwardMovementDepartureCustomsWarehouseUsualProcessingFlag);
			});
		}

		public void TestInwardMovementDepartureCustomsWarehouseAdditionalInformation()
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var doc = entryInstruction.PreviousDocuments.AddNew();
			doc.CSI_Description = "Some info";

			CombineAssertions(() =>
			{
				AssertNull("Prev doc is not linked to invoice line", Provider.InwardMovementDepartureCustomsWarehouseAdditionalInformation);

				doc.CSI_ItemNumber = invoiceLine.JI_LineNo;
				AssertEquals("Prev doc is linked to invoice line", "Some info", GetProvider().InwardMovementDepartureCustomsWarehouseAdditionalInformation);
			});
		}

		public void TestInwardMovementDepartureCustomsWarehouseDebitAmount()
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var doc = entryInstruction.PreviousDocuments.AddNew();
			doc.CSI_Quantity2 = 12.5m;
			doc.CSI_UnitOfQuantity2 = "KGMG";

			CombineAssertions(() =>
			{
				AssertAmount("Prev doc is not linked to invoice line", Provider.InwardMovementDepartureCustomsWarehouseDebitAmount, decimal.Zero, string.Empty, string.Empty);

				doc.CSI_ItemNumber = invoiceLine.JI_LineNo;
				AssertAmount("Prev doc is linked to invoice line", GetProvider().InwardMovementDepartureCustomsWarehouseDebitAmount, 12.5m, "KGM", "G");
			});

			void AssertAmount(string message, IAmount amount, decimal expectedQuantity, string expectedMeasurementUnit, string expectedQualifier)
			{
				AssertEquals($"{message}: Quantity", expectedQuantity, amount.Quantity);
				AssertEquals($"{message}: MeasurementUnit", expectedMeasurementUnit, amount.MeasurementUnit);
				AssertEquals($"{message}: Qualifier", expectedQualifier, amount.Qualifier);
			}
		}

		public void TestForeignTradeStatisticsQuantity()
		{
			AssertExceptionThrown<NotSupportedException>(() => _ = Provider.ForeignTradeStatisticsQuantity);
		}

		public void TestForeignTradeStatisticsGrossMassMeasure()
		{
			AssertExceptionThrown<NotSupportedException>(() => _ = Provider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestEntryInstructionPrevDocLinkedToRandomInvoiceLine()
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var doc1 = entryInstruction.PreviousDocuments.AddNew();
			var doc2 = entryInstruction.PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertNull("There are no prev docs linked to the invoice line", new CUSWATLineProvider(entryLine).EntryInstructionPrevDocLinkedToRandomInvoiceLine);

				doc2.CSI_ItemNumber = invoiceLine.JI_LineNo;
				AssertEquals("There's exactly 1 prev doc linked to the invoice line", doc2.PK, new CUSWATLineProvider(entryLine).EntryInstructionPrevDocLinkedToRandomInvoiceLine.PK);

				doc1.CSI_ItemNumber = invoiceLine.JI_LineNo;
				AssertExceptionThrown<InvalidOperationException>("There are multiple prev docs linked to the invoice line", () => _ = new CUSWATLineProvider(entryLine).EntryInstructionPrevDocLinkedToRandomInvoiceLine);
			});
		}

		protected override IEnumerable<Expression<Func<CUSWATLineProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ContainerIdentificationNumbers;
			yield return x => x.InwardMovementAmount;
			yield return x => x.InwardMovementDepartureCustomsWarehouseDebitAmount;
		}

		protected override CUSWATLineProvider GetProvider() => new CUSWATLineProvider(entryLine);

		new ICUSWATLine Provider => base.Provider;
	}
}
