using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise
{
	public class FormDetails : NonPersistentBusinessObject, IObsoleteValidation
	{
		public FormDetails(Form form)
		{
			FormCaption = form.Text;
		}

		public ZString FormCaption
		{
			get;
			private set;
		}
	}

	public class NSFormCollection : NonPersistentBusinessObjectCollection<FormDetails>
	{
		protected override bool AllowNewCore
		{
			get { return false; }
		}
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}

	public class PerformanceStatisticWithForms : PerformanceStatistic
	{
		public override void Load()
		{
			OpenedForms.RemoveAll();

			base.Load();

			foreach (var form in ZApplication.GetOpenForms())
			{
				if (!form.Disposing)
				{
					try
					{
						OpenedForms.Add(new FormDetails(form));
					}
					catch (ObjectDisposedException)
					{
						// Race condition error swallowed. It should be almost impossible to trigger this,
						// and if it does get triggered it will show a malformed item in the forms tab until refreshed.
					}
				}
			}
		}

		public NSFormCollection OpenedForms { get; } = new NSFormCollection();
	}
}
