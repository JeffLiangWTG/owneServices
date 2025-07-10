using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Customs.AsycudaCustoms.DataTransfer.Universal.Testing
{
	class DeclarationDataObjectWriterTest : TestCaseWithUniversalObjectFactory
	{
		public void TestWriteJE_ManifestNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ManifestNumber = "TEST";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertEquals("Export JE_ManifestNumber", "TEST", declarationData.ManifestNumber);
		}

		public void TestWriteInBondMoveHeaderCollection()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "IM7";
			CreateCusInBondMoveHeader(entryInstruction1, "1145a", new ZDate(2021, 1, 1), new ZDate(2021, 1, 10), new ZDate(2021, 1, 20), "1145a comment", 0, 0, 0);

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "EX1";
			CreateCusInBondMoveHeader(entryInstruction2, "1189a", new ZDate(2021, 2, 1), new ZDate(2021, 2, 10), new ZDate(2021, 2, 20), "1189a comment", 0, 0, 0);
			CreateCusInBondMoveHeader(entryInstruction2, "1189b", new ZDate(2021, 3, 1), new ZDate(2021, 3, 10), new ZDate(2021, 3, 20), "1189b comment", 0, 0, 0);

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			var entryInstructionsData = declarationData.EntryInstructionCollection;
			AssertEquals(2, entryInstructionsData.Count);

			var entryInstruction1Data = declarationData.EntryInstructionCollection.First(x => x.Style.Value == "IM7");
			var entryInstruction1Link = entryInstruction1Data.Link.Value;
			var inBondMoveHeader1Data = GetInBondMoveHeaderData(declarationData, entryInstruction1Link, "1145a");
			AssertInBondMoveHeaderData(inBondMoveHeader1Data, "1145a", new ZDate(2021, 1, 1), new ZDate(2021, 1, 10), new ZDate(2021, 1, 20), "1145a comment");

			var entryInstruction2Data = declarationData.EntryInstructionCollection.First(x => x.Style.Value == "EX1");
			var entryInstruction2Link = entryInstruction2Data.Link.Value;
			var inBondMoveHeader2Data = GetInBondMoveHeaderData(declarationData, entryInstruction2Link, "1189a");
			AssertInBondMoveHeaderData(inBondMoveHeader2Data, "1189a", new ZDate(2021, 2, 1), new ZDate(2021, 2, 10), new ZDate(2021, 2, 20), "1189a comment");
			var inBondMoveHeader3Data = GetInBondMoveHeaderData(declarationData, entryInstruction2Link, "1189b");
			AssertInBondMoveHeaderData(inBondMoveHeader3Data, "1189b", new ZDate(2021, 3, 1), new ZDate(2021, 3, 10), new ZDate(2021, 3, 20), "1189b comment");
		}

		static void CreateCusInBondMoveHeader(CusEntryInstruction entryInstruction, ZString permitNumber, ZDate issueDate, ZDate arrivalDate, ZDate expiryDate, ZString comment, ZDecimal monetaryValue, ZDecimal netWeight, ZDecimal customsQuantity)
		{
			var inBondMoveHeader = entryInstruction.CusInBondPermitsHeaders.AddNew();
			inBondMoveHeader.BM_Calc_PermitNumber = permitNumber;
			inBondMoveHeader.BM_Calc_IssueDate = issueDate;
			inBondMoveHeader.BM_ArrivalDate = arrivalDate;
			inBondMoveHeader.BM_Calc_ValidityDate = expiryDate;
			inBondMoveHeader.BM_AdditionalText = comment;
			inBondMoveHeader.BM_MonetaryValue = monetaryValue;
			inBondMoveHeader.BM_NetWeight = netWeight;
			inBondMoveHeader.BM_CustomsQuantity = customsQuantity;
		}

		static InBondMoveHeader GetInBondMoveHeaderData(Shipment declarationData, ZInt entryInstructionLink, ZString permitNumber)
		{
			return declarationData.InBondMoveHeaderCollection.First(x =>
			{
				var additionalReferenceData = x.AdditionalReferenceCollection.First(ar => ar.Type.Code.Value == "EIL" && ar.ContextInformation.Value == "InstructionLink");
				var entryNumData = x.EntryNumberCollection.First(em => em.Type.Code.Value == "PMT");
				return additionalReferenceData.ReferenceNumber.Value == entryInstructionLink.ToString() && entryNumData.Number.Value == permitNumber;
			});
		}

		static void AssertInBondMoveHeaderData(InBondMoveHeader inBondMoveHeaderData, ZString permitNumber, ZDate issueDate, ZDate arrivalDate, ZDate expiryDate, ZString comment)
		{
			var entryNumberData = inBondMoveHeaderData.EntryNumberCollection.First(x => x.Type.Code.Value == UniversalCustomsDataConstants.PermitNumberType);
			AssertEquals(permitNumber, entryNumberData.Number);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, entryNumberData.CountryOfIssue.Code);
			AssertEquals(GlbCompany.CurrentCompany.Country.Description, entryNumberData.CountryOfIssue.Name);
			AssertEquals(issueDate, entryNumberData.IssueDate);
			AssertEquals(expiryDate, entryNumberData.ExpiryDate);

			var arrivalDateData = inBondMoveHeaderData.DateCollection.First(x => x.Type == DateType.Arrival);
			AssertEquals(arrivalDate, arrivalDateData.Value);
			AssertEquals(false, arrivalDateData.IsEstimate);

			AssertEquals(comment, inBondMoveHeaderData.AdditionalText);
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
