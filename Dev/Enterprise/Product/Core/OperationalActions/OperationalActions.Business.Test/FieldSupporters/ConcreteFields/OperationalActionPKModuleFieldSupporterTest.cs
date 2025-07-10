using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionPKModuleFieldSupporterTest : TestCaseWithFactory
	{
		public void TestDefaultStrategies()
		{
			OperationalActionPKModuleFieldSupporter supporter = new OperationalActionPKModuleFieldSupporter("fieldName", false, (f) => new RefUNLOCOCollection(f));
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("Supported defaulting strategies", new Type[] { typeof(FixedPKModuleFieldDefaultingStrategy) }, Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestAsFilterStringCore()
		{
			RefUNLOCO aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			RefUNLOCO ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			OperationalActionPKModuleFieldSupporter supporter = new OperationalActionPKModuleFieldSupporter("FieldName", false, (f) => new RefUNLOCOCollection(Factory));
			AssertEquals("AUBNE", supporter.AsFilterString(aubne.PK, Factory));
			AssertEquals("AUSYD", supporter.AsFilterString(ausyd.PK, Factory));
			AssertEquals(null, supporter.AsFilterString(ZGuid.NewZGuid(), Factory));
		}

		public void TestAsFilterStringCore_BusinessObjectCollection()
		{
			var organisation = Factory.LoadTop1<OrgHeader>(new CargoWise.EntityFramework.ZQuery(OrgHeaderSchema.OH_Code, "4BELEV"));
			OperationalActionPKModuleFieldSupporter supporter = new OperationalActionPKModuleFieldSupporter("FieldName", false, (f) => new OrganisationsFindBoxCollection(Factory));
			AssertEquals("4BELEV", supporter.AsFilterString(organisation.PK, Factory));
			AssertEquals(null, supporter.AsFilterString(ZGuid.NewZGuid(), Factory));
		}
	}
}
