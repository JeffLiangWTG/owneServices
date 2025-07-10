using Enterprise.Customs.CA.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	[TestedType(typeof(eManifestStatusNoticeDataContextManager))]
	sealed class eManifestStatusNoticeDataContextManagerTest : DataContextManagerTestCase<eManifestStatusNoticeDataContextManager, CusEntryHeader>
	{
		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("CusEntryHeader doesn't have any JobNumber", true);
		}

		protected override void TestAttributeIsOnBusinessObjectCore()
		{
			Assert("This Data Context Manager may process several different types of Business Objects", true);
		}

		protected override void TestManagerIsLoadableViaSpringCore()
		{
			Assert("This Data Context Manager may process several different types of Business Objects", true);
		}

		public void TestGetDataContextKeyMatchingQuery()
		{
			var eventXmlText = @"<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAeManifestStatusNotice</Type>
					<Key>12345000000011</Key>
				</DataTarget>
			</DataTargetCollection>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>CD4</Code>
					<Description>CA Customs IID/D4 Status Notice</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
	</Event>
</UniversalEvent>
";
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader.CH_BGMReference = "12345000000011";
			Factory.SaveForTesting();
			var xmlEvent = new XmlEventDeserializer().Parse(eventXmlText);
			var finder = new eManifestStatusNoticeEventParentFinder(Factory.BOFactory, new eManifestStatusNoticeDataContextManager(), new XmlSessionTracker(new ServiceTaskLogForTesting()));
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as CusEntryHeader;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Number", "12345000000011", relatedObj.CH_BGMReference);
		}
	}
}
