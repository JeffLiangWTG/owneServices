using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;

namespace Enterprise.DocumentWrappers
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class DocCommissionPayment : DocBaseWrapper
	{
		#region Constructors

		protected DocCommissionPayment(ICommissionPayable commissionPayable, BusinessObjectFactory factory)
			: base(commissionPayable, factory)
		{
		}

		public static DocCommissionPayment New(ICommissionPayable commissionPayable, BusinessObjectFactory factory)
		{
			if (commissionPayable == null)
			{
				return null;
			}

			return new DocCommissionPayment(commissionPayable, factory);
		}

		#endregion

		#region WrappedObject

		new ICommissionPayable WrappedObject
		{
			get { return (ICommissionPayable)base.WrappedObject; }
		}

		#endregion

		#region Properties

		public ZString BatchNumber
		{
			get { return WrappedObject.BatchNumber; }
		}

		#endregion

		#region EntityTotalGroupings

		public DocViewCommissionLineGroupingCollection EntityTotalGroupings
		{
			get
			{
				if (entityTotalGroupings == null)
				{
					var entityGroupedLines =
						from line in WrappedObject.CommissionLinesForPayment
						group line by new { line.VCL_GS_NKStaff, line.VCL_OH_Party, line.VCL_GC_PreferredPaymentCompany } into grp
						select grp;

					var wrappedEntityTotalGroupings = new ViewCommissionLineGroupingCollection(Factory);
					foreach (var grouping in entityGroupedLines)
					{
						wrappedEntityTotalGroupings.AddNew(grouping, null);
					}

					entityTotalGroupings = new DocViewCommissionLineGroupingCollection(wrappedEntityTotalGroupings, Factory);
				}

				return entityTotalGroupings;
			}
		}
		DocViewCommissionLineGroupingCollection entityTotalGroupings;

		#endregion

		#region EntitySummaryGroupings

		public DocViewCommissionLineGroupingCollection EntitySummaryGroupings
		{
			get
			{
				if (entitySummaryGroupings == null)
				{
					var entityAndCompanyGroupedLines =
						from line in WrappedObject.CommissionLinesForPayment
						group line by new { line.VCL_GS_NKStaff, line.VCL_OH_Party, line.VCL_GC_PreferredPaymentCompany, line.VCL_GC_Company } into grp
						select grp;

					var wrappedEntitySummaryGroupings = new ViewCommissionLineGroupingCollection(Factory);
					foreach (var grouping in entityAndCompanyGroupedLines)
					{
						wrappedEntitySummaryGroupings.AddNew(grouping, null);
					}

					entitySummaryGroupings = new DocViewCommissionLineGroupingCollection(wrappedEntitySummaryGroupings, Factory);
				}

				return entitySummaryGroupings;
			}
		}
		DocViewCommissionLineGroupingCollection entitySummaryGroupings;

		#endregion

		#region CommissionDetails

		public DocViewCommissionLineGroupingCollection CommissionDetails
		{
			get
			{
				if (commissionDetails == null)
				{
					var entityAndCompanyAndSourceGroupedLines =
						from line in WrappedObject.CommissionLinesForPayment
						group line by new { line.VCL_GS_NKStaff, line.VCL_OH_Party, line.VCL_GC_PreferredPaymentCompany, line.VCL_GC_Company, line.VCL_GroupingSourceID } into grp
						select grp;

					var wrappedDetailGroupings = new ViewCommissionLineGroupingCollection(Factory);
					foreach (var grouping in entityAndCompanyAndSourceGroupedLines)
					{
						wrappedDetailGroupings.AddNew(grouping, null);
					}

					commissionDetails = new DocViewCommissionLineGroupingCollection(wrappedDetailGroupings, Factory);
				}

				return commissionDetails;
			}
		}
		DocViewCommissionLineGroupingCollection commissionDetails;

		#endregion
	}
}
