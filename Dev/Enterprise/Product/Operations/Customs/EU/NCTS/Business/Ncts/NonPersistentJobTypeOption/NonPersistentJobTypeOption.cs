using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NonPersistentJobTypeOption : AutoNonPersistentJobTypeOption
	{
		public NonPersistentJobTypeOption(BusinessObjectFactory factory) : base(factory)
		{
		}

		[ResourceStringData("NonPersistentJobTypeOption.JobType", Caption = "Job Type")]
		[List(nameof(Lookups) + "." + nameof(NonPersistentJobTypeOptionLookups.JobTypeList))]
		public override ZString JobType
		{
			get { return base.JobType; }
			set { base.JobType = value; }
		}

		public NonPersistentJobTypeOptionLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new NonPersistentJobTypeOptionLookups(this);
				}

				return fLookups;
			}
		}
		NonPersistentJobTypeOptionLookups fLookups;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JobType = CusInBondApplicationCodeList.Codes.NCTS4;
		}
	}
}
