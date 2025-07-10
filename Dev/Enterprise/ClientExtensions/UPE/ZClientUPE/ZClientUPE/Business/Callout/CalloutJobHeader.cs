using System.Data;

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutJobHeader : JobHeader
	{
		public CalloutJobHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CalloutChargeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new CalloutChargeCollection(this);
					fCharges.SetReadOnlyIncludingChildren(true);
					fCharges.Load();
					fCharges.IsManagedForDataRefresh = true;
				}
				return fCharges;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase)
			{
				JH_JobNum = UPENumberFountains.Instance.CalloutJobHeaderNumberFountain.GetNextFormatted(Factory);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JH_GB = GlbBranch.CurrentBranch.PK;
			JH_GE = GlbDepartment.CurrentDepartment.PK;
			JH_ParentTableCode = CusHAWBSchema.Constants.Prefix;

			if (!Factory.IsConstructingNullBusinessObject)
			{
				JH_Status = JobHeaderStatus.Working.Code;
			}
		}

		CalloutChargeCollection fCharges;

		protected override bool IsChargesCollectionLoaded
		{
			get { return fCharges != null && fCharges.IsLoaded; }
		}
	}
}
