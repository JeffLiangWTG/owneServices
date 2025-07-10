using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APJournalCollection : ActiveBusinessObjectCollection<APJournal>
	{
		public APJournalCollection(BusinessObjectFactory factory, MatchingBase matchingBO, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
			this.matchingBO = matchingBO;
		}

		readonly MatchingBase matchingBO;

		public Action<AccGLHeaderCollection, List<AccGLHeader>> ShowGLAccountsForImportAction;

		protected override void SetDefaultsForNewElementCore(APJournal newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ShowGLAccountsForImportAction = ShowGLAccountsForImportAction;
			newElement.EnableCheckSubAccountsForGLHeader = true;
			newElement.AH_InvoiceDate = matchingBO.MatchDate;
			newElement.AH_PostDate = Journal.Journal.GetPostDate(matchingBO.MatchDate);
			newElement.AH_OH = matchingBO.PrimaryOrganization;
			newElement.AH_AG = (Guid)AccountingConfigurationRegistry.Instance.APJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public void SetUseJournalValidation(bool value)
		{
			foreach (APJournal journal in this)
			{
				journal.UseJournalValidation = value;
			}
		}
	}
}
