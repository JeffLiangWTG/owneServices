using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class BindingContextExtensionsTest : TestCase
	{
		public void TestBindingContextIndexer_ForCurrencyManager()
		{
			MockMasterObjectCollection collection =
									new MockMasterObjectCollection();
			BindingManagerBase currencyManager = Form.BindingContext[collection];
			Assert(currencyManager is CurrencyManager);
			PropertyDescriptor property = currencyManager.GetItemProperties()["StringProperty"];
			Assert(property is KPropertyDescriptor);
		}

		public void TestGetBindingManager_WithWrappedProperties()
		{
			BindingContext bc = new BindingContext();
			MockMasterObject entity = new MockMasterObject();
			entity.DetailObjects.AddNew();

			BindingManagerBase bm = BindingContextExtensions.EnsureListManager(bc, entity, "RelatedObject+StringProperty");
			AssertNotNull("Should find the BindingManagerBase", bm);
			BindingManagerBase metaBM = BindingContextExtensions.GetBindingManager(bc, entity, "RelatedObject+StringProperty", MetaDataTypes.ReadOnly);
			AssertNotNull("Should find the meta-data BindingManagerBase", metaBM);
		}

		class Dummy
		{
			public Dummy DummyProp { get { return this; } }
		}
		public void TestConcurrentAccessToBindingContext()
		{
			var caughtExceptions = new BlockingCollection<Exception>();
			var bindingContext = new BindingContext();
			var dataSource = new Dummy();
			var dataMember = string.Join(".", Enumerable.Repeat("DummyProp", 20)); //The deeper it has to traverse the properties, the more likely it is to encounter a duplicate

			var threadMethod = new ThreadStart(() =>
			{
				try
				{
					BindingContextExtensions.GetBindingContextEnsureListManagerSafe(bindingContext, dataSource, dataMember);
				}
				catch (Exception ex)
				{
					caughtExceptions.Add(ex);
				}
			});

			var someThreads = Enumerable.Range(0, 12).Select(_ => new Thread(threadMethod)).ToList();

			someThreads.ForEach(t => t.Start());
			someThreads.ForEach(t => t.Join());

			AssertEquals("Did not expect exceptions", 0, caughtExceptions.Count);
		}

		#region Implementation

		Form Form
		{
			get { return form ?? (form = new Form()); }
		}
		Form form;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
