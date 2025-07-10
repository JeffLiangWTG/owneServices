using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(DocumentDeliveryInstructions))]
	sealed class DocumentDeliveryInstructionsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var supportable = Factory.New<DummyDocumentSupportable>();

			var delivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument(),
				PrintInstructions = new DummyPrintInstructions(),
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = Factory.New<DummyWithLogs>()
			};

			var pack = new DocumentPack
			{
				DocumentSupporter = supportable.DocumentSupporter
			};

			return new DocumentDeliveryInstructions(Factory, pack, new[] { delivery });
		}

		public void TestDeliverablesToBePrinted()
		{
			var supportable = Factory.New<DummyDocumentSupportable>();
			var logParent = Factory.New<DummyWithLogs>();

			var documentDeliveries = new[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument(),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = "doc1"
					},
					EDocsInstructions = new DummyEDocsInstructions(),
					LogParent = logParent
				},
				new DummyDocumentDelivery
				{
					Document = new DummyDocument(),
					PrintInstructions = new DummyPrintInstructions
					{
						Title = "doc2"
					},
					EDocsInstructions = new DummyEDocsInstructions(),
					LogParent = logParent
				}
			};

			var pack = new DocumentPack
			{
				DocumentSupporter = supportable.DocumentSupporter
			};

			var instructions = new DocumentDeliveryInstructions(Factory, pack, documentDeliveries);

			AssertContainsExactElementsInAnyOrder(new ZString[] { "doc1", "doc2" },
				instructions.DeliverablesToBePrinted.Cast<DocumentDeliverable>().Select(x => x.Name));
		}

		public void TestFactoryStrategy()
		{
			var supportable = Factory.New<DummyDocumentSupportable>();

			var documentDeliveries = new[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument()
				}
			};

			var pack = new DocumentPack
			{
				DocumentSupporter = supportable.DocumentSupporter
			};

			var instructions = new DocumentDeliveryInstructions(Factory, pack, documentDeliveries);
			Assert(instructions.FactorySaveStrategy is FactoryStrategy.PopulateButDoNotSave);
		}

		public void TestAllowModify()
		{
			Assert(!PrepareDocumentDeliveryInstructions().AllowModify);
		}

		public void TestIsDeliveringForm()
		{
			Assert(PrepareDocumentDeliveryInstructions().IsDeliveringFormDocument);
		}

		DocumentDeliveryInstructions PrepareDocumentDeliveryInstructions()
		{
			var supportable = Factory.New<DummyDocumentSupportable>();

			var documentDeliveries = new[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument()
				}
			};

			var pack = new DocumentPack
			{
				DocumentSupporter = supportable.DocumentSupporter
			};

			return new DocumentDeliveryInstructions(Factory, pack, documentDeliveries);
		}

		public void TestDeliverableCollectionView()
		{
			var logParent = Factory.New<DummyWithLogs>();

			var printInstructions = new DummyPrintInstructions
			{
				Title = "to print",
				DeliveryModes = new[]
				{
					nameof(PrintCopyType.PRN)
				}
			};

			var emailAndFaxInstructions = new DummyPrintInstructions
			{
				Title = "to email and fax",
				DeliveryModes = new[]
				{
					nameof(PrintCopyType.FAX),
					nameof(PrintCopyType.EML)
				}
			};

			var allInstructions = new DummyPrintInstructions
			{
				Title = "to all",
				DeliveryModes = new[]
				{
					nameof(PrintCopyType.ALL)
				}
			};

			var documentDeliveries = new[]
			{
				new DummyDocumentDelivery
				{
					Document = new DummyDocument(),
					DeliveryMode = nameof(PrintCopyType.PRN),
					PrintInstructions = printInstructions,
					EDocsInstructions = new DummyEDocsInstructions(),
					LogParent = logParent
				},
				new DummyDocumentDelivery
				{
					Document = new DummyDocument(),
					DeliveryMode = nameof(PrintCopyType.EML),
					PrintInstructions = emailAndFaxInstructions,
					EDocsInstructions = new DummyEDocsInstructions(),
					LogParent = logParent
				},
				new DummyDocumentDelivery
				{
					Document = new DummyDocument(),
					DeliveryMode = nameof(PrintCopyType.FAX),
					PrintInstructions = emailAndFaxInstructions,
					EDocsInstructions = new DummyEDocsInstructions(),
					LogParent = logParent
				},
				new DummyDocumentDelivery
				{
					Document = new DummyDocument(),
					DeliveryMode = nameof(PrintCopyType.ALL),
					PrintInstructions = allInstructions,
					EDocsInstructions = new DummyEDocsInstructions(),
					LogParent = logParent
				}
			};

			var instructions = new DocumentDeliveryInstructions(Factory, DocumentPack.EmptyPack, documentDeliveries);
			AssertEquals("prerequisite - no recipients", false, instructions.Recipients.Any());

			var recipient = instructions.Recipients.AddNew();
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			AssertContainsExactElementsInAnyOrder("deliverables to email",
				new[]
				{
					"to email and fax",
					"to all"
				},
				instructions.DeliverablesToBePrinted.Cast<DocumentDeliverable>().Where(d => d.IncludedInPrint).Select(d => d.Name));

			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Print;
			AssertContainsExactElementsInAnyOrder("deliverables to print",
				new[]
				{
					"to print",
					"to all"
				},
				instructions.DeliverablesToBePrinted.Cast<DocumentDeliverable>().Where(d => d.IncludedInPrint).Select(d => d.Name));

			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Fax;
			AssertContainsExactElementsInAnyOrder("deliverables to fax",
				new[]
				{
					"to email and fax",
					"to all"
				},
				instructions.DeliverablesToBePrinted.Cast<DocumentDeliverable>().Where(d => d.IncludedInPrint).Select(d => d.Name));
		}
	}
}
