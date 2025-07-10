using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ZApplicationTest : TestCase
	{
		public void TestGetOpenForms()
		{
			var done = false;
			Exception exceptionThrown = null;
			var openFormsObserverThread = new Thread(() =>
			{
				try
				{
					while (!done)
					{
						ZApplication.GetOpenForms();
					}
				}
				catch (Exception ex)
				{
					exceptionThrown = ex;
				}
			});
			openFormsObserverThread.Start();

			var forms = new List<Form>();
			try
			{
				for (var i = 0; i <= 500; i++)
				{
					var form = new Form();
					form.Show();
					forms.Add(form);
				}
			}
			finally
			{
				done = true;
				openFormsObserverThread.Join();
				foreach (var form in forms.ToArray())
				{
					form.Dispose();
				}
			}

			AssertNull(exceptionThrown);
		}
	}
}
