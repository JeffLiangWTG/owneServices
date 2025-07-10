using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Registry.Business
{
	[TestedType(typeof(CognosModeMapping.AvailableCognosGlbDepartmentCollection))]
	class AvailableCognosGlbDepartmentCollectionTest : ActiveBusinessObjectCollectionTestCase<CognosModeMapping.AvailableCognosGlbDepartmentCollection>
	{
		protected override CognosModeMapping.AvailableCognosGlbDepartmentCollection GetCollectionToTest()
		{
			var dictionary = new Dictionary<CognosModes, List<ZGuid>>();
			var list = new List<ZGuid>();
			dictionary.Add(CognosModes.AE, list);
			return new CognosModeMapping.AvailableCognosGlbDepartmentCollection(Factory, dictionary);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<GlbDepartment>();
		}
	}
}
