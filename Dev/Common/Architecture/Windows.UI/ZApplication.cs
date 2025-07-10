using System;
using System.Linq;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	public static class ZApplication
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "The only place that should use Application.OpenForms")]
		public static Form[] GetOpenForms()
		{
#if WINZOR

			return System.Windows.Forms.Application.OpenForms.GetSnapshot().ToArray();

#else

			var retries = 0;

			while (true)
			{
				try
				{
					return System.Windows.Forms.Application.OpenForms.Cast<Form>().ToArray();
				}
				catch (InvalidOperationException)
				{
					retries++;
					if (retries > 5)
					{
						throw;
					}
				}
			}

#endif
		}
	}
}
