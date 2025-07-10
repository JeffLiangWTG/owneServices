using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	[Serializable]
	public class UnableToFindInvoiceDocumentCommandException : Exception
	{
		public UnableToFindInvoiceDocumentCommandException() : base() { }

		public UnableToFindInvoiceDocumentCommandException(string message) : base(message) { }

#if NETFRAMEWORK
		protected UnableToFindInvoiceDocumentCommandException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	public class InvoicePrintCommandManager
	{
		#region Constructors and Type Overriding

		protected InvoicePrintCommandManager(InvoicingBase invoice, ZString menuName, bool isLegacyDocument = false)
		{
			this.invoice = invoice;
			this.menuName = menuName;
			this.isLegacyDocument = isLegacyDocument;
		}

		protected InvoicePrintCommandManager(InvoicingBase invoice, ZGuid menuPK, bool isLegacyDocument = false)
		{
			this.invoice = invoice;
			this.menuPK = menuPK;
			this.isLegacyDocument = isLegacyDocument;
		}

		public static InvoicePrintCommandManager New(InvoicingBase invoice, ZString menuName, bool isLegacyDocument = false)
		{
			InvoicePrintCommandManager result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(invoice, menuName, isLegacyDocument);
			}
			else if (invoice != null)
			{
				result = new InvoicePrintCommandManager(invoice, menuName, isLegacyDocument);
			}
			return result;
		}

		public static InvoicePrintCommandManager New(InvoicingBase invoice, ZGuid menuPK, bool isLegacyDocument = false)
		{
			InvoicePrintCommandManager result = null;
			if (invoice != null)
			{
				result = new InvoicePrintCommandManager(invoice, menuPK, isLegacyDocument);
			}
			return result;
		}

		protected readonly InvoicingBase invoice;
		protected ZString menuName;
		protected ZGuid menuPK;
		readonly bool isLegacyDocument;

		protected delegate InvoicePrintCommandManager NewDelegate(InvoicingBase invoice, ZString menuName, bool isLegacyDocument = false);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		public PrintTask GetNewPrintTaskWithChildCommandsLoaded(bool shouldCreateeDocs = true)
		{
			PrintTask task = new PrintTask();

			PrintTaskDocumentPackLoader loader = new PrintTaskDocumentPackLoader(task, Command, null, shouldCreateeDocs: shouldCreateeDocs);

			if (IsUsingDocBuilderInvoice)
			{
				loader.LoadChildCommands();
			}
			else
			{
				loader.LoadChildCommandsFromProviderPlaceholder();
			}

			return task;
		}

		public DocumentCommand Command
		{
			get { return command ?? (command = GetInvoicePrintCommand()); }
		}
		DocumentCommand command;

		DocumentCommand GetInvoicePrintCommand()
		{
			if (menuPK.IsValid)
			{
				DocumentCommand command = invoice.Factory.Load<DocumentCommand>(menuPK);
				if (command != null)
				{
					command.Parent = invoice;
					return command;
				}
				else
				{
					throw new UnableToFindInvoiceDocumentCommandException("Unable to find Invoice document command for transaction " + invoice.AH_TransactionNum);
				}
			}
			else
			{
				BusinessObject[] printInvoiceCommands = null;

				if (IsUsingDocBuilderInvoice)
				{
					if (InvoiceTypeCalculationProvider.IsDeferredInvoiceType(invoice.AH_TransactionCategory))
					{
						printInvoiceCommands = GetPrintInvoiceMenuCommand(invoice, false);
					}
					else
					{
						IDocumentSupportable documentSupportableParentJob = new InvoicePrintTask.ParentJobLoader(invoice.Factory).Load(invoice) as IDocumentSupportable;
						if (documentSupportableParentJob != null)
						{
							printInvoiceCommands = GetPrintInvoiceMenuCommand(documentSupportableParentJob, false);
						}
					}
				}

				if (printInvoiceCommands == null || printInvoiceCommands.Length == 0)
				{
					printInvoiceCommands = GetPrintInvoiceMenuCommand(invoice, isLegacyDocument);
				}

				if (printInvoiceCommands == null || printInvoiceCommands.Length != 1)
				{
					throw new UnableToFindInvoiceDocumentCommandException(Res.GetString("2AFD64CB-0B77-44CD-B3F1-9A429847F010", "Unable to find Invoice document command for transaction {0} with menu name: {1}, menu path: {2}. Total commands found: {3}.", invoice.AH_TransactionNum, menuName, GetMenuPath(isLegacyDocument), printInvoiceCommands?.Length ?? 0));
				}

				return (DocumentCommand)printInvoiceCommands[0];
			}
		}

		BusinessObject[] GetPrintInvoiceMenuCommand(IDocumentSupportable parentContainingMenuCommand, bool isLegacyDocument)
		{
			var documentCommands = new DocumentCommandCollection(parentContainingMenuCommand);
			documentCommands.Load();

			var menuFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
			menuFilter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, ZBool.True);
			menuFilter.AddToFilter(StmMenuItemSchema.SU_MenuPath, GetMenuPath(isLegacyDocument));
			var result = documentCommands.Find(menuFilter);

			if (result.Length == 0)
			{
				menuFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
				menuFilter.AddToFilter(StmMenuItemSchema.SU_MenuPath, GetMenuPath(isLegacyDocument));
				result = documentCommands.Find(menuFilter);
			}
			return result;
		}

		string GetMenuPath(bool isLegacyDocument)
		{
			return isLegacyDocument ? Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments : string.Empty;
		}

		bool IsUsingDocBuilderInvoice
		{
			get { return menuName == JobInvoicingEDocsProviderSupporter.DocBuilderInvoiceName; }
		}
	}
}
