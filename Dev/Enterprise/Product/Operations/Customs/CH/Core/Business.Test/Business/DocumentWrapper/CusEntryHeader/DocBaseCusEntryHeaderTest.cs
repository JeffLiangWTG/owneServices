using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class DocBaseCusEntryHeaderTest<T, TWrapper> : DocBaseCusEntryHeaderAbstractTest<T, TWrapper>
	where T : CusEntryHeader where TWrapper : DocBaseCusEntryHeader
{
	public void TestReleaseDate()
	{
		EntryHeaderInternal.CH_EntryReleaseDate = new ZDateTime(2023, 5, 1, 13, 45, 0);
		AssertEquals(new ZDateTime(2023, 5, 1, 13, 45, 0), EntryHeaderWrapperInternal.AcceptanceDateTime);
	}

	public void TestSelectionResult() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateSelectionResultCodeListAndFrenchLanguage(Factory, code: "4", description: "description-4");
		EntryHeaderInternal.MovementReferenceNumberSetter("00CH12345678901234", entryStatus: "4");
		Factory.Save();
		AssertEquals("SelectionResult", "4", EntryHeaderWrapperInternal.SelectionResult);
		AssertEquals("SelectionResultDescription", "description-4", EntryHeaderWrapperInternal.SelectionResultDescription);
	});

	public void TestDeclarationTime()
	{
		RefCusCodeTestHelper.CreateEnsubCodeList(Factory);
		EntryHeaderInternal.EntryInstruction.CEI_SubStyle = "02";
		AssertEquals("Voranmeldung", EntryHeaderWrapperInternal.DeclarationTime);
	}

	public void TestDeclarationType()
	{
		EntryHeaderInternal.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		RefCusCodeTestHelper.CreateEnstyCodeList(Factory);
		EntryHeaderInternal.EntryInstruction.CEI_Style = "01";
		AssertEquals("Definitiv", EntryHeaderWrapperInternal.DeclarationType);
	}

	public void TestTraderReference() => CombineAssertions(() =>
	{
		EntryHeaderInternal.Declaration.JE_DeclarationReference = "B00183715";
		EntryHeaderInternal.Declaration.JE_OwnerRef = "CH000001";

		AssertEquals("TraderReference = JE_OwnerRef not empty", EntryHeaderInternal.Declaration.JE_OwnerRef, EntryHeaderWrapperInternal.TraderReference);

		EntryHeaderInternal.Declaration.JE_OwnerRef = ZString.Empty;
		AssertEquals("TraderReference = JE_DeclarationReference if JE_OwnerRef empty", EntryHeaderInternal.Declaration.JE_DeclarationReference, EntryHeaderWrapperInternal.TraderReference);
	});

	public void TestCurrency()
	{
		var invoice = EntryHeaderInternal.Declaration.Invoices.AddNew();
		invoice.JZ_InvoiceAmount = 99m;
		invoice.JZ_RX_NKInvoice_Currency = "CHF";
		AssertEquals("Currency", "CHF", EntryHeaderWrapperInternal.Currency);
	}

	public void TestCountryOfDestination()
	{
		EntryHeaderInternal.Declaration.JE_GoodsDestination = "CH";
		AssertEquals("CountryOfDestination", "CH", EntryHeaderWrapperInternal.CountryOfDestination);
	}

	public void TestExporter() => CombineAssertions(() =>
	{
		AssertNotNull(EntryHeaderWrapperInternal.Exporter);
		AssertType<DocTraderDataWrapper>(EntryHeaderWrapperInternal.Exporter);
	});

	public void TestConsignor() => CombineAssertions(() =>
	{
		AssertNotNull(EntryHeaderWrapperInternal.Consignor);
		AssertType<DocTraderDataWrapper>(EntryHeaderWrapperInternal.Consignor);
	});

	public virtual void TestConsignee() => CombineAssertions(() =>
	{
		AssertNotNull(EntryHeaderWrapperInternal.Consignee);
		AssertType<DocTraderDataWrapper>(EntryHeaderWrapperInternal.Consignee);
	});

	public void TestAuthorizedConsignee() => CombineAssertions(() =>
	{
		AssertNotNull(EntryHeaderWrapperInternal.AuthorizedConsignee);
		AssertType<DocTraderDataWrapper>(EntryHeaderWrapperInternal.AuthorizedConsignee);
	});

	public void TestDeclarant() => CombineAssertions(() =>
	{
		AssertNotNull(EntryHeaderWrapperInternal.Declarant);
		AssertType<DocTraderDataWrapper>(EntryHeaderWrapperInternal.Declarant);
	});

	public void TestBrokerName() => CombineAssertions(() =>
	{
		var broker = Factory.NewWithValidTestData<GlbStaff>();
		broker.GS_Code = "BR1";

		var externalPassword = Factory.New<GlbExternalPassword_CHD>();
		externalPassword.GP_GS = broker.PK;
		externalPassword.GP_UserID = "901";
		externalPassword.Staff.GS_FullName = "Broker One";

		AssertEquals("No broker", ZString.Empty, EntryHeaderWrapperInternal.BrokerName);

		EntryHeaderInternal.Declaration.JE_GS_NKCusAgent = broker.GS_Code;
		AssertEquals("Broker Name", "Broker One", EntryHeaderWrapperInternal.BrokerName);
	});

	public void TestFinanceData() => CombineAssertions(() =>
	{
		AssertNotNull(EntryHeaderWrapperInternal.FinanceData);
		AssertType<DocFinanceDataWrapper>(EntryHeaderWrapperInternal.FinanceData);
	});

	public void TestTransportationNumber() => CombineAssertions(() =>
	{
		EntryHeaderInternal.Declaration.JE_VoyageFlightNo = ZString.Empty;
		EntryHeaderInternal.Declaration.JE_VesselName = ZString.Empty;

		EntryHeaderInternal.Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		AssertEquals("Air TransportationNumber null", ZString.Empty, EntryHeaderWrapperInternal.TransportationNumber);

		EntryHeaderInternal.Declaration.JE_VoyageFlightNo = "HB123";
		AssertEquals("Air TransportationNumber not null", "HB123", EntryHeaderWrapperInternal.TransportationNumber);

		EntryHeaderInternal.Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
		AssertEquals("Road TransportationNumber null", ZString.Empty, EntryHeaderWrapperInternal.TransportationNumber);

		EntryHeaderInternal.Declaration.JE_VesselName = "BL123456";
		AssertEquals("Road TransportationNumber not null", "BL123456", EntryHeaderWrapperInternal.TransportationNumber);
	});

	public void TestPlaceOfUnloading() => CombineAssertions(() =>
	{
		AssertEquals("PlaceofUnloading missing", ZString.Empty, EntryHeaderWrapperInternal.PlaceOfUnloading);

		EntryHeaderInternal.Declaration.JE_LocationOfGoods = "LOC123";
		AssertEquals("PlaceofUnloading", "LOC123", EntryHeaderWrapperInternal.PlaceOfUnloading);
	});

	public void TestEntryLines() => CombineAssertions(() =>
	{
		EntryHeaderInternal.MergedLines.RemoveAndDeleteAll();
		EntryHeaderInternal.MergedLines.AddNew().CL_LineNumber = 2;
		EntryHeaderInternal.MergedLines.AddNew().CL_LineNumber = 1;
		EntryHeaderInternal.MergedLines.AddNew().CL_LineNumber = 3;

		var entryHeaderWrapper = EntryHeaderWrapperInternal;
		AssertEquals("count", 3, entryHeaderWrapper.EntryLines.Count);
		AssertEntryLine(0);
		AssertEntryLine(1);
		AssertEntryLine(2);

		AssertSame("cached", entryHeaderWrapper.EntryLines, entryHeaderWrapper.EntryLines);

		void AssertEntryLine(int index)
		{
			AssertSame($"EntryLines[{index}].EntryHeader", entryHeaderWrapper, entryHeaderWrapper.EntryLines[index].EntryHeader);
			AssertEquals($"EntryLines[{index}].LineNumber", index + 1, entryHeaderWrapper.EntryLines[index].LineNumber);
		}
	});

	public void TestPreviousDocuments()
	{
		var entryHeaderWrapper = EntryHeaderWrapperInternal;

		CombineAssertions(() =>
		{
			AssertNotNull("PreviousDocuments", entryHeaderWrapper.PreviousDocuments);
			AssertType<DocDocumentCollection>("Collection Type", entryHeaderWrapper.PreviousDocuments);
			AssertEquals("count", 3, entryHeaderWrapper.PreviousDocuments.Count);
		});
	}

	public void TestPrevDocReferences()
	{
		AssertEquals("Previous Docs References", "123 - RST, 456 - UVW, 789 - XYZ", EntryHeaderWrapperInternal.PrevDocReferences);
	}

	void PopulatePreviousDocuments(CusEntryHeader entryHeader)
	{
		var previousDoc1 = entryHeader.EntryInstruction.PreviousDocuments.AddNew();
		previousDoc1.CSI_Code = "123";
		previousDoc1.CSI_ReferenceNumber = "RST";
		var previousDoc2 = entryHeader.EntryInstruction.PreviousDocuments.AddNew();
		previousDoc2.CSI_Code = "456";
		previousDoc2.CSI_ReferenceNumber = "UVW";

		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);

		var previousDoc3 = invoiceHeader.PreviousDocuments.AddNew();
		previousDoc3.CSI_Code = "123";
		previousDoc3.CSI_ReferenceNumber = "RST";
		var previousDoc4 = invoiceHeader.PreviousDocuments.AddNew();
		previousDoc4.CSI_Code = "789";
		previousDoc4.CSI_ReferenceNumber = "XYZ";
	}

	public void TestCustomsOfficeName() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateCustomsOfficesList(Factory);

		AssertEquals("Customs Office missing", ZString.Empty, EntryHeaderWrapperInternal.CustomsOfficeName);

		EntryHeaderInternal.Declaration.JE_CustomsOffice = "CH001251";
		AssertEquals("Customs Office Number", "Allschwil 1", EntryHeaderWrapperInternal.CustomsOfficeName);
	});

	#region Implementation

	protected override T GetNewEntryHeader()
	{
		var entryHeader = base.GetNewEntryHeader();
		entryHeader.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		var invoiceHeader = entryHeader.Declaration.Invoices.AddNew();
		entryHeader.CH_CEI_Instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew().PK;
		PopulatePreviousDocuments(entryHeader);
		return entryHeader;
	}

	protected override string TestingCountry
	{
		get { return Core.Constants.CountryCodes.Switzerland; }
	}

	#endregion
}
