using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExportStatusRequestNCTSRoleListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCodeDescriptionPairs()
		{
			var exportStatusRequestNCTSRoleList = new ExportStatusRequestNCTSRoleList();
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(exportStatusRequestNCTSRoleList.Count, Is.EqualTo(5), "Count");
				NUnit.Framework.Assert.That(exportStatusRequestNCTSRoleList.GetDescriptionFromCode("1"), Is.EqualTo("Consignor"), "Code '1'");
				NUnit.Framework.Assert.That(exportStatusRequestNCTSRoleList.GetDescriptionFromCode("2"), Is.EqualTo("Consignee"), "Code '2'");
				NUnit.Framework.Assert.That(exportStatusRequestNCTSRoleList.GetDescriptionFromCode("3"), Is.EqualTo("Principal"), "Code '3'");
				NUnit.Framework.Assert.That(exportStatusRequestNCTSRoleList.GetDescriptionFromCode("4"), Is.EqualTo("Authorized Consignee"), "Code '4'");
				NUnit.Framework.Assert.That(exportStatusRequestNCTSRoleList.GetDescriptionFromCode("5"), Is.EqualTo("Representative"), "Code '5'");
			});
		}
	}
}
