using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportDocumentSupporter : DocumentSupporter
	{
		protected AccComplianceReportDocumentSupporter(AccComplianceReport report)
			: base(report)
		{
		}

		public static AccComplianceReportDocumentSupporter New(AccComplianceReport report)
		{
			AccComplianceReportDocumentSupporter result = null;

			if (report != null)
			{
				result = new AccComplianceReportDocumentSupporter(report);
			}

			return result;
		}

		AccComplianceReport Report
		{
			get { return (AccComplianceReport)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ComplianceReport; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Enterprise.Core.Constants.DataContext.ComplianceReport)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.DocumentWrappers.DocAccComplianceReport", Report) };	// Just for now, to test it works. Later may create a new method on DocumentWrapperFactory
			}
			else
			{
				return null;
			}
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Core.Constants.DataContext.ComplianceReport };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			return null;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion
	}
}
