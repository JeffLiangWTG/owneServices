using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocJobRequiredDocument))]
	sealed class DocJobRequiredDocumentTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocJobRequiredDocument.New(DocRequired, Factory) };
		}

		public void TestType()
		{
			DocRequired.EQ_DocType = Core.Constants.RefDocTypes.PackingList;
			AssertEquals(Core.Constants.RefDocTypes.PackingList, DocRequiredWrapper.Type);
		}

		public void TestDescription()
		{
			DocRequired.EQ_DocType = Core.Constants.RefDocTypes.PackingList;
			AssertEquals("Precondition - description is autopopulated", Core.Constants.RefDocTypeDescriptions.PackingList, DocRequired.EQ_DocDescription);
			AssertEquals(Core.Constants.RefDocTypeDescriptions.PackingList, DocRequiredWrapper.Description);
		}

		public void TestDescriptionMultilingual()
		{
			DocRequired.EQ_DocType = Core.Constants.RefDocTypes.PackingList;
			AssertEquals("Precondition - description is autopopulated", Core.Constants.RefDocTypeDescriptions.PackingList, DocRequired.EQ_DocDescription);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				string key = CustomizableDataResourceStrings.GetCustomizableDataKey(RefDocType.Schema.RT_Desc, Core.Constants.RefDocTypeDescriptions.PackingList);
				mockRes.Put(key, new ResourceStringData(key, "包装清单"));

				AssertEquals("包装清单", DocRequiredWrapper.Description);
			}
		}

		public void TestDateReceived()
		{
			DocRequired.EQ_DateReceived = new ZDateTimeOffset(2005, 7, 18);
			AssertEquals(new ZDateTimeOffset(2005, 7, 18), DocRequiredWrapper.DateReceived);
		}

		public void TestIsReceived()
		{
			AssertEquals("Precondition - received date empty by default", ZDateTimeOffset.Empty, DocRequired.EQ_DateReceived);
			AssertEquals(false, DocRequiredWrapper.IsReceived);
			DocRequired.EQ_DateReceived = new ZDateTimeOffset(2005, 4, 2);
			AssertEquals(true, DocRequiredWrapper.IsReceived);
		}

		#region Implementation

		JobRequiredDocument DocRequired;
		DocJobRequiredDocument DocRequiredWrapper;

		protected override void SetUp()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocRequired = shipment.DocsAndCartage.RequiredDocuments.AddNew();
			DocRequiredWrapper = DocJobRequiredDocument.New(DocRequired, Factory);

			base.SetUp();
		}

		#endregion
	}
}
