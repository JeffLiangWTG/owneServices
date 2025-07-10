using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class PanelLayoutTest : TestCaseWithFactory
	{
		public void TestInclude()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.Control1);
			AssertEquals(true, layout.ShouldInclude(BagForTest.Instance.Control1));
		}

		public void TestLayoutHasNoAssociatedControlBag()
		{
			AssertExceptionThrown<InvalidOperationException>("Unused Controls",
				"'Enterprise.ZArchitecture.GUI.PanelLayout' layout does not have a bag associated with control 'BagForTest.Control1'.",
				() => layout.Include(BagForTest.Instance.Control1));
		}

		public void TestControlNotFoundInBag()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			AssertExceptionThrown<InvalidOperationException>("Control not found",
				"Control 'IDoNotExist' is not part of Enterprise.ZArchitecture.GUI.Testing.BagForTest.",
				() => layout.Include(new ControlReference(BagForTest.Instance, "IDoNotExist")));
		}

		public void TestSetVisibility_DependenciesExist()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.Control1);
			layout.Include(BagForTest.Instance.Control2);
			layout.SetVisibility<DummyBusinessObject>(BagForTest.Instance.Control1, d => d.Z0_NVarChar == "A", d => d.Z0_NVarCharInfo);
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();

			CombineAssertions(() =>
			{
				var control1Dependencies = layout.GetVisibilityDependencies(BagForTest.Instance.Control1, dummyBusinessObject).ToArray();
				AssertEquals("Control 1 dependencies", 1, control1Dependencies.Length);
				AssertEquals("Dependecy PropteryInfo", dummyBusinessObject.Z0_NVarCharInfo, control1Dependencies[0]);

				var control2Dependencies = layout.GetVisibilityDependencies(BagForTest.Instance.Control2, dummyBusinessObject).ToArray();
				AssertEquals("Control 2 no dependencies", 0, control2Dependencies.Length);
			});
		}

		public void TestSetVisibility_DependenciesLogic()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.Control1);
			layout.SetVisibility<DummyBusinessObject>(BagForTest.Instance.Control1, d => d.Z0_NVarChar == "A", d => d.Z0_NVarCharInfo);

			var dummyBusinessObject = Factory.New<DummyBusinessObject>();
			CombineAssertions(() =>
			{
				dummyBusinessObject.Z0_NVarChar = "B";
				AssertEquals("not A", false, layout.IsVisible(BagForTest.Instance.Control1, dummyBusinessObject));

				dummyBusinessObject.Z0_NVarChar = "A";
				AssertEquals("is A", true, layout.IsVisible(BagForTest.Instance.Control1, dummyBusinessObject));

				AssertEquals("Default for Null BO", false, layout.IsVisible(BagForTest.Instance.Control1, null));
			});
		}

		public void TestSetVisibility_OnInvalidControl()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			AssertExceptionThrown<InvalidOperationException>("Control not found",
				"Control 'IDoNotExist' is not part of Enterprise.ZArchitecture.GUI.Testing.BagForTest.",
				() => layout.SetVisibility<DummyBusinessObject>(new ControlReference(BagForTest.Instance, "IDoNotExist"), d => d.Z0_NVarChar == "A", d => d.Z0_NVarCharInfo));
		}

		public void TestIsVisible_NullBusinessObject()
		{
			AssertEquals(true, layout.IsVisible(BagForTest.Instance.Control1, null));
		}

		public void TestSetCaption_DependenciesExist()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.Control1);
			layout.Include(BagForTest.Instance.Control2);
			layout.SetCaption<DummyBusinessObject>(BagForTest.Instance.Control1, d => Res.GetData("23C96C1E-A03C-47AC-BBED-146F455C6472", "Caption"), d => d.Z0_NVarCharInfo);
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();

			CombineAssertions(() =>
			{
				var control1Dependencies = layout.GetCaptionDependencies(BagForTest.Instance.Control1, dummyBusinessObject).ToArray();
				AssertEquals("Control 1 dependencies", 1, control1Dependencies.Length);
				AssertEquals("Dependecy PropteryInfo", dummyBusinessObject.Z0_NVarCharInfo, control1Dependencies[0]);

				var control2Dependencies = layout.GetCaptionDependencies(BagForTest.Instance.Control2, dummyBusinessObject).ToArray();
				AssertEquals("Control 2 no dependencies", 0, control2Dependencies.Length);
			});
		}

		public void TestSetCaption_DependenciesLogic()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.Control1);
			layout.SetCaption<DummyBusinessObject>(BagForTest.Instance.Control1, d => Res.GetData("28371BD4-25F7-4850-917C-7D0C0D1BCC6E", d.Z0_NVarChar), d => d.Z0_NVarCharInfo);

			var dummyBusinessObject = Factory.New<DummyBusinessObject>();
			CombineAssertions(() =>
			{
				dummyBusinessObject.Z0_NVarChar = "First Set";
				AssertEquals(true, layout.TryGetCaption(BagForTest.Instance.Control1, dummyBusinessObject, out var resourceStringData));
				AssertEquals("Set", "First Set", resourceStringData.Caption);

				dummyBusinessObject.Z0_NVarChar = "Updated Caption";
				AssertEquals(true, layout.TryGetCaption(BagForTest.Instance.Control1, dummyBusinessObject, out resourceStringData));
				AssertEquals("Updated", "Updated Caption", resourceStringData.Caption);
			});
		}

		public void TestSetCaption_OnInvalidControl()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			AssertExceptionThrown<InvalidOperationException>("Control not found",
				"Control 'IDoNotExist' is not part of Enterprise.ZArchitecture.GUI.Testing.BagForTest.",
				() => layout.SetCaption<DummyBusinessObject>(new ControlReference(BagForTest.Instance, "IDoNotExist"), d => Res.GetData("8C6A6103-EB71-48DA-804F-F53C899512FE", d.Z0_NVarChar), d => d.Z0_NVarCharInfo));
		}

		public void TestTryGetCaption_NullBusinessObject()
		{
			CombineAssertions(() =>
			{
				AssertEquals("TryGet failed", false, layout.TryGetCaption(BagForTest.Instance.Control1, null, out var resourceStringData));
				AssertNull("Empty", resourceStringData);
			});
		}

		public void TestSetCaptions_DependenciesExist()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.ComplexControl3);
			layout.Include(BagForTest.Instance.Control2);
			layout.SetCaptions<DummyBusinessObject>(BagForTest.Instance.ComplexControl3, d => new Dictionary<string, ResourceStringData>()
			{
				{ nameof(ComplexControl3), Res.GetData("ABC3", d.Z0_Description) },
				{ nameof(ComplexControl3.Child1), Res.GetData("ABC", d.Z0_NVarChar) },
				{ nameof(ComplexControl3.Child3), Res.GetData("ABC2", d.Z0_Code) }
			}, d => d.Z0_NVarCharInfo, d => d.Z0_CodeInfo, d => d.Z0_DescriptionInfo);
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();

			CombineAssertions(() =>
				{
					var control1Dependencies = layout.GetCaptionDependencies(BagForTest.Instance.ComplexControl3, dummyBusinessObject).ToArray();
					AssertEquals("Control 1 dependencies", 3, control1Dependencies.Length);
					AssertEquals("Dependecy Z0_NVarCharInfo", dummyBusinessObject.Z0_NVarCharInfo, control1Dependencies[0]);
					AssertEquals("Dependecy Z0_CodeInfo", dummyBusinessObject.Z0_CodeInfo, control1Dependencies[1]);
					AssertEquals("Dependecy Z0_DescriptionInfo", dummyBusinessObject.Z0_DescriptionInfo, control1Dependencies[2]);

					var control2Dependencies = layout.GetCaptionDependencies(BagForTest.Instance.Control2, dummyBusinessObject).ToArray();
					AssertEquals("Control 2 no dependencies", 0, control2Dependencies.Length);
				});
		}

		public void TestSetCaptions_DependenciesLogic()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.ComplexControl3);
			layout.SetCaptions<DummyBusinessObject>(BagForTest.Instance.ComplexControl3, d => new Dictionary<string, ResourceStringData>()
			{
				{ nameof(ComplexControl3), Res.GetData("ABC", d.Z0_Description) },
				{ nameof(ComplexControl3.Child2), Res.GetData("ABC2", d.Z0_NVarChar) },
				{ nameof(ComplexControl3.Child3), Res.GetData("ABC2", d.Z0_Code) }
			}, d => d.Z0_NVarCharInfo, d => d.Z0_CodeInfo, d => d.Z0_DescriptionInfo);

			var dummyBusinessObject = Factory.New<DummyBusinessObject>();
			CombineAssertions(() =>
			{
				dummyBusinessObject.Z0_Description = "HELLO WORLD";
				dummyBusinessObject.Z0_NVarChar = "First Set";
				dummyBusinessObject.Z0_Code = "ABC";
				AssertEquals(true, layout.TryGetCaptionData(BagForTest.Instance.ComplexControl3, dummyBusinessObject, out var captionData));
				AssertEquals("complex.Length", 3, captionData.Count);
				var child1 = captionData["ComplexControl3"];
				AssertEquals("child1.captionData.Caption", "HELLO WORLD", child1.Caption);
				var child2 = captionData["Child2"];
				AssertEquals("child2.captionData.Caption", "First Set", child2.Caption);
				var child3 = captionData["Child3"];
				AssertEquals("child3.captionData.Caption", "ABC", child3.Caption);

				dummyBusinessObject.Z0_NVarChar = "Updated Caption";
				AssertEquals(true, layout.TryGetCaptionData(BagForTest.Instance.ComplexControl3, dummyBusinessObject, out captionData));
				AssertEquals("complex.Length", 3, captionData.Count);
				child1 = captionData["ComplexControl3"];
				AssertEquals("child1.captionData.Caption", "HELLO WORLD", child1.Caption);
				child2 = captionData["Child2"];
				AssertEquals("child2.captionData.Caption", "Updated Caption", child2.Caption);
				child3 = captionData["Child3"];
				AssertEquals("child3.captionData.Caption", "ABC", child3.Caption);
			});
		}

		public void TestSetCaptions_OnInvalidControl()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			AssertExceptionThrown<InvalidOperationException>("Control not found",
				"Control 'IDoNotExist' is not part of Enterprise.ZArchitecture.GUI.Testing.BagForTest.",
				() => layout.SetCaptions<DummyBusinessObject>(new ControlReference(BagForTest.Instance, "IDoNotExist"), d => new Dictionary<string, ResourceStringData>() { { nameof(ComplexControl3.Child2), Res.GetData("ABC", d.Z0_NVarChar) } }, d => d.Z0_NVarCharInfo));
		}

		public void TestTryGetCaptions_NullBusinessObject()
		{
			CombineAssertions(() =>
			{
				AssertEquals("TryGet failed", false, layout.TryGetCaptionData(BagForTest.Instance.ComplexControl3, null, out var captionData));
				AssertEquals("Empty", false, captionData.Any());
			});
		}

		public void TestGetControlBehaviours()
		{
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.ComplexControl3);
			layout.Include(BagForTest.Instance.Control2);
			layout.SetBehaviours(BagForTest.Instance.Control2, new ControlBehaviourContainerCollection { new ControlBehaviourContainer<DummyBusinessObject>(new Control2Behaviour(), null) });

			var control2States = layout.GetControlBehaviours(BagForTest.Instance.Control2);
			AssertEquals("Control2 States Count", 1, control2States.Count);
			AssertEquals("Control2 Has Control2Behaviour Behaviour", true, layout.HasBehaviourByBehaviourType(BagForTest.Instance.Control2, typeof(Control2Behaviour)));

			var complexControl3States = layout.GetControlBehaviours(BagForTest.Instance.ComplexControl3);
			AssertEquals("Complex Control3 States Count", 0, complexControl3States.Count);
		}

		public void TestHasVisibilityBehaviour()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.Control1);
			layout.Include(BagForTest.Instance.Control2);
			layout.SetVisibility<DummyBusinessObject>(BagForTest.Instance.Control1, d => d.Z0_NVarChar == "A", d => d.Z0_NVarCharInfo);
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();

			CombineAssertions(() =>
			{
				AssertEquals("Visibility Behaviour for Control 1", true, layout.HasVisibilityBehaviour(BagForTest.Instance.Control1));
				AssertEquals("Visibility Behaviour for Control 2", false, layout.HasVisibilityBehaviour(BagForTest.Instance.Control2));
			});
		}

		public void TestGetControlBehaviourContainer()
		{
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.ComplexControl3);
			layout.Include(BagForTest.Instance.Control2);

			var controlBehaviourCollection = new ControlBehaviourContainerCollection
			{
				new ControlBehaviourContainer<DummyBusinessObject>(new Control2Behaviour(), null),
				new ControlBehaviourContainer<DummyBusinessObject>(new Control2BehaviourWithCustomName(), null)
			};

			layout.SetBehaviours(BagForTest.Instance.Control2, controlBehaviourCollection);

			CombineAssertions(() =>
			{
				var control2BehaviourContainer = layout.GetControlBehaviourContainer<Control2Behaviour>(BagForTest.Instance.Control2);
				AssertNotNull(control2BehaviourContainer);
				AssertType<Control2Behaviour>(control2BehaviourContainer.ControlBehaviour);

				var control2CustomBehaviourContainer = layout.GetControlBehaviourContainerByBehaviourName(BagForTest.Instance.Control2, "CustomName");
				AssertNotNull(control2CustomBehaviourContainer);
				AssertType<Control2BehaviourWithCustomName>(control2CustomBehaviourContainer.ControlBehaviour);
			});
		}

		public void TestGetControlBehaviour()
		{
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.ComplexControl3);
			layout.Include(BagForTest.Instance.Control2);
			var controlBehaviourCollection = new ControlBehaviourContainerCollection
			{
				new ControlBehaviourContainer<DummyBusinessObject>(new Control2Behaviour(), null),
				new ControlBehaviourContainer<DummyBusinessObject>(new Control2BehaviourWithCustomName(), null)
			};

			layout.SetBehaviours(BagForTest.Instance.Control2, controlBehaviourCollection);

			CombineAssertions(() =>
			{
				var behaviour1 = layout.GetControlBehaviour<Control2Behaviour>(BagForTest.Instance.Control2);
				AssertNotNull(behaviour1);

				var behaviour2 = layout.GetControlBehaviourByBehaviourName(BagForTest.Instance.Control2, "CustomName");
				AssertNotNull(behaviour2);
				AssertType<Control2BehaviourWithCustomName>(behaviour2);
			});
		}

		public void TestTryGetCaption_WithAndWithoutDependencies()
		{
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.Control2);
			layout.Include(BagForTest.Instance.ComplexControl3);
			layout.SetCaption<DummyBusinessObject>(BagForTest.Instance.Control2, _ => Res.GetData("85e14d4e-fc29-477f-907a-54f0e9558be1", "Test Caption"));
			layout.SetCaption<DummyBusinessObject>(BagForTest.Instance.ComplexControl3, _ => Res.GetData("30a3cc23-ef3b-4c4a-a832-33e611861a74", "Dependent Caption"), d => d.Z0_CalculatedInfo);

			CombineAssertions(() =>
			{
				layout.TryGetCaption(BagForTest.Instance.Control2, null, out var control2CaptionData);
				AssertEquals("Caption Control 2", "Test Caption", control2CaptionData?.Caption);

				layout.TryGetCaption(BagForTest.Instance.ComplexControl3, null, out var control3CaptionData);
				AssertNull("Caption with Dependency", control3CaptionData);

				layout.TryGetCaption(BagForTest.Instance.ComplexControl3, Factory.New<DummyBusinessObject>(), out control3CaptionData);
				AssertEquals("Caption Control 2", "Dependent Caption", control3CaptionData?.Caption);
			});
		}

		public void TestTryGetCaption_WithBusinessObjectHavingCaptionStrings()
		{
			layout.RegisterControlBag(BagForTest.Instance);
			layout.Include(BagForTest.Instance.Control2);
			layout.SetCaption<DummyBusinessObjectWithResourceStrings>(BagForTest.Instance.Control2, d => d?.Caption);

			CombineAssertions(() =>
			{
				var hasCaptionData = layout.TryGetCaption(BagForTest.Instance.Control2, null, out var resourceStringData);
				AssertEquals("HasCaptionData when BO is null", false, hasCaptionData);
				AssertNull("ResourceString when BO is null", resourceStringData);

				var dummyBusinessObject = Factory.New<DummyBusinessObjectWithResourceStrings>();
				hasCaptionData = layout.TryGetCaption(BagForTest.Instance.Control2, dummyBusinessObject, out resourceStringData);
				AssertEquals("HasCaptionData when BO is not null", true, hasCaptionData);
				AssertNotNull("ResourceString when BO is not null", resourceStringData);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			layout = new PanelLayout();
		}
		PanelLayout layout;
	}

	sealed class BagForTest : IControlBag
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is no way to change static backing field of this property.")]
		public static BagForTest Instance { get; } = new BagForTest();

		public BagForTest()
		{
			Control1 = new ControlReference(this, nameof(Control1));
			Control2 = new ControlReference(this, nameof(Control2));
			ComplexControl3 = new ControlReference(this, nameof(ComplexControl3));
		}

		public void CreateControls(Control container, IDictionary<ControlReference, Control> controlsByReference)
		{
			controlsByReference.Add(new ControlReference(this, nameof(Control1)), new Control { Name = nameof(Control1) });
			controlsByReference.Add(new ControlReference(this, nameof(Control2)), new Control { Name = nameof(Control2) });
			controlsByReference.Add(new ControlReference(this, nameof(ComplexControl3)), new ComplexControl3() { Name = nameof(ComplexControl3) });
		}

		public bool ContainsControl(string name) => Controls.Any(c => c.Name == name);

		public ControlWidthClass GetWidthClass(string name) => ControlWidthClass.Auto;

		public IReadOnlyCollection<ControlReference> Controls => new List<ControlReference> { Control1, Control2, ComplexControl3 };

		public ControlReference Control1 { get; }
		public ControlReference Control2 { get; }
		public ControlReference ComplexControl3 { get; }

		public Control TemplateControl => null;
	}

	sealed class ComplexControl3 : Control
	{
		public ComplexControl3()
		{
			Child1 = new Control() { Name = nameof(Child1) };
			Child2 = new Control() { Name = nameof(Child2) };
			Child3 = new Control() { Name = nameof(Child3) };
			Controls.Add(Child1);
			Controls.Add(Child2);
			Controls.Add(Child3);
		}

		internal Control Child1;
		internal Control Child2;
		internal Control Child3;
	}

	sealed class Control2Behaviour : ControlBehaviour
	{
		public override void UpdateBehaviour(Control control, object dataItem)
		{
			throw new NotImplementedException();
		}
	}

	sealed class Control2BehaviourWithCustomName : ControlBehaviourWithCustomName<Control, DummyBusinessObject>
	{
		protected override void UpdateBehaviourCore(Control control, DummyBusinessObject dataItem)
		{
			throw new NotImplementedException();
		}

		protected override string GetCustomBehaviourNameCore() => "CustomName";
	}

	sealed class DummyBusinessObjectWithResourceStrings : DummyBusinessObject
	{
		public DummyBusinessObjectWithResourceStrings(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		internal ResourceStringData Caption => Res.GetData("17fa8300-195e-41f4-960b-097d9301cbb3", "Test Caption");
	}
}
