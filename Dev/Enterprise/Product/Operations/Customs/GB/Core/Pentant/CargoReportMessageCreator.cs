using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Pentant
{
	public class CargoReportMessageCreator
	{
		public CargoReportMessageCreator(JobDeclaration declaration, CargoReportType reportType)
		{
			this.declaration = declaration;
			this.reportType = reportType;
		}

		public bool CreateCargoReportMessage()
		{
			var entry = declaration.CustomsEntryHeaders[0];  // Don't need any entry-specific properties, the first entry is fine
			if (entry == null)
			{
				return false;
			}
			if (declaration.IsExport)
			{
				exportWrapper = new GbChiefExportHeader(entry);
				wrapper = exportWrapper;
			}
			else if (declaration.IsImport)
			{
				importWrapper = new GbChiefImportHeader(entry);
				wrapper = importWrapper;
			}

			if (wrapper == null)
			{
				return false;
			}

			var credential = CredentialsSetting.GetCredentialsForBadge(declaration.JE_CustomsProfile, declaration.CompanyPK);
			if (credential == null)
			{
				return false;
			}
			var cargoMessage = PopulateMessageFields(credential);
			var messageString = cargoMessage.Serialise();
			var ediMessage = entry.Factory.New<GbEDIMessage>();
			ediMessage.EM_LinkedObject = entry;
			ediMessage.EM_ApplicationCode = credential.IsCDS ? ApplicationCodeList.Codes.GbCustomsDeclarationServices : ApplicationCodeList.Codes.Pentant;
			ediMessage.EM_MessageType = PentantConstants.CargoMessageType;
			ediMessage.EM_MessageSubType = reportType == CargoReportType.AMEND ? AmendMessage : NewMessage;
			ediMessage.EM_MessageText = messageString;
			ediMessage.EM_MessageOwner = declaration.JE_CustomsProfile.Left(GbEDIMessage.Schema.EM_MessageOwnerMaxLength);
			ediMessage.EM_ApplicationReference = credential.SenderID;
			ediMessage.EM_MessageInterpretation = GetInterpretation();
			ediMessage.MessageNumberStrategy = new GbMessageNumberStrategy(ediMessage.Factory, ApplicationCodeList.Codes.Pentant);
			entry.Messages.Add(ediMessage);
			return !string.IsNullOrEmpty(messageString) && ediMessage != null;
		}

		ZString GetInterpretation()
		{
			var interpretation = MessagePrettierCss.CSS + GetReportTypeInterpretation();
			return interpretation;
		}

		ZString GetReportTypeInterpretation() => ZString.Format("Cargo report {0} message for ACA {1}", reportType == CargoReportType.AMEND ? "amend" : "create", declaration.JE_ACAReference);

		CargoReportMessage PopulateMessageFields(CredentialsSetting credential)
		{
			var cargoReport = new CargoReportMessage();
			cargoReport.MsgRef = GbEDIMessage.MessageNumberPlaceHolder;
			cargoReport.Sender = credential?.SenderID ?? "";
			cargoReport.Recipient = credential?.ReceiverID ?? "";
			cargoReport.MsgFunction = CargoReportMessageFunction;
			cargoReport.ACARef = declaration.JE_ACAReference;
			cargoReport.AgentsOwnRef = declaration.JE_UCR;
			cargoReport.ImportExport = wrapper.DeclarationType.SubstringSafe(0, 1);  // I or E
			var now = ZDateTime.Now;
			cargoReport.Date = now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);  //
			cargoReport.Time = now.ToShortTimeString();
			cargoReport.Port = GetPort();
			cargoReport.Shed = wrapper.ShedCode;
			cargoReport.VehicleNoPlate = GetVehicleNoPlate();
			cargoReport.TrailerNo = GetTrailer();
			cargoReport.NoPackages = wrapper.TotalPackages.ToString();
			cargoReport.Weight = wrapper.GrossWeightInKilograms.ToStringTrimZeros();
			cargoReport.Marks = GetAllPackageMarks();
			cargoReport.TypePackages = "PKGS";// GetAllPackageTypes(); Pentant only offer a few different options, a mix of codes and words, so even if there were only one pack type it might not match one of theirs.
			cargoReport.Hazardous = exportWrapper?.Lines?.Any(l => !l.UNDGCode.IsEmpty) ?? false ? "Y" : "N";
			cargoReport.GoodsDescription = wrapper.GoodsDescription;
			cargoReport.ModeOfTransport = "VEH";
			cargoReport.PlaceArrivalExport = GetPlaceArrivalExport();
			cargoReport.OrginalFinalPOD = exportWrapper?.CountryOfDestination + importWrapper?.CountryOfExport.Left(2);
			return cargoReport;
		}

		protected virtual string GetPort() => wrapper.LocationOfGoodsWithoutGbPrefix;

		protected virtual string GetVehicleNoPlate() => wrapper.TransportIdentityAtTheBorderBox21;

		protected virtual string GetPlaceArrivalExport() => wrapper.LocationOfGoodsWithoutGbPrefix;

		protected string GetTrailer()
		{
			var trailer = declaration?.RelevantConsol?.MostInterestingTransportForBinding?[0].JW_VesselForBinding ?? ZString.Empty;
			if (trailer.IsEmpty)
			{
				var transport = declaration.TransportsIncludingRelated.FirstLeg;
				if (transport != null && transport.JW_TransportMode.Equals(Core.Constants.TransportModes.Road) && transport.JW_TransportType.Equals(RefTransportModeList.Codes.MAI))
				{
					trailer = transport.JW_VesselForBinding;
				}
			}
			return trailer;
		}

		protected string GetAllPackageMarks()
		{
			return ZString.Join(", ", wrapper.Lines.SelectMany(line => line.Packages.Select(pack => pack.PackageMarks.Trim())).Where(x => !x.IsEmpty).Distinct().ToArray()).Left(35);
		}

		protected ZString CargoReportMessageFunction => reportType == CargoReportType.CREATE ? createMessageFunction : amendMessageFunction;

		protected GbChiefHeader wrapper;
		protected GbChiefExportHeader exportWrapper;
		protected GbChiefImportHeader importWrapper;
		protected JobDeclaration declaration;
		readonly CargoReportType reportType;

		const string createMessageFunction = "C";
		const string amendMessageFunction = "A";

		const string NewMessage = GBMessageTypeList.Codes.New;
		const string AmendMessage = GBMessageTypeList.Codes.Amend;

		public enum CargoReportType
		{
			CREATE,
			AMEND
		}
	}
}
