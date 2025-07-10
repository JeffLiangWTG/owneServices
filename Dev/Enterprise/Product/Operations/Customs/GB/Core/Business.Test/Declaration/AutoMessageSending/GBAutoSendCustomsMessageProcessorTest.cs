using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public abstract class GBAutoSendCustomsMessageProcessorTest : Customs.Business.Testing.AutoSendCustomsMessageProcessorTest
	{
		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration)
		{
			var dec = (JobDeclaration)declaration;
			return dec.ApplicationExtender.GetEntryDeclarationMessageProcessor(dec);
		}
		protected override Customs.Business.CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration)
		{
			return declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>().FirstOrDefault();
		}
	}
}
