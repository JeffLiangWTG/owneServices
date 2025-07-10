using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Client.ATL.DocWrappers;
using Enterprise.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Client.ATL.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestInitialiseAndUnitialise()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			APPayment payment = factory.NewWithValidTestData<APPayment>();
			ClientOverride.Instance.Uninitialise();
			AssertEquals(typeof(DocAPPayment), DocAPPayment.New(payment, factory).GetType());
			ClientOverride.Instance.Initialise();
			AssertEquals(typeof(DocATLAPPayment), DocAPPayment.New(payment, factory).GetType());
			ClientOverride.Instance.Uninitialise();
			AssertEquals(typeof(DocAPPayment), DocAPPayment.New(payment, factory).GetType());
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
