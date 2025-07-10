using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.General.Testing
{
	[TestedType(typeof(DocJobComInvoiceGroupHeader))]
	sealed class DocJobComInvoiceGroupHeaderTest : DocBaseJobComInvoiceGroupHeaderAbstractTest<BaseJobComInvoiceGroupHeader, DocJobComInvoiceGroupHeader>
	{
		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Fiji; }
		}

		protected override DocJobComInvoiceGroupHeader CreateGroupHeaderWrapper(BaseJobComInvoiceGroupHeader groupHeaderInternal)
		{
			return DocJobComInvoiceGroupHeader.New(groupHeaderInternal, Factory);
		}

		#endregion
	}
}
