using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataConverters.CustomsFiles
{
	#region ConverterData
	public struct ConverterData
	{
		public int RecordsImporting;
		public int CurrentRow;
		public int RecordsExcluded;
		public int RecordsUpdated;
		public StringCollection Log;

		public ConverterData(int recordsImporting, int currentRow, int recordsExcluded, int recordsUpdated, StringCollection log)
		{
			this.RecordsImporting = recordsImporting;
			this.CurrentRow = currentRow;
			this.RecordsExcluded = recordsExcluded;
			this.RecordsUpdated = recordsUpdated;
			this.Log = log;
		}
	}
	#endregion

	public abstract class CustomsDataImporter
	{
		public CustomsDataImporter(string dataLocation, bool classification, bool limitedClass)
		{
			Directory = dataLocation;
			UpdateClassifications = classification || limitedClass;
			LimitedClassifications = limitedClass;
		}

		protected BusinessObjectFactory Factory;

		public void SaveChanges()
		{
			Factory.Save();
		}

		protected readonly string Directory;
		protected readonly bool UpdateClassifications;
		protected readonly bool LimitedClassifications;

		protected bool ProcessingExportClass;
		protected bool ProcessingExportParts;
		protected bool GenerateNewOrgCode;
		protected bool UsesTariffStat;

		protected ZString DeliveranceSystemCode;
		protected ZString SavedPartOwnerCode;

		protected Guid TransactionPK;
		protected ZGuid SavedPartOwnerCodePK;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		protected readonly DataSet ImportDataSet = new DataSet();

		#region DataFields

		#region ClassificationTable

		protected ZString ClassCode;
		protected ZString ClassType;
		protected ZString Tariff;
		protected ZString TariffStat;
		protected ZString Description;
		protected ZString AddInfo;
		protected ZString Origin;
		protected ZString Pref;
		protected ZString InstrumentCode;
		protected ZString InstrumentType;
		protected ZString TreatmentCode;
		protected bool IsAudited;

		#endregion

		#region PartsTable

		protected ZString PartNo;
		protected ZString ValidPartNo;
		protected ZString PartDesc;
		protected ZString PartFullDesc;
		protected ZString PartDivision;
		protected ZString PartSupplierCode;
		protected ZGuid PartSupplierCodePK;
		protected ZString PartOwnerCode;
		protected ZGuid PartOwnerCodePK;
		protected ZString Supplier;
		protected ZString PartAddInfo;
		protected ZString PartTreatmentCode;
		protected ZString PartClass;
		protected ZString ExportPartClass;
		protected decimal PartCount;
		protected decimal PartLastCostAmount;
		protected decimal PartUnitPerKG;
		protected decimal PartUnitPerM3;
		protected decimal PartWeight;
		protected ZString PartUQ;
		protected ZString PartOrigin;
		protected ZString PartPreference;
		protected ZBool PartAudited;

		#endregion

		#endregion

		#region Counters

		protected int CurrentRow;
		protected int RecordsToImport;
		protected int RecsToUpdate;
		protected int RecsExcluded;
		protected int RecsCreated;
		protected int RecsUpdated;
		protected int DisplayCount;
		protected int ExcludeRecordCount;
		protected int BulkRecordsToImportCount;

		protected void InitialiseCounters()
		{
			RecordsToImport = 0;
			CurrentRow = 0;
			RecsToUpdate = 0;
			RecsExcluded = 0;
			RecsCreated = 0;
			RecsUpdated = 0;
			DisplayCount = 0;
			ExcludeRecordCount = 0;
			BulkRecordsToImportCount = 0;
		}

		#endregion

		#region PrepareForExportClassifications

		protected void SetupForExportClassificatonCreationParse()
		{
			ProcessingExportClass = true;

			AddInfo = ZString.Empty;
			Pref = ZString.Empty;
			Origin = ZString.Empty;
			InstrumentCode = ZString.Empty;
			InstrumentType = ZString.Empty;
			TreatmentCode = ZString.Empty;
		}

		#endregion

		#region PrepareForExportProducts

		protected void SetUpForExportProductCreationParse()
		{
			ProcessingExportParts = true;

			PartDivision = ZString.Empty;
			PartOwnerCode = ZString.Empty;
			PartSupplierCode = ZString.Empty;
		}

		#endregion

		#region Load Data

		protected string CreateAndLoadEnterprisePart()
		{
			string errorMsg = "";
			TransactionPK = Guid.Empty;

			if (ClassificationLookupExists(PartClass, ExportPartClass))
			{
				try
				{
					AUOrgSupplierPart newEnterprisePart = AUOrgSupplierPart.New(Factory);
					newEnterprisePart.SuspendValidation();
					LoadImportedPartValues(newEnterprisePart);
					TransactionPK = newEnterprisePart.PK.ToGuid();
					RecsToUpdate++;
					RecsCreated++;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					RecsExcluded++;
					DisplayFormatLogMessage(ex.Message);
				}
			}
			else
			{
				errorMsg = PartExcludedAsAlreadyInEnterpriseOrLookupNotFound(true);
			}

			return errorMsg;
		}

		protected string UpdateExistingPartWithExportLookupIfRequired(MasterFiles.Business.OrgSupplierPart enterprisePart)
		{
			string updateMsg = "";
			bool updateExistingPartWithExportLookup = false;
			if (ProcessingExportParts)
			{
				updateExistingPartWithExportLookup = LoadExportLookupIfItExists(enterprisePart, ExportPartClass);
			}

			if (updateExistingPartWithExportLookup)
			{
				RecsToUpdate++;
				RecsUpdated++;
				updateMsg = BrandingFactory.Instance.ProductName + " Part " + PartNo + " / " + PartDesc + " - has been updated with Export Lookup " + ExportPartClass;
			}

			return updateMsg;
		}

		protected string PartExcludedAsAlreadyInEnterpriseOrLookupNotFound(bool lookupError)
		{
			RecsExcluded++;
			ExcludeRecordCount++;
			Factory.ClearQueryCache();

			string errorMsg = "Part No. excluded: " + PartNo + " / " + PartDesc + " - already exists in the " + BrandingFactory.Instance.ProductName + " database table";
			if (lookupError)
			{
				string lookupNotFound = (ProcessingExportParts ? ExportPartClass : PartClass);
				errorMsg = "Part excluded: " + PartNo + " - Lookup code " + lookupNotFound + " - does not exist in the " + BrandingFactory.Instance.ProductName + " database table";
			}

			return errorMsg;
		}

		protected void LoadImportedPartValues(MasterFiles.Business.OrgSupplierPart enterprisePart)
		{
			enterprisePart.OP_PartNum = PartNo;
			enterprisePart.OP_Desc = PartDesc;
			enterprisePart.OP_Division = PartDivision;
			enterprisePart.OP_QtyInStock = PartCount;
			enterprisePart.OP_Weight = PartWeight;
			enterprisePart.OP_Cubic = PartUnitPerM3;
			enterprisePart.OP_StockKeepingUnit = PartUQ;
			if (PartLastCostAmount > 0)
			{
				enterprisePart.OP_LastCost = PartLastCostAmount;
			}

			if (!PartOwnerCode.IsEmpty)
			{
				if (PartOwnerCodePK.IsEmpty)
				{
					PartOwnerCodePK = GetOwnerPK();
				}

				LoadOrgPartRelation(enterprisePart, PartOwnerCodePK, "OWN");
			}

			if (!PartSupplierCode.IsEmpty)
			{
				if (PartSupplierCodePK.IsEmpty)
				{
					PartSupplierCodePK = GetOwnerSupplierCodePK(PartSupplierCode);
				}

				LoadOrgPartRelation(enterprisePart, PartSupplierCodePK, "SUP");
			}

			if (!PartClass.IsEmpty)
			{
				ZGuid classificationPK = GetClassificationPK(PartClass, "IMP");
				if (classificationPK != Guid.Empty)
				{
					LoadCusClassPartPivot(enterprisePart, classificationPK, PartAddInfo, PartTreatmentCode);
				}
			}

			if (!ExportPartClass.IsEmpty)
			{
				ZGuid classificationPK = Guid.Empty;
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == "ZA")
				{
					classificationPK = GetClassificationPK(ExportPartClass, "BTH");
				}
				else
				{
					classificationPK = GetClassificationPK(ExportPartClass, "EXP");
				}

				if (classificationPK != Guid.Empty)
				{
					LoadCusClassPartPivot(enterprisePart, classificationPK, "", "");
				}
			}

			LoadDescriptionNote(enterprisePart, PartFullDesc, "Full Product Description");

			enterprisePart.Logs.AddNew(Events.DataImport);
			if (PartAudited)
			{
				enterprisePart.Logs.AddNew(Events.RecordAudited, "Audited in Deliverance - data import");
			}
		}

		protected abstract void LoadCusClassPartPivot(MasterFiles.Business.OrgSupplierPart enterprisePart, ZGuid cusClassPK, ZString addInfo, ZString trtCode);

		protected void LoadImportedClassificationValues(Classification enterpriseCusClass)
		{
			enterpriseCusClass.CC_LookupCode = ClassCode;
			enterpriseCusClass.CC_Description = Description;
			enterpriseCusClass.CC_ClassificationType = ClassType;
			enterpriseCusClass.CC_TariffNum = Tariff;
			enterpriseCusClass.CC_AddInfo = ConstructAddInfo();
			enterpriseCusClass.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			enterpriseCusClass.Logs.AddNew(Events.DataImport);
			if (IsAudited)
			{
				enterpriseCusClass.CC_LastAuditedDate = ZDateTime.Now;
				enterpriseCusClass.Logs.AddNew(Events.RecordAudited, "Audited in Deliverance - data import");
			}
		}

		ZString ConstructAddInfo()
		{
			if (!InstrumentCode.IsEmpty)
			{
				AddInfo = AppendAddInfo(AddInfo, "InstrumentCode_Hidden=" + InstrumentCode);
			}

			if (!InstrumentType.IsEmpty)
			{
				AddInfo = AppendAddInfo(AddInfo, "InstrumentType_Hidden=" + InstrumentType);
			}

			if (!TreatmentCode.IsEmpty)
			{
				AddInfo = AppendAddInfo(AddInfo, "TreatmentCode_Hidden=" + TreatmentCode);
			}

			if (!Pref.IsEmpty)
			{
				AddInfo = AppendAddInfo(AddInfo, "PRF=" + Pref);
			}

			if (!Origin.IsEmpty)
			{
				AddInfo = AppendAddInfo(AddInfo, "ORG=" + Origin);
			}

			return AddInfo;
		}

		string AppendAddInfo(string addInfo, string newAddInfo)
		{
			if (string.IsNullOrEmpty(addInfo))
			{
				addInfo = newAddInfo;
			}
			else
			{
				addInfo += "*" + newAddInfo;
			}

			return addInfo;
		}

		void LoadOrgPartRelation(MasterFiles.Business.OrgSupplierPart enterprisePart, ZGuid ownerSupplierPK, string relationship)
		{
			if (!ownerSupplierPK.IsEmpty)
			{
				ZQuery orgPartFilter = new ZQuery(OrgPartRelationSchema.OU_OP, enterprisePart.PK);
				orgPartFilter.AddToFilter(OrgPartRelationSchema.OU_OH, ownerSupplierPK);
				orgPartFilter.AddToFilter(OrgPartRelationSchema.OU_Relationship, relationship);
				OrgPartRelation orgPartLink = Factory.LoadTop1<OrgPartRelation>(orgPartFilter);
				if (orgPartLink == null)
				{
					OrgPartRelation newOrgPartLink = enterprisePart.RelatedOrganisations.AddNew();
					newOrgPartLink.OU_OH = ownerSupplierPK;
					newOrgPartLink.OU_Relationship = relationship;
				}
			}
		}

		void LoadDescriptionNote(MasterFiles.Business.OrgSupplierPart enterprisePart, ZString fullDescription, string noteType)
		{
			if (!fullDescription.IsEmpty)
			{
				ZQuery noteFilter = new ZQuery(StmNoteSchema.ST_ParentID, enterprisePart.PK);
				noteFilter.AddToFilter(StmNoteSchema.ST_Table, "OrgSupplierPart");
				noteFilter.AddToFilter(StmNoteSchema.ST_Description, noteType);
				StmNote orgNote = Factory.LoadTop1<StmNote>(noteFilter);
				if (orgNote == null)
				{
					StmNote newOrgNote = enterprisePart.Notes.AddNew();
					newOrgNote.ST_Description = noteType;
					newOrgNote.ST_Table = "OrgSupplierPart";
					newOrgNote.ST_NoteDataAsText = fullDescription;
				}
			}
		}

		protected bool LoadExportLookupIfItExists(MasterFiles.Business.OrgSupplierPart enterprisePart, string expLookup)
		{
			bool exportLookupLoaded = false;
			if (!string.IsNullOrEmpty(expLookup))
			{
				ZGuid classificationPK = GetClassificationPK(expLookup, "EXP");
				if (classificationPK.IsEmpty)
				{
					classificationPK = CreateLookupIfValidExportTariffAndAustralia(expLookup);
				}

				if (!classificationPK.IsEmpty)
				{
					LoadCusClassPartPivot(enterprisePart, classificationPK, "", "");
					exportLookupLoaded = true;
				}
			}

			return exportLookupLoaded;
		}

		protected ZGuid CreateLookupIfValidExportTariffAndAustralia(ZString exportLookup)
		{
			ZGuid classPKCreated = ZGuid.Empty;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				if (IsValidExportTariffCode(exportLookup))
				{
					Classification enterpriseClass = Factory.New<Classification>();
					enterpriseClass.CC_LookupCode = exportLookup;
					enterpriseClass.CC_TariffNum = exportLookup;
					enterpriseClass.CC_ClassificationType = "EXP";
					enterpriseClass.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					classPKCreated = enterpriseClass.PK;
				}
			}

			return classPKCreated;
		}

		protected abstract void DisplayFormatLogMessage(string detailedExceptionMessage);

		#endregion

		#region ExtractData

		protected virtual void ExtractRequiredClassificationFields(DataRow classRecord)
		{
			if (ProcessingExportClass)
			{
				ExtractExportClassificationFields(classRecord);
			}
			else
			{
				ExtractRequiredDeliveranceClassificationFields(classRecord);
			}
		}

		protected void ExtractRequiredDeliveranceClassificationFields(DataRow classRecord)
		{
			ClassCode = new ZString(classRecord[Constants.DeliveranceDataFields.Code]).Trim();
			ZString tariffStat = new ZString(classRecord[Constants.DeliveranceDataFields.Tstat]).Trim();
			if (tariffStat.Length > 2)
			{
				tariffStat = tariffStat.Replace("00", "");
			}

			Tariff = new ZString(classRecord[Constants.DeliveranceDataFields.Tariff]).Trim() + tariffStat;
			Description = new ZString(classRecord[Constants.DeliveranceDataFields.TDesc]).Trim();
			AddInfo = ConvertDeliveranceOriginCodeToEnterpriseOriginCodeIfPresent(new ZString(classRecord[Constants.DeliveranceDataFields.Tinfo]).Trim());
			InstrumentCode = new ZString(classRecord[Constants.DeliveranceDataFields.Tinstrno]).Trim();
			InstrumentType = new ZString(classRecord[Constants.DeliveranceDataFields.Tinstr]).Trim();
			TreatmentCode = new ZString(classRecord[Constants.DeliveranceDataFields.Ttrt]).Trim();
			IsAudited = (bool)classRecord[Constants.DeliveranceDataFields.Taudited];
			ClassType = "IMP";
		}

		protected void ExtractExportClassificationFields(DataRow classRecord)
		{
			ClassCode = new ZString(classRecord[Constants.DeliveranceDataFields.Ahec_lup]).Trim();
			Tariff = new ZString(classRecord[Constants.DeliveranceDataFields.Ahmf_code]).Trim();
			Description = new ZString(classRecord[Constants.DeliveranceDataFields.Ahdesc]).Trim();
			ClassType = "EXP";
		}

		protected bool ExtractRequiredPartFieldsIsOK(OleDbDataReader reader)
		{
			if (ProcessingExportParts)
			{
				return ExtractExportPartFields(reader);
			}
			else
			{
				return ExtractRequiredDeliverancePartFields(reader);
			}
		}

		protected bool ExtractRequiredDeliverancePartFields(OleDbDataReader reader)
		{
			DeliveranceSystemCode = new ZString(reader[Constants.DeliveranceDataFields.cp_uniqeid]).Left(6);
			PartNo = new ZString(reader[Constants.DeliveranceDataFields.Ppart]).Trim().Left(30);
			PartDesc = new ZString(reader[Constants.DeliveranceDataFields.PartName]).Trim().Left(80);
			if (PartDesc == "")
			{
				PartDesc = PartNo;
			}

			PartClass = new ZString(reader[Constants.DeliveranceDataFields.Tlink]).Trim();
			PartDivision = new ZString(reader[Constants.DeliveranceDataFields.Division]).Trim().Left(15);
			PartOwnerCode = new ZString(reader[Constants.DeliveranceDataFields.Impuse]).Trim();
			PartSupplierCode = new ZString(reader[Constants.DeliveranceDataFields.Supuse]).Trim();
			PartCount = (decimal)reader[Constants.DeliveranceDataFields.Pt_cnt];
			PartUQ = new ZString(reader[Constants.DeliveranceDataFields.cp_uq_purc]).Trim();
			PartLastCostAmount = (decimal)reader[Constants.DeliveranceDataFields.Lastcost];
			PartAddInfo = ConvertDeliveranceOriginCodeToEnterpriseOriginCodeIfPresent(new ZString(reader[Constants.DeliveranceDataFields.Pinfo]).Trim());
			PartTreatmentCode = new ZString(reader[Constants.DeliveranceDataFields.Ptrt]).Trim();
			PartWeight = (decimal)reader[Constants.DeliveranceDataFields.Punit_kg];
			PartUnitPerM3 = (decimal)reader[Constants.DeliveranceDataFields.Punit_m3];
			PartAudited = new ZBool(reader[Constants.DeliveranceDataFields.Paudited]);

			return OwnerAndSupplierExist();
		}

		bool OwnerAndSupplierExist()
		{
			bool linkedOrgsExist = true;
			PartOwnerCodePK = ZGuid.Empty;
			PartSupplierCodePK = ZGuid.Empty;
			if (!PartOwnerCode.IsEmpty)
			{
				PartOwnerCodePK = GetOwnerSupplierCodePK(PartOwnerCode);
				if (PartOwnerCodePK.IsEmpty)
				{
					linkedOrgsExist = false;
				}
			}

			if (!PartSupplierCode.IsEmpty)
			{
				PartSupplierCodePK = GetOwnerSupplierCodePK(PartSupplierCode);
				if (PartSupplierCodePK.IsEmpty)
				{
					linkedOrgsExist = false;
				}
			}

			return linkedOrgsExist;
		}

		protected bool ExtractExportPartFields(OleDbDataReader reader)
		{
			DeliveranceSystemCode = new ZString(reader[Constants.DeliveranceDataFields.xp_uniqeid]).Left(6);
			PartNo = new ZString(reader[Constants.DeliveranceDataFields.Xpart]).Trim().Left(30);
			PartDesc = new ZString(reader[Constants.DeliveranceDataFields.Xdesc]).Trim().Left(80);
			if (PartDesc == "")
			{
				PartDesc = PartNo;
			}

			PartClass = "";
			ExportPartClass = new ZString(reader[Constants.DeliveranceDataFields.Ahec_link]).Trim();
			PartCount = (decimal)reader[Constants.DeliveranceDataFields.Xpart_cnt];
			PartUQ = new ZString(reader[Constants.DeliveranceDataFields.Fuoq_uq]).Trim();
			PartLastCostAmount = (decimal)reader[Constants.DeliveranceDataFields.Xlast_cost];
			PartWeight = (decimal)reader[Constants.DeliveranceDataFields.Xprtkg];
			PartUnitPerM3 = (decimal)reader[Constants.DeliveranceDataFields.Xprtm3];
			PartSupplierCode = new ZString(reader[Constants.DeliveranceDataFields.Fexp_use]).Trim();

			return OwnerAndSupplierExist();
		}

		protected AUOrgSupplierPart GetPartIfItExists(ZString partNo, ZGuid ownerPK, ZGuid supplierPK)
		{
			AUOrgSupplierPart enterprisePart = null;
			ZQuery partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, partNo);
			AUOrgSupplierPart[] enterpriseParts = (AUOrgSupplierPart[])Factory.Load(typeof(AUOrgSupplierPart), partFilter);

			foreach (AUOrgSupplierPart checkPart in enterpriseParts)  // check owner & supplier
			{
				if (ownerPK.IsEmpty && supplierPK.IsEmpty)
				{
					enterprisePart = checkPart;
					break;
				}
				else
				{
					ZQuery checkOwnerFilter = new ZQuery(OrgPartRelationSchema.OU_OP, checkPart.PK);
					checkOwnerFilter.AddToFilter(OrgPartRelationSchema.OU_OH, ownerPK);
					checkOwnerFilter.AddToFilter(OrgPartRelationSchema.OU_Relationship, "OWN");
					OrgPartRelation partOwner = Factory.LoadTop1<OrgPartRelation>(checkOwnerFilter);

					if (partOwner == null) // Check if part exists for supplier
					{
						ZQuery checkSupplierFilter = new ZQuery(OrgPartRelationSchema.OU_OP, checkPart.PK);
						checkSupplierFilter.AddToFilter(OrgPartRelationSchema.OU_OH, supplierPK);
						checkSupplierFilter.AddToFilter(OrgPartRelationSchema.OU_Relationship, "SUP");
						OrgPartRelation partSupplier = Factory.LoadTop1<OrgPartRelation>(checkSupplierFilter);

						if (partSupplier != null)
						{
							enterprisePart = checkPart;
							break;
						}
					}
					else if (supplierPK.IsEmpty) // part already exists for this owner only
					{
						enterprisePart = checkPart;
						break;
					}
					else // check if part exists for owner & supplier
					{
						ZQuery checkSupplierFilter = new ZQuery(OrgPartRelationSchema.OU_OP, checkPart.PK);
						checkSupplierFilter.AddToFilter(OrgPartRelationSchema.OU_OH, supplierPK);
						checkSupplierFilter.AddToFilter(OrgPartRelationSchema.OU_Relationship, "SUP");
						OrgPartRelation partSupplier = Factory.LoadTop1<OrgPartRelation>(checkSupplierFilter);

						if (partSupplier != null)
						{
							enterprisePart = checkPart;
							break;
						}
					}
				}
			}

			return enterprisePart;
		}

		protected virtual ZGuid GetOwnerPK()
		{
			ZGuid ownerPK = ZGuid.Empty;

			if (PartOwnerCode == SavedPartOwnerCode)
			{
				ownerPK = SavedPartOwnerCodePK;
			}
			else
			{
				SavedPartOwnerCode = PartOwnerCode;
				ownerPK = SavedPartOwnerCodePK = GetOwnerSupplierCodePK(PartOwnerCode);
			}

			return ownerPK;
		}

		/// <summary>
		/// Search for organisation will try and match to a Deliverance Code only as only loading from Deliverance
		/// </summary>
		protected virtual ZGuid GetOwnerSupplierCodePK(string ownerSupplierLegacyCode)
		{
			ZGuid ownerSupplierPK = ZGuid.Empty;
			OrgHeader enterpriseOrg = GetOrgFromDeliveranceCode(DeliveranceSystemCode + "_" + ownerSupplierLegacyCode);
			if (enterpriseOrg != null)
			{
				ownerSupplierPK = enterpriseOrg.PK;
			}

			return ownerSupplierPK;
		}

		protected OrgHeader GetOrgFromDeliveranceCode(ZString orgLegacyCode)
		{
			OrgHeader org = null;
			ZQuery codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.DeliveranceCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, orgLegacyCode);
			OrgCusCode orgLegacyCodeRecord = Factory.LoadTop1<OrgCusCode>(codeFilter);
			if (orgLegacyCodeRecord != null)
			{
				org = Factory.Load<OrgHeader>(orgLegacyCodeRecord.OK_OH);
			}
			return org;
		}

		protected ZGuid GetClassificationPK(string partClassCode, string classType)
		{
			ZGuid classificationPK = ZGuid.Empty;
			ZQuery classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, partClassCode);
			classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, classType);
			BaseCusClassification classification = Factory.LoadTop1<BaseCusClassification>(classFilter);
			if (classification != null)
			{
				classificationPK = classification.PK;
			}
			return classificationPK;
		}

		protected bool IsValidExportTariffCode(ZString tariffCode) => AUCAHECCWrapper.Load(Factory, tariffCode, ZDateTime.Today) != null;

		bool ClassificationLookupExists(ZString impLookup, ZString expLookup)
		{
			bool classOK = true;
			if (!impLookup.IsEmpty)
			{
				ZGuid classificationPK = GetClassificationPK(impLookup, "IMP");
				if (classificationPK.IsEmpty)
				{
					classOK = false;
				}
			}

			if (!expLookup.IsEmpty && classOK)
			{
				ZGuid classificationPK = GetClassificationPK(expLookup, "EXP");
				if (classificationPK.IsEmpty)
				{
					classificationPK = CreateLookupIfValidExportTariffAndAustralia(expLookup);
					classOK = !classificationPK.IsEmpty;
				}
			}

			return classOK;
		}

		#endregion

		#region Handle Deliverance Orgin Code

		ZString ConvertDeliveranceOriginCodeToEnterpriseOriginCodeIfPresent(ZString addInfo)
		{
			if (addInfo.Contains("ORG="))
			{
				addInfo = MapDeliveranceOriginToEnterpriseOrigin(addInfo);
			}

			return addInfo;
		}

		ZString MapDeliveranceOriginToEnterpriseOrigin(ZString addInfo)
		{
			ZString[] addInfoFields = addInfo.Split('*');
			ZString deliveranceCountryCode = "";
			ZString enterpriseCountryCode = "";

			for (int i = 0; i < addInfoFields.Length; i++)
			{
				if (addInfoFields[i].Contains("ORG="))
				{
					deliveranceCountryCode = addInfoFields[i].Split('=')[1];

					/// These values have been obtained from the deliverance fox directory, table cusecou
					switch (deliveranceCountryCode)
					{
						case "UAEM":
							enterpriseCountryCode = "AE";
							break;
						case "AFGH":
							enterpriseCountryCode = "AF";
							break;
						case "AGUA":
							enterpriseCountryCode = "AG";
							break;
						case "ANGA":
							enterpriseCountryCode = "AI";
							break;
						case "ALBA":
							enterpriseCountryCode = "AL";
							break;
						case "ARMN":
							enterpriseCountryCode = "AM";
							break;
						case "ANTI":
							enterpriseCountryCode = "AN";
							break;
						case "ANGO":
							enterpriseCountryCode = "AO";
							break;
						case "ARGE":
							enterpriseCountryCode = "AR";
							break;
						case "SAMO":
							enterpriseCountryCode = "AS";
							break;
						case "ASTA":
							enterpriseCountryCode = "AT";
							break;
						case "AUST":
							enterpriseCountryCode = "AU";
							break;
						case "AZBJ":
							enterpriseCountryCode = "AZ";
							break;
						case "BOHR":
							enterpriseCountryCode = "BA";
							break;
						case "BARB":
							enterpriseCountryCode = "BB";
							break;
						case "BADE":
							enterpriseCountryCode = "BD";
							break;
						case "BLGM":
							enterpriseCountryCode = "BE";
							break;
						case "BURK":
							enterpriseCountryCode = "BF";
							break;
						case "BULG":
							enterpriseCountryCode = "BG";
							break;
						case "BHRN":
							enterpriseCountryCode = "BH";
							break;
						case "BRND":
							enterpriseCountryCode = "BI";
							break;
						case "BENR":
							enterpriseCountryCode = "BJ";
							break;
						case "BMDA":
							enterpriseCountryCode = "BM";
							break;
						case "BRUN":
							enterpriseCountryCode = "BN";
							break;
						case "BOLI":
							enterpriseCountryCode = "BO";
							break;
						case "BRAZ":
							enterpriseCountryCode = "BR";
							break;
						case "BAHA":
							enterpriseCountryCode = "BS";
							break;
						case "BHUT":
							enterpriseCountryCode = "BT";
							break;
						case "BOTS":
							enterpriseCountryCode = "BW";
							break;
						case "BELA":
							enterpriseCountryCode = "BY";
							break;
						case "BELE":
							enterpriseCountryCode = "BZ";
							break;
						case "CAN":
							enterpriseCountryCode = "CA";
							break;
						case "COCO":
							enterpriseCountryCode = "CC";
							break;
						case "CEAR":
							enterpriseCountryCode = "CF";
							break;
						case "COBR":
							enterpriseCountryCode = "CG";
							break;
						case "SWIT":
							enterpriseCountryCode = "CH";
							break;
						case "IVOR":
							enterpriseCountryCode = "CI";
							break;
						case "COOK":
							enterpriseCountryCode = "CK";
							break;
						case "CHLE":
							enterpriseCountryCode = "CL";
							break;
						case "CHIN":
							enterpriseCountryCode = "CN";
							break;
						case "COMB":
							enterpriseCountryCode = "CO";
							break;
						case "COST":
							enterpriseCountryCode = "CR";
							break;
						case "CUBA":
							enterpriseCountryCode = "CU";
							break;
						case "CVER":
							enterpriseCountryCode = "CV";
							break;
						case "CHRI":
							enterpriseCountryCode = "CX";
							break;
						case "CYPR":
							enterpriseCountryCode = "CY";
							break;
						case "CZEH":
							enterpriseCountryCode = "CZ";
							break;
						case "GDR":
							enterpriseCountryCode = "DD";
							break;
						case "FGMY":
							enterpriseCountryCode = "DE";
							break;
						case "DJIB":
							enterpriseCountryCode = "DJ";
							break;
						case "DENM":
							enterpriseCountryCode = "DK";
							break;
						case "DMCA":
							enterpriseCountryCode = "DM";
							break;
						case "DOMI":
							enterpriseCountryCode = "DO";
							break;
						case "ALGR":
							enterpriseCountryCode = "DZ";
							break;
						case "ECUA":
							enterpriseCountryCode = "EC";
							break;
						case "ESTO":
							enterpriseCountryCode = "EE";
							break;
						case "EGYP":
							enterpriseCountryCode = "EG";
							break;
						case "SARA":
							enterpriseCountryCode = "EH";
							break;
						case "ERIT":
							enterpriseCountryCode = "ER";
							break;
						case "SPAI":
							enterpriseCountryCode = "ES";
							break;
						case "ETHI":
							enterpriseCountryCode = "ET";
							break;
						case "FINL":
							enterpriseCountryCode = "FI";
							break;
						case "FIJI":
							enterpriseCountryCode = "FJ";
							break;
						case "FALK":
							enterpriseCountryCode = "FK";
							break;
						case "MICR":
							enterpriseCountryCode = "FM";
							break;
						case "FRAN":
							enterpriseCountryCode = "FR";
							break;
						case "GABO":
							enterpriseCountryCode = "GA";
							break;
						case "UK":
							enterpriseCountryCode = "GB";
							break;
						case "GNDA":
							enterpriseCountryCode = "GD";
							break;
						case "GERG":
							enterpriseCountryCode = "GE";
							break;
						case "FRGU":
							enterpriseCountryCode = "GF";
							break;
						case "GHAN":
							enterpriseCountryCode = "GH";
							break;
						case "GIBR":
							enterpriseCountryCode = "GI";
							break;
						case "GAMB":
							enterpriseCountryCode = "GM";
							break;
						case "GUIN":
							enterpriseCountryCode = "GN";
							break;
						case "EGUI":
							enterpriseCountryCode = "GQ";
							break;
						case "GREE":
							enterpriseCountryCode = "GR";
							break;
						case "GMLA":
							enterpriseCountryCode = "GT";
							break;
						case "GUAM":
							enterpriseCountryCode = "GU";
							break;
						case "BGUI":
							enterpriseCountryCode = "GW";
							break;
						case "GUYA":
							enterpriseCountryCode = "GY";
							break;
						case "HONG":
							enterpriseCountryCode = "HK";
							break;
						case "HDRS":
							enterpriseCountryCode = "HN";
							break;
						case "CROA":
							enterpriseCountryCode = "HR";
							break;
						case "HAIT":
							enterpriseCountryCode = "HT";
							break;
						case "HGRY":
							enterpriseCountryCode = "HU";
							break;
						case "INDO":
							enterpriseCountryCode = "ID";
							break;
						case "IRE":
							enterpriseCountryCode = "IE";
							break;
						case "ISRA":
							enterpriseCountryCode = "IL";
							break;
						case "INIA":
							enterpriseCountryCode = "IN";
							break;
						case "BIOT":
							enterpriseCountryCode = "IO";
							break;
						case "IRAQ":
							enterpriseCountryCode = "IQ";
							break;
						case "PSIA":
							enterpriseCountryCode = "IR";
							break;
						case "ICEL":
							enterpriseCountryCode = "IS";
							break;
						case "ITAL":
							enterpriseCountryCode = "IT";
							break;
						case "JMCA":
							enterpriseCountryCode = "JM";
							break;
						case "JORD":
							enterpriseCountryCode = "JO";
							break;
						case "JAP":
							enterpriseCountryCode = "JP";
							break;
						case "JSIS":
							enterpriseCountryCode = "JT";
							break;
						case "KENY":
							enterpriseCountryCode = "KE";
							break;
						case "KYRG":
							enterpriseCountryCode = "KG";
							break;
						case "CMBD":
							enterpriseCountryCode = "KH";
							break;
						case "KIRI":
							enterpriseCountryCode = "KI";
							break;
						case "CMRO":
							enterpriseCountryCode = "KM";
							break;
						case "STCH":
						case "STCN":
							enterpriseCountryCode = "KN";
							break;
						case "KRDR":
							enterpriseCountryCode = "KP";
							break;
						case "RKOR":
							enterpriseCountryCode = "KR";
							break;
						case "KUWA":
							enterpriseCountryCode = "KW";
							break;
						case "CAYM":
							enterpriseCountryCode = "KY";
							break;
						case "KAZA":
							enterpriseCountryCode = "KZ";
							break;
						case "LAOS":
							enterpriseCountryCode = "LA";
							break;
						case "LEBA":
							enterpriseCountryCode = "LB";
							break;
						case "STLU":
							enterpriseCountryCode = "LC";
							break;
						case "SRIL":
							enterpriseCountryCode = "LK";
							break;
						case "LIBE":
							enterpriseCountryCode = "LR";
							break;
						case "LESO":
							enterpriseCountryCode = "LS";
							break;
						case "LITH":
							enterpriseCountryCode = "LT";
							break;
						case "LATV":
							enterpriseCountryCode = "LV";
							break;
						case "LBYA":
							enterpriseCountryCode = "LY";
							break;
						case "MORO":
							enterpriseCountryCode = "MA";
							break;
						case "MDOV":
							enterpriseCountryCode = "MD";
							break;
						case "MASY":
							enterpriseCountryCode = "MG";
							break;
						case "MARS":
							enterpriseCountryCode = "MH";
							break;
						case "MIDW":
							enterpriseCountryCode = "MI";
							break;
						case "FYRM":
							enterpriseCountryCode = "MK";
							break;
						case "MALI":
							enterpriseCountryCode = "ML";
							break;
						case "BURM":
							enterpriseCountryCode = "MM";
							break;
						case "MNGL":
							enterpriseCountryCode = "MN";
							break;
						case "MACA":
							enterpriseCountryCode = "MO";
							break;
						case "MRNS":
							enterpriseCountryCode = "MP";
							break;
						case "MRTN":
							enterpriseCountryCode = "MR";
							break;
						case "MONT":
							enterpriseCountryCode = "MS";
							break;
						case "MLTA":
							enterpriseCountryCode = "MT";
							break;
						case "MAUS":
							enterpriseCountryCode = "MU";
							break;
						case "MLDV":
							enterpriseCountryCode = "MV";
							break;
						case "MLWI":
							enterpriseCountryCode = "MW";
							break;
						case "MEXI":
							enterpriseCountryCode = "MX";
							break;
						case "MLAY":
							enterpriseCountryCode = "MY";
							break;
						case "MOZA":
							enterpriseCountryCode = "MZ";
							break;
						case "NAMI":
							enterpriseCountryCode = "NA";
							break;
						case "NCAL":
							enterpriseCountryCode = "NC";
							break;
						case "NIGE":
							enterpriseCountryCode = "NE";
							break;
						case "NORF":
							enterpriseCountryCode = "NF";
							break;
						case "NGRA":
							enterpriseCountryCode = "NG";
							break;
						case "NICA":
							enterpriseCountryCode = "NI";
							break;
						case "NETH":
							enterpriseCountryCode = "NL";
							break;
						case "NWAY":
							enterpriseCountryCode = "NO";
							break;
						case "NEPA":
							enterpriseCountryCode = "NP";
							break;
						case "NAUR":
							enterpriseCountryCode = "NR";
							break;
						case "NIUE":
							enterpriseCountryCode = "NU";
							break;
						case "NZ":
							enterpriseCountryCode = "NZ";
							break;
						case "OMAN":
							enterpriseCountryCode = "OM";
							break;
						case "PNMA":
							enterpriseCountryCode = "PA";
							break;
						case "PERU":
							enterpriseCountryCode = "PE";
							break;
						case "PLYN":
							enterpriseCountryCode = "PF";
							break;
						case "PNG":
							enterpriseCountryCode = "PG";
							break;
						case "PHIL":
							enterpriseCountryCode = "PH";
							break;
						case "PAKI":
							enterpriseCountryCode = "PK";
							break;
						case "POLA":
							enterpriseCountryCode = "PL";
							break;
						case "PIER":
							enterpriseCountryCode = "PM";
							break;
						case "PITC":
							enterpriseCountryCode = "PN";
							break;
						case "RICO":
							enterpriseCountryCode = "PR";
							break;
						case "PORT":
							enterpriseCountryCode = "PT";
							break;
						case "OAPI":
							enterpriseCountryCode = "PU";
							break;
						case "PALU":
							enterpriseCountryCode = "PW";
							break;
						case "PRGY":
							enterpriseCountryCode = "PY";
							break;
						case "QATA":
							enterpriseCountryCode = "QA";
							break;
						case "REUN":
							enterpriseCountryCode = "RE";
							break;
						case "ROUM":
							enterpriseCountryCode = "RO";
							break;
						case "RUSS":
							enterpriseCountryCode = "RU";
							break;
						case "RWAN":
							enterpriseCountryCode = "RW";
							break;
						case "SAUD":
							enterpriseCountryCode = "SA";
							break;
						case "SOLO":
							enterpriseCountryCode = "SB";
							break;
						case "SEYC":
							enterpriseCountryCode = "SC";
							break;
						case "SUDA":
							enterpriseCountryCode = "SD";
							break;
						case "SWED":
							enterpriseCountryCode = "SE";
							break;
						case "SING":
							enterpriseCountryCode = "SG";
							break;
						case "STHE":
							enterpriseCountryCode = "SH";
							break;
						case "SLOV":
							enterpriseCountryCode = "SI";
							break;
						case "SVAK":
							enterpriseCountryCode = "SK";
							break;
						case "SLEO":
							enterpriseCountryCode = "SL";
							break;
						case "SENE":
							enterpriseCountryCode = "SN";
							break;
						case "SOML":
							enterpriseCountryCode = "SO";
							break;
						case "SRNM":
							enterpriseCountryCode = "SR";
							break;
						case "SAOT":
							enterpriseCountryCode = "ST";
							break;
						case "USSR":
							enterpriseCountryCode = "SU";
							break;
						case "SALV":
							enterpriseCountryCode = "SV";
							break;
						case "SYRI":
							enterpriseCountryCode = "SY";
							break;
						case "SWZI":
							enterpriseCountryCode = "SZ";
							break;
						case "TRCA":
							enterpriseCountryCode = "TC";
							break;
						case "CHAD":
							enterpriseCountryCode = "TD";
							break;
						case "FSAT":
							enterpriseCountryCode = "TF";
							break;
						case "TOGO":
							enterpriseCountryCode = "TG";
							break;
						case "THAI":
							enterpriseCountryCode = "TH";
							break;
						case "TAJI":
							enterpriseCountryCode = "TJ";
							break;
						case "TOKI":
							enterpriseCountryCode = "TK";
							break;
						case "TURS":
							enterpriseCountryCode = "TM";
							break;
						case "TUNI":
							enterpriseCountryCode = "TN";
							break;
						case "TNGA":
							enterpriseCountryCode = "TO";
							break;
						case "ETIM":
							enterpriseCountryCode = "TP";
							break;
						case "TURK":
							enterpriseCountryCode = "TR";
							break;
						case "TRIN":
							enterpriseCountryCode = "TT";
							break;
						case "TUVALU":
							enterpriseCountryCode = "TV";
							break;
						case "TAIW":
							enterpriseCountryCode = "TW";
							break;
						case "TANZ":
							enterpriseCountryCode = "TZ";
							break;
						case "UKRA":
							enterpriseCountryCode = "UA";
							break;
						case "UGAN":
							enterpriseCountryCode = "UG";
							break;
						case "USA":
							enterpriseCountryCode = "US";
							break;
						case "URUG":
							enterpriseCountryCode = "UY";
							break;
						case "UZBK":
							enterpriseCountryCode = "UZ";
							break;
						case "STVI":
							enterpriseCountryCode = "VC";
							break;
						case "VENZ":
							enterpriseCountryCode = "VE";
							break;
						case "BVIR":
							enterpriseCountryCode = "VG";
							break;
						case "VIRG":
							enterpriseCountryCode = "VI";
							break;
						case "VIET":
							enterpriseCountryCode = "VN";
							break;
						case "VANU":
							enterpriseCountryCode = "VU";
							break;
						case "WALL":
							enterpriseCountryCode = "WF";
							break;
						case "WAKE":
							enterpriseCountryCode = "WK";
							break;
						case "WSAM":
							enterpriseCountryCode = "WS";
							break;
						case "TTPI":
							enterpriseCountryCode = "XA";
							break;
						case "EEC":
							enterpriseCountryCode = "XE";
							break;
						case "EUR":
							enterpriseCountryCode = "XE";
							break;
						case "ZONA":
							enterpriseCountryCode = "XZ";
							break;
						case "AYRE":
							enterpriseCountryCode = "YE";
							break;
						case "YEMN":
							enterpriseCountryCode = "YE";
							break;
						case "YUGO":
							enterpriseCountryCode = "YU";
							break;
						case "SAFR":
							enterpriseCountryCode = "ZA";
							break;
						case "ZMBA":
							enterpriseCountryCode = "ZM";
							break;
						case "ZAIR":
							enterpriseCountryCode = "ZR";
							break;
						case "ZIMB":
							enterpriseCountryCode = "ZW";
							break;
						default:
							enterpriseCountryCode = deliveranceCountryCode;
							break;
					}
				}
			}

			ZString deliveranceValue = "ORG=" + deliveranceCountryCode;
			ZString enterpriseValue = "ORG=" + enterpriseCountryCode;
			addInfo = addInfo.Replace(deliveranceValue, enterpriseValue);

			return addInfo;
		}

		#endregion
	}
}
