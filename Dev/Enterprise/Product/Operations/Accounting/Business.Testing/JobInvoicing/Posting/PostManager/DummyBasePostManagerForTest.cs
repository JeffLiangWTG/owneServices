using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestClass]
	class DummyBasePostManagerForTest : BasePostManager
	{
		public DummyBasePostManagerForTest(BusinessObjectFactory fallbackFactory, IEnumerable<Job> jobs) : base(fallbackFactory, jobs) { }

		public Charge[] OnlyForTestListOfCharges
		{
			get
			{
				return Charges;
			}
		}
	}
}