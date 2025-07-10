using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;

namespace Enterprise.Customs.AsycudaCustoms.DataTransfer.Universal.Testing
{
	class JobDeclarationDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestFillJE_ManifestNumber()
		{
			AssertFilllJE_ManifestNumber("Import JE_ManifestNumber", "TEST", "TEST");
		}

		public void TestFillJE_ManifestNumberWithExceedMaxLength()
		{
			AssertFilllJE_ManifestNumber("The max length of JE_ManifestNumber in AsycudaCustoms is 28 chars", "123456789012345678901234567890", "1234567890123456789012345678");
		}

		void AssertFilllJE_ManifestNumber(ZString message, ZString manifestNumber, ZString expectedJE_ManifestNumber)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair { Code = Customs.Business.JobMessageTypeList.Codes.Import },
				WayBillNumber = "MYMASTER",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master },
				ManifestNumber = manifestNumber
			};

			var logger = new TestErrorLogger();
			var provider = new CustomsShipmentDataObjectReaderProvider();
			BusinessObject bizObj = null;
			provider.GetReader(declarationData, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
			var declaration = (Customs.Business.BaseJobDeclaration)bizObj;
			AssertEquals(message, expectedJE_ManifestNumber, declaration.JE_ManifestNumber);
		}

		public void TestFillInBondMoveHeaderCollection()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair { Code = Customs.Business.JobMessageTypeList.Codes.Import },
				WayBillNumber = "MYMASTER",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master }
			};
			declarationData.SetEntryInstructionCollection(() => new List<EntryInstruction>
			{
				new EntryInstruction
				{
					Style = "IM7",
					Link = 1
				},
				new EntryInstruction
				{
					Style = "EX1",
					Link = 2
				}
			});

			var inBondMoveHeaderData1 = CreateInBondMoveHeaderData(1, "1145a", new ZDate(2021, 1, 1), new ZDate(2021, 1, 10), new ZDate(2021, 1, 20), "1145a comment");
			var inBondMoveHeaderData2 = CreateInBondMoveHeaderData(2, "1189a", new ZDate(2021, 2, 1), new ZDate(2021, 2, 10), new ZDate(2021, 2, 20), "1189a comment");
			var inBondMoveHeaderData3 = CreateInBondMoveHeaderData(2, "1189b", new ZDate(2021, 3, 1), new ZDate(2021, 3, 10), new ZDate(2021, 3, 20), "1189b comment");

			declarationData.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>
			{
				inBondMoveHeaderData1,
				inBondMoveHeaderData2,
				inBondMoveHeaderData3
			});

			var logger = new TestErrorLogger();
			var provider = new CustomsShipmentDataObjectReaderProvider();
			BusinessObject bizObj = null;
			provider.GetReader(declarationData, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
			var declaration = (Customs.Business.BaseJobDeclaration)bizObj;
			AssertEquals(2, declaration.CustomsEntryInstructions.Count);

			var entryInstruction1 = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().First(x => x.CEI_Style == "IM7");
			AssertEquals(1, entryInstruction1.CusInBondPermitsHeaders.Count);
			var inBondMoveHeader1 = entryInstruction1.CusInBondPermitsHeaders.First(x => x.BM_Calc_PermitNumber == "1145a");
			AssertInBondMoveHeader(inBondMoveHeader1, "1145a", new ZDate(2021, 1, 1), new ZDate(2021, 1, 10), new ZDate(2021, 1, 20), "1145a comment", 0, 0, 0);

			var entryInstruction2 = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().First(x => x.CEI_Style == "EX1");
			AssertEquals(2, entryInstruction2.CusInBondPermitsHeaders.Count);
			var inBondMoveHeader2 = entryInstruction2.CusInBondPermitsHeaders.First(x => x.BM_Calc_PermitNumber == "1189a");
			AssertInBondMoveHeader(inBondMoveHeader2, "1189a", new ZDate(2021, 2, 1), new ZDate(2021, 2, 10), new ZDate(2021, 2, 20), "1189a comment", 0, 0, 0);
			var inBondMoveHeader3 = entryInstruction2.CusInBondPermitsHeaders.First(x => x.BM_Calc_PermitNumber == "1189b");
			AssertInBondMoveHeader(inBondMoveHeader3, "1189b", new ZDate(2021, 3, 1), new ZDate(2021, 3, 10), new ZDate(2021, 3, 20), "1189b comment", 0, 0, 0);
		}

		static InBondMoveHeader CreateInBondMoveHeaderData(ZInt entryInstructionLink, ZString permitNumber, ZDate issueDate, ZDate arrivalDate, ZDate expiryDate, ZString comment)
		{
			var inBondMoveHeaderData = new InBondMoveHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AdditionalText = comment
			};
			inBondMoveHeaderData.SetEntryNumberCollection(() => new List<EntryNumber>
			{
				new EntryNumber
				{
					Number = "#12312",
					Type = new EntryType
					{
						Code = UniversalCustomsDataConstants.PermitNumberType
					},
					CountryOfIssue = new Country
					{
						Code = Core.Constants.CountryCodes.NeutralZone
					},
					IssueDate = ZDate.BrettsBirthday,
					ExpiryDate = ZDate.BrettsBirthday.AddDays(10)
				},
				new EntryNumber
				{
					Number = "#86465",
					Type = new EntryType
					{
						Code = "$#D	"
					},
					CountryOfIssue = new Country
					{
						Code = GlbCompany.CurrentCompany.GC_RN_NKCountryCode
					},
					IssueDate = ZDate.BrettsBirthday,
					ExpiryDate = ZDate.BrettsBirthday.AddDays(10)
				},
				new EntryNumber
				{
					Number = permitNumber,
					Type = new EntryType
					{
						Code = UniversalCustomsDataConstants.PermitNumberType
					},
					CountryOfIssue = new Country
					{
						Code = GlbCompany.CurrentCompany.GC_RN_NKCountryCode
					},
					IssueDate = issueDate,
					ExpiryDate = expiryDate
				}
			});
			inBondMoveHeaderData.SetDateCollection(() => new List<Date>
			{
				new Date
				{
					Type = DateType.Arrival,
					Value = arrivalDate,
					IsEstimate = false
				}
			});
			inBondMoveHeaderData.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
			{
				new AdditionalReference
				{
					Type = new EntryType
					{
						Code = UniversalCustomsDataConstants.EntryInstructionLinkType
					},
					ContextInformation = "BobTheBuilder",
					ReferenceNumber = "200"
				},
				new AdditionalReference
				{
					Type = new EntryType
					{
						Code = "#$L"
					},
					ContextInformation = UniversalCustomsDataConstants.EntryInstructionLinkContext,
					ReferenceNumber = "300"
				},
				new AdditionalReference
				{
					Type = new EntryType
					{
						Code = UniversalCustomsDataConstants.EntryInstructionLinkType
					},
					ContextInformation = UniversalCustomsDataConstants.EntryInstructionLinkContext,
					ReferenceNumber = entryInstructionLink.ToString()
				}
			});

			return inBondMoveHeaderData;
		}

		static void AssertInBondMoveHeader(CusInBondMoveHeader inBondMoveHeader, ZString permitNumber, ZDate issueDate, ZDate arrivalDate, ZDate expiryDate, ZString comment, ZDecimal monetaryValue, ZDecimal netWeight, ZDecimal customsQuantity)
		{
			AssertEquals(permitNumber, inBondMoveHeader.BM_Calc_PermitNumber);
			AssertEquals(issueDate, inBondMoveHeader.BM_Calc_IssueDate);
			AssertEquals(arrivalDate, inBondMoveHeader.BM_ArrivalDate);
			AssertEquals(expiryDate, inBondMoveHeader.BM_Calc_ValidityDate);
			AssertEquals(comment, inBondMoveHeader.BM_AdditionalText);
			AssertEquals(monetaryValue, inBondMoveHeader.BM_MonetaryValue);
			AssertEquals(netWeight, inBondMoveHeader.BM_NetWeight);
			AssertEquals(customsQuantity, inBondMoveHeader.BM_CustomsQuantity);
		}
	}
}
