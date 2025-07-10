using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.General.Testing
{
	[TestedType(typeof(DocDeclaration))]
	sealed class DocJobDeclarationTest : DocBaseJobDeclarationAbstractTest<BaseJobDeclaration, DocDeclaration>
	{
		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Fiji; }
		}
	}
}
