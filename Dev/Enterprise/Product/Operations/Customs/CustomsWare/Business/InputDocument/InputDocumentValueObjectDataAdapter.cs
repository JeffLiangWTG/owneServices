using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.CustomsWare.Business.XSD;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class InputDocumentValueObjectDataAdapter : ValueObjectDataAdapter<BaseJobDeclaration, InputDocument>, ICustomsWareInputDocumentValueObjectDataAdapter
	{
		#region Constructor

		protected InputDocumentValueObjectDataAdapter()
			: base()
		{
		}

		public static InputDocumentValueObjectDataAdapter New()
		{
			return New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public static InputDocumentValueObjectDataAdapter New(string countryCode)
		{
			InputDocumentValueObjectDataAdapter result = null;

			if (countryCode == Core.Constants.CountryCodes.Germany)
			{
				result = (InputDocumentValueObjectDataAdapter)ObjectFactory.Get("CustomWare.ICustomsWareDEInputDocumentValueObjectDataAdapter");
			}
			else if (countryCode == Core.Constants.CountryCodes.UnitedArabEmirates)
			{
				result = (InputDocumentValueObjectDataAdapter)ObjectFactory.Get("CustomWare.ICustomsWareAEInputDocumentValueObjectDataAdapter");
			}
			else if (countryCode == Core.Constants.CountryCodes.Sweden)
			{
				result = (InputDocumentValueObjectDataAdapter)ObjectFactory.Get("CustomWare.ICustomsWareSEInputDocumentValueObjectDataAdapter");
			}
			else if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(countryCode))
			{
				result = (InputDocumentValueObjectDataAdapter)ObjectFactory.Get("CustomWare.ICustomsWareEUInputDocumentValueObjectDataAdapter");
			}
			else
			{
				result = new InputDocumentValueObjectDataAdapter();
			}

			return result;
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(BaseJobDeclaration bizObj, InputDocument constructedValueObject, IValueObjectExportContext context)
		{
			CreateConsignment(bizObj, constructedValueObject, context);
		}

		void CreateConsignment(BaseJobDeclaration dec, InputDocument inputDocument, IValueObjectExportContext context)
		{
			inputDocument.ConsignmentList = new ConsignmentList();
			var cons = inputDocument.ConsignmentList.Consignment.AddNew();
			cons.Command = "UPDATE";
			PopulateConsignmentHeader(dec, cons.ConsignmentHeader, context);
		}

		protected virtual void PopulateConsignmentHeader(BaseJobDeclaration dec, ConsignmentHeader consHeader, IValueObjectExportContext context)
		{
			consHeader.ConsignmentReference = dec.JE_DeclarationReference;
			consHeader.SiteID = CustomsWareRegistry.Instance.CustomsWareSiteID.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var date = consHeader.ConsignmentDate.AddNew();
			date.DateTime = ZDateTime.Today.ToISO8601String();

			consHeader.Terms.TermsCode = dec.JE_ShipmentIncoTerm;

			var transportMode = GetNumericEquivalent(dec.JE_TransportMode);
			var vesselNAT = dec.Vessel != null ? dec.Vessel.RV_RN_NKCountryOfReg : ZString.Empty;
			if (!string.IsNullOrEmpty(transportMode) || vesselNAT != "" || dec.JE_VesselName != "")
			{
				var transport = consHeader.Transport.AddNew();
				CreateTransport(transport, XSD.TransportTransportType.Border, transportMode, dec.JE_VesselName, vesselNAT);
			}

			CreateReference(consHeader.Reference.AddNew(), "HWB", dec.JE_HouseBill);
			CreateReference(consHeader.Reference.AddNew(), "MWB", dec.JE_MasterBill);
			CreateCountry(consHeader.Country.AddNew(), "Dispatch", dec.JE_GoodsOrigin != ZString.Empty ? dec.JE_GoodsOrigin : dec.JE_RL_NKOrigin.Left(2));
			CreateCountry(consHeader.Country.AddNew(), "Destination", dec.JE_GoodsDestination != ZString.Empty ? dec.JE_GoodsDestination : dec.JE_RL_NKFinalDestination.Left(2));

			CreatePort(consHeader, "ConsignmentOrigin", dec.JE_RL_NKOrigin);
			CreatePort(consHeader, "ConsignmentDestination", dec.JE_RL_NKFinalDestination);
			CreatePort(consHeader, "FirstLoading", GetFirstLoadingPort(dec));
			CreatePort(consHeader, "LastLoading", dec.JE_RL_NKPortOfLoading);
			CreatePort(consHeader, "FirstArrival", GetFirstArrivalPort(dec));
			CreatePort(consHeader, "Discharge", dec.JE_RL_NKPortOfArrival);

			CreateParty(consHeader.Party.AddNew(), "Consignor", dec.Supplier, dec.SupplierDocumentaryAddress);
			CreateParty(consHeader.Party.AddNew(), "Consignee", dec.Importer, dec.ImporterDocumentaryAddress);
			consHeader.GoodsDescription = dec.JE_GoodsDescription;
			CreateMeasure(consHeader.Measure.AddNew(), XSD.ApplicationUnitsOfMeasureUOMCode.DocumentPieces, dec.JE_TotalNoOfPacks.ToString());

			var weightInKG = Core.Constants.Weight.ConvertSafe(dec.JE_TotalWeight, dec.JE_TotalWeightUnit, Core.Constants.Weight.Kilograms);
			CreateMeasure(consHeader.Measure.AddNew(), XSD.ApplicationUnitsOfMeasureUOMCode.DocumentGrossWeight, weightInKG.ToString());

			CreateContainers(dec, consHeader, context);
		}

		ZString GetFirstLoadingPort(BaseJobDeclaration dec)
		{
			string result = string.Empty;

			var transports = ((IRoutingSupport)dec).TransportsIncludingRelated;
			if (transports.Count > 0)
			{
				var transportHelp = new TransportOrderHelper(transports);
				var firstLoadingPort = transportHelp.FirstLegMatching(transport => transport.JW_RL_NKDiscPort.Left(2) != transport.JW_RL_NKLoadPort.Left(2));
				if (firstLoadingPort != null)
				{
					result = firstLoadingPort.JW_RL_NKLoadPort;
				}
			}
			return result;
		}

		ZString GetFirstArrivalPort(BaseJobDeclaration dec)
		{
			string result = string.Empty;
			var transports = ((IRoutingSupport)dec).TransportsIncludingRelated;
			if (transports.Count > 0)
			{
				var transportHelp = new TransportOrderHelper(transports);
				var firstArrivalPort = transportHelp.FirstLegMatching(transport => transport.JW_RL_NKLoadPort.Left(2) == dec.JE_RL_NKPortOfArrival.Left(2));
				if (firstArrivalPort != null)
				{
					result = firstArrivalPort.JW_RL_NKLoadPort;
				}
			}
			return result;
		}

		void CreateContainers(BaseJobDeclaration dec, ConsignmentHeader consHeader, IValueObjectExportContext context)
		{
			foreach (BaseCusContainer cont in dec.CusContainers)
			{
				var xsdcontainer = consHeader.Container.ContainerItem.AddNew();
				xsdcontainer.ContainerRef = cont.CO_ContainerNumber;
				xsdcontainer.ContainerSealNumber = cont.CO_Seal;
				xsdcontainer.ContainerSize = cont.CO_ContainerSize;
				if (cont.Container != null)
				{
					xsdcontainer.ContainerType = cont.Container.RC_Code;
				}
			}
		}

		protected void CreateTransport(XSD.Transport transport, TransportTransportType transportType, ZString transportMode, ZString conveyance, ZString conveyanceNationality)
		{
			transport.TransportType = transportType;

			if (!transportMode.IsEmpty)
			{
				transport.TPMode.CodeType = XSD.TPModeCodeType.NUM;
				transport.TPMode.Text = new string[] { transportMode };
				transport.TPModeSpecified = true;
			}

			if (!conveyance.IsEmpty)
			{
				transport.Conveyance = conveyance;
				transport.ConveyanceSpecified = true;
			}

			if (!conveyanceNationality.IsEmpty)
			{
				transport.ConveyanceNat.CodeType = XSD.ConveyanceNatCodeType.ISO;
				transport.ConveyanceNat.Text = new string[] { conveyanceNationality };
				transport.ConveyanceNatSpecified = true;
			}
		}

		void CreateCountry(Country country, ZString countryType, ZString countryCode)
		{
			country.CountryType = countryType;
			country.CodeType = XSD.CountryCodeType.ISO;
			country.Text = new string[] { countryCode };
			country.IsSpecified = !countryCode.IsEmpty;
		}

		void CreatePort(ConsignmentHeader consHeader, ZString portType, ZString unloco)
		{
			if (!unloco.IsEmpty)
			{
				Port port = consHeader.Port.AddNew();
				port.CodeType = XSD.PortCodeType.UNLOC;
				port.PortCountry = unloco.Left(2);
				port.PortType = portType;
				port.Text = new string[] { unloco };
			}
		}

		protected virtual void CreateParty(Party party, ZString partyType, OrgHeader org, IDocAddress address)
		{
			if (org != null)
			{
				party.PartyType = partyType;
				party.NameAddress.Name = org.OH_FullName.Left(35);

				if (address == null)
				{
					address = org.MainAddress;
				}

				party.NameAddress.Address1 = address.E2_Address1.Left(35);
				party.NameAddress.Address2 = address.E2_Address2.Left(35);
				party.NameAddress.Address3 = address.E2_City;
				party.NameAddress.PostCode = address.E2_Postcode.Left(9);
				party.NameAddress.Country.CodeType = XSD.CountryCodeType.ISO;
				party.NameAddress.Country.CountryType = partyType;
				party.NameAddress.Country.Text = new string[] { address.E2_PortCode.Left(2) };
				party.NameAddress.Country.IsSpecified = !address.E2_PortCode.IsEmpty;
			}
		}

		protected void CreateReference(Reference reference, ZString refCode, ZString referenceValue)
		{
			if (!referenceValue.IsEmpty)
			{
				reference.RefCode = refCode;
				reference.RefText = referenceValue;
				reference.IsSpecified = !referenceValue.IsEmpty;
			}
		}

		void CreateMeasure(ApplicationUnitsOfMeasure measure, ApplicationUnitsOfMeasureUOMCode uomCode, ZString value)
		{
			measure.UOMCode = uomCode;
			measure.UOMValue.Value = value;
		}

		#endregion

		#region Import

		#region Find Declaration

		protected override BaseJobDeclaration FindBusinessObject(InputDocument value, IValueObjectImportContext context)
		{
			BaseJobDeclaration result = null;

			//search by consignment reference first
			result = FindJobByDeclaration(value, context);

			if (result == null)
			{
				//search by event reference
				result = FindJobByEvent(value, context);
			}

			if (result == null)
			{
				result = FindJobByAttachment(value, context);
			}

			return result;
		}

		protected override bool MinimumRequirementsMetForNewImport(InputDocument shipmentValue, IValueObjectImportContext importContext)
		{
			return false;//never create new
		}

		BaseJobDeclaration FindJobByDeclaration(InputDocument inputDocument, IValueObjectImportContext context)
		{
			BaseJobDeclaration result = null;

			var xsddec = inputDocument.DeclarationList.Cast<Declaration>().FirstOrDefault();
			if (xsddec != null && !xsddec.DeclarationHeader.DocumentRef.IsEmpty)
			{
				var declarations = context.Factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, xsddec.DeclarationHeader.DocumentRef));
				result = declarations.Cast<BaseJobDeclaration>().FirstOrDefault(x => IsDeclarationOwnedByCurrentCompany(x));
			}

			return result;
		}

		BaseJobDeclaration FindJobByEvent(InputDocument inputDocument, IValueObjectImportContext context)
		{
			BaseJobDeclaration result = null;

			XSD.Event latestEvent = GetLatestDeclarationEvent(inputDocument);

			if (latestEvent != null && !latestEvent.EventRef.IsEmpty && !latestEvent.EventSubRef.IsEmpty)
			{
				var bgmReference = BGMReference(latestEvent.EventRef, latestEvent.EventSubRef);
				var entries = context.Factory.Load<CusEntryHeader>(new ZQuery(CusEntryHeaderSchema.CH_BGMReference, bgmReference));

				var entry = entries.Cast<CusEntryHeader>().FirstOrDefault(x => IsDeclarationOwnedByCurrentCompany(x.Declaration));

				result = entry != null ? entry.Declaration : null;
			}

			return result;
		}

		BaseJobDeclaration FindJobByAttachment(InputDocument inputDocument, IValueObjectImportContext context)
		{
			BaseJobDeclaration result = null;

			var xsdattachment = inputDocument.AttachmentList.Cast<Attachment>().FirstOrDefault();
			if (xsdattachment != null)
			{
				var ownerItem = xsdattachment.OwnerList.Cast<OwnerItem>().FirstOrDefault();
				if (ownerItem != null && !ownerItem.DocumentRef.IsEmpty)
				{
					var declarations = context.Factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, ownerItem.DocumentRef));
					result = declarations.Cast<BaseJobDeclaration>().FirstOrDefault(x => IsDeclarationOwnedByCurrentCompany(x));
				}
			}

			return result;
		}

		bool IsDeclarationOwnedByCurrentCompany(BaseJobDeclaration declaration)
		{
			return declaration.Company != null && declaration.Company.PK == GlbCompany.CurrentCompany.PK;
		}

		XSD.Event GetLatestDeclarationEvent(InputDocument inputDocument)
		{
			ZDateTime latestEventTime = ZDateTime.MinSmallDateTimeValue;

			XSD.Event result = null;

			foreach (XSD.Event xsdevent in inputDocument.EventList)
			{
				if (xsdevent.OwnerType == "Declaration")
				{
					ZDateTime xsdEventDateTime;

					if (ZDateTime.TryParseISO8601Date(xsdevent.EventDateTime, out xsdEventDateTime))
					{
						if (latestEventTime < xsdEventDateTime)
						{
							result = xsdevent;
						}
					}
				}
			}

			return result;
		}

		#endregion

		protected virtual void ImportDeclaration(BaseJobDeclaration dec, Declaration xsdDec)
		{
			foreach (ApplicationLocation location in xsdDec.DeclarationHeader.Location)
			{
				if (location.LocationType.ToUpper() == "GOODSLOCATION")
				{
					dec.JE_LocationOfGoods = location.LocationCode.Left(dec.JE_LocationOfGoodsInfo.MaxLength);
					break;
				}
			}

			var transportModeTranslator = new EU.Business.TransportModeTranslator();
			foreach (XSD.Transport transport in xsdDec.DeclarationHeader.Transport)
			{
				if (transport.TransportType == TransportTransportType.Inland)
				{
					var tpIdentifier = transport.TPIdentifier.Left(dec.JE_TransportModeInlandInfo.MaxLength);
					dec.JE_TransportModeInland = transportModeTranslator.TranslateToCargoWiseCode(tpIdentifier, true);
				}
				else if (transport.TransportType == TransportTransportType.Border && transport.ConveyanceNat.Text != null)
				{
					dec.JE_RN_NKTransportNationality = transport.ConveyanceNat.Text[0];
				}
			}
		}

		protected override void ImportFromValueObjectCore(BaseJobDeclaration bizObj, InputDocument value, IValueObjectImportContext context)
		{
			foreach (Declaration xsddec in value.DeclarationList)
			{
				ImportDeclaration(bizObj, xsddec);
				var entry = FindEntry(bizObj, xsddec.DeclarationHeader.DocumentRef, xsddec.DeclarationHeader.DocumentSubRef);
				if (entry == null)
				{
					entry = bizObj.ActiveEntryHeaders.AddNew();
					entry.CH_BGMReference = BGMReference(xsddec.DeclarationHeader.DocumentRef, xsddec.DeclarationHeader.DocumentSubRef);
				}
				ImportEntry(bizObj, entry, xsddec, context);
			}

			XSD.Event latestDeclarationEvent = GetLatestDeclarationEvent(value);
			if (latestDeclarationEvent != null)
			{
				var entry = FindEntry(bizObj, latestDeclarationEvent.EventRef, latestDeclarationEvent.EventSubRef);
				if (entry != null)
				{
					ImportEvent(entry, latestDeclarationEvent, context);
				}
			}

			ImportAttachments(bizObj, value, context);
		}

		CusEntryHeader FindEntry(BaseJobDeclaration dec, ZString reference, ZString subReference)
		{
			foreach (CusEntryHeader entry in dec.ActiveEntryHeaders)
			{
				if (entry.CH_BGMReference == BGMReference(reference, subReference))
				{
					return entry;
				}
			}

			return null;
		}

		protected virtual void ImportEntry(BaseJobDeclaration dec, CusEntryHeader entry, Declaration xsddec, IValueObjectImportContext context)
		{
			var xsdheader = xsddec.DeclarationHeader;

			var decType = GetDeclarationType(xsdheader);

			context.SetPropertyInfoValue(entry.CH_MessageTypeInfo, decType);

			foreach (Reference xsdreference in xsddec.DeclarationHeader.Reference)
			{
				if (xsdreference.RefCode == "MRN")
				{
					entry.MovementReferenceNumberSetter(xsdreference.RefText, ZDateTime.Empty);
				}
			}

			ImportEntryLines(dec, entry, xsddec, context);
		}

		protected virtual ZString GetDeclarationType(DeclarationHeader xsdDeclarationHeader)
		{
			ZString result = xsdDeclarationHeader.DeclarationType.Right(3);

			if (result == "CIM")
			{
				result = "IMP";
			}
			else if (result == "CEX")
			{
				result = "EXP";
			}

			return result;
		}

		void ImportEntryLines(BaseJobDeclaration dec, CusEntryHeader entry, Declaration xsddec, IValueObjectImportContext context)
		{
			//update
			entry.MergedLines.RemoveAndDeleteAll();
			entry.Charges.RemoveAndDeleteAll();

			var entryGST = 0m;
			var entryDUTY = 0m;

			foreach (DeclarationDetail xsddetail in xsddec.DeclarationDetail)
			{
				var line = entry.MergedLines.AddNew();
				line.CL_LineNumber = ZShort.ParseSafe(xsddetail.ItemNo.ToString(), 0);

				ZDecimal customsValue;
				if (ZDecimal.TryParse(xsddetail.LineStatValue, out customsValue))
				{
					line.CL_CustomsValue = customsValue;
				}

				if (line.CL_CustomsValue.IsEmpty)
				{
					foreach (ValueAmount xsdvalueAmount in xsddetail.ValueAmount)
					{
						if (xsdvalueAmount.Type.ToUpper() == "LINESTATISTICALVALUE")
						{
							line.CL_CustomsValue = xsdvalueAmount.AmountValue;
							break;
						}
					}
				}

				context.SetPropertyInfoValue(line.CL_AdValoremTariffInfo, xsddetail.CommodityCode.Code1);
				line.CL_CustomsPostedStatus = "ACT";
				context.SetPropertyInfoValue(line.CL_DescriptionInfo, xsddetail.MarksNumbers.GoodsDescription);

				var chargeCodeAndAmountList = new List<Tuple<ZString, ZDecimal>>();
				foreach (TaxDetails xsdtaxdetail in xsddetail.TaxDetails)
				{
					if (xsdtaxdetail.TaxGroup == XSD.TaxDetailsTaxGroup.ConfirmedTaxes || xsdtaxdetail.TaxGroup == XSD.TaxDetailsTaxGroup.AdditionalTaxes)
					{
						chargeCodeAndAmountList.Add(Tuple.Create(xsdtaxdetail.TaxType.Left(3), (ZDecimal)xsdtaxdetail.TaxAmount));
					}
				}

				//recalculate Duty and GST
				var gst = 0m;
				var duty = 0m;
				GetDutyandTaxes(out duty, out gst, chargeCodeAndAmountList);

				if (duty > 0)
				{
					line.Fees.AddOrUpdate(FeeTypeList.Codes.A00, duty);
					entryDUTY += duty;
				}

				if (gst > 0)
				{
					line.Fees.AddOrUpdate(FeeTypeList.Codes.B00, gst);
					entryGST += gst;
				}
			}

			entry.Charges.AddNew(FeeTypeList.Codes.A00, entryDUTY);
			entry.Charges.AddNew(FeeTypeList.Codes.B00, entryGST);
			context.SetPropertyInfoValue(entry.CH_TotalPaidInfo, entryDUTY + entryGST, 19, 4);
		}

		protected virtual void GetDutyandTaxes(out decimal duty, out decimal gST, List<Tuple<ZString, ZDecimal>> chargeCodeAndAmountList)
		{
			duty = 0m;
			gST = 0m;

			foreach (var fee in chargeCodeAndAmountList)
			{
				if (fee.Item1.StartsWith("B"))
				{
					gST += fee.Item2;
				}
				else if (fee.Item1.StartsWith("A"))
				{
					duty += fee.Item2;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		protected virtual void ImportAttachments(BaseJobDeclaration dec, InputDocument inputDocument, IValueObjectImportContext context)
		{
			dec.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;//to avoid unnecessary concurrency errors
			var documentFactory = (BusinessObjectFactory)dec.DocManagerInfo.MasterFactory;

			if (!dec.Factory.ChildFactories.Contains(documentFactory))
			{
				dec.Factory.ChildFactories.Add(documentFactory);
			}

			//if we dont do this, it results in StackOverflowException as documentFactory is both parent and child of dec.Factory.
			if (documentFactory.ChildFactories.Contains(dec.Factory))
			{
				documentFactory.ChildFactories.Remove(dec.Factory);
			}

			foreach (Attachment xsdattachment in inputDocument.AttachmentList)
			{
				var attachmentStream = xsdattachment.AttachmentStream;
				if (!attachmentStream.IsEmpty)
				{
					byte[] byteArray = System.Convert.FromBase64String(attachmentStream);
					var eDoc = dec.DocManagerInfo.AddFileOrDocument(byteArray, xsdattachment.FileName, "MSC");
					eDoc.Description = Res.GetString("6316D2C1-D7F2-4F29-B910-E9F567EDE811", "Miscellaneous");

					dec.Logs.AddNew(Events.DocumentAllocated, StmALogEventSourceExtensions.GenerateEventReference("MSC", eDoc.UniqueKey));
				}
			}
		}

		protected void ImportEvent(CusEntryHeader entry, XSD.Event xsdevent, IValueObjectImportContext context)
		{
			ZDateTime xsdEventDateTime;
			var logDate = ZDateTime.Now;

			if (!xsdevent.EventCode.IsEmpty)
			{
				if (ZDateTime.TryParseISO8601Date(xsdevent.EventDateTime, out xsdEventDateTime))
				{
					logDate = xsdEventDateTime;
				}

				var alreadyLogged = entry.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsEntryStatus, xsdevent.EventCode);
				if (alreadyLogged == null || alreadyLogged.SL_EventTime != xsdEventDateTime)
				{
					entry.Logs.AddNew(Events.CustomsEntryStatus, xsdevent.EventCode, logDate.ToOffset()); // It is possible this has the wrong timezone currently.
				}

				var newStatus = CustomsWareEntryStatusList.GetCodeFromCustomsWareStatusText(xsdevent.EventCode);
				if (newStatus != "")
				{
					entry.CH_EntryStatus = newStatus;
					if (entry.CH_EntryStatus == CustomsWareEntryStatusList.Codes.Cleared || entry.CH_EntryStatus == CustomsWareEntryStatusList.Codes.Released)
					{
						if (entry.Declaration != null)
						{
							entry.Declaration.LogCustomsClearedIfNeeded();
						}

						entry.CH_EntryReleaseDate = logDate;
					}
					else if (entry.CH_EntryStatus == CustomsWareEntryStatusList.Codes.Sent)
					{
						entry.CH_EntrySubmittedDate = logDate;
					}
				}
			}
		}

		ZString BGMReference(ZString reference, ZString subReference)
		{
			return reference + @"\" + subReference;
		}

		#endregion

		#region Implementation

		protected string GetNumericEquivalent(string transportMode)
		{
			switch (transportMode)
			{
				case Constants.TransportModes.Air:
					return Constants.TransportCodes.Air;
				case Constants.TransportModes.Sea:
					return Constants.TransportCodes.Sea;
				case Constants.TransportModes.Road:
					return Constants.TransportCodes.Road;
				case Constants.TransportModes.Rail:
					return Constants.TransportCodes.Rail;
				default:
					return "";
			}
		}

		#endregion

		#region Schema

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return SchemaDefinitions.Instance.InputDocumentSchema; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return SchemaDefinitions.Instance.InputDocumentSchema; }
		}

		public override string RootElementName
		{
			get { return "InputDocument"; }
		}

		public override string RootCollectionElementName
		{
			get { return "InputDocuments"; }
		}

		#endregion
	}
}
