using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RequiredDocumentsWrapper))]
	sealed class RequiredDocumentsWrapperTest : Base.Testing.GenericWrapperTest
	{
		public void TestIsCorrectUsage()
		{
			AssertUsage(JobRequiredDocument.DocUsage.Export, DocumentDirection.ANY, true);
			AssertUsage(JobRequiredDocument.DocUsage.Export, DocumentDirection.DEP, true);
			AssertUsage(JobRequiredDocument.DocUsage.Export, DocumentDirection.ARV, false);

			AssertUsage(JobRequiredDocument.DocUsage.Import, DocumentDirection.ANY, true);
			AssertUsage(JobRequiredDocument.DocUsage.Import, DocumentDirection.DEP, false);
			AssertUsage(JobRequiredDocument.DocUsage.Import, DocumentDirection.ARV, true);

			AssertUsage(JobRequiredDocument.DocUsage.Both, DocumentDirection.ANY, true);
			AssertUsage(JobRequiredDocument.DocUsage.Both, DocumentDirection.DEP, true);
			AssertUsage(JobRequiredDocument.DocUsage.Both, DocumentDirection.ARV, true);

			AssertUsage(JobRequiredDocument.DocUsage.Domestic, DocumentDirection.ANY, true);
			AssertUsage(JobRequiredDocument.DocUsage.Domestic, DocumentDirection.DEP, true);
			AssertUsage(JobRequiredDocument.DocUsage.Domestic, DocumentDirection.ARV, true);
		}

		void AssertUsage(ZString usage, DocumentDirection direction, bool isCorrectUsage)
		{
			DocRequired.EQ_DocUsage = usage;
			DocRequiredWrapper.SetDocumentDirectionForTesting(direction.ToString());
			AssertEquals(isCorrectUsage, DocRequiredWrapper.IsCorrectUsage);
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

		public override void TestWrapperMappingsEmpty()
		{
			RequiredDocumentsWrapper wrapperEmpty = new RequiredDocumentsWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Description", ZString.Empty, wrapperEmpty.Description);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
RequiredDocuments                         (Default Field: Description)
======================================================================
Name                                    Type
----------------------------------------------------------------------
DateReceived                            DateTimeOffset
Description                             String
IsOriginalRequired                      Bool
IsReceived                              Bool
Type                                    String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			JobRequiredDocument requiredDocument = Factory.New<JobRequiredDocument>();
			return new RequiredDocumentsWrapper(requiredDocument, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new RequiredDocumentsWrapper(null, Factory);
		}

		#region Implementation

		JobRequiredDocument DocRequired;
		RequiredDocumentsWrapper DocRequiredWrapper;

		protected override void SetUp()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocRequired = shipment.DocsAndCartage.RequiredDocuments.AddNew();
			DocRequiredWrapper = new RequiredDocumentsWrapper(DocRequired, Factory);

			base.SetUp();
		}

		#endregion
	}
}
