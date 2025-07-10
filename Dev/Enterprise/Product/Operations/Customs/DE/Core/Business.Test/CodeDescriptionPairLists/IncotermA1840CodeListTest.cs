using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class IncotermA1840CodeListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCodeDescriptionPairs()
		{
			var importIncotermList = new IncotermA1840CodeList();
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(importIncotermList.Count, Is.EqualTo(12), "Count");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("CFR"), Is.EqualTo("Cost And Freight"), "Code 'CFR'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("CIF"), Is.EqualTo("Cost, Insurance And Freight"), "Code 'CIF'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("CIP"), Is.EqualTo("Carriage and Insurance Paid To"), "Code 'CIP'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("CPT"), Is.EqualTo("Carriage Paid To"), "Code 'CPT'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("DAP"), Is.EqualTo("Delivered At Place"), "Code 'DAP'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("DAT"), Is.EqualTo("Delivered At Terminal"), "Code 'DAT'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("DDP"), Is.EqualTo("Delivered Duty Paid"), "Code 'DDP'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("DPU"), Is.EqualTo("Delivered at Place Unloaded"), "Code 'DPU'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("EXW"), Is.EqualTo("Ex Works"), "Code 'EXW'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("FAS"), Is.EqualTo("Free Alongside Ship"), "Code 'FAS'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("FCA"), Is.EqualTo("Free Carrier"), "Code 'FCA'");
				NUnit.Framework.Assert.That(importIncotermList.GetDescriptionFromCode("FOB"), Is.EqualTo("Free On Board"), "Code 'FOB'");
			});
		}
	}
}
