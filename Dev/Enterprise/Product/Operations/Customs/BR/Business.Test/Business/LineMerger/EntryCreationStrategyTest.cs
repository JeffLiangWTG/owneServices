using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public abstract class EntryCreationStrategyTest<T> : Customs.Business.Testing.EntryCreationStrategyTest where T : EntryCreationStrategy, IEntryCreationStrategyForTesting
	{
		public void TestIsActive()
		{
			var entryCreationStrategy = GetNewEntryCreationStrategyForTesting();
			Assert("IsActive should br TRUE", entryCreationStrategy.IsActiveExposed);
		}

		public void TestMessageTypeToNewEntryHeader()
		{
			var entryCreationStrategy = GetNewEntryCreationStrategyForTesting();
			AssertEquals(ExpectedMessageTypeToNewEntryHeader, entryCreationStrategy.MessageTypeToNewEntryHeaderExposed);
		}

		public void TestUseAdditionalEntryLineLinks()
		{
			var entryCreationStrategy = GetNewEntryCreationStrategyForTesting();
			AssertEquals(ExpectedAdditionalEntryLineLinks, entryCreationStrategy.UseAdditionalEntryLineLinksExposed);

			var invoiceLine = jobDeclaration.Invoices[0].InvoiceLines[0];
			var entryLine1 = new EntryManager(jobDeclaration, entryCreationStrategy).GetOrCreateEntryLine(invoiceLine) as CusEntryLine;
			var additionalEntryLineLink1 = invoiceLine.AdditionalEntryLineLinks.FirstOrDefault();
			AssertEntryLineLinkToInvoiceLine(entryLine1);

			var entryLine2 = new EntryManager(jobDeclaration, entryCreationStrategy).GetOrCreateEntryLine(invoiceLine) as CusEntryLine;
			var additionalEntryLineLink2 = invoiceLine.AdditionalEntryLineLinks.FirstOrDefault();

			AssertEntryLineLinkToInvoiceLine(entryLine2);
			AssertSame("Entry Header reused", entryLine1.Header, entryLine2.Header);
			AssertSame("Entry Line reused", entryLine1, entryLine2);

			if (ExpectedAdditionalEntryLineLinks)
			{
				AssertSame("AdditionalEntryLineLink reused", additionalEntryLineLink1, additionalEntryLineLink2);
			}

			void AssertEntryLineLinkToInvoiceLine(CusEntryLine entryLine)
			{
				CombineAssertions(() =>
				{
					AssertEquals("MessageTypeToNewEntryHeader", ExpectedMessageTypeToNewEntryHeader, entryLine.Header.CH_MessageType);

					if (ExpectedAdditionalEntryLineLinks)
					{
						var additionalEntryLineLink = invoiceLine.AdditionalEntryLineLinks.FirstOrDefault();

						AssertEquals("JI_CL not set", ZGuid.Empty, invoiceLine.JI_CL);
						AssertEquals("AdditionalEntryLineLink Added", 1, invoiceLine.AdditionalEntryLineLinks.Count);
						AssertEquals("AdditionalEntryLineLink.BU_JI", invoiceLine.PK, additionalEntryLineLink.BU_JI);
						AssertEquals("AdditionalEntryLineLink.BU_CL", entryLine.PK, additionalEntryLineLink.BU_CL);
					}
					else
					{
						AssertEquals("JI_CL set to Entry Line PK", entryLine.PK, invoiceLine.JI_CL);
						AssertEquals("No AdditionalEntryLineLink Added", 0, invoiceLine.AdditionalEntryLineLinks.Count);
					}
				});
			}

			entryCreationStrategy.ClearReferenceToEntryLineWhenLineIsNotValidForMergeExposed(invoiceLine);
			if (ExpectedAdditionalEntryLineLinks)
			{
				Assert("AdditionalEntryLineLinks should be delete", additionalEntryLineLink1.IsDeleted);
				AssertEquals("AdditionalEntryLineLinks count should be 0", 0, invoiceLine.AdditionalEntryLineLinks.Count);
			}
			else
			{
				Assert("Invoice Line JI_CL should be Empty", invoiceLine.JI_CL.IsEmpty);
			}
		}

		public override void TestGetKeyForHeader()
		{
			var entryCreationStrategy = GetNewEntryCreationStrategyForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("Entry Instruction in header", false, jobDeclaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
				Assert("Entry Instruction pk", entryCreationStrategy.GetKeyForHeader(jobDeclaration.InvoiceLines[0]).Contains(jobDeclaration.CustomsEntryInstructions[0].PK));
			});
		}

		protected JobDeclaration jobDeclaration;

		protected virtual T GetNewEntryCreationStrategyForTesting()
		{
			jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			jobDeclaration.JE_MessageType = JobDeclarationMessageType;
			var provider = jobDeclaration.CustomsEntryInstructionProvider;
			var instruction = provider.CustomsEntryInstructions.AddNew();
			var invHeader = jobDeclaration.Invoices.AddNew();
			var invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = instruction.PK;

			return CreateNewEntryCreationStrategy(jobDeclaration);
		}

		protected abstract T CreateNewEntryCreationStrategy(JobDeclaration declaration);

		protected abstract ZString JobDeclarationMessageType { get; }

		protected abstract ZString ExpectedMessageTypeToNewEntryHeader { get; }

		protected virtual ZBool ExpectedAdditionalEntryLineLinks => false;
	}

	public interface IEntryCreationStrategyForTesting
	{
		bool UseAdditionalEntryLineLinksExposed { get; }
		bool IsActiveExposed { get; }
		string MessageTypeToNewEntryHeaderExposed { get; }
		void ClearReferenceToEntryLineWhenLineIsNotValidForMergeExposed(BaseJobComInvoiceLine invoiceLine);
	}
}
