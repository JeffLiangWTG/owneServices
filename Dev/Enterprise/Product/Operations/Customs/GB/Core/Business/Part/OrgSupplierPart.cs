using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.MasterFiles;
using EUMasterFiles = Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.GB.Business
{
	public class OrgSupplierPart : EUMasterFiles.OrgSupplierPart, Integration.Customs.GB.IOrgSupplierPart, ISupportMultipleResourceStringData
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)(ICusClassPartPivotCollection<BaseCusClassPartPivot>)base.PivotsForBinding;

		public CusClassPartPivot SelectedPivot { get; set; }

		public IReadOnlyList<string> MultipleKeysToUse
		{
			get
			{
				if (SelectedPivot == null && PivotsForBinding.Count > 0)
				{
					SelectedPivot = PivotsForBinding[0];
				}

				if (SelectedPivot != null && !SelectedPivot.IsDeleted && ((EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport)SelectedPivot).IsExport)
				{
					return new[] { JobDeclaration.MultipleKeyCdsExport };
				}
				return new[] { JobDeclaration.MultipleKeyCdsImport };
			}
		}

		[ResourceStringData("563F3E0F-F5BF-4503-AEB0-4BDDE606C393", Caption = "[33] Supplements", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("20EB5DDA-9232-4FCD-AD68-426F8E42874B", Caption = "[UCC 6/16 & 6/17] Additional Codes")]
		public ZString SupplementsCaption => CaptionForProperty(nameof(SupplementsCaption));

		[ResourceStringData("1EE136AD-CD57-4FBB-99D1-404510244B49", Caption = "[44] Supporting Documents", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("D7FF7490-38C1-4499-858A-C80CB2F16914", Caption = "[UCC 2/3 && 8/7] Supporting Documents")]
		public ZString SupportingDocumentsCaption => CaptionForProperty(nameof(SupportingDocumentsCaption));

		[ResourceStringData("0B38F9F8-A63A-48B8-BB3D-901161E2D7F3", Caption = "[44] Additional Info", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("47B8137F-3FB2-4A38-B109-17E0F9AC1776", Caption = "[UCC 2/2] Additional Info")]
		public ZString AdditionalInfosCaption => CaptionForProperty(nameof(AdditionalInfosCaption));

		[ResourceStringData("DDE4776F-533C-42B5-A8BE-EBDEBCF494E0", Caption = "[40] Previous Documents", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("2A5F78DF-64C2-4A80-92A4-3755D9B1838E", Caption = "[UCC 2/1] Previous Documents")]
		public ZString PreviousDocumentsCaption => CaptionForProperty(nameof(PreviousDocumentsCaption));

		#region IOrgSupplierPart Members

		IBusinessObjectCollection<Integration.Customs.GB.ICusClassPartPivot> Integration.Customs.GB.IOrgSupplierPart.PivotsForBinding => PivotsForBinding;

		public void AddNote(ZString noteText, ZString noteType)
		{
			var note = Notes.AddNew();
			note.ST_NoteText = noteText;
			note.ST_NoteType = noteType;
		}

		#endregion

		string CaptionForProperty(string propertyName) => DataBoundResourceStrings.GetDataForProperty(typeof(OrgSupplierPart), propertyName, MultipleKeysToUse)?.Caption ?? string.Empty;

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.UnitedKingdom);

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new EUMasterFiles.ClassificationCollection<EUMasterFiles.CusClassification>(this, Core.Constants.CountryCodes.UnitedKingdom);
	}
}
