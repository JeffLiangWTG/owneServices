using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD
{
	public class ArticleWrapper : IArticle
	{
		public ArticleWrapper(CusEntryLine entryLine, List<IDocAapurer> documents)
		{
			this.entryLine = entryLine;
			this.documents = documents;
		}

		public ArticleWrapper(CreditCODDataObject item, List<IDocAapurer> documents) : this(item.PreviousEntryLine, documents)
		{
			this.item = item;
		}
		readonly CreditCODDataObject item;
		readonly CusEntryLine entryLine;
		readonly List<IDocAapurer> documents;

		public ZString EntryNumber => entryLine.Header.EntryNumber;

		public ZString Direction => entryLine.Declaration.JE_MessageType;

		public ZString ItemNumber => entryLine.CL_LineNumber.ToString();

		public IEnumerable<IDocAapurer> Documents => documents;

		public IApur Apur => item != null ? new ApurWrapper(item) : null;

		public IEnumerable<ITaxeAapurer> Taxes => throw new NotImplementedException();
	}
}
