using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PrintSummary : NonPersistentBusinessObject, IDocumentSupportable, IObsoleteValidation
	{
		public PrintSummary(BusinessObjectFactory factory, Dictionary<string, PrintStatement> printStatements)
			: base(factory)
		{
			this.PrintStatements = printStatements;
		}

		public readonly Dictionary<string, PrintStatement> PrintStatements;

		public static new string TableName
		{
			get { return "PrintSummary"; }
		}

		#region Properties

		public PrintStatement FirstPrintStatement
		{
			get
			{
				ArrayList codes = new ArrayList(PrintStatements.Keys);
				return PrintStatements[(string)codes[0]];
			}
		}

		public OrgHeader Organisation
		{
			get
			{
				return FirstPrintStatement.Organisation;
			}
		}

		#endregion

		public DocumentSupporter DocumentSupporter
		{
			get { return new PrintSummaryDocumentSupporter(this); }
		}
	}

	public class PrintSummaryDocumentSupporter : DocumentSupporter
	{
		public PrintSummaryDocumentSupporter(PrintSummary statement)
			: base(statement)
		{
		}

		public static PrintSummaryDocumentSupporter New(PrintSummary statement)
		{
			PrintSummaryDocumentSupporter result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(statement);
			}
			else if (statement != null)
			{
				result = new PrintSummaryDocumentSupporter(statement);
			}

			return result;
		}

		public PrintSummary PrintSummary
		{
			get { return (PrintSummary)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.StatementSummary; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Enterprise.Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, PrintSummary);
			}
			else if (dataContext == Core.Constants.DataContext.StatementSummary)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(dataContext, PrintSummary) };
			}
			else
			{
				return null;
			}
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.StatementSummary };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeader orgHeader = Factory.Load<OrgHeader>(PrintSummary.Organisation.PK);
			return new OrgHeaderContact(orgHeader, null);
		}

		#endregion

		protected delegate PrintSummaryDocumentSupporter NewDelegate(PrintSummary printSummary);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
	}
}