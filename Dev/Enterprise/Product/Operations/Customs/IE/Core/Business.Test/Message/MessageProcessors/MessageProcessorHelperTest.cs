using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	class MessageProcessorHelperTest : TestCaseWithFactory
	{
		public void TestPopulateConfirmedDutiesAndTaxes()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var taxes = CreateDutiesAndTaxesType03Data();
			var taxProviders = taxes.Select(x => new DutiesAndTaxesTypeProvider(x));

			MessageProcessorHelper.PopulateConfirmedDutiesAndTaxes(entryLine, taxProviders);

			MessageProcessorHelperTest.AssertConfirmedDutiesAndTaxesAdded(entryLine);
		}

		public void TestPopulateRequestedDocuments()
		{
			var instruction = Factory.New<CusEntryInstruction>();

			var additionalInformations = new Collection<DocumentAdditionalInformationType>()
			{
				new DocumentAdditionalInformationType()
				{
					DocumentType = "D001",
					DocumentComplementaryInformation = "DocInfo"
				}
			};
			var documentAdditionalInformationProvider = additionalInformations.Select(x => new DocumentAdditionalInformationProvider(x));

			MessageProcessorHelper.PopulateRequestedDocuments(instruction, documentAdditionalInformationProvider, ZDateTime.Today, ZDateTime.Today.AddDays(1), "RCV");

			AssertEquals(1, instruction.RequestedDocuments.Count);
			var requestedDocument = instruction.RequestedDocuments[0];
			AssertEquals("D001", requestedDocument.CSI_Code);
			AssertEquals("DocInfo", requestedDocument.RequestInformation);
			AssertEquals(ZDateTime.Today, requestedDocument.CSI_DateOfIssue);
			AssertEquals(ZDateTime.Today.AddDays(1), requestedDocument.CSI_DateOfExpiry);
			AssertEquals("RCV", requestedDocument.CSI_Status);
			AssertNullOrEmpty(requestedDocument.CSI_RN_NKCountryCode);
		}

		public void TestPopulateRequestedDocumentsWhenCcQualifierIsNotEmpty()
		{
			var instruction = Factory.New<CusEntryInstruction>();

			var additionalInformations = new Collection<MRequestedDocumentsType02>()
			{
				new MRequestedDocumentsType02()
				{
					Type = "D001",
					Description = "DocInfo",
					CcQualifier = "XY"
				}
			};
			var documentAdditionalInformationProvider = additionalInformations.Select(x => new DocumentAdditionalInformationProvider(x));

			MessageProcessorHelper.PopulateRequestedDocuments(instruction, documentAdditionalInformationProvider, ZDateTime.Today, ZDateTime.Today.AddDays(1), "RCV");

			AssertEquals(1, instruction.RequestedDocuments.Count);
			var requestedDocument = instruction.RequestedDocuments[0];
			AssertEquals("D001", requestedDocument.CSI_Code);
			AssertEquals("DocInfo", requestedDocument.RequestInformation);
			AssertEquals(ZDateTime.Today, requestedDocument.CSI_DateOfIssue);
			AssertEquals(ZDateTime.Today.AddDays(1), requestedDocument.CSI_DateOfExpiry);
			AssertEquals("RCV", requestedDocument.CSI_Status);
			AssertEquals("XY", requestedDocument.CSI_RN_NKCountryCode);
		}

		public static Collection<MDutiesAndTaxesType03> CreateDutiesAndTaxesType03Data()
		{
			return new Collection<MDutiesAndTaxesType03>()
			{
				new MDutiesAndTaxesType03()
				{
					TaxType = "A01",
					MethodOfPayment = "A",
					SequenceNumber = "001",
					TaxBase = new Collection<MTaxBaseType01>()
					{
						new MTaxBaseType01()
						{
							MeasurementUnitAndQualifier = "KG",
							Quantity = 100m,
							Amount = 0,
							TaxRate = 1m,
							TaxAmount = 10m,
							SequenceNumber = "01"
						}
					}
				},
				new MDutiesAndTaxesType03()
				{
					TaxType = "A02",
					MethodOfPayment = "B",
					SequenceNumber = "002",
					TaxBase = new Collection<MTaxBaseType01>()
					{
						new MTaxBaseType01()
						{
							MeasurementUnitAndQualifier = "GR",
							Quantity = 0,
							Amount = 200m,
							TaxRate = 2m,
							TaxAmount = 20m,
							SequenceNumber = "02"
						}
					}
				},
			};
		}

		public static void AssertConfirmedDutiesAndTaxesAdded(CusEntryLine entryLine)
		{
			AssertEquals(2, entryLine.ConfirmedFees.Count);
			AssertEquals("Confirmed Fee 1 - CF_ChargeType", "A01", entryLine.ConfirmedFees[0].CF_ChargeType);
			AssertEquals("Confirmed Fee 1 - CF_MethodOfCalculation", "KG", entryLine.ConfirmedFees[0].CF_MethodOfCalculation);
			AssertEquals("Confirmed Fee 1 - CF_BaseValue", 100m, entryLine.ConfirmedFees[0].CF_BaseValue);
			AssertEquals("Confirmed Fee 1 - CF_Rate", 1m, entryLine.ConfirmedFees[0].CF_Rate);
			AssertEquals("Confirmed Fee 1 - CF_ChargeAmount", 10m, entryLine.ConfirmedFees[0].CF_ChargeAmount);
			AssertEquals("Confirmed Fee 1 - CF_MethodOfPayment", "A", entryLine.ConfirmedFees[0].CF_MethodOfPayment);

			AssertEquals(2, entryLine.ConfirmedFees.Count);
			AssertEquals("Confirmed Fee 2 - CF_ChargeType", "A02", entryLine.ConfirmedFees[1].CF_ChargeType);
			AssertEquals("Confirmed Fee 2 - CF_MethodOfCalculation", "GR", entryLine.ConfirmedFees[1].CF_MethodOfCalculation);
			AssertEquals("Confirmed Fee 2 - CF_BaseValue", 200m, entryLine.ConfirmedFees[1].CF_BaseValue);
			AssertEquals("Confirmed Fee 2 - CF_Rate", 2m, entryLine.ConfirmedFees[1].CF_Rate);
			AssertEquals("Confirmed Fee 2 - CF_ChargeAmount", 20m, entryLine.ConfirmedFees[1].CF_ChargeAmount);
			AssertEquals("Confirmed Fee 2 - CF_MethodOfPayment", "B", entryLine.ConfirmedFees[1].CF_MethodOfPayment);
		}
	}
}
