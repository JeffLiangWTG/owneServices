using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE810HeaderProvider))]
	sealed class IE810HeaderProviderTest : HeaderProviderAbstractTest<IE810HeaderProvider>
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

		public void TestDateAndTimeOfValidationOfCancellation()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.EMCSGB_ValidationAttributeAllowed, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, false))
			{
				AssertNull(HeaderProvider.DateAndTimeOfValidationOfCancellation);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.EMCSGB_ValidationAttributeAllowed, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				AssertNotNull(HeaderProvider.DateAndTimeOfValidationOfCancellation);
				CombineAssertions(() =>
				{
					AssertEquals(0, HeaderProvider.DateAndTimeOfValidationOfCancellation.Value.Millisecond);
					AssertEquals(DateTimeKind.Unspecified, HeaderProvider.DateAndTimeOfValidationOfCancellation.Value.Kind);
				});
			}
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
