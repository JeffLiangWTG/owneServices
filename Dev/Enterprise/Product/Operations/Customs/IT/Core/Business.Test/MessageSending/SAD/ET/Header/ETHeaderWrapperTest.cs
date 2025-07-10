using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class ETHeaderWrapperAbstactTest<T> : SADHeaderCommonWrapperTest<T>
	where T : IETHeader
{
	public void TestHeaderDataDeclaredOnItems()
	{
		CombineAssertions("Assert HeaderDataDeclaredOnItems", () =>
		{
			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
			AssertEquals("When ParticipantType is not Buyer's consol, HeaderDataDeclaredOnItems", false, sadHeaderWrapper.HeaderDataDeclaredOnItems);

			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
			AssertEquals("When ParticipantType is Buyer's consol but related Invoices do not have the Supplier, HeaderDataDeclaredOnItems", true, sadHeaderWrapper.HeaderDataDeclaredOnItems);
		});
	}

	public override void TestCountryOfDispatch()
	{
		var orgHeader1 = Factory.New<OrgHeader>();
		orgHeader1.MainAddress.OA_RN_NKCountryCode = "ES";
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceHeader1 = jobDeclaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_CL = entryLine1.PK;

		CombineAssertions("entry instruction participants", () =>
		{
			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
			AssertEquals("JE_RL_NKOrigin not set - when participants is not buyers' console", "", sadHeaderWrapper.CountryOfDispatch);

			jobDeclaration.JE_RL_NKOrigin = "IT";
			AssertEquals("JE_RL_NKOrigin set - when participants is not buyers' console", "IT", sadHeaderWrapper.CountryOfDispatch);

			jobDeclaration.JE_RL_NKOrigin = string.Empty;
			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
			entryHeader.ResetInvoiceHeadersAndLines();
			entryLine1.InvoiceLines.Reload(true);
			sadHeaderWrapper = GetHeaderWrapper(entryHeader);
			AssertEquals("When participants is buyers' console but there are no Supplier", ZString.Empty, sadHeaderWrapper.CountryOfDispatch);

			invoiceHeader1.JZ_OH_Supplier = orgHeader1.PK;
			entryHeader.ResetInvoiceHeadersAndLines();
			entryLine1.InvoiceLines.Reload(true);
			sadHeaderWrapper = GetHeaderWrapper(entryHeader);
			AssertEquals("when participants is buyers' console", "ES", sadHeaderWrapper.CountryOfDispatch);
		});
	}

	public override void TestCountryOfDestination()
	{
		var refUNLOCO = Factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "FIN";
		refUNLOCO.RL_RN_NKCountryCode = "IT";
		AssertEquals(ZString.Empty, sadHeaderWrapper.CountryOfDestination);
		jobDeclaration.JE_RL_NKFinalDestination = "FIN";
		AssertEquals(jobDeclaration.JE_GoodsDestination, sadHeaderWrapper.CountryOfDestination);
	}

	public void TestMeansOfTransportAtDeparture()
	{
		jobDeclaration.ZG_Box18TransportNationality = "";
		jobDeclaration.ZG_Box18TransportID = "";
		var meansOfTransportOnArrival = sadHeaderWrapper.MeansOfTransportAtDeparture;
		AssertEquals(ZString.Empty, meansOfTransportOnArrival.Nationality);
		AssertEquals(ZString.Empty, meansOfTransportOnArrival.Identity);

		jobDeclaration.ZG_Box18TransportNationality = "KR";
		jobDeclaration.ZG_Box18TransportID = "RX2839A";
		meansOfTransportOnArrival = sadHeaderWrapper.MeansOfTransportAtDeparture;
		AssertEquals("KR", meansOfTransportOnArrival.Nationality);
		AssertEquals("RX2839A", meansOfTransportOnArrival.Identity);
	}

	public void TestMeansOfTransportCrossingBorder()
	{
		CombineAssertions(() =>
		{
			var meansOfTransportCrossingBorder = sadHeaderWrapper.MeansOfTransportCrossingBorder;
			AssertNotNull(meansOfTransportCrossingBorder);
			AssertType<ETHeaderMeansOfTransportCrossingBorderWrapper>(meansOfTransportCrossingBorder);
		});

		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, sadHeaderWrapper.MeansOfTransportCrossingBorder.Nationality);

			jobDeclaration.JE_RN_NKTransportNationality = "SG";
			AssertEquals("SG", sadHeaderWrapper.MeansOfTransportCrossingBorder.Nationality);
			AssertEquals(ZString.Empty, sadHeaderWrapper.MeansOfTransportCrossingBorder.Identity);

			jobDeclaration.JE_TransportMode = "SEA";
			jobDeclaration.JE_VesselName = "SE";
			jobDeclaration.JE_VoyageFlightNo = "3434";
			AssertEquals("SE3434", sadHeaderWrapper.MeansOfTransportCrossingBorder.Identity);

			jobDeclaration.JE_TransportMode = "AIR";
			jobDeclaration.JE_VesselName = "AE";
			jobDeclaration.JE_VoyageFlightNo = "4343";
			AssertEquals("4343", sadHeaderWrapper.MeansOfTransportCrossingBorder.Identity);

			jobDeclaration.JE_TransportMode = "ROA";
			jobDeclaration.JE_VesselName = "SE";
			jobDeclaration.JE_VoyageFlightNo = "3434";
			AssertEquals("SE", sadHeaderWrapper.MeansOfTransportCrossingBorder.Identity);

			jobDeclaration.JE_TransportMode = "";
			jobDeclaration.JE_VesselName = "SE";
			jobDeclaration.JE_VoyageFlightNo = "3434";
			AssertEquals("SE", sadHeaderWrapper.MeansOfTransportCrossingBorder.Identity);
		});
	}

	public void TestDialogLanguageIndicatorAtDeparture()
	{
		AssertEquals(ZString.Empty, sadHeaderWrapper.DialogLanguageIndicatorAtDeparture);
	}

	public override void TestIsContainerizedTransport()
	{
		jobDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
		AssertEquals("When JE_ContainerMode is FCL", true, sadHeaderWrapper.IsContainerizedTransport);
		jobDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
		AssertEquals("When JE_ContainerMode is LCL", true, sadHeaderWrapper.IsContainerizedTransport);
		jobDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.ULD;
		AssertEquals("When JE_ContainerMode is ULD", true, sadHeaderWrapper.IsContainerizedTransport);
		jobDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
		AssertEquals("When JE_ContainerMode is CNT", true, sadHeaderWrapper.IsContainerizedTransport);

		jobDeclaration.JE_ContainerMode = ZString.Empty;
		AssertEquals("When JE_ContainerMode is empty", false, sadHeaderWrapper.IsContainerizedTransport);
		jobDeclaration.JE_ContainerMode = "XYZ";
		AssertEquals("When JE_ContainerMode is an invalid value", false, sadHeaderWrapper.IsContainerizedTransport);
	}

	public override void TestTermsOfDelivery()
	{
		CombineAssertions(() =>
		{
			var termsOfDelivery = sadHeaderWrapper.TermsOfDelivery;
			AssertNotNull(termsOfDelivery);
			AssertType<SADTermsOfDeliveryWrapper>(termsOfDelivery);
		});
	}

	public override void TestTransactionData()
	{
		CombineAssertions(() =>
		{
			var transactionData = sadHeaderWrapper.TransactionData;
			AssertNotNull(transactionData);
			AssertType<SADTransactionDataWrapper>(transactionData);
		});
	}

	public override void TestTransportModeAtBorder()
	{
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, sadHeaderWrapper.TransportModeAtBorder);
			jobDeclaration.JE_TransportMode = "SEA";
			AssertEquals("1", sadHeaderWrapper.TransportModeAtBorder);
			jobDeclaration.JE_TransportMode = "RAI";
			AssertEquals("2", sadHeaderWrapper.TransportModeAtBorder);
			jobDeclaration.JE_TransportMode = "ROA";
			AssertEquals("3", sadHeaderWrapper.TransportModeAtBorder);
			jobDeclaration.JE_TransportMode = "AIR";
			AssertEquals("4", sadHeaderWrapper.TransportModeAtBorder);
			jobDeclaration.JE_TransportMode = "MAI";
			AssertEquals("5", sadHeaderWrapper.TransportModeAtBorder);
			jobDeclaration.JE_TransportMode = "FIX";
			AssertEquals("7", sadHeaderWrapper.TransportModeAtBorder);
			jobDeclaration.JE_TransportMode = "IWT";
			AssertEquals("8", sadHeaderWrapper.TransportModeAtBorder);
			jobDeclaration.JE_TransportMode = "OWN";
			AssertEquals("9", sadHeaderWrapper.TransportModeAtBorder);
		});
	}

	public override void TestInlandTransportMode()
	{
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, sadHeaderWrapper.InlandTransportMode);
			jobDeclaration.JE_TransportModeInland = "SEA";
			AssertEquals("1", sadHeaderWrapper.InlandTransportMode);
			jobDeclaration.JE_TransportModeInland = "RAI";
			AssertEquals("2", sadHeaderWrapper.InlandTransportMode);
			jobDeclaration.JE_TransportModeInland = "ROA";
			AssertEquals("3", sadHeaderWrapper.InlandTransportMode);
			jobDeclaration.JE_TransportModeInland = "AIR";
			AssertEquals("4", sadHeaderWrapper.InlandTransportMode);
			jobDeclaration.JE_TransportModeInland = "MAI";
			AssertEquals("5", sadHeaderWrapper.InlandTransportMode);
			jobDeclaration.JE_TransportModeInland = "FIX";
			AssertEquals("7", sadHeaderWrapper.InlandTransportMode);
			jobDeclaration.JE_TransportModeInland = "IWT";
			AssertEquals("8", sadHeaderWrapper.InlandTransportMode);
			jobDeclaration.JE_TransportModeInland = "OWN";
			AssertEquals("9", sadHeaderWrapper.InlandTransportMode);
		});
	}

	public void TestExitCustomsOffice()
	{
		AssertEquals(ZString.Empty, sadHeaderWrapper.ExitCustomsOffice);
		jobDeclaration.CustomsOffices.RemoveAndDeleteAll();

		var exitCustomsOffice1 = Factory.New<EuOfficeCode>();
		jobDeclaration.CustomsOffices.Add(exitCustomsOffice1);
		exitCustomsOffice1.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
		exitCustomsOffice1.CY_Data = "IT303199";
		AssertEquals("IT303199", sadHeaderWrapper.ExitCustomsOffice);

		var exitCustomsOffice2 = Factory.New<EuOfficeCode>();
		jobDeclaration.CustomsOffices.Add(exitCustomsOffice2);
		exitCustomsOffice2.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
		exitCustomsOffice2.CY_Data = "IT000000";
		AssertEquals("IT303199", sadHeaderWrapper.ExitCustomsOffice);
	}

	public void TestAgreedLocationOfGoods()
	{
		CombineAssertions(() =>
		{
			var agreedLocationOfGoods = sadHeaderWrapper.AgreedLocationOfGoods;
			AssertNotNull(agreedLocationOfGoods);
			AssertType<ETHeaderAgreedLocationOfGoodsWrapper>(agreedLocationOfGoods);
		});
	}

	public void TestSeals()
	{
		AssertArrayEqualsByElements(Array.Empty<ZString>(), sadHeaderWrapper.Seals.ToArray());

		var cnt1 = AddNewDeclarationContainerWithSeals("A", "B");
		var cnt2 = AddNewDeclarationContainerWithSeals("C", "B");
		var cnt3 = AddNewDeclarationContainerWithSeals("D", "E");
		entryInstruction.Seals.AddNew().CY_Data = "";
		entryInstruction.Seals.AddNew().CY_Data = "F";
		entryInstruction.Seals.AddNew().CY_Data = "D";

		var invoiceLine = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(cnt1).IsForInvoiceLine = true;
		invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(cnt2).IsForInvoiceLine = true;
		invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(cnt3).IsForInvoiceLine = true;

		AssertArrayEqualsByElements(new ZString[] { "F", "D", "A", "B", "C", "E" }, sadHeaderWrapper.Seals.ToArray());

		EU.Business.Declaration.CusContainer AddNewDeclarationContainerWithSeals(ZString seal, ZString secondSeal)
		{
			var container = jobDeclaration.CusContainers.AddNew();
			container.CO_Seal = seal;
			container.CO_SecondSeal = secondSeal;
			return container;
		}
	}

	public void TestControlResult()
	{
		CombineAssertions(() =>
		{
			var controlResult = sadHeaderWrapper.ControlResult;
			AssertNotNull(controlResult);
			AssertType<ETHeaderControlResultWrapper>(controlResult);
		});
	}

	protected override Type GetExpectedDeclarationType() => typeof(ETDeclarationWrapper);

	public override void TestWarehouseIdentification()
	{
		CombineAssertions(() =>
		{
			var warehouseIdentification = sadHeaderWrapper.WarehouseIdentification;
			AssertNotNull(warehouseIdentification);
			AssertType<SADEmptyWarehouseIdentificationWrapper>(warehouseIdentification);
		});
	}
}

