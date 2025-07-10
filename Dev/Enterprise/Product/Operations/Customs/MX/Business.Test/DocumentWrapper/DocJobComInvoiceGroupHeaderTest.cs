using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceGroupHeader))]
	sealed class DocJobComInvoiceGroupHeaderTest : DocBaseJobComInvoiceGroupHeaderAbstractTest<JobComInvoiceGroupHeader, DocJobComInvoiceGroupHeader>
	{
		#region Implementation

		protected override string TestingCountry => Enterprise.Core.Constants.CountryCodes.Mexico;

		protected override DocJobComInvoiceGroupHeader CreateGroupHeaderWrapper(JobComInvoiceGroupHeader groupHeaderInternal) => DocJobComInvoiceGroupHeader.New(groupHeaderInternal, Factory);

		#endregion
	}
}
