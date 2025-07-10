using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GbMessageChangedStatusDeterminerToDictateWhetherSavingAllowed : MessageChangedStatusDeterminerToDictateWhetherSavingAllowed
	{
		public GbMessageChangedStatusDeterminerToDictateWhetherSavingAllowed(Eu.JobDeclaration dec)
			: base(dec)
		{ }

		public override EDIMessage[] MakeMessagesOnThisBizoForComparison(BusinessObject bizo)
		{
			// This is where we generate messages. The argument will be our factory object or our DB object
			JobDeclaration dec = bizo as JobDeclaration;
			List<EDIMessage> ediMessagesToReturnForComparison = new List<EDIMessage>();

			Type type = ObjectFactory.GetType<Integration.Customs.GB.GBChief.ISimpleGenerator>();

			CusdecMessageFunction declarationMessageFunction = new CusdecMessageFunction.Amended();
			var messageGenerator = (IMessageGenerator<Eu.CusEntryHeader>)Activator.CreateInstance(type, new object[] { declarationMessageFunction });

			foreach (Eu.CusEntryHeader entry in dec.CustomsEntryHeaders)
			{
				IBuilderResult result = messageGenerator.Generate(entry);
				ediMessagesToReturnForComparison.Add(result.Message);
			}

			return ediMessagesToReturnForComparison.ToArray<EDIMessage>();
		}
	}
}
