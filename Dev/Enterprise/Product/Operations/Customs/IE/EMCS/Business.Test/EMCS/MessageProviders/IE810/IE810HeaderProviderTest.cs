using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE810HeaderProvider))]
	class IE810HeaderProviderTest : HeaderProviderAbstractTest<IE810HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE810HeaderProvider(emcsDeclaration, null));
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
			dataProvider = new IE810HeaderProvider(emcsDeclaration, cancellationOfEad);
		}
		CancellationSendingAction cancellationOfEad;
		IE810HeaderProvider dataProvider;

		protected override IE810HeaderProvider GetHeaderProvider() => dataProvider;

		protected override IEnumerable<Expression<Func<IE810HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ComplementaryInformation;
		}
	}
}
