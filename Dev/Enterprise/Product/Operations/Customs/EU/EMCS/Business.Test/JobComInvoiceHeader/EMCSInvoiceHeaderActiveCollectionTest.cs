using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSInvoiceHeaderActiveCollection))]
	sealed class EMCSInvoiceHeaderActiveCollectionTest : ActiveBusinessObjectCollectionTestCase<EMCSInvoiceHeaderActiveCollection>
	{
		protected override EMCSInvoiceHeaderActiveCollection GetCollectionToTest()
		{
			return GroupHeader.JobComInvoiceHeaders;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<EMCSJobComInvoiceHeader>();
			result.JZ_JE = Declaration.PK;
			result.JZ_JZ_GroupInvoiceFK = GroupHeader.PK;
			return result;
		}

		EMCSJobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<EMCSJobDeclaration>()); }
		}
		EMCSJobDeclaration declaration;

		EMCSJobComInvoiceGroupHeader GroupHeader
		{
			get { return groupHeader ?? (groupHeader = Declaration.JobComInvoiceGroupHeaders[0]); }
		}
		EMCSJobComInvoiceGroupHeader groupHeader;
	}
}
