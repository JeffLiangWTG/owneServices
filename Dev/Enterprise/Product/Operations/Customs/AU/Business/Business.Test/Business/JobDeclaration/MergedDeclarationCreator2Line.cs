using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class MergedDeclarationCreator2Line : Customs.Business.Testing.MergedDeclarationCreator2Line<JobDeclaration>
	{
		public MergedDeclarationCreator2Line(BusinessObjectFactory factory, ZDateTime? systemCreateDate = null)
			: base(factory)
		{
			this.systemCreateDate = systemCreateDate;
		}
		readonly ZDateTime? systemCreateDate;

		public new JobComInvoiceLine InvoiceLine1
		{
			get { return (JobComInvoiceLine)base.InvoiceLine1; }
		}

		public new JobComInvoiceLine InvoiceLine2
		{
			get { return (JobComInvoiceLine)base.InvoiceLine2; }
		}

		public new CusEntryHeader Entry1
		{
			get { return (CusEntryHeader)base.Entry1; }
		}

		public new CusEntryLine EntryLine1
		{
			get { return (CusEntryLine)base.EntryLine1; }
		}

		public new CusEntryLine EntryLine2
		{
			get { return (CusEntryLine)base.EntryLine2; }
		}

		protected override void SetupDeclarationPreMerge(JobDeclaration declaration)
		{
			base.SetupDeclarationPreMerge(declaration);
			if (systemCreateDate.HasValue)
			{
				declaration.JE_SystemCreateTimeUtc = systemCreateDate.Value;
			}
		}
	}
}
