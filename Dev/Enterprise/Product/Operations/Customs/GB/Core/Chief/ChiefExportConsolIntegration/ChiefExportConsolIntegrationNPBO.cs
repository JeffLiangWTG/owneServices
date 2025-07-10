using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class ChiefExportConsolIntegrationNPBO : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string UCR = "UCR";
			public const string Action = "Action";
			public const string Status = "Status";
			public const string CreationTime = "CreationTime";
			public const string Entry = "Entry";
		}

		public ChiefExportConsolIntegrationNPBO(EDIMessage eacMessage)
			: base(eacMessage.Factory)
		{
			this.eacMessage = eacMessage;
		}

		public ZString UCR
		{
			get { return Entry != null ? Entry.CH_BGMReference : ZString.Empty; }
		}

		public ZString Action
		{
			get { return eacMessage.EM_MessageSubType; }
		}

		public ZString Status
		{
			get { return eacMessage.EM_Status; }
		}

		public ZDateTime CreationTime
		{
			get { return eacMessage.EM_SystemCreateTimeUtc; }
		}

		public CusEntryHeader Entry
		{
			get { return entry ?? (entry = eacMessage.EM_LinkedObject as CusEntryHeader); }
		}

		CusEntryHeader entry;
		readonly EDIMessage eacMessage;
	}
}
