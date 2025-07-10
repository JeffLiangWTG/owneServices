using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "CusTempStorageDecs")]
	public class CHGOFFCusTempStorageDec : CusTempStorageDec
	{
		public CHGOFFCusTempStorageDec(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		public override ZString ReferenceNumber => FormattedOwnerReferenceNumber;

		public override ZString STH_IdentificationIndicator
		{
			get => base.STH_IdentificationIndicator;
			set
			{
				var hasChanges = STH_IdentificationIndicator != value;
				if (hasChanges)
				{
					base.STH_IdentificationIndicator = value;
					if (STH_IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.REG)
					{
						foreach (CHGOFFCusTempStorageLine line in CusTempStorageLines)
						{
							line.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.REG;
						}
					}
					else if (IsAWBDeclaration)
					{
						var regLines = CusTempStorageLines.Cast<CHGOFFCusTempStorageLine>().Where(
							x => x.TSL_OwnerReferenceType == OwnerReferenceTypeList.Codes.REG);
						foreach (var line in regLines)
						{
							line.TSL_OwnerReferenceType = ZString.Empty;
						}
					}
				}
			}
		}

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageDecLookups GetNewLookups() => new CHGOFFCusTempStorageDecLookups(this);

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageDecValidation GetNewValidation() => new CHGOFFCusTempStorageDecValidation(this);

		#endregion

		#endregion

		#region CusTempStorageLines

		public new CHGOFFCusTempStorageLineCollection CusTempStorageLines => (CHGOFFCusTempStorageLineCollection)base.CusTempStorageLines;

		protected override EU.Business.CusTempStorage.CusTempStorageLineCollection CreateNewCusTempStorageLines() => new CHGOFFCusTempStorageLineCollection(this);

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader;
		}

		#endregion
	}
}
