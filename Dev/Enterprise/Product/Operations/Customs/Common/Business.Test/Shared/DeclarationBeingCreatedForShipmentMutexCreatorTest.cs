using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class DeclarationBeingCreatedForShipmentMutexCreatorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMutexDetails()
		{
			var guid = ZGuid.NewZGuid();
			var mutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(guid);
			NUnit.Framework.Assert.That(mutex.MutexID, Is.EqualTo(MutexIDs.JobBeingCreatedForShipment), "MutexID");
			NUnit.Framework.Assert.That(mutex.RecordIdentifier, Is.EqualTo(guid.ToString() + GlbCompany.CurrentCompany.GC_RN_NKCountryCode).Using(CustomComparers.TypeComparison), "RecordIdentifier");

			mutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(guid, Core.Constants.CountryCodes.Antarctica);
			NUnit.Framework.Assert.That(mutex.MutexID, Is.EqualTo(MutexIDs.JobBeingCreatedForShipment), "MutexID");
			NUnit.Framework.Assert.That(mutex.RecordIdentifier, Is.EqualTo(guid.ToString() + Core.Constants.CountryCodes.Antarctica).Using(CustomComparers.TypeComparison), "RecordIdentifier");
		}
	}
}
