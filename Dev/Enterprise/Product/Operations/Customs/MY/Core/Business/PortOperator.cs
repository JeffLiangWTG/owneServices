using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using static Enterprise.Integration.Customs.MY;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MY.Business
{
	public class PortOperator : IPortOperator
	{
		public PortOperator(CommonConsol consol, bool isImport)
		{
			this.consol = consol;
			this.isImport = isImport;
		}

		public ZString PortOperatorCode
		{
			get { return CTO == null ? ZString.Empty : CTO.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, Malaysia).ToUpper(); }
		}

		public ZString PortOperatorAndBerth
		{
			get { return PortOperatorCode + (Berth.IsEmpty ? ZString.Empty : Berth.PadLeft(2, '0')); }
		}

		public ZString PortOperatorAndSCN
		{
			get { return PortOperatorCode + ":" + SCN; }
		}

		public ZString CustomsStation
		{
			get
			{
				ZString result = ZString.Empty;
				if (Env.Registry.MYCustoms.IsTestMode)
				{
					result = "H10";
				}
				else if (CTO != null)
				{
					result = CTO.CustomsCodes.GetCustomsRegNo(MalaysiaOrgCusCodeInfo.OrgCusCodes.CustomsStation, Malaysia).ToUpper();
				}
				return result;
			}
		}

		public void Validate(INotifications notify)
		{
			if (PortOperatorCode.IsEmpty)
			{
				notify.Notify(new ErrorNotification(MYDataErrorType.Error, "Mandatory field Port Operator Code (defined as a custom code on CTO) not entered"));
			}
			if (CustomsStation.IsEmpty)
			{
				notify.Notify(new ErrorNotification(MYDataErrorType.Error, "Mandatory field Customs Station Code (defined as a custom code on CTO) not entered"));
			}
			if (Berth.IsEmpty)
			{
				notify.Notify(new ErrorNotification(MYDataErrorType.Error, "Mandatory field Berth Number not entered"));
			}
			foreach (char c in Berth)
			{
				if (!char.IsNumber(c))
				{
					notify.Notify(new ErrorNotification(MYDataErrorType.BerthNumberInvalid, Berth));
					break;
				}
			}
			if (Berth.Length > 2)
			{
				notify.Notify(new ErrorNotification(MYDataErrorType.BerthNumberTooBig, Berth));
			}
		}

		#region Implementation

		readonly CommonConsol consol;
		readonly bool isImport;

		ZString SCN
		{
			get { return ImportOrExportTransport == null ? ZString.Empty : ImportOrExportTransport.JW_JX_DepartOrArriveReference; }
		}

		ZString Berth
		{
			get { return ImportOrExportTransport == null ? ZString.Empty : ImportOrExportTransport.JW_JX_DepartOrArriveBerth; }
		}

		Transport ImportOrExportTransport
		{
			get { return isImport ? consol.Transports.ImportTransport : consol.Transports.ExportTransport; }
		}

		RefCountry Malaysia
		{
			get { return (RefCountry)consol.Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, "MY"); }
		}

		OrgHeader CTO
		{
			get
			{
				OrgHeader result = null;
				if (isImport)
				{
					result = (consol.ArrivalCTOAddress == null) ? null : consol.ArrivalCTOAddress.Header;
				}
				else
				{
					result = (consol.DepartureCTOAddress == null) ? null : consol.DepartureCTOAddress.Header;
				}
				return result;
			}
		}

		#endregion
	}
}
