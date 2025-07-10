using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStoragePreviousDocument : CusSupportingInfo, Integration.Customs.EU.IPreviousDocument
	{
		public TemporaryStoragePreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int ReferenceNumberMaxLength = 70;
			public const int LineNoMaxLength = 5;
		}

		[MaxLength(Schema.ReferenceNumberMaxLength)]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePreviousDocumentLookups.CodeList))]
		[ReadOnlyMember(nameof(CSI_CodeReadOnly))]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }
		protected bool CSI_CodeReadOnly => Parent is TemporaryStorageHeader header && (header.IsTransfer || header.IsDeconsolidation);

		[MaxLength(Schema.LineNoMaxLength)]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		protected override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return base.GetShouldPropertiesBeReadOnly(property) || (TemporaryStorageHeader?.IsReadOnlyBasedOnTSDLogic() ?? false) || (TemporaryStorageHeader?.IsAmendableFieldsEditAllowedCustomsStatus() ?? false);
		}

		public new TemporaryStoragePreviousDocumentLookups Lookups => (TemporaryStoragePreviousDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new TemporaryStoragePreviousDocumentLookups(this);
		}

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new TemporaryStoragePreviousDocumentValidation(this);
		}

		public TemporaryStorageHeader TemporaryStorageHeader
		{
			get
			{
				TemporaryStorageHeader result = null;
				switch (Parent)
				{
					case TemporaryStorageHeader temporaryStorageHeader:
						result = temporaryStorageHeader;
						break;
					case TemporaryStorageBill bill:
						result = bill.Header;
						break;
					case TemporaryStoragePackedItem packedItem:
						result = packedItem.Bill?.Header;
						break;
				}
				return result;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("F893F13C-017F-466C-85E3-7A1DEEEEA485", "Previous Document");
			}
		}

		protected override ZString HumanReadableNameForPluralCore
		{
			get
			{
				return Res.GetString("8C0485EC-CDB5-4C90-8AC1-1EF93FC0F00C", "Previous Documents");
			}
		}
	}
}
