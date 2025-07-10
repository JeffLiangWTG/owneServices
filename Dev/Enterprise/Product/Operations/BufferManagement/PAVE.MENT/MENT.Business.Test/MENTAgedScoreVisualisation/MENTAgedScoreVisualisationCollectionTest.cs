using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(MENTAgedScoreVisualisationCollection))]
	class MENTAgedScoreVisualisationCollectionTest : ActiveBusinessObjectCollectionTestCase<MENTAgedScoreVisualisationCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(MENTAgedScoreVisualisationCollection);
		}

		protected override MENTAgedScoreVisualisationCollection GetCollectionToTest()
		{
			return new MENTAgedScoreVisualisationCollection(MENTTestHelper.CreateExtraction(Factory, "Froot"));
		}
	}
}
