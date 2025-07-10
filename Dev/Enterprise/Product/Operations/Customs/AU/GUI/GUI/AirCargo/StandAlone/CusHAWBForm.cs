using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	/// <summary>
	/// Base Air Cargo House form without Messaging functionality (i.e. no Messaging menu item & Messaging tab)
	/// </summary>
	public partial class CusHAWBForm : ZTemplateForm
	{
		[Obsolete("Design-time only", true)]
		public CusHAWBForm()
		{
		}

		public CusHAWBForm(CusHAWB houseBill)
			: base(houseBill)
		{
			DataContext = Core.Constants.DataContext.CusHAWB;
			fHouseUserControl.HAWB = houseBill;
			HouseUserControl.Dock = DockStyle.Fill;
		}

		public override string FormCaption
		{
			get { return "Air Cargo House"; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (!initialized && dataSource != null)
			{
				InitialiseHouseUserControl();
				HouseUserControl.SetupPlugins();
				initialized = true;
			}
		}
		bool initialized;

		protected CusHAWB HouseBill
		{
			get { return (CusHAWB)DataSource; }
		}

		#region House User Control

		void InitialiseHouseUserControl()
		{
			if (HouseBill.MAWB != null)
			{
				HouseBill.MAWB.SetReadOnly(false);
			}
			HouseUserControl.Dock = DockStyle.Fill;
			MainTabPage.Controls.Add(HouseUserControl);
		}

		internal BaseAirCargoHouseUserControl HouseUserControl
		{
			get
			{
				if (fHouseUserControl == null)
				{
					fHouseUserControl = new CMRAirCargoHouseUserControl();
				}
				return fHouseUserControl;
			}
		}
		BaseAirCargoHouseUserControl fHouseUserControl;

		#endregion
	}
}
