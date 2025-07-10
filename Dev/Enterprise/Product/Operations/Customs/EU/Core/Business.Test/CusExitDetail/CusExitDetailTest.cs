using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusExitDetail))]
	class CusExitDetailTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestMessages()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_LinkedObject = exitDetail;
			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkedObject = exitDetail;
			NUnit.Framework.Assert.That(exitDetail.Messages.Cast<EDIMessage>(), NUnit.Framework.Is.EquivalentTo(new[] { message1, message2 }));
		}

		[ExpectNoExceptions]
		public void TestCusExitItemsChildEditable()
		{
			NUnit.Framework.Assert.That(exitDetail.IsRegisteredEditableChildObject(exitDetail.CusExitItems), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCED_ArrivalNotificationPlaceMaxLength()
		{
			NUnit.Framework.Assert.That(exitDetail.CED_ArrivalNotificationPlaceInfo.MaxLength, NUnit.Framework.Is.EqualTo(50));
		}

		[ExpectNoExceptions]
		public void TestCED_TransportIDMaxLength()
		{
			NUnit.Framework.Assert.That(exitDetail.CED_TransportIDInfo.MaxLength, NUnit.Framework.Is.EqualTo(50));
		}

		[ExpectNoExceptions]
		public void TestCED_OA_CarrierCaption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(exitDetail.CED_OA_CarrierInfo).Caption, NUnit.Framework.Is.EqualTo("Carrier"));
		}

		[ExpectNoExceptions]
		public void TestDocManagerInfo()
		{
			var docManagerInfo = ((IDocManagerSupport)exitDetail).DocManagerInfo;
			IeDoc addedEDoc = docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Test Blob Thing"), "dodgy.pdf", "MCD");
			addedEDoc.Description = "Something";
			Factory.Save();
			docManagerInfo.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			CusExitDetail exitDetailReloaded = anotherFactory.Load<CusExitDetail>(exitDetail.PK);
			IStorageDocsBaseCollection eDocs = ((IDocManagerSupport)exitDetailReloaded).DocManagerInfo.AllEDocs;
			NUnit.Framework.Assert.That(eDocs.Count, NUnit.Framework.Is.EqualTo(1));
			IeDoc eDoc = eDocs[0];
			NUnit.Framework.Assert.That(eDoc.FileName, NUnit.Framework.Is.EqualTo("dodgy.pdf").Using(CustomComparers.TypeComparison), "eDoc.FileName");
			NUnit.Framework.Assert.That(eDoc.Description, NUnit.Framework.Is.EqualTo("Something").Using(CustomComparers.TypeComparison), "eDoc.Description");
			NUnit.Framework.Assert.That(eDoc.DocType, NUnit.Framework.Is.EqualTo("MCD").Using(CustomComparers.TypeComparison), "eDoc.DocType");
			using (var streamReader = new StreamReader(eDoc.GetImageDataReader()))
			{
				NUnit.Framework.Assert.That(streamReader.ReadToEnd(), NUnit.Framework.Is.EqualTo("Test Blob Thing"), "Encoding.UTF8.GetString(eDoc.ImageData)");
			}
		}

		protected override BusinessObject GetNewBusinessObject() => exitDetail;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var exitHeader = factory.NewWithValidTestData<CusExitControlHeader>();
			return exitHeader.CusExitDetails.AddNew();
		}

		[ExpectNoExceptions]
		public void TestAdditionalInfos()
		{
			var info1 = exitDetail.AdditionalInfos.AddNew();
			var info2 = exitDetail.AdditionalInfos.AddNew();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(exitDetail.AdditionalInfos, NUnit.Framework.Is.TypeOf<AdditionalInfoCollection>(), "Collection Type is AdditionalInfoCollection");
				NUnit.Framework.Assert.That(exitDetail.AdditionalInfos.Cast<AdditionalInfo>(), NUnit.Framework.Is.EquivalentTo(new[] { info1, info2 }), "AdditionalInfos contains elements");
				NUnit.Framework.Assert.That(exitDetail.IsRegisteredEditableChildObject(exitDetail.AdditionalInfos), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsRegisteredEditableChildObject");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitDetail = exitHeader.CusExitDetails.AddNew();
		}
		CusExitDetail exitDetail;
	}
}
