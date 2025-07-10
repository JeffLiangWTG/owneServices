using System;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	public abstract class BaseExtensionTest<TExtension> : TestCase where TExtension : class, IControlExtension
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestRequiresOwner()
		{
			Extension.Initialize(null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInitializationIsAllowedOnlyOnce()
		{
			Extension.Initialize(GetOwner());
			Extension.Initialize(GetOwner());
		}

		public void TestDisposeNullifyReferences()
		{
			Extension.Initialize(GetOwner());
			AssertNotNull(Extension.Owner);

			Extension.Dispose();
			AssertNull(Extension.Owner);
		}

		protected virtual IControlExtension Create()
		{
			return Activator.CreateInstance<TExtension>();
		}

		protected virtual IExtendedControl GetOwner()
		{
			return new GenericExtendedControl();
		}

		#region Implementation

		IControlExtension Extension
		{
			get { return extension ?? (extension = Create()); }
		}
		IControlExtension extension;

		protected override void TearDown()
		{
			base.TearDown();
			IDisposable disposableExtension = Extension;
			if (disposableExtension != null)
			{
				disposableExtension.Dispose();
			}
		}

		#endregion
	}
}
