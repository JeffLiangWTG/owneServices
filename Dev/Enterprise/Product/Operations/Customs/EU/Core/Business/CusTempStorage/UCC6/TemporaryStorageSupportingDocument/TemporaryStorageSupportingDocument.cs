using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageSupportingDocument : SupportingDocument
	{
		public TemporaryStorageSupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int ReferenceNumberMaxLength = 70;
		}

		[MaxLength(nameof(CSI_ReferenceNumberMaxLength))]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageSupportingDocumentLookups.CodeList))]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		public TemporaryStorageHeader TemporaryStorageHeader
		{
			get
			{
				TemporaryStorageHeader result = null;
				switch (Parent)
				{
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

		protected override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			var result = base.GetShouldPropertiesBeReadOnly(property);

			if (!result)
			{
				switch (Parent)
				{
					case TemporaryStorageBill bill:
						{
							result = bill.Header?.IsNoEditAllowedCustomsStatus() ?? false;
						}
						break;
					case TemporaryStoragePackedItem packedItem:
						{
							result = packedItem.Bill is TemporaryStorageBill bill
								&& bill.Header is TemporaryStorageHeader header
								&&
								(
									bill.ABL_BolType.EqualsIgnoringCase(TemporaryStorageBill.ChildBolCode) ?
										header.IsReadOnlyBasedOnTSDLogic() :
										header.IsNoEditAllowedCustomsStatus());
						}
						break;
				}
			}

			return result;
		}

		public new TemporaryStorageSupportingDocumentLookups Lookups => (TemporaryStorageSupportingDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new TemporaryStorageSupportingDocumentLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation() => new TemporaryStorageSupportingDocumentValidation(this);

		public new TemporaryStorageSupportingDocumentValidation Validation => (TemporaryStorageSupportingDocumentValidation)base.Validation;

		protected override ZString HumanReadableNameCore => Res.GetString("26D5510B-213F-440C-BAB7-977F1EBDABAF", "Supporting Document");

		protected virtual int CSI_ReferenceNumberMaxLength => Schema.ReferenceNumberMaxLength;
	}
}
