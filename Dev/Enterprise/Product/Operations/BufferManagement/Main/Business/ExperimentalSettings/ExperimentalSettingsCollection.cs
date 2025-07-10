using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ExperimentalSettingsCollection : NonPersistentBusinessObjectCollection<ExperimentalSetting>
	{
		public void Add(IList<ExperimentalSetting> list)
		{
			if (list != null)
			{
				foreach (var setting in list)
				{
					Add(setting);
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ExperimentalSetting();
		}
	}
}
