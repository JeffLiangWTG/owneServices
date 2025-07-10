using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Message.MessageBuilder
{
	public class ILGEN920MessageBuilder : ILMessageBuilderBase
	{
		public ILGEN920MessageBuilder(GlbCompany company, IEnumerable<ZString> correlationIdList) : base(null)
		{
			this.company = Argument.NotNull(company, nameof(company));
			this.factory = company.Factory;
			this.correlationIdList = correlationIdList;
		}

		protected override ILEDIMessage GetMessage()
		{
			var message = factory.New<ILGEN920RequestMessage>();
			message.EM_GB = company.ActiveBranches.FirstOrDefault().PK;
			return message;
		}

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => null;

		protected override string GetMessageText()
		{
			var messageBuilder = new Sync9200MessageBuilder(MessageSynchronize9200Wrapper.NewOrNull(correlationIdList)) as IXmlMessageBuilder;
			return messageBuilder?.GenerateXmlMessage().GetSerializedString() ?? ZString.Empty;
		}

		readonly BusinessObjectFactory factory;
		readonly GlbCompany company;
		readonly IEnumerable<ZString> correlationIdList;
	}
}
