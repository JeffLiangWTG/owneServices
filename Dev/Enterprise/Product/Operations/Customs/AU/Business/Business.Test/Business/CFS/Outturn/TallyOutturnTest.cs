using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(TallyOutturn))]
	public class TallyOutturnTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContainer()
		{
			AssertNull(outturn.Container);
			TallyContainer container = Factory.New<TallyContainer>();
			outturn.Parent = CFSTallyContainerWrapper.Load(container);
			AssertEquals(container, outturn.Container);
			AssertEquals(typeof(TallyContainer), outturn.Container.GetType());
		}

		public void TestHeader()
		{
			AssertNull(outturn.Header);
			TallyOutturnHeader header = Factory.New<TallyOutturnHeader>();
			header.Outturns.Add(outturn);
			AssertEquals(header, outturn.Header);
			AssertEquals(typeof(TallyOutturnHeader), outturn.Header.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			TallyOutturn result = (TallyOutturn)base.GetNewBusinessObjectForDeleteTest(factory);
			result.Messages.RemoveAndDeleteAll();
			return result;
		}

		class OutturnLinkableTestHelper : IOutturnLinkable
		{
			void IOutturnLinkable.SetOutturnLink(IOutturnLink listener)
			{
				this.Listener = listener;
			}

			public IOutturnLink Listener;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<TallyOutturn>();
		}

		protected override void SetUp()
		{
			base.SetUp();

			outturn = Factory.New<TallyOutturn>();
		}

		TallyOutturn outturn;

		#endregion
	}
}
