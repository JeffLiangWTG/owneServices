using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(StmTranslationFeedback))]
	sealed class StmTranslationFeedbackTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<StmTranslationFeedback>();
		}

		protected override void SetUp()
		{
			mockSources = ResourceStringsFactory.MockSources();
			base.SetUp();
		}

		protected override void TearDown()
		{
			mockSources.Dispose();
			base.TearDown();
		}

		IDisposable mockSources;
	}
}
