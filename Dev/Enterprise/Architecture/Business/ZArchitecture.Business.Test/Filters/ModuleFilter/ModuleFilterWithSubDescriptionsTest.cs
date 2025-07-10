using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	abstract class ModuleFilterWithSubDescriptionsTest : ModuleFilterTestCase<ModuleFilterWithSubDescriptions>
	{
		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			Assert("Nothing to test for the abstract ModuleFilterWithSubDescriptions class.", true);
		}

		#endregion

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestLongDescription()
		{
			var longDescription = "here is a long-ass description";
			var shortDescription = "short description";
			Filter.SetItemDescriptions(new ResourceStringData("1", longDescription), new ResourceStringData("2", shortDescription));
		}

		protected override ModuleFilterWithSubDescriptions GetNewModuleFilter()
		{
			return new DummyModuleFilterWithSubDescriptions("moo", DummyBizoSchema.Z0_Guid);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		#region class DummyModuleFilterWithSubDescriptions

		public class DummyModuleFilterWithSubDescriptions : ModuleFilterWithSubDescriptions
		{
			protected DummyModuleFilterWithSubDescriptions(FilterCategory category, ModuleFilterCollection parentCollection)
				: base(category, parentCollection)
			{
			}

			public DummyModuleFilterWithSubDescriptions(ZString description, SchemaColumn filterColumn)
				: base(description, filterColumn)
			{
			}

			#region GetNewCommonModuleFilter, CopyPropertiesToFilter

			protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			{
				return new DummyModuleFilterWithSubDescriptions(category, parentCollection);
			}

			protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
			{
			}

			#endregion

			#region IsExpensiveQuery

			public override bool IsExpensiveQuery
			{
				get { return false; }
			}

			#endregion

			#region DefaultCategory

			protected override FilterCategory DefaultCategory
			{
				get { return FilterCategories.Other; }
			}

			#endregion

			protected override bool IsEmptyCore => true;

			protected override void ClearCore()
			{
			}

			protected override ModuleFilterValidation GetNewValidation()
			{
				return new DummyModuleFilterValidation(this);
			}

			protected override ZQuery GetQueryUsingFilterColumns()
			{
				return new ZQuery();
			}

			protected override object[] QueryDelegateParameters
			{
				get { return Array.Empty<object>(); }
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
			}

			protected override void DeserializePropertiesFromXml(XmlReader reader)
			{
			}

			protected override void FillWithValidTestFilterValueCore()
			{
			}
		}

		#endregion
	}
}
