using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CYDReceiveAdviceLineWrapper))]
	sealed class CYDReceiveAdviceLineWrapperTest : GenericWrapperTest
	{
		public void TestWrapperMappings()
		{
			var wrapper = (CYDReceiveAdviceLineWrapper)GetSetupWrapperForDefaultFormatting();
			CombineAssertions(() =>
			{
				AssertEquals("wrapper.PreviousOnHireDate", ZDate.Today, wrapper.PreviousOnHireDate);
				AssertEquals("wrapper.OffHireDate", ZDate.Today, wrapper.OffHireDate);
			});
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (CYDReceiveAdviceLineWrapper)GetNewDocumentWrapper();
			CombineAssertions(() =>
			{
				AssertNotEquals("wrapper.PreviousOnHireDate", ZDate.Today, wrapper.PreviousOnHireDate);
				AssertNotEquals("wrapper.OffHireDate", ZDate.Today, wrapper.OffHireDate);
			});
		}

		protected override ZString ExpectedDefaultFormatting => @"
Registry : (No Default Field Value Available on Registry)";

		protected override string ExpectedFieldMap => @"
CYDReceiveAdviceLine
======================================================================
Name                                    Type
----------------------------------------------------------------------";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CYDReceiveAdviceLineWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var receiveAdviceLine = GetNewReceiveLine();
			return new CYDReceiveAdviceLineWrapper(receiveAdviceLine, Factory);
		}

		CYDReceiveAdviceLine GetNewReceiveLine()
		{
			var receiveAdviceLine = Factory.New<CYDReceiveAdviceLine>();
			receiveAdviceLine.YRL_OffHireDate = ZDate.Today;
			receiveAdviceLine.YRL_PreviousOnHireDate = ZDate.Today;
			return receiveAdviceLine;
		}
	}
}
