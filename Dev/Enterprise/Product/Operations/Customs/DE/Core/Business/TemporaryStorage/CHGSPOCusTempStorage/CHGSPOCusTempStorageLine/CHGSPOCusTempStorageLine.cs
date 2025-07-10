using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CHGSPOCusTempStorageDec), "CusTempStorageLines")]
	public class CHGSPOCusTempStorageLine : CusTempStorageLine
	{
		public CHGSPOCusTempStorageLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CHGSPOCusTempStorageDec Dec => (CHGSPOCusTempStorageDec)base.Dec;

		[ResourceStringData("A48FE40C-535B-45C5-9C68-A942022BADDA", Caption = "New Owner Reference Type", ShortCaption = "New Owner Ref. Type")]
		public override ZString TSL_OwnerReferenceType { get => base.TSL_OwnerReferenceType; set => base.TSL_OwnerReferenceType = value; }

		[ResourceStringData("F9411D4D-02D0-4F7B-AB50-C8B377CD7EC9", Caption = "New Owner Reference No.", ShortCaption = "New Owner Ref. No.")]
		public override ZString TSL_OwnerReferenceNumber
		{
			get => base.TSL_OwnerReferenceNumber;
			set => base.TSL_OwnerReferenceNumber = value;
		}

		[ResourceStringData("6E1D23EA-AD6D-462D-9BDD-131F52F8D11D", Caption = "Reference Line Number", ShortCaption = "Reference Line No.")]
		public override ZInt TSL_LineNo { get => base.TSL_LineNo; set => base.TSL_LineNo = value; }

		protected override bool ReadOnlyTSL_LineNo => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB;
		}

		protected override bool SequenceNumberEnabledCore => false;

		protected override bool SetLineNumOnSettingTSL_STHEnabled => false;

		public new CHGSPOCusTempStorageLineValidation Validation => (CHGSPOCusTempStorageLineValidation)base.Validation;
		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new CHGSPOCusTempStorageLineValidation(this);

		public new CHGSPOCusTempStorageLineLookups Lookups => (CHGSPOCusTempStorageLineLookups)base.Lookups;
		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new CHGSPOCusTempStorageLineLookups(this);
	}
}
