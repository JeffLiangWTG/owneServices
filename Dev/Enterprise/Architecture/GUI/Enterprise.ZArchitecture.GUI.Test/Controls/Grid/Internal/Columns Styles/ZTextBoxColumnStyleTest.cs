using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Res = Enterprise.ZArchitecture.GUI.Res;

#pragma warning disable CW1108 // Do Not Use DataSet

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZTextBoxColumnStyleTest : TransactionedTestCase
	{
		public void TestCommitInvalidShort()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			{
				using (var grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) })
				{
					form.Controls.Add(grid);
					var info2 = new ZCheckBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code };
					grid.ColumnStyles.Add(info2);
					dummy.Collection.AddNew();
					grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);

					using (var col = new ZTextBoxColumnStyleForTest(info2))
					{
						col.TextBox.Text = "51623996 lfd 07/23";
						col.PropertyDescriptor = TypeDescriptor.GetProperties(typeof(ZShort))[0];
						((DataGridTextBox)col.TextBox).IsInEditOrNavigateMode = false;
						Assert(grid.ListManager.Position == 0);
						Assert("Commit should return false.", !col.Commit_DebugAccess(grid.ListManager, 0));
					}
				}
			}
		}

		public void TestCommitReadOnly()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			var item = dummy.Collection.AddNew();

			using (var form = new ZForm(dummy))
			{
				using (var grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) })
				{
					form.Controls.Add(grid);
					var info = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax };
					grid.ColumnStyles.Add(info);
					grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);

					using (var style = new ZTextBoxColumnStyle(info))
					{
						style.PropertyDescriptor = TypeDescriptor
							.GetProperties(typeof(DummyChildBusinessObject))
							.OfType<PropertyDescriptor>()
							.Single(descriptor => descriptor.DisplayName == DummyBizoSchema.Constants.Z0_VarCharMax);

						style.TextBox.Text = "This is SPARTA!";
						((DataGridTextBox)style.TextBox).IsInEditOrNavigateMode = false;
						AssertEquals("Expecting success commit when cell is editable", true, style.Commit_DebugAccess(grid.ListManager, 0));
						AssertEquals("Expecting a value update to editable cell", "This is SPARTA!", item.Z0_VarCharMax);

						style.isCurrentCellReadOnlyForTest = true;
						style.TextBox.Text = "You are not SPARTAN!";
						((DataGridTextBox)style.TextBox).IsInEditOrNavigateMode = false;
						AssertEquals("Expecting success commit when cell is read-only", true, style.Commit_DebugAccess(grid.ListManager, 0));
						AssertEquals("Expecting NO value update to read-only cell", "This is SPARTA!", item.Z0_VarCharMax);
					}
				}
			}
		}

#if !WINZOR

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestStartWithNewLine()
		{
			var textToDisplay = string.Empty;
			using (var testForm = new ZForm())
			{
				var data = new DataSet("TestData");
				var testTable = new DataTable("GridTable");
				data.Tables.Add(testTable);
				testTable.Columns.Add("TS_TestField", typeof(string));
				testTable.Rows.Add(new object[] { "\r\n\r\nhank" });

				var testGrid = new ZGrid { DataSource = data, DataMember = "GridTable" };
				var tableStyle = new DataGridTableStyle { MappingName = "GridTable" };
				var columnInfo = new ZTextBoxColumnStyleInfo("TS_TestField", 50) { Caption = "Test Field" };
				testGrid.Columns.Add(columnInfo);

				testForm.Controls.Add(testGrid);
				testGrid.SetDataBinding(data, "GridTable");
				using (var style = testGrid.Columns[0].ColumnStyle as ZTextBoxColumnStyle)
				{
					try
					{
						style.PaintHighlights += Style_PaintHighlights;

						testForm.Show();
						Application.DoEvents();

						AssertEquals("HANK", textToDisplay);
					}
					finally
					{
						style.PaintHighlights -= Style_PaintHighlights;
						style.Dispose();
					}
				}
			}

			void Style_PaintHighlights(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, StringFormat format, Font cellFont, Brush backBrush, Brush foreBrush, bool rightToLeft, bool useEllipsis)
			{
				textToDisplay = cellText;
			}
		}

#endif

		#region TestBGColourEmpty

