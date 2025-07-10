using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsManifestSenderWrapper : GVMSMessageDataObject, IGvmsMessageBuilder
	{
		public GvmsManifestSenderWrapper(AsycudaManifestHeader manifestHeader)
		{
			header = Argument.NotNull(manifestHeader, "manifest cannot be null");
		}

		public override string direction
		{
			get
			{
				if (GVMSManifestNature.NatureFullNameMapping.TryGetValue(header.AMA_Nature, out ZString result))
				{
					return result;
				}
				return ZString.Empty;
			}
		}

		public override string haulierType => (string)header.HaulierType switch
		{
			GVMSHaulierType.Codes.Standard => "STANDARD",
			GVMSHaulierType.Codes.FastParcelOperatorsThatAreMembersOfTheAntiSmugglingNetwork => "FPO_ASN",
			GVMSHaulierType.Codes.FastParcelOperatorsThatAreNotMembersOfTheAsnAndAreNotMovingGoodsWithAMemorandumOfUnderstanding => "FPO_OTHER",
			GVMSHaulierType.Codes.NatoOrMinistryOfDefence => "NATO_MOD",
			GVMSHaulierType.Codes.RoyalMailGroup => "RMG",
			GVMSHaulierType.Codes.ExtraTerritorialOfficeOfExchange => "ETOE",
			_ => null,
		};

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public override bool isUnaccompanied => header.IsUnaccompanied;
		[DefaultValue("")]
		public override string vehicleRegNum => header.AMA_VehicleRegistration;

		public override string sAndSMasterRefNum
		{
			get
			{
				string result = null;

				var icsRef = header.GvmsCustomsReferenceCollection.OfType<GvmsItemReference>().FirstOrDefault(x => x.CSI_Code == GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration
																												&& x.CSI_ReferenceNumber.IsEmpty
																												&& !x.CSI_ReferenceNumber2.IsEmpty);

				if (icsRef != null)
				{
					result = icsRef.CSI_ReferenceNumber2;
				}

				return result;
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public override List<string> trailerRegistrationNums
		{
			get
			{
				List<string> result = new List<string>();
				if (!header.AMA_Trailer1RegNo.IsEmpty)
				{ result.Add(header.AMA_Trailer1RegNo); }
				if (!header.AMA_Trailer2RegNo.IsEmpty)
				{ result.Add(header.AMA_Trailer2RegNo); }

				return result.Count > 0 ? result : null;
			}
		}

		public override List<string> containerReferenceNums
		{
			get
			{
				var codes = header.Containers.Select(x => (string)x.ACN_ContainerNumber).Where(x => !string.IsNullOrEmpty(x)).ToList();
				return codes.Count == 0 ? null : codes;
			}
		}

		public override Plannedcrossing plannedCrossing
		{
			get
			{
				Plannedcrossing plannedCrossing = new Plannedcrossing();
				plannedCrossing.routeId = header.RouteId;
				plannedCrossing.localDateTimeOfDeparture = header.AMA_E_DEP.ToString("yyyy-MM-ddThh:mm");
				return plannedCrossing;
			}
		}

		public override Emptyvehicle emptyVehicle
		{
			get
			{
				if (header.EmptyVehicle != ZString.Empty)
				{
					Emptyvehicle emptyVehicle = new Emptyvehicle();
					emptyVehicle.isOwnVehicle = header.EmptyVehicle == GVMSEmptyVehicle.Codes.EmptyVehicleIsNotBeingMovedViaAContractOfCarriage;
					var customsRef = header.GvmsCustomsReferenceCollection.Cast<GvmsItemReference>().FirstOrDefault(x => x.CSI_Code == GVMSCustomsReference.Codes.SSReferenceForEmptyVehicle);
					emptyVehicle.sAndSMasterRefNum = customsRef == null ? ZString.Empty : customsRef.CSI_ReferenceNumber2 != ZString.Empty ? customsRef.CSI_ReferenceNumber2 : customsRef.CSI_ReferenceNumber;
					return emptyVehicle;
				}

				return null;
			}
		}

		public override ExemptionDeclaration exemptionDeclaration
		{
			get
			{
				var exemptionDeclaration = new ExemptionDeclaration();
				var exemptedGoods = new List<ExemptedGoods>();
				foreach (GvmsItemReference customsItemReference in header.GvmsCustomsReferenceCollection)
				{
					if (!customsItemReference.CSI_ReferenceNumber2.IsEmpty && customsItemReference.CSI_Code == GVMSCustomsReference.Codes.ExemptGoods)
					{
						var exemptedGood = new ExemptedGoods()
						{
							sAndSMasterRefNum = customsItemReference.CSI_ReferenceNumber2
						};
						exemptedGoods.Add(exemptedGood);
					}
				}
				exemptionDeclaration.exemptedGoods = exemptedGoods;
				return exemptedGoods.Any() ? exemptionDeclaration : null;
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public override List<Customsdeclaration> customsDeclarations
		{
			get
			{
				var customsDeclarations = new List<Customsdeclaration>();
				foreach (GvmsItemReference customsItemReference in header.GvmsCustomsReferenceCollection)
				{
					if (!customsItemReference.CSI_ReferenceNumber.IsEmpty &&
						customsItemReference.CSI_Code != GVMSCustomsReference.Codes.TirCarnet &&
						customsItemReference.CSI_Code != GVMSCustomsReference.Codes.AtaCarnet &&
						customsItemReference.CSI_Code != GVMSCustomsReference.Codes.IndirectExportDeclarationEad &&
						customsItemReference.CSI_Code != GVMSCustomsReference.Codes.ExemptGoods &&
						customsItemReference.CSI_Code != GVMSCustomsReference.Codes.SSReferenceForEmptyVehicle)
					{
						var customsdeclaration = new Customsdeclaration()
						{
							customsDeclarationId = customsItemReference.CSI_ReferenceNumber
						};

						if (customsItemReference.CSI_Code == GVMSCustomsReference.Codes.CdsExportDeclarationUniqueConsignmentReferenceDucr && customsItemReference.CSI_ReferenceNumber.Contains("/"))
						{
							var indexOfSlash = customsItemReference.CSI_ReferenceNumber.LastIndexOf('/');
							var reference = customsItemReference.CSI_ReferenceNumber.SubstringSafe(0, indexOfSlash);
							var partId = customsItemReference.CSI_ReferenceNumber.SubstringSafe(indexOfSlash + 1);
							if (partId != string.Empty)
							{
								customsdeclaration.customsDeclarationPartId = partId;
								customsdeclaration.customsDeclarationId = reference;
							}
						}

						if (!customsItemReference.CSI_ReferenceNumber2.IsEmpty)
						{
							customsdeclaration.sAndSMasterRefNum = customsItemReference.CSI_ReferenceNumber2;
						}

						customsDeclarations.Add(customsdeclaration);
					}
				}
				return customsDeclarations.Count > 0 ? customsDeclarations : null;
			}
		}

		public override MtpDeclaration mtpDeclaration
		{
			get
			{
				var codes = header.GvmsCustomsReferenceCollection
					.Where(x => x.CSI_Code == GVMSCustomsReference.Codes.ManualTransitProcedure)
					.Select(x => new MtpGoodsItem { sAndSMasterRefNum = x.CSI_ReferenceNumber2 })
					.ToList();
				return codes.Count == 0 ? null : new MtpDeclaration { mtpGoods = codes };
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public override List<Eidrdeclaration> eidrDeclarations
		{
			get
			{
				var eidrDeclarations = new List<Eidrdeclaration>();
				foreach (GvmsItemReference eidrItemReference in header.GvmsEidrAndOralReferenceCollection)
				{
					if (eidrItemReference.CSI_Code == GVMSCustomsReference.Codes.EntryInDeclarantsRecord)
					{
						var dec = new Eidrdeclaration
						{
							traderEORI = eidrItemReference.CSI_ReferenceNumber
						};

						if (!eidrItemReference.CSI_ReferenceNumber2.IsEmpty)
						{
							dec.sAndSMasterRefNum = eidrItemReference.CSI_ReferenceNumber2;
						}
						if (!eidrItemReference.CSI_Procedure.IsEmpty)
						{
							dec.procedureCode = eidrItemReference.CSI_Procedure;
						}
						if (!eidrItemReference.CSI_Description.IsEmpty)
						{
							dec.localReferenceNumber = eidrItemReference.CSI_Description;
						}

						eidrDeclarations.Add(dec);
					}
				}
				return eidrDeclarations.Count > 0 ? eidrDeclarations : null;
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public override List<UkimsEidrdeclaration> ukimsEidrDeclarations
		{
			get
			{
				var ukimsEidrDeclarations = new List<UkimsEidrdeclaration>();
				foreach (GvmsItemReference ukimsEidrItemReference in header.GvmsEidrAndOralReferenceCollection)
				{
					if (ukimsEidrItemReference.CSI_Code == GVMSCustomsReference.Codes.UkInternalMarketSchemeEntryInDeclarantsRecordsDeclaration)
					{
						var dec = new UkimsEidrdeclaration
						{
							traderEORI = ukimsEidrItemReference.CSI_ReferenceNumber
						};
						if (!ukimsEidrItemReference.CSI_Description.IsEmpty)
						{
							dec.localReferenceNumber = ukimsEidrItemReference.CSI_Description;
							dec.nopWaiver = false;
						}
						if (ukimsEidrItemReference.CSI_Description.IsEmpty)
						{
							dec.nopWaiver = true;
						}
						ukimsEidrDeclarations.Add(dec);
					}
				}
				return ukimsEidrDeclarations.Count > 0 ? ukimsEidrDeclarations : null;
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public override Dbcdeclaration dbcDeclaration
		{
			get
			{
				var dbcDeclaration = new Dbcdeclaration();
				dbcDeclaration.isOwnVehicle = header.EmptyVehicle == GVMSEmptyVehicle.Codes.EmptyVehicleIsNotBeingMovedViaAContractOfCarriage;
				foreach (GvmsItemReference eidrItemReference in header.GvmsEidrAndOralReferenceCollection)
				{
					if (eidrItemReference.CSI_Code == GVMSCustomsReference.Codes.OralDeclaration)
					{
						if (dbcDeclaration.dbcGoods is null)
						{ dbcDeclaration.dbcGoods = new List<DbcGoodsItem>(); }
						dbcDeclaration.dbcGoods.Add(new DbcGoodsItem()
						{
							sAndSMasterRefNum = eidrItemReference.CSI_ReferenceNumber2
						});
					}
				}

				return dbcDeclaration.dbcGoods?.Count > 0 ? dbcDeclaration : null;
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public override List<IndirectExportdeclaration> indirectExportDeclarations
		{
			get
			{
				var indirectExportDeclarations = new List<IndirectExportdeclaration>();
				foreach (GvmsItemReference customsItemReference in header.GvmsCustomsReferenceCollection)
				{
					if (customsItemReference.CSI_Code == GVMSCustomsReference.Codes.IndirectExportDeclarationEad)
					{
						indirectExportDeclarations.Add(new IndirectExportdeclaration()
						{
							eadMasterRefNum = customsItemReference.CSI_ReferenceNumber
						});
					}
				}
				return indirectExportDeclarations.Count > 0 ? indirectExportDeclarations : null;
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public override List<Tirdeclaration> tirDeclarations
		{
			get
			{
				var tirDeclarations = new List<Tirdeclaration>();
				foreach (GvmsItemReference tirItemReference in header.GvmsCustomsReferenceCollection)
				{
					if (tirItemReference.CSI_Code == GVMSCustomsReference.Codes.TirCarnet)
					{
						tirDeclarations.Add(new Tirdeclaration()
						{
							tirCarnetId = tirItemReference.CSI_ReferenceNumber
						});
					}
				}
				return tirDeclarations.Count > 0 ? tirDeclarations : null;
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public override List<Transitdeclaration> transitDeclarations
		{
			get
			{
				var transitDeclarations = new List<Transitdeclaration>();
				foreach (GvmsItemReference transItemReference in header.GvmsTransitReferenceCollection)
				{
					if (transItemReference.CSI_Code == GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber)
					{
						var dec = new Transitdeclaration
						{
							transitDeclarationId = transItemReference.CSI_ReferenceNumber,
							isTSAD = (transItemReference.CSI_Status == YesNoList.Codes.Yes)
						};

						if (!transItemReference.CSI_ReferenceNumber2.IsEmpty)
						{
							dec.sAndSMasterRefNum = transItemReference.CSI_ReferenceNumber2;
						}

						transitDeclarations.Add(dec);
					}
				}
				return transitDeclarations.Count > 0 ? transitDeclarations : null;
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public override List<Atadeclaration> ataDeclarations
		{
			get
			{
				var ataDeclarations = new List<Atadeclaration>();
				foreach (GvmsItemReference ataItemReference in header.GvmsCustomsReferenceCollection)
				{
					if (ataItemReference.CSI_Code == GVMSCustomsReference.Codes.AtaCarnet)
					{
						var dec = new Atadeclaration
						{
							ataCarnetId = ataItemReference.CSI_ReferenceNumber
						};

						if (!ataItemReference.CSI_ReferenceNumber2.IsEmpty)
						{
							dec.sAndSMasterRefNum = ataItemReference.CSI_ReferenceNumber2;
						}

						ataDeclarations.Add(dec);
					}
				}
				return ataDeclarations.Count > 0 ? ataDeclarations : null;
			}
		}

		public override UkcDeclaration ukcDeclaration
		{
			get
			{
				var code = header.GvmsEidrAndOralReferenceCollection
					.Where(x => x.CSI_Code == GVMSCustomsReference.Codes.UkCarrier)
					.Select(x => x.CSI_ReferenceNumber)
					.FirstOrDefault();
				return code.IsEmpty ? null : new UkcDeclaration { fpoEORI = code };
			}
		}

		readonly AsycudaManifestHeader header;
	}
}
