using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class OwnerOfGoods : JobDocAddress
		, IShortSequenceNumberLine
		, ISupportMultipleResourceStringData
	{
		public OwnerOfGoods(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("C28DE107-93B1-424E-B69A-2B849100B967", "Owner Of Goods");

		[ResourceStringData("5D318B82-2324-4736-97A2-B2DFEE3BFAE2|OwnerOfGoods|OrganisationPK", Caption = "Organization")]
		[ResourceStringData("5D318B82-2324-4736-97A2-B2DFEE3BFAE2|IMPUCC6|OwnerOfGoods|OrganisationPK", Caption = "Organization", FullDescription = "[Annex A 3/8] Parties > Owner of the Goods > Organization", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[List(nameof(Lookups) + "." + nameof(JobDocAddressLookups.OrgHeader_List))]
		public new ZGuid OrganisationPK
		{
			get => base.OrganisationPK;
			set => base.OrganisationPK = value;
		}

		[ResourceStringData("59C6DE94-F86C-43CD-A87F-9CAEC4A5EE9F|OwnerOfGoods|E2_OA_Address", Caption = "Address")]
		[ResourceStringData("59C6DE94-F86C-43CD-A87F-9CAEC4A5EE9F|IMPUCC6|OwnerOfGoods|E2_OA_Address", Caption = "Address", FullDescription = "[Annex A 3/8] Parties > Owner of the Goods > Address", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid E2_OA_Address
		{
			get => base.E2_OA_Address;
			set => base.E2_OA_Address = value;
		}

		protected override JobDocAddressValidation GetNewValidation() => new OwnerOfGoodsValidation(this);

		public CusEntryInstruction Instruction => Factory.Load<CusEntryInstruction>(E2_ParentID);

		public override ZGuid E2_ParentID
		{
			get => base.E2_ParentID;
			set
			{
				var oldValue = E2_ParentID;
				base.E2_ParentID = value;
				if (!IsCopying && oldValue != E2_ParentID)
				{
					Instruction?.SequenceGenerator.RecalculateWhenAdded(this);
				}
			}
		}

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => new ZShort(E2_AddressSequence);
			set => E2_AddressSequence = ZByte.ParseSafe(value.ToString(), ZByte.Zero);
		}

		ZGuid ISequenceNumberLine.FKToHeader => E2_ParentID;

		public IReadOnlyList<string> MultipleKeysToUse => Instruction?.MultipleKeysToUse ?? Array.Empty<string>();
	}
}
