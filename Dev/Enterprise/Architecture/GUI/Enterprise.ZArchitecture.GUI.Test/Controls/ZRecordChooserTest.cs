using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZRecordChooserTest : TestCaseWithFactory
	{
		public void TestShowModal()
		{
			using (var form = new ZTestForm())
			{
				form.Show();

				var recordChooser = new ZRecordChooser<BusinessObject>(ModuleIDs.RefCountry);
				var selectedCountryCode = "";
				recordChooser.ShowModal(form, delegate(BusinessObject[] selectedBusinessObjects)
					{
						selectedCountryCode = selectedBusinessObjects[0][RefCountrySchema.RN_Code].ToString();
					});
				var popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm;
				((IActiveBusinessObjectCollection)popup.Module.GridCollection).AdditionalFilter = new ZQuery(RefCountrySchema.RN_Code, "AU");
				object lazyLoad = popup.Module.GridCollection.Count;
				((ZDisplayGrid)popup.Module.DisplayGrid).Select(0);
				popup.ExposedOKButtonForTesting.PerformClick();
				AssertEquals("Should be selected and event handler should be invoked", "AU", selectedCountryCode);
			}
		}

		public void TestNoLeakingEvents()
		{
			var recordChooser = new ZRecordChooser<BusinessObject>(ModuleIDs.RefCountry);
			var weakReference = GetWeakReferenceForPopup(recordChooser);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert(!weakReference.IsAlive);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		WeakReference GetWeakReferenceForPopup(ZRecordChooser<BusinessObject> recordChooser)
		{
			WeakReference result;

			using (var form = new ZTestForm())
			{
				form.Show();

				recordChooser.ShowModal(form, delegate(BusinessObject[] selectedBusinessObjects)
				{ });
				var popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm;
				result = new WeakReference(popup);
				((IActiveBusinessObjectCollection)popup.Module.GridCollection).AdditionalFilter = new ZQuery();
				object lazyLoad = popup.Module.GridCollection.Count;
				((ZDisplayGrid)popup.Module.DisplayGrid).Select(0);
				popup.ExposedOKButtonForTesting.PerformClick();
			}

			return result;
		}
	}
}
