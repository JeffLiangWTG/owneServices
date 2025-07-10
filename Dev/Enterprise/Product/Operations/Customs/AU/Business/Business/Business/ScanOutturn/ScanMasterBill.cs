using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class ScanMasterBill : IScanMasterBill
	{
		protected ScanMasterBill(IScanMasterBillProvider masterBill)
		{
			this.fMasterBill = Argument.NotNull(masterBill, "IScanMasterBillProvider");
		}
		protected readonly IScanMasterBillProvider fMasterBill;

		public IScanMasterBillProvider MasterBill
		{
			get { return fMasterBill; }
		}

		public virtual string Validate()
		{
			var errorString = ZString.Empty;
			if (Underbonds == null)
			{
				errorString = Res.GetString("22ec7290-87bd-48c9-89fe-ee7d3c81ee45", "No Underbond Movement exists for Master '{0}'. A Master Underbond Movement must exist for scanning. If this job was created after the underbond was approved then you will need to manually create a 'dummy' Master Underbond Movement with the same premise codes as in the original underbond. Note this record is for internal use only and, as the movement has already been reported to Customs, so you do not need to send the Underbond message for this dummy record.", MasterBill.MasterBill);
			}
			return errorString;
		}

		public IEnumerable<IScanHouseBillProvider> GetChildBills()
		{
			return MasterBill.GetChildBills(SelectedUnderbond);
		}

		public CusUnderbond[] Underbonds
		{
			get
			{
				if (underbonds == null && MasterBill.Underbonds.Any())
				{
					underbonds = MasterBill.Underbonds.ToArray();
				}
				return underbonds;
			}
		}
		CusUnderbond[] underbonds;

		public void RefreshUnderbonds()
		{
			underbonds = null;
		}

		public ZBool IsStandAlone
		{
			get { return MasterBill.IsStandAlone; }
		}

		public CusUnderbond SelectedUnderbond { get; set; }

		public ZString MasterBillNumber
		{
			get { return MasterBill.MasterBill; }
		}

		public BusinessObjectFactory Factory
		{
			get { return MasterBill.Factory; }
		}

		public virtual string ValidateSelectedUnderbond()
		{
			var errorBuilder = new StringBuilder();
			if (SelectedUnderbond == null)
			{
				errorBuilder.AppendLine(NoUnderbondSelected);
			}
			return errorBuilder.ToString();
		}

		internal static string NoUnderbondSelected
		{
			get { return Res.GetString("22ec7290-97bd-48c9-89fe-ee7d3c81ee41", "No Underbond selected. Please select an underbond first."); }
		}

		public IEnumerable<IScanHouseBillProvider> SelectedShipments { get; set; }

		public IEnumerable<ShipmentSelectorLine> SelectedLines
		{
			get { return selectedLines; }
			set
			{
				selectedLines = value;
				List<IScanHouseBillProvider> selectShipments = new List<IScanHouseBillProvider>();
				foreach (var line in selectedLines)
				{
					selectShipments.AddRange(line.HouseBills);
				}
				SelectedShipments = selectShipments;
			}
		}
		IEnumerable<ShipmentSelectorLine> selectedLines;

		public bool ShipmentsWasSelected
		{
			get { return SelectedShipments != null && SelectedShipments.Any(); }
		}

		#region IScanMasterBill

		string[] IScanMasterBill.GetMutexKeys()
		{
			if (IsStandAlone)
			{
				return new[] { GetMutexKey(MasterBill.MasterBill, MasterBill.MasterHouseBill, true, true) };
			}
			else
			{
				if (ShipmentsWasSelected)
				{
					var result = new List<string>();

					foreach (var shipment in SelectedLines)
					{
						result.Add(GetShipmentMutexKey(shipment));
					}

					return result.ToArray();
				}
				else
				{
					return System.Array.Empty<string>();
				}
			}
		}
		#endregion

		protected string GetShipmentMutexKey(ShipmentSelectorLine shipment)
		{
			if (shipment.Type == Core.Constants.ShipmentTypes.StandardHouse)
			{
				return GetMutexKey(MasterBill.MasterBill, string.Empty, false, false);
			}
			else
			{
				return GetMutexKey(MasterBill.MasterBill, shipment.Shipment, false, true);
			}
		}

		public static string GetMutexKey(string masterBill, string houseBill, bool isStandAlone, bool isHVLVShipment)
		{
			if (isStandAlone)
			{
				return string.Format(CultureInfo.InvariantCulture, "MasterBill:{0} MasterHouseBill:{1}", masterBill, houseBill);  //Suppress warning: mutex is internal and not visible for user.
			}
			else
			{
				if (isHVLVShipment)
				{
					return string.Format(CultureInfo.InvariantCulture, "MasterBill:{0} HouseBill:{1}", masterBill, houseBill); //Suppress warning: mutex is internal and not visible for user.
				}
				else
				{
					return string.Format(CultureInfo.InvariantCulture, "MasterBill:{0}", masterBill);  //Suppress warning: mutex is internal and not visible for user.
				}
			}
		}
	}
}
