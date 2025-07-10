using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Windows.UI.Controls.Internal;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class KUserControlTest : TestCase
	{
		/// <summary>
		/// DatabaseUpgradeException shouldn't be caught.
		/// If there's a leak report due to one, that report should be suppressed, not the exception.
		/// Database upgrades don't happen often and when successful they cause the current process to exit.
		/// Leaks are not important at that time.
		/// </summary>
		public void TestDispose_DoesNotCatchDatabaseUpgradeException()
		{
			var control = new TestUserControl();

			var dataSource = new TestDataSource();
			dataSource.Property = "Property value";
			dataSource.Property2 = "Property2 value";

			control.SetDataSourceBinding("BoundValue", "Property");
			control.SetDataBinding(dataSource, "");

			control.SetDataBindingAction = () =>
			{
				throw new DatabaseUpgradedException();
			};

			AssertExceptionThrown<DatabaseUpgradedException>(control.Dispose);

			control.SetDataBindingAction = null;
			control.Dispose();
		}

		public void TestExtenderProviderMembersAreProtectedFields()
		{
			ExtendedProviderMemberTestHelper.AssertExtenderMembersAreProtectedFields(typeof(KForm));
		}

		public void TestFinalizer()
		{
			// 1st time - no report
			MakeControl();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertEquals("", ErrorReporter.LastMessageReported);

			// 2nd time - report
			MakeControl();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertNotEquals("", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			// 3rd and subsequent - no more reports (to minimise performance problems)
			MakeControl();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertEquals("", ErrorReporter.LastMessageReported);

			MakeControl();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertEquals("", ErrorReporter.LastMessageReported);

			GCTracker.ClearListForTest();
		}

		public void TestFinalizerMessage()
		{
			// 1st time - no report
			MakeControl();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertEquals("", ErrorReporter.LastMessageReported);

			// 2nd time - report with explicit message
			MakeControl();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertContains("This means dispose has not been called.\r\nAny exception occurring during the construction of this object can lead to this kind of problems upon destruction. Therefore looking into Issues Manager for exceptions occurring in Constructors, for the same client and at a very close date and time, may help you in understanding this problem.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			GCTracker.ClearListForTest();
		}

		void MakeControl()
		{
			var control = new KUserControl();
			control.Name = "Cecil";
			control.DesignerActionExtenderProvider.Dispose();

			DisposableLeakListener.Instance.UnRegisterDisposable(control);
		}

		public void TestGCCollectorTrackerIsNullWhenFinalize()
		{
			AssertExceptionThrown<Exception>(() =>
			{
				_ = new KUserControlForTest();
			});
			AssertNoExceptionThrown(() =>
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			});
		}

		public void TestGCCollectorTracker_WhenMessageIsTooLong()
		{
			MakeControlForTest("Test");
			GC.Collect();
			GC.WaitForPendingFinalizers();

			MakeControlForTest(new string('t', 31839));

			AssertNoExceptionThrown(() =>
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			});

			GCTracker.ClearListForTest();
		}

		void MakeControlForTest(string name)
		{
			var control = new KUserControlForTest(null);
			control.Name = name;
			control.DesignerActionExtenderProvider.Dispose();

			DisposableLeakListener.Instance.UnRegisterDisposable(control);
		}

		class KUserControlForTest : KUserControl
		{
			static Control GetControlWhenExceptionThrown() => throw new Exception();

			public KUserControlForTest() : this(GetControlWhenExceptionThrown())
			{
			}

			public KUserControlForTest(Control control)
			{
				this.control = control;
			}

			protected override bool ReportDisposeCalledToEventLog
			{
				get { return true; }
			}
			[SuppressMessage("Maintainability", "IDE0052: Remove unread private member.", Justification = "The unit test need this feild to trigger GC!")]
			readonly Control control;
		}

		#region Garbage Collection

		public void TestGarbageCollectableAfterBinding()
		{
			CreateFormWithBinding(out var formRef, out var dataSourceRef);
			GC.Collect();

			Assert("Top level data source should be collected", !IsAlive(dataSourceRef));
			Assert("Control should be collected", !IsAlive(formRef));
		}

		bool IsAlive<T>(WeakReference<T> wr) where T : class
			=> wr.TryGetTarget(out var ignored);

		void CreateFormWithBinding(out WeakReference<KForm> formRef, out WeakReference<TestSimpleDataSource> dataSourceRef)
		{
			var datasource = new TestSimpleDataSource();
			var form = new KForm();
			var control = new TestUserControlWithNoDataSourceType();

			var box = new KTextBox();
			control.Controls.Add(box);
			control.BindingSource.SetBindingMember(box, "SomeProp");
			control.SetDataBinding(datasource, "");
			form.Controls.Add(control);
			form.Show();

			formRef = new WeakReference<KForm>(form);
			dataSourceRef = new WeakReference<TestSimpleDataSource>(datasource);

			form.Dispose();
		}

		#endregion

		#region SetDataSourceBinding

		public void TestSetDataSourceBinding()
		{
			using (KForm form = new KForm())
			using (TestUserControl control = new TestUserControl())
			{
				control.BindingSource.DataSourceType = typeof(object);
				form.Controls.Add(control);
				form.Show();

				TestDataSource dataSource = new TestDataSource();
				dataSource.Property = "Property value";
				dataSource.Property2 = "Property2 value";

				control.SetDataSourceBinding("BoundValue", "Property");
				control.SetDataBinding(dataSource, "");
				AssertEquals("Property value", control.BoundValue);

				control.SetDataSourceBinding("BoundValue", "Property2");
				AssertEquals("Property2 value", control.BoundValue);
			}
		}

		public void TestSetAbsoluteDataSourceBinding()
		{
			using (KForm form = new KForm())
			using (TestUserControl control = new TestUserControl())
			{
				control.BindingSource.DataSourceType = typeof(object);
				form.Controls.Add(control);
				form.Show();

				TestDataSource dataSource = new TestDataSource();
				dataSource.Property = "Property value";
				dataSource.Property2 = "Property2 value";

				control.SetAbsoluteDataSourceBinding(control, "BoundValue", "Property", false);
				control.SetDataBinding(dataSource, "ChildRelation"); // dataMember will be ignored in call to SetAbsoluteDataSourceBinding
				AssertEquals("Property value", control.BoundValue);

				control.SetAbsoluteDataSourceBinding(control, "BoundValue", "Property2", false);
				AssertEquals("Property2 value", control.BoundValue);
			}
		}

		#endregion

		#region OnCurrentDataItemChanging / OnCurrentDataItemChanged

		object lastDataSourceOnChanging;
		object lastDataSourceOnChanged;

		[RequiresSTA]
		public void TestDataSourceChangingChanged()
		{
			using (KForm form = new KForm())
			{
				TestUserControl control = new TestUserControl(this);
				form.Controls.Add(control);
				KTextBox textBox = new KTextBox();
				control.Controls.Add(textBox);
				control.BindingSource.SetBindingMember(textBox, "Text");

				TestDataSource data = new TestDataSource();
				TestRelatedDataSource entity1 = data.ChildRelation.AddNew();
				TestRelatedDataSource entity2 = data.ChildRelation.AddNew();

				control.SetDataBinding(data, "ChildRelation");
				AssertChangingAndChangedFired(true, false);

				control.SetDataBinding(null, "");
				AssertChangingAndChangedFired(false, true);

				control.SetDataBinding(data, "ChildRelation");
				AssertChangingAndChangedFired(true, false);

				control.BindingContext[data, "ChildRelation"].Position = 1;
				AssertChangingAndChangedFired(false, false);

				data.ChildRelation.RemoveAt(1);
				AssertChangingAndChangedFired(false, false);

				data.ChildRelation.RemoveAt(0);
				AssertChangingAndChangedFired(false, true);

				TestRelatedDataSource newEntity = data.ChildRelation.AddNew();
				AssertChangingAndChangedFired(true, false);

				data.ChildRelation.Remove(newEntity);
				AssertChangingAndChangedFired(false, true);
			}
		}

		public void TestDataSourceChangingChanged_WhenIsTopLevelControl()
		{
			using (KForm form = new KForm())
			{
				TestUserControl control = new TestUserControl(this);
				form.Controls.Add(control);
				form.Show();

				TestDataSource data = new TestDataSource();
				TestRelatedDataSource entity1 = data.ChildRelation.AddNew();
				TestRelatedDataSource entity2 = data.ChildRelation.AddNew();

				control.SetDataBinding(data, "ChildRelation");
				AssertChangingAndChangedFired(true, false);

				control.SetDataBinding(null, "");
				AssertChangingAndChangedFired(false, true);

				control.SetDataBinding(data, "ChildRelation");
				AssertChangingAndChangedFired(true, false);

				control.BindingContext[data, "ChildRelation"].Position = 1;
				AssertChangingAndChangedFired(false, false);

				data.ChildRelation.RemoveAt(1);
				AssertChangingAndChangedFired(false, false);

				data.ChildRelation.RemoveAt(0);
				AssertChangingAndChangedFired(false, true);

				TestRelatedDataSource newEntity = data.ChildRelation.AddNew();
				AssertChangingAndChangedFired(true, false);

				control.BindingContext = new BindingContext();
				//					AssertChangingAndChangedFired(false, false);
				data.ChildRelation.Remove(newEntity);
				AssertChangingAndChangedFired(false, true);
			}
		}

		public void TestChangingChangedNotFiredIncorrectly()
		{
			using (KForm form = new KForm())
			using (TestUserControl control = new TestUserControl(this))
			{
				KTextBox box = new KTextBox();
				control.Controls.Add(box);
				control.BindingSource.SetBindingMember(box, "Text");

				TestDataSource data = new TestDataSource();
				control.SetDataBinding(data, "ChildRelation");
				TestRelatedDataSource new_entity = data.ChildRelation.AddNew();
				AssertChangingAndChangedFired(true, false);

				new_entity.Text = "xxx";
				AssertNull("The data source should not have changed", this.lastDataSourceOnChanging);
				AssertNull("The data source should not have changed", this.lastDataSourceOnChanged);
			}
		}

		[ExpectNoExceptions]
		public void TestCurrentDataItem_DuringSetDataBinding()
		{
			using (KForm form = new KForm())
			using (KUserControl control = new KUserControl())
			{
				KTextBox textBox = new KTextBox();
				textBox.TextChanged += delegate
				{ AssertNotNull("CurrentDataItem available during SetDataBinding", control.CurrentDataItem); };

				control.BindingSource.DataSourceType = typeof(object);
				control.BindingSource.SetBindingMember(textBox, "Property");
				TestDataSource data = new TestDataSource();
				data.Property = "value";

				form.Show();
				control.Controls.Add(textBox);
				form.Controls.Add(control);
				BindingContext created = textBox.BindingContext;
				control.SetDataBinding(data, "");
			}
		}

		void AssertChangingAndChangedFired(bool expectChangingDataSourceEmpty, bool expectChangedDataSourceEmpty)
		{
			if (expectChangingDataSourceEmpty)
			{
				if (this.lastDataSourceOnChanging != null)// && !this.lastDataSourceOnChanging.IsReadOnly)
				{
					Fail("Expected lastDataSourceOnChanging data source empty");
				}
			}
			else
			{
				AssertNotNull("lastDataSourceOnChanging data source null", this.lastDataSourceOnChanging);
			}
			if (expectChangedDataSourceEmpty)
			{
				if (this.lastDataSourceOnChanged != null)// && !this.lastDataSourceOnChanged.IsReadOnly)
				{
					Fail("Expected lastDataSourceOnChanged data source empty");
				}
			}
			else
			{
				AssertNotNull("lastDataSourceOnChanged data source null", this.lastDataSourceOnChanged);
			}

			Assert(
				"both changed and changing data sources equal!",
				this.lastDataSourceOnChanged != this.lastDataSourceOnChanging);
			this.lastDataSourceOnChanged = null;
			this.lastDataSourceOnChanging = null;
		}

		#endregion

		#region User Control Test Classes

		[DefaultBindingProperty("BoundValue")]
		class TestUserControl : TestUserControlWithNoDataSourceType
		{
			readonly KUserControlTest owner;

			public TestUserControl()
				: this(null)
			{
			}

			public TestUserControl(KUserControlTest owner)
			{
				this.owner = owner;
				this.BindingSource.DataSourceType = typeof(object);
			}

			public Action SetDataBindingAction;

			public override void SetDataBinding(object dataSource, string dataMember)
			{
				SetDataBindingAction?.Invoke();
				base.SetDataBinding(dataSource, dataMember);
			}

			public StackTrace stack;

			public string BoundValue
			{
				get
				{
					return boundValue;
				}
				set
				{
					boundValue = value;
					stack = new StackTrace();
				}
			}
			string boundValue;

			#region OnCurrentChanging / OnCurrentChanged

			protected override void OnCurrentDataItemChanging(EventArgs e)
			{
				base.OnCurrentDataItemChanging(e);
				if (owner != null)
				{
					if (owner.lastDataSourceOnChanging != null)
					{
						throw new InvalidOperationException("OnCurrentDataItemChanging got called twice unnecessarily");
					}
					owner.lastDataSourceOnChanging = CurrentDataItem;
				}
			}

			protected override void OnCurrentDataItemChanged(EventArgs e)
			{
				base.OnCurrentDataItemChanged(e);
				if (owner != null)
				{
					if (owner.lastDataSourceOnChanged != null)
					{
						throw new InvalidOperationException("OnCurrentChanged got called twice unnecessarily");
					}
					owner.lastDataSourceOnChanged = CurrentDataItem;
				}
			}

			#endregion
		}

		class TestUserControlWithNoDataSourceType : KUserControl
		{
			public TestUserControlWithNoDataSourceType()
			{ this.BindingSource.DataSourceType = typeof(object); }
		}

		#endregion

		#region Entity Test Classes

		public class TestSimpleDataSource : ComponentModel.Testing.KComponentWithPropertyChange
		{
			public string SomeProp
			{
				get { return someProp; }
				set
				{
					if (someProp != value)
					{
						someProp = value;
						OnSomePropChanged(EventArgs.Empty);
					}
				}
			}
			string someProp;

			public event EventHandler SomePropChanged;

			void OnSomePropChanged(EventArgs e)
			{
				if (SomePropChanged != null)
				{
					SomePropChanged(this, e);
				}
			}
		}

		public class TestDataSource : ComponentModel.Testing.KComponent
		{
			public string Property { get; set; }
			public string Property2 { get; set; }

			public TestRelatedDataSourceCollection ChildRelation
			{
				get { return childRelation ?? (childRelation = new TestRelatedDataSourceCollection()); }
			}
			TestRelatedDataSourceCollection childRelation;
		}

		public class TestRelatedDataSource : ComponentModel.Testing.KComponent
		{
			public string Text { get; set; }
		}

		public class TestRelatedDataSourceCollection : ComponentModel.Testing.KBindingList<TestRelatedDataSource>
		{
		}

		#endregion

		#region TrackInstantiatedControls

		public void TestTrackInstantiatedControls()
		{
			AssertNull(KUserControl.instantiatedControlCounts);
			AssertNull(KUserControl.InstantiatedControls_ForTest);

			using (new KUserControl())
			{
				AssertNull(KUserControl.instantiatedControlCounts);
				AssertNull(KUserControl.InstantiatedControls_ForTest);
			}

			using (KUserControl.TrackInstantiatedControls_ForTest())
			{
				AssertNotNull(KUserControl.instantiatedControlCounts);
				AssertNotNull(KUserControl.InstantiatedControls_ForTest);

				using (new KUserControl())
				{ }
				Assert(KUserControl.instantiatedControlCounts.TryGetValue(typeof(KUserControl), out var kUserControlCount));
				AssertEquals(1, kUserControlCount);

				using (new KUserControl())
				{ }
				Assert(KUserControl.instantiatedControlCounts.TryGetValue(typeof(KUserControl), out kUserControlCount));
				AssertEquals(2, kUserControlCount);

				using (new KTextBox())
				{ }
				var dictionary = KUserControl.instantiatedControlCounts;
				var dictionaryCount = dictionary.Count;
				AssertEquals("KTextBox is not a KUserControl, so doesn't get counted", 1, dictionaryCount);
				Assert(KUserControl.instantiatedControlCounts.TryGetValue(typeof(KUserControl), out kUserControlCount));
				AssertEquals(2, kUserControlCount);

				using (new DummyKUserControl())
				{ }
				Assert(KUserControl.instantiatedControlCounts.TryGetValue(typeof(KUserControl), out kUserControlCount));
				AssertEquals(2, kUserControlCount);
				Assert(KUserControl.instantiatedControlCounts.TryGetValue(typeof(DummyKUserControl), out var dummyKUserControlCount));
				AssertEquals(1, dummyKUserControlCount);
			}

			AssertNull(KUserControl.instantiatedControlCounts);
			AssertNull(KUserControl.InstantiatedControls_ForTest);
		}

		class DummyKUserControl : KUserControl
		{
		}

		#endregion
	}
}
