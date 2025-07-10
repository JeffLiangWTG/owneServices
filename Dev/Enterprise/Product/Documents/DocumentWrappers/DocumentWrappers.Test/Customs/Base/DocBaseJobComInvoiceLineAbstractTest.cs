using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestsSubclassesOf(typeof(DocBaseJobComInvoiceLine))]
	public abstract class DocBaseJobComInvoiceLineAbstractTest<T, TWrapper> : DocumentWrapperTestCase
			where T : BaseJobComInvoiceLine
			where TWrapper : DocBaseJobComInvoiceLine
	{
		public void TestToString()
		{
			InvoiceLine.JI_OrderNumber = "Number";
			AssertEquals("ToString()", InvoiceLine.JI_OrderNumber, InvoiceLineWrapper.ToString());
		}

		#region Abstract

		protected abstract TWrapper CreateInvoiceLineWrapper(T invoiceLine);

		#endregion

		#region ZString Fields

		public void TestFormattedInvoiceQuantity()
		{
			AssertEquals("Formatted Invoice Qty to min 2 decimals", "0.00", InvoiceLineWrapper.FormattedInvoiceQuantity);
			InvoiceLine.JI_InvoiceQuantity = 1M;
			AssertEquals("Formatted Invoice Qty to min 2 decimals", "1.00", InvoiceLineWrapper.FormattedInvoiceQuantity);
		}

		public void TestFormattedLinePrice()
		{
			AssertEquals("Formatted Line Price to min 2 decimals", "0.00", InvoiceLineWrapper.FormattedLinePrice);
			InvoiceLine.JI_LinePrice = 1.22M;
			AssertEquals("Formatted Line Price to min 2 decimals", "1.22", InvoiceLineWrapper.FormattedLinePrice); //Line Price can have 2 decimals anyway
		}

		public void TestFormattedUnitPrice()
		{
			AssertEquals("Formatted Unit Price to min 2 decimals", "0.00", InvoiceLineWrapper.FormattedUnitPrice);
			InvoiceLine.JI_InvoiceQuantity = 100.000M;
			InvoiceLine.JI_LinePrice = 1250.00M;
			AssertEquals("Formatted Unit Price to min 2 decimals", "12.50", InvoiceLineWrapper.FormattedUnitPrice);
		}

		public void TestBlankCustomsQty()
		{
			InvoiceLine.JI_CustomsQuantity = 0;
			AssertEquals("Blank Customs Quantity", ZString.Empty, InvoiceLineWrapper.CustomsQty);
		}

		public void TestBlankInvoiceQty()
		{
			InvoiceLine.JI_InvoiceQuantity = 0;
			AssertEquals("Blank Invoice", ZString.Empty, InvoiceLineWrapper.InvoiceQty);
		}

		public void TestTariffLookup()
		{
			var classification = InvoiceLine.Factory.New<BaseCusClassification>();
			classification.CC_Description = "Hello";
			InvoiceLine.JI_CC = classification.PK;
			AssertEquals("Tariff Lookup", "Hello", InvoiceLineWrapper.TariffLookup);
		}

		public void TestTariffLookupCode()
		{
			BaseCusClassification classification = InvoiceLine.Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "SHOES";
			InvoiceLine.JI_CC = classification.PK;
			AssertEquals("Tariff Lookup Code", "SHOES", InvoiceLineWrapper.TariffLookupCode);
		}

		public void TestInvoiceQty()
		{
			InvoiceLine.JI_InvoiceQuantity = 12.32M;
			AssertEquals("Invoice QTY", "12.32", InvoiceLineWrapper.InvoiceQty);

			InvoiceLine.JI_InvoiceQuantity = 12.0M;
			AssertEquals("Invoice QTY", "12.00", InvoiceLineWrapper.InvoiceQty);
		}

		public void TestBondedWarehouseQty()
		{
			InvoiceLine.JI_BondedWhsQuantity = 12.99M;
			AssertEquals("Bonded WHS QTY", "12.99", InvoiceLineWrapper.BondedWarehouseQty);
			InvoiceLine.JI_BondedWhsQuantity = 0M;
			AssertEquals("Bonded WHS QTY", "", InvoiceLineWrapper.BondedWarehouseQty);
		}

		public void TestBondedWarehouseUQ()
		{
			InvoiceLine.JI_BondedWhsUnitQty = "KG";
			AssertEquals("Bonded WHS UQ", "KG", InvoiceLineWrapper.BondedWarehouseUQ);
		}

		public void TestAmountAsString()
		{
			InvoiceLine.JI_InvoiceQuantity = 20.000M;
			InvoiceLine.JI_LinePrice = 5000.00M;

			AssertEquals("5000.00  ", InvoiceLineWrapper.AmountAsString);
			AssertEquals("250.000  ", InvoiceLineWrapper.UnitPriceAsString);
			AssertEquals(250.0000M, InvoiceLineWrapper.UnitPrice);
		}

		public void TestInvoice()
		{
			AssertEquals("Invoice", InvoiceLine.JI_Calc_Invoice, InvoiceLineWrapper.Invoice);
		}

		public void TestAddInfo()
		{
			InvoiceLine.JI_AddInfo = "AddInfo";
			AssertEquals("AddInfo", InvoiceLine.JI_AddInfo, InvoiceLineWrapper.AddInfo);
		}

		public virtual void TestConcessionOrder()
		{
			InvoiceLine.JI_ConcessionOrder = "ConcessionOrder";
			AssertEquals("ConcessionOrder", InvoiceLine.JI_ConcessionOrder, InvoiceLineWrapper.ConcessionOrder);
		}

		public void TestCustomAttrib1()
		{
			InvoiceLine.JI_CustomAttrib1 = "CustomAttrib1";
			AssertEquals("CustomAttrib1", InvoiceLine.JI_CustomAttrib1, InvoiceLineWrapper.CustomAttrib1);
		}

		public void TestCustomAttrib2()
		{
			InvoiceLine.JI_CustomAttrib2 = "CustomAttrib2";
			AssertEquals("CustomAttrib2", InvoiceLine.JI_CustomAttrib2, InvoiceLineWrapper.CustomAttrib2);
		}

		public void TestCustomAttrib3()
		{
			InvoiceLine.JI_CustomAttrib3 = "CustomAttrib3";
			AssertEquals("CustomAttrib3", InvoiceLine.JI_CustomAttrib3, InvoiceLineWrapper.CustomAttrib3);
		}

		public void TestCustomAttrib4()
		{
			InvoiceLine.JI_CustomAttrib4 = "CustomAttrib4";
			AssertEquals("CustomAttrib4", InvoiceLine.JI_CustomAttrib4, InvoiceLineWrapper.CustomAttrib4);
		}

		public void TestCustomsUnitQty()
		{
			InvoiceLine.JI_CustomsUnitQty = "Qty";
			AssertEquals("CustomsUnitQty", InvoiceLine.JI_CustomsUnitQty, InvoiceLineWrapper.CustomsUnitQty);
		}

		public void TestDescription()
		{
			InvoiceLine.JI_Description = "Description";
			AssertEquals("Description", InvoiceLine.JI_Description, InvoiceLineWrapper.Description);
		}

		public void TestOrderNumber()
		{
			InvoiceLine.JI_OrderNumber = "OrderNumber";
			AssertEquals("OrderNumber", InvoiceLine.JI_OrderNumber, InvoiceLineWrapper.OrderNumber);
		}

		public void TestMergedLineNumber()
		{
			AssertEquals("MergedLineNumber", InvoiceLine.MergedLineNumber, InvoiceLineWrapper.MergedLineNumber);
		}

		public void TestPartAttrib1()
		{
			InvoiceLine.JI_PartAttrib1 = "PartAttrib";
			AssertEquals("PartAttrib1", InvoiceLine.JI_PartAttrib1, InvoiceLineWrapper.PartAttrib1);
		}

		public void TestPartAttrib2()
		{
			InvoiceLine.JI_PartAttrib2 = "PartAttrib";
			AssertEquals("PartAttrib2", InvoiceLine.JI_PartAttrib2, InvoiceLineWrapper.PartAttrib2);
		}

		public void TestPartAttrib3()
		{
			InvoiceLine.JI_PartAttrib3 = "PartAttrib";
			AssertEquals("PartAttrib3", InvoiceLine.JI_PartAttrib3, InvoiceLineWrapper.PartAttrib3);
		}

		public void TestSerialNumber()
		{
			InvoiceLine.JI_SerialNumber = "SerialNumber";
			AssertEquals("SerialNumber", InvoiceLine.JI_SerialNumber, InvoiceLineWrapper.SerialNumber);
		}

		public void TestPartNo()
		{
			InvoiceLine.JI_PartNo = "PartNo";
			AssertEquals("PartNo", InvoiceLine.JI_PartNo, InvoiceLineWrapper.PartNo);
		}

		public void TestTariff()
		{
			AssertEquals("Tariff", InvoiceLine.JI_Tariff, InvoiceLineWrapper.Tariff);
		}

		public void TestVolumeUQ()
		{
			InvoiceLine.JI_VolumeUQ = "UQ";
			AssertEquals("VolumeUQ", InvoiceLine.JI_VolumeUQ, InvoiceLineWrapper.VolumeUQ);
		}

		public void TestWeightUQ()
		{
			InvoiceLine.JI_WeightUQ = "UQ";
			AssertEquals("WeightUQ", InvoiceLine.JI_WeightUQ, InvoiceLineWrapper.WeightUQ);
		}

		public void TestInvoiceUQ()
		{
			InvoiceLine.JI_InvoiceUQ = "UQ";
			AssertEquals("InvoiceUQ", InvoiceLine.JI_InvoiceUQ, InvoiceLineWrapper.InvoiceUQ);
		}

		public virtual void TestOriginCode()
		{
			InvoiceLine.JI_CountryOfOrigin = "ZA";
			AssertEquals("Origin", InvoiceLine.JI_CountryOfOrigin, InvoiceLineWrapper.OriginCode);
		}

		public virtual void TestLinePriceCurrencyCode()
		{
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			AssertEquals(InvoiceLineWrapper.LinePriceCurrencyCode, currency.RX_Code);
		}

		public virtual void TestMergedLineNo()
		{
			EntryLine.CL_LineNumber = 5;
			AssertEquals(InvoiceLineWrapper.MergedLineNo, "5");
		}

		public virtual void TestMergedNumericLineNo()
		{
			EntryLine.CL_LineNumber = 5;
			AssertEquals(InvoiceLineWrapper.MergedNumericLineNo, (ZShort)5);
		}

		#endregion

		#region ZDecimal Fields

		public void TestBondedWarehouseQuantity()
		{
			InvoiceLine.JI_BondedWhsQuantity = 123.34M;
			AssertEquals("BondedWarehouseQuantity", InvoiceLine.JI_BondedWhsQuantity, InvoiceLineWrapper.BondedWarehouseQuantity);
		}

		public void TestFOB()
		{
			AssertEquals("FOB", InvoiceLine.JI_Calc_FOB, InvoiceLineWrapper.FOB);
		}

		public void TestCIF()
		{
			AssertEquals("CIF", InvoiceLine.JI_Calc_CIF, InvoiceLineWrapper.CIF);
		}

		public void TestInsuranceInInvoiceCurr()
		{
			AssertEquals("InsuranceInInvoiceCurr", InvoiceLine.JI_Calc_InsuranceInInvoiceCurr, InvoiceLineWrapper.InsuranceInInvoiceCurr);
		}

		public void TestFreightInInvoiceCurr()
		{
			AssertEquals("FreightInInvoiceCurr", InvoiceLine.JI_Calc_FreightInInvoiceCurr, InvoiceLineWrapper.FreightInInvoiceCurr);
		}

		public void TestLinesTotal()
		{
			AssertEquals("LinesTotal", InvoiceLine.JI_Calc_LinesTotal, InvoiceLineWrapper.LinesTotal);
		}

		public void TestLinesEntered()
		{
			AssertEquals("LinesEntered", InvoiceLine.JI_Calc_LinesEntered, InvoiceLineWrapper.LinesEntered);
		}

		public void TestBalance()
		{
			AssertEquals("Balance", InvoiceLine.JI_Calc_Balance, InvoiceLineWrapper.Balance);
		}

		public void TestDutyAmount()
		{
			AssertEquals("DutyAmount", InvoiceLine.JI_Calc_DutyAmount, InvoiceLineWrapper.DutyAmount);
		}

		public void TestGSTVATAmount()
		{
			AssertEquals("GSTVATAmount", InvoiceLine.JI_Calc_GSTVATAmount, InvoiceLineWrapper.GSTVATAmount);
		}

		public void TestUnitPrice()
		{
			AssertEquals("UnitPrice", InvoiceLine.UnitPrice, InvoiceLineWrapper.UnitPrice);
		}

		public void TestCustomDecimal1()
		{
			InvoiceLine.JI_CustomDecimal1 = 123.34M;
			AssertEquals("CustomDecimal1", InvoiceLine.JI_CustomDecimal1, InvoiceLineWrapper.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			InvoiceLine.JI_CustomDecimal2 = 123.34M;
			AssertEquals("CustomDecimal2", InvoiceLine.JI_CustomDecimal2, InvoiceLineWrapper.CustomDecimal2);
		}

		public void TestCustomDecimal3()
		{
			InvoiceLine.JI_CustomDecimal3 = 123.34M;
			AssertEquals("CustomDecimal3", InvoiceLine.JI_CustomDecimal3, InvoiceLineWrapper.CustomDecimal3);
		}

		public void TestCustomsQuantity()
		{
			InvoiceLine.JI_CustomsQuantity = 123.34M;
			AssertEquals("CustomsQuantity", InvoiceLine.JI_CustomsQuantity, InvoiceLineWrapper.CustomsQuantity);
		}

		public void TestLinePrice()
		{
			InvoiceLine.JI_LinePrice = 123.34M;
			AssertEquals("LinePrice", InvoiceLine.JI_LinePrice, InvoiceLineWrapper.LinePrice);
		}

		public void TestVolume()
		{
			InvoiceLine.JI_Volume = 123.34M;
			AssertEquals("Volume", InvoiceLine.JI_Volume, InvoiceLineWrapper.Volume);
		}

		public void TestWeight()
		{
			InvoiceLine.JI_Weight = 123.34M;
			AssertEquals("Weight", InvoiceLine.JI_Weight, InvoiceLineWrapper.Weight);
		}

		public void TestInvoiceQuantity()
		{
			InvoiceLine.JI_InvoiceQuantity = 123.34M;
			AssertEquals("InvoiceQuantity", InvoiceLine.JI_InvoiceQuantity, InvoiceLineWrapper.InvoiceQuantity);
		}

		public void TestUnitPriceInLocalCurrency()
		{
			InvoiceLine.JI_InvoiceQuantity = 10m;
			InvoiceLine.JI_LinePrice = 1500m;
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AssertEquals("Unit price", 150m, InvoiceLine.UnitPriceInLocalCurrency);
			AssertEquals("Unit price in local currency", 150m, InvoiceLineWrapper.UnitPriceInLocalCurrency);
		}

		public void TestDutyPercentBase()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_DutyPercent = 0.5m;
			InvoiceLine.JI_CL = entryLine.PK;
			AssertEquals("Duty percent", 0.5m, InvoiceLineWrapper.DutyPercent);
		}

		public virtual void TestDutyRateDescriptionBase()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var entryLine = Factory.New<CusEntryLine>();
				entryLine.CL_DutyPercent = 0.5m;
				InvoiceLine.JI_CL = entryLine.PK;
				AssertEquals("Duty rate description", "0.50%", InvoiceLineWrapper.DutyRateDescription);
			}
		}

		public void TestVOTI()
		{
			TestVOTICore();
		}

		protected virtual void TestVOTICore()
		{
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_CustomsValue = 100m;
			InvoiceLine.JI_CL = entryLine.PK;
			AssertEquals("VOTI should be Zero", 0m, InvoiceLineWrapper.VOTI);
		}

		#endregion

		#region ZDateTime Fields

		public void TestCustomDate1()
		{
			ZDateTime customDate1 = new ZDateTime(2004, 04, 04);
			InvoiceLine.JI_CustomDate1 = customDate1;
			AssertEquals("CustomDate1", customDate1, InvoiceLineWrapper.CustomDate1);
		}

		public void TestCustomDate2()
		{
			ZDateTime customDate2 = new ZDateTime(2004, 04, 04);
			InvoiceLine.JI_CustomDate2 = customDate2;
			AssertEquals("CustomDate2", customDate2, InvoiceLineWrapper.CustomDate2);
		}

		public void TestCustomDate3()
		{
			ZDateTime customDate3 = new ZDateTime(2004, 04, 04);
			InvoiceLine.JI_CustomDate3 = customDate3;
			AssertEquals("CustomDate3", customDate3, InvoiceLineWrapper.CustomDate3);
		}

		#endregion

		#region Wrapper Fields

		public virtual void TestLinePriceCurr()
		{
			AssertNull("LinePriceCurr", InvoiceLineWrapper.LinePriceCurr);
		}

		public void TestOrderLine()
		{
			AssertNull("OrderLine", InvoiceLineWrapper.OrderLine);

			InvoiceLine.JI_JO = Factory.New(typeof(Enterprise.Freight.Forwarding.Orders.Business.OrderLine)).PK;
			AssertNotNull("OrderLine", InvoiceLineWrapper.OrderLine);
			AssertEquals("OrderLine is of type DocOrderLine", typeof(DocOrderLine), InvoiceLineWrapper.OrderLine.GetType());
		}

		public void TestSupplierPart()
		{
			AssertNull("SupplierPart", InvoiceLineWrapper.SupplierPart);

			InvoiceLine.JI_OP = Enterprise.MasterFiles.Business.OrgSupplierPart.New(Factory).PK;
			AssertNotNull("SupplierPart", InvoiceLineWrapper.SupplierPart);
			AssertEquals("SupplierPart is of type DocOrgSupplierPart", typeof(DocOrgSupplierPart), InvoiceLineWrapper.SupplierPart.GetType());
		}

		public void TestOrigin()
		{
			AssertNull("Origin", InvoiceLineWrapper.Origin);

			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			InvoiceLine.JI_CountryOfOrigin = country.RN_Code;
			AssertNotNull("Origin", InvoiceLineWrapper.Origin);
			AssertEquals("Origin is of type DocCountry", typeof(DocCountry), InvoiceLineWrapper.Origin.GetType());
		}

		public virtual void TestCountryOfOrigin()
		{
			AssertNull("CountryOfOrigin", InvoiceLineWrapper.CountryOfOrigin);

			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			InvoiceLine.JI_CountryOfOrigin = country.RN_Code;
			AssertNotNull("CountryOfOrigin", InvoiceLineWrapper.CountryOfOrigin);
			AssertEquals("CountryOfOrigin is of type DocCountry", typeof(DocCountry), InvoiceLineWrapper.CountryOfOrigin.GetType());
		}

		#endregion

		#region ZBool Fields

		public void TestCustomFlag1()
		{
			InvoiceLine.JI_CustomFlag1 = ZBool.False;
			Assert("!CustomFlag1", !InvoiceLineWrapper.CustomFlag1);

			InvoiceLine.JI_CustomFlag1 = ZBool.True;
			Assert("CustomFlag1", InvoiceLineWrapper.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			InvoiceLine.JI_CustomFlag2 = ZBool.False;
			Assert("!CustomFlag2", !InvoiceLineWrapper.CustomFlag2);

			InvoiceLine.JI_CustomFlag2 = ZBool.True;
			Assert("CustomFlag2", InvoiceLineWrapper.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			InvoiceLine.JI_CustomFlag3 = ZBool.False;
			Assert("!CustomFlag3", !InvoiceLineWrapper.CustomFlag3);

			InvoiceLine.JI_CustomFlag3 = ZBool.True;
			Assert("CustomFlag3", InvoiceLineWrapper.CustomFlag3);
		}

		#endregion

		#region ZShort Fields

		public void TestLineNo()
		{
			InvoiceLine.JI_LineNo = Convert.ToByte(1);
			AssertEquals("LineNo", InvoiceLine.JI_LineNo, InvoiceLineWrapper.LineNo);
		}

		public void TestParentLine()
		{
			InvoiceLine.JI_ParentLine = Convert.ToByte(1);
			AssertEquals("ParentLine", InvoiceLine.JI_ParentLine, InvoiceLineWrapper.ParentLine);
		}

		#endregion

		#region Implementation Methods

		public void TestInvoiceHeaderInternal()
		{
			DocBaseJobComInvoiceLineTestClass invoiceLineWrapper = DocBaseJobComInvoiceLineTestClass.New(InvoiceLine, Factory);
			AssertNotNull("InvoiceHeaderInternal", invoiceLineWrapper.InvoiceHeaderInternalTestMethod);
		}

		public void TestCusEntryLineInternal()
		{
			InvoiceLine.JI_CL = Factory.New(typeof(CusEntryLine)).PK;
			DocBaseJobComInvoiceLineTestClass invoiceLineWrapper = DocBaseJobComInvoiceLineTestClass.New(InvoiceLine, Factory);
			AssertNotNull("CusEntryLine", invoiceLineWrapper.CusEntryLineInternalTestMethod);
		}

		#endregion

		#region Sub Class Tests

		public void TestComInvoiceHeader()
		{
			PropertyInfo property = InvoiceLineWrapper.GetType().GetProperty("ComInvoiceHeader");
			AssertNotNull("You must implement a property call ComInvoiceHeader", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseJobComInvoiceHeader comInvoiceHeader = (DocBaseJobComInvoiceHeader)method.Invoke(InvoiceLineWrapper, Array.Empty<object>());
			AssertNotNull("ComInvoiceHeader is not null", comInvoiceHeader);
			Assert("ComInvoiceHeader is of type DocJobComInvoiceHeader", comInvoiceHeader.GetType().ToString().EndsWith("DocJobComInvoiceHeader"));
		}

		public virtual void TestCusEntryLine()
		{
			PropertyInfo property = InvoiceLineWrapper.GetType().GetProperty("EntryLine");
			AssertNotNull("You must implement a property call EntryLine", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseCusEntryLine entryLine = (DocBaseCusEntryLine)method.Invoke(InvoiceLineWrapper, Array.Empty<object>());
			AssertNotNull("EntryLine is not null", entryLine);
			Assert("EntryLine is of type DocCusEntryLine", entryLine.GetType().ToString().EndsWith("DocCusEntryLine"));
		}

		#endregion

		#region FormatNumberToMinDecimals Tests
		public void TestFormatNumberToMinDecimals()
		{
			DocBaseJobComInvoiceLineTestClass invoiceLineWrapper = DocBaseJobComInvoiceLineTestClass.New(InvoiceLine, Factory);
			AssertEquals("0.00", invoiceLineWrapper.GetFormattedNumberToMinDecimals(0m, 2));
			AssertEquals("1.00", invoiceLineWrapper.GetFormattedNumberToMinDecimals(1m, 2));
			AssertEquals("1.10", invoiceLineWrapper.GetFormattedNumberToMinDecimals(1.1m, 2));
			AssertEquals("1.12", invoiceLineWrapper.GetFormattedNumberToMinDecimals(1.12m, 2));
			AssertEquals("1.123", invoiceLineWrapper.GetFormattedNumberToMinDecimals(1.123m, 2));
			AssertEquals("1.12345", invoiceLineWrapper.GetFormattedNumberToMinDecimals(1.12345m, 2));
			AssertEquals("1.12", invoiceLineWrapper.GetFormattedNumberToMinDecimals(1.12000m, 2));
		}
		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				InvoiceLineWrapper
			};
		}

		#region Implementation

		protected T InvoiceLine;
		protected TWrapper InvoiceLineWrapper
		{
			get { return CreateInvoiceLineWrapper(InvoiceLine); }
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return CreateInvoiceLineWrapper(InvoiceLine);
		}

		protected override void SetUp()
		{
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			InvoiceLine = GetNewInvoiceLine();
			base.SetUp();
		}

		protected virtual T GetNewInvoiceLine()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			EntryLine = entryHeader.MergedLines.AddNew();
			BaseJobComInvoiceLine newInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			newInvoiceLine.JI_CL = EntryLine.PK;
			return (T)newInvoiceLine;
		}

		protected CusEntryLine EntryLine;

		#endregion
	}
}
