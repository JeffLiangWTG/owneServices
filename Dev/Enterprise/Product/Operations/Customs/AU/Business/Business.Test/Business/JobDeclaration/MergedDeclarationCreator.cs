using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public sealed class MergedDeclarationCreator : Customs.Business.Testing.MergedDeclarationCreator<JobDeclaration>
	{
		public MergedDeclarationCreator(BusinessObjectFactory factory, ZDateTime? systemCreateDate = null)
			: base(factory)
		{
			this.systemCreateDate = systemCreateDate;
		}
		readonly ZDateTime? systemCreateDate;

		public new JobComInvoiceLine InvoiceLine1
		{
			get { return (JobComInvoiceLine)base.InvoiceLine1; }
		}

		public new CusEntryHeader Entry1
		{
			get { return (CusEntryHeader)base.Entry1; }
		}

		public new CusEntryLine EntryLine1
		{
			get { return (CusEntryLine)base.EntryLine1; }
		}

		protected override void SetupDeclarationPreMerge(JobDeclaration declaration)
		{
			base.SetupDeclarationPreMerge(declaration);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			if (systemCreateDate.HasValue)
			{
				declaration.JE_SystemCreateTimeUtc = systemCreateDate.Value;
			}
		}
	}
}
