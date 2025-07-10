using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(DeclarationValueChangedAnnouncer))]
	sealed class DeclarationValueChangedAnnouncerTest : TestCaseWithFactory
	{
		public void TestAnnounceValueChangedEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var changedEventCalled = 0;

			using (var announcer = new DeclarationValueChangedAnnouncer(declaration))
			{
				announcer.OnValueChanged += new EventHandler(delegate
				{
					changedEventCalled++;
				});

				entryInstruction.CEI_DeclarationCargoType = DeclarationCargoTypeList.Codes.U;
				AssertEquals("should have been incremented", 1, changedEventCalled);

				declaration.JE_RL_NKPortOfLoading = "ZZZ";
				AssertEquals("should have been incremented", 2, changedEventCalled);
			}

			entryInstruction.CEI_DeclarationCargoType = DeclarationCargoTypeList.Codes.E;
			AssertEquals("Announcer is disposed and counter should stay same", 2, changedEventCalled);

			declaration.JE_RL_NKPortOfLoading = "YYY";
			AssertEquals("Announcer is disposed and counter should stay same", 2, changedEventCalled);
		}
	}
}
