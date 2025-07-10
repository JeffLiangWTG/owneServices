using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument
	{
		public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.PackageTypeList))]
		public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

		public override ZBool ShowCodeFindBoxForReferenceNumber => IsTemporaryStorageDocument;

		public ZBool IsTemporaryStorageDocument => CSI_Code == PreviousDocumentCodeList.Codes.IST || CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;

		public ZBool IsUCC6TemporaryStorage => (Declaration?.IsUCC6 ?? false) && CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

		public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;

		[ReadOnlyMember(nameof(IsUCC6TemporaryStorage))]
		public override ZString CSI_UnitOfQuantity
		{
			get
			{
				return base.CSI_UnitOfQuantity;
			}
			set
			{
				base.CSI_UnitOfQuantity = value;
			}
		}

		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				if (CSI_Code != value)
				{
					base.CSI_Code = value;
					if (IsUCC6TemporaryStorage)
					{
						CSI_UnitOfQuantity = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogramme;
					}
				}
			}
		}

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			if (Declaration?.IsUCC6 ?? false)
			{
				return new DeltaIEPreviousDocumentValidation(this);
			}
			else
			{
				return new DeltaGPreviousDocumentValidation(this);
			}
		}
	}
}
