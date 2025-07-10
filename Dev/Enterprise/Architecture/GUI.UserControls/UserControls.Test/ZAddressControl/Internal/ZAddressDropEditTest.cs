using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	sealed class ZAddressDropEditTest : TestCaseWithFactory
	{
		public void TestSetDataBindingWithSlowWorkFlowException()
		{
			ExceptionReporterTestListener.Instance.Clear();
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var addressDropEditForTest = new ZAddressDropEditForTest())
			{
				addressDropEditForTest.SetDataBinding(dummy, "DocAddress.OrganisationPK");
				UserIdleWorker.Flush();
				addressDropEditForTest.SetDataBinding(dummy, "DocAddress.OrganisationPK");
				UserIdleWorker.Flush();
				AssertEquals("SlowWorkFlow Exception should be suppressed", false, ExceptionReporterTestListener.Instance.Any());
			}
		}

		public void TestShouldPopupFormWhenHasEnoughPADAddressForPICAndDLV()
		{
			using (OrganisationsDataRegistry.Instance.DropdownAddressCountThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 8))
			{
				using (var edit = new ZAddressDropEdit())
				{
					edit.SetDataBinding(GetDummyWithCertainTypeAddresses(9, AddressType.PAD, AddressType.DLV), "DocAddress.OrganisationPK");
					Assert("Should popup because DLV is PAD", edit.AddressDropButton.ShouldPopupForm);

					edit.SetDataBinding(GetDummyWithCertainTypeAddresses(9, AddressType.PAD, AddressType.PIC), "DocAddress.OrganisationPK");
					Assert("Should popup because DLV is PAD", edit.AddressDropButton.ShouldPopupForm);

					edit.SetDataBinding(GetDummyWithCertainTypeAddresses(9, AddressType.PAD, AddressType.OFC), "DocAddress.OrganisationPK");
					Assert("Should not popup because OFC is not PAD", !edit.AddressDropButton.ShouldPopupForm);
				}
			}
		}

		DummyWithDocAddress GetDummyWithCertainTypeAddresses(int num, AddressType type, AddressType defaultType, bool active = true)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var dummy = Factory.New<DummyWithDocAddress>();
			dummy.DocAddresses.AddNew();
			dummy.DocAddress.DefaultAddressType = defaultType;
			for (var i = 0; i < num; i++)
			{
				var address = org.AddressesActive.AddNew();
				address.Address1 = $"Address {i + 1}";
				address.OA_IsActive = active;
				var capability = Factory.New<OrgAddressCapability>();
				capability.PZ_OA = address.PK;
				capability.PZ_AddressType = type.ToString();
			}
			dummy.DocAddresses.Load();
			dummy.DocAddresses[0].OrganisationPK = org.PK;
			Factory.Save();

			return dummy;
		}

		public void TestBizObjNullRef()
		{
			using (var edit = new ZAddressDropEdit())
			{
				var biz = edit.BizObj;     //there was an exception.
				AssertEquals(true, true);  //supress nunit error.
			}
		}

		public void TestUserIdleWorkItemOption()
		{
			using (var edit = new ZAddressDropEdit())
			{
				AssertEquals(UserIdleWorkItemOptions.DisableSlowRunningWarning, edit.WorkItemOption);
			}
		}

		#region Dummy
		class DummyWithDocAddress : DummyBusinessObject, IDocAddresses
		{
			public DummyWithDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public JobDocAddressDependentCollection DocAddresses
			{
				get
				{
					return fDocAddresses ?? (fDocAddresses = new JobDocAddressDependentCollection(this));
				}
			}
			JobDocAddressDependentCollection fDocAddresses;

			ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
			{
				return null;
			}

			Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
			{
				return null;
			}

			IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
			{
				get
				{
					return new DocAddressType[] { DocAddressType.SupplierPickupDeliveryAddress, DocAddressType.TransportBillToAddress };
				}
			}

			JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
			{
				JobDocAddressRequirement requirement = new JobDocAddressRequirement();
				if (addressType == DocAddressType.TransportBillToAddress)
				{
					requirement.IsMandatory = true;
				}
				return requirement;
			}

			public JobDocAddress DocAddress => DocAddresses[0];

			void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
			{
			}

			bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
			{
				return false;
			}

			OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
			{
				return null;
			}
		}
		#endregion

		public void TestPopupFormForAddressesSelection()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var dummy = Factory.New<DummyWithDocAddress>();
			dummy.DocAddresses.AddNew();
			dummy.DocAddress.DefaultAddressType = AddressType.OFC;
			for (int i = 0; i < 8; i++)
			{
				var address = org.AddressesActive.AddNew();
				address.Address1 = $"Address {i + 1}";
				var capability = Factory.New<OrgAddressCapability>();
				capability.PZ_OA = address.PK;
				capability.PZ_AddressType = nameof(AddressType.OFC);
			}
			dummy.DocAddresses.Load();
			dummy.DocAddresses[0].OrganisationPK = org.PK;

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.DropdownAddressCountThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 8))
			using (var mainForm = new ZForm())
			using (var form = new ZForm(dummy))
			using (var edit = new ZAddressDropEdit())
			{
				mainForm.Show();
				form.Controls.Add(edit);
				edit.SetDataBinding(dummy, "DocAddress.OrganisationPK");
				form.Show();

				AssertEquals("Precondition", false, edit.AddressDropButton.PopupFormShowed);

				edit.ShowDropDown();
				Application.DoEvents();
				var popup = edit.AddressDropButton.popupForm;
				Assert(popup.Visible);
				AssertEquals(true, edit.AddressDropButton.PopupFormShowed);
				Assert(!form.CanFocus);
				Assert(mainForm.CanFocus);
				popup.Close();
				Assert(form.CanFocus);
				Assert(!popup.IsDisposed);
				AssertEquals(false, edit.AddressDropButton.PopupFormShowed);

				UserIdleWorker.Flush();
			}
		}

		public void TestPopupFormWhenFilterAddressedByDefaultTypeIsFalse()
		{
			using (OrganisationsDataRegistry.Instance.DropdownAddressCountThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 8))
			using (var edit = new ZAddressDropEdit())
			{
				edit.FilterAddressedByDefaultType = false;
				edit.SetDataBinding(GetDummyWithCertainTypeAddresses(9, AddressType.APM, AddressType.OFC), "DocAddress.OrganisationPK");
				Assert("Should popup because FilterAddressedByDefaultType is false", edit.AddressDropButton.ShouldPopupForm);

				edit.FilterAddressedByDefaultType = true;
				edit.SetDataBinding(GetDummyWithCertainTypeAddresses(9, AddressType.APM, AddressType.OFC), "DocAddress.OrganisationPK");
				Assert("Should not popup because FilterAddressedByDefaultType is true", !edit.AddressDropButton.ShouldPopupForm);
			}
		}

		public void TestPopupFormOnlyCountTheActiveAddress()
		{
			using (OrganisationsDataRegistry.Instance.DropdownAddressCountThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 8))
			using (var edit = new ZAddressDropEdit())
			{
				edit.SetDataBinding(GetDummyWithCertainTypeAddresses(9, AddressType.APM, AddressType.APM, false), "DocAddress.OrganisationPK");
				Assert("Should not popup because address is inactive", !edit.AddressDropButton.ShouldPopupForm);

				edit.SetDataBinding(GetDummyWithCertainTypeAddresses(9, AddressType.APM, AddressType.APM, true), "DocAddress.OrganisationPK");
				Assert("Should popup because address is active", edit.AddressDropButton.ShouldPopupForm);
			}
		}

		public void TestPopupFormDoNotDispose()
		{
			using (OrganisationsDataRegistry.Instance.DropdownAddressCountThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 8))
			using (var edit = new ZAddressDropEdit())
			{
				edit.SetDataBinding(GetDummyWithCertainTypeAddresses(9, AddressType.APM, AddressType.APM), "DocAddress.OrganisationPK");
				AssertEquals(true, edit.AddressDropButton.ShouldPopupForm);
				AssertNotNull(edit.AddressDropButton.popupForm);
				AssertEquals(false, edit.AddressDropButton.popupForm.IsDisposed);

				edit.AddressDropButton.popupForm.Dispose();
				AssertEquals("Mock popup form been disposed", true, edit.AddressDropButton.popupForm.IsDisposed);
				AssertEquals("Access the ShouldPopupForm property", true, edit.AddressDropButton.ShouldPopupForm);
				AssertEquals("Popup form is renew when ShouldPopupForm is true", false, edit.AddressDropButton.popupForm.IsDisposed);

				edit.SetDataBinding(GetDummyWithCertainTypeAddresses(4, AddressType.APM, AddressType.APM), "DocAddress.OrganisationPK");
				AssertEquals(false, edit.AddressDropButton.ShouldPopupForm);
				AssertEquals("ShouldPopupForm is false", false, edit.AddressDropButton.ShouldPopupForm);
				AssertEquals("Not dispose the popup form when ShouldPopupForm is false", false, edit.AddressDropButton.popupForm.IsDisposed);

				edit.AddressDropButton.popupForm.Dispose();
				AssertEquals("Mock popup form been disposed", true, edit.AddressDropButton.popupForm.IsDisposed);
				AssertEquals("ShouldPopupForm is false", false, edit.AddressDropButton.ShouldPopupForm);
				AssertEquals("Not renew the popup form when ShouldPopupForm is true", true, edit.AddressDropButton.popupForm.IsDisposed);
			}
		}

		public void TestClickCheckBoxShowPopupForm()
		{
			AssertClickCheckBoxShowPopupForm(true);
		}

		public void TestClickCheckBoxNotShowPopupForm()
		{
			AssertClickCheckBoxShowPopupForm(false);
		}

		#region Implementation

		class ZAddressDropEditForTest : ZAddressDropEdit
		{
			protected override void UpdateSelection()
			{
				System.Threading.Thread.Sleep(6000);
				base.UpdateSelection();
			}
		}

		class ZAddressDropFormForTest : ZAddressDropForm
		{
			public ZAddressDropFormForTest(IZAddressDropEdit parentDropEdit) : base(parentDropEdit)
			{
			}

			public bool HandleClickInSpecialAreaForTest(Point point) => HandleClickInSpecialArea(point);

			public ZCheckBox AddressTypeFilterCheckBoxForTest => AddressTypeFilterCheckBox;
		}

		void AssertClickCheckBoxShowPopupForm(bool showPopupForm)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var dummy = Factory.New<DummyWithDocAddress>();
			dummy.DocAddresses.AddNew();
			dummy.DocAddress.DefaultAddressType = AddressType.PAD;

			for (var i = 0; i < (showPopupForm ? 7 : 2); i++)
			{
				var address = org.AddressesActive.AddNew();
				address.Address1 = $"Address {i + 1}";
				var capability = Factory.New<OrgAddressCapability>();
				capability.PZ_OA = address.PK;
				capability.PZ_AddressType = nameof(AddressType.OFC);
			}
			var addressPAD = org.AddressesActive.AddNew();
			addressPAD.Address1 = "Address 8";
			var capabilityPAD = Factory.New<OrgAddressCapability>();
			capabilityPAD.PZ_OA = addressPAD.PK;
			capabilityPAD.PZ_AddressType = nameof(AddressType.PAD);

			dummy.DocAddresses.Load();
			dummy.DocAddresses[0].OrganisationPK = org.PK;

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.DropdownAddressCountThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 8))
			using (var mainForm = new ZForm())
			using (var form = new ZForm(dummy))
			using (var edit = new ZAddressDropEdit())
			{
				mainForm.Show();
				form.Controls.Add(edit);
				edit.SetDataBinding(dummy, "DocAddress.OrganisationPK");
				form.Show();

				edit.ShowDropDown();
				Application.DoEvents();
				var popup = edit.AddressDropButton.popupForm;
				AssertNull("Should not show popup form", popup);

				using (var dropForm = new ZAddressDropFormForTest(edit))
				{
					var result = dropForm.HandleClickInSpecialAreaForTest(dropForm.AddressTypeFilterCheckBoxForTest.Bounds.Location);
					AssertEquals(true, result);
				}

				popup = edit.AddressDropButton.popupForm;

				if (showPopupForm)
				{
					AssertNotNull("Should show popup form", popup);
					Assert(popup.Visible);
					Assert(!form.CanFocus);
					Assert(mainForm.CanFocus);
					popup.Close();
					Assert(form.CanFocus);
					Assert(!popup.IsDisposed);
				}
				else
				{
					AssertNull("Should not show popup form", popup);
				}

				UserIdleWorker.Flush();
			}
		}

		#endregion
	}
}