sealed class ETHeaderWrapperBaseOnlyTest : ETHeaderWrapperAbstactTest<ETHeaderWrapper>
{
	public void TestSecurityData()
	{
		AssertNull(sadHeaderWrapper.SecurityData);
	}

	public void TestPrincipalTrader()
	{
		CombineAssertions(() =>
		{
			var principalTrader = sadHeaderWrapper.PrincipalTrader;
			AssertNotNull(principalTrader);
			AssertType<ETHeaderPrincipalTraderWrapper>(principalTrader);
		});
	}

	public void TestSecurityBlock()
	{
		CombineAssertions(() =>
		{
			var securityBlock = sadHeaderWrapper.SecurityBlock;
			AssertNotNull(securityBlock);
			AssertType<ETHeaderSecurityBlockWrapper>(securityBlock);
		});
	}

	public void TestTransitCustomsOffices()
	{
		CombineAssertions(() =>
		{
			var transitCustomsOffices = sadHeaderWrapper.TransitCustomsOffices;
			AssertNotNull(transitCustomsOffices);
			AssertEquals(0, transitCustomsOffices.Count());
		});
	}

	public void TestGuarantees()
	{
		CombineAssertions(() =>
		{
			var guarantees = sadHeaderWrapper.Guarantees;
			AssertNotNull(guarantees);
			AssertEquals(0, guarantees.Count());
		});
	}

	public void TestDestinationCustomsOffice()
	{
		AssertEquals(ZString.Empty, sadHeaderWrapper.DestinationCustomsOffice);
	}

	protected override ETHeaderWrapper GetHeaderWrapper(CusEntryHeader entryHeader) => new ETHeaderWrapper(entryHeader);
}
