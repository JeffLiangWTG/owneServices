using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using NUnit.Framework;

namespace ZClientEDI.Business.Test.IdentityCertificate
{
	[TestedType(typeof(EdiIdentityCertificateCollection))]
	public class EdiIdentityCertificateCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiIdentityCertificateCollection>
	{
	}
}
