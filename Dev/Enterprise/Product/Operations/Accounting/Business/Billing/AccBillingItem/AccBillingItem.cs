using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Billing
{
	[DependentBusinessObject(typeof(AccBillingHeader), "BillingItems")]
	public class AccBillingItem : AutoAccBillingItem
	{
		public AccBillingItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("BillingHeader")]
		public override ZGuid ABI_ABH
		{
			get => base.ABI_ABH;
			set => base.ABI_ABH = value;
		}

		public virtual AccBillingHeader BillingHeader
		{
			get { return Factory.Load<AccBillingHeader>(ABI_ABH); }
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			NewBusinessObjectTestDataHelper().FillWithValidTestData(this, kind, propertyPath);
			ABI_ParentTableCode = JobShipmentSchema.Constants.Prefix;
		}
#endif
	}
}
