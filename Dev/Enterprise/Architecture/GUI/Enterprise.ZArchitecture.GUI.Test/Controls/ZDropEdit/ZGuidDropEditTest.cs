using System;
using System.Collections;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGuidDropEditTest : TestCaseWithFactory
	{
		public void TestOnFormatValueIfDestroyed()
		{
			using (var form = new ZForm())
			using (var dropEditTest = new ZGuidDropEditForTest())
			{
				form.Controls.Add(dropEditTest);
				form.Show();

				var listElement = Dummy.Dependents.AddNew();
				listElement.ZD1_Code = "Value";
				Dummy.Z0_Guid = listElement.PK;
				dropEditTest.BindToList = "Dependents";
				dropEditTest.SetDataBinding(Dummy, DummyBizoSchema.Z0_Guid.Name);
				var binding = dropEditTest.CodeBox.DataBindings[nameof(MediaTypeNames.Text)];
				var field = typeof(Binding).GetField("onFormat", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField) ?? typeof(Binding).GetField("_onFormat", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
				var eventDelegate = field.GetValue(binding) as Delegate;
				Assert(eventDelegate.GetInvocationList().Length == 1);

				dropEditTest.DestroyHandle_exposed();
				eventDelegate = field.GetValue(binding) as Delegate;
				AssertNull(eventDelegate);
			}
		}

		public void TestOnFormatValue()
		{
			using (var guidDrop = new ZGuidDropEditForFormat())
			{
				guidDrop.DisableInvalidation = true;
				AssertNoExceptionThrown(() => guidDrop.Format());
			}
		}

		class ZGuidDropEditForFormat : ZGuidDropEdit
		{
			public void Format()
			{
				var list = new ArrayList();
				list.Add(new DummyForFormat());
				List = list;
				OnFormatValue(new ConvertEventArgs(ZGuid.NewZGuid(), typeof(ZGuid)));
			}

			protected override object DataSourceCore => new DummyForFormat();

			protected override void InvalidateListCore() { }
		}

		class ZGuidDropEditForTest : ZGuidDropEdit
		{
			public void DestroyHandle_exposed()
			{
				base.DestroyHandle();
			}
		}

		class DummyForFormat : ICodeDescription
		{
			public object PK { get; set; }
			public string Code { get; set; }
			public string Description { get; set; }
		}

		public void TestBinding()
		{
			Form.Controls.Add(DropEdit);
			Form.Show();

			var listElement = Dummy.Dependents.AddNew();
			listElement.ZD1_Code = "Value";
			Dummy.Z0_Guid = listElement.PK;
			DropEdit.BindToList = "Dependents";
			DropEdit.SetDataBinding(Dummy, DummyBizoSchema.Z0_Guid.Name);

			UserIdleWorker.Flush();
			AssertEquals("ReadOnly set when idle binding complete", false, DropEdit.GetReadOnly());
			AssertEquals("Bound after idle binding", "VALUE", DropEdit.Text);
		}

		public void TestBindingForGuidDropEditCodeBox_AddCodeBindingWhenVisible()
		{
			DropEdit.Visible = true;
			Form.Controls.Add(DropEdit);
			Form.Show();

			var listElement = Dummy.Dependents.AddNew();
			listElement.ZD1_Code = "Value";
			Dummy.Z0_Guid = listElement.PK;
			DropEdit.BindToList = "Dependents";
			DropEdit.SetDataBinding(Dummy, DummyBizoSchema.Z0_Guid.Name);

			Thread.Sleep(200);
			Application.DoEvents();

			Assert(DropEdit.CodeBindingAdded);
		}

		public void TestBindingForGuidDropEditCodeBox_AddCodeBindingEvenWhenNotVisible()
		{
			DropEdit.Visible = false;
			Form.Controls.Add(DropEdit);
			Form.Show();

			var listElement = Dummy.Dependents.AddNew();
			listElement.ZD1_Code = "Value";
			Dummy.Z0_Guid = listElement.PK;
			DropEdit.BindToList = "Dependents";
			DropEdit.SetDataBinding(Dummy, DummyBizoSchema.Z0_Guid.Name);

			Thread.Sleep(200);
			Application.DoEvents();

			Assert(DropEdit.CodeBindingAdded);
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (dropEdit != null)
			{
				dropEdit.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		DummyWithDependentsBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithDependentsBusinessObject>()); }
		}
		DummyWithDependentsBusinessObject dummy;

		ZForm Form
		{
			get { return form ?? (form = new ZForm()); }
		}
		ZForm form;

		ZGuidDropEdit DropEdit
		{
			get { return dropEdit ?? (dropEdit = new ZGuidDropEdit()); }
		}
		ZGuidDropEdit dropEdit;

		#endregion
	}
}
