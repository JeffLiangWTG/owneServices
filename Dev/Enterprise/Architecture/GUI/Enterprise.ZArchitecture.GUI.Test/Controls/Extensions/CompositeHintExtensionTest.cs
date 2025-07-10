using System;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class CompositeHintExtensionTest : BaseExtensionTest<CompositeHintExtension>
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestRequiresChildren()
		{
			CompositeHintExtension.Create(null);
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestRequiresAtLeastOneChild()
		{
			CompositeHintExtension.Create(Array.Empty<IExtendedControl>());
		}

		public void TestShortCaptionCombinesChildShortCaptions()
		{
			var mockery = new MockRepository(MockBehavior.Strict);
			var child1 = mockery.Create<IHintExtension>();
			var child2 = mockery.Create<IHintExtension>();

			using (var owner = new GenericExtendedControl())
			{
				owner.Extensions = new ControlExtensionCollection(owner);
				var extension = CreateAndInitializeExtension(new[] { child1.Object, child2.Object }, owner);

				child1.Setup(m => m.ShortCaption).Returns("shortcaption1");
				child2.Setup(m => m.ShortCaption).Returns("shortcaption2");
				AssertEquals("shortcaption1 & shortcaption2", extension.ShortCaption);
			}
		}

		public void TestCaptionReturnsOverridenCaptionIfSet()
		{
			var mockery = new MockRepository(MockBehavior.Strict);
			var child1 = mockery.Create<IHintExtension>();
			var child2 = mockery.Create<IHintExtension>();

			using (var owner = new GenericExtendedControl())
			{
				owner.Extensions = new ControlExtensionCollection(owner);
				var extension = CreateAndInitializeExtension(new[] { child1.Object, child2.Object }, owner);
				extension.Caption = "TEST";
				AssertEquals("TEST", extension.Caption);
			}
		}

		public void TestCaptionCombinesChildCaptions()
		{
			var mockery = new MockRepository(MockBehavior.Strict);
			var child1 = mockery.Create<IHintExtension>();
			var child2 = mockery.Create<IHintExtension>();

			using (var owner = new GenericExtendedControl())
			{
				owner.Extensions = new ControlExtensionCollection(owner);
				var extension = CreateAndInitializeExtension(new[] { child1.Object, child2.Object }, owner);

				child1.Setup(m => m.Caption).Returns("caption1");
				child2.Setup(m => m.Caption).Returns("caption2");
				AssertEquals("caption1 & caption2", extension.Caption);
			}
		}

		public void TesDescriptionReturnsOverridenDescriptionIfSet()
		{
			var mockery = new MockRepository(MockBehavior.Strict);
			var child1 = mockery.Create<IHintExtension>();
			var child2 = mockery.Create<IHintExtension>();

			using (var owner = new GenericExtendedControl())
			{
				owner.Extensions = new ControlExtensionCollection(owner);
				var extension = CreateAndInitializeExtension(new[] { child1.Object, child2.Object }, owner);
				extension.Description = "TEST";
				AssertEquals("TEST", extension.Description);
			}
		}

		public void TestDescriptionCombinesChildDescriptions()
		{
			var mockery = new MockRepository(MockBehavior.Strict);
			var child1 = mockery.Create<IHintExtension>();
			var child2 = mockery.Create<IHintExtension>();

			using (var owner = new GenericExtendedControl())
			{
				owner.Extensions = new ControlExtensionCollection(owner);
				var extension = CreateAndInitializeExtension(new[] { child1.Object, child2.Object }, owner);

				child1.Setup(m => m.Description).Returns("desc1");
				child2.Setup(m => m.Description).Returns("desc2");
				AssertEquals("desc1\r\ndesc2", extension.Description);
			}
		}

		CompositeHintExtension CreateAndInitializeExtension(IHintExtension[] children, GenericExtendedControl owner)
		{
			var extension = new TestCompositeHintExtension();

			for (var i = 0; i < children.Length; i++)
			{
				extension.Add(children[i]);
			}
			extension.Initialize(owner);

			return extension;
		}

		#region Support

		class TestCompositeHintExtension : CompositeHintExtension
		{
			public new void Add(IHintExtension extension)
			{
				base.Add(extension);
			}
		}

		#endregion
	}
}
