using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	[TestedType(typeof(EntryHeaderDataObject))]
	public class EntryHeaderDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCorrectDv1DetailCanBeRetrieved_WhenDeclarationHasMultipleDv1Details()
		{
			declaration.DV1Details.AddNew();
			var seq1 = declaration.DV1Details[0].Sequence;
			var seq2 = declaration.DV1Details[1].Sequence;
			declaration.DV1Details[0].DV1_RelationDetails = "ABC";
			declaration.DV1Details[1].DV1_RelationDetails = "DEF";

			entryInstruction.DV1DetailsPivots.Cast<EU.Business.Declaration.NonPersistentCusDV1DetailPivot>().FirstOrDefault(x => x.Sequence == seq1).IsForEntryInstruction = true;
			entryInstruction.DV1DetailsPivots.Cast<EU.Business.Declaration.NonPersistentCusDV1DetailPivot>().FirstOrDefault(x => x.Sequence == seq2).IsForEntryInstruction = false;
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals("ABC", dataObject.Q7cDetails);

			entryInstruction.DV1DetailsPivots.Cast<EU.Business.Declaration.NonPersistentCusDV1DetailPivot>().FirstOrDefault(x => x.Sequence == seq1).IsForEntryInstruction = false;
			entryInstruction.DV1DetailsPivots.Cast<EU.Business.Declaration.NonPersistentCusDV1DetailPivot>().FirstOrDefault(x => x.Sequence == seq2).IsForEntryInstruction = true;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals("DEF", dataObject.Q7cDetails);

			entryInstruction.DV1DetailsPivots.Cast<EU.Business.Declaration.NonPersistentCusDV1DetailPivot>().FirstOrDefault(x => x.Sequence == seq1).IsForEntryInstruction = false;
			entryInstruction.DV1DetailsPivots.Cast<EU.Business.Declaration.NonPersistentCusDV1DetailPivot>().FirstOrDefault(x => x.Sequence == seq2).IsForEntryInstruction = false;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(ZString.Empty, dataObject.Q7cDetails);
		}

		public void TestHasRelatedParty()
		{
			declaration.DV1Details[0].DV1_Relationship = YesNoList.Codes.Yes;
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(true, dataObject.HasRelatedParty);

			declaration.DV1Details[0].DV1_Relationship = YesNoList.Codes.No;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(false, dataObject.HasRelatedParty);
		}

		public void TestHasRelatedParty2()
		{
			declaration.DV1Details[0].DV1_PriceInfluence = YesNoList.Codes.Yes;
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(true, dataObject.HasRelatedParty2);

			declaration.DV1Details[0].DV1_PriceInfluence = YesNoList.Codes.No;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(false, dataObject.HasRelatedParty2);
		}

		public void TestHasRestrictions()
		{
			declaration.DV1Details[0].DV1_Restrictions = YesNoList.Codes.Yes;
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(true, dataObject.HasRestrictions);

			declaration.DV1Details[0].DV1_Restrictions = YesNoList.Codes.No;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(false, dataObject.HasRestrictions);
		}

		public void TestHasSaleConditions()
		{
			declaration.DV1Details[0].DV1_Consideration = YesNoList.Codes.Yes;
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(true, dataObject.HasSaleConditions);

			declaration.DV1Details[0].DV1_Consideration = YesNoList.Codes.No;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(false, dataObject.HasSaleConditions);
		}

		public void TestHasRoyaltyCharges()
		{
			declaration.DV1Details[0].DV1_RoyaltiesLicence = YesNoList.Codes.Yes;
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(true, dataObject.HasRoyaltyCharges);

			declaration.DV1Details[0].DV1_RoyaltiesLicence = YesNoList.Codes.No;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(false, dataObject.HasRoyaltyCharges);
		}

		public void TestHasDisposal()
		{
			declaration.DV1Details[0].DV1_Resale = YesNoList.Codes.Yes;
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(true, dataObject.HasDisposal);

			declaration.DV1Details[0].DV1_Resale = YesNoList.Codes.No;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(false, dataObject.HasDisposal);
		}

		public void TestPreviousCustomsDecisions()
		{
			declaration.DV1Details[0].DV1_CustomsDecisionNumber = "ABC";
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals("ABC", dataObject.PreviousCustomsDecisions);

			declaration.ZG_IsHighValueOvrd = false;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(ZString.Empty, dataObject.PreviousCustomsDecisions);
		}

		public void TestQ7cDetails()
		{
			declaration.DV1Details[0].DV1_RelationDetails = "ABC";
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals("ABC", dataObject.Q7cDetails);

			declaration.ZG_IsHighValueOvrd = false;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(ZString.Empty, dataObject.Q7cDetails);
		}

		public void TestQ8bDetails()
		{
			declaration.DV1Details[0].DV1_RestrictionConsiderationDetails = "ABC";
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals("ABC", dataObject.Q8bDetails);

			declaration.ZG_IsHighValueOvrd = false;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(ZString.Empty, dataObject.Q8bDetails);
		}

		public void TestQ9bDetails()
		{
			declaration.DV1Details[0].DV1_RoyaltiesLicenceDetails = "ABC";
			declaration.DV1Details[0].DV1_ResaleDetails = "DEF";
			var dataObject = new EntryHeaderDataObject(entry);
			var expectedQ9bDetails = new ZStringBuilder();
			expectedQ9bDetails.AppendLine("ABC");
			expectedQ9bDetails.Append("DEF");
			AssertEquals((ZString)expectedQ9bDetails.ToString(), dataObject.Q9bDetails);

			declaration.DV1Details[0].DV1_RoyaltiesLicenceDetails = ZString.Empty;
			dataObject = new EntryHeaderDataObject(entry);
			expectedQ9bDetails = new ZStringBuilder();
			expectedQ9bDetails.Append("DEF");
			AssertEquals((ZString)expectedQ9bDetails.ToString(), dataObject.Q9bDetails);

			declaration.ZG_IsHighValueOvrd = false;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(ZString.Empty, dataObject.Q9bDetails);
		}

		public void TestPrintDefaultFlag()
		{
			declaration.ZG_IsHighValueOvrd = true;
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(1, declaration.DV1Details.Count);
			AssertEquals(true, dataObject.PrintDefaultFlag);

			declaration.ZG_IsHighValueOvrd = false;
			dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(0, declaration.DV1Details.Count);
			AssertEquals(false, dataObject.PrintDefaultFlag);
		}

		public void TestPrintQ7CFlag()
		{
			var dataObject = new EntryHeaderDataObject(entry);
			AssertEquals(false, dataObject.PrintQ7CFlag);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new EntryHeaderDataObject(entryHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.ZG_IsHighValueOvrd = true;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.DV1DetailsPivots.Cast<EU.Business.Declaration.NonPersistentCusDV1DetailPivot>().FirstOrDefault().IsForEntryInstruction = true;
			var invoice1 = declaration.Invoices.AddNew();
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
		}

		JobDeclaration declaration;
		CusEntryHeader entry;
		CusEntryInstruction entryInstruction;
	}
}
