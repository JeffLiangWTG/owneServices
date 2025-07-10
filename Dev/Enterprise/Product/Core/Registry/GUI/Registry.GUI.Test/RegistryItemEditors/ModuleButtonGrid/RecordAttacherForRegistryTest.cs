#if !WINZOR
using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RecordAttacherForRegistryTest : ZRecordAttacherTest
	{
		#region TestAttach

		public void TestAttach()
		{
			using (var form = new ZForm())
			{
				var firstControl = new RegistryZUserControl();
				var parentControl = new RegistryZUserControl();
				form.Controls.Add(firstControl);
				form.Controls.Add(parentControl);
				form.Show();

				var collection = new DummyProxyCollection();
				var attacher = new RecordAttacherForRegistry<DummyProxy>(parentControl, collection, new DummyBusinessObjectCollection(Factory), DummyModuleIDs.Dummy);
				attacher.Show(form);
				AssertEquals("Precondition: Nothing in collection.", 0, collection.Count);

				var dummyBizO1 = Factory.New<CargoWise.EntityFramework.Testing.DummyBusinessObject>();
				Factory.Save();

				using (var popup = attacher.LastShownAttachPopupForTesting)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { dummyBizO1 });
					AssertEquals("No Message means attach was successful.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertContainsExactElementsInAnyOrder(new[] { dummyBizO1.PK }, collection.Cast<DummyProxy>().Select(d => d.ProxyPK));
				}

				AssertEquals("Parent Registry control should be focused after attaching.", true, parentControl.Focused);

				using (var popup = attacher.LastShownAttachPopupForTesting)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { dummyBizO1 });
					AssertEquals("Warning Message means attach was not successful, this is because the PK was already in the collection.", true, UnitTestUserNotification.Instance.LastMessage.WasWarning);

					var dummyBizO2 = Factory.New<CargoWise.EntityFramework.Testing.DummyBusinessObject>();
					var dummyBizO3 = Factory.New<CargoWise.EntityFramework.Testing.DummyBusinessObject>();
					Factory.Save();

					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { dummyBizO3, dummyBizO2 });
					AssertContainsExactElementsInAnyOrder(new[] { dummyBizO1.PK, dummyBizO3.PK, dummyBizO2.PK }, collection.Cast<DummyProxy>().Select(d => d.ProxyPK));
				}
			}
		}

		#endregion

		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException),
				() => new RecordAttacherForRegistry<DummyProxy>(null, new DummyProxyCollection(), new DummyBusinessObjectCollection(Factory), DummyModuleIDs.Dummy));
		}

		#endregion

		#region Implementation

		internal class DummyProxy : RegistryProxyBusinessObject
		{
			public ZString Name
			{
				get { return "Dummy"; }
			}

			protected override RegistryProxyBusinessObject GetNew(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			{
				return new DummyProxy();
			}

			protected override Type ParentCollectionType
			{
				get { return typeof(DummyProxyCollection); }
			}
		}

		internal class DummyProxyCollection : RegistryProxyBusinessObjectCollection<DummyProxy>
		{
			public DummyProxyCollection()
				: base(null, null)
			{
			}

			protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			{
				return new DummyProxyCollection();
			}
		}

		#endregion
	}
}
#endif
