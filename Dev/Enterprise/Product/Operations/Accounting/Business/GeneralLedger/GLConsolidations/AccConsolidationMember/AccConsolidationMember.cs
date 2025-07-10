using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class AccConsolidationMember : AutoAccConsolidationMember
	{
		public AccConsolidationMember(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public AccConsolidationGroup ConsolidationGroup
		{
			get { return Factory.Load<AccConsolidationGroup>(YM_YR_ConsolidationGroup); }
		}

		[RelatedBusinessObject("ConsolidationGroup")]
		public override ZGuid YM_YR_ConsolidationGroup
		{
			get { return base.YM_YR_ConsolidationGroup; }
			set { base.YM_YR_ConsolidationGroup = value; }
		}

		public override ZGuid YM_GC_Company
		{
			get { return base.YM_GC_Company; }
			set
			{
				base.YM_GC_Company = value;
				Validation.ValidateYM_OH_Organisation();
			}
		}

		public override ZGuid YM_OH_Organisation
		{
			get { return base.YM_OH_Organisation; }
			set
			{
				base.YM_OH_Organisation = value;
				Validation.ValidateYM_GC_Company();
			}
		}

		public ZString Description
		{
			get
			{
				string result = "";

				if (Organisation != null)
				{
					result = Organisation.OH_FullNameTruncated;
				}
				else if (Company != null)
				{
					result = Company.GC_Name;
				}

				return result;
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}
	}
}