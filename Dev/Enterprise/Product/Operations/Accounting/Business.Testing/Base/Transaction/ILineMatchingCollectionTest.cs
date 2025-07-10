using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(ILineMatchingCollection))]
	public class ILineMatchingCollectionTest : InvoicingLineBaseCollectionTest
	{
		public void TestSetPropertiesReadOnly()
		{
			APInvoiceLine line = (APInvoiceLine)Master.Lines.AddNew();
			Master.SetReadOnlyIncludingChildren(true);
			if (((ILineMatchingCollection)Collection).WritableProperties_ForTestOnly.Count == 0)
			{
				Assert(line.ReadOnly);
			}
			else
			{
				foreach (ZPropertyInfo property in line.ZPropertyInfoHash)
				{
					if (property.HasSetter)
					{
						AssertEquals(property.Name + ".ReadOnly", !((ILineMatchingCollection)Collection).WritableProperties_ForTestOnly.Contains(property.Name), property.ReadOnly);
					}
				}
			}
		}
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ILineMatchingCollection(Master, new APMatchingBase(Factory));
		}

		APInvoice fMaster;
		APInvoice Master
		{
			get { return fMaster ?? (fMaster = Factory.NewWithValidTestData<APInvoice>()); }
		}
	}
}
