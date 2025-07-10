using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	sealed class DocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<DocumentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new DocumentProvider(null));
		}

		public void TestDescription()
		{
			var description = dataProvider.Description;
			CombineAssertions(() =>
			{
				AssertEquals("Text", "DESCRPTION OF THE DOCUMENT", description.Text);
				AssertEquals("Language", "en", description.Language);
			});
		}

		public void TestReference()
		{
			var reference = dataProvider.Reference;
			CombineAssertions(() =>
			{
				AssertEquals("Text", "11DE11111111111115", reference.Text);
				AssertEquals("Language", "en", reference.Language);
			});
		}

		public void TestDocumentType()
		{
			document.CSI_SubType = "SUBTY";
			AssertEquals("SUBTY", dataProvider.DocumentType);
		}

		public void TestDocumentReference()
		{
			document.CSI_ReferenceNumber = "REFERENCE";
			AssertEquals("REFERENCE", dataProvider.DocumentReference);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			document = (EMCSDocument)declaration.Documents.AddNew();
			document.CSI_ReferenceNumber = "11DE11111111111115";
			document.CSI_Description = "DESCRPTION OF THE DOCUMENT";
			dataProvider = new DocumentProvider(document);
		}
		EMCSDocument document;
		IEMCSDocument dataProvider;

		protected override DocumentProvider GetProvider() => (DocumentProvider)dataProvider;

		protected override IEnumerable<Expression<Func<DocumentProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Description;
			yield return x => x.Reference;
		}
	}
}
