using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class StandardSummaryWriterFactoryTest : TestCase
	{
		public void TestGetWriter()
		{
			CombineAssertions(() =>
			{
				AssertWriterType(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN, typeof(ADNStandardSummaryWriter));
				AssertWriterType(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, typeof(IATAStandardSummaryWriter));
				AssertWriterType(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.RID, typeof(RIDStandardSummaryWriter));
				AssertWriterType(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADR, typeof(ADRStandardSummaryWriter));
				AssertWriterType(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR, typeof(CFRStandardSummaryWriter));
				AssertWriterType(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.JTT, typeof(JTTStandardSummaryWriter));
				AssertWriterType(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, typeof(IMOStandardSummaryWriter));
				AssertWriterType("Test Standard", typeof(IMOStandardSummaryWriter), $"{nameof(StandardSummaryWriterFactory.GetWriter)} method should return {typeof(IMOStandardSummaryWriter)} for any other standard");
				AssertWriterType("", typeof(IMOStandardSummaryWriter), $"{nameof(StandardSummaryWriterFactory.GetWriter)} method should return {typeof(IMOStandardSummaryWriter)} for empty standard");
				AssertWriterType(null, typeof(IMOStandardSummaryWriter), $"{nameof(StandardSummaryWriterFactory.GetWriter)} method should return {typeof(IMOStandardSummaryWriter)} for null standard");
			});
		}

		void AssertWriterType(string standard, Type expectedType, string errorMsg)
		{
			var writer = StandardSummaryWriterFactory.GetWriter(standard);
			AssertNotNull(errorMsg, writer);
			AssertEquals(errorMsg, expectedType, writer.GetType());
		}

		void AssertWriterType(string standard, Type expectedType)
		{
			AssertWriterType(standard, expectedType, $"{nameof(StandardSummaryWriterFactory.GetWriter)} method should return {expectedType} for standard {standard}");
		}
	}
}
