using System.Collections.Generic;
using CargoWise.Customs.NL.MessageDefinitions.DMS;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AmendmentMessagePrettierTest : TestCaseWithFactory
{
	public void TestGetFormatted()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_BGMReference = "123";
		entryHeader.MovementReferenceNumberSetter("MRN123");
		var message = entryHeader.Messages.AddNew();

		AssertEquals("<font size='2' face='Courier New'><H1>Request to Amend</H1><table style='margin-left:10pt'><tr><td><b>MRN:</b></td><td><i>MRN123</i></td></tr><tr><td><b>Funct. Reference ID:</b></td><td><i>123</i></td></tr></table><H2>Changes</H2><table style='margin-left:10pt'><tr><td><b>Name path:</b></td><td><i>Declaration/GoodsShipment/XXXX</i></td></tr><tr><td><b>Old value:</b></td><td><i>Old value</i></td></tr><tr><td><b>New value:</b></td><td><i>New value</i></td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td><b>Name path:</b></td><td><i>Declaration/GoodsShipment/YYYY</i></td></tr><tr><td><b>Old value:</b></td><td><i>Previous value</i></td></tr><tr><td><b>New value:</b></td><td><i>Updated value</i></td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td><b>Name path:</b></td><td><i>Declaration/GoodsShipment/ZZZZ</i></td></tr><tr><td><b>Old value:</b></td><td><i>Original data</i></td></tr><tr><td><b>New value:</b></td><td><i>Modified data</i></td></tr><tr><td colspan='2'>&nbsp;</td></tr></table></font>", new AmendmentMessagePrettier(message, GetChanges()).GetFormatted());
	}

	List<Change> GetChanges()
	{
		var changes = new List<Change>();
		var change1 = new Change();
		change1.NamePath = (NoResString)"Declaration/GoodsShipment/XXXX";
		change1.OldValue = (NoResString)"Old value";
		change1.NewValue = (NoResString)"New value";
		changes.Add(change1);

		var change2 = new Change();
		change2.NamePath = (NoResString)"Declaration/GoodsShipment/YYYY";
		change2.OldValue = (NoResString)"Previous value";
		change2.NewValue = (NoResString)"Updated value";
		changes.Add(change2);

		var change3 = new Change();
		change3.NamePath = (NoResString)"Declaration/GoodsShipment/ZZZZ";
		change3.OldValue = (NoResString)"Original data";
		change3.NewValue = (NoResString)"Modified data";
		changes.Add(change3);

		return changes;
	}
}
