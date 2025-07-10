using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using BusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.DocumentEngine.Testing
{
	public abstract class DocumentTemplateTestCase : TestCaseWithFactory
	{
		protected abstract IDocumentSupportable GetNewParentBusinessObject();

		protected IDocumentSupportable ParentBusinessObject
		{
			get { return fParentBusinessObject ?? (fParentBusinessObject = GetNewParentBusinessObject()); }
		}
		IDocumentSupportable fParentBusinessObject;

		protected DocumentSupporter DocumentSupporter
		{
			get { return fDocumentSupporter ?? (fDocumentSupporter = ParentBusinessObject.DocumentSupporter); }
		}
		DocumentSupporter fDocumentSupporter;

		protected BusinessContext BusinessContext
		{
			get { return DocumentSupporter.BusinessContext; }
		}

		protected void AssertDocumentCanBeRendered(ZString menuName)
		{
			DocumentZQuery query = new DocumentZQuery(BusinessContext, menuName);
			AssertDocumentCanBeRendered(Factory.LoadTop1<DocumentCommand>(query));
		}

		protected void AssertDocumentCanBeRendered(DocumentCommand documentCommand)
		{
			AssertDocumentCanBeRenderedHasBeenRun = true;
			AssertNotNull("No DocumentCommand (Menu Item Record) was found for the Menu Name/Parameters specified.", documentCommand);
			AssertNotNull("You must provide a valid ParentBusinessObject the implements IDocumentSupportable to obtain a valid DocumentSupporter.", ParentBusinessObject);
			using (DocumentPack documentPack = new UtilityClasses.DocumentPackAlwaysIncludesSections(documentCommand, ParentBusinessObject, null))
			{
				PrintTask printTask = new PrintTask(); // Don't dispose this as the Pack is already being disposed.
				printTask.Add(documentPack);

				DeliveryInstructions deliveryInstructions = new DeliveryInstructions();
				deliveryInstructions.Destination = DeliveryInstructionDestination.None;

				AssertNoExceptionThrown(delegate
				{ printTask.Run(deliveryInstructions); });
			}
		}

		protected override void TearDown()
		{
			Assert("You must call an overload of AssertDocumentCanBeRenderedHasBeenRun() once for every test method defined in this test class.", AssertDocumentCanBeRenderedHasBeenRun);
			base.TearDown();
		}
		bool AssertDocumentCanBeRenderedHasBeenRun;
	}
}
