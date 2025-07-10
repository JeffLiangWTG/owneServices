using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgSupplierPartDataSave : DataSaveWithColumns
	{
		public OrgSupplierPartDataSave() { }

		public void ExportProductData(string dataLocation)
		{
			ExportData(dataLocation, "Product");
		}

		#region Export to .csv file

		protected override OCsvLine ProcessDataForThisBizo(BusinessObject bizo)
		{
			OCsvLine result = new OCsvLine(ZString.Empty);
			OrgSupplierPart orgSupplierPart = bizo as OrgSupplierPart;
			if (orgSupplierPart != null)
			{
				try
				{
					result = MapRecordToCsvLine(MapToRecord(orgSupplierPart, NewPartsDataToSave()));
					RunCounters.LinesCreated++;
				}
				catch (ArgumentException ex)
				{
					RunCounters.BizosExcluded++;
					DisplayLogMessage(Res.GetString("b766849a-40c5-4b28-9f5e-49ce3ce68490", "Row {0} excluded... data is inconsistent with required format.", RunCounters.CurrentRow.ToString()));
					DisplayLogMessage(ex.Message);
					RunCounters.BizosExcluded++;
				}
				OnProgressChanged();
			}
			return result;
		}

		protected virtual PartsDataToSave MapToRecord(OrgSupplierPart bizo, PartsDataToSave record)
		{
			record.PartNo = bizo.OP_PartNum;
			record.PartDesc = bizo.OP_Desc;
			record.PartFullDesc = String.Join(" ", ((from StmNote n in bizo.Notes.FindByDescription("Full Product Description") select n.ST_NoteDataAsText.Replace("\r\n", "").ToString()).ToArray()));
			record.PartDepartment = bizo.OP_Department;
			record.PartDivision = bizo.OP_Division;
			record.PartCount = bizo.OP_QtyInStock;
			record.PartUQ = bizo.OP_StockKeepingUnit;
			record.PartWeight = bizo.OP_Weight;
			record.PartWeightUnit = bizo.OP_WeightUQ;
			record.PartVolume = bizo.OP_Cubic;
			record.PartVolumeUnit = bizo.OP_CubicUQ;
			record.PartLastCost = bizo.OP_LastCost;

			if (bizo.UNDGs != null && bizo.UNDGs.Count > 0)
			{
				record.PartUNDGCode = bizo.UNDGs[0].Substance.DG_Code;
				record.PartUNDGPK = bizo.UNDGs[0].Substance.PK;
			}

			record.Commodity = bizo.OP_RH_NKCommodityCode;
			record.PartBrandName = bizo.OP_Brand;
			record.PartModel = bizo.OP_Model;

			MapSupplierOwnerDetails(bizo, record);
			MapUnitConversions(bizo, record);
			MapCountrySpecificClassification(bizo, record);
			MapCountrySpecificTariff(bizo, "IMP", record);
			MapCountrySpecificTariff(bizo, "EXP", record);
			return record;
		}

		protected virtual void MapSupplierOwnerDetails(OrgSupplierPart bizo, PartsDataToSave record)
		{
			OrgPartRelationCollection relations = bizo.RelatedOrganisations;
			if (relations != null)
			{
				record.PartOwnerCodePK = new List<ZGuid>();
				record.PartOwnerCode = new List<ZString>();
				record.PartSupplierCodePK = new List<ZGuid>();
				record.PartSupplierCode = new List<ZString>();

				foreach (OrgPartRelation orgPartRelation in bizo.RelatedOrganisations)
				{
					OrgHeader ownerSupplier = null;
					switch (orgPartRelation.OU_Relationship)
					{
						case Business.OrgPartRelation.RelationshipTypes.Supplier:
							record.PartSupplierCodePK.Add(orgPartRelation.OU_OH);
							ownerSupplier = Factory.Load<OrgHeader>(orgPartRelation.OU_OH);
							record.PartSupplierCode.Add(ownerSupplier.OH_Code);
							break;

						case Business.OrgPartRelation.RelationshipTypes.Both:
							record.PartSupplierCodePK.Add(orgPartRelation.OU_OH);
							record.PartOwnerCodePK.Add(orgPartRelation.OU_OH);
							ownerSupplier = Factory.Load<OrgHeader>(orgPartRelation.OU_OH);
							record.PartSupplierCode.Add(ownerSupplier.OH_Code);
							record.PartOwnerCode.Add(ownerSupplier.OH_Code);
							break;

						case Business.OrgPartRelation.RelationshipTypes.Owner:
							record.PartOwnerCodePK.Add(orgPartRelation.OU_OH);
							ownerSupplier = Factory.Load<OrgHeader>(orgPartRelation.OU_OH);
							record.PartOwnerCode.Add(ownerSupplier.OH_Code);
							break;
					}
				}
			}
		}

		protected virtual void MapUnitConversions(OrgSupplierPart bizo, PartsDataToSave record)
		{
			int counter = 0;
			record.UnitConversions = new List<UnitConversion>(5);
			foreach (OrgPartUnit orgPartUnit in bizo.PartUnits)
			{
				if (counter < 5)
				{
					UnitConversion unitConversion = new UnitConversion(orgPartUnit.OF_QuantityInParent, orgPartUnit.OF_PackType, orgPartUnit.OF_ParentPackType);
					if (!IsUnitConversionARepeatFromTheOrgSupplierPartBizo(unitConversion, bizo))
					{
						record.UnitConversions.Add(unitConversion);
						counter++;
					}
				}
				else
				{
					break;
				}
			}
		}

		bool IsUnitConversionARepeatFromTheOrgSupplierPartBizo(UnitConversion unitConversion, OrgSupplierPart bizo)
		{
			return unitConversion.ParentPackageType == bizo.OP_StockKeepingUnit &&
						((unitConversion.QuantityInParent == bizo.OP_Weight && unitConversion.PackageType == bizo.OP_WeightUQ) ||
						(unitConversion.QuantityInParent == bizo.OP_Cubic && unitConversion.PackageType == bizo.OP_CubicUQ));
		}

		protected virtual void MapCountrySpecificClassification(OrgSupplierPart bizo, PartsDataToSave record)
		{
			record.PartOrigin = ZString.Empty;
			record.PartClassification = ZString.Empty;
			record.PartExportClassification = ZString.Empty;
		}

		protected virtual void MapCountrySpecificTariff(OrgSupplierPart bizo, string classType, PartsDataToSave record)
		{
			record.ImportTariff = ZString.Empty;
			record.ExportTariff = ZString.Empty;
		}

		OCsvLine MapRecordToCsvLine(PartsDataToSave record)
		{
			return new OCsvLine(ConvertRecordToFieldList(record).ToArray());
		}

		protected virtual IList<String> ConvertRecordToFieldList(PartsDataToSave record)
		{
			List<String> result = new List<String>(35);
			result.Add(record.PartNo);
			result.Add(record.PartFullDesc.IsEmpty ? record.PartDesc : record.PartFullDesc);
			result.Add(record.PartUQ);
			result.Add(record.PartExportClassification);
			result.Add(record.PartClassification);
			result.Add(ZString.Join(";", record.PartOwnerCode.ToArray()));
			result.Add(ZString.Join(";", record.PartSupplierCode.ToArray()));
			result.Add(record.PartWeight.ToString("0.000"));
			result.Add(record.PartWeightUnit);
			result.Add(record.PartVolume.ToString("0.000"));
			result.Add(record.PartVolumeUnit);
			result.Add(record.PartDepartment);
			result.Add(record.PartDivision);
			result.Add(record.PartCount.ToString("0.000"));
			result.Add(record.PartOrigin);
			result.Add(record.PartLastCost.ToString("0.000"));
			result.Add(record.PartUNDGCode);
			result.Add(record.LocalPartNumber);
			result.Add(record.LocalPartDescription);
			result.Add(record.ImportTariff);
			result.Add(record.ExportTariff);
			result.Add(record.Use_Attribute1 ? "Y" : "N");
			result.Add(record.Use_Attribute2 ? "Y" : "N");
			result.Add(record.Use_Attribute3 ? "Y" : "N");
			result.Add(record.UC1_QtyParent);
			result.Add(record.UC2_QtyParent);
			result.Add(record.UC3_QtyParent);
			result.Add(record.UC4_QtyParent);
			result.Add(record.UC5_QtyParent);
			result.Add(record.UC1_Package);
			result.Add(record.UC2_Package);
			result.Add(record.UC3_Package);
			result.Add(record.UC4_Package);
			result.Add(record.UC5_Package);
			result.Add(record.UC1_ParentPackage);
			result.Add(record.UC2_ParentPackage);
			result.Add(record.UC3_ParentPackage);
			result.Add(record.UC4_ParentPackage);
			result.Add(record.UC5_ParentPackage);
			result.Add(record.Commodity);
			result.Add(record.PartBrandName);
			result.Add(record.PartModel);
			return result;
		}

		protected override List<String> NewColumnNames()
		{
			List<String> result = new List<String>(42);
			result.Add("Code");
			result.Add("Description");
			result.Add("UQ");
			result.Add("ExportClassification");
			result.Add("ImportClassification");
			result.Add("Owner");
			result.Add("Supplier");
			result.Add("Unit_Weight");
			result.Add("Weight_Unit");
			result.Add("Unit_Volume");
			result.Add("Volume_Unit");
			result.Add("Department");
			result.Add("Division");
			result.Add("QtyInStock");
			result.Add("Origin");
			result.Add("Last_Cost");
			result.Add("UNDG_Code");
			result.Add("LocalPartNumber");
			result.Add("LocalPartDescription");
			result.Add("ImportTariff");
			result.Add("ExportTariff");
			result.Add("Use_Attribute1");
			result.Add("Use_Attribute2");
			result.Add("Use_Attribute3");
			result.Add("UC1_QtyParent");
			result.Add("UC2_QtyParent");
			result.Add("UC3_QtyParent");
			result.Add("UC4_QtyParent");
			result.Add("UC5_QtyParent");
			result.Add("UC1_Package");
			result.Add("UC2_Package");
			result.Add("UC3_Package");
			result.Add("UC4_Package");
			result.Add("UC5_Package");
			result.Add("UC1_ParentPackage");
			result.Add("UC2_ParentPackage");
			result.Add("UC3_ParentPackage");
			result.Add("UC4_ParentPackage");
			result.Add("UC5_ParentPackage");
			result.Add("Commodity");
			result.Add("BrandName");
			result.Add("Model");
			return result;
		}

		protected virtual PartsDataToSave NewPartsDataToSave()
		{
			return new PartsDataToSave();
		}

		#endregion

		protected override BusinessObjectCollection NewBusinessObjectCollection()
		{
			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory);
			collection.Load();
			return collection;
		}
	}
}
