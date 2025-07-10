using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionDocumentPivotView))]
	internal sealed class OperationalActionDocumentPivotViewTest : BusinessObjectCollectionViewTestCase<OperationalActionDocumentPivotView>
	{
		protected override OperationalActionDocumentPivotView GetCollectionToTest()
		{
			OperationalAction action = Factory.New<OperationalAction>();
			return new OperationalActionDocumentPivotView(action.DocumentPivots);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OperationalActionDocumentPivot>();
		}
	}
}
