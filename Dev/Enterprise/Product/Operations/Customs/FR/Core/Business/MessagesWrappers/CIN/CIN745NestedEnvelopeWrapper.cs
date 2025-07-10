using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN
{
	public class CIN745NestedEnvelopeWrapper : ICINNested745Message
	{
		public const string oaciCode = "EZ1";
		public const string oaciDest = "BL1";
		public const string schemaVersion = "1";
		public const string schemaID = "Message745";
		public CIN745NestedEnvelopeWrapper(CusEntryHeader header)
		{
			this.cusEntryHeader = Argument.NotNull(header, "CusEntryHeader cannot be null");
		}
		public ZString OACI => oaciCode;
		public ZString BUR_DOUANE => cusEntryHeader?.CustomOffice ?? ZString.Empty;
		public ZString MRN_ECS => cusEntryHeader?.MrnNumber ?? ZString.Empty;
		public ZString REFERENCE => cusEntryHeader?.BGMReference ?? ZString.Empty;
		public ZString DEST_OACI => oaciDest;
		public ZString OACI_Carrier => cusEntryHeader.OACI_Carrier;
		public ZString OACI_Shipper => cusEntryHeader.OACI_Shipper;
		public ZString SchemaVersion => schemaVersion;
		public ZString SchemaID => schemaID;
		public ZString TransactionID => cusEntryHeader.TransactionIDCIN745;
		public ZDate Date => ZDate.Today;
		public ZDateTime Time => ZDateTime.Today;
		public ZString Level => cusEntryHeader.Level;
		public ZString Name => cusEntryHeader.Name;
		public ZString ExitOfOffice => cusEntryHeader.ExitOfOffice;
		public ZString MrnNumber => cusEntryHeader.MrnNumber;
		public ZInt MrnQuantity => cusEntryHeader.MrnQuantity;
		public ZDecimal MrnWeight => cusEntryHeader.MrnWeight;
		public ZString MAGASIN => cusEntryHeader.Magasin;

		readonly ICINMessage745 cusEntryHeader;
	}
}
