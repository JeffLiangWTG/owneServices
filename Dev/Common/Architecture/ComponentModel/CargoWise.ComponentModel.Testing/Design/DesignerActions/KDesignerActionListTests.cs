#if DEBUG
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class KDesignerActionListTests : TestCase
	{
		public void TestGetSortedActionItems()
		{
			DesignerActionItemCollection collection = ActionList.GetSortedActionItems();
			AssertEquals("Property", collection[0].DisplayName);
			AssertEquals("InterfaceProperty", collection[1].DisplayName);
		}

		public void TestGetProperties()
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(ActionList);
			PropertyDescriptor property = properties["Property"];
			PropertyDescriptor interfaceProperty = properties["InterfaceProperty"];

			AssertEquals("Should return the properties on the TestComponent (not on ZDesignerActionList)", typeof(TestComponent).FullName, property.ComponentType.FullName);
			AssertEquals("Should return the properties on the TestComponent (not the interface)", typeof(TestComponent).FullName, interfaceProperty.ComponentType.FullName);
		}

		public void TestVerbs()
		{
			KDesignerActionList actionList = new KDesignerActionList(new TestComponentWithVerbs());
			DesignerActionItemCollection actionItems = actionList.GetSortedActionItems();
			AssertEquals(2, actionItems.Count);

			KDesignerActionMethodItem verb1 = (KDesignerActionMethodItem)actionItems[0];
			KDesignerActionMethodItem verb2 = (KDesignerActionMethodItem)actionItems[1];
			if (verb1.MemberName != "RunVerb1")
			{
				KDesignerActionMethodItem tmp = verb1;
				verb1 = verb2;
				verb2 = tmp;
			}

			AssertEquals("MemberName", "RunVerb1", verb1.MemberName);
			AssertEquals("DisplayName", "Verb1", verb1.DisplayName);
			AssertEquals("IncludeAsDesignerVerb", true, verb1.IncludeAsDesignerVerb);

			AssertEquals("MemberName", "RunVerb2", verb2.MemberName);
			AssertEquals("DisplayName", "Verb2", verb2.DisplayName);
			AssertEquals("IncludeAsDesignerVerb", true, verb2.IncludeAsDesignerVerb);
		}

		public void TestExtenderProviderVerbs()
		{
			TestExtenderProviderWithVerb extenderProvider = new TestExtenderProviderWithVerb();
			TestComponentWithExtendedProperties component = new TestComponentWithExtendedProperties(extenderProvider);
			component.Site.Container.Add(component);
			component.Site.Container.Add(extenderProvider);

			KDesignerActionList actionList = new KDesignerActionList(component);
			DesignerActionItemCollection actionItems = actionList.GetSortedActionItems();
			AssertEquals(1, actionItems.Count);

			KDesignerActionMethodItem extenderProviderVerb = (KDesignerActionMethodItem)actionItems[0];
			AssertEquals("MemberName", "RunExtenderProviderVerb", extenderProviderVerb.MemberName);
			AssertEquals("DisplayName", "ExtenderProviderVerb", extenderProviderVerb.DisplayName);
			AssertEquals("IncludeAsDesignerVerb", true, extenderProviderVerb.IncludeAsDesignerVerb);
		}

		#region Implementation

		readonly TestComponent Component = new TestComponent();

		KDesignerActionList ActionList
		{
			get
			{
				if (actionList == null)
				{
					actionList = new KDesignerActionList(Component);
				}
				return actionList;
			}
		}
		KDesignerActionList actionList;

		#endregion

		#region Test Classes

		[ProvideProperty("ExtendedProperty1", typeof(object))]
		[ProvideProperty("ExtendedProperty2", typeof(object))]
		class TestExtenderProviderWithVerb : Component, IExtenderProvider, IDesignerActionItemSource
		{
			public bool CanExtend(object extendee)
			{ return true; }

			public string GetExtendedProperty1(object extendee)
			{ return ""; }

			public string GetExtendedProperty2(object extendee)
			{ return ""; }

			DesignerActionItem[] IDesignerActionItemSource.GetSortedActionItems(DesignerActionList actionList)
			{
				return new DesignerActionItem[]
									{
												new KDesignerActionMethodItem(actionList, "RunExtenderProviderVerb", "ExtenderProviderVerb", true),
									};
			}
		}

		class TestExtenderListService : IExtenderListService
		{
			public TestExtenderListService(IExtenderProvider provider)
			{ this.provider = provider; }

			public IExtenderProvider[] GetExtenderProviders()
			{ return new IExtenderProvider[] { provider }; }

			readonly IExtenderProvider provider;
		}

		class TestComponentWithVerbs : Component, IDesignerActionItemSource
		{
			DesignerActionItem[] IDesignerActionItemSource.GetSortedActionItems(DesignerActionList actionList)
			{
				return new DesignerActionItem[]
									{
												new KDesignerActionMethodItem(actionList, "RunVerb1", "Verb1", true),
												new KDesignerActionMethodItem(actionList, "RunVerb2", "Verb2", true),
									};
			}
		}

		class TestComponentWithExtendedProperties : Component
		{
			public TestComponentWithExtendedProperties(IExtenderProvider provider)
			{ this.provider = provider; }

			public override ISite Site
			{
				get { return site ?? (site = new TestSite(provider)); }
				set { }
			}
			ISite site;

			readonly IExtenderProvider provider;
		}

		class TestSite : ISite
		{
			public TestSite(IExtenderProvider provider)
			{ this.provider = provider; }

			#region ISite Members

			IComponent ISite.Component
			{ get { return (IComponent)provider; } }

			public IContainer Container
			{ get { return container ?? (container = new Container()); } }
			Container container;

			bool ISite.DesignMode
			{ get { return false; } }

			string ISite.Name
			{
				get { return ""; }
				set { }
			}

			#endregion

			#region IServiceProvider Members

			object IServiceProvider.GetService(Type serviceType)
			{
				object result = null;
				if (serviceType == typeof(IExtenderListService))
				{
					result = new TestExtenderListService(provider);
				}
				return result;
			}

			#endregion

			readonly IExtenderProvider provider;
		}

		class TestComponent : IComponent, ITestInterface
		{
			[SmartTagVisible(1)]
			public string Property
			{ get { return ""; } }

			public string InterfaceProperty
			{ get { return ""; } }

			protected void RunVerb1() { }
			protected void RunVerb2() { }

			#region IComponent Unsupported Members

			event EventHandler IComponent.Disposed
			{
				add { throw new Exception("The method or operation is not implemented."); }
				remove { throw new Exception("The method or operation is not implemented."); }
			}

			ISite IComponent.Site
			{
				get { return null; }
				set { throw new Exception("The method or operation is not implemented."); }
			}

			void IDisposable.Dispose()
			{
				throw new Exception("The method or operation is not implemented.");
			}

			#endregion
		}

		interface ITestInterface
		{
			[SmartTagVisible(2)]
			string InterfaceProperty { get; }
		}

		#endregion
	}
}
#endif
