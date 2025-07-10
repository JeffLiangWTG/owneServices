using System.IO;
using CargoWise.BuildTools;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	public abstract class BaseRunDocumentsTest : TestCaseWithFactory //TODO: Should descend from Enterprise.DocumentEngine.Testing.DocumentTemplateTestCase
	{
		protected BaseRunDocumentsTest()
		{
			fRunDocumentWithAllSections = ZBool.True;
		}

		public virtual ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}
		ZQuery fFilterForMenuItem;

		public abstract BusinessObject GetBusinessObject { get; }

		public abstract BusinessContext BusinessContext { get; }

		public ZBool RunDocumentWithAllSections
		{
			get { return fRunDocumentWithAllSections; }
			set { fRunDocumentWithAllSections = value; }
		}
		protected ZBool fRunDocumentWithAllSections;

		public void CreateDocumentMenuAndSetPivot(ZString menuName, ZString templateName, ZString dataContext)
		{
			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = menuName;
			command.SU_BusinessContext = BusinessContext.ToString();
			command.SU_ContactType = ContactType.NoContactType.ToString();
			command.SU_IsSystemDefined = ZBool.True;
			var template = Factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_Name, SQLComparisonOperator.Equal, templateName));
			var pivot = Factory.New<StmMenuTemplatePivot>();
			template.SO_DataContext = dataContext; // this is to trick the docengine to create the correct wrapper.
			pivot.SI_SO = template.PK;
			pivot.SI_SU = command.PK;
			pivot.SI_IsSystemDefined = ZBool.True;
			pivot.SI_DocumentTitle = menuName;
		}

		protected void RunDocument()
		{
			ZQuery menuItemFilter = FilterForMenuItem;
			Assert("You need to set FilterForMenuItem", !menuItemFilter.IsEmpty);
			RunDocument(menuItemFilter);
		}

		protected virtual void RunDocument(string menuItemName)
		{
			DocumentCommand menuItem;
			RunDocument(menuItemName, out menuItem);
		}

		protected void RunDocument(string menuItemName, out DocumentCommand menuItem)
		{
			RunDocument(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, menuItemName), out menuItem);
		}

		protected void RunDocument(ZQuery menuItemFilter)
		{
			DocumentCommand menuItem;
			RunDocument(menuItemFilter, out menuItem);
		}

		protected void RunDocument(ZQuery menuItemFilter, out DocumentCommand menuItem)
		{
			menuItem = GetDocumentCommand(menuItemFilter);

			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.None;

			using (DocumentPack documentPack = RunDocumentWithAllSections ? new DocumentPackAlwaysIncludesSections(menuItem, (IDocumentSupportable)GetBusinessObject, null) : new DocumentPack(menuItem, (IDocumentSupportable)GetBusinessObject, null, null))
			{
				using (PrintTask printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					printTask.Run(instructions);
				}
			}
		}

		protected void AssertRunDocument(IDocumentSupportable businessObject, string expectedOutput, string language = default, string message = default)
		{
			var menuItem = GetDocumentCommand();
			using (var documentPack = RunDocumentWithAllSections
				? new DocumentPackAlwaysIncludesSections(menuItem, businessObject, null)
				: new DocumentPack(menuItem, businessObject, null, null, language: language))
			{
				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					AssertPrintTaskRun(printTask, expectedOutput, message);
				}
			}
		}

		#region Legacy

		protected void AssertLegacyTemplate(string templatePath, string expectedTemplate, string message = default)
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(Path.Combine(BuildConstants.LocalEnterprisePath, templatePath));
				excelInterface.ActiveWorksheet = 0;
				AssertMultilineASCIIEquals(message, expectedTemplate, excelInterface.WorkSheets[0].ToString());
			}
		}

		protected void AssertRunDocumentLegacy(IDocumentSupportable documentSupportable, string expectedOutput, string message = default)
		{
			var menuItem = GetDocumentCommand();
			var supporter = (RatingHeaderDocumentSupporter)(documentSupportable).DocumentSupporter;
			using (var printTask = supporter.BuildPrintTask(menuItem))
			{
				AssertPrintTaskRun(printTask, expectedOutput, message);
			}
		}

		#endregion

		#region Implementation

		internal void AssertPrintTaskRun(PrintTask printTask, string expectedOutput, string language = default, string message = default)
		{
			var instructions = new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			instructions.Recipients.RemoveAndDeleteAll();

			if (!string.IsNullOrEmpty(language))
			{
				instructions.Language = language;
			}

			var recipient = instructions.Recipients.AddNew();
			recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.AttachmentType = OrgConstants.AttachmentType.PDF;

			printTask.Run(instructions);

			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();

			AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

			using (var excelInterface = new ExcelInterface())
			using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
			{
				excelInterface.LoadExcelFile(stream);
				var actual = string.Join("\r\n", excelInterface.WorkSheets);
				AssertContainsExactLinesInExactOrder(message, expectedOutput, actual);
			}
		}

		protected DocumentCommand GetDocumentCommand()
		{
			var filterForMenuItem = FilterForMenuItem;
			Assert("You need to set FilterForMenuItem", !filterForMenuItem.IsEmpty);

			return GetDocumentCommand(filterForMenuItem);
		}

		DocumentCommand GetDocumentCommand(ZQuery filterForMenuItem)
		{
			filterForMenuItem.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.Equal, BusinessContext);
			filterForMenuItem.AddToFilter(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Documents);
			var menuItem = Factory.LoadTop1<DocumentCommand>(filterForMenuItem);
			AssertNotNull("The menuItemFilter you specified returned no StmMenuItem.\r\n\r\n" + filterForMenuItem.ToCSharpCode(), menuItem);

			return menuItem;
		}

		#endregion
	}
}
