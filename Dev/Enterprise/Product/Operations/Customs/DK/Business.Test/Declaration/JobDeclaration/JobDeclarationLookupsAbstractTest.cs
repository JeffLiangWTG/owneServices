using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DK.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationLookups))]
	abstract class JobDeclarationLookupsAbstractTest<T> : BusinessObjectLookupsTestCase
		where T : JobDeclarationLookups
	{
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			lookups = GetLookups();
		}
		protected JobDeclaration jobDeclaration;
		protected T lookups;

		protected abstract string MessageType { get; }
		protected abstract T GetLookups();
	}
}
