using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Module.Test
{
	sealed class EntryHeaderFilterUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestNewZFilterStrip()
		{
			using (var control = new EntryHeaderFilterUserControlForTest(Factory))
			using (var filterStrip = control.NewZFilterStripForTest())
			{
				AssertType(typeof(EntryHeaderStrip), filterStrip);
			}
		}

		sealed class EntryHeaderFilterUserControlForTest : EntryHeaderFilterUserControl
		{
			public EntryHeaderFilterUserControlForTest(BusinessObjectFactory factory) : base(new ActiveCusEntryHeaderCollection(factory.New<JobDeclaration>()), new EntryHeaderFilterBusinessObject())
			{
			}

			public ZFilterStrip NewZFilterStripForTest() => NewZFilterStrip();
		}
	}
}
