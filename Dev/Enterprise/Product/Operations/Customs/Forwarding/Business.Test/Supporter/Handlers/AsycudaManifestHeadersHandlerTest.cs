using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	sealed class AsycudaManifestHeadersHandlerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			var headers1 = CreateManifestHeaders(consol1);
			var headers2 = CreateManifestHeaders(consol2);
			Factory.Save();
			var newFactory = NewFactory();
			consol1 = newFactory.Load<ForwardingConsol>(consol1.PK);
			consol2 = newFactory.Load<ForwardingConsol>(consol2.PK);
			consol3 = newFactory.Load<ForwardingConsol>(consol3.PK);
			var handler = new AsycudaManifestHeadersHandler();
			var expectedPks = headers1.Select(c => c.PK).ToArray();
			var actualPks = handler.Load(consol1, true).Cast<BusinessObject>().Select(c => c.PK);
			NUnit.Framework.Assert.That(actualPks, Is.EquivalentTo(expectedPks));
			actualPks = handler.Load(consol1, false).Cast<BusinessObject>().Select(c => c.PK);
			NUnit.Framework.Assert.That(actualPks, Is.EquivalentTo(expectedPks));
			expectedPks = headers2.Select(c => c.PK).ToArray();
			actualPks = handler.Load(consol2, true).Cast<BusinessObject>().Select(c => c.PK);
			NUnit.Framework.Assert.That(actualPks, Is.EquivalentTo(expectedPks));
			actualPks = handler.Load(consol2, false).Cast<BusinessObject>().Select(c => c.PK);
			NUnit.Framework.Assert.That(actualPks, Is.EquivalentTo(expectedPks));
			NUnit.Framework.Assert.That(handler.Load(consol3, true).FirstOrDefault(), Is.EqualTo(default(ICancellable)), "Should be null as it does not have any related AsycudaManifestHeader. - should be [null]");
			NUnit.Framework.Assert.That(handler.Load(consol3, false).FirstOrDefault(), Is.EqualTo(default(ICancellable)), "Should be null as it does not have any related AsycudaManifestHeader. - should be [null]");
		}

		Integration.Customs.ManifestBase.IAsycudaManifestHeader[] CreateManifestHeaders(ForwardingConsol consol)
		{
			var normalHeader = Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			((BusinessObject)normalHeader).FillWithValidTestData();
			normalHeader.AMA_JobReference = consol.JK_UniqueConsignRef + "_NM";
			normalHeader.AMA_ParentId = consol.PK;
			normalHeader.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var sgHeader = Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			((BusinessObject)sgHeader).FillWithValidTestData();
			sgHeader.AMA_JobReference = consol.JK_UniqueConsignRef + "_SG";
			sgHeader.AMA_ParentId = consol.PK;
			sgHeader.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var aceHeader = Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			((BusinessObject)aceHeader).FillWithValidTestData();
			aceHeader.AMA_JobReference = consol.JK_UniqueConsignRef + "_ACE";
			aceHeader.AMA_ParentId = consol.PK;
			aceHeader.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var zaHeader = Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			((BusinessObject)zaHeader).FillWithValidTestData();
			zaHeader.AMA_JobReference = consol.JK_UniqueConsignRef + "_ZA";
			zaHeader.AMA_ParentId = consol.PK;
			zaHeader.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
			return new Integration.Customs.ManifestBase.IAsycudaManifestHeader[] { normalHeader, sgHeader, aceHeader, zaHeader };
		}
	}
}
