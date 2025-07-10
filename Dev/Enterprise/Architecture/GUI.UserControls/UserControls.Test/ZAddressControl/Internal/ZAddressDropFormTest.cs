using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Application = System.Windows.Forms.Application;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class ZAddressDropFormTest : TestCaseWithFactory
	{
		public void TestShowAddressTypeFilter()
		{
			var dummy = Factory.New<DummyWithZAddress>();
			using (var form = new FormWithZAddressDropEdit(dummy))
			{
				form.Show();
				Application.DoEvents();

				dummy.Z0_Guid_ZAddress.DefaultAddressType = AddressType.NoDefault;
				using (var dropForm = new ZAddressDropFormForTest(form.AddressDropEdit))
				{
					Assert("No filter if default address type is not set", !dropForm.ShowAddressTypeFilter);
				}

				dummy.Z0_Guid_ZAddress.DefaultAddressType = AddressType.OFC;
				using (var dropForm = new ZAddressDropFormForTest(form.AddressDropEdit))
				{
					Assert("Show filter when default address type is set", dropForm.ShowAddressTypeFilter);
				}
			}
		}

		public void TestList()
		{
			AssertExpectedAddresses(AddressType.NoDefault, "*");
			AssertExpectedAddresses(AddressType.OFC, "Office");
			AssertExpectedAddresses(AddressType.PIC, "Pickup", "P and D");
			AssertExpectedAddresses(AddressType.DLV, "Delivery", "P and D");
			AssertExpectedAddresses(AddressType.PST, "Postal");
			AssertExpectedAddresses(AddressType.APM);
		}

		void AssertExpectedAddresses(AddressType defaultAddressType, params string[] expectedAddresses)
		{
			var dummy = Factory.New<DummyWithZAddress>();
			using (var form = new FormWithZAddressDropEdit(dummy))
			{
				form.Show();
				Application.DoEvents();

				dummy.Z0_Guid_ZAddress.DefaultAddressType = defaultAddressType;
				using (var dropForm = new ZAddressDropFormForTest(form.AddressDropEdit))
				{
					var iDropEdit = (IZAddressDropEdit)form.AddressDropEdit;
					iDropEdit.FilterAddressedByDefaultType = false;
					dropForm.RefreshList();

					AssertSame(form.AddressDropEdit.List, dropForm.List_Exposed);

					iDropEdit.FilterAddressedByDefaultType = true;
					dropForm.RefreshList();

					if (expectedAddresses.FirstOrDefault() == "*")
					{
						AssertSame(form.AddressDropEdit.List, dropForm.List_Exposed);
					}
					else
					{
						AssertEquals(expectedAddresses.Length, dropForm.List_Exposed.Count);

						foreach (var expectedAddress in expectedAddresses)
						{
							Assert("Should contain address " + expectedAddress, dropForm.List_Exposed.Cast<ICodeDescription>().Any(item => item.Code == expectedAddress));
						}
					}
				}
			}
		}

		public void TestAddressTypeFilterCheckBox()
		{
			Env.Security.OrgAddressChooseUnmatchingOnJob.IsAllowed = true;
			AssertAddressTypeFilterCheckBoxCaption(AddressType.OFC, "OFC - Office Address");
			AssertAddressTypeFilterCheckBoxCaption(AddressType.PIC, "PIC, PAD - Consignment Pickup Address, Consignment Pickup and Delivery Address");
			AssertAddressTypeFilterCheckBoxCaption(AddressType.DLV, "DLV, PAD - Consignment Delivery Address, Consignment Pickup and Delivery Address");
			AssertAddressTypeFilterCheckBoxCaption(AddressType.PST, "PST - Postal Address");

			Env.Security.OrgAddressChooseUnmatchingOnJob.IsAllowed = false;
			AssertAddressTypeFilterCheckBoxCaption(AddressType.OFC, string.Format("OFC - Office Address\r\nYou do not have security access to uncheck filter"));
			AssertAddressTypeFilterCheckBoxCaption(AddressType.PIC, string.Format("PIC, PAD - Consignment Pickup Address, Consignment Pickup and Delivery Address\r\nYou do not have security access to uncheck filter"));
			AssertAddressTypeFilterCheckBoxCaption(AddressType.DLV, string.Format("DLV, PAD - Consignment Delivery Address, Consignment Pickup and Delivery Address\r\nYou do not have security access to uncheck filter"));
			AssertAddressTypeFilterCheckBoxCaption(AddressType.PST, string.Format("PST - Postal Address\r\nYou do not have security access to uncheck filter"));
		}

		void AssertAddressTypeFilterCheckBoxCaption(AddressType defaultAddressType, string expectedCaption)
		{
			var dummy = Factory.New<DummyWithZAddress>();
			using (var form = new FormWithZAddressDropEdit(dummy))
			{
				form.Show();
				Application.DoEvents();

				dummy.Z0_Guid_ZAddress.DefaultAddressType = defaultAddressType;
				using (var dropForm = new ZAddressDropForm(form.AddressDropEdit))
				{
					AssertEquals(expectedCaption, dropForm.AddressTypeFilterCheckBox.Text);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestDeletedAddress()
		{
			var dummy = Factory.New<DummyWithZAddress>();
			//// create OrgHeaders
			//IOrgHeader OrgHeader = Factory.New<IOrgHeader>();
			//BusinessObject OrgHeaderBizObj = (BusinessObject)OrgHeader;
			//OrgHeaderBizObj["OH_RL_NKClosestPort"] = "AUMLB";
			//OrgHeaderBizObj[OrgHeaderSchema.OH_Code] = "OH" + new Random().Next(10000).ToString();

			//IOrgAddress OrgAddress = Factory.New<IOrgAddress>();
			//BusinessObject AddressBizObj = (BusinessObject)OrgAddress;
			//AddressBizObj["OA_OH"] = OrgHeader.PK;
			//AddressBizObj["OA_Address1"] = "101";
			//AddressBizObj["OA_Address2"] = "Collins St.";
			//AddressBizObj["OA_City"] = "Melbourne";
			//AddressBizObj["OA_State"] = "VIC";
			//AddressBizObj["OA_PostCode"] = "2000";
			//AddressBizObj["OA_CompanyNameOverride"] = "Hugo Boss";
			//dummy.Z0_Guid_ZAddress.OrgPK = OrgHeader.PK;
			Factory.Save();

			using (var form = new FormWithZAddressDropEdit(dummy))
			{
				form.Show();
				Application.DoEvents();

				dummy.Z0_Guid_ZAddress.Delete();
				using (var dropForm = new ZAddressDropForm(form.AddressDropEdit))
				{
					KeySender.PostKeyDown(form.AddressDropEdit, Keys.F3);
					Application.DoEvents();
				}
			}
		}

		public void TestFilteredListShouldChangeAsPerList()
		{
			var dummy = Factory.New<DummyWithZAddress>();
			dummy.Z0_Guid_ZAddress.DefaultAddressType = AddressType.DLV;

			using (var form = new FormWithZAddressDropEdit(dummy))
			{
				form.Show();
				Application.DoEvents();

				var filteredList = typeof(ZAddressDropEdit).GetProperty("FilteredList", BindingFlags.NonPublic | BindingFlags.Instance);

				form.AddressDropEdit.ShowDropDown();
				AssertEquals(2, ((IList)filteredList.GetValue(form.AddressDropEdit)).Count);

				((ZDropButton)form.AddressDropEdit.Controls.Find("DropButton", true)[0]).DropDown_Exposed.HideDropDown();
				var newAddressList = new ZAddressList();
				dummy.SetAddressList(newAddressList);
				form.AddressDropEdit.ShowDropDown();
				AssertEquals(0, ((IList)filteredList.GetValue(form.AddressDropEdit)).Count);

				((ZDropButton)form.AddressDropEdit.Controls.Find("DropButton", true)[0]).DropDown_Exposed.HideDropDown();
				newAddressList.AddAddress(ZGuid.NewZGuid(), "P and D", "Pickup and deliver address", new AddressCapabilityItem { Capability = OrgConstants.AddressType.PickupAndDelivery });
				dummy.SetAddressList(newAddressList);
				form.AddressDropEdit.ShowDropDown();
				AssertEquals(1, ((IList)filteredList.GetValue(form.AddressDropEdit)).Count);
			}
		}

		public void TestDropFormIsItemSelectable()
		{
			var dummy = Factory.New<DummyWithZAddress>();
			using (var form = new FormWithZAddressDropEdit(dummy))
			{
				form.Show();
				Application.DoEvents();

				using (var dropForm = new ZAddressDropFormForTest(form.AddressDropEdit))
				{
					Assert(!dropForm.IsItemSelectable(null));
					Assert(dropForm.IsItemSelectable(dropForm.List_Exposed[0] as ICodeDescription));
				}
			}
		}

		#region Test classes

		#region DummyWithZAddress

		class DummyWithZAddress : DummyBusinessObject
		{
			public DummyWithZAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZAddress Z0_Guid_ZAddress
			{
				get
				{
					if (address == null)
					{
						address = new ZAddress(Z0_GuidInfo);
						address.AddresssListOverride = (f, l) => AddressList;
						RegisterEditableChildObject(address);
					}
					return address;
				}
			}
			ZAddress address;

			ZAddressList AddressList
			{
				get
				{
					if (addressList == null)
					{
						addressList = new ZAddressList();
						addressList.AddAddress(ZGuid.NewZGuid(), "Office", "Office address", new AddressCapabilityItem { Capability = OrgConstants.AddressType.Office });
						addressList.AddAddress(ZGuid.NewZGuid(), "Pickup", "Pickup address", new AddressCapabilityItem { Capability = OrgConstants.AddressType.Pickup });
						addressList.AddAddress(ZGuid.NewZGuid(), "Delivery", "Deliver address", new AddressCapabilityItem { Capability = OrgConstants.AddressType.Delivery });
						addressList.AddAddress(ZGuid.NewZGuid(), "P and D", "Pickup and deliver address", new AddressCapabilityItem { Capability = OrgConstants.AddressType.PickupAndDelivery });
						addressList.AddAddress(ZGuid.NewZGuid(), "Postal", "Postal address", new AddressCapabilityItem { Capability = OrgConstants.AddressType.Postal });
						addressList.AddAddress(ZGuid.NewZGuid(), "Misc", "Misc address", new AddressCapabilityItem { Capability = OrgConstants.AddressType.Miscellaneous });
					}
					return addressList;
				}
			}
			ZAddressList addressList;

			public void SetAddressList(ZAddressList addressList)
			{
				this.addressList = addressList;
			}
		}

		#endregion

		#region FormWithZAddressDropEdit

		class FormWithZAddressDropEdit : ZForm
		{
			public FormWithZAddressDropEdit(DummyWithZAddress bizo) : base(bizo)
			{
				var addressParentControl = new AddressParentControl();
				addressParentControl.SetBindingMember(DummyBizoSchema.Constants.Z0_Guid);
				Controls.Add(addressParentControl);

				AddressDropEdit = new ZAddressDropEdit();
				AddressDropEdit.SetBindingMember(".");
				addressParentControl.Controls.Add(AddressDropEdit);
			}

			public ZAddressDropEdit AddressDropEdit { get; private set; }
		}

		class AddressParentControl : ZUserControl, IZAddressParent
		{
			void IZAddressParent.SetControlSize() { }

			ZGuid IZAddressParent.ParseCode(string code)
			{
				return ZGuid.Invalid;
			}

			bool IZAddressParent.CheckZAddressBindingSuffix => true;
		}

		#endregion

		#endregion
	}

	class ZAddressDropFormForTest : ZAddressDropForm
	{
		public ZAddressDropFormForTest(IZAddressDropEdit parentDropEdit) : base(parentDropEdit)
		{
		}

		internal IList List_Exposed => List;
	}
}
