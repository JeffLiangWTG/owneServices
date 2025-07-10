using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	class DEEDIMessageTest : TestCaseWithFactory
	{
		public void TestNoteTypes()
		{
			var deEDIMessage = Factory.New<DEEDIMessageForTest>();
			CombineAssertions(() =>
			{
				AssertEquals("LogbookLocalReferenceNumberNoteDescription", true, deEDIMessage.NoteTypes.IsPredefinedNoteTypeByDescription(LogbookHelper.LogbookLocalReferenceNumberNoteDescription));
				AssertEquals("LogbookRegistrationNumberNoteDescription", true, deEDIMessage.NoteTypes.IsPredefinedNoteTypeByDescription(LogbookHelper.LogbookRegistrationNumberNoteDescription));
				AssertEquals("GUAMainAccessCode", true, deEDIMessage.NoteTypes.IsPredefinedNoteTypeByDescription(LogbookHelper.LogbookGUAMainAccessCode));
			});
		}

		public void TestTypeDecider()
		{
			AssertType<EDIMessageTypeDecider>(DEEDIMessage.TypeDecider);
		}

		public void TestLinkedObjectTypes()
		{
			var deEDIMessage = Factory.New<DEEDIMessageForTest>();
			AssertCollectionContains(typeof(CusGuaranteeHeader), deEDIMessage.AdditionalRegisteredLinkedObjectTypes);
		}

		public void TestClearMessageNumberOnFailureToSave()
		{
			var message = Factory.New<DEEDIMessageForTest>();
			AssertEquals(true, message.ClearMessageNumberOnFailureToSave);
		}
	}

	class DEEDIMessageForTest : DEEDIMessage
	{
		public DEEDIMessageForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public IEnumerable<Type> AdditionalRegisteredLinkedObjectTypes => GetAdditionalRegisteredLinkedObjectTypes();
	}
}
