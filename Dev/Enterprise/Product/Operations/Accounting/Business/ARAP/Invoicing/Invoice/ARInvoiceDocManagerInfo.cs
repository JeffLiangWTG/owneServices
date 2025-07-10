using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARInvoiceDocManagerInfo : InvoicingDocManagerInfo
	{
		protected ARInvoiceDocManagerInfo(BusinessObject parent, string docManagerCode)
			: base(parent, docManagerCode)
		{
		}

		public static new ARInvoiceDocManagerInfo New(BusinessObject parent, string docManagerCode)
		{
			var overridden = OverridableNewDelegate.Value;
			if (overridden == null)
			{
				return new ARInvoiceDocManagerInfo(parent, docManagerCode);
			}
			else
			{
				return overridden(parent, docManagerCode);
			}
		}

		protected delegate ARInvoiceDocManagerInfo NewDelegate(BusinessObject parent, string docManagerCode);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
	}
}

#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class ARInvoiceDocManagerInfoSubclass : ARInvoiceDocManagerInfo
	{
		protected ARInvoiceDocManagerInfoSubclass(BusinessObject parent, string docManagerCode)
			: base(parent, docManagerCode)
		{
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNew);
		}

		static ARInvoiceDocManagerInfo OverriddenNew(BusinessObject parent, string docManagerCode)
		{
			return new ARInvoiceDocManagerInfoSubclass(parent, docManagerCode);
		}
	}
}
#endif
#endregion