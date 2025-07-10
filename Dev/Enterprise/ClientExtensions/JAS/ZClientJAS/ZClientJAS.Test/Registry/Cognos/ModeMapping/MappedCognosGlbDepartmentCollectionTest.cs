using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Registry.Business
{
	[TestedType(typeof(CognosModeMapping.MappedCognosGlbDepartmentCollection))]
	class MappedCognosGlbDepartmentCollectionTest : ActiveBusinessObjectCollectionTestCase<CognosModeMapping.MappedCognosGlbDepartmentCollection>
	{
		protected override CognosModeMapping.MappedCognosGlbDepartmentCollection GetCollectionToTest()
		{
			return new CognosModeMapping.MappedCognosGlbDepartmentCollection(Factory, Dictionary)
			{ SelectedModeAsEnum = CognosModes.AE };
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var guid = Guid.NewGuid();
			Dictionary[CognosModes.AE].Add(guid);
			return Factory.NewWithPrimaryKey<GlbDepartment>(guid);
		}

		Dictionary<CognosModes, List<ZGuid>> fDictionary;
		Dictionary<CognosModes, List<ZGuid>> Dictionary
		{
			get
			{
				if (fDictionary == null)
				{
					fDictionary = new Dictionary<CognosModes, List<ZGuid>>();
					var list = new List<ZGuid>();
					foreach (var dept in LoadDepartmentCollection())
					{
						list.Add(dept.PK);
					}

					fDictionary.Add(CognosModes.AE, list);
				}

				return fDictionary;
			}
		}

		GlbDepartmentCollection LoadDepartmentCollection()
		{
			var collection = new GlbDepartmentCollection(Factory);
			var query = new ZQuery();
			query.MaximumRows = 10;
			collection.AdditionalFilter = query;
			AssertEquals("Pre-condition. There should be at least 10 Departments in test database, add manually if this fails", 10, collection.Count);
			return collection;
		}
	}
}
