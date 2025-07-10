using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class MetaDataWrapper : IMetaData
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MetaDataWrapper(CusEntryHeader entryHeader, string messageSubType)
		{
			this.itemEntryHeader = Argument.NotNull(entryHeader, "CustEntryHeader cannot be null");

			string subType = Argument.NotNullOrEmpty(messageSubType, "Message Application Code cannot be null or empty");

			if (subType == MessageSubTypeList.Codes.IMC)
			{
				this.messageApplicationCode = "DELTAC";
				this.messageEntryType = itemEntryHeader.IsImport ?
						Customs.Common.EU.EUJobMessageTypeList.Codes.Import
					: Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
			}
			else
			{
				this.messageApplicationCode = Argument.NotNull("", "Message Application Code cannot be unknown");
			}
			switch (messageSubType)
			{
				case MessageSubTypeList.Codes.IMC:
					this.messageApplicationCode = "DELTAC";
					this.messageEntryType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
					break;
				case MessageSubTypeList.Codes.EXC:
					this.messageApplicationCode = "DELTAC";
					this.messageEntryType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
					break;
				case MessageSubTypeList.Codes.IMD:
					this.messageApplicationCode = "DELTAD";
					this.messageEntryType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
					break;
				case MessageSubTypeList.Codes.EXD:
					this.messageApplicationCode = "DELTAD";
					this.messageEntryType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
					break;
				case MessageSubTypeList.Codes.DCG:
					this.messageApplicationCode = "DELTAD";
					break;
				case MessageSubTypeList.Codes.CIN:
					this.messageApplicationCode = "CIN";
					break;
				default:
					this.messageApplicationCode = Argument.NotNull("", "Message Application Code cannot be unknown");
					break;
			}
		}

		public ZString Application => messageApplicationCode;

		public ZString DeclarationReference => itemEntryHeader.CH_BGMReference;

		public ZString EntryNumberType => messageEntryType;

		protected CusEntryHeader itemEntryHeader;
		protected ZString messageApplicationCode;
		protected ZString messageEntryType;
	}
}
