using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(EUManifestTypes))]
[assembly: UsesConstants(typeof(ICSManifestTypes))]

namespace Enterprise.Customs.GB.GVMS
{
	public static class GVMSExtensions
	{
		public static ZString Serialize<T>(T dataObj)
		{
			var settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			settings.Indent = true;

			using (var stream = new StringWriter(CultureInfo.InvariantCulture))
			using (var xmlWritter = XmlWriter.Create(stream, settings))
			{
				var serializer = ZXmlSerializer.New(typeof(T));
				serializer.Serialize(xmlWritter, dataObj);
				return stream.ToString();
			}
		}

		public static ZString Serialize(this GBCustomsRequest requestData) => Serialize<GBCustomsRequest>(requestData);

		public static void PopulateReferenceFromConsol(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType)
		{
			if (header is AsycudaManifestHeader gvmsHeader)
			{
				var consol = gvmsHeader.Consol;
				var allShipments = consol.Shipments.OfType<ForwardingShipment>();
				IEnumerable<ForwardingShipment> shipments = null;

				switch (gvmsHeader.AMA_Nature)
				{
					case GVMSManifestNature.Codes.GBtoNI:
						shipments = allShipments.Where(x => x.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)
															&& x.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)
															&& x.Origin.IsInGreatBritain
															&& x.Destination.IsInNorthernIreland);
						break;
					case GVMSManifestNature.Codes.NItoGB:
						shipments = allShipments.Where(x => x.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)
															&& x.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)
															&& x.Origin.IsInNorthernIreland
															&& x.Destination.IsInGreatBritain);
						break;
					case GVMSManifestNature.Codes.Import:
						shipments = allShipments.Where(x => (!x.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)) && (!x.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes)) && x.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.UnitedKingdom));
						break;
					case GVMSManifestNature.Codes.Export:
						shipments = allShipments.Where(x => x.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom) && (!x.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)) && (!x.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes)));
						break;
					default:
						break;
				}

				if (shipments != null)
				{
					var declarations = shipments.Select(x => x.GetDeclaration() as JobDeclaration);
					var entries = declarations.OfType<JobDeclaration>()?.SelectMany(x => x.CustomsEntryHeaders);
					if (entries != null)
					{
						foreach (var entryHeader in entries)
						{
							if (!entryHeader.CusEntryNumber?.CE_EntryNum.IsEmpty ?? false)
							{
								var declaration = entryHeader.Declaration;
								var code = declaration.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Customs_Declaration_Services ? GVMSCustomsReference.Codes.CdsMovementReferenceNumberMrn : GVMSCustomsReference.Codes.ChiefImportEntryReferenceNumber;

								var reference = gvmsHeader.GvmsCustomsReferenceCollection.Cast<GvmsItemReference>().FirstOrDefault(x => x.CSI_Code == code
																																	&& x.CSI_ReferenceNumber == entryHeader.CusEntryNumber.CE_EntryNum
																																	&& x.CSI_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom
																																	&& x.CSI_DateOfIssue == entryHeader.CusEntryNumber.CE_IssueDate
																																	&& x.CSI_IssuerType == GvmsItemReference.SystemStatus);
								if (reference == null)
								{
									reference = gvmsHeader.GvmsCustomsReferenceCollection.AddNew();
									reference.CSI_Code = code;
									reference.CSI_ReferenceNumber = entryHeader.CusEntryNumber.CE_EntryNum;
									reference.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
									reference.CSI_DateOfIssue = entryHeader.CusEntryNumber.CE_IssueDate;
									reference.CSI_IssuerType = GvmsItemReference.SystemStatus;
								}
							}
						}
					}
				}

				var queryNctsConsol = new ZQuery(CusInBondHeaderSchema.BH_ParentID, consol.PK);
				queryNctsConsol.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, consol.Prefix);
				var overallQuery = new ZQuery();
				overallQuery.AddToFilter(queryNctsConsol);

				var shipmentPKs = shipments?.Select(s => s.PK);
				if (shipmentPKs != null && shipmentPKs.Any())
				{
					var queryNctsShipments = new ZQuery(CusInBondHeaderSchema.BH_ParentID, shipments.Select(s => s.PK).ToArray());
					queryNctsShipments.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
					overallQuery.AddToFilter(queryNctsShipments, JoinCondition.Or);
				}
				var nctsHeaders = consol.Factory.Load<Business.NctsHeader>(overallQuery);

				foreach (var nctsHeader in nctsHeaders)
				{
					if (!nctsHeader.MovementReferenceEntryNumber?.CE_EntryNum.IsEmpty ?? false)
					{
						var reference = gvmsHeader.GvmsTransitReferenceCollection.Cast<GvmsItemReference>().FirstOrDefault(x => x.CSI_Code == GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber
																																	&& x.CSI_ReferenceNumber == nctsHeader.MovementReferenceEntryNumber.CE_EntryNum
																																	&& x.CSI_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom
																																	&& x.CSI_DateOfIssue == nctsHeader.MovementReferenceEntryNumber.CE_IssueDate
																																	&& x.CSI_IssuerType == GvmsItemReference.SystemStatus);
						if (reference == null)
						{
							reference = gvmsHeader.GvmsTransitReferenceCollection.AddNew();
							reference.CSI_Code = GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber;
							reference.CSI_ReferenceNumber = nctsHeader.MovementReferenceEntryNumber.CE_EntryNum;
							reference.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
							reference.CSI_DateOfIssue = nctsHeader.MovementReferenceEntryNumber.CE_IssueDate;
							reference.CSI_IssuerType = GvmsItemReference.SystemStatus;
							reference.CSI_Status = nctsHeader.BH_FTZMove ? YesNoList.Descriptions.Yes : YesNoList.Descriptions.No;
						}
					}
				}

				var queryICSConsol = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, consol.PK);
				queryICSConsol.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentTableCode, consol.Prefix);
				var icss = consol.Factory.Load<ASYCUDA.Business.AsycudaManifestHeader>(queryICSConsol);
				var icsManifestsOnThisConsol = icss?.Where(m => (m.AMA_ManifestType == ICSManifestTypes.Codes.SAS || m.AMA_ManifestType == EUManifestTypes.Codes.ICS) && m.AMA_RN_NKCountry == Core.Constants.CountryCodes.UnitedKingdom);

				if (icsManifestsOnThisConsol != null)
				{
					foreach (var ics in icsManifestsOnThisConsol)
					{
						if (!ics.RegistrationNumber.IsEmpty)
						{
							var reference = gvmsHeader.GvmsCustomsReferenceCollection.Cast<GvmsItemReference>().FirstOrDefault(x => x.CSI_Code == GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration
																																	&& x.CSI_ReferenceNumber2 == gvmsHeader.RegistrationNumber
																																	&& x.CSI_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom
																																	&& x.CSI_DateOfIssue == gvmsHeader.RegistrationDate
																																	&& x.CSI_IssuerType == GvmsItemReference.SystemStatus);
							if (reference == null)
							{
								reference = gvmsHeader.GvmsCustomsReferenceCollection.AddNew();
								reference.CSI_Code = GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration;
								reference.CSI_ReferenceNumber2 = ics.RegistrationNumber;
								reference.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
								reference.CSI_DateOfIssue = ics.RegistrationDate;
								reference.CSI_IssuerType = GvmsItemReference.SystemStatus;
							}
						}
					}
				}
			}
		}
	}
}
