using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CFSTallyContainerWrapper))]
	public class CFSTallyContainerWrapperTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestOutturn()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9920A";
			AssertNull(Wrapper.Outturn);
			var header1 = Factory.New<TallyOutturnHeader>();
			header1.C6_OutturningPremiseID = "9921B";
			var outturn1 = header1.Outturns.AddNew();
			Wrapper.Outturns.Add(outturn1);
			AssertEquals(outturn1, Wrapper.Outturn);

			var header2 = Factory.New<TallyOutturnHeader>();
			header2.C6_OutturningPremiseID = "9922C";
			var outturn2 = header2.Outturns.AddNew();
			Wrapper.Outturns.Add(outturn2);
			AssertEquals(outturn1, Wrapper.Outturn);

			header2.C6_OutturningPremiseID = "9920A";
			AssertEquals(outturn2, Wrapper.Outturn);
		}

		public void TestHeader()
		{
			AssertNull(Wrapper.Header);
			TallyOutturnHeader header = Factory.New<TallyOutturnHeader>();
			TallyOutturn outturn = header.Outturns.AddNew();
			Wrapper.Outturns.Add(outturn);
			AssertEquals(header, Wrapper.Header);
		}

		public void TestOutturns()
		{
			AssertNotNull(Wrapper.Outturns);
			AssertEquals(typeof(CFSTallyContainerOutturnCollection), Wrapper.Outturns.GetType());
			AssertEquals(true, Wrapper.IsRegisteredEditableChildObject(Wrapper.Outturns));
			AssertEquals(true, Wrapper.Outturns.IsLoaded);
			TallyOutturn outturn = Wrapper.Outturns.AddNew();

			AssertEquals(Wrapper, outturn.Parent);
			AssertEquals(Container, outturn.Container);
		}

		#endregion

		#region IOutturnableLine Members

		public void TestIOutturnableLinePackagesManifested()
		{
			CFSPackLine packLine = Factory.New<CFSPackLine>();
			packLine.JL_PackageCount = 8;
			Container.PackLines.Add(packLine);
			AssertEquals(8, ((IOutturnableLine)Wrapper).PackagesManifested);
		}

		#endregion

		#region Implementation

		CFSTallyContainerWrapper Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = (CFSTallyContainerWrapper)GetNewBusinessObject();
				}
				return wrapper;
			}
		}
		CFSTallyContainerWrapper wrapper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return CFSTallyContainerWrapper.Load(Container);
		}

		TallyContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Factory.New<TallyContainer>();
				}
				return container;
			}
		}
		TallyContainer container;

		#endregion
	}
}
