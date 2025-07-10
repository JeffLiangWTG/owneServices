using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ConvertUTCDateTimeToLocal))]
	sealed class ConvertUTCDateTimeToLocalTimeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertUTCDateTimeToLocal(AField)>", Passes.FirstPass));
			Assert("should not match", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertUCDateTimeToLocal>", Passes.FirstPass));
			Assert("should not match", !ValueProviderToTest.IsResponsibleForReplacing("<Convert UCTDateTime To Local>", Passes.FirstPass));

			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<ConvertUTCDateTimeToLocal('<Field>')>", Passes.FirstPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<ConvertUTCDateTimeToLocal('19 Oct 2004')>", Passes.FirstPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<ConvertUTCDateTimeToLocal('19/10/2004')>", Passes.FirstPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<ConvertUTCDateTimeToLocal('08/08/2007 14:15:30')>", Passes.FirstPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<ConvertUTCDateTimeToLocal('<ReportDate.CreationTimeUTC>')>", Passes.FirstPass));
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestConvertFromUtcToLocal()
		{
			DateTime expectedTime = new DateTime(2007, 8, 9, 0, 15, 30);
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ReportDate.CreationTimeUTC", new DateTime(2007, 8, 8, 14, 15, 30)));
			AssertEquals("Explicit DateTime", expectedTime, ValueProviderToTest.GetReplacement("<ConvertUTCDateTimeToLocal('08/08/2007 14:15:30')>", Report));
			AssertEquals("DateTime Macro", expectedTime, Report.MacroTranslator.GetValue("<ConvertUTCDateTimeToLocal('<ReportDate.CreationTimeUTC>')>", Passes.FirstPass));
		}

		[TestUtcOffset(10, 0, 0)]
		public override void TestDocumentation()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ReportDate.CreationTimeUTC", new DateTime(2007, 10, 23, 0, 30, 0)));
			base.TestDocumentation();
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ConvertUTCDateTimeToLocal();
		}

		protected override Type ValueProviderType
		{
			get { return typeof(ConvertUTCDateTimeToLocal); }
		}
	}
}
