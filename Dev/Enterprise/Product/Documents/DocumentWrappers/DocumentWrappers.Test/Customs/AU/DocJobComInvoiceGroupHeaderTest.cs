using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocJobComInvoiceGroupHeader))]
	sealed class DocJobComInvoiceGroupHeaderTest : DocBaseJobComInvoiceGroupHeaderAbstractTest<JobComInvoiceGroupHeader, DocJobComInvoiceGroupHeader>
	{
		#region ZString Fields

		public void TestCommissionType()
		{
			GroupHeaderInternal.JZ_CommissionType = "C";
			AssertEquals("CommissionType", GroupHeaderInternal.JZ_CommissionType, GroupHeaderWrapperInternal.CommissionType);
		}

		#endregion

		#region Wrapper Fields

		public void TestDeclaration()
		{
			GroupHeaderInternal.JZ_JE = Factory.New(typeof(JobDeclaration)).PK;
			AssertNotNull("Declaration", GroupHeaderWrapperInternal.Declaration);
			AssertEquals("Declaration is of type DocDeclaration", typeof(DocDeclaration), GroupHeaderWrapperInternal.Declaration.GetType());
		}

		#endregion

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Australia; }
		}

		protected override DocJobComInvoiceGroupHeader CreateGroupHeaderWrapper(JobComInvoiceGroupHeader groupHeaderInternal)
		{
			return DocJobComInvoiceGroupHeader.New(groupHeaderInternal, Factory);
		}

		#endregion
	}
}
