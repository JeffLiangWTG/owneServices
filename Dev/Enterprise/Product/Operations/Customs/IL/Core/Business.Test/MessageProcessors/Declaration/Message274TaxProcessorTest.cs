using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business.MessageProcessors;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class Message274TaxProcessorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("linkedEntryHeader is must", () => new Message274TaxProcessor(null));
		}

		public void TestUpdateEntryLineFeesIfMatch_ReturnFalse_WhenResponseGoodsShipmentListIsNull()
		{
			var message274TaxProcessor = new Message274TaxProcessor(CreateEntryHeader());
			var updateResult = message274TaxProcessor.DoEntryLineKeysMatch(null);
			AssertEquals("When responseGoodsShipmentList is null", false, updateResult);
		}

		public void TestUpdateEntryLineFeesIfMatch_ReturnFalse_WhenResponseGoodsShipmentListNotMatch()
		{
			var message274TaxProcessor = new Message274TaxProcessor(CreateEntryHeader());
			var responseGoodsShipmentList = CreateResponseGoodsShipmentList();
			responseGoodsShipmentList.Single().GovernmentAgencyGoodsItem.Single().SequenceNumeric = 2;
			var updateResult = message274TaxProcessor.DoEntryLineKeysMatch(responseGoodsShipmentList);
			AssertEquals("When responseGoodsShipmentList does not match the entry Lines", false, updateResult);
		}

		public void TestUpdateEntryLineFeesIfMatch_ReturnTrueShouldUpdateFees_WhenResponseGoodsShipmentListIsMatch()
		{
			var linkedEntryHeader = CreateEntryHeader();
			var goodsShipmentList = CreateResponseGoodsShipmentList();
			var processor = new Message274TaxProcessor(linkedEntryHeader);

			var updateResult = processor.DoEntryLineKeysMatch(goodsShipmentList);

			AssertEquals("When responseGoodsShipmentList matches the entry Lines and updates fees, should return true", true, updateResult);

			processor.UpdateEntryLineConfirmedFees(goodsShipmentList);
			var entryLine = linkedEntryHeader.MergedLines.Single();
			AssertEquals("3 new ConfirmedFees should be", 3, entryLine.ConfirmedFees.Count);
			var confirmedFees = entryLine.ConfirmedFees.Cast<CusEntryLineFee>();
			var entryLineFeeBadShouldBeRemoved = confirmedFees.FirstOrDefault(r => r.CF_ChargeType == "BAD");
			AssertNull("The Bad Fee should be removed", entryLineFeeBadShouldBeRemoved);

			AssertEntryLineFee(confirmedFees, expectedChargeType: "1", expectedBaseValue: 101m, expectedChargeAmount: 11.1m, expectedRate: 0.1m);
			AssertEntryLineFee(confirmedFees, expectedChargeType: "15", expectedBaseValue: 100m, expectedChargeAmount: 21m, expectedRate: 0.1m);
			AssertEntryLineFee(confirmedFees, expectedChargeType: "99", expectedBaseValue: 0m, expectedChargeAmount: 0m, expectedRate: 0m);
		}

		public void TestUpdateConfirmedCharges_ShouldRemoveAllAndAddNewCharges()
		{
			var linkedEntryHeader = CreateEntryHeader();
			linkedEntryHeader.ConfirmedCharges.AddNew().C1_ChargeType = "C";
			linkedEntryHeader.ConfirmedCharges.AddNew().C1_ChargeType = "D";

			var processor = new Message274TaxProcessor(linkedEntryHeader);

			var declarationDutyTaxFeeList = new Collection<DeclarationDutyTaxFee>
			{
				new DeclarationDutyTaxFee
				{
					TypeCode = new DutyTaxFeeTypeCodeType { Value = "A" },
					DmExtensions = new DeclarationDutyTaxFeeDmExtensions
					{
						CalculatedTax = new DeclarationDutyTaxFeeDmExtensionsCalculatedTax
						{
							Amount = new AmountAmountType { Value = 50m }
						}
					}
				},
				new DeclarationDutyTaxFee
				{
					TypeCode = new DutyTaxFeeTypeCodeType { Value = "B" },
					DmExtensions = new DeclarationDutyTaxFeeDmExtensions
					{
						CalculatedTax = new DeclarationDutyTaxFeeDmExtensionsCalculatedTax
						{
							Amount = new AmountAmountType { Value = 30m }
						}
					}
				},
				new DeclarationDutyTaxFee
				{
					TypeCode = new DutyTaxFeeTypeCodeType { Value = "A" },
					DmExtensions = new DeclarationDutyTaxFeeDmExtensions
					{
						CalculatedTax = new DeclarationDutyTaxFeeDmExtensionsCalculatedTax
						{
							Amount = new AmountAmountType { Value = 10m }
						}
					}
				}
			};

			processor.UpdateConfirmedCharges(declarationDutyTaxFeeList);

			var confirmedCharges = linkedEntryHeader.ConfirmedCharges;
			AssertEquals("ConfirmedCharges count should be 2 (Group by TypeCode.value)", 2, confirmedCharges.Count);

			var charges = confirmedCharges.Cast<CusEntryHeaderCharges>().ToList();
			AssertCharge(charges, "A", 60m);
			AssertCharge(charges, "B", 30m);
		}

		public void TestUpdateConfirmedCharges_ShouldHandleNulls()
		{
			var linkedEntryHeader = CreateEntryHeader();
			var processor = new Message274TaxProcessor(linkedEntryHeader);

			processor.UpdateConfirmedCharges(null);

			AssertEquals("ConfirmedCharges count should be 0 when input is null", 0, linkedEntryHeader.ConfirmedCharges.Count);

			var declarationDutyTaxFeeListNotValid = new Collection<DeclarationDutyTaxFee>
			{
				new DeclarationDutyTaxFee
				{
					TypeCode = new DutyTaxFeeTypeCodeType { },
					DmExtensions = new DeclarationDutyTaxFeeDmExtensions
					{
						CalculatedTax = new DeclarationDutyTaxFeeDmExtensionsCalculatedTax
						{
							Amount = new AmountAmountType {  }
						}
					}
				},
				new DeclarationDutyTaxFee
				{
					TypeCode = new DutyTaxFeeTypeCodeType { Value = "" },
					DmExtensions = new DeclarationDutyTaxFeeDmExtensions
					{
					}
				}
				,
				new DeclarationDutyTaxFee
				{
				}
			};

			processor.UpdateConfirmedCharges(declarationDutyTaxFeeListNotValid);
			AssertEquals("ConfirmedCharges count should be 0 when input is null", 0, linkedEntryHeader.ConfirmedCharges.Count);
		}

		static void AssertCharge(IEnumerable<CusEntryHeaderCharges> charges, string expectedChargeType, decimal expectedAmount)
		{
			CombineAssertions($"Charge with Type: {expectedChargeType}", () =>
			{
				var charge = charges.FirstOrDefault(c => c.C1_ChargeType == expectedChargeType);
				AssertNotNull($"Charge with Type: {expectedChargeType} should exist", charge);
				AssertEquals("C1_ChargeAmount", expectedAmount, charge.C1_ChargeAmount);
				AssertEquals("C1_Source", CusEntryHeaderChargesSourceCodeList.Codes.CUS, charge.C1_Source);
				AssertEquals("C1_IsLandedCostOnly", false, charge.C1_IsLandedCostOnly);
			});
		}

		static void AssertEntryLineFee(IEnumerable<CusEntryLineFee> confirmedFees, ZString expectedChargeType, decimal expectedBaseValue, decimal expectedChargeAmount, decimal expectedRate)
		{
			CombineAssertions($"Entry Line fee with Charge Type: {expectedChargeType}", () =>
			{
				var entryLineFee = confirmedFees.FirstOrDefault(r => r.CF_ChargeType == expectedChargeType);
				AssertNotNull($"Fee with Charge Type: {expectedChargeType} should be", entryLineFee);
				AssertEquals("CF_BaseValue", expectedBaseValue, entryLineFee.CF_BaseValue);
				AssertEquals("CF_ChargeAmount", expectedChargeAmount, entryLineFee.CF_ChargeAmount);
				AssertEquals("CF_MethodOfCalculation", "%", entryLineFee.CF_MethodOfCalculation);
				AssertEquals("CF_Rate", expectedRate, entryLineFee.CF_Rate);
			});
		}

		CusEntryHeader CreateEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var cusEntryLineFee = invoiceLine.CusEntryLine.ConfirmedFees.AddNew();
			cusEntryLineFee.CF_ChargeType = "BAD";
			cusEntryLineFee.CF_ChargeAmount = 123.45m;

			return declaration.CustomsEntryHeaders.Single();
		}

		static Collection<DeclarationGoodsShipment> CreateResponseGoodsShipmentList()
		{
			var goodsShipmentList = new Collection<DeclarationGoodsShipment>();
			var goodsShipment = new DeclarationGoodsShipment
			{
				SequenceNumeric = 1,
				GovernmentAgencyGoodsItem = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>
					{
						new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
						{
							SequenceNumeric = 1,
							Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
							{
								DutyTaxFee = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee>
								{
									new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
									{
										AdValoremTaxBaseAmount = new DutyTaxFeeAdValoremTaxBaseAmountType
										{
											Value = 101,
											CurrencyId = Iso3AlphaCurrencyCodeContentType.Usd
										},
										DmExtensions = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeDmExtensions
										{
											CalculatedTax = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeDmExtensionsCalculatedTax
											{
												Amount = new AmountAmountType
												{
													Value = 11.1m,
													CurrencyId = Iso3AlphaCurrencyCodeContentType.Usd
												}
											}
										},
										TypeCode = new DutyTaxFeeTypeCodeType
										{
											Value = "1"
										},
										TaxRate = 0.1m
									},
									new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
									{
										AdValoremTaxBaseAmount = new DutyTaxFeeAdValoremTaxBaseAmountType
										{
											Value = 100,
											CurrencyId = Iso3AlphaCurrencyCodeContentType.Usd
										},
										DmExtensions = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeDmExtensions
										{
											CalculatedTax = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeeDmExtensionsCalculatedTax
											{
												Amount = new AmountAmountType
												{
													Value = 21m,
													CurrencyId = Iso3AlphaCurrencyCodeContentType.Usd
												}
											}
										},
										TypeCode = new DutyTaxFeeTypeCodeType
										{
											Value = "15"
										},
										TaxRate = 0.1m
									},
									new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
									{
										TypeCode = new DutyTaxFeeTypeCodeType
										{
											Value = "99"
										}
									}
								}
							}
						}
					}
			};
			goodsShipmentList.Add(goodsShipment);
			return goodsShipmentList;
		}
	}
}

