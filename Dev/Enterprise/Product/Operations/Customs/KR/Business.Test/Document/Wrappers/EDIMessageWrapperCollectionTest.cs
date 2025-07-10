using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(EDIMessageWrapperCollection))]
	sealed class EDIMessageWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EDIMessageWrapperCollection>
	{
		public void TestIBODocDataProviderCollectionMembers()
		{
			var message1 = Entry.Messages.AddNew();
			message1.EM_MessageType = ElectronicDocumentTypeList.Codes._5BD;
			message1.EM_SystemCreateTimeUtc = ZDate.Today;

			var collection = GetCollectionToTest();
			var element = collection[0];
			var collAsI = (IBODocDataProviderCollection)collection;
			AssertEquals(1, collAsI.Count);
			AssertEquals(element, collAsI[0]);
			AssertEquals(element, collAsI["1"]);
		}
		protected override EDIMessageWrapperCollection GetCollectionToTest()
		{
			return new EDIMessageWrapperCollection(Entry.Messages.Cast<EDIMessage>(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var message = Factory.New<EDIMessage>();
			Entry.Messages.Add(message);
			return new EDIMessageWrapper(message);
		}

		CusEntryHeader Entry
		{
			get
			{
				if (entry == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					entry = declaration.CustomsEntryHeaders.AddNew();
				}
				return entry;
			}
		}
		CusEntryHeader entry;
	}
}
