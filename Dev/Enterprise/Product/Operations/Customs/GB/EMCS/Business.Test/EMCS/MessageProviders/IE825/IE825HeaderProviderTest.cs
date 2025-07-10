using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE825HeaderProvider))]
	sealed class IE825HeaderProviderTest : HeaderProviderAbstractTest<IE825HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE825HeaderProvider(null));
		}

		public void TestUpstreamArc()
		{
			AssertEquals(nameof(headerProvider.UpstreamArc), "20GB41000000001870745", HeaderProvider.UpstreamArc);
		}

		public void TestMemberStateCode()
		{
			AssertEquals(nameof(headerProvider.MemberStateCode), Constants.CountryCodes.Portugal, HeaderProvider.MemberStateCode);
		}

		public void TestsplitDetails_Empty()
		{
			AssertEquals(true, HeaderProvider.SplitDetails.Count == 0);
		}

		public void TestSplitDetails()
		{
			emcsDeclaration.Invoices.AddNew();
			emcsDeclaration.Invoices.AddNew();
			AssertEquals("Splits returned", 2, HeaderProvider.SplitDetails.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			emcsDeclaration.EADNumber = "20GB41000000001870745";
			emcsDeclaration.ZG_CCTMSA = Constants.CountryCodes.Portugal;
			dataProvider = new IE825HeaderProvider(emcsDeclaration);
		}
		IE825HeaderProvider dataProvider;

		protected override IE825HeaderProvider GetHeaderProvider() => dataProvider;

		protected override IEnumerable<Expression<Func<IE825HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.SplitDetails;
		}
	}
}
