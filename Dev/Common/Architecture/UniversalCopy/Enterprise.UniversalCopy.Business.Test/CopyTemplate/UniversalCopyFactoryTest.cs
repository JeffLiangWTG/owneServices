using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.UniversalCopy.Business.Testing
{
	public class UniversalCopyFactoryTest : TestCaseWithFactory
	{
		public void TestShouldRemoveIgnoreElementWhenExtendTemplate()
		{
			var ucFactory = new UniversalCopyFactory(typeof(DummyBusinessObjectForIgnoreElement), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			var nodes = ((EntityCopyTemplateNode)template.CopyTemplateTree.CopyTemplateNode.InnerNode).Nodes;

			AssertCollectionNotContains("Should not contains Property2", "Property2", nodes.Select(x => x.Name));

			nodes.Add(new PropertyCopyTemplateNode { Name = "Property2" });
			AssertCollectionContains("Should contains Property2", "Property2", nodes.Select(x => x.Name));

			ucFactory.ExtendTemplate(template, null);
			AssertCollectionNotContains("Should not contains Property2", "Property2", nodes.Select(x => x.Name));
		}

		public void TestShouldRemoveIgnoreElementOnChildrenWhenExtendingTemplate()
		{
			var ucFactory = new UniversalCopyFactory(typeof(DummyBusinessObjectWithNestedIgnoreElement), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			var dummyChildNode = ((EntityCopyTemplateNode)template.CopyTemplateTree.CopyTemplateNode.InnerNode).Nodes.First(x => x.Name == "DummyChild");

			var dummyGrandchildNodes = ((EntityCopyTemplateNode)((WrappedCopyTemplateNode)dummyChildNode).InnerNode).Nodes;
			var orgHeaderExcludedProperty = (typeof(OrgHeader).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), true)[0] as UniversalCopyIgnoreElementAttribute).ElementNames[0];
			dummyGrandchildNodes.Add(new PropertyCopyTemplateNode { Name = orgHeaderExcludedProperty });
			ucFactory.ExtendTemplate(template, null);
			AssertCollectionNotContains("Excluded properties should be removed by extending", orgHeaderExcludedProperty, dummyGrandchildNodes.Select(x => x.Name));
		}

		public void TestShouldNotHaveIgnoredBusinessObjectWhenCreatingNewTemplate()
		{
			var ucFactory = new UniversalCopyFactory(typeof(DummyBusinessObjectWithNestedIgnoreElement), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			var childNodes = ((EntityCopyTemplateNode)template.CopyTemplateTree.CopyTemplateNode.InnerNode).Nodes.Select(x => x.Name);
			AssertCollectionNotContains("Excluded business objects should be removed upon new template creation", "IgnoredChild", childNodes);
		}

		public void TestShouldRemoveIgnoredBusinessObjectWhenImportingOldTemplate()
		{
			const string xmlWithIgnoredBusinessObject =
@"<CopyTemplateTree
	xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
	xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""DummyBusinessObjectWithIgnoreBusinessObject"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
	<E N=""DummyBizo"">
		<P N=""Dummy1"" Do=""None"" />
		<P N=""Dummy2"" Do=""None"" />

		<R N=""JobHeader"" RelatedPropertyName="""" RelatedEntityTableName="""" Do=""Copy"">
			<E N=""JobHeader"">
				<P N=""JH_E1"" Do=""None"" />
				<P N=""JH_E2"" Do=""None"" />
			</E>
		</R>
	</E>
</CopyTemplateTree>";

			var ucFactory = new UniversalCopyFactory(typeof(DummyBusinessObjectWithIgnoredBusinessObject), null);
			CopyTemplateTree template;
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(xmlWithIgnoredBusinessObject)))
			{
				template = CopyTemplateTree.Deserialize(stream);
			}
			var ucTemplate = ucFactory.GetNewCopyTemplateFromImport(template, typeof(DummyBusinessObjectWithIgnoredBusinessObject));
			var childNodes = ((EntityCopyTemplateNode)ucTemplate.CopyTemplateTree.CopyTemplateNode.InnerNode).Nodes.Select(x => x.Name);
			AssertCollectionNotContains("Excluded business objects should be removed upon old template import", "JobHeader", childNodes);
		}

		public void TestCreateCopyTemplateTree()
		{
			var ucFactory = new UniversalCopyFactory(typeof(DummyBusinessObject), null);
			AssertEquals("Interface", ucFactory.CreateCopyTemplateTree(typeof(IInterface)).Name);
			AssertNull(ucFactory.CreateCopyTemplateTree(null));

			ucFactory = new UniversalCopyFactory(typeof(DummyForCopy), null);
			AssertEquals("Interface", ucFactory.CreateCopyTemplateTree(typeof(IInterface)).Name);
			AssertEquals("DummyForCopy", ucFactory.CreateCopyTemplateTree(null).Name);
		}

		public void TestPriorityAndIsMandatoryLoadedCorrectly()
		{
			var ucFactory = new UniversalCopyFactory(typeof(DummyBo), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			template.PrepareForSave();
			template.Factory.Save();

			var propertyNode1 = (template.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode).Nodes.Find(n => n.Name == "Property");
			AssertEquals(1, propertyNode1.Priority);
			Assert(propertyNode1.IsMandatory);

			var loadedTemplate = NewFactory().Load<UniversalCopyTemplate>(template.PK);
			AssertNotEquals(template, loadedTemplate);

			var extendedTemplate = ucFactory.GetCopyTemplateInLocalFactoryAndExtend(loadedTemplate, null);
			var propertyNode2 = (extendedTemplate.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode).Nodes.Find(n => n.Name == "Property");
			AssertEquals(1, propertyNode2.Priority);
			Assert(propertyNode2.IsMandatory);
		}

		public void TestSyncNodes()
		{
			var ucFactory1 = new UniversalCopyFactory(typeof(UniversalCopyDummy3), null);

			var template1 = ucFactory1.GetNewCopyTemplate(null);
			var dummy2Node = template1.CopyTemplateTree.ChildNodes.OfType<RelatedEntityCopyTemplateBizo>().First(n => n.Name == nameof(UniversalCopyDummy2));
			dummy2Node.CopyMethod = RelatedEntityCopyTemplateBizo.CopyMethodCodes.Copy;

			template1.PrepareForSave();
			template1.Factory.Save();

			var ucFactory2 = new UniversalCopyFactory(typeof(UniversalCopyDummy2), null);
			var extendedTemplate = ucFactory2.GetCopyTemplateInLocalFactoryAndExtend(template1, null);

			AssertEquals("UniversalCopyDummy2", extendedTemplate.CopyTemplateTree.Name);
		}

		#region Test classes

		interface IInterface
		{
		}

		[UniversalCopyWithExtendedEntities]
		class DummyForCopy : DummyBusinessObject
		{
			public DummyForCopy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		[UniversalCopyWithExtendedEntities]
		class DummyBo : NonPersistentBusinessObject
		{
			[UniversalCopyExtraMetadata(Priority = 1, IsMandatory = true)]
			public ZString Property { get; set; }

			public ZPropertyInfo PropertyInfo => GetZPropertyInfo(nameof(Property));
		}

		[UniversalCopyWithExtendedEntities]
		[UniversalCopyIgnoreElement("Property2")]
		class DummyBusinessObjectForIgnoreElement : NonPersistentBusinessObject
		{
			public ZString Property1 { get; set; }
			public ZPropertyInfo Property1Info => GetZPropertyInfo(nameof(Property1));

			public ZString Property2 { get; set; }
			public ZPropertyInfo Property2Info => GetZPropertyInfo(nameof(Property2));
		}

		[UniversalCopyWithExtendedEntities]
		public class DummyBusinessObjectWithNestedIgnoreElement : DummyBusinessObject
		{
			public DummyBusinessObjectWithNestedIgnoreElement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			[UniversalCopyRelatedEntity(DisableCopyMethodLink = true, CommaSeparatedSkipPropertiesNames = "*")]
			public OrgHeader DummyChild { get; set; }
			public ZPropertyInfo DummyChildInfo => GetZPropertyInfo(nameof(DummyChild));

			[UniversalCopyRelatedEntity(DisableCopyMethodLink = true, CommaSeparatedSkipPropertiesNames = "*")]
			public JobHeader IgnoredChild { get; set; }
			public ZPropertyInfo IgnoredChildInfo => GetZPropertyInfo(nameof(IgnoredChild));
		}

		[UniversalCopyWithExtendedEntities]
		public class DummyBusinessObjectWithIgnoredBusinessObject : DummyBusinessObject
		{
			public DummyBusinessObjectWithIgnoredBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZBool Dummy1 { get; set; }

			public ZString Dummy2 { get; set; }

			[UniversalCopyRelatedEntity(DisableCopyMethodLink = true, CommaSeparatedSkipPropertiesNames = "*")]
			public JobHeader IgnoredChild { get; set; }
			public ZPropertyInfo IgnoredChildInfo => GetZPropertyInfo(nameof(IgnoredChild));
		}

		#endregion
	}

	[UniversalCopyWithExtendedEntities]
	[UniversalCopyInstanceType(ShouldSyncTreeNodes = true)]
	public class UniversalCopyDummy2 : DummyBusinessObject
	{
		public UniversalCopyDummy2(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}

	[UniversalCopyWithExtendedEntities]
	class UniversalCopyDummy3 : DummyBusinessObject
	{
		public UniversalCopyDummy3(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[UniversalCopyRelatedEntity(DisableCopyMethodLink = true, CommaSeparatedSkipPropertiesNames = "*")]
		public UniversalCopyDummy2 UniversalCopyDummy2 { get; set; }
	}
}
