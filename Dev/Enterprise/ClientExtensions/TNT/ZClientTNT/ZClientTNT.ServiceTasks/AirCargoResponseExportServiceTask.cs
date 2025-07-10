using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.TNT;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.Client.TNT.ServiceTasks;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	TNTConstants.AirCargoResponseExportSrvTaskCode,
	"Air Cargo Response Files Export",
	"CSP",
	typeof(AirCargoResponseExportServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia,
	MinimumPeriod = "30seconds",
	DefaultScheduleRunEvery = "30seconds"
	)]
namespace Enterprise.Client.TNT.ServiceTasks
{
	class AirCargoResponseExportServiceTask : TNTServiceTask
	{
		protected override void Execute(NotificationBuffer notify, CancellationToken token)
		{
			if (TNTDataRegistry.Instance.AirCargoResponseExportHighWaterMark.IsEmpty || !TNTDataRegistry.Instance.AirCargoResponseExportHighWaterMark.IsValid)
			{
				TNTDataRegistry.Instance.AirCargoResponseExportHighWaterMark = ZDateTime.Today.AddDays(-1);
			}

			ZDateTime newHighwatermark = ZDateTime.UtcNow.AddMinutes(-5);

			if (TNTDataRegistry.Instance.AirCargoResponseExportHighWaterMark < newHighwatermark)
			{
				notify.Notify(new InfoNotification("Air Cargo Response File Export Begins ...... "));
				StmALog[] logs = Factory.Load<StmALog>(LogFilter(newHighwatermark));

				notify.Notify(new InfoNotification(logs.Length.ToString(CultureInfo.InvariantCulture) + " log(s) found in this cycle ...... "));
				foreach (StmALog current in logs)
				{
					token.ThrowIfCancellationRequested();
					CusHAWB cusHawb = Factory.Load<CusHAWB>(current.SL_Parent);
					if (cusHawb != null)
					{
						ExportData(cusHawb, current, notify);
					}
				}

				TNTDataRegistry.Instance.AirCargoResponseExportHighWaterMark = newHighwatermark;
				notify.Notify(new InfoNotification("Air Cargo Response File Export Finished ......  "));
			}
		}

		void ExportData(CusHAWB cusHawb, StmALog log, NotificationBuffer notify)
		{
			StmNote[] notes = cusHawb.Notes.FindByDescription(ConsignmentUpdator.OriginalQuantumSectorNoteDescription);
			if (notes.Length > 0)
			{
				TNTReturnExitStatus status = new TNTReturnExitStatus(notify);
				ZString[] noteAsText = notes[0].ST_NoteDataAsText.Split('-');
				if (noteAsText.Length == 4)
				{
					string origin = noteAsText[1];
					string destination = noteAsText[2];
					string branchCode = noteAsText[3];
					string customsStatus = cusHawb.CS_CustomsStatus;

					status.SendEDNReply(branchCode, cusHawb.CS_HAWB, origin, destination, cusHawb.CS_TranshipmentEntryNum, customsStatus, GetClearedForDeliveryIndicator(customsStatus));
				}
			}
		}

		ZQuery LogFilter(ZDateTime endDateTime)
		{
			ZQuery result = new ZQuery(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan, TNTDataRegistry.Instance.AirCargoResponseExportHighWaterMark); //select logs between last hwm
			result.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, endDateTime);
			result.AddToFilter(StmALogSchema.SL_Table, CusHAWBSchema.Constants.TableName);
			result.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);

			ZQuery eventQuery = new ZQuery();
			eventQuery.DefaultJoinCondition = JoinCondition.Or;

			foreach (ICodeDescription current in TNTDataRegistry.Instance.CustomsStatusCode)
			{
				eventQuery.AddToFilter(StmALogSchema.SL_Reference, current.Code);
			}

			result.AddToFilter(eventQuery);
			result.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name;
			result.IsNoLock = false;

			return result;
		}

		internal
		string GetClearedForDeliveryIndicator(string status)
		{
			string result = "N";

			foreach (ICodeDescription customsStatus in TNTDataRegistry.Instance.CustomsStatusCode)
			{
				if (status == customsStatus.Code.ToUpper())
				{
					result = (new ZString(customsStatus.Description.Trim()).Left(1).ToUpper() == "Y") ? "Y" : result;
					break;
				}
			}

			return result;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		protected override ZString RegistriesNotSetErrMesg
		{
			get { return ErrorMessage; }
		}

		protected override bool IsEnvironmentValid
		{
			get
			{
				return !TNTDataRegistry.Instance.TNTReplyDirectory.IsEmpty && Directory.Exists(TNTDataRegistry.Instance.TNTReplyDirectory) &&
					TNTDataRegistry.Instance.CustomsStatusCode.Count > 0;
			}
		}

		internal const string ErrorMessage = "Air Cargo Customs Response File Registry Items are not set or invalid. Please verify the values in Admin-> Registry-> TNT Client Extensions-> Air Cargo Customs Response File";
	}
}
