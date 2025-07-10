using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageAdditionalInfo : AdditionalInfo
	{
		public TemporaryStorageAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

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
			var result = base.GetShouldPropertiesBeReadOnly(property) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);

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
								&& header.CustomsStatus is ZString customsStatus
								&& (TemporaryStorageHeader.IsNoEditAllowedCustomsStatus(customsStatus)
									||
									(
										TemporaryStorageHeader.IsAmendableFieldsEditAllowedCustomsStatus(customsStatus)
										&& header.ENSReuse == 1 && bill.ABL_BolType != TemporaryStorageBill.ChildBolCode)
									);
						}
						break;
				}
			}

			return result;
		}

		[ResourceStringData("EU.TemporaryStorageAdditionalInfo|CSI_Code", Caption = "Type")]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[ReadOnlyMember(nameof(CSI_SubTypeReadOnly))]
		[ResourceStringData("EU.TemporaryStorageAdditionalInfo|CSI_SubType", Caption = "Kind")]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = base.CSI_SubType;
				base.CSI_SubType = value;
				if (oldValue != value && !IsCopying)
				{
					if (IsAnAdditionalReference)
					{
						CSI_Description = ZString.Empty;
					}
					else if (IsAnAdditionalInformation)
					{
						CSI_ReferenceNumber = ZString.Empty;
					}
				}
			}
		}

		public bool CSI_SubTypeReadOnly
		{
			get
			{
				var result = false;
				if (Parent is TemporaryStorageBill bill)
				{
					result = bill.ABL_BolType == TemporaryStorageBill.ChildBolCode;
				}

				return result;
			}
		}

		[ReadOnlyMember(nameof(CSI_ReferenceNumberReadOnly))]
		[ResourceStringData("EU.TemporaryStorageAdditionalInfo|CSI_ReferenceNumber", Caption = "Reference")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		public bool CSI_ReferenceNumberReadOnly => IsAnAdditionalInformation;

		[ReadOnlyMember(nameof(CSI_DescriptionReadOnly))]
		[ResourceStringData("EU.TemporaryStorageAdditionalInfo|CSI_Description", Caption = "Description")]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		public ZBool CSI_DescriptionReadOnly => IsAnAdditionalReference;

		protected override CusSupportingInfoLookups GetNewLookups() => new TemporaryStorageAdditionalInfoLookups(this);

		public new TemporaryStorageAdditionalInfoLookups Lookups => (TemporaryStorageAdditionalInfoLookups)base.Lookups;

		protected override CusSupportingInfoValidation GetNewValidation() => new TemporaryStorageAdditionalInfoValidation(this);

		public new TemporaryStorageAdditionalInfoValidation Validation => (TemporaryStorageAdditionalInfoValidation)base.Validation;
	}
}
