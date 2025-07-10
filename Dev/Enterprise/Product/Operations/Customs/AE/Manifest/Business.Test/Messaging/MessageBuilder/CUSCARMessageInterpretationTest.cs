using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Edifact.D23A.Messages.CUSCAR;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class CUSCARMessageInterpretationTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestToHtml() => CombineAssertions(() =>
	{
		var message = Activator.CreateInstance<CUSCARMessage>();
		message.UNH.InstantiateAChildAndAddItToChildrenCollection().MessageReferenceNumber = "MESSAGE";
		message.BGM.InstantiateAChildAndAddItToChildrenCollection().MessageFunctionCode = MessageFunctionCodeList.GetFromString("1");
		var characterSet = AECharacterSet.New();
		var interpretation = new CUSCARMessageInterpretation(message, characterSet);
		var html = interpretation.ToHtml();
		NUnit.Framework.Assert.That(html, NUnit.Framework.Does.Contain("Line No."));
		NUnit.Framework.Assert.That(html, NUnit.Framework.Does.Contain("Segment"));
		NUnit.Framework.Assert.That(html, NUnit.Framework.Does.Contain("<td>1</td><td>UNH+MESSAGE"));
		NUnit.Framework.Assert.That(html, NUnit.Framework.Does.Contain("<td>2</td><td>BGM+++1"));
	});
}
