using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class LineMergerTest : EU.Business.Testing.LineMergerTest
	{
		protected override Type GetSupportingDocumentType() => typeof(SupportingDocument);

		protected override Type ExpectedDutyCalculatorStrategyType => typeof(Declaration.DutyCalculatorStrategy);

		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(Declaration.EntryCreationStrategy) };

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new Declaration.LineMerger((JobDeclaration)declaration);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
