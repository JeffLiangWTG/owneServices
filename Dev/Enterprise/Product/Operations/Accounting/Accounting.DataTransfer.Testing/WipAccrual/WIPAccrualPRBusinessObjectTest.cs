using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.WIPAccrual;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.WipsAndAccruals.Testing
{
	[TestedType(typeof(WIPAccrualPRBusinessObject))]
	public class WIPAccrualPRBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPostedWIP()
		{
			TestWIPObj.AL_PostDate = new ZDateTime(2005, 02, 02);
			WIPAccrualPRBusinessObject bizObj = new WIPAccrualPRBusinessObject(TestWIPObj, Xsd.WipOrAccrualPostOrReverse.P);
			AssertNotNull(bizObj.WIP);
			AssertEquals(Xsd.WipOrAccrualPostOrReverse.P, bizObj.PostedOrReverseStatus);
		}

		public void TestReversedWIP()
		{
			TestWIPObj.AL_PostDate = new ZDateTime(2005, 02, 02);
			TestWIPObj.AL_ReverseDate = new ZDateTime(2005, 02, 02);
			WIPAccrualPRBusinessObject bizObj = new WIPAccrualPRBusinessObject(TestWIPObj, Xsd.WipOrAccrualPostOrReverse.R);
			AssertNotNull(bizObj.WIP);
			AssertEquals(Xsd.WipOrAccrualPostOrReverse.R, bizObj.PostedOrReverseStatus);
		}

		WIP TestWIPObj
		{
			get { return Factory.NewWithValidTestData<WIP>(); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WIPAccrualPRBusinessObject(TestWIPObj, Xsd.WipOrAccrualPostOrReverse.P);
		}
	}
}
