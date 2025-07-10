using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCusSCAPivotDataObjectWriter : CusSCAPivotDataObjectWriter<CusSCAPivot>
	{
		public CMRCusSCAPivotDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{ }

		protected override CargoWise.Integration.ICodeDescriptionPairList GetPackageTypes(CusSCAPivot bizObj)
		{
			return bizObj.Lookups.PackageTypes;
		}

		protected override CargoWise.Integration.ICodeDescriptionPairList GetUnitsOfWeight(CusSCAPivot bizObj)
		{
			return bizObj.CV_WeightUQ_List;
		}

		protected override void PopulateCountrySpecificData(CusSCAPivot bizObj, PackingLine data, bool keepExistingData)
		{
			base.PopulateCountrySpecificData(bizObj, data, keepExistingData);
			var bizObjRow = (IColumnIndexer)bizObj;
			data.RequiresFumigationCertificate = PopulateValue(data.RequiresFumigationCertificate, keepExistingData,
				() => bizObjRow.GetValue(CusSCAPivotSchema.CV_FumigationCert));
			data.IsPersonalEffects = PopulateValue(data.IsPersonalEffects, keepExistingData,
				() => bizObjRow.GetValue(CusSCAPivotSchema.CV_PersonalEffects));
			data.IsTimber = PopulateValue(data.IsTimber, keepExistingData, () => bizObjRow.GetValue(CusSCAPivotSchema.CV_Timber));
			data.IsPerishable = PopulateValue(data.IsPerishable, keepExistingData, () => bizObjRow.GetValue(CusSCAPivotSchema.CV_PerishableGoods));
			data.IsHVLVClearance = PopulateValue(data.IsHVLVClearance, keepExistingData, () => bizObjRow.GetValue(CusSCAPivotSchema.CV_IsSAC));
			data.IsFlammable = PopulateValue(data.IsFlammable, keepExistingData, () => bizObjRow.GetValue(CusSCAPivotSchema.CV_Flammable));
		}

		protected override void WriteUNDGCollection(CusSCAPivot bizObj, PackingLine data, bool keepExistingData)
		{
			data.SetUNDGCollection(() =>
			{
				if (!keepExistingData && bizObj.GetValue(CusSCAPivotSchema.CV_HazardousGoods))
				{
					var undgs = new List<UNDG>();
					var undg = new UNDG(writeManager.WriterStrategy) { UNDGCode = Enterprise.Customs.Business.YesNoList.Codes.Yes };
					undgs.Add(undg);
					return undgs;
				}
				return data.UNDGCollection;
			});
		}
	}
}
