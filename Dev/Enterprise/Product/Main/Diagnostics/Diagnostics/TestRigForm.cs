using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.Main.Diagnostics
{
	public partial class TestRigForm : KForm
	{
#if DEBUG
		#region Licencing

		void DecryptButton_Click(object sender, EventArgs e)
		{
			try
			{
				ISystemRegistrationKey key = SystemRegistrationKey.NewFromEncryptedXmlKey(SystemRegCodeTextBox.Text);
				textBox1.Text = key.DatabaseName;
				textBox2.Text = key.DatabaseType;
				textBox3.Text = key.DbInstanceName;
				textBox4.Text = key.ServerSid.ToString();
				textBox5.Text = key.SystemExpiryDate.ToShortDateString();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.Show("Packet Corrupt");
			}
		}

		void SystemRegKeyButton_Click(object sender, EventArgs e)
		{
			SystemRegCodeTextBox.Text = EnvProxy.Instance.Registry.LegacyEncryptedSystemRegistrationKey;
		}

		#endregion

		#region Messaging

		class IPop3DownloaderDummy : IMailDownloader
		{
			public IPop3DownloaderDummy(string rawMIMEString)
			{
				this.rawMIMEString = rawMIMEString;
			}
			string rawMIMEString;

			#region IPop3Downloader Members

			public long? MessageCount => 1;

			public void DownloadFromServer()
			{
				if (EmailDownloaded != null)
				{
					bool continueDownloading = false;
					EmailDownloaded(Guid.NewGuid().ToString(), ref rawMIMEString, ref continueDownloading);
					if (LogMessage != null)
					{
						LogMessage(System.Diagnostics.TraceEventType.Information, "Data downloaded.");
					}
					if (DownloaderClosing != null)
					{
						DownloaderClosing(1);
					}
				}
			}

			void IMailDownloader.DeleteMessage(string msgId)
			{ }

			public void Dispose() { }

			public event EmailDownloadedHandler EmailDownloaded;
			public event LogMessageHandler LogMessage;
			public event DownloaderClosingHandler DownloaderClosing;

			#endregion
		}

		void ImportEmailFileButton_Click(object sender, EventArgs e)
		{
			if (File.Exists(EmailFilenameTextBox.Text))
			{
				string rawEmail = File.ReadAllText(EmailFilenameTextBox.Text);

				if (rawEmail.Length > 0)
				{
					MailSaver mailSaver = new MailSaver(new IPop3DownloaderDummy(rawEmail));
					mailSaver.Retrieve();

					Globals.Message.Show("1 MailDbItem was created", "Email Imported", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					Globals.Message.ShowError("Empty email file: " + EmailFilenameTextBox.Text, "Loading email");
				}
			}
			else
			{
				Globals.Message.ShowError("File doesn't exist: " + EmailFilenameTextBox.Text, "Missing filename");
			}
		}

		void BrowseButton_Click(object sender, EventArgs e)
		{
			using (ZOpenFileDialog dialog = new ZOpenFileDialog())
			{
				dialog.Filter = "Email files (*.eml)|*.eml";
				dialog.CheckPathExists = true;
				dialog.CheckFileExists = true;
				dialog.DefaultExt = "eml";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					EmailFilenameTextBox.Text = dialog.UnmappedFileName;
				}
			}
		}

		#endregion

		#region Time Zones

		void RefreshTimesButton_Click(object sender, EventArgs e)
		{
			DateTime utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;
			DateTime localNow = EnvProxy.Instance.Time.CurrentLocalDateTime;

			UtcTextBox.Text = EnvProxy.Instance.Time.FormatDateTimeWithSeconds(utcNow);
			LocalTimeTextBox.Text = EnvProxy.Instance.Time.FormatDateTimeWithSeconds(localNow);
		}

		#endregion

		void CreateUSTestJobsButton_Click(object sender, EventArgs e)
		{
			var xxxaCarrier = GetOrCreateXXXACarrier();
			var unknCarrier = GetOrCreateUNKNCarrier();
			var auexp = GetOrCreateAUEXP();
			var usimp = GetOrCreateUSIMP();
			var usexp = GetOrCreateUSEXP();
			var auimp = GetOrCreateAUIMP();
			var usfwd = GetOrCreateUSFWD();
			var aufwd = GetOrCreateAUFWD();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_RL_NKLastForeignPort = "AUSYD";
			consol.JK_MasterBillNum = "OB" + ZDateTime.Now.ToString("yyMMddhhmm");
			consol.JK_OA_ShippingLineAddress = xxxaCarrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = aufwd.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = usfwd.MainAddress.PK;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var mostInterestingTransport = consol.MostInterestingTransportForBinding[0];
			mostInterestingTransport.JW_VoyageFlight = ZDateTime.Now.ToString("mmss");
			mostInterestingTransport.JW_Vessel = "APL EMERALD";
			mostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(1);
			mostInterestingTransport.JW_ETA = ZDateTime.Now.AddDays(10);
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = usimp.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = auexp.PK;
			shipment.JS_HouseBill = "HB" + ZDateTime.Now.ToString("yyMMddhhmm");
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_E_DEP = ZDateTime.Now.AddDays(1);
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.JS_GoodsDescription = "GOODS";
			shipment.JS_MarksAndNumbers = "MARKS";
			shipment.JS_OuterPacks = 100;
			shipment.JS_F3_NKPackType = Constants.PkgUnit.Package;
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 1m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			var packLine = shipment.OuterPackLines[0];
			packLine.JL_HarmonisedCode = "1001100010";
			packLine.JL_RN_NKOrigin = Constants.CountryCodes.Australia;

			var amsJob = Factory.New<CusInBondHeader>();
			amsJob.BH_ParentID = consol.PK;
			amsJob.BH_ParentTableCode = consol.TablePrefix;
			amsJob.Synchroniser.Synchronise(true);
			amsJob.BH_OverrideFreightDefaults = true;
			amsJob.BH_CarrierSCAC = "OTT1";
			var amsBill = amsJob.Bills[0];
			amsBill.B0_IssuerCode = "OTT1";
			amsBill.B0_MasterBillNumber = shipment.JS_HouseBill;
			amsBill.SecondaryNotifyParties.DeleteAll();
			var snp = amsBill.SecondaryNotifyParties.AddNew();
			snp.CY_Data = "OTT1";
			snp.CY_Order = 1;

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_JS = shipment.PK;
			importDeclaration.ShipmentSynchroniser.Synchronise(true);
			importDeclaration.JE_OverrideFreightDefaults = true;
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			importDeclaration.US_EnableENS = true;
			importDeclaration.IOROrgPK = usimp.PK;
			importDeclaration.JE_MasterBill = consol.JK_MasterBillNum;
			importDeclaration.JE_MasterBillIssuerSCAC = "OTT1";
			importDeclaration.US_SchDEntry = importDeclaration.US_SchDArrival;
			importDeclaration.JE_HouseBill = shipment.JS_HouseBill;
			importDeclaration.JE_HouseBillIssuerSCAC = "OTT1";
			importDeclaration.US_DestinationState = "CA";
			importDeclaration.US_US_NKLocationOfGoods = "W014";
			importDeclaration.US_7501Purchased = Enterprise.Customs.US.Business.YesNoDefaultList.Codes.No;
			importDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			var importInvoice = importDeclaration.Invoices.AddNew();
			importInvoice.JZ_InvoiceNumber = "IMPINV" + ZDateTime.Now.ToString("yyMMddhhmm");
			importInvoice.JZ_InvoiceAmount = 1000m;
			importInvoice.JZ_InvoiceDate = ZDateTime.Now.Date;
			importInvoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.UnitedStates;
			var importInvoiceLine = importInvoice.JobComInvoiceLines.AddNew();
			importInvoiceLine.JI_Tariff = "1001100010";
			importInvoiceLine.JI_LinePrice = 1000m;
			importInvoiceLine.JI_InvoiceQuantity = 1000m;
			importInvoiceLine.JI_InvoiceUQ = Constants.Weight.Kilograms;
			importInvoiceLine.JI_CustomsQuantity = 1000m;
			importInvoiceLine.JI_CustomsUnitQty = Constants.Weight.Kilograms;
			importInvoiceLine.US_UC_NKCountryOfExport = Constants.CountryCodes.Australia;
			importInvoiceLine.US_UC_NKCountryOfOrigin = Constants.CountryCodes.Australia;
			importInvoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			importInvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			importDeclaration.MessageInitiator = new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer();
			importDeclaration.DoMerge();

			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_OH_Supplier = usexp.PK;
			exportDeclaration.JE_OH_Importer = auimp.PK;
			exportDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			exportDeclaration.JE_ContainerMode = Constants.ContainerModes.NonContainerised;
			exportDeclaration.US_TariffType = "HTS";
			exportDeclaration.JE_MasterBill = "MB" + ZDateTime.Now.ToString("yyMMddhhmm");
			exportDeclaration.JE_VesselName = "APL EMERALD";
			exportDeclaration.JE_VoyageFlightNo = ZDateTime.Now.ToString("mmss");
			exportDeclaration.JE_DateAtOrigin = ZDateTime.Now;
			exportDeclaration.JE_ExportDate = ZDateTime.Now.AddDays(1);
			exportDeclaration.JE_DateOfArrival = ZDateTime.Now.AddDays(10);
			exportDeclaration.JE_RL_NKOrigin = "USCHI";
			exportDeclaration.JE_RL_NKFinalDestination = "AUBNE";
			exportDeclaration.JE_GoodsDescription = "GOODS";
			exportDeclaration.JE_TotalWeight = 1000m;
			exportDeclaration.JE_TotalWeightUnit = Constants.Weight.Kilograms;
			exportDeclaration.JE_TotalVolume = 1m;
			exportDeclaration.JE_TotalVolumeUnit = Constants.Volume.CubicMetres;
			exportDeclaration.JE_TotalNoOfPacks = 100;
			exportDeclaration.JE_TotalNoOfPacksPackType = ShippingOrPackingingUnitList.Codes.Package;
			exportDeclaration.US_TransportReference = exportDeclaration.JE_MasterBill.Left(30);
			exportDeclaration.JE_OH_Forwarder = usfwd.PK;
			exportDeclaration.JE_OH_ShippingLine = unknCarrier.PK;
			exportDeclaration.US_InbondType = InbondTypeList.Codes.MerchandiseNOTShippedInbond;

			var exportInvoice = exportDeclaration.Invoices.AddNew();
			exportInvoice.JZ_InvoiceNumber = "EXPINV" + ZDateTime.Now.ToString("yyMMddhhmm");
			exportInvoice.JZ_InvoiceAmount = 1000m;
			exportInvoice.JZ_InvoiceDate = ZDateTime.Now.Date;
			exportInvoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.UnitedStates;
			var exportInvoiceLine = exportInvoice.JobComInvoiceLines.AddNew();
			exportInvoiceLine.JI_Tariff = "2001100000";
			exportInvoiceLine.JI_LinePrice = 1000m;
			exportInvoiceLine.JI_InvoiceQuantity = 1000m;
			exportInvoiceLine.JI_InvoiceUQ = Constants.Weight.Kilograms;
			exportInvoiceLine.JI_CustomsQuantity = 1000m;
			exportInvoiceLine.JI_CustomsUnitQty = Constants.Weight.Kilograms;
			exportInvoiceLine.JI_CountryOfOrigin = Constants.CountryCodes.UnitedStates;

			exportDeclaration.MessageInitiator = new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer();
			exportDeclaration.DoMerge();

			var isfJob = Factory.New<CusISFHeader>();
			isfJob.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			isfJob.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			isfJob.BF_TransportMode = Enterprise.Customs.US.ISF.Business.TransportModeCodes.Codes.OceanVesselContainerized;
			isfJob.BF_SCAC = "OTT1";
			isfJob.BF_OH_Importer = usimp.PK;
			isfJob.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			isfJob.BF_OceanBill = "OB" + ZDateTime.Now.ToString("yyMMddhhmm");
			isfJob.MainShipToParty.OrganisationPK = usimp.PK;
			isfJob.BF_BondNumberOrHolder = isfJob.BF_ImporterCode;
			isfJob.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
			isfJob.BF_BondType = BondTypeList.Codes.SingleTransactionBond;
			isfJob.BF_SuretyCode = "891";
			isfJob.BF_BondReferenceNumber = ZDateTime.Now.ToString("yyMMddhhmm");
			isfJob.BuyingParty.OrganisationPK = usimp.PK;
			isfJob.SellingParty.OrganisationPK = auexp.PK;
			isfJob.Consolidator.OrganisationPK = xxxaCarrier.PK;
			isfJob.StuffingLocation.OrganisationPK = xxxaCarrier.PK;
			var isfManufacturerAddress = isfJob.ManufacturerAddresses.AddNew();
			isfManufacturerAddress.OrganisationPK = auexp.PK;
			var isfLine = isfJob.Lines.AddNew();
			isfLine.BL_HarmonisedNum = "1001100010";
			isfLine.BL_RN_NKGoodsOrigin = Constants.CountryCodes.Australia;
			isfLine.BL_ManufacturerDocAddressPK = isfManufacturerAddress.PK;
			Factory.Save();

			new EntrySummaryMessageBuilder(importDeclaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, true).PopulateMessage();
			AESMessageManager.SubmitToCustoms(new AESDeclaration(exportDeclaration));
			var amsSendingBill = new MessageSendingObject(amsBill.MovementDetail, ActionCode.Creating);
			new ACEAMSMessageBuilder(amsSendingBill, ActionCode.Creating).PopulateMessage();
			new ImporterSecurityFilingMessageBuilder<ACEInputBlockControlGenerator, AABIInputB, AABIInputY>(isfJob, UpdateActionCode.Add).PopulateMessage();
			Factory.Save();
			var builder = new ZStringBuilder();
			builder.Append("The following jobs were created:");
			builder.Append("Consol: " + consol.JK_UniqueConsignRef);
			builder.Append("Shipment: " + shipment.JS_UniqueConsignRef);
			builder.Append("Import Declaratation: " + importDeclaration.JE_DeclarationReference);
			builder.Append("Export Declaratation: " + exportDeclaration.JE_DeclarationReference);
			builder.Append("ISF: " + isfJob.BF_JobReference);
			Globals.Message.Show(builder.ToStringWithNewLineBetweenAppends(), "JOBS CREATED", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		OrgHeader GetOrCreateXXXACarrier()
		{
			OrgHeader shippingLine = null;
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, "XXXA");
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);
			var cccOrgCusCode = Factory.LoadTop1<OrgCusCode>(query);
			if (cccOrgCusCode != null)
			{
				shippingLine = cccOrgCusCode.Header;
			}
			if (shippingLine == null)
			{
				shippingLine = Factory.New<OrgHeader>();
				shippingLine.OH_Code = GetUniqueCode("USCAR");
				shippingLine.OH_IsShippingProvider = true;
				shippingLine.OH_IsShippingLine = true;
				shippingLine.OH_FullName = "USCAR COMPANY NAME";
				shippingLine.OH_RL_NKClosestPort = "USCHI";
				var mainAddress = shippingLine.MainAddress;
				mainAddress.OA_Address1 = "USCAR ADDRESS 1";
				mainAddress.OA_Address1 = "USCAR ADDRESS 2";
				mainAddress.OA_City = "USCARCITY";
				mainAddress.OA_State = "IL";
				mainAddress.OA_Phone = "+1 (925) 688-2600";
				mainAddress.OA_PostCode = "60148";
				cccOrgCusCode = shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "XXXA", Constants.CountryCodes.UnitedStates);
			}
			return shippingLine;
		}

		OrgHeader GetOrCreateUNKNCarrier()
		{
			OrgHeader shippingLine = null;
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, "UNKN");
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);
			var cccOrgCusCode = Factory.LoadTop1<OrgCusCode>(query);
			if (cccOrgCusCode != null)
			{
				shippingLine = cccOrgCusCode.Header;
			}
			if (shippingLine == null)
			{
				shippingLine = Factory.New<OrgHeader>();
				shippingLine.OH_Code = GetUniqueCode("AUCAR");
				shippingLine.OH_IsShippingProvider = true;
				shippingLine.OH_IsShippingLine = true;
				shippingLine.OH_FullName = "AUCAR COMPANY NAME";
				shippingLine.OH_RL_NKClosestPort = "AUBNE";
				var mainAddress = shippingLine.MainAddress;
				mainAddress.OA_Address1 = "AUCAR ADDRESS 1";
				mainAddress.OA_Address1 = "AUCAR ADDRESS 2";
				mainAddress.OA_City = "AUCARCITY";
				mainAddress.OA_State = "QLD";
				mainAddress.OA_Phone = "+61 (7) 3344-4744";
				mainAddress.OA_PostCode = "4127";
				cccOrgCusCode = shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "UNKN", Constants.CountryCodes.UnitedStates);
			}
			return shippingLine;
		}

		OrgHeader GetOrCreateUSFWD()
		{
			OrgHeader usfwd = null;
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, "635298741");
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
			var dunOrgCusCode = Factory.LoadTop1<OrgCusCode>(query);
			if (dunOrgCusCode != null)
			{
				usfwd = dunOrgCusCode.Header;
			}
			if (usfwd == null)
			{
				usfwd = Factory.New<OrgHeader>();
				usfwd.OH_Code = GetUniqueCode("USFWD");
				usfwd.OH_IsForwarder = true;
				usfwd.OH_FullName = "USFWD COMPANY NAME";
				usfwd.OH_RL_NKClosestPort = "USCHI";
				var mainAddress = usfwd.MainAddress;
				mainAddress.OA_Address1 = "USFWD ADDRESS 1";
				mainAddress.OA_Address1 = "USFWD ADDRESS 2";
				mainAddress.OA_City = "USFWDCITY";
				mainAddress.OA_State = "IL";
				mainAddress.OA_Phone = "+1 (123) 443-4533";
				mainAddress.OA_PostCode = "60035";
				dunOrgCusCode = usfwd.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "635298741", Constants.CountryCodes.UnitedStates);
				var contact = usfwd.Contacts.AddNew();
				contact.OC_ContactName = "USFWD CONTACT";
				var document = contact.Documents.AddNew();
				document.OD_DefaultContact = true;
				document.OD_DocumentGroup = ContactType.All.ToString();
				var agentPorts = usfwd.AppointedAgentPorts.AddNew();
				agentPorts.O5_PortOrCountry = "USCHI";
				agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
				agentPorts.O5_SeaAgentStatus = AgentStatusList.Codes.Published;
				agentPorts.O5_AirAgentStatus = AgentStatusList.Codes.Published;
				agentPorts.O5_RoadAgentStatus = AgentStatusList.Codes.Published;
				agentPorts.O5_RailAgentStatus = AgentStatusList.Codes.Published;
				agentPorts.O5_OA_AgentOfficeAddress = usfwd.MainAddress.PK;
			}
			return usfwd;
		}

		OrgHeader GetOrCreateAUFWD()
		{
			OrgHeader aufwd = null;
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, "97006926998");
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber);
			var abnOrgCusCode = Factory.LoadTop1<OrgCusCode>(query);
			if (abnOrgCusCode != null)
			{
				aufwd = abnOrgCusCode.Header;
			}
			if (aufwd == null)
			{
				aufwd = Factory.New<OrgHeader>();
				aufwd.OH_Code = GetUniqueCode("AUFWD");
				aufwd.OH_IsForwarder = true;
				aufwd.OH_FullName = "AUFWD COMPANY NAME";
				aufwd.OH_RL_NKClosestPort = "USCHI";
				var mainAddress = aufwd.MainAddress;
				mainAddress.OA_Address1 = "AUFWD ADDRESS 1";
				mainAddress.OA_Address1 = "AUFWD ADDRESS 2";
				mainAddress.OA_City = "AUFWDCITY";
				mainAddress.OA_State = "BNE";
				mainAddress.OA_Phone = "+64 (7) 8654-6952";
				mainAddress.OA_PostCode = "4058";
				abnOrgCusCode = aufwd.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "97006926998", Constants.CountryCodes.UnitedStates);
				var agentPorts = aufwd.AppointedAgentPorts.AddNew();
				agentPorts.O5_PortOrCountry = "AUBNE";
				agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
				agentPorts.O5_SeaAgentStatus = AgentStatusList.Codes.Published;
				agentPorts.O5_AirAgentStatus = AgentStatusList.Codes.Published;
				agentPorts.O5_RoadAgentStatus = AgentStatusList.Codes.Published;
				agentPorts.O5_RailAgentStatus = AgentStatusList.Codes.Published;
				agentPorts.O5_OA_AgentOfficeAddress = aufwd.MainAddress.PK;
			}
			return aufwd;
		}

		OrgHeader GetOrCreateUSIMP()
		{
			OrgHeader usimp = null;
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, "91-013199000");
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
			var einOrgCusCode = Factory.LoadTop1<OrgCusCode>(query);
			if (einOrgCusCode != null)
			{
				usimp = einOrgCusCode.Header;
			}
			if (usimp == null)
			{
				usimp = Factory.New<OrgHeader>();
				usimp.OH_Code = GetUniqueCode("USIMP");
				usimp.OH_IsConsignee = true;
				usimp.OH_FullName = "USIMP COMPANY NAME";
				usimp.OH_RL_NKClosestPort = "USLAX";
				var mainAddress = usimp.MainAddress;
				mainAddress.OA_Address1 = "USIMP ADDRESS 1";
				mainAddress.OA_Address1 = "USIMP ADDRESS 2";
				mainAddress.OA_City = "USIMPCITY";
				mainAddress.OA_State = "CA";
				mainAddress.OA_Phone = "+1 (801) 232-2230";
				mainAddress.OA_PostCode = "90008";
				einOrgCusCode = usimp.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000", Constants.CountryCodes.UnitedStates);
			}
			return usimp;
		}

		OrgHeader GetOrCreateAUIMP()
		{
			OrgHeader auimp = null;
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, "45-4567896");
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.EDISiteID);
			var einOrgCusCode = Factory.LoadTop1<OrgCusCode>(query);
			if (einOrgCusCode != null)
			{
				auimp = einOrgCusCode.Header;
			}
			if (auimp == null)
			{
				auimp = Factory.New<OrgHeader>();
				auimp.OH_Code = GetUniqueCode("AUIMP");
				auimp.OH_IsConsignee = true;
				auimp.OH_FullName = "AUIMP COMPANY NAME";
				auimp.OH_RL_NKClosestPort = "AUSYD";
				var mainAddress = auimp.MainAddress;
				mainAddress.OA_Address1 = "AUIMP ADDRESS 1";
				mainAddress.OA_Address1 = "AUIMP ADDRESS 2";
				mainAddress.OA_City = "AUIMPCITY";
				mainAddress.OA_State = "NSW";
				mainAddress.OA_Phone = "+61 (2) 9043-1212";
				mainAddress.OA_PostCode = "3421";
				einOrgCusCode = auimp.CustomsCodes.AddNew(OrgCusCode.CodeTypes.EDISiteID, "45-4567896", Constants.CountryCodes.UnitedStates);
			}
			return auimp;
		}

		OrgHeader GetOrCreateAUEXP()
		{
			OrgHeader auexp = null;
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, "AUAUECOM2AUE");
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.ManufacturerID);
			var midOrgCusCode = Factory.LoadTop1<OrgCusCode>(query);
			if (midOrgCusCode != null)
			{
				auexp = midOrgCusCode.Header;
			}
			if (auexp == null)
			{
				auexp = Factory.New<OrgHeader>();
				auexp.OH_Code = GetUniqueCode("AUEXP");
				auexp.OH_IsConsignor = true;
				auexp.OH_FullName = "AUEXP COMPANY NAME";
				auexp.OH_RL_NKClosestPort = "AUSYD";
				var mainAddress = auexp.MainAddress;
				mainAddress.OA_Address1 = "AUEXP ADDRESS 1";
				mainAddress.OA_Address1 = "AUEXP ADDRESS 2";
				mainAddress.OA_City = "AUEXPCITY";
				mainAddress.OA_State = "NSW";
				mainAddress.OA_Phone = "+61 (2) 9845-6576";
				mainAddress.OA_PostCode = "2000";
				midOrgCusCode = auexp.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUAUECOM2AUE", Constants.CountryCodes.UnitedStates);
				midOrgCusCode.OK_OA_PremisesAddress = mainAddress.PK;
			}
			return auexp;
		}

		OrgHeader GetOrCreateUSEXP()
		{
			OrgHeader usexp = null;
			var query = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, "USUSEXP2USE");
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.ManufacturerID);
			var midOrgCusCode = Factory.LoadTop1<OrgCusCode>(query);
			if (midOrgCusCode != null)
			{
				usexp = midOrgCusCode.Header;
			}
			if (usexp == null)
			{
				usexp = Factory.New<OrgHeader>();
				usexp.OH_Code = GetUniqueCode("USEXP");
				usexp.OH_IsConsignor = true;
				usexp.OH_FullName = "USEXP COMPANY NAME";
				usexp.OH_RL_NKClosestPort = "USLAX";
				var mainAddress = usexp.MainAddress;
				mainAddress.OA_Address1 = "USEXP ADDRESS 1";
				mainAddress.OA_Address1 = "USEXP ADDRESS 2";
				mainAddress.OA_City = "USEXPCITY";
				mainAddress.OA_State = "CA";
				mainAddress.OA_Phone = "+1 (112) 123-2121";
				mainAddress.OA_PostCode = "90007";
				midOrgCusCode = usexp.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "USUSEXP2USE", Constants.CountryCodes.UnitedStates);
				midOrgCusCode.OK_OA_PremisesAddress = mainAddress.PK;
			}
			return usexp;
		}

		ZString GetUniqueCode(ZString code)
		{
			var result = code;
			while (Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, result) != null)
			{
				result = code + new Random().Next(1000000).ToString();
			}
			return result;
		}

		void BillingDecodeDataButton_Click(object sender, EventArgs e)
		{
			try
			{
				var data = BillingEncodedDataTextBox.Text.Trim();
				const string usageTransactionHeader = "<UsageTransaction xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.edi.com.au/EnterpriseService/#Usage_";
				if (Encoding.UTF8.GetString(BillingDataEncryptor.Decrypt(data)).Contains(usageTransactionHeader))
				{
					BillingDataTextBox.Text = BillingManager.DecryptUsageTransaction(data).ToString().Replace(", ", Environment.NewLine);
				}
				else
				{
					var billingTransaction = BillingManager.DecryptTransaction(BillingEncodedDataTextBox.Text.Trim(), BillingManager.CurrentSchemaVersion);
					BillingDataTextBox.Text = Regex.Replace(billingTransaction.ToString(), @",\s*", System.Environment.NewLine);
				}
			}
			catch (Exception ex) when (ex is ArgumentNullException || ex is FormatException || ex is CryptographicException)
			{
				BillingDataTextBox.Text = "*** ERROR ***" + System.Environment.NewLine + System.Environment.NewLine + ex.Message;
			}
		}

		#region FeatureControl
		void FeatureControlLoadButton_Click(object sender, EventArgs e)
		{
			FeatureControlContentTextBox.Text = "";
			using (new DisposableAction(() => FeatureControlLoadButton.Enabled = false,
						() => FeatureControlLoadButton.Enabled = true))
			{
				using var ms = new MemoryStream(Convert.FromBase64String(WebDataRegistry.Instance.FeatureControlRuleContent.Value));
				using var zipStream = new GZipStream(ms, CompressionMode.Decompress);
				using (var resultStream = new MemoryStream())
				{
					zipStream.CopyTo(resultStream);
					var xml = Encoding.UTF8.GetString(resultStream.ToArray());
					FeatureControlContentTextBox.Text = xml;
				}
			}
		}

		void FeatureControlSaveButton_Click(object sender, EventArgs e)
		{
			using (var ms = new MemoryStream())
			{
				using (var zipStream = new GZipStream(ms, CompressionMode.Compress))
				{
					var xmlBytes = Encoding.UTF8.GetBytes(FeatureControlNewContentTextBox.Text);
					zipStream.Write(xmlBytes, 0, xmlBytes.Length);
				}
				if (ObjectFactory.Get<IFeatureControlRuleRepository>().SaveFeatureControlRuleContent(ms.ToArray()))
				{
					Globals.Message.Show("OK");
				}
				else
				{
					Globals.Message.Show("Invalid XML");
				}
			}
		}

		void FeatureControlParameterButton_Click(object sender, EventArgs e)
		{
			if (!DateTime.TryParseExact(FeatureControlUtcNowTextbox.Text, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var userInput))
			{
				Globals.Message.Show($"Invalid date (dd/MM/yyyy HH:mm:ss), {FeatureControlUtcNowTextbox.Text}");
				return;
			}

			FeatureControlParameterTextBox.Text = "";
			using (new DisposableAction(() => FeatureControlParameterButton.Enabled = false,
					() => FeatureControlParameterButton.Enabled = true))
			using (ObjectCache.OverrideDateTimeProvider(new FeatureControlDateTimeProvider() { CurrentUtcDateTimeOverride = userInput }))
			{
				var mgr = ObjectFactory.Get<IFeatureControlManager>();
				FeatureControlParameterTextBox.Text = mgr.GetFeatureData(FeatureControlCodeTextbox.Text)?.Parameter;
				if (!string.IsNullOrWhiteSpace(FeatureControlParameterTextBox.Text))
				{
					try
					{
						_ = JsonDocument.Parse(FeatureControlParameterTextBox.Text);
					}
					catch (Exception ex)
					{
						Globals.Message.Show($"Invalid json string - {ex.Message}");
					}
				}
			}
		}

		class FeatureControlDateTimeProvider : Enterprise.Environment.DateTimeProvider
		{
			public DateTime CurrentUtcDateTimeOverride { get; set; }

			public override DateTime CurrentUtcDateTime => CurrentUtcDateTimeOverride;
		}

		void FeatureControlSampleButton_Click(object sender, EventArgs e)
		{
			var xml = @"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>2024-07-15T00:28:17.873Z</TimestampUtc>
  <Rules>
    <Rule>
      <FCM_FeatureControlCode>AUTHLOGIN</FCM_FeatureControlCode>
      <FCR_RuleType>CLI</FCR_RuleType>
      <FCR_StartDateUtc>2024-07-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2024-07-31T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>{
  ""UserSettings"": {
    ""Username"": ""john_doe_client_name"",
    ""Email"": ""john.doe@example.com"",
    ""Preferences"": {
      ""Theme"": ""dark"",
      ""Language"": ""en-US"",
      ""Notifications"": {
        ""Email"": true,
        ""SMS"": false
      }
    }
  }
}</FCR_Parameters>
    </Rule>
    <Rule>
      <FCM_FeatureControlCode>AUTHLOGIN</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2024-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2024-12-31T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>{
  ""UserSettings"": {
    ""Username"": ""john_doe_global_name"",
    ""Email"": ""john.doe@example.com"",
    ""Preferences"": {
      ""Theme"": ""dark"",
      ""Language"": ""en-US"",
      ""Notifications"": {
        ""Email"": true,
        ""SMS"": false
      }
    }
  }
}</FCR_Parameters>
    </Rule>
  </Rules>
</FeatureControl>";
			Globals.Message.Show(xml);
		}

		#endregion

#endif
	}
}

