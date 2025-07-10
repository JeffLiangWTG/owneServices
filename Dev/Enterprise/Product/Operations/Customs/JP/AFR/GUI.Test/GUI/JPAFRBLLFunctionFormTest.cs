using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	[TestedType(typeof(JPAFRBLLFunctionForm))]
	class JPAFRBLLFunctionFormTest : ZFormBasherTest
	{
		public void TestAddButton_Click()
		{
			foreach (var functioncode in new[]
			{
				BLLFunctionCode.RegisterSplit
				,BLLFunctionCode.RegisterSwitch
				, BLLFunctionCode.RegisterMerge
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				bllFunction.AvailableBills.Add(new BLLFunctionBill(header.Bills.AddNew("123")));
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var availableGrid = (ZGrid)form.Controls.Find("availableGrid", true).First();
					availableGrid.SelectSingleElement(bllFunction.AvailableBills[0]);
					var addButton = (ZButton)form.Controls.Find("addButton", true).First();
					addButton.PerformClick();

					AssertEquals(functioncode.ToString(), 1, bllFunction.SelectedBills.Count);
					AssertEquals(functioncode.ToString(), "123", bllFunction.SelectedBills[0].JPM_BillOfLadingNumber);
				}
			}

			foreach (var functioncode in new[]
			{
				BLLFunctionCode.CancelSplit
				, BLLFunctionCode.CancelSwitch
				, BLLFunctionCode.CancelMerge
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				bllFunction.AvailableBills.Add(new BLLFunctionBill(header.Bills.AddNew("123")));
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var availableGrid = (ZGrid)form.Controls.Find("availableGrid", true).First();
					availableGrid.SelectSingleElement(bllFunction.AvailableBills[0]);
					var addButton = (ZButton)form.Controls.Find("addButton", true).First();
					addButton.PerformClick();

					AssertEquals(functioncode.ToString(), 0, bllFunction.SelectedBills.Count);
				}
			}
		}

		public void TestAddButton_Enabled()
		{
			foreach (var functioncode in new[]
			{
				BLLFunctionCode.RegisterSplit
				, BLLFunctionCode.RegisterMerge
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var addButton = (ZButton)form.Controls.Find("addButton", true).First();
					AssertEquals(false, addButton.Enabled);
					form.BusinessEntity.AvailableBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(true, addButton.Enabled);
					for (var i = 0; i < 9; i++)
					{
						form.BusinessEntity.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
						AssertEquals(true, addButton.Enabled);
					}
					form.BusinessEntity.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(false, addButton.Enabled);
				}
			}

			foreach (var functioncode in new[]
			{
				BLLFunctionCode.RegisterSwitch
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var addButton = (ZButton)form.Controls.Find("addButton", true).First();
					AssertEquals(false, addButton.Enabled);
					form.BusinessEntity.AvailableBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(true, addButton.Enabled);
					form.BusinessEntity.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(false, addButton.Enabled);
				}
			}

			foreach (var functioncode in new[]
			{
				BLLFunctionCode.CancelSplit
				, BLLFunctionCode.CancelSwitch
				, BLLFunctionCode.CancelMerge
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var removeButton = (ZButton)form.Controls.Find("addButton", true).First();
					AssertEquals(false, removeButton.Enabled);
					form.BusinessEntity.AvailableBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(false, removeButton.Enabled);
				}
			}
		}

		public void TestRemoveButton_Click()
		{
			foreach (var functioncode in new[]
			{
				BLLFunctionCode.RegisterSplit
				, BLLFunctionCode.RegisterSwitch
				, BLLFunctionCode.RegisterMerge
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				bllFunction.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew("123")));
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var selectedGrid = (ZGrid)form.Controls.Find("selectedGrid", true).First();
					selectedGrid.SelectSingleElement(bllFunction.SelectedBills[0]);
					var removeButton = (ZButton)form.Controls.Find("removeButton", true).First();
					removeButton.PerformClick();

					AssertEquals(functioncode.ToString(), 1, bllFunction.AvailableBills.Count);
					AssertEquals(functioncode.ToString(), "123", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
				}
			}

			foreach (var functioncode in new[]
			{
				BLLFunctionCode.CancelSplit
				, BLLFunctionCode.CancelSwitch
				, BLLFunctionCode.CancelMerge
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				bllFunction.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew("123")));
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var selectedGrid = (ZGrid)form.Controls.Find("selectedGrid", true).First();
					selectedGrid.SelectSingleElement(bllFunction.SelectedBills[0]);
					var removeButton = (ZButton)form.Controls.Find("removeButton", true).First();
					removeButton.PerformClick();

					AssertEquals(functioncode.ToString(), 0, bllFunction.AvailableBills.Count);
				}
			}
		}

		public void TestRemoveButton_Enabled()
		{
			foreach (var functioncode in new[]
			{
				BLLFunctionCode.RegisterSplit
				,BLLFunctionCode.RegisterSwitch
				, BLLFunctionCode.RegisterMerge
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var removeButton = (ZButton)form.Controls.Find("removeButton", true).First();
					AssertEquals(false, removeButton.Enabled);
					form.BusinessEntity.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(true, removeButton.Enabled);
				}
			}

			foreach (var functioncode in new[]
			{
				BLLFunctionCode.CancelSplit
				, BLLFunctionCode.CancelSwitch
				, BLLFunctionCode.CancelMerge
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var removeButton = (ZButton)form.Controls.Find("removeButton", true).First();
					AssertEquals(false, removeButton.Enabled);
					form.BusinessEntity.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(false, removeButton.Enabled);
				}
			}
		}

		public void TestSendButton_Enabled()
		{
			foreach (var functioncode in new[]
			{
				BLLFunctionCode.RegisterSplit
				, BLLFunctionCode.RegisterMerge
				, BLLFunctionCode.CancelSplit
				, BLLFunctionCode.CancelMerge
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var sendButton = (ZButton)form.Controls.Find("SendButton", true).First();
					AssertEquals(false, sendButton.Enabled);
					form.BusinessEntity.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(false, sendButton.Enabled);
					for (var i = 0; i < 9; i++)
					{
						form.BusinessEntity.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
						AssertEquals(true, sendButton.Enabled);
					}
					form.BusinessEntity.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(false, sendButton.Enabled);
				}
			}

			foreach (var functioncode in new[]
			{
				BLLFunctionCode.RegisterSwitch,
				BLLFunctionCode.CancelSwitch
			})
			{
				var header = Factory.New<JPAFRHeader>();
				var bllFunction = BLLFunction.New(header, functioncode);
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					var sendButton = (ZButton)form.Controls.Find("SendButton", true).First();
					AssertEquals(false, sendButton.Enabled);
					form.BusinessEntity.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(true, sendButton.Enabled);
					form.BusinessEntity.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
					AssertEquals(false, sendButton.Enabled);
				}
			}
		}

		public void TestJPM_BillOfLadingNumberCaption()
		{
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSplit)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Register Split", form.FormCaption);
				var billNumberDropEdit = (ZDropEdit)form.Controls.Find("billNumberDropEdit", true).First();
				AssertEquals("Original Bill No.", billNumberDropEdit.CaptionResourceString.Caption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSwitch)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Register Switch", form.FormCaption);
				var billNumberDropEdit = (ZDropEdit)form.Controls.Find("billNumberDropEdit", true).First();
				AssertEquals("Original Bill No.", billNumberDropEdit.CaptionResourceString.Caption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterMerge)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Register Merge", form.FormCaption);
				var billNumberDropEdit = (ZDropEdit)form.Controls.Find("billNumberDropEdit", true).First();
				AssertEquals("New Bill No.", billNumberDropEdit.CaptionResourceString.Caption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelSplit)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Cancel Split", form.FormCaption);
				var billNumberDropEdit = (ZDropEdit)form.Controls.Find("billNumberDropEdit", true).First();
				AssertEquals("Original Bill No.", billNumberDropEdit.CaptionResourceString.Caption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelSwitch)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Cancel Switch", form.FormCaption);
				var billNumberDropEdit = (ZDropEdit)form.Controls.Find("billNumberDropEdit", true).First();
				AssertEquals("Original Bill No.", billNumberDropEdit.CaptionResourceString.Caption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelMerge)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Cancel Merge", form.FormCaption);
				var billNumberDropEdit = (ZDropEdit)form.Controls.Find("billNumberDropEdit", true).First();
				AssertEquals("New Bill No.", billNumberDropEdit.CaptionResourceString.Caption);
			}
		}

		public void TestSelectedGridCaption()
		{
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSplit)))
			{
				form.Show();
				var selectedGroupBox = (ZGroupBox)form.Controls.Find("selectedGroupBox", true).First();
				AssertEquals("Split Bills", selectedGroupBox.CaptionResourceString.Caption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSwitch)))
			{
				form.Show();
				var selectedGroupBox = (ZGroupBox)form.Controls.Find("selectedGroupBox", true).First();
				AssertEquals("New Bill", selectedGroupBox.CaptionResourceString.Caption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterMerge)))
			{
				form.Show();
				var selectedGroupBox = (ZGroupBox)form.Controls.Find("selectedGroupBox", true).First();
				AssertEquals("Merge Bills", selectedGroupBox.CaptionResourceString.Caption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelSplit)))
			{
				form.Show();
				var selectedGroupBox = (ZGroupBox)form.Controls.Find("selectedGroupBox", true).First();
				AssertEquals("Split Bills", selectedGroupBox.CaptionResourceString.Caption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelSwitch)))
			{
				form.Show();
				var selectedGroupBox = (ZGroupBox)form.Controls.Find("selectedGroupBox", true).First();
				AssertEquals("New Bill", selectedGroupBox.CaptionResourceString.Caption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelMerge)))
			{
				form.Show();
				var selectedGroupBox = (ZGroupBox)form.Controls.Find("selectedGroupBox", true).First();
				AssertEquals("Merge Bills", selectedGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestFormCaption()
		{
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSplit)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Register Split", form.FormCaption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSwitch)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Register Switch", form.FormCaption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterMerge)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Register Merge", form.FormCaption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelSplit)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Cancel Split", form.FormCaption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelSwitch)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Cancel Switch", form.FormCaption);
			}
			using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelMerge)))
			{
				form.Show();
				AssertEquals("Bill Manipulation (BLL) - Cancel Merge", form.FormCaption);
			}
		}

		public void TestBusinessEntity()
		{
			foreach (var functioncode in new[]
			{
				BLLFunctionCode.RegisterSplit
				,BLLFunctionCode.RegisterSwitch
				, BLLFunctionCode.RegisterMerge
				, BLLFunctionCode.CancelSplit
				, BLLFunctionCode.CancelSwitch
				, BLLFunctionCode.CancelMerge
			})
			{
				var bllFunction = BLLFunction.New(Factory.New<JPAFRHeader>(), functioncode);
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					AssertEquals(bllFunction, form.BusinessEntity);
				}
			}
		}

		public void TestFormVerb()
		{
			foreach (var functioncode in new[]
			{
				BLLFunctionCode.RegisterSplit
				,BLLFunctionCode.RegisterSwitch
				, BLLFunctionCode.RegisterMerge
				, BLLFunctionCode.CancelSplit
				, BLLFunctionCode.CancelSwitch
				, BLLFunctionCode.CancelMerge
			})
			{
				var bllFunction = BLLFunction.New(Factory.New<JPAFRHeader>(), functioncode);
				using (var form = new JPAFRBLLFunctionForm(bllFunction))
				{
					form.Show();
					AssertEquals("", form.FormVerb);
				}
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore() => new JPAFRBLLFunctionForm(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSplit));
	}
}