#if !WINZOR

		public void TestBGColourEmpty()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			{
				using (var grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) })
				{
					form.Controls.Add(grid);
					var info2 = new ZCheckBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code };
					grid.ColumnStyles.Add(info2);
					dummy.Collection.AddNew();
					dummy.Collection.AddNew();
					grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);

					var readOnlyColor = SystemDataRegistry.Instance.ColorTheme.GridReadOnlyColor;
					using (var col = new ZTextBoxColumnStyleForTest(info2))
					{
						col.SetParentGrid(grid);
						col.Paint(grid.CreateGraphics(), new Rectangle(0, 0, 2000, 2000), grid.ListManager, 2, grid.ReadOnlyBrushFromRowNum(0), grid.ReadOnlyBrushFromRowNum(0), false);
						AssertEquals(readOnlyColor, ((SolidBrush)col.backBrush).Color);

						grid.ReadOnly = true;
						col.Paint(grid.CreateGraphics(), new Rectangle(0, 0, 2000, 2000), grid.ListManager, 0, grid.ReadOnlyBrushFromRowNum(0), grid.ReadOnlyBrushFromRowNum(0), false);
						AssertEquals(readOnlyColor, ((SolidBrush)col.backBrush).Color);

						col.ReadOnly = true;
						grid.ColourDeciding += Grid_ColourDeciding;

						col.Paint(grid.CreateGraphics(), new Rectangle(0, 0, 2000, 2000), grid.ListManager, 0, grid.ReadOnlyBrushFromRowNum(0), grid.ReadOnlyBrushFromRowNum(0), false);
						AssertEquals(Color.Azure, ((SolidBrush)col.backBrush).Color);

						grid.ReadOnly = false;
						col.ReadOnly = false;
						col.Paint(grid.CreateGraphics(), new Rectangle(0, 0, 2000, 2000), grid.ListManager, 0, grid.ReadOnlyBrushFromRowNum(0), grid.ReadOnlyBrushFromRowNum(0), false);
						AssertEquals(Color.LightGreen, ((SolidBrush)col.backBrush).Color);

						grid.ColourDeciding -= Grid_ColourDeciding;
						grid.ReadOnly = false;
						col.ReadOnly = false;
						col.Paint(grid.CreateGraphics(), new Rectangle(0, 0, 2000, 2000), grid.ListManager, 0, BrushProvider.FromColor(grid.GetCustomRowBackgroundColour(0)), grid.ReadOnlyBrushFromRowNum(0), false);
						AssertEquals(Color.FromArgb(0, 0, 0, 0), ((SolidBrush)col.backBrush).Color);
					}
				}
			}
		}

		void Grid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			e.Colour = Color.LightGreen;
			e.ReadOnlyColour = Color.Azure;
		}

#endif

		class ZTextBoxColumnStyleForTest : ZTextBoxColumnStyle
		{
			public ZTextBoxColumnStyleForTest(ZCheckBoxColumnStyleInfo info)
				: base(info)
			{
				GetColumnValueAtRowOverride = "";
			}

#if !WINZOR

			public new void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignedToRight)
			{
				base.Paint(g, bounds, source, paintingRowNum, backBrush, foreBrush, alignedToRight);
			}

			protected internal override void PaintText(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, Font cellFont, Brush backBrush, Brush foreBrush, bool rightToLeft)
			{
				this.backBrush = backBrush;
			}
			public Brush backBrush;

#endif
		}

		#endregion

		#region TestPaintWithExceptionThrown

#if !WINZOR

		public void TestPaintWithExceptionThrown()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			{
				using (var grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) })
				{
					form.Controls.Add(grid);
					var info2 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description };
					grid.ColumnStyles.Add(info2);
					dummy.Collection.AddNew();
					grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);

					using (var col = new ZTextBoxColumnStyleForTestPaint(info2))
					{
						col.SetParentGrid(grid);
						AssertNoExceptionThrown(() => col.Paint(grid.CreateGraphics(), new Rectangle(0, 0, 2000, 2000), grid.ListManager, 0, grid.ReadOnlyBrushFromRowNum(0), grid.ReadOnlyBrushFromRowNum(0), false));
						AssertEquals("Exception occurred while getting the cell text: Test Exception. Column name: Z0_Description, row number: 0.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Test Exception", ErrorReporter.LastExceptionReported.Message);
						AssertEquals("Cell with exception should show error info.","*ERROR*", col.cellText);
						ErrorReporter.Clear();
					}
				}
			}
		}

