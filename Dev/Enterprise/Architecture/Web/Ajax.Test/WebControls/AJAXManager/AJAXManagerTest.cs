using System;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class AJAXManagerTest : TransactionedTestCase
	{
		public void TestScriptManager()
		{
			AJAXControlForTest ajax = new AJAXControlForTest();
			ajax.CreateChildControlsForTest();

			AssertNotNull(ajax.ScriptManager);
		}

		public void TestResources()
		{
			AJAXControlForTest ajax = new AJAXControlForTest();
			bool containsIndicatorImage = ajax.Resources.Count > 0 && ajax.Resources[0].Equals(ajax.AjaxIndicatorImageForTest);
			Assert("AJAX inidcator image file should be included in the Resources collection", containsIndicatorImage);
		}

		public void TestAsyncPostBackTimeout()
		{
			AJAXControlForTest ajax = new AJAXControlForTest();
			ajax.CreateChildControlsForTest();
			AssertEquals("AsyncPostBackTimeout should be 300 by default", 300, ajax.ScriptManager.AsyncPostBackTimeout);

			int initialValue = WebDataRegistry.Instance.RequestTimeout.Value;
			try
			{
				WebDataRegistry.Instance.RequestTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 123);
				ajax.CreateChildControlsForTest();
				AssertEquals("AsyncPostBackTimeout should be controlled by registry", 123, ajax.ScriptManager.AsyncPostBackTimeout);
			}
			finally
			{
				WebDataRegistry.Instance.RequestTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, initialValue);
			}
		}

		#region Implementation

		class AJAXControlForTest : AJAXManager
		{
			public void CreateChildControlsForTest()
			{
				base.CreateChildControls();
			}

			public ZWebResource AjaxIndicatorImageForTest
			{
				get
				{
					return base.AjaxIndicatorImage;
				}
			}
		}

		#endregion
	}
}
