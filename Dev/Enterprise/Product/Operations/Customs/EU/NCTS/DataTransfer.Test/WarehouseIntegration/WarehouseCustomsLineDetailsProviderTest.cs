using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.WarehouseIntegration.Testing
{
	sealed class WarehouseCustomsLineDetailsProviderTest : TestCaseWithFactory
	{
		public void TestIWarehouseCustomsLineDetailsMembers()
		{
			CombineAssertions(() =>
			{
				var contextMock = new Mock<IDataContextDataObject>();
				contextMock.Setup(e => e.DataSourceCollection).Returns(
					new List<IDataSourceDataObject>()
					{
						new DataSource() { Type = nameof(DataContextType.NctsHeader) }
					});
				contextMock.Setup(m => m.CountryCodeToImportInto).Returns(Core.Constants.CountryCodes.Latvia);
				var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProvider(Shipment);
				AssertData(provider.GetLineDetails().ToArray());
				AssertData(Shipment.GetWarehouseCustomsLineDetails(contextMock.Object).ToArray());
				contextMock.VerifyAll();
			});
		}

		void AssertData(IWarehouseCustomsLineDetails[] lines)
		{
			AssertEquals(2, lines.Length);
			var line1 = lines[0];
			AssertEquals("ENT328434", line1.EntryNumber);
			AssertNull(line1.PreviousEntryLineNumber);
			AssertNull(line1.PreviousEntryNumber);
			AssertEquals(InvoiceLine1, line1.InvoiceLine);
			var line1PackDetails = line1.PackDetails.ToArray();
			AssertEquals(0, line1PackDetails.Length);

			var line2 = lines[1];
			AssertEquals((ZShort?)1, line2.EntryLineNumber);
			AssertEquals("ENT328434", line2.EntryNumber);
			AssertNull(line2.PreviousEntryLineNumber);
			AssertNull(line2.PreviousEntryNumber);
			AssertEquals(InvoiceLine2, line2.InvoiceLine);
			var line2PackDetails = line2.PackDetails.ToArray();
			AssertEquals(0, line2PackDetails.Length);
		}

		Shipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						DataContext = new DataContext
						{
							Company = new Company
							{
								Country = new Country
								{
									Code = Core.Constants.CountryCodes.France
								}
							}
						},
						CommercialInfo = new CommercialInfo()
						{
							Name = "GROUPINV",
							CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
							{
								Invoice
							})
						}
					};
				}
				return shipment;
			}
		}
		Shipment shipment;

		CommercialInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV123",
						InvoiceCurrency = new Currency
						{
							Code = Core.Constants.CurrencyCodes.EuropeanUnion
						},
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<CommercialInvoiceLine>(new[]
						{
							InvoiceLine1, InvoiceLine2
						})));
				}
				return invoice;
			}
		}
		CommercialInvoiceHeader invoice;

		CommercialInvoiceLine InvoiceLine1
		{
			get
			{
				if (invoiceLine1 == null)
				{
					invoiceLine1 = new CommercialInvoiceLine()
					{
						LineNo = 1,
						EntryLineNumber = 2,
						EntryNumber = "ENT328434",
						Description = "DONGS",
						BondedWarehouseQuantity = 56,
						CommercialChargeCollection = new List<CommercialCharge>(new[]
						{
							new CommercialCharge()
							{
								ChargeType = new CodeDescriptionPair() { Code = "CH1" },
								Currency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia },
								Amount = new ZDecimal(1100m),
								IsDutiable = ZBool.True,
								IsGSTApplicable = ZBool.False,
								IsStatisticalValueApplicable = ZBool.True
							},
							new CommercialCharge()
							{
								ChargeType = new CodeDescriptionPair() { Code = "CH2" },
								Currency = new Currency() { Code = Core.Constants.CurrencyCodes.NewZealand },
								Amount = new ZDecimal(2200m),
								IsDutiable = ZBool.False,
								IsGSTApplicable = ZBool.True,
								IsStatisticalValueApplicable = ZBool.False
							}
						})
					};
				}
				return invoiceLine1;
			}
		}
		CommercialInvoiceLine invoiceLine1;

		CommercialInvoiceLine InvoiceLine2
		{
			get
			{
				if (invoiceLine2 == null)
				{
					invoiceLine2 = new CommercialInvoiceLine()
					{
						LineNo = 2,
						EntryNumber = "ENT328434",
						EntryLineNumber = 1,
						Description = "BOOKS",
						BondedWarehouseQuantity = 69,
						CommercialChargeCollection = new List<CommercialCharge>(new[]
						{
							new CommercialCharge()
							{
								ChargeType = new CodeDescriptionPair() { Code = "CH3" },
								Currency = new Currency() { Code = Core.Constants.CurrencyCodes.UnitedStates },
								Amount = new ZDecimal(3300m),
								IsDutiable = ZBool.True,
								IsGSTApplicable = ZBool.True,
								IsStatisticalValueApplicable = ZBool.False
							}
						})
					};
				}
				return invoiceLine2;
			}
		}
		CommercialInvoiceLine invoiceLine2;
	}
}
