using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AddInfoQuarantineExDocHeader))]
	sealed class AddInfoQuarantineExDocHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var quarantineHeader = helper.Header1.QuarantineExDocHeader;
			quarantineHeader.QH_AuthorisationEstablishment = "1234";
			quarantineHeader.QH_StorageEstablishment = "1234";
			return new AddInfoQuarantineExDocHeader(quarantineHeader.QH_AddInfoInfo);
		}
	}
}
