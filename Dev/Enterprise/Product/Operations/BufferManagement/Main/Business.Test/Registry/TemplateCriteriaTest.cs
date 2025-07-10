using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TemplateCriteria))]
	public class TemplateCriteriaTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestDefaultRegistryItem()
		{
			var item = BMSRegistry.Instance.AutoAssignmentCapabilityTasksFailure.DefaultValue;

			AssertEquals(false, item.Enabled);
			AssertEquals(string.Empty, item.Criterion1);
			AssertEquals(string.Empty, item.Criterion2);
			AssertEquals(string.Empty, item.Criterion3);
			AssertEquals(string.Empty, item.Criterion4);
			AssertEquals(string.Empty, item.Criterion5);
		}

		public void TestValidateInvalid()
		{
			var criteria = new TemplateCriteria();
			AssertEquals(false, criteria.HasErrors);

			criteria.Criterion1 = "AAA";
			criteria.Criterion2 = "AAA";
			criteria.Criterion3 = "AAA";
			criteria.Criterion4 = "AAA";
			criteria.Criterion5 = "AAA";

			AssertEquals(true, criteria.HasErrors);
			AssertEquals(true, criteria.Criterion1Info.HasError("Enter a valid selection."));
			AssertEquals(true, criteria.Criterion2Info.HasError("Enter a valid selection."));
			AssertEquals(true, criteria.Criterion3Info.HasError("Enter a valid selection."));
			AssertEquals(true, criteria.Criterion4Info.HasError("Enter a valid selection."));
			AssertEquals(true, criteria.Criterion5Info.HasError("Enter a valid selection."));
		}

		public void TestValidateValid()
		{
			Mock<ISelectionCriteriaLookup> lookup = new Mock<ISelectionCriteriaLookup>();
			lookup.Setup(m => m.GetCriterionList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns((string s1, string s2, string s3, string s4) =>
				{
					if (s1 == null)
					{
						return new CodeDescriptionPairList { new CodeDescriptionPair("AAA", "a a a"), new CodeDescriptionPair("ZZZ", "z z z") };
					}
					if (s2 == null)
					{
						return new CodeDescriptionPairList { new CodeDescriptionPair("YYY", "y y y"), new CodeDescriptionPair("BBB", "b b b") };
					}
					if (s3 == null)
					{
						return new CodeDescriptionPairList { new CodeDescriptionPair("CCC", "c c c"), new CodeDescriptionPair("XXX", "x x x") };
					}
					if (s4 == null)
					{
						return new CodeDescriptionPairList { new CodeDescriptionPair("WWW", "w w w"), new CodeDescriptionPair("DDD", "d d d") };
					}
					return new CodeDescriptionPairList { new CodeDescriptionPair("EEE", "e e e"), new CodeDescriptionPair("VVV", "v v v") };
				});
			ObjectFactory.Substitute(lookup.Object);

			var criteria = new TemplateCriteria();
			AssertEquals(false, criteria.HasErrors);

			criteria.Criterion1 = "AAA";
			criteria.Criterion2 = "BBB";
			criteria.Criterion3 = "CCC";
			criteria.Criterion4 = "DDD";
			criteria.Criterion5 = "EEE";

			AssertEquals(false, criteria.HasErrors);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NewPopulatedBusinessObject();
		}

		TemplateCriteria NewPopulatedBusinessObject()
		{
			return new TemplateCriteria(Factory);
		}
	}
}
