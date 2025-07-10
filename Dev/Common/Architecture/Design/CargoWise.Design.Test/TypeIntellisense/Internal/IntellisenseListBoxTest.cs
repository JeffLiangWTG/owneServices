using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Design.TypeIntellisense.Testing
{
	public class IntellisenseListBoxTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1109:Form", Justification = "Required for testing only")]
		public void TestUpdateSelectedItem()
		{
			using (Form f = new Form())
			using (IntellisenseListBox listbox = new IntellisenseListBox())
			{
				f.Controls.Add(listbox);
				f.Show();
				Application.DoEvents();
				List<Type> types = new List<Type>();
				types.Add(new MyFakeType(typeof(object), "type1"));
				types.Add(new MyFakeType(typeof(object), "type2"));
				types.Add(new MyFakeType(typeof(object), "type3"));
				types.Add(new MyFakeType(typeof(object), "type4"));
				types.Add(new MyFakeType(typeof(object), "type5"));
				listbox.DataSource = new IntellisenseDataSource(types, new WindowsFormsSynchronizationContext());
				listbox.DataSource.PartialName = "type3";
				AssertEquals("Should select and highlight item 2", 2, listbox.SelectedIndex);
				AssertEquals("Should select and highlight item 2", true, listbox.GetSelected(2));
				listbox.DataSource.PartialName = "type3_splaty";
				AssertEquals("Should select and NOT highlight item 2", 2, listbox.SelectedIndex);
				AssertEquals("Should select and NOT highlight item 2", true, listbox.GetSelected(2));
				AssertEquals("Should select and NOT highlight item 2", true, listbox.OutlineSelectedItem);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1109:Form", Justification = "Required for testing only")]
		public void TestUpdateSelectedItemInLightOfNewlyAddedItem()
		{
			using (Form f = new Form())
			using (IntellisenseListBox listbox = new IntellisenseListBox())
			{
				f.Show();
				Application.DoEvents();
				f.Controls.Add(listbox);
				Application.DoEvents();
				List<Type> types = new List<Type>();
				types.Add(new MyFakeType(typeof(object), "type1"));
				types.Add(new MyFakeType(typeof(object), "type2"));
				types.Add(new MyFakeType(typeof(object), "type3"));
				types.Add(new MyFakeType(typeof(object), "type4"));
				types.Add(new MyFakeType(typeof(object), "type5"));
				listbox.DataSource = new IntellisenseDataSource(types, new WindowsFormsSynchronizationContext());
				listbox.DataSource.PartialNameAsync = "type5";
				AssertEquals("Should have nothing highlighted initially", -1, listbox.SelectedIndex);
				DateTime before = DateTime.Now;
				while (listbox.SelectedIndex != 4 && DateTime.Now.Subtract(before) < new TimeSpan(0, 0, 5))
				{
					Application.DoEvents();
				}

				AssertEquals("Should select and highlight item 4 after it has been populated", 4, listbox.SelectedIndex);
			}
		}

		#region Test Classes
		[Serializable]
		class MyFakeType : TypeNameHolder
		{
#if NETFRAMEWORK
			protected MyFakeType(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif

			public MyFakeType(Type typeForIsSubclassOf, string fullName) : base(fullName)
			{
				this.typeForIsSubclassOf = typeForIsSubclassOf;
			}

			public override bool IsSubclassOf(Type c)
			{
				return typeForIsSubclassOf.IsSubclassOf(c);
			}

			readonly Type typeForIsSubclassOf;
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			WindowsFormsSynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
		}
		#endregion
	}
}
