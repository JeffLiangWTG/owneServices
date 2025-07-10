using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGuidFindBoxTest2 : TestCaseWithFactory
	{
		public void TestAddFetchHint()
		{
			using (var findBox = new ZGuidFindBox())
			{
				var bizO = Factory.New<DummyDependantBusinessObject>();
				findBox.BindTo = "PK";
				findBox.BindToList = "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett";
				AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
				((IFetchHintGenerator)findBox).AddFetchHint(bizO, "");
				AssertEquals(1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			}
		}

		public void TestOnParseValue()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "Bob";
			dummy1.Z0_Description = "Bob Description (1)";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "Bob";
			dummy2.Z0_Description = "Bob Description (2)";

			Factory.Save();
			using (var findBox = new ZGuidFindBox())
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;

				var args = new ConvertEventArgs(null, typeof(String));
				findBox.Code = "Bob";
				findBox.SetGuid(dummy1.PK, "Bob");
				findBox.OnParseValueExposed(args);
				AssertEquals(dummy1.PK, args.Value);

				findBox.SetGuid(dummy2.PK, "Bob");
				findBox.OnParseValueExposed(args);
				AssertEquals(dummy2.PK, args.Value);
			}
		}

		public void TestGetDescription()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "Bob";
			dummy1.Z0_Description = "Bob Description (1)";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "Bob";
			dummy2.Z0_Description = "Bob Description (2)";

			Factory.Save();

			using (var form = new ZForm())
			using (var findBox = new ZGuidFindBox())
			{
				form.Controls.Add(findBox);
				form.Show();
				findBox.ModuleID = DummyModuleIDs.Dummy;
				findBox.List = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);

				findBox.CodeBox.Text = string.Empty;
				findBox.SetGuid(ZGuid.Empty, string.Empty);
				AssertEquals(Constants.FindBoxMessages.NoneSelected, findBox.GetDescriptionExposed());
				AssertEquals(Constants.FindBoxMessages.NoneSelected, findBox.DescriptionExposed);

				findBox.CodeBox.Text = "Bob";
				findBox.SetGuid(dummy1.PK, "Bob");
				AssertEquals(dummy1.Z0_Description, findBox.GetDescriptionExposed());
				AssertEquals(dummy1.Z0_Description, findBox.DescriptionExposed);

				//set to Bob of dummy2
				findBox.SetGuid(dummy2.PK, "Bob");
				findBox.Code = "Bob";
				AssertEquals(dummy2.Z0_Description, findBox.GetDescriptionExposed());
				AssertEquals(dummy2.Z0_Description, findBox.DescriptionExposed);

				findBox.CodeBox.Text = "XYZ";
				findBox.SetGuid(ZGuid.NewZGuid(), "XYZ");
				AssertEquals(Constants.FindBoxMessages.InvalidSelection, findBox.GetDescriptionExposed());
				AssertEquals(Constants.FindBoxMessages.InvalidSelection, findBox.DescriptionExposed);
			}
		}

		public void TestOnFormatValue()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "Bob";
			dummy1.Z0_Description = "Bob Description (1)";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "Bob";
			dummy2.Z0_Description = "Bob Description (2)";

			Factory.Save();

			using (var findBox = new ZGuidFindBox())
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				findBox.List = new DummyBusinessObjectCollection(Factory);

				var args = new ConvertEventArgs(dummy1.PK, typeof(String));
				findBox.OnFormatValueExposed(args);
				UserIdleWorker.Flush();
				AssertEquals("Bob", args.Value);
				AssertEquals("Bob Description (1)", findBox.DescriptionExposed);

				args = new ConvertEventArgs(dummy2.PK, typeof(String));
				findBox.OnFormatValueExposed(args);
				UserIdleWorker.Flush();
				AssertEquals("Bob", args.Value);
				AssertEquals("Bob Description (2)", findBox.DescriptionExposed);
			}
		}

		public void TestListProviderHasNonPersistentBizOSetMaxLenghtDoesNotThrow()
		{
			var collection = new DummyNonPersistentBusinessObjectCollection(Factory);
			var bizo = new DummyNonPersistentBusinessObject(collection);
			bizo.ChildDummyPK = new ZGuid();
			var bizo2 = new DummyNonPersistentBusinessObject(collection);
			bizo2.ChildDummyPK = new ZGuid();
			collection.Add(bizo);
			collection.Add(bizo2);

			using (var findBox = new ZGuidFindBox { BindTo = nameof(bizo.ChildDummyPK) })
			using (var codefindbox = new ZCodeFindBox { BindTo = nameof(bizo.ChildDummyPK) })
			using (var form = new ZForm(collection))
			{
				form.Controls.Add(findBox);
				form.Controls.Add(codefindbox);

				form.Show();
				Application.DoEvents();

				codefindbox.List = collection;
				AssertNoExceptionThrown("No exception thrown. NonPersistentBizo is handled", () => findBox.Visible = false);
			}
		}

		public void TestGetBizObjsToEditOrView_IgnoreCaseOfCode()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummyChild1 = dummy1.Collection.AddNew();
			dummyChild1.Z0_Code = "Bob";
			dummyChild1.Z0_Description = "dummyChild1";
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummyChild2 = dummy2.Collection.AddNew();
			dummyChild2.Z0_Code = "Bob";
			dummyChild2.Z0_Description = "dummyChild2";

			using (var findBox = new ZGuidFindBox())
			{
				findBox.List = dummy1.Collection;
				findBox.SetGuid(dummyChild1.PK, "Bob");
				findBox.Code = "bob";
				AssertContainsExactElementsInAnyOrder("Matches correct child even when input is lower case", x => x.Z0_Description,
					new[] { dummyChild1 },
					findBox.GetBizObjsToEditOrView().Cast<DummyChildBusinessObject>());
			}

			using (var findBox = new ZGuidFindBox())
			{
				findBox.List = dummy2.Collection;
				findBox.SetGuid(dummyChild2.PK, "Bob");
				findBox.Code = "BOB";
				AssertContainsExactElementsInAnyOrder("Matches correct child even when input is upper case", x => x.Z0_Description,
					new[] { dummyChild2 },
					findBox.GetBizObjsToEditOrView().Cast<DummyChildBusinessObject>());
			}
		}
	}
}
