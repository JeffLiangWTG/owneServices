using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlExtensionCollectionTest : TestCase
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestRequiresOwner()
		{
			new ControlExtensionCollection(null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestAddDoNotAcceptNullArgument()
		{
			using (ControlExtensionCollection collection = new ControlExtensionCollection(null))
			{
				collection.Add(null);
			}
		}

		public void TestDoNotInitializeExtensionAfterAddingExtensionToCollectionIfOwnerIsNotNull()
		{
			ControlExtensionCollection collection = new ControlExtensionCollection(owner);

			extension.Setup(m => m.Owner).Returns(owner);

			collection.Add(extension.Object);

			Assert(GetFirst(collection) == extension.Object);

			ExpectDisposable(collection);
		}

		public void TestInitializesExtensionAfterAddingExtensionToCollectionIfOwnerIsNull()
		{
			ControlExtensionCollection collection = new ControlExtensionCollection(owner);

			extension.Setup(m => m.Owner).Returns((IExtendedControl)null);
			extension.Object.Initialize(owner);

			collection.Add(extension.Object);

			Assert(GetFirst(collection) == extension.Object);

			ExpectDisposable(collection);
		}

		public void TestAllowsToAddIncompatibleExtensions()
		{
			using (ControlExtensionCollection collection = new ControlExtensionCollection(owner))
			{
				collection.Add(new AExtension());
				collection.Add(new CExtension());

				Assert(collection.Get<AExtension>() != null);
				Assert(collection.Get<CExtension>() != null);
			}
		}

		public void TestAllowsToAddIncompatibleExtensionsReverse()
		{
			using (ControlExtensionCollection collection = new ControlExtensionCollection(owner))
			{
				collection.Add(new CExtension());
				collection.Add(new AExtension());

				Assert(collection.Get<CExtension>() != null);
				Assert(collection.Get<AExtension>() != null);
			}
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestDoNotAllowToAddCompatibleExtensions()
		{
			using (ControlExtensionCollection collection = new ControlExtensionCollection(owner))
			{
				collection.Add(new AExtension());
				collection.Add(new BExtension());
			}
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestDoNotAllowToAddCompatibleExtensionsReverse()
		{
			using (ControlExtensionCollection collection = new ControlExtensionCollection(owner))
			{
				collection.Add(new BExtension());
				collection.Add(new AExtension());
			}
		}

		public void TestSubsituteExtension()
		{
			using (ControlExtensionCollection collection = new ControlExtensionCollection(owner))
			{
				collection.Add(new CExtension());
				Assert(collection.Get<CExtension>() != null);

				collection.Substitute<CExtension>(new BExtension());
				Assert(collection.Get<CExtension>() == null);
				Assert(collection.Get<BExtension>() != null);
			}
		}

		void ExpectDisposable(ControlExtensionCollection collection)
		{
			foreach (IControlExtension each in collection)
			{
				each.Dispose();
			}

			collection.Dispose();
		}

		#region Test Classes

		class AExtension : IControlExtension
		{
			IExtendedControl owner;

			public IExtendedControl Owner
			{
				get { return owner; }
			}

			public void Initialize(IExtendedControl owner)
			{
				this.owner = owner;
			}

			public void Dispose()
			{
				owner = null;
			}
		}

		class BExtension : AExtension
		{
		}

		class CExtension : IControlExtension
		{
			IExtendedControl owner;

			public IExtendedControl Owner
			{
				get { return owner; }
			}

			public void Initialize(IExtendedControl owner)
			{
				this.owner = owner;
			}

			public void Dispose()
			{
				owner = null;
			}
		}

		#endregion

		#region Implementation

		Mock<IControlExtension> extension;
		IExtendedControl owner;

		protected override void SetUp()
		{
			base.SetUp();

			extension = new Mock<IControlExtension>();
			owner = new GenericExtendedControl();
		}

		T GetFirst<T>(IEnumerable<T> enumerable)
		{
			var enumerator = enumerable.GetEnumerator();
			return enumerator.MoveNext() ? enumerator.Current : default(T);
		}

		#endregion
	}
}
