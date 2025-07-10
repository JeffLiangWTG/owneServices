using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Testing
{
	[TestedType(typeof(Contact))]
	sealed class ContactTest : DataObjectTestCase<Contact>
	{
	}
}
