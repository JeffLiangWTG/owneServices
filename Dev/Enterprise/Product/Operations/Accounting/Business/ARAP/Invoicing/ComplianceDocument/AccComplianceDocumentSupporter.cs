using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentSupporter : DocumentSupporter
	{
		public AccComplianceDocumentSupporter(AccComplianceDocumentHeader complianceDocumentHeader)
			: base(complianceDocumentHeader)
		{
		}

		protected AccComplianceDocumentHeader ComplianceDocumentHeader
		{
			get { return (AccComplianceDocumentHeader)BusinessObject; }
		}

		#region Overrides

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			var result = base.GetDataStateBeforeRun(commandAboutToBeRun);
			if (result.IsValid && commandAboutToBeRun != null && ComplianceDocumentHeader.ShouldPreventPrintDocument)
			{
				result = new DocumentSupporterDataState(false, Res.GetString("F9E4F6B3-B0CF-4091-9D0E-66B579AB32F1", "Cannot produce this TXE Document because the Debtor's organization category is 'NAT' and has a MID-Mobile Carrier ID / PIG-Public Interest Group registration code."));
			}
			return result;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return ComplianceDocumentHeader.ADH_Ledger == LedgerTypes.AccountsReceivable ? Env.Security.CustomizeReceivablesComplianceDocuments : Env.Security.None; }
		}

		public override BusinessContext BusinessContext
		{
			get
			{
				BusinessContext context;
				if (ComplianceDocumentHeader.ADH_Ledger == LedgerTypes.AccountsPayable)
				{
					context = BusinessContext.APComplianceDocument;
				}
				else if (ComplianceDocumentHeader.ADH_Ledger == LedgerTypes.AccountsReceivable)
				{
					context = BusinessContext.ARComplianceDocument;
				}
				else
				{
					context = BusinessContext.INVALID;
				}

				return context;
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == DataContext.ARComplianceDocument && ComplianceDocumentHeader.ADH_Ledger == LedgerTypes.AccountsReceivable)
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.Taiwan)
				{
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateAccountingWrapper(DataContext.ARComplianceDocument, ComplianceDocumentHeader, CountryCodes.Taiwan) };
				}
				else
				{
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateAccountingWrapper(DataContext.ARComplianceDocument, ComplianceDocumentHeader) };
				}
			}
			else
			{
				return null;
			}
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return BusinessObject is APComplianceDocumentHeader ? Array.Empty<DataContext>() : new DataContext[] { Constants.DataContext.ARComplianceDocument };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new OrgHeaderContact(ComplianceDocumentHeader.Organisation, null);
		}

		#endregion
	}
}
