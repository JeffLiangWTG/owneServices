using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.EdiFact.UKCINV;
using Enterprise.Customs.GB.Chief.GenericMessagingHarness;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class ErsReportProcessor
	{
		public ErsReportProcessor(ErsReport ersReport, EDIMessage ediMessage, UkcinvUnderstander ukCinvUnderstander)
		{
			this.ersReport = ersReport;
			inboundEdiMessage = ediMessage;
			this.ukCinvUnderstander = ukCinvUnderstander;
		}

		internal bool DoAllProcessing()
		{
			var processedMucrOk = ProcessMucr(ersReport.MasterUCR, null);
			var processedDeclarationsOk = UpdateEachChildRecord();
			return processedMucrOk || processedDeclarationsOk;
		}

		bool ProcessMucr(ZString masterUcr, IRoutingProvider provider)
		{
			var consol = FindGbExportConsolFromMucrOrFromLinkedMessage(masterUcr);
			if (consol != null)
			{
				var wrapper = new CustomsExportConsolIntegrationWrapper(consol, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				UpdateAndProcessConsol(wrapper.MawbExportHelper, provider);
				AddAndPositivelyUpdateReceivedMessage(consol);
				return true;
			}
			return false;
		}

		protected virtual void UpdateAndProcessConsol(MawbExportAddInfo mawbExportAddInfo, IRoutingProvider provider)
		{
			if (!ersReport.EntryProcessingUnitID.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefEntryProcessingUnitID = ersReport.EntryProcessingUnitID.Left(mawbExportAddInfo.ME_ChiefEntryProcessingUnitIDInfo.MaxLength);
			}

			if (!ersReport.EntryProcessingUnitNumber.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefEntryProcessingUnitNumber = ersReport.EntryProcessingUnitNumber.Left(mawbExportAddInfo.ME_ChiefEntryProcessingUnitNumberInfo.MaxLength);
			}

			if (!ersReport.GoodsArrivalDateTime.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefGoodsArrivalDateTime = ersReport.GoodsArrivalDateTime;
			}

			if (!ersReport.GoodsLocation.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefGoodsLocation = ersReport.GoodsLocation.Left(mawbExportAddInfo.ME_ChiefGoodsLocationInfo.MaxLength);
			}

			if (!ersReport.MovementReference.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefMovementReference = ersReport.MovementReference.Left(mawbExportAddInfo.ME_ChiefMovementReferenceInfo.MaxLength);
			}

			if (!ersReport.Shed.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefShed = ersReport.Shed.Left(mawbExportAddInfo.ME_ChiefShedInfo.MaxLength);
			}

			if (provider != null && !provider.RouteOfEntry.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefMasterRouteOfEntry = provider.RouteOfEntry.Left(mawbExportAddInfo.ME_ChiefMasterRouteOfEntryInfo.MaxLength);
			}

			if (provider != null && !provider.StyleOfEntry.IsEmpty)
			{
				mawbExportAddInfo.ME_ChiefMasterStyleOfEntry = provider.StyleOfEntry.Left(mawbExportAddInfo.ME_ChiefMasterStyleOfEntryInfo.MaxLength);
			}
		}

		protected virtual void AddAndPositivelyUpdateReceivedMessage(ForwardingConsol consol)
		{
			inboundEdiMessage.EM_MessageInterpretation = GetInterpretation();
			inboundEdiMessage.EM_Status = EDIMessage.Status.Received;
			inboundEdiMessage.EM_MessageType = ukCinvUnderstander.Class.ToString();
			inboundEdiMessage.EM_LinkedObject = consol;
		}

		protected virtual ZString GetInterpretation()
		{
			var result = MessagePrettierCss.CSS;
			result += "<h3>" + MessageTitle + "</h3>";

			var headerTable = CreateHeaderTable();
			result += headerTable.ToHtml();

			foreach (var decReport in ersReport.Declarations)
			{
				var ducr = new DucrAndPartHelper(decReport.DeclarationUCR, decReport.DeclarationUcrPart).DucrAndPartWithoutChecksum;
				result += "<h4>Declaration " + ducr + "</h4>";
				var declarationHtmlTable = new HtmlTableCreator(new string[] { "Field", "Value", "Meaning" });
				declarationHtmlTable.WriteRow("SOE", decReport.StyleOfEntry, GetStyleMeaning(decReport.StyleOfEntry));
				declarationHtmlTable.WriteRow("Route", decReport.RouteOfEntry, GetRouteMeaning(decReport.RouteOfEntry));
				declarationHtmlTable.WriteRow("Role", decReport.SubmittingRole, "");
				declarationHtmlTable.WriteRow("Mass", decReport.TotalNetMassKilos, "");
				declarationHtmlTable.WriteRow("Packages", decReport.TotalPackages, "");
				declarationHtmlTable.WriteRow("Commodity", decReport.CommodityCode, "");
				result += declarationHtmlTable.ToHtml();
			}
			return result;
		}

		protected ZString GetStyleMeaning(ZString style)
		{
			return new ExportStyleOfEntries().GetDescriptionFromCode(style);
		}

		protected string GetRouteMeaning(ZString route)
		{
			return new Common.EU.MessageStatusList().GetDescriptionFromCode(StatusChecker.GetStatusCodeFromRouteOfEntryStatic(route));
		}

		protected virtual ZString MessageTitle
		{
			get { return "ERS - Route or Status Change"; }
		}

		protected virtual HtmlTableCreator CreateHeaderTable()
		{
			var headerHtmlTable = new HtmlTableCreator(new string[] { "Field", "Value" });
			headerHtmlTable.WriteRow("Master UCR", ersReport.MasterUCR);
			headerHtmlTable.WriteRow("Movement Reference", ersReport.MovementReference);
			headerHtmlTable.WriteRow("Goods' location & Shed", ersReport.GoodsLocation + ersReport.Shed);
			headerHtmlTable.WriteRow("Arrival Date", ersReport.GoodsArrivalDateTime);
			headerHtmlTable.WriteRow("EPU", ersReport.EntryProcessingUnitNumber + " " + ersReport.EntryProcessingUnitID);
			return headerHtmlTable;
		}

		ForwardingConsol FindGbExportConsolFromMucrOrFromLinkedMessage(ZString mucr)
		{
			if (MucrIsInStandardAirFormat(mucr))
			{
				return inboundEdiMessage.Factory.LoadTop1<ForwardingConsol>(GetConsolLoadQuery(mucr));
			}
			else
			{
				var maybeConsol = inboundEdiMessage.EM_LinkedObject as ForwardingConsol;
				if (maybeConsol != null)
				{
					return maybeConsol;
				}
			}
			return null;
		}

		public static ZQuery GetConsolLoadQuery(string mucr)
		{
			var mawbNumber = mucr.Replace("A:", "");
			var query = new ZQuery(JobConsolSchema.JK_RL_NKLoadPort, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.UnitedKingdom);
			query.AddToFilter(JobConsolSchema.JK_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddYears(-1));
			query.AddToFilter(JobConsolSchema.JK_MasterBillNum, mawbNumber);
			return query;
		}

		internal static bool MucrIsInStandardAirFormat(string ucr)
		{
			return Regex.IsMatch(ucr, @"^A\:([A-Z]{3}|[0-9]{3})[0-9]{8}$");   // A:LHR12345678 or A:12512345678
		}

		bool UpdateEachChildRecord()
		{
			var success = false;
			foreach (var decReport in ersReport.Declarations)
			{
				if (ErsReportProcessor.MucrIsInStandardAirFormat(decReport.DeclarationUCR))
				{   // mucr
					success = ProcessMucr(decReport.DeclarationUCR, decReport) || success;
				}
				else
				{   // ducr
					var entry = LoadCusEntryHeaderFromDucr(decReport);
					if (entry != null && entry.Declaration != null)
					{
						if (!decReport.RouteOfEntry.IsEmpty)
						{
							entry.CH_RouteOfEntry = decReport.RouteOfEntry;
						}

						if (!decReport.ImportCustomsStatus.IsEmpty)
						{
							entry.CH_ImportClearanceStatusICS = decReport.ImportCustomsStatus;
						}

						if (!decReport.StyleOfEntry.IsEmpty)
						{
							entry.CH_StyleOfEntrySOE = decReport.StyleOfEntry;
						}

						AddErsMessageToEntry(entry);
						success = true;
					}
				}
			}
			return success;
		}

		void AddErsMessageToEntry(CusEntryHeader entry)
		{
			if (inboundEdiMessage.EM_LinkedObject == null)
			{
				inboundEdiMessage.EM_LinkedObject = entry;
			}
			inboundEdiMessage.EM_Status = EDIMessage.Status.Received;
			inboundEdiMessage.EM_MessageType = ukCinvUnderstander.Class.ToString();
			inboundEdiMessage.EM_MessageInterpretation = GetInterpretation();
		}

		CusEntryHeader LoadCusEntryHeaderFromDucr(EmrOrErsDeclarationReport decReport)
		{
			var query = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, new DucrAndPartHelper(decReport.DeclarationUCR, decReport.DeclarationUcrPart).DucrAndPartWithoutChecksum);
			return inboundEdiMessage.Factory.LoadTop1<CusEntryHeader>(query);
		}

		protected ErsReport ersReport;
		protected EDIMessage inboundEdiMessage;
		protected UkcinvUnderstander ukCinvUnderstander;
	}
}
