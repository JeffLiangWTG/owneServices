using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZAddressDropEditColumnStyleTest : TestCaseWithFactory
	{
		#region TestEndToEnd

		public void TestEndToEnd()
		{
			var bizobj = Factory.New<DummyBusinessObjectWithList>();

			var relatedBizObj0 = bizobj.List.AddNew();
			relatedBizObj0.Z0_Code = "Z";
			relatedBizObj0.Z0_Guid = relatedBizObj0.PK;

			var relatedBizObj1 = bizobj.List.AddNew();
			relatedBizObj1.Z0_Code = "A";
			relatedBizObj1.Z0_Guid = relatedBizObj1.PK;

			var relatedBizObj2 = bizobj.List.AddNew();
			relatedBizObj2.Z0_Code = "C";
			relatedBizObj2.Z0_Guid = relatedBizObj2.PK;

			var relatedBizObj3 = bizobj.List.AddNew();
			relatedBizObj3.Z0_Code = "D";
			relatedBizObj3.Z0_Guid = relatedBizObj3.PK;

			relatedBizObj0.LookupList.Add(relatedBizObj0);
			relatedBizObj0.LookupList.Add(relatedBizObj1);
			relatedBizObj0.LookupList.Add(relatedBizObj2);

			using (var form = new ZAddressDropEditColumnStyleTestForm(bizobj))
			{
				form.Show();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals(typeof(ZAddressGridGuidDropEdit), form.ActiveControl.GetType());

				var codeEdit = (ZAddressGridGuidDropEdit)form.ActiveControl;

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.A);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals("Codes should be correctly linked", relatedBizObj1.Z0_Guid, relatedBizObj0.Z0_Guid);

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Shift | Keys.Tab);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.D);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals(ZGuid.Invalid, relatedBizObj0.Z0_Guid);

				form.zAddressDropEditColumnStyleInfo1.Parse = delegate(ZString code)
				{ return relatedBizObj3.PK; };

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Shift | Keys.Tab);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.A);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals("Codes should be correctly linked", relatedBizObj1.Z0_Guid, relatedBizObj0.Z0_Guid);

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Shift | Keys.Tab);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.D);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals(relatedBizObj3.PK, relatedBizObj0.Z0_Guid);
			}
		}

		#endregion

		#region TestReadOnlyColumnDisplaysCodeText

		public void TestReadOnlyColumnDisplaysCodeText()
		{
			var bizobj = Factory.New<DummyBusinessObjectWithList>();

			var relatedBizObj0 = bizobj.List.AddNew();
			relatedBizObj0.Z0_Code = "Z";
			relatedBizObj0.Z0_Guid = relatedBizObj0.PK;
			relatedBizObj0.LookupList.Add(relatedBizObj0);

			var relatedBizObj1 = bizobj.List.AddNew();
			relatedBizObj1.Z0_Code = "A";
			relatedBizObj1.Z0_Guid = relatedBizObj1.PK;
			relatedBizObj1.LookupList.Add(relatedBizObj1);

			var relatedBizObj2 = bizobj.List.AddNew();
			relatedBizObj2.Z0_Code = "C";
			relatedBizObj2.Z0_Guid = relatedBizObj2.PK;
			relatedBizObj2.LookupList.Add(relatedBizObj2);

			bizobj.SetReadOnlyIncludingChildren(true);

			relatedBizObj0.SetReadOnlyIncludingChildren(true);
			relatedBizObj1.SetReadOnlyIncludingChildren(true);
			relatedBizObj2.SetReadOnlyIncludingChildren(true);

			using (var form = new ZAddressDropEditColumnStyleTestForm(bizobj))
			{
				form.Show();
				form.Grid.Focus();

				AssertEquals(typeof(DataGridTextBox), form.ActiveControl.GetType());
				DataGridTextBox textBox = (DataGridTextBox)form.ActiveControl;
				AssertEquals("should have code value not guid or raw data", "Z", textBox.Text);
			}
		}

		#endregion

		#region TestSorting

		public void TestSorting()
		{
			var bizobj = Factory.New<DummyBusinessObjectWithList>();

			var relatedBizObj0 = bizobj.List.AddNew();
			relatedBizObj0.Z0_Code = "Z";
			relatedBizObj0.Z0_Guid = relatedBizObj0.PK;
			relatedBizObj0.LookupList.Add(relatedBizObj0);

			var relatedBizObj1 = bizobj.List.AddNew();
			relatedBizObj1.Z0_Code = "A";
			relatedBizObj1.Z0_Guid = relatedBizObj1.PK;
			relatedBizObj1.LookupList.Add(relatedBizObj1);

			var relatedBizObj2 = bizobj.List.AddNew();
			relatedBizObj2.Z0_Code = "C1";
			relatedBizObj2.Z0_Guid = relatedBizObj2.PK;
			relatedBizObj2.LookupList.Add(relatedBizObj2);

			var relatedBizObj3 = bizobj.List.AddNew();
			relatedBizObj3.Z0_Code = "C0";
			relatedBizObj3.Z0_Guid = relatedBizObj3.PK;
			relatedBizObj3.LookupList.Add(relatedBizObj3);

			using (var form = new ZAddressDropEditColumnStyleTestForm(bizobj))
			{
				form.Show();
				form.Grid.Focus();

				const int ClickX = 80;
				const int ClickY = 10;
				AssertEquals("Click will hit ColumnHeader", DataGrid.HitTestType.ColumnHeader, form.Grid.HitTest(new Point(ClickX, ClickY)).Type);

				var grid = form.Grid as DataGrid;
				var eventArgs = new MouseEventArgs(MouseButtons.Left, 1, ClickX, ClickY, 0);
				typeof(DataGrid).InvokeMember("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, grid, new object[] { eventArgs });
				typeof(DataGrid).InvokeMember("OnMouseUp", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, grid, new object[] { eventArgs });

				AssertEquals("A", ((DummyBusinessObjectInList)form.Grid.ListManager.List[0]).Z0_Code);
				AssertEquals("C0", ((DummyBusinessObjectInList)form.Grid.ListManager.List[1]).Z0_Code);
				AssertEquals("C1", ((DummyBusinessObjectInList)form.Grid.ListManager.List[2]).Z0_Code);
				AssertEquals("Z", ((DummyBusinessObjectInList)form.Grid.ListManager.List[3]).Z0_Code);

				typeof(DataGrid).InvokeMember("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, grid, new object[] { eventArgs });
				typeof(DataGrid).InvokeMember("OnMouseUp", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, grid, new object[] { eventArgs });

				AssertEquals("Z", ((DummyBusinessObjectInList)form.Grid.ListManager.List[0]).Z0_Code);
				AssertEquals("C1", ((DummyBusinessObjectInList)form.Grid.ListManager.List[1]).Z0_Code);
				AssertEquals("C0", ((DummyBusinessObjectInList)form.Grid.ListManager.List[2]).Z0_Code);
				AssertEquals("A", ((DummyBusinessObjectInList)form.Grid.ListManager.List[3]).Z0_Code);
			}
		}

		#endregion

		#region Test Addy & GetFilteredListForDropDown

		public void TestAddy_BizObjIsCurrentItem()
		{
			var dummyWithDummiesWithZAddress = Factory.New<DummyWithDummiesWithZAddress>();
			var dummyWithZAddress = Factory.New<DummyWithZAddress>();
			dummyWithZAddress.Address_ZAddress.AddresssListOverride += dummyWithZAddress.AddresssListOverride1;
			dummyWithDummiesWithZAddress.List.Add(dummyWithZAddress);
			using (var form = new ZForm(dummyWithDummiesWithZAddress))
			{
				var grid = new ZGrid();
				grid.Width = 200;
				grid.ColumnStyles.Clear();
				form.Controls.Add(grid);
				var addressColumnStyleInfo = new ZAddressDropEditColumnStyleInfo { ColumnName = "Address" };
				grid.ColumnStyles.Add(addressColumnStyleInfo);
				grid.SetDataBinding(form.DataSource, "List");

				form.Show();
				Application.DoEvents();

				AssertEquals("Initialize the the grid cell value", "test", ((DummyWithZAddress)grid.ListManager.List[0]).Address_ZAddress.OrgAddress_List.List[0].Code);

				Rectangle newCell = grid.GetCellBounds(1, 0);
				var eventArgs = new MouseEventArgs(MouseButtons.Left, 1, newCell.X, newCell.Y, 0);
				typeof(DataGrid).InvokeMember("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, grid, new object[] { eventArgs });
				grid.Refresh();

				AssertEquals("when click new line, the previous shoule be still exist", "test", ((DummyWithZAddress)grid.ListManager.List[0]).Address_ZAddress.OrgAddress_List.List[0].Code);
			}
		}

		public void TestOverrideGetFilteredListForDropDown()
		{
			var dummyWithDummiesWithZAddress = Factory.New<DummyWithDummiesWithZAddress>();
			var dummyWithZAddress = Factory.New<DummyWithZAddress>();
			dummyWithZAddress.Address_ZAddress.AddresssListOverride += dummyWithZAddress.AddresssListOverride1;
			dummyWithDummiesWithZAddress.List.Add(dummyWithZAddress);
			using (var form = new ZForm(dummyWithDummiesWithZAddress))
			{
				ZAddressGridGuidDropEdit zAddressGridGuidDropEdit = new ZAddressGridGuidDropEdit();
				form.Controls.Add(zAddressGridGuidDropEdit);
				zAddressGridGuidDropEdit.SetDataBinding(dummyWithZAddress, "Address");
				form.Show();
				Application.DoEvents();

				var filteredListForDropDown = (IList)typeof(ZAddressGridGuidDropEdit).GetMethod("GetFilteredListForDropDown", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(zAddressGridGuidDropEdit, null);

				AssertEquals("it remains 1 addrress in filteredList", 1, filteredListForDropDown.Count);
				AssertEquals("there are 3 addrresses in List", 3, zAddressGridGuidDropEdit.List.Count);

				dummyWithZAddress.Address_ZAddress.AddresssListOverride -= dummyWithZAddress.AddresssListOverride1;
				dummyWithZAddress.Address_ZAddress.AddresssListOverride += dummyWithZAddress.AddresssListOverride2;

				zAddressGridGuidDropEdit.SetDataBinding(dummyWithZAddress, "Address"); //Refreshes internal list
				zAddressGridGuidDropEdit.Refresh();
				Thread.Sleep(1000);
				Application.DoEvents();
				filteredListForDropDown = (IList)typeof(ZAddressGridGuidDropEdit).GetMethod("GetFilteredListForDropDown", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(zAddressGridGuidDropEdit, null);

				AssertEquals("it remains 2 addrress in filteredList", 2, filteredListForDropDown.Count);
				AssertEquals("there are 2 addrresses in List", 2, zAddressGridGuidDropEdit.List.Count);
			}
		}

		class DummyWithZAddress : DummyBusinessObject
		{
			public DummyWithZAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				if (address == null)
				{
					address = new ZAddress(AddressInfo);
					address.DefaultAddressType = AddressType.APM;
				}
			}

			public ZAddress address;

			public ZGuid Address { get { return address.PK; } }

			public ZPropertyInfo AddressInfo { get { return GetZPropertyInfo(nameof(Address)); } }

			public ZAddress Address_ZAddress { get { return address; } }

			public ZAddressList AddresssListOverride1(BusinessObjectFactory factory, ZAddressList addressList)
			{
				var addrList = new ZAddressList();
				var address1 = new ZAddress(GetZPropertyInfo(nameof(Address)));
				var address2 = new ZAddress(GetZPropertyInfo(nameof(Address)));

				addrList.AddAddress(address.PK, "test", "Addr", new AddressCapabilityItem[] { new AddressCapabilityItem() { Capability = "APM", IsDefault = true } });
				addrList.AddAddress(address1.PK, "test1", "Addr1", new AddressCapabilityItem[] { new AddressCapabilityItem() { Capability = "ARM", IsDefault = false } });
				addrList.AddAddress(address2.PK, "test2", "Addr2", new AddressCapabilityItem[] { new AddressCapabilityItem() { Capability = "ARM", IsDefault = true } });
				return addrList;
			}

			public ZAddressList AddresssListOverride2(BusinessObjectFactory factory, ZAddressList addressList)
			{
				var addrList = new ZAddressList();
				var address1 = new ZAddress(GetZPropertyInfo(nameof(Address)));

				addrList.AddAddress(address.PK, "test3", "Addr3", new AddressCapabilityItem[] { new AddressCapabilityItem() { Capability = "APM", IsDefault = true } });
				addrList.AddAddress(address1.PK, "test4", "Addr4", new AddressCapabilityItem[] { new AddressCapabilityItem() { Capability = "APM", IsDefault = false } });

				return addrList;
			}
		}

		class DummyWithZAddressCollection : BusinessObjectCollection<DummyWithZAddress>
		{
			public DummyWithZAddressCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		class DummyWithDummiesWithZAddress : DummyBusinessObject
		{
			public DummyWithDummiesWithZAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			DummyWithZAddressCollection list;
			public DummyWithZAddressCollection List
			{
				get
				{
					if (list == null)
					{
						list = new DummyWithZAddressCollection(Factory);
					}
					return list;
				}
			}
		}

		#endregion

		#region TestAddy_ListContainsIZAddress

		public void TestAddy_ListContainsIZAddress()
		{
			var address1 = Factory.New<DummyBusinessObjectInList>();
			address1.Z0_Code = "A";
			address1.Z0_Guid = ZGuid.NewZGuid();

			var address2 = Factory.New<DummyBusinessObjectInList>();
			address2.Z0_Code = "B";
			address2.Z0_Guid = ZGuid.NewZGuid();

			var childWithAddresses = Factory.New<DummyJobDocAddress>();
			childWithAddresses.Z0_Code = "CHILD";
			childWithAddresses.Z0_Guid = address2.Z0_Guid;
			childWithAddresses.LookupList.Add(address1);
			childWithAddresses.LookupList.Add(address2);

			var master = Factory.New<DummyBusinessObjectWithList>();
			master.List.Add(childWithAddresses);

			using (var form = new ZAddressDropEditColumnStyleTestForm(master))
			{
				form.Show();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals(typeof(ZAddressGridGuidDropEdit), form.ActiveControl.GetType());

				var codeEdit = (ZAddressGridGuidDropEdit)form.ActiveControl;
				var iCodeEdit = (IZAddressDropEdit)codeEdit;
				AssertEquals(childWithAddresses, iCodeEdit.Addy);

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.A);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals("bizObj should have A selected", address1.PK, childWithAddresses.Z0_Guid);
			}
		}

		#endregion

		#region TestAddy_ListContainsObjectsThatReferenceAnotherObjectWithIZAddress

		public void TestAddy_ListContainsObjectsThatReferenceAnotherObjectWithIZAddress()
		{
			var address1 = Factory.New<DummyBusinessObjectInList>();
			address1.Z0_Code = "A";
			address1.Z0_Guid = ZGuid.NewZGuid();

			var address2 = Factory.New<DummyBusinessObjectInList>();
			address2.Z0_Code = "B";
			address2.Z0_Guid = ZGuid.NewZGuid();

			var childWithAddresses = Factory.New<DummyWithDocAddress>();
			childWithAddresses.Z0_Code = "CHILD";
			childWithAddresses.Z0_Guid = ZGuid.NewZGuid();
			childWithAddresses.Address.Z0_Code = "ADDY";
			childWithAddresses.Address.E2_OA_Address = address2.Z0_Guid;
			childWithAddresses.LookupList.Add(address1);
			childWithAddresses.LookupList.Add(address2);

			var master = Factory.New<DummyWithAddressList>();
			master.AddressList.Add(childWithAddresses);

			using (var form = new ZAddressDropEditColumnStyleTestFormWithJobDocAddress(master))
			{
				form.Show();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals(typeof(ZAddressGridGuidDropEdit), form.ActiveControl.GetType());

				var codeEdit = (ZAddressGridGuidDropEdit)form.ActiveControl;
				var iCodeEdit = (IZAddressDropEdit)codeEdit;
				AssertEquals(childWithAddresses.Address, iCodeEdit.Addy);

				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.A);
				Application.DoEvents();
				KeySender.PostKeyDown(codeEdit.CodeBox, codeEdit.CodeBox.Handle, Keys.Tab);
				Application.DoEvents();

				AssertEquals("bizObj should have A selected", address1.PK, childWithAddresses.Address.E2_OA_Address);
			}
		}

		#region DummyWithAddressList

		class DummyWithAddressList : DummyBusinessObject
		{
			public DummyWithAddressList(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public DummyWithDocAddressCollection AddressList
			{
				get { return list ?? (list = new DummyWithDocAddressCollection(Factory)); }
			}
			DummyWithDocAddressCollection list;
		}

		#endregion

		#region DummyWithDocAddress

		class DummyWithDocAddress : DummyBusinessObjectInList
		{
			public DummyWithDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public DummyJobDocAddress Address
			{
				get { return address ?? (address = Factory.New<DummyJobDocAddress>()); }
			}
			DummyJobDocAddress address;
		}

		class DummyWithDocAddressCollection : BusinessObjectCollection<DummyWithDocAddress>
		{
			public DummyWithDocAddressCollection(BusinessObjectFactory factory) : base(factory) { }

			public new DummyWithDocAddress this[int i]
			{
				get { return (DummyWithDocAddress)Elements[i]; }
			}
		}

		#endregion

		#region ZAddressDropEditColumnStyleTestFormWithJobDocAddress

		[TestExcludeZWinFormHasTypedConstructor]
		[TestExcludeZWinFormsAllHaveFormBashers]
		class ZAddressDropEditColumnStyleTestFormWithJobDocAddress : ZTestForm
		{
			public ZAddressDropEditColumnStyleTestFormWithJobDocAddress(BusinessObject bizObj)
				: base(bizObj)
			{
			}

			public ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1;

			#region InitializeComponent

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.Grid.ColumnStyles.Clear();
				Grid.BindTo = "AddressList";
				zAddressDropEditColumnStyleInfo1 = new ZAddressDropEditColumnStyleInfo();
				zAddressDropEditColumnStyleInfo1.BindToList = "LookupList";
				zAddressDropEditColumnStyleInfo1.Caption = "LOL";
				zAddressDropEditColumnStyleInfo1.ColumnName = "Address+E2_OA_Address";
				this.Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			}

			#endregion
		}

		#endregion

		#endregion

		#region DummyJobDocAddress

		class DummyJobDocAddress : DummyBusinessObjectInList, IZAddress
		{
			public DummyJobDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			#region E2_OA_Address

			public ZGuid E2_OA_Address { get; set; }

			public ZPropertyInfo E2_OA_AddressInfo
			{
				get { return GetZPropertyInfo(nameof(E2_OA_Address)); }
			}

			#endregion

			#region IZAddress Members

			ZAddressList IZAddress.AddressList
			{
				get { return null; }
			}

			AddressType IZAddress.DefaultAddressType
			{
				get { return AddressType.NoDefault; }
			}

			BusinessObjectFactory IZAddress.Factory
			{
				get { return Factory; }
			}

			bool IZAddress.IsDeleted
			{
				get { return IsDeleted; }
			}

			ZGuid IZAddress.OrgPK
			{
				get { throw new NotImplementedException(); }
			}

			#endregion
		}

		#endregion

		#region ZAddressDropEditColumnStyleTestForm

		[TestExcludeZWinFormHasTypedConstructor]
		[TestExcludeZWinFormsAllHaveFormBashers]
		class ZAddressDropEditColumnStyleTestForm : ZTestForm
		{
			public ZAddressDropEditColumnStyleTestForm(BusinessObject bizObj)
				: base(bizObj)
			{
			}

			public ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1;

			#region InitializeComponent

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.Grid.ColumnStyles.Clear();
				Grid.BindTo = "List";
				zAddressDropEditColumnStyleInfo1 = new ZAddressDropEditColumnStyleInfo();
				zAddressDropEditColumnStyleInfo1.BindToList = "LookupList";
				zAddressDropEditColumnStyleInfo1.Caption = "LOL";
				zAddressDropEditColumnStyleInfo1.ColumnName = "Z0_Guid";
				this.Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			}

			#endregion
		}

		#endregion
	}
}
