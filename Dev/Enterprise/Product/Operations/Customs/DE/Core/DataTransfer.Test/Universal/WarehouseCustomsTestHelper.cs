using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DE.DataTransfer.Universal.Testing
{
	static class WarehouseCustomsTestHelper
	{
		internal static Shipment Shipment
		{
			get
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = new DataContext
					{
						Company = new Company
						{
							Country = new Country
							{
								Code = Core.Constants.CountryCodes.Germany
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
					},
					TransportMode = new CodeDescriptionPair { Code = "SEA", Description = "Description SEA" },
					PortOfLoading = new UNLOCO { Code = "DEWIB" },
					PortOfFirstArrival = new UNLOCO { Code = "DEHAM" },
				};
				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { Importer, Supplier, Buyer, Seller });
				shipment.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>
				{
					new CustomsSupportingInformation()
					{
						Type = new CodeDescriptionPair6Char() { Code = "C014" },
						DateOfExpiry = ZDateTime.BrettsBirthday,
						ReferenceNumber = "header-support1"
					}
				});
				return shipment;
			}
		}

		internal static CommercialInvoiceHeader Invoice
			=> new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV123",
				InvoiceDate = new ZDateTime(2023, 06, 21),
				IncoTerm = new CodeDescriptionPair() { Code = "FOB", Description = "Description FOB" },
				AdditionalTerms = "Frankfurt",
				ValuationCode = new CodeDescriptionPair() { Code = "21", Description = "Description 21" },
				InvoiceCurrency = new Currency
				{
					Code = Core.Constants.CurrencyCodes.EuropeanUnion
				},
				CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>
				{
					new CustomsSupportingInformation()
					{
						Type = new CodeDescriptionPair6Char() { Code = "C014" },
						DateOfExpiry = ZDateTime.BrettsBirthday,
						ReferenceNumber = "header-support1"
					}
				},
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
				new DataObjectList<CommercialInvoiceLine>(new[]
				{
					InvoiceLine
				})));

		internal static CommercialInvoiceLine InvoiceLine => new CommercialInvoiceLine()
		{
			AddInfoCollection = new List<AddInfo>
			{
				new AddInfo()
				{
					Key = Customs.DataTransfer.Universal.Constants.AddInfoKeys.InvoiceLine.NetPrice,
					Value = "10.3"
				}
			},
			CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>
			{
				new CustomsSupportingInformation()
				{
					Type = new CodeDescriptionPair6Char() { Code = "C015" },
					DateOfExpiry = ZDateTime.BrettsBirthday,
					ReferenceNumber = "line-support1"
				}
			}
		};

		internal static OrganizationAddress Importer
		{
			get
			{
				var importerAddress = new OrganizationAddress
				{
					AddressType = "ImporterDocumentaryAddress",
					OrganizationCode = "123",
					AddressShortCode = "MAI",
					CompanyName = "Importer ORG",
					Address1 = "Teststraße 1",
					Address2 = "Gebaude 1",
					Postcode = "55126",
					City = "Mainz",
					Country = new Country
					{
						Code = "DE",
						Name = "Deutschland"
					},
				};
				importerAddress.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
				return importerAddress;
			}
		}

		internal static OrganizationAddress Supplier
		{
			get
			{
				var supplierAddress = new OrganizationAddress
				{
					AddressType = "SupplierDocumentaryAddress",
					OrganizationCode = "456",
					AddressShortCode = "WIE",
					CompanyName = "Supplier ORG",
					Address1 = "Teststraße 2",
					Address2 = "Gebaude 2",
					Postcode = "65195",
					City = "Wiesbaden",
					Country = new Country
					{
						Code = "DE",
						Name = "Deutschland"
					},
				};
				supplierAddress.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
				return supplierAddress;
			}
		}

		internal static OrganizationAddress Buyer
		{
			get
			{
				var buyerAddress = new OrganizationAddress
				{
					AddressType = Customs.DataTransfer.Universal.Constants.AddressTypes.UltimateConsignee,
					CompanyName = "Buyer ORG",
					AddressShortCode = "KOL",
					OrganizationCode = "678",
					Address1 = "Teststraße 3",
					Address2 = "Gebaude 3",
					Postcode = "65293",
					City = "Koln",
					Country = new Country
					{
						Code = "DE",
						Name = "Deutschland"
					}
				};
				buyerAddress.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
				return buyerAddress;
			}
		}

		internal static OrganizationAddress Seller
		{
			get
			{
				var sellerAddress = new OrganizationAddress
				{
					AddressType = Customs.DataTransfer.Universal.Constants.AddressTypes.Seller,
					OrganizationCode = "098",
					CompanyName = "Seller ORG",
					AddressShortCode = "BON",
					Address1 = "Teststraße 4",
					Postcode = "63453",
					City = "Bonn",
					Country = new Country
					{
						Code = "DE",
						Name = "Deutschland"
					}
				};
				sellerAddress.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
				return sellerAddress;
			}
		}
	}
}
