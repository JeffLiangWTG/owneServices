using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class InwardProcessingPlace : JobDocAddress, IShortSequenceNumberLine
	{
		public InwardProcessingPlace(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("FA020DA1-EF42-495D-939E-F0E371F1C3D8|InwardProcessingPlace|E2_AddressSequence", Caption = "Sequence")]
		public override ZByte E2_AddressSequence
		{
			get => base.E2_AddressSequence;
			set => base.E2_AddressSequence = value;
		}

		[ResourceStringData("AF87AB95-C195-4150-B2AC-E986AA6E2EA3|InwardProcessingPlace|OrganisationPK", Caption = "Processor")]
		[List(nameof(Lookups) + "." + nameof(JobDocAddressLookups.OrgHeader_List))]
		public new ZGuid OrganisationPK
		{
			get => base.OrganisationPK;
			set => base.OrganisationPK = value;
		}

		[ResourceStringData("CA0581BB-6A92-44DB-BDBD-4551EF4CEDBB|InwardProcessingPlace|E2_OA_Address", Caption = "Address")]
		public override ZGuid E2_OA_Address
		{
			get => base.E2_OA_Address;
			set => base.E2_OA_Address = value;
		}

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
		protected override ZString HumanReadableNameCore => Res.GetString("9EDD0399-C3D6-4C87-98A9-6D9F8D7F052D", "Inward Processing Place");
	}
}
