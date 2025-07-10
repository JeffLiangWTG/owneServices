using System.ComponentModel;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Billing
{
	public class AccBillingHeader : AutoAccBillingHeader, IAccountingNumberFountainDataSource
	{
		public AccBillingHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABH_GC_Company = GlbCompany.CurrentCompany.PK;
			ABH_GS_NKEventUser = GlbStaff.CurrentUser.GS_Code;
		}

		#region Lines

		[ChildEditable()]
		public AccBillingItemCollection BillingItems
		{
			get
			{
				if (billingItems == null)
				{
					billingItems = new AccBillingItemCollection(this, Factory);
					if (IsInDatabase)
					{
						billingItems.Load(new ZQuery(AccBillingItemSchema.ABI_ABH, SQLComparisonOperator.Equal, PK));
					}
					RegisterEditableChildObject(billingItems);
				}
				return billingItems;
			}
		}
		AccBillingItemCollection billingItems;

		#endregion

		public override void Delete()
		{
			BillingItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				ABH_InternalReferenceNumber = AccountingNumberFountainWrapperFactory.Instance.AccBillingHeaderInternalRef.Generate(this);
			}
			base.OnSaving();
		}

		#region IAccountingNumberFountainDataSource

		public ZDateTime PostDate => ABH_SystemCreateTimeUtc;

		public GlbBranch Branch => GlbBranch.CurrentBranch;

		public GlbDepartment Department => GlbDepartment.CurrentDepartment;

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			NewBusinessObjectTestDataHelper().FillWithValidTestData(this, kind, propertyPath);
			ABH_BillingCode = "GSH";
			ABH_EventType = "PST";
			ABH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			ABH_BillingCounter = 1;
		}

#endif
	}
}
