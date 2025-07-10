namespace Enterprise.Customs.CA.Business
{
	using System.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageProcessors;
	using Enterprise.MasterFiles.Business.CustomValues;
	using Enterprise.Messaging.Business;

	public class ACIHouseBillMessage : ACIForwarderMessage
	{
		public ACIHouseBillMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoEDIMessage.Schema
		{
			public const string PrimaryCCN = "PrimaryCCN";
		}

		public ZString PrimaryCCN
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.PrimaryCCN); }
			set { this.SetSystemDefinedValue(Schema.PrimaryCCN, value); }
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
		}

		internal ManifestForwardHouseBillWrapper Wrapper
		{
			get
			{
				return wrapper ?? (wrapper = new ManifestForwardHouseBillWrapper(this));
			}
		}
		ManifestForwardHouseBillWrapper wrapper;

		public override ZString SNPType
		{
			get { return this.GetSystemDefinedValue<ZString>(EDIMessage.Schema.SNPType); }
		}

		public override ZString CargoControlNumber
		{
			get { return Wrapper.HouseBillCCN; }
		}

		public override ZString PrimaryCargoControlNumber
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.PrimaryCCN); }
		}

		public override ZString TransactionNumber
		{
			get
			{
				var header = EM_LinkedObject as CusEntryHeader;
				return header != null && header.Declaration != null ? header.Declaration.DeclarationNumber : ZString.Empty;
			}
		}

		public override ZString SubLocation
		{
			get { return this.GetSystemDefinedValue<ZString>(EDIMessage.Schema.SubLocation); }
		}

		public override ZString CBSAOffice
		{
			get { return this.GetSystemDefinedValue<ZString>(EDIMessage.Schema.CBSAOffice); }
		}

		#endregion
	}
}
