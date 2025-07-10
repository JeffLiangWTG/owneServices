using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryHeader))]
	public class AddInfoCusEntryHeaderTest : EU.Business.Declaration.Testing.AddInfoCusEntryHeaderBOTest
	{
		protected override BusinessObject GetNewBusinessObject()
			=> new AddInfoCusEntryHeader(Factory.New<CusEntryHeader>().CH_AddInfoInfo);
	}
}
