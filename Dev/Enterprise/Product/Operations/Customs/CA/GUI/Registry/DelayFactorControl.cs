using System;
using Enterprise.Customs.CA.Registry;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.CA.GUI
{
	public partial class DelayFactorControl : RegistryZUserControl
	{
		public DelayFactorControl()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(HVSDelayIntervalTypeDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(CONDelayIntervalTypeDropEdit);
		}

		public new DelayFactorRegistryBusinessObject CurrentDataItem
		{
			get { return (DelayFactorRegistryBusinessObject)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetBusinessEntityReadOnly();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			SetBusinessEntityReadOnly();
		}

		void SetBusinessEntityReadOnly()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.ReadOnly = ReadOnly;
			}
		}
	}
}
