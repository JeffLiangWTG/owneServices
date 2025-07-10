using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED810HeaderProvider))]
	class ED810HeaderProviderTest : HeaderProviderAbstractTest<ED810HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED810HeaderProvider(emcsDeclaration, null));
		}

		public void TestCancellationReasonCode()
		{
			AssertEquals("Empty string defaults to Zero", 0, HeaderProvider.CancellationReasonCode);
			cancellationOfEad.Reason = "B";
			AssertEquals("Character defaults to Zero", 0, HeaderProvider.CancellationReasonCode);
			cancellationOfEad.Reason = "2";
			AssertEquals("Numeric is parsed", 2, HeaderProvider.CancellationReasonCode);
		}

		public void TestComplementaryInformation()
		{
			AssertEquals("CUSTOMS MESSAGE REMARKS", HeaderProvider.ComplementaryInformation.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cancellationOfEad = new CancellationSendingAction(emcsDeclaration);
			cancellationOfEad.Information = "CUSTOMS MESSAGE REMARKS";
			dataProvider = new ED810HeaderProvider(emcsDeclaration, cancellationOfEad);
		}
		CancellationSendingAction cancellationOfEad;
		ED810HeaderProvider dataProvider;

		protected override ED810HeaderProvider GetHeaderProvider() => dataProvider;

		protected override IEnumerable<Expression<Func<ED810HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ComplementaryInformation;
		}
	}
}
