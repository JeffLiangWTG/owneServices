using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[TestedType(typeof(DocJobComInvoiceGroupHeader))]
	sealed class DocJobComInvoiceGroupHeaderTest : DocBaseJobComInvoiceGroupHeaderAbstractTest<JobComInvoiceGroupHeader, DocJobComInvoiceGroupHeader>
	{
		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.NewZealand; }
		}

		protected override DocJobComInvoiceGroupHeader CreateGroupHeaderWrapper(JobComInvoiceGroupHeader groupHeaderInternal)
		{
			return DocJobComInvoiceGroupHeader.New(groupHeaderInternal, Factory);
		}
	}
}
