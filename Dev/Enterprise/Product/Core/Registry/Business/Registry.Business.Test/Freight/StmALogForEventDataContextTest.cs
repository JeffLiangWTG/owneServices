using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	sealed class StmALogForEventDataContextTest : TestCaseWithFactory
	{
		public void TestEventDataContextShouldContainAllSystemFreightServiceTypes()
		{
			var freighServiceTypes = (CodeDescriptionPairList)ObjectFactory.Get<IFreightServiceTypes>();
			var currentCompany = StaticCurrentFetcher.Instance.CurrentCompany;
			var companyPK = currentCompany != null && !currentCompany.PK.IsEmpty ? currentCompany.PK.ToGuid() : Guid.Empty;
			var registryItemValue = FreightDataRegistry.Instance.JobServices.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty).GetCodeDescriptionPairList();

			var eventDataContextType = typeof(CargoWise.EventReference.EventReferenceExtensions).Assembly.GetType("CargoWise.EventReference.EventDataContext");
			var dictionaryType = eventDataContextType.GetField("serviceTypesDictionary", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			var dictionaryValue = (IReadOnlyDictionary<string, string>)dictionaryType.GetValue(null);

			foreach (var serviceType in freighServiceTypes.ToArray())
			{
				var code = serviceType.Code;
				var description = serviceType.Description;

				if (!registryItemValue.ContainsCode(code))
				{
					var message = string.Format(
						@"If you add a new code and description in FreightServiceTypes, please also add it into the serviceTypesDictionary in EventDataContext class.
The omissive code is {0} and description is {1}",
						code,
						description);
					Assert(message, dictionaryValue.ContainsKey(code));
					AssertEquals(message, description, dictionaryValue[code]);
				}
			}

			AssertEquals(freighServiceTypes.Count, registryItemValue.Count);
		}
	}
}
