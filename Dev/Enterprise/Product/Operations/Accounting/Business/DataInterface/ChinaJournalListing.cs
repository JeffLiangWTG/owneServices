using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.DataInterface
{
	public class ChinaJournalListing : NonPersistentBusinessObject, IObsoleteValidation, IDocumentSupportable
	{
		public ChinaJournalListing(BusinessObjectFactory factory) : base(factory)
		{ }

		public ChinaJournalListing(BusinessObjectFactory factory, ZDateTime fromDate, ZDateTime endDate, ZString branchCode)
			: base(factory)
		{
			this.FromDate = fromDate;
			this.EndDate = endDate;
			this.BranchCode = branchCode;
		}

		public ChinaJournalCollection AccountingVouchers
		{
			get
			{
				var chinaJournalCollection = new ChinaJournalCollection(Factory);
				var endDate = EndDate.IsValid ? EndDate.Date.AddDays(1) : EndDate;
				chinaJournalCollection.AddElements(FromDate, endDate, BranchCode);
				return chinaJournalCollection;
			}
		}

		public ZDateTime FromDate;
		public ZDateTime EndDate;
		public ZString BranchCode;

		#region IDocumentSupportable Members

		[XmlIgnore]
		public DocumentSupporter DocumentSupporter
		{
			get { return new ChinaJournalListingDocumentSupporter(this); }
		}

		#endregion

		public class ChinaJournalListingDocumentSupporter : DocumentSupporter
		{
			public ChinaJournalListingDocumentSupporter(ChinaJournalListing chinaJournalListing)
				: base(chinaJournalListing)
			{ }

			protected ChinaJournalListing ChinaJournalListing
			{
				get { return (ChinaJournalListing)BusinessObject; }
			}

			#region Overrides

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return Env.Security.None; }
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.ChinaJournalListing; }
			}

			protected override Constants.DataContext[] GetSupportedDataContexts()
			{
				return new[] { Constants.DataContext.ChinaJournalListing };
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return new[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.ChinaJournalListing, ChinaJournalListing) };
			}

			#endregion
		}
	}
}
