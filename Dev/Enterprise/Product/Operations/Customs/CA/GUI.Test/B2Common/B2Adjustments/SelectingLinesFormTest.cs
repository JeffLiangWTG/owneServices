using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Messaging;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using ExpectedB3HeaderForTesting = Enterprise.Customs.CA.Business.Testing.ClassificationLineWrapperCollectionTest.ExpectedB3HeaderForTesting;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(SelectingLinesForm))]
	class SelectingLinesFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var b3Header = new ExpectedB3HeaderForTesting(Factory);
			b3Header.PositiveClassificationLines = System.Array.Empty<IClassificationLine1>();
			var coll = new ClassificationLineWrapperCollection(b3Header);
			return new SelectingLinesForm(coll);
		}
	}
}