#endif

		class ZTextBoxColumnStyleForTestPaint : ZTextBoxColumnStyle
		{
			public ZTextBoxColumnStyleForTestPaint(ZTextBoxColumnStyleInfo info)
				: base(info)
			{
			}

#if !WINZOR

			public new void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignedToRight)
			{
				base.Paint(g, bounds, source, paintingRowNum, backBrush, foreBrush, alignedToRight);
			}

			protected internal override void PaintText(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, Font cellFont, Brush backBrush, Brush foreBrush, bool rightToLeft)
			{
				this.cellText = cellText;
			}
			public string cellText;

			protected override void OnGettingColumnTextAtRow(CurrencyManager source, int rowNum)
			{
				throw new ArgumentException("Test Exception");
			}
#endif
		}

		#endregion

		public void TestNullText()
		{
			AssertEquals("Null text should be blank", "", TestGridEdit.NullText);
		}

		[ExpectNoExceptions]
		public void TestGenericGDIError()
		{
			//A Generic GDI+ error can occur when running through terminal services. This is a .Net bug and can be reproduced
			//outside of Enterprise. It is reported at http://lab.msdn.microsoft.com/productfeedback/viewfeedback.aspx?feedbackid=7566bf8c-82f1-497e-8c75-2ef86311a208
			//
			//This occurs if the length of the text in the column exceeds 192 characters. The fix appears to be to tell the
			//drawing code to use the ellipsis character to truncate text, or to clip or wrap the text.
			//
			//This test fails on terminal services when not using the ellipsis character, though it may not fail locally.

			using (var testForm = new Form())
			{
				testForm.Width = 1024;
				testForm.Height = 768;

				var data = new DataSet("TestData"); // for testing only
				var testTable = new DataTable("GridTable");
				data.Tables.Add(testTable);
				testTable.Columns.Add("TS_TestField", typeof(string));
				testTable.Rows.Add(new object[] { "SomeText".PadRight(193, 'x') });

				var testGrid = new ZGrid { Width = 580, Height = 288, DataSource = data, DataMember = "GridTable" };
				var tableStyle = new DataGridTableStyle { MappingName = "GridTable" };
				var columnInfo = new ZTextBoxColumnStyleInfo("TS_TestField", 50) { Caption = "Test Field" };
				testGrid.Columns.Add(columnInfo);

				testForm.Controls.Add(testGrid);
				testGrid.SetDataBinding(data, "GridTable");
				testForm.Show();
				Application.DoEvents();

				AssertNotNull(testGrid.ListManager);
				AssertEquals(1, testGrid.ListManager.List.Count);
			}
		}

		[ExpectNoExceptions]
		public void TestCommitShouldNotBlowUpWhenBindingContextIsRemoved()
		{
			var testDataSet = new DataSet(); // for testing only
			var testTable = new DataTable("TestTable");
			var testDataColumn = new DataColumn("TestColumn");
			testTable.Columns.Add(testDataColumn);
			testDataSet.Tables.Add(testTable);

			using (var testForm = new Form())
			{
				var testGrid = new ZGrid();
				testGrid.Columns.AddTextColumn("TestColumn", 100, true, true, false);
				var tabControl = new ZTabControl();
				var tabPageWithGrid = new ZTabPage(); // will be deleted tomorrow
				tabPageWithGrid.Controls.Add(testGrid);
				tabControl.TabPages.Add(tabPageWithGrid);
				testForm.Controls.Add(tabControl);
				testGrid.SetDataBinding(testDataSet, "TestTable");

				testForm.Show();
				testGrid.BeginEdit(testGrid.Columns[0].ColumnStyle, 0);
				var newRowView = (DataRowView)testGrid.List.AddNew();
				newRowView[0] = "meh meh";
				tabControl.TabPages.Remove(tabPageWithGrid);

				tabPageWithGrid.Dispose();
			}
		}

		public void TestFormatInfo()
		{
			AssertEquals("Should have Current Company format culture", Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture, TestGridEdit.FormatInfo);
		}

		public void TestIsSensitiveValue()
		{
			var textColInfo1 = new ZTextBoxColumnStyleInfo();
			AssertEquals(false, textColInfo1.IsSensitiveValue);

			var textColInfo2 = new ZTextBoxColumnStyleInfo();
			textColInfo2.PasswordChar = '*';
			AssertEquals(true, textColInfo2.IsSensitiveValue);
		}

		#region TestEmailAddress

		public void TestControlEInsertsEmailAddress()
		{
			SetCurrentUsersEmail("blah@blah.org");

			var dummy = new DummyWithEmailChildren();
			var child = dummy.Collection.AddNew();
			child.EmailProperty = "";
			AssertEquals("Email property should have validation error", true, child.EmailPropertyInfo.HasErrors());
			AssertEquals("Please enter an Email Address.", child.EmailPropertyInfo.Notifications.GetErrors().GetFirstMessage());

			var hotkeyHandled = CreateGridWithColumnAndPressHotkey(dummy, "EmailProperty", Keys.Control | Keys.E, false);
			Assert("Should state the key was handled", hotkeyHandled);
			AssertEquals("Should set the email properties value to the current users email address", "blah@blah.org", child.EmailProperty);

			AssertEquals("Email property should still have validation error before current cell of grid changed", true, child.EmailPropertyInfo.HasErrors());
			AssertEquals("Please enter an Email Address.", child.EmailPropertyInfo.Notifications.GetErrors().GetFirstMessage());

			CreateGridWithColumnAndPressHotkey(dummy, "EmailProperty", Keys.Control | Keys.E);
			AssertEquals("Email property should not have validation error after current cell of grid changed", false, child.EmailPropertyInfo.HasErrors());
		}

		public void TestControlEInsertsEmailAddress_OverridesExisting()
		{
			SetCurrentUsersEmail("blah@blah.org");

			var dummy = new DummyWithEmailChildren();
			var child = dummy.Collection.AddNew();
			child.EmailProperty = "Someone@elses.email";

			var hotkeyHandled = CreateGridWithColumnAndPressHotkey(dummy, "EmailProperty", Keys.Control | Keys.E);
			Assert("Should state the key was handled", hotkeyHandled);
			AssertEquals("Should set the email properties value to the current users email address", "blah@blah.org", child.EmailProperty);
		}

		public void TestControlEInsertsEmailAddress_CurrentUserIsBlank()
		{
			SetCurrentUsersEmail("");

			var dummy = new DummyWithEmailChildren();
			var child = dummy.Collection.AddNew();
			child.EmailProperty = "Someone@elses.email";

			var hotkeyHandled = CreateGridWithColumnAndPressHotkey(dummy, "EmailProperty", Keys.Control | Keys.E);
			AssertEquals("Should have left it alone", "Someone@elses.email", child.EmailProperty);
			AssertEquals("Should warn the user their email is blank", "Your email address has not been set", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestControlEInsertsEmailAddress_NotAnEmailColumn()
		{
			SetCurrentUsersEmail("");

			var dummy = new DummyWithEmailChildren();
			var child = dummy.Collection.AddNew();
			child.EmailProperty = "Someone@elses.email";

			var hotkeyHandled = CreateGridWithColumnAndPressHotkey(dummy, "NotEmailProperty", Keys.Control | Keys.E);
			Assert("Hotkey shouldnt have been handled", !hotkeyHandled);
			AssertEquals("Should have left it alone", "Someone@elses.email", child.EmailProperty);
		}

		bool CreateGridWithColumnAndPressHotkey(BusinessObject bizo, string columnName, Keys hotkey, bool isCurrentCellChanged = true)
		{
			using (var form = new ZForm(bizo))
			using (var grid = new ZGrid())
			{
				grid.Columns.Add(new ZTextBoxColumnStyleInfo(columnName, 120));

				grid.BindTo = "Collection";
				form.Controls.Add(grid);

				form.Show();

				var column = (ZTextBoxColumnStyle)grid.Columns[columnName].ColumnStyle;
				column.CharacterCasing = CharacterCasing.Normal;

				grid.BeginEdit(column, 0);

				var someMessage = new Message();
				var result = column.ProcessCmdKey(ref someMessage, Keys.Control | Keys.E);

				if (isCurrentCellChanged)
				{
					grid.BeginEdit(column, 1);
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
		void SetCurrentUsersEmail(string email)
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff["GS_EmailAddress"] = email;
			factory.Save();

			Env.SetUserContext(new UserContext((IUser)staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK));
		}

		class DummyChildWithEmail : NonPersistentBusinessObject
		{
			[EmailAddress]
			public ZString EmailProperty
			{
				get { return emailProperty; }
				set
				{
					SetNonPersistentPropertyValue(EmailPropertyInfo, ref emailProperty, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateEmailProperty();
					}
				}
			}

			ZString emailProperty;
			public ZPropertyInfo EmailPropertyInfo => GetZPropertyInfo(nameof(EmailProperty));

			public ZString NotEmailProperty
			{
				get { return notEmailProperty; }
				set { SetNonPersistentPropertyValue(NotEmailPropertyInfo, ref notEmailProperty, value); }
			}

			ZString notEmailProperty;
			ZPropertyInfo NotEmailPropertyInfo => GetZPropertyInfo(nameof(NotEmailProperty));

			public DummyChildWithEmailValidation Validation
			{
				get { return new DummyChildWithEmailValidation(this); }
			}
		}

		class DummyChildWithEmailValidation : ZValidation
		{
			public DummyChildWithEmailValidation(DummyChildWithEmail parent)
			: base(parent)
			{
				this.parent = parent;
			}

			readonly DummyChildWithEmail parent;

			public void ValidateEmailProperty()
			{
				ValidateCalculatedProperty(parent.EmailPropertyInfo);
			}

			protected void CheckEmailProperty()
			{
				MandatoryValidation.CheckEntered(parent.EmailPropertyInfo, "Email Address");
			}

			#region ZValidation Members

			public override Type AutoValidationType
			{
				get
				{
					return typeof(DummyChildWithEmailValidation);
				}
			}

			public override void ValidateAll()
			{
				ValidateEmailProperty();
			}

			#endregion
		}

		class DummyChildWithEmailCollection : NonPersistentBusinessObjectCollection<DummyChildWithEmail>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyChildWithEmail();
			}
		}

		class DummyWithEmailChildren : NonPersistentBusinessObject
		{
			public DummyChildWithEmailCollection Collection { get; } = new DummyChildWithEmailCollection();
		}

		#endregion

		#region TestCommitMultilingualString

		public void TestCommitMultilingualString()
		{
			using (Res.UseMockData())
			{
				var dummy = new DummyWithCollectionWithMultilingualStringField();
				dummy.Collection.AddNew();
				dummy.Collection[0].Description = (NoResString)"meow";
				dummy.Collection.AddNew();
				using (var testForm = new MultilingualStringFieldTestForm(dummy))
				{
					testForm.Show();
					testForm.grid.BeginEdit(testForm.grid.Columns[0].ColumnStyle, 0);
					AssertEquals("meow", ((ZTextBoxColumnStyle)testForm.grid.Columns[0].ColumnStyle).TextBox.Text);
					((ZTextBoxColumnStyle)testForm.grid.Columns[0].ColumnStyle).TextBox.Text = "woof woof";
					((DataGridTextBox)((ZTextBoxColumnStyle)testForm.grid.Columns[0].ColumnStyle).TextBox).IsInEditOrNavigateMode = false;
					testForm.grid.BeginEdit(testForm.grid.Columns[0].ColumnStyle, 1);
					AssertEquals("woof woof", dummy.Collection[0].Description);
				}
			}
		}

		class MultilingualStringFieldTestForm : ZForm
		{
			public MultilingualStringFieldTestForm(BusinessObject businessObject)
				: base(businessObject)
			{
				grid = new ZGrid();
				this.BindingSource.SetBindingMember(grid, "Collection");
				grid.Columns.AddTextColumn("Description", 100, true, true, false, CharacterCasing.Normal);
				this.Controls.Add(grid);
			}

			public ZGrid grid;
		}

		class DummyWithMultilingualStringField : NonPersistentBusinessObject, IObsoleteValidation
		{
			public MultilingualString Description
			{
				get { return description ?? (NoResString)""; }
				set
				{
					if (value == null)
					{
						value = (NoResString)"";
					}
					SetNonPersistentPropertyValue(DescriptionInfo, ref description, value, false);
				}
			}

			MultilingualString description;

			public ZPropertyInfo DescriptionInfo
			{
				get { return GetZPropertyInfo(nameof(Description)); }
			}
		}

		class DummyWithMultilingualStringFieldCollection : NonPersistentBusinessObjectCollection<DummyWithMultilingualStringField>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyWithMultilingualStringField();
			}
		}

		class DummyWithCollectionWithMultilingualStringField : NonPersistentBusinessObject, IObsoleteValidation
		{
			public DummyWithMultilingualStringFieldCollection Collection
			{
				get
				{
					if (collection == null)
					{
						collection = new DummyWithMultilingualStringFieldCollection();
						RegisterEditableChildObject(collection);
					}
					return collection;
				}
			}
			DummyWithMultilingualStringFieldCollection collection;
		}

		#endregion

		#region Overridable PropertyDescriptor

		public void TestOverridablePropertyDescriptor()
		{
			var columnInfo = new ZTextBoxColumnStyleInfo("abc", 100);
			((IOverridablePropertyDescriptor)columnInfo).PropertyDescriptor = new ConstantValuePropertyDescriptor(null, "abc", typeof(object), "xyz");

			using (var columnStyle = new ZTextBoxColumnStyle(columnInfo))
			{
				AssertEquals(typeof(ConstantValuePropertyDescriptor), columnStyle.PropertyDescriptor.GetType());
			}
		}

		public void TestIsCellReadOnlyCore()
		{
			var columnInfo = new ZTextBoxColumnStyleInfo("abc", 100);
			((IOverridablePropertyDescriptor)columnInfo).PropertyDescriptor = new PropertyDescriptorForReadonlyTest("abc");

			using (var columnStyle = new ZTextBoxColumnStyleReadonlyTest(columnInfo))
			{
				Assert(!columnStyle.IsCellReadOnlyCoreExposed("abc"));
				Assert(columnStyle.IsCellReadOnlyCoreExposed("readonly"));
			}
		}

		class ZTextBoxColumnStyleReadonlyTest : ZTextBoxColumnStyle
		{
			public ZTextBoxColumnStyleReadonlyTest(ZTextBoxColumnStyleInfo columnInfo) : base(columnInfo) { }

			public bool IsCellReadOnlyCoreExposed(object component)
			{
				return IsCellReadOnlyCore(component);
			}
		}

		class PropertyDescriptorForReadonlyTest : KPropertyDescriptor, IPropertyReadonlyOnComponentRetriver
		{
			public PropertyDescriptorForReadonlyTest(string name) : base(null, name, null) { }

			public bool IsReadOnlyOnComponent(object component)
			{
				return component.ToString() == "readonly";
			}
		}

		#endregion

		#region Implementation

		ZTextBoxColumnStyle TestGridEdit;

		protected override void SetUp()
		{
			base.SetUp();
			TestGridEdit = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo("Test", 0));
		}

		protected override void TearDown()
		{
			TestGridEdit.Dispose();
			base.TearDown();
		}

		#endregion
	}

	sealed class ZTextBoxColumnStyleNotificationInGridTest : NotificationInGridTestCase
	{
#if !WINZOR

		[ExpectNoExceptions]
		public void TestGenericGDIError()
		{
			// if the text is greater than ushort.MaxValue then it throws an ExternalException without the fix inside PaintText.
			using (var testForm = new Form())
			{
				using (var g = testForm.CreateGraphics())
				{
					var s = new string('a', ushort.MaxValue + 1);
					var bounds = new Rectangle(0, 0, 79, 14);

					var columnInfo = new ZTextBoxColumnStyleInfo();
					using (var column = new ZTextBoxColumnStyle(columnInfo))
					{
						column.PaintText(g, bounds, null, -1, s, testForm.Font, SystemBrushes.Control, SystemBrushes.ControlText, false, true);
					}
				}
			}
		}

		public void TestPadRightIfRightAligned()
		{
			form.Show();
			var style = (ZTextBoxColumnStyle)form.Grid.TableStyles[0].GridColumnStyles[0];

			using (style.UsePadRightForTesting())
			{
				style.Alignment = HorizontalAlignment.Right;
				Assert(style.HeaderText.EndsWith("" + (char)32 + (char)31));

				style.Alignment = HorizontalAlignment.Center;
				Assert(!style.HeaderText.EndsWith("" + (char)32 + (char)31));
			}
		}

#endif

		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			CheckSizeAndLocation(50, new Point(37, 21), new Size(47, 13));

			Dummy.Collection[0].Z0_DescriptionInfo.AddError("baad");
			CheckSizeAndLocation(50, new Point(49, 21), new Size(35, 13));
		}

		protected override string GetColumnName()
		{
			return CargoWise.EntityFramework.Testing.AutoDummyBizo.Schema.Z0_Description;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			return new ZTextBoxColumnStyleInfo();
		}
	}
}
