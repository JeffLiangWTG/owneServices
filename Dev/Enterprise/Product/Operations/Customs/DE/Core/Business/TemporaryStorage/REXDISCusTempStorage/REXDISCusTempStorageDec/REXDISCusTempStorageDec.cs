using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), nameof(REXDISCusTempStorageDec))]
	public class REXDISCusTempStorageDec : CusTempStorageDec
	{
		public REXDISCusTempStorageDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : CusTempStorageDec.Schema
		{
			public new const int STH_DeclarationSubTypeMaxLength = 1;
		}

		#endregion

		#region Construction / Loading
		public static REXDISCusTempStorageDec LoadOrCreate(CusTempStorageJobHeader parent)
		{
			return Load(parent) ?? New(parent);
		}

		public static REXDISCusTempStorageDec Load(CusTempStorageJobHeader parent)
		{
			return (REXDISCusTempStorageDec)Load(parent, TemporaryStorageDeclarationTypeList.Codes.ReExportDispatch);
		}

		public static REXDISCusTempStorageDec New(CusTempStorageJobHeader parent)
		{
			var result = parent.Factory.New<REXDISCusTempStorageDec>();
			using (result.SuspendSettingHasChanges())
			{
				result.STH_SJH = parent.PK;
			}
			return result;
		}
		#endregion

		#region Properties

		[ReadOnly(true)]
		[ResourceStringData("F5D08AA8-4C2E-4B8A-ADF0-25582C00D670", Caption = "Re-Export Reference")]
		public override ZString ReferenceNumber { get => base.ReferenceNumber; set => base.ReferenceNumber = value; }

		public override ZString STH_IdentificationIndicator
		{
			get => base.STH_IdentificationIndicator;
			set
			{
				bool hasChanges = STH_IdentificationIndicator != value;
				if (hasChanges)
				{
					base.STH_IdentificationIndicator = value;
					foreach (REXDISCusTempStorageReExportLine line in CusTempStorageLines)
					{
						if (IsREGDeclaration)
						{
							line.SumALine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.REG;
						}
						else
						{
							line.SumALine.TSL_LineNo = 0;
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(REXDISCusTempStorageDecLookups.ProcedureTypeList))]
		[MaxLength(Schema.STH_DeclarationSubTypeMaxLength)]
		public override ZString STH_DeclarationSubType { get => base.STH_DeclarationSubType; set => base.STH_DeclarationSubType = value; }

		protected override ZString EntryNumberType => CusEntryNumberTypes.Germany.ReExportEntryNumber;
		#endregion

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageDecLookups GetNewLookups() => new REXDISCusTempStorageDecLookups(this);

		public new REXDISCusTempStorageDecLookups Lookups => (REXDISCusTempStorageDecLookups)base.Lookups;

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageDecValidation GetNewValidation() => new REXDISCusTempStorageDecValidation(this);

		#endregion

		public new REXDISCusTempStorageReExportLineCollection CusTempStorageLines => (REXDISCusTempStorageReExportLineCollection)base.CusTempStorageLines;

		protected override EU.Business.CusTempStorage.CusTempStorageLineCollection CreateNewCusTempStorageLines() => new REXDISCusTempStorageReExportLineCollection(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ReExportDispatch;
		}
	}
}
