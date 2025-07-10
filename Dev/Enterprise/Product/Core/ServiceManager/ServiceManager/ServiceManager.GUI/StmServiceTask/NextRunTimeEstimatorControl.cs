using System;
using System.ComponentModel;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ServiceManager.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class NextRunTimeEstimatorControl : ZUserControl
	{
		public NextRunTimeEstimatorControl()
		{
			InitializeComponent();
		}

		NextRunTimeEstimator NextRunTimeEstimator
		{
			get { return (NextRunTimeEstimator)BindingSource.Current; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (NextRunTimeEstimator != null)
			{
				NextRunTimeEstimator.NextRunTimeInfo.ValueChanged -= RefreshNextRunTimeList;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (NextRunTimeEstimator != null)
			{
				NextRunTimeEstimator.NextRunTimeInfo.ValueChanged += RefreshNextRunTimeList;
				RefreshNextRunTimeList();
			}
		}

		void RefreshNextRunTimeList(object sender, EventArgs e)
		{
			RefreshNextRunTimeList();
		}

		void RefreshNextRunTimeList()
		{
			try
			{
				NextRunTimeEstimator.ReloadNextRunTimeList();
			}
			catch (Exception e) when (e is OverflowException)
			{
				NextRunTimeEstimator.AddRowError(Res.GetString("0E75D5FF-EEBF-4360-90A1-89B63E49D3C9", "Invalid Next Run Time - estimated future run times could not be calculated."));
			}
		}

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<NextRunTimeEstimatorControl>().Result;
		}
	}
}
