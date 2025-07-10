using System.Collections.Generic;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DefaultControlExtensionCollectionTest : TestCase
	{
		public void TestCollection_AtDesignTime()
		{
			DesignModeFinder.SetIsDesigningForTest(true);
			try
			{
				AssertEquals(1, ExtensionArray.Length);
				AssertEquals(typeof(ZLabelCaptionRenderer), ExtensionArray[0].GetType());
			}
			finally
			{
				DesignModeFinder.SetIsDesigningForTest(false);
			}
		}

		public void TestCollection_AtRuntime()
		{
			AssertEquals(6, ExtensionArray.Length);
		}

		#region Implementation

		IControlExtension[] ExtensionArray
		{
			get { return new List<IControlExtension>(Extensions).ToArray(); }
		}

		DefaultControlExtensionCollection Extensions
		{
			get { return extensions ?? (extensions = new DefaultControlExtensionCollection(new GenericExtendedControl())); }
		}
		DefaultControlExtensionCollection extensions;

		protected override void TearDown()
		{
			base.TearDown();
			if (extensions != null)
			{
				extensions.Dispose();
			}
		}

		#endregion
	}
}
