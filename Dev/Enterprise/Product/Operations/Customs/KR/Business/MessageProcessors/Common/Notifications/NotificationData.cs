using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Business
{
	public class NotificationData
	{
		public IControllerIDProvider ControllerIDProvider { get; set; }
		public IEDIMessageCollectionProvider MessagesParent { get; set; }
		public GlbStaff AlternativeRecipientStaff { get; set; }
		public string JobNumberDescription { get; set; }
		public GlbBranch Branch { get; set; }
		public GuidRegistryItem EmailGroup { get; set; }
		public ZString EmailBody { get; set; }
		public ZString MessageTypeDescription { get; set; }
		public ZString[] OriginalMessageTypes { get; set; }
		public virtual ZString EntryNumber { get; set; }
	}

	public class DeclarationEntryNotificationData : NotificationData
	{
		public CusEntryHeader Entry
		{
			get => entry;
			set
			{
				entry = value;
				MessagesParent = entry;
				if (entry?.Declaration != null)
				{
					ControllerIDProvider = entry.Declaration;
					AlternativeRecipientStaff = entry.Declaration.CusAgent;
					SetJobNumberDescription();
				}
			}
		}
		CusEntryHeader entry;
		public JobDeclaration Declaration => Entry?.Declaration;

		public override ZString EntryNumber
		{
			get => base.EntryNumber;
			set
			{
				base.EntryNumber = value;
				SetJobNumberDescription();
			}
		}
		#region SuppressResourceStringsCheckRegion
		void SetJobNumberDescription()
		{
			if (Declaration != null && !EntryNumber.IsEmpty)
			{
				JobNumberDescription = $"Declaration Number: {Declaration.JE_DeclarationReference} / 제출번호: {EntryNumber}";
			}
		}
		#endregion
	}
}
