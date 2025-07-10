using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	[TestedType(typeof(AddInfoCusClassification))]
	public class AddInfoCusClassificationBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CusClassification entryHeader = Factory.New<CusClassification>();
			return new AddInfoCusClassification(entryHeader.CC_AddInfoInfo);
		}
	}
}
