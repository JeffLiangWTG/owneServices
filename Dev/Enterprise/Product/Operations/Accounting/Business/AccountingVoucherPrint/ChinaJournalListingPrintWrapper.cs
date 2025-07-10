using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingVoucherPrint
{
	public class ChinaJournalListingPrintWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ChinaJournalListingPrintWrapper()
			: base(new BusinessObjectFactory())
		{ }

		public ChinaJournalListingPrintWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Branch

		public ZString BranchCode
		{
			get
			{
				var selectedBranch = Factory.Load<GlbBranch>(Branch);
				return selectedBranch == null ? ZString.Empty : selectedBranch.GB_Code;
			}
		}

		[List("Branches")]
		public ZGuid Branch
		{
			get
			{
				return fBranch;
			}
			set
			{
				if (fBranch != value)
				{
					SetNonPersistentPropertyValue(BranchInfo, ref fBranch, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateBranch();
				}
			}
		}

		ZGuid fBranch;

		public ZPropertyInfo BranchInfo
		{
			get { return GetZPropertyInfo(nameof(Branch)); }
		}

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}
				return fBranches;
			}
		}

		GlbBranchCollection fBranches;

		#endregion

		#region From Period

		public ZInt FromPeriod
		{
			get { return fromPeriod; }
			set
			{
				SetNonPersistentPropertyValue(FromPeriodInfo, ref fromPeriod, value);
				SetDatesFromFromPeriod();
				FromDateInfo.RefreshBinding();
				EndDateInfo.RefreshBinding();
			}
		}

		ZInt fromPeriod;

		public ZPropertyInfo FromPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(FromPeriod)); }
		}

		void SetDatesFromFromPeriod()
		{
			if (!fromPeriod.IsEmpty)
			{
				fromDate = PeriodCalculator.GetFirstDayForPeriod(FromPeriod);
				endDate = PeriodCalculator.GetLastDayForPeriod(FromPeriod);
			}
		}

		AccountingPeriodCalculator PeriodCalculator
		{
			get { return fPeriodCalculator ?? (fPeriodCalculator = new AccountingPeriodCalculator(Factory)); }
		}

		AccountingPeriodCalculator fPeriodCalculator;

		#endregion

		#region End Date

		public ZDateTime EndDate
		{
			get { return endDate; }
			set { SetNonPersistentPropertyValue(EndDateInfo, ref endDate, value); }
		}

		public ZPropertyInfo EndDateInfo
		{
			get { return GetZPropertyInfo(nameof(EndDate)); }
		}

		ZDateTime endDate;

		#endregion

		#region From Date

		public ZDateTime FromDate
		{
			get { return fromDate; }
			set { SetNonPersistentPropertyValue(FromDateInfo, ref fromDate, value); }
		}

		ZDateTime fromDate;

		public ZPropertyInfo FromDateInfo
		{
			get { return GetZPropertyInfo(nameof(FromDate)); }
		}

		#endregion

		#region Validation

		public void ValidateFromPeriod()
		{
			FromPeriodInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FromPeriodInfo);

			if (!PeriodCalculator.IsPeriodGLClosed(FromPeriod))
			{
				FromPeriodInfo.AddError(Res.GetString("9fe76c8f-1806-4eb7-9c7f-ce442dbf7ba8", "This period is not closed."));
			}
		}

		public void ValidateFromDate()
		{
			FromDateInfo.ClearAllNotifications();

			TypeValidation.CheckValidZDateTimeWithoutRange(FromDateInfo);
			TypeValidation.CheckValidZDateTimeRange(FromDateInfo);
			MandatoryValidation.CheckEntered(FromDateInfo);

			if ((PeriodCalculator.GetFirstDayForPeriod(FromPeriod) > FromDate) || (PeriodCalculator.GetLastDayForPeriod(FromPeriod) < FromDate))
			{
				FromDateInfo.AddError(Res.GetString("26876afc-c88f-44e9-81fd-75dc33a31bdc", "From Date must be within the Period specified."));
			}
		}

		public void ValidateEndDate()
		{
			EndDateInfo.ClearAllNotifications();

			TypeValidation.CheckValidZDateTimeWithoutRange(EndDateInfo);
			TypeValidation.CheckValidZDateTimeRange(EndDateInfo);
			MandatoryValidation.CheckEntered(EndDateInfo);

			if ((PeriodCalculator.GetFirstDayForPeriod(FromPeriod) > EndDate) || (PeriodCalculator.GetLastDayForPeriod(FromPeriod) < EndDate))
			{
				EndDateInfo.AddError(Res.GetString("20d4bde7-26d1-4b74-b6e4-592c589b3ae2", "End Date must be within the Period specified."));
			}
		}

		public void ValidateBranch()
		{
			BranchInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(BranchInfo, Branches, ResString.GetMultilingualString("2a3adf82-0af7-47b2-9b68-a25c2ce0c0f8", "Please enter a valid Branch."));
			if (!Branch.IsEmpty)
			{
				BranchInfo.AddWarning(ResString.GetMultilingualString("B99AAB84-298C-4968-8749-0B38147C9693", "You cannot select Branch Filter if you are printing Job Costing Voucher."));
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateFromDate();
			ValidateEndDate();
			ValidateFromPeriod();
			ValidateBranch();
		}

		#endregion

		public void PrintChinaJournalListingDocument()
		{
			PrintUtil = new AccPrintingUtility(Factory, Constants.DataContext.ChinaJournalListing);
			if (PrintUtil != null & GetWrapperCount() > 0)
			{
				PrintCore();
			}
		}

#if DEBUG
		virtual
#endif
		protected void PrintCore()
		{
			PrintUtil.PrintDocument(ChinaJournalListingLineDocWrappers, "ChinaJournalListing", DocumentEngine.AllowedDeliveryOptions.All);
		}

		public ZInt GetWrapperCount()
		{
			ChinaJournalListingLineDocWrappers = GenerateDocWrapper();
			return (ChinaJournalListingLineDocWrappers == null ? 0 : ChinaJournalListingLineDocWrappers.Length);
		}

		public DocumentWrapper[] GenerateDocWrapper()
		{
			if (ChinaJournalListingLineDocWrappers != null && ChinaJournalListing != null && ChinaJournalListing.FromDate == fromDate && ChinaJournalListing.EndDate == endDate && ChinaJournalListing.BranchCode == BranchCode)
			{
				return ChinaJournalListingLineDocWrappers;
			}

			var wrappersToReturn = new ArrayList();

			ChinaJournalListing = new ChinaJournalListing(Factory, fromDate, endDate, BranchCode);
			DocumentWrapper wrapper = DocumentWrapperFactory.CreateWrapper(Constants.DataContext.ChinaJournalListing, ChinaJournalListing);
			if (wrapper != null)
			{
				wrappersToReturn.Add(wrapper);
			}

			ChinaJournalListingLineDocWrappers = (DocumentWrapper[])wrappersToReturn.ToArray(typeof(DocumentWrapper));

			return ChinaJournalListingLineDocWrappers;
		}

		ChinaJournalListing ChinaJournalListing;
		AccPrintingUtility PrintUtil;
		DocumentWrapper[] ChinaJournalListingLineDocWrappers;
	}
}
