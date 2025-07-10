using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(TestDocumentCustomLabel))]
	class DocumentCustomLabelTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <DocumentCustomLabel(x,x)>", ValueProviderToTest.IsResponsibleForReplacing("<DocumentCustomLabel(x,x)>", Passes.FirstPass));
		}

		public void TestGetConfigOrganisation()
		{
			var valueProvider = (TestDocumentCustomLabel)GetNewValueProvider();
			var bO = new TestBusinessObject();
			var wrapper = new TestDocumentWrapper(bO, bO.Factory);
			using (var report = new Report(new DocumentPack(), null, wrapper, "test document", null, DocumentDirection.ANY, false))
			{
				bO.ShouldReturnConfigOrg = true;
				AssertEquals("Found the correct organisation", "test", valueProvider.GetConfigOrganisation(report).OH_Code);

				bO.ShouldReturnConfigOrg = false;
				AssertNull("Should find no orgnanisation", valueProvider.GetConfigOrganisation(report));
			}
		}

		protected override System.Type ValueProviderType
		{
			get { return typeof(DocumentCustomLabel); }
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new TestDocumentCustomLabel();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var bO = new TestBusinessObject();
			bO.ShouldReturnConfigOrg = true;
			var customLabel = bO.ConfigOrg.CustomLabels.AddNew();
			customLabel.OT_FieldName = "CustomText1";
			customLabel.OT_Caption = "PalletValue";
			customLabel.OT_Type = OrgConstants.CustomLabelType.Document;

			var wrapper = new TestDocumentWrapper(bO, bO.Factory);
			Report = new Report(new DocumentPack(), null, wrapper, "test document", null, DocumentDirection.ANY, false);
		}

		#region Implementation

		protected class TestDocumentCustomLabel : DocumentCustomLabel
		{
			public new OrgHeader GetConfigOrganisation(Report report)
			{
				return base.GetConfigOrganisation(report);
			}
		}

		internal class TestBusinessObject : NonPersistentBusinessObject, ICustomLabelsConfigOrgProvider
		{
			public TestBusinessObject()
				: base(new BusinessObjectFactory())
			{
			}

			public bool ShouldReturnConfigOrg;

			OrgHeader fConfigOrg;
			public OrgHeader ConfigOrg
			{
				get
				{
					if (fConfigOrg == null)
					{
						fConfigOrg = Factory.New<OrgHeader>();
						fConfigOrg.OH_Code = "test";
					}
					return ShouldReturnConfigOrg ? fConfigOrg : null;
				}
			}

			public event EventHandler ConfigOrgChanged
			{
				add { }
				remove { }
			}
		}

		internal class TestDocumentWrapper : DocumentWrapper
		{
			public TestDocumentWrapper(TestBusinessObject bO, BusinessObjectFactory factoryToWrap)
				: base(bO, factoryToWrap)
			{
			}

			public override string ToString()
			{
				return "splayt";
			}
		}

		#endregion
	}
}
